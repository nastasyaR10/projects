package storage

import (
	"context"
	"os"
	"testing"
	"time"

	"github.com/joho/godotenv"
)

func newTestStore(t *testing.T) (*RequestStore, int64) {
	t.Helper()
	_ = godotenv.Load("../../.env")

	dsn := os.Getenv("TEST_DATABASE_URL")
	if dsn == "" {
		t.Skip("TEST_DATABASE_URL not set, skipping DB test")
	}
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	store, err := NewRequestStore(ctx, dsn, "../../data/script.sql")
	if err != nil {
		t.Fatalf("NewRequestStore: %v", err)
	}
	userID := uniqueUserID()

	t.Cleanup(func() {
		_, err := store.db.ExecContext(context.Background(),
			`DELETE FROM requests WHERE user_id = $1`, userID)
		if err != nil {
			t.Logf("cleanup delete: %v", err)
		}
		if err := store.Close(); err != nil {
			t.Logf("store.Close: %v", err)
		}
	})
	return store, userID
}

func uniqueUserID() int64 {
	return time.Now().UnixNano() % 1_000_000_000
}

func TestSaveAndList(t *testing.T) {
	store, userID := newTestStore(t)
	ctx := context.Background()

	if err := store.SaveRequest(ctx, userID, "park_kultury_s", "komsomolskaya_k", "fast"); err != nil {
		t.Fatalf("SaveRequest (1): %v", err)
	}
	time.Sleep(2 * time.Millisecond)
	if err := store.SaveRequest(ctx, userID, "sokolniki_s", "belorusskaya_z", "fewest_transfers"); err != nil {
		t.Fatalf("SaveRequest (2): %v", err)
	}

	reqs, err := store.ListByUser(ctx, userID, 10)
	if err != nil {
		t.Fatalf("ListByUser: %v", err)
	}
	if len(reqs) != 2 {
		t.Fatalf("expected 2 requests, got %d", len(reqs))
	}

	if reqs[0].FromStation != "sokolniki_s" || reqs[0].ToStation != "belorusskaya_z" {
		t.Fatalf("expected newest first (sokolniki_s → belorusskaya_z), got %q → %q",
			reqs[0].FromStation, reqs[0].ToStation)
	}
	if reqs[0].Mode != "fewest_transfers" {
		t.Fatalf("expected mode fewest_transfers, got %q", reqs[0].Mode)
	}
	if reqs[1].FromStation != "park_kultury_s" || reqs[1].Mode != "fast" {
		t.Fatalf("unexpected second row: %+v", reqs[1])
	}

	if reqs[0].CreatedAt.IsZero() {
		t.Fatal("CreatedAt is zero, Scan into time.Time failed")
	}
	if reqs[0].UserID != userID {
		t.Fatalf("expected userID %d, got %d", userID, reqs[0].UserID)
	}
}

func TestListByUser_Empty(t *testing.T) {
	store, _ := newTestStore(t)
	reqs, err := store.ListByUser(context.Background(), -1, 10)
	if err != nil {
		t.Fatalf("ListByUser: %v", err)
	}
	if len(reqs) != 0 {
		t.Fatalf("expected empty, got %d", len(reqs))
	}
}

func TestListByUser_Limit(t *testing.T) {
	store, userID := newTestStore(t)
	ctx := context.Background()

	for i := 0; i < 5; i++ {
		if err := store.SaveRequest(ctx, userID, "a", "b", "fast"); err != nil {
			t.Fatalf("SaveRequest #%d: %v", i, err)
		}
	}

	reqs, err := store.ListByUser(ctx, userID, 2)
	if err != nil {
		t.Fatalf("ListByUser: %v", err)
	}
	if len(reqs) != 2 {
		t.Fatalf("expected 2 (limit), got %d", len(reqs))
	}
}

func TestSaveRequest_CancelledContext(t *testing.T) {
	store, _ := newTestStore(t)

	ctx, cancel := context.WithCancel(context.Background())
	cancel()

	err := store.SaveRequest(ctx, uniqueUserID(), "a", "b", "fast")
	if err == nil {
		t.Fatal("expected error on cancelled context")
	}
}

func TestSetupSchema_Idempotent(t *testing.T) {
	store, _ := newTestStore(t)
	ctx := context.Background()

	if err := SetupSchema(ctx, store.db, "../../data/script.sql"); err != nil {
		t.Fatalf("SetupSchema (second run): %v", err)
	}
}
