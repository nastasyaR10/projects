package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/joho/godotenv"

	"github.com/nastasyaR10/projects/metro-route/internal/api"
	"github.com/nastasyaR10/projects/metro-route/internal/storage"
)

func main() {
	_ = godotenv.Load()
	setupLogger()

	dsn := os.Getenv("DATABASE_URL")
	if dsn == "" {
		slog.Error("DATABASE_URL is not set")
		os.Exit(1)
	}
	addr := os.Getenv("HTTP_ADDR")
	if addr == "" {
		addr = ":8080"
	}

	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()

	schema, err := storage.LoadSchema("data/metro.json")
	if err != nil {
		slog.Error("load schema failed", "err", err)
		os.Exit(1)
	}
	g, err := storage.BuildGraph(schema)
	if err != nil {
		slog.Error("build graph failed", "err", err)
		os.Exit(1)
	}

	dbCtx, dbCancel := context.WithTimeout(ctx, 10*time.Second)
	store, err := storage.NewRequestStore(dbCtx, dsn, "data/script.sql")
	dbCancel()
	if err != nil {
		slog.Error("init db failed", "err", err)
		os.Exit(1)
	}
	defer func() {
		if err := store.Close(); err != nil {
			slog.Warn("close db failed", "err", err)
		}
	}()

	const maxConcurrent = 20
	pool := make(chan struct{}, maxConcurrent)

	h := api.NewHandler(g, store)
	base := h.Routes()

	limited := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		pool <- struct{}{}
		defer func() { <-pool }()
		base.ServeHTTP(w, r)
	})

	srv := &http.Server{
		Addr:              addr,
		Handler:           limited,
		ReadHeaderTimeout: 5 * time.Second,
		ReadTimeout:       10 * time.Second,
		WriteTimeout:      15 * time.Second,
		IdleTimeout:       60 * time.Second,
	}

	serverErr := make(chan error, 1)
	go func() {
		slog.Info("http server started", "addr", addr, "maxConcurrent", maxConcurrent)
		if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			serverErr <- err
		}
	}()

	select {
	case <-ctx.Done():
		slog.Info("try to stop http server")
	case err := <-serverErr:
		slog.Error("http server failed", "err", err)
		stop()
	}

	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer shutdownCancel()

	if err := srv.Shutdown(shutdownCtx); err != nil {
		slog.Warn("shutdown failed", "err", err)
	}
	slog.Info("http server stopped")
}

func setupLogger() {
	level := slog.LevelInfo
	if os.Getenv("LOG_LEVEL") == "debug" {
		level = slog.LevelDebug
	}
	slog.SetDefault(slog.New(slog.NewTextHandler(os.Stdout, &slog.HandlerOptions{
		Level: level,
	})))
}
