CREATE TABLE IF NOT EXISTS requests (
    id           BIGSERIAL PRIMARY KEY,
    user_id      BIGINT      NOT NULL,
    from_station TEXT        NOT NULL,
    to_station   TEXT        NOT NULL,
    mode         TEXT        NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_requests_user_created ON requests (user_id, created_at DESC);