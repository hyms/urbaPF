-- Migration: 003_add_photo_url_to_users

-- Add photo_url column to users table
ALTER TABLE users ADD COLUMN IF NOT EXISTS photo_url TEXT;
