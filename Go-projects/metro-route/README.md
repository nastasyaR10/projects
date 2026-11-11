# Metro Route

Сервис построения маршрутов в московском метро с двумя интерфейсами:
Telegram-бот и HTTP API.

## Быстрый старт

Требования: Go 1.27+, Docker Compose.

```bash
# 1. Создать файл .env и указать
# BOT_TOKEN=...       - токен Telegram-бота от @BotFather
# LOG_LEVEL=...     - вариант вывода логов (info/debug) (по умолчанию info)
# DATABASE_URL=postgres://metro:metro@localhost:5433/metro-db?sslmode=disable - cтрока подключения к Postgres
# TEST_DATABASE_URL=... - строка подключения к тестируемой БД (может совпадать с DATABASE_URL)
# HTTP_ADDR=... - адрес HTTP-сервера (по умолчанию :8080)

# 2. Поднять Postgres
make up

# 3. Запустить HTTP-сервер (Postgres + API)
make run-api
# или Telegram-бота (Postgres + бот)
make run-bot

# 4. Для прекращения работы программы
# Ctrl+C в терминале, где запущен run-api / run-bot - корректное завершение
make down      # остановить контейнеры

# Для более подробной информации о тестировании, сборке, запуске в Docker
make help
