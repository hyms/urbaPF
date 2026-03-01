-- Migration: 002_audit_and_soft_delete
-- Date: 2026-02-27
-- Description: Add audit tables and soft delete columns (using deleted_at only)

-- Add soft delete columns to existing tables
ALTER TABLE users ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;
ALTER TABLE users ADD COLUMN deleted_by_id UUID REFERENCES users(id);

ALTER TABLE condominiums ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE incidents ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE posts ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE comments ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE polls ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE votes ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE alerts ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

ALTER TABLE expenses ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE;

-- Drop old is_deleted columns if they exist (optional - comment out if you want to keep them)
ALTER TABLE users DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE condominiums DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE incidents DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE posts DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE comments DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE polls DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE votes DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE alerts DROP COLUMN IF EXISTS is_deleted;
ALTER TABLE expenses DROP COLUMN IF EXISTS is_deleted;

-- Audit log table
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    table_name VARCHAR(100) NOT NULL,
    record_id UUID NOT NULL,
    action VARCHAR(20) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    changed_by_id UUID REFERENCES users(id) ON DELETE SET NULL,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Audit indexes
CREATE INDEX IF NOT EXISTS idx_audit_logs_table_record ON audit_logs(table_name, record_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_by ON audit_logs(changed_by_id);

-- Create audit function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO audit_logs (table_name, record_id, action, new_values, changed_by_id, ip_address)
        VALUES (TG_TABLE_NAME, NEW.id, 'INSERT', to_jsonb(NEW), 
                (SELECT id FROM users WHERE email = current_user LIMIT 1),
                COALESCE(current_setting('app.client_addr', true), '0.0.0.0')::inet);
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO audit_logs (table_name, record_id, action, old_values, new_values, changed_by_id, ip_address)
        VALUES (TG_TABLE_NAME, OLD.id, 'UPDATE', to_jsonb(OLD), to_jsonb(NEW),
                (SELECT id FROM users WHERE email = current_user LIMIT 1),
                COALESCE(current_setting('app.client_addr', true), '0.0.0.0')::inet);
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO audit_logs (table_name, record_id, action, old_values, changed_by_id, ip_address)
        VALUES (TG_TABLE_NAME, OLD.id, 'DELETE', to_jsonb(OLD),
                (SELECT id FROM users WHERE email = current_user LIMIT 1),
                COALESCE(current_setting('app.client_addr', true), '0.0.0.0')::inet);
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit triggers for each table
DROP TRIGGER IF EXISTS audit_users_trigger ON users;
CREATE TRIGGER audit_users_trigger AFTER INSERT OR UPDATE OR DELETE ON users
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_condominiums_trigger ON condominiums;
CREATE TRIGGER audit_condominiums_trigger AFTER INSERT OR UPDATE OR DELETE ON condominiums
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_incidents_trigger ON incidents;
CREATE TRIGGER audit_incidents_trigger AFTER INSERT OR UPDATE OR DELETE ON incidents
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_posts_trigger ON posts;
CREATE TRIGGER audit_posts_trigger AFTER INSERT OR UPDATE OR DELETE ON posts
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_comments_trigger ON comments;
CREATE TRIGGER audit_comments_trigger AFTER INSERT OR UPDATE OR DELETE ON comments
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_polls_trigger ON polls;
CREATE TRIGGER audit_polls_trigger AFTER INSERT OR UPDATE OR DELETE ON polls
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_votes_trigger ON votes;
CREATE TRIGGER audit_votes_trigger AFTER INSERT OR UPDATE OR DELETE ON votes
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_alerts_trigger ON alerts;
CREATE TRIGGER audit_alerts_trigger AFTER INSERT OR UPDATE OR DELETE ON alerts
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

DROP TRIGGER IF EXISTS audit_expenses_trigger ON expenses;
CREATE TRIGGER audit_expenses_trigger AFTER INSERT OR UPDATE OR DELETE ON expenses
FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

-- Soft delete indexes (using deleted_at)
CREATE INDEX IF NOT EXISTS idx_users_deleted ON users(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_condominiums_deleted ON condominiums(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_incidents_deleted ON incidents(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_posts_deleted ON posts(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_comments_deleted ON comments(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_polls_deleted ON polls(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_votes_deleted ON votes(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_alerts_deleted ON alerts(deleted_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_expenses_deleted ON expenses(deleted_at) WHERE deleted_at IS NULL;
