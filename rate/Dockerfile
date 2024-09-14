FROM docker.io/golang:latest AS builder
WORKDIR /build
EXPOSE 5003

COPY . ./
RUN go mod download -x
ENV CGO_ENABLED 0
RUN go build -v -o rate

FROM alpine:latest
RUN apk --no-cache add ca-certificates

WORKDIR /app
COPY --from=builder /build/rate rate

CMD ["/app/rate"]
