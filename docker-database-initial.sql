CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE player_profiles (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    rank_points INTEGER DEFAULT 0,
    wins INTEGER DEFAULT 0,
    losses INTEGER DEFAULT 0,
    current_streak INTEGER DEFAULT 0,
    max_streak INTEGER DEFAULT 0,
    medals_json JSONB DEFAULT '[]'::jsonb,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE matches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player1_id UUID NOT NULL REFERENCES users(id),
    player2_id UUID REFERENCES users(id), -- Pode ser NULL se for PvE (vs IA)
    winner_id UUID REFERENCES users(id),

    -- Enums convertidos para String
    game_mode VARCHAR(20) NOT NULL CHECK (game_mode IN ('Classic', 'Dynamic')),
    ai_difficulty VARCHAR(20) CHECK (ai_difficulty IN ('Basic', 'Intermediate', 'Advanced')),
    status VARCHAR(20) NOT NULL DEFAULT 'Setup' CHECK (status IN ('Setup', 'InProgress', 'Finished')),

    -- Controle de Turno e Tempo
    current_turn_player_id UUID, -- Persistência de quem é a vez
    started_at TIMESTAMP WITH TIME ZONE NOT NULL,
    last_move_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP, -- Para validar os 30s
    finished_at TIMESTAMP WITH TIME ZONE,

    -- Tabuleiros completos como JSON
    player1_board_json JSONB DEFAULT '{}'::jsonb,
    player2_board_json JSONB DEFAULT '{}'::jsonb
);

CREATE TABLE medals (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NOT NULL,
    code VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE user_medals (
    user_id UUID NOT NULL REFERENCES users(id),
    medal_id INTEGER NOT NULL REFERENCES medals(id),
    earned_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, medal_id)
);

CREATE INDEX idx_profiles_rank_points ON player_profiles (rank_points DESC);
CREATE INDEX idx_matches_player1 ON matches (player1_id);
CREATE INDEX idx_matches_player2 ON matches (player2_id);
CREATE INDEX idx_matches_status ON matches (status);


INSERT INTO medals (name, description, code) VALUES
('Almirante', 'Vencer sem perder navios.', 'ADMIRAL'),
('Capitão de Mar e Guerra', 'Acertar 8 tiros seguidos.', 'CAPTAIN_WAR'),
('Capitão', 'Acertar 7 tiros seguidos.', 'CAPTAIN'),
('Marinheiro', 'Vencer em determinado tempo.', 'SAILOR');