package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"net/http/httputil"
	"strconv"
	"strings"
	"time"
)

const notFoundError = `{"status":404,"code":"NOT_FOUND","message":"Not found"}`
const internalServerErrorTemplate = `{"status":500,"code":"INTERNAL_SERVER_ERROR","error_id":"%v","message":"Internal server error"}`
const rateLimitedErrorTemplate = `{"status":429,"code":"RATE_LIMITED","reset":%v,"message":"You are being rate limited"}`

// error ID chosen by fair duckduckgo search, guaranteed to be random
// (we just need to return *any* error ID, this will do)
const errorID = "951c03eadb6b474db35c23d47a10f6c0"

type Handler struct {
	Port        int    `json:"port"`
	ProxyTarget string `json:"proxy_target"`
	Debug       bool   `json:"debug"`
	PoweredBy   string `json:"powered_by"`

	limiter *Limiter
	proxy   *httputil.ReverseProxy
	client  *http.Client
}

func (hn *Handler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	if hn.PoweredBy != "" {
		w.Header().Set("X-Powered-By", hn.PoweredBy)
	}

	// all public api endpoints are prefixed with this
	if !strings.HasPrefix(r.URL.Path, "/api/v2") {
		w.WriteHeader(http.StatusNotFound)
		return
	}

	data, err := hn.requestData(r)
	if err != nil {
		if rde, ok := err.(requestDataError); ok {
			switch rde.Type {
			case "badRequest":
				if hn.Debug {
					log.Printf("Bad request error for path %v\n", r.URL.Path)
				}

				w.Header().Set("Content-Type", "application/json; charset=utf-8")
				w.Header().Set("Content-Length", strconv.Itoa(len(notFoundError)))
				w.WriteHeader(http.StatusNotFound)
				_, _ = w.Write([]byte(notFoundError))

				return
			case "internalServerError":
				respBody := fmt.Sprintf(internalServerErrorTemplate, rde.ErrorID)
				w.Header().Set("Content-Type", "application/json; charset=utf-8")
				w.Header().Set("Content-Length", strconv.Itoa(len(respBody)))
				w.WriteHeader(http.StatusInternalServerError)
				_, _ = w.Write([]byte(respBody))

				return
			}
		}

		log.Printf("internal error while parsing request data response: %v\n", err)

		respBody := fmt.Sprintf(internalServerErrorTemplate, errorID)
		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		w.Header().Set("Content-Length", strconv.Itoa(len(respBody)))
		w.WriteHeader(http.StatusInternalServerError)
		_, _ = w.Write([]byte(respBody))

		return
	}

	if hn.Debug {
		log.Printf("proxying request to %v %v", r.Method, data.Template)
	}

	isLimited, err := hn.limiter.TryLimit(w, r, data.UserID, data.Template)
	if err != nil {
		log.Printf("error checking rate limit: %v\n", err)

		respBody := fmt.Sprintf(internalServerErrorTemplate, errorID)
		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		w.Header().Set("Content-Length", strconv.Itoa(len(respBody)))
		w.WriteHeader(http.StatusInternalServerError)
		_, _ = w.Write([]byte(respBody))
		return
	}

	if isLimited {
		var resetTime time.Time
		if reset := getReset(w); reset == 0 {
			resetTime = time.Now().UTC().Add(10 * time.Second)
		} else {
			resetTime = time.Unix(reset, 0)
		}

		respBody := fmt.Sprintf(rateLimitedErrorTemplate, resetTime.UnixMilli())
		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		w.Header().Set("Content-Length", strconv.Itoa(len(respBody)))
		w.WriteHeader(http.StatusTooManyRequests)
		_, _ = w.Write([]byte(respBody))
		return
	}

	hn.proxy.ServeHTTP(w, r)
}

func (hn *Handler) requestData(r *http.Request) (data requestDataResponse, err error) {
	var token *string
	if header := r.Header.Get("Authorization"); header != "" {
		token = &header
	}

	url := hn.ProxyTarget + "/api/internal/request-data"

	reqData, err := json.Marshal(requestDataRequest{Token: token, Method: r.Method, Path: r.URL.Path})
	if err != nil {
		return data, fmt.Errorf("marshaling request json: %w", err)
	}

	req, err := http.NewRequestWithContext(r.Context(), "POST", url, bytes.NewReader(reqData))
	if err != nil {
		return data, fmt.Errorf("creating request: %w", err)
	}
	req.Header.Set("User-Agent", "Foxnouns.NET/rate")
	req.Header.Set("Content-Type", "application/json; charset=utf-8")

	resp, err := hn.client.Do(req)
	if err != nil {
		return data, fmt.Errorf("making request: %w", err)
	}
	defer resp.Body.Close()

	// If we got a bad request error, that means the endpoint wasn't found, and we should tell the client that
	if resp.StatusCode == http.StatusBadRequest {
		return data, requestDataError{Type: "badRequest"}
	}

	// If we got an internal server error we have to forward the error ID
	if resp.StatusCode == http.StatusInternalServerError {
		b, err := io.ReadAll(resp.Body)
		if err != nil {
			return data, fmt.Errorf("reading internal server error body: %w", err)
		}

		var fne foxnounsError
		err = json.Unmarshal(b, &fne)
		if err != nil {
			return data, fmt.Errorf("unmarshaling internal server error: %w", err)
		}

		return data, requestDataError{Type: "internalServerError", ErrorID: fne.ErrorId}
	}

	b, err := io.ReadAll(resp.Body)
	if err != nil {
		return data, fmt.Errorf("reading body: %w", err)
	}

	err = json.Unmarshal(b, &data)
	if err != nil {
		return data, fmt.Errorf("unmarshaling data: %w", err)
	}
	return data, nil
}

type requestDataRequest struct {
	Token  *string `json:"token"`
	Method string  `json:"method"`
	Path   string  `json:"path"`
}

type requestDataResponse struct {
	UserID   *string `json:"user_id"`
	Template string  `json:"template"`
}

type foxnounsError struct {
	ErrorId string `json:"error_id"`
}

type requestDataError struct {
	Type    string
	ErrorID string
}

func (e requestDataError) Error() string { return "request data error: " + e.Type }
