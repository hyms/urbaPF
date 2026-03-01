-- Migration: 001_initial_schema
-- Date: 2026-02-27
-- Description: Initial database schema for UrbaPF

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Condominiums (Conjuntos residenciales)
CREATE TABLE IF NOT EXISTS condominiums (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    logo_url TEXT,
    description TEXT,
    rules TEXT,
    monthly_fee DECIMAL(12, 2) DEFAULT 0,
    currency VARCHAR(3) DEFAULT 'BOB',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT true
);

-- Users
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    role INTEGER DEFAULT 0 CHECK (role BETWEEN 0 AND 4),
    credibility_level INTEGER DEFAULT 1 CHECK (credibility_level BETWEEN 1 AND 5),
    status INTEGER DEFAULT 0 CHECK (status BETWEEN 0 AND 3),
    condominium_id UUID REFERENCES condominiums(id) ON DELETE SET NULL,
    lot_number VARCHAR(50),
    street_address VARCHAR(255),
    photo_url TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    fcm_token TEXT,
    is_validated BOOLEAN DEFAULT false,
    manager_votes INTEGER DEFAULT 0
);

-- Incidents
CREATE TABLE IF NOT EXISTS incidents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominium_id UUID NOT NULL REFERENCES condominiums(id) ON DELETE CASCADE,
    reported_by_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    type INTEGER NOT NULL CHECK (type BETWEEN 1 AND 99),
    priority INTEGER DEFAULT 2 CHECK (priority BETWEEN 1 AND 5),
    status INTEGER DEFAULT 1 CHECK (status BETWEEN 1 AND 6),
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    location_description TEXT,
    occurred_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    resolved_at TIMESTAMP WITH TIME ZONE,
    resolved_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    resolution_notes TEXT
);

-- Posts
CREATE TABLE IF NOT EXISTS posts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominium_id UUID NOT NULL REFERENCES condominiums(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    category INTEGER DEFAULT 1 CHECK (category BETWEEN 1 AND 10),
    is_pinned BOOLEAN DEFAULT false,
    is_announcement BOOLEAN DEFAULT false,
    status INTEGER DEFAULT 1 CHECK (status BETWEEN 1 AND 5),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    published_at TIMESTAMP WITH TIME ZONE,
    view_count INTEGER DEFAULT 0,
    approved_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    approved_at TIMESTAMP WITH TIME ZONE
);

-- Comments
CREATE TABLE IF NOT EXISTS comments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    post_id UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    author_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    parent_comment_id UUID REFERENCES comments(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    credibility_level INTEGER DEFAULT 1 CHECK (credibility_level BETWEEN 1 AND 5),
    is_hidden BOOLEAN DEFAULT false,
    is_edited BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Polls (Asambleas virtuales)
CREATE TABLE IF NOT EXISTS polls (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominium_id UUID NOT NULL REFERENCES condominiums(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    options JSONB NOT NULL DEFAULT '[]',
    poll_type INTEGER DEFAULT 1 CHECK (poll_type BETWEEN 1 AND 4),
    starts_at TIMESTAMP WITH TIME ZONE NOT NULL,
    ends_at TIMESTAMP WITH TIME ZONE NOT NULL,
    requires_justification BOOLEAN DEFAULT false,
    min_role_to_vote INTEGER DEFAULT 2,
    server_secret VARCHAR(255) NOT NULL,
    status INTEGER DEFAULT 1 CHECK (status BETWEEN 1 AND 5),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE
);

-- Votes
CREATE TABLE IF NOT EXISTS votes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    poll_id UUID NOT NULL REFERENCES polls(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    option_index INTEGER NOT NULL,
    digital_signature VARCHAR(255) NOT NULL,
    voted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45),
    UNIQUE(poll_id, user_id)
);

-- Alerts (En Camino)
CREATE TABLE IF NOT EXISTS alerts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominium_id UUID NOT NULL REFERENCES condominiums(id) ON DELETE CASCADE,
    created_by_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    alert_type INTEGER NOT NULL CHECK (alert_type BETWEEN 1 AND 4),
    message TEXT NOT NULL,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    destination_address TEXT,
    estimated_arrival TIMESTAMP WITH TIME ZONE NOT NULL,
    status INTEGER DEFAULT 1 CHECK (status BETWEEN 1 AND 6),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    acknowledged_at TIMESTAMP WITH TIME ZONE,
    arrived_at TIMESTAMP WITH TIME ZONE
);

-- Expenses (Expensas)
CREATE TABLE IF NOT EXISTS expenses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominium_id UUID NOT NULL REFERENCES condominiums(id) ON DELETE CASCADE,
    description TEXT NOT NULL,
    amount DECIMAL(12, 2) NOT NULL,
    category VARCHAR(100) NOT NULL,
    due_date TIMESTAMP WITH TIME ZONE NOT NULL,
    paid_at TIMESTAMP WITH TIME ZONE,
    paid_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    receipt_url TEXT,
    status INTEGER DEFAULT 1 CHECK (status BETWEEN 1 AND 4),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_condominium ON users(condominium_id);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_incidents_condominium ON incidents(condominium_id);
CREATE INDEX IF NOT EXISTS idx_incidents_status ON incidents(status);
CREATE INDEX IF NOT EXISTS idx_posts_condominium ON posts(condominium_id);
CREATE INDEX IF NOT EXISTS idx_posts_author ON posts(author_id);
CREATE INDEX IF NOT EXISTS idx_posts_status ON posts(status);
CREATE INDEX IF NOT EXISTS idx_comments_post ON comments(post_id);
CREATE INDEX IF NOT EXISTS idx_comments_credibility ON comments(credibility_level DESC, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_polls_condominium ON polls(condominium_id);
CREATE INDEX IF NOT EXISTS idx_polls_status ON polls(status);
CREATE INDEX IF NOT EXISTS idx_votes_poll ON votes(poll_id);
CREATE INDEX IF NOT EXISTS idx_alerts_condominium ON alerts(condominium_id);
CREATE INDEX IF NOT EXISTS idx_alerts_status ON alerts(status);
CREATE INDEX IF NOT EXISTS idx_expenses_condominium ON expenses(condominium_id);
CREATE INDEX IF NOT EXISTS idx_expenses_status ON expenses(status);

-- Insert default admin user (password: Admin123!)
INSERT INTO users (id, email, password_hash, full_name, role, status, is_validated)
VALUES (
    uuid_generate_v4(),
    'admin@urbapf.local',
    '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYI6eKqW.yy', -- Admin123!
    'Administrator',
    4, -- Administrator
    1, -- Active
    true
) ON CONFLICT (email) DO NOTHING;
