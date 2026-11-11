package storage

import (
	"context"
	"database/sql"
	"fmt"
	"os"
	"strings"
)

func SetupSchema(ctx context.Context, db *sql.DB, path string) error {
	sqlBytes, err := os.ReadFile(path)
	if err != nil {
		return fmt.Errorf("read schema sql %q: %w", path, err)
	}

	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		return fmt.Errorf("begin tx: %w", err)
	}
	defer func() { _ = tx.Rollback() }()

	for i, stmt := range splitStatements(string(sqlBytes)) {
		if _, err := tx.ExecContext(ctx, stmt); err != nil {
			return fmt.Errorf("apply schema sql %q (statement %d): %w", path, i+1, err)
		}
	}

	if err := tx.Commit(); err != nil {
		return fmt.Errorf("commit schema: %w", err)
	}
	return nil
}

func splitStatements(sqlText string) []string {
	var out []string
	for _, chunk := range strings.Split(sqlText, ";") {
		s := strings.TrimSpace(chunk)
		if s == "" {
			continue
		}
		out = append(out, s)
	}
	return out
}
