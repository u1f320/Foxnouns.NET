package main

import (
	"encoding/json"
	"log"
	"net/http"
	"net/http/httputil"
	"net/url"
	"os"
	"strconv"
)

func main() {
	// read config file and parse it
	confB, err := os.ReadFile("proxy-config.json")
	if err != nil {
		log.Fatalf("reading config.json: %v", err)
	}

	hn := &Handler{}
	err = json.Unmarshal(confB, hn)
	if err != nil {
		log.Fatalf("unmarshaling config.json: %v", err)
	}

	proxyURL, err := url.Parse(hn.ProxyTarget)
	if err != nil {
		log.Fatalf("parsing proxy_target as URL: %v", err)
	}
	hn.proxy = &httputil.ReverseProxy{
		Rewrite: func(pr *httputil.ProxyRequest) {
			pr.SetURL(proxyURL)
			pr.Out.Host = pr.In.Host
		},
	}
	hn.limiter = NewLimiter()
	hn.client = &http.Client{}

	log.Printf("serving on port %v", hn.Port)

	err = http.ListenAndServe(":"+strconv.Itoa(hn.Port), hn)
	if err != nil {
		log.Fatalf("listening on port %v: %v", hn.Port, err)
	}

}
