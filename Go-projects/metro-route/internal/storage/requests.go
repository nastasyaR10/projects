package storage

import (
	"context"
	"database/sql"
	"fmt"
	"time"

	_ "github.com/jackc/pgx/v5/stdlib"
)

type Request struct {
	ID          int64
	UserID      int64
	FromStation string
	ToStation   string
	Mode        string
	CreatedAt   time.Time
}

type RequestStore struct {
	db *sql.DB
}

func NewRequestStore(ctx context.Context, dsn, schemaPath string) (*RequestStore, error) {
	db, err := sql.Open("pgx", dsn)
	if err != nil {
		return nil, fmt.Errorf("sql.Open: %w", err)
	}

	db.SetMaxOpenConns(10)
	db.SetMaxIdleConns(5)

	if err := db.PingContext(ctx); err != nil {
		_ = db.Close()
		return nil, fmt.Errorf("ping db: %w", err)
	}

	if err := SetupSchema(ctx, db, schemaPath); err != nil {
		_ = db.Close()
		return nil, fmt.Errorf("setup schema: %w", err)
	}

	return &RequestStore{db: db}, nil
}

func (s *RequestStore) Close() error {
	return s.db.Close()
}

func (s *RequestStore) SaveRequest(ctx context.Context, userID int64, from, to, mode string) error {
	const q = `
		INSERT INTO requests (user_id, from_station, to_station, mode)
		VALUES ($1, $2, $3, $4)`

	var lastErr error
	for i := 0; i < 3; i++ {
		if i > 0 {
			select {
			case <-ctx.Done():
				return fmt.Errorf("save request: %w (last error: %v)", ctx.Err(), lastErr)
			case <-time.After(time.Duration(i) * 200 * time.Millisecond):
			}
		}
		if _, err := s.db.ExecContext(ctx, q, userID, from, to, mode); err == nil {
			return nil
		} else {
			lastErr = err
		}
	}
	return fmt.Errorf("save request after retries: %w", lastErr)
}

func (s *RequestStore) ListByUser(ctx context.Context, userID int64, limit int) ([]*Request, error) {
	const q = `
		SELECT id, user_id, from_station, to_station, mode, created_at
		FROM requests
		WHERE user_id = $1
		ORDER BY created_at DESC
		LIMIT $2
		`

	rows, err := s.db.QueryContext(ctx, q, userID, limit)
	if err != nil {
		return nil, fmt.Errorf("query requests: %w", err)
	}
	defer rows.Close()

	out := make([]*Request, 0, limit)
	for rows.Next() {
		var r Request
		if err := rows.Scan(&r.ID, &r.UserID, &r.FromStation, &r.ToStation, &r.Mode, &r.CreatedAt); err != nil {
			return nil, fmt.Errorf("scan request: %w", err)
		}
		out = append(out, &r)
	}
	if err := rows.Err(); err != nil {
		return nil, fmt.Errorf("rows iteration: %w", err)
	}
	return out, nil
}
