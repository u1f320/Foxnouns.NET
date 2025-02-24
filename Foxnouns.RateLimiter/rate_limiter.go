package main

import (
	"encoding/hex"
	"fmt"
	"net/http"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/go-chi/httprate"
)

func userOrIp(r *http.Request, userId *string) (string, error) {
	if userId != nil {
		return *userId, nil
	}

	return httprate.KeyByRealIP(r)
}

type Limiter struct {
	bucketRateLimiters map[bucketKey]*httprate.RateLimiter
	globalRateLimiters map[string]*httprate.RateLimiter

	bucketMu *sync.Mutex
	globalMu *sync.Mutex
}

type LimitData struct {
	Global    bool
	Bucket    string
	Remaining int
	Reset     int
}

type bucketKey struct {
	user, bucket string
}

func NewLimiter() *Limiter {
	return &Limiter{
		bucketRateLimiters: make(map[bucketKey]*httprate.RateLimiter),
		globalRateLimiters: make(map[string]*httprate.RateLimiter),

		bucketMu: new(sync.Mutex),
		globalMu: new(sync.Mutex),
	}
}

// TryLimit handles rate limiting for a request.
// If this function returns true, a rate limit was hit, and the request should be aborted with a 429 status.
func (l *Limiter) TryLimit(w http.ResponseWriter, r *http.Request, userID *string, template string) (bool, error) {
	user, err := userOrIp(r, userID)
	if err != nil {
		return false, fmt.Errorf("getting user or IP: %w", err)
	}

	// Check the globalLimiter rate limit for this user
	globalLimiter := l.globalLimiter(user)
	if globalLimiter.OnLimit(w, r, "") {
		w.Header().Add("X-RateLimit-Global", "true")
		return true, nil
	}

	bucket := requestBucket(r.Method, template)
	w.Header().Add("X-RateLimit-Bucket", bucket)

	bucketLimiter := l.bucketLimiter(user, r.Method, bucket)
	if bucketLimiter.OnLimit(w, r, "") {
		return true, nil
	}

	return false, nil
}

func getReset(w http.ResponseWriter) int64 {
	header := w.Header().Get("X-RateLimit-Reset")
	if header == "" {
		return 0
	}

	i, err := strconv.ParseInt(header, 10, 64)
	if err != nil {
		return 0
	}

	return i
}

func requestBucket(method, template string) string {
	return hex.EncodeToString([]byte(method + "-" + template))
}

func (l *Limiter) globalLimiter(user string) *httprate.RateLimiter {
	l.globalMu.Lock()
	defer l.globalMu.Unlock()

	limiter, ok := l.globalRateLimiters[user]
	if ok {
		return limiter
	}

	limiter = httprate.NewRateLimiter(20, time.Second)
	l.globalRateLimiters[user] = limiter
	return limiter
}

func (l *Limiter) bucketLimiter(user, method, bucket string) *httprate.RateLimiter {
	l.bucketMu.Lock()
	defer l.bucketMu.Unlock()

	limiter, ok := l.bucketRateLimiters[bucketKey{user, bucket}]
	if ok {
		return limiter
	}

	requestLimit, windowLength := requestLimitFor(method)

	limiter = httprate.NewRateLimiter(requestLimit, windowLength)
	l.bucketRateLimiters[bucketKey{user, bucket}] = limiter
	return limiter
}

// returns the request limit and window length for the given method.
// methods that change state have lower rate limits
func requestLimitFor(method string) (int, time.Duration) {
	switch strings.ToUpper(method) {
	case "PATCH", "POST":
		return 3, time.Second
	case "DELETE":
		return 2, 5 * time.Second
	case "GET":
		return 10, time.Second
	default:
		return 5, time.Second
	}
}
