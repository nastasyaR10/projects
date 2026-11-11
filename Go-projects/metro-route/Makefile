.PHONY: help test test-race test-more build run-api run-bot up up-api up-bot down

help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-12s\033[0m %s\n", $$1, $$2}'

test: ## Запустить тесты (опционально: для тестирования БД предварительно выполнить команду up)
	go test ./...

test-race: ## Запустить тесты с race detector (опционально: для тестирования БД предварительно выполнить команду up)
	go test -race ./...

test-more: ## Запустить тесты с подробным выводом и покрытием (опционально: для тестирования БД предварительно выполнить команду up)
	go test -v -cover ./...

build: ## Собрать бинарники в bin/
	go build -o bin/api ./cmd/api
	go build -o bin/bot ./cmd/bot

run-api: up ## Запустить HTTP-сервер
	go run ./cmd/api

run-bot: up ## Запустить Telegram-бота
	go run ./cmd/bot

up: ## Поднять Postgres
	docker compose up -d --wait

up-api: ## Поднять Postgres + HTTP-сервер
	docker compose --profile api up -d --wait

up-bot: ## Поднять Postgres + бот
	docker compose --profile bot up -d --wait

down: ## Остановить все контейнеры
	docker compose down