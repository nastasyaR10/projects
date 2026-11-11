package main

import (
	"context"
	"log/slog"
	"os"
	"os/signal"
	"sync"
	"syscall"
	"time"

	"github.com/go-telegram/bot"
	"github.com/go-telegram/bot/models"
	"github.com/joho/godotenv"

	"github.com/nastasyaR10/projects/metro-route/internal/storage"
	"github.com/nastasyaR10/projects/metro-route/internal/telegram"
)

func main() {
	_ = godotenv.Load()
	setupLogger()

	ctx, cancel := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer cancel()

	token := os.Getenv("BOT_TOKEN")
	if token == "" {
		slog.Error("BOT_TOKEN is not set")
		os.Exit(1)
	}

	dsn := os.Getenv("DATABASE_URL")
	if dsn == "" {
		slog.Error("DATABASE_URL is not set")
		os.Exit(1)
	}

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

	h := telegram.NewHandler(g, telegram.NewStateStore(), store)

	const maxConcurrent = 20
	pool := make(chan struct{}, maxConcurrent)
	var wg sync.WaitGroup

	b, err := bot.New(token, bot.WithDefaultHandler(func(ctx context.Context, b *bot.Bot, update *models.Update) {
		wg.Add(1)
		defer wg.Done()

		pool <- struct{}{}
		defer func() { <-pool }()

		reqCtx, cancel := context.WithTimeout(ctx, 10*time.Second)
		defer cancel()

		h.Handle(reqCtx, b, update)
	}))
	if err != nil {
		slog.Error("create bot failed", "err", err)
		os.Exit(1)
	}

	slog.Info("bot started, press Ctrl+C to stop", "maxConcurrent", maxConcurrent)
	b.Start(ctx)

	done := make(chan struct{})
	go func() {
		wg.Wait()
		close(done)
	}()

	select {
	case <-done:
		slog.Info("all handlers finished")
	case <-time.After(5 * time.Second):
		slog.Warn("timeout waiting for handlers, exit anyway", "timeout", 5*time.Second)
	}
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
