-- Migration 004: Add type_id to users and seed initial administrator
-- User Types: Undefined = 0, Standard = 1, Administrator = 2

-- 1. Add type_id column with default value 1 (Standard)
ALTER TABLE `users` ADD COLUMN `type_id` INT NOT NULL DEFAULT 1 AFTER `password_hash`;

-- 2. Seed Administrator user for administrative access
-- Credentials: Username='administrator' | Email='administrator@gymtron.local' | Password='password'
INSERT INTO `users` (`username`, `email`, `password_hash`, `type_id`, `is_active`, `created_at`)
VALUES 
    ('administrator', 'administrator@gymtron.local', 'GkRdnMxtr1jvD87kEmZPfw==:wRrYCQAK2CyLUFDru+eXqIK1QBi5sOXhTvuP1AhWIBc=:100000:SHA256', 2, 1, NOW())
ON DUPLICATE KEY UPDATE `type_id` = 2, `password_hash` = VALUES(`password_hash`), `is_active` = 1;
