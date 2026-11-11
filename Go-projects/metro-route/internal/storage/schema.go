package storage

import (
	"encoding/json"
	"fmt"
	"os"

	"github.com/nastasyaR10/projects/metro-route/internal/models"
)

func LoadSchema(path string) (*models.MetroSchema, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("read schema file: %w", err)
	}

	var schema models.MetroSchema
	if err := json.Unmarshal(data, &schema); err != nil {
		return nil, fmt.Errorf("parse schema json: %w", err)
	}

	return &schema, nil
}
