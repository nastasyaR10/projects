package storage

import (
	"errors"
	"io/fs"
	"os"
	"path/filepath"
	"testing"
)

func TestLoadSchema_RealFile(t *testing.T) {
	schema, err := LoadSchema("../../data/metro.json")
	if err != nil {
		t.Fatalf("LoadSchema: %v", err)
	}
	if len(schema.Stations) != 70 {
		t.Fatalf("expected 70 stations, got %d", len(schema.Stations))
	}
	if len(schema.Edges) != 67 {
		t.Fatalf("expected 67 edges, got %d", len(schema.Edges))
	}
	if len(schema.Transfers) != 9 {
		t.Fatalf("expected 9 transfers, got %d", len(schema.Transfers))
	}
}

func TestLoadSchema_FileNotFound(t *testing.T) {
	_, err := LoadSchema("../../data/no_such_file.json")
	if err == nil {
		t.Fatal("expected error for missing file")
	}
	if !errors.Is(err, fs.ErrNotExist) {
		t.Fatalf("expected not-exist error, got %v", err)
	}
}

func TestLoadSchema_InvalidJSON(t *testing.T) {
	dir := t.TempDir()
	path := filepath.Join(dir, "bad.json")
	if err := os.WriteFile(path, []byte(`{"stations": [`), 0644); err != nil {
		t.Fatalf("write temp file: %v", err)
	}

	_, err := LoadSchema(path)
	if err == nil {
		t.Fatal("expected error for invalid JSON")
	}
}
