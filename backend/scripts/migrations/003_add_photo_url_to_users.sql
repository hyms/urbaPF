-- Migration: 003_add_photo_url_to_users.sql

-- Add photo_url column to users table
ALTER TABLE users ADD COLUMN photo_url TEXT NULL;