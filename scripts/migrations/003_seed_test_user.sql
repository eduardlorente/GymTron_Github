-- Migration 003: Seed local test user
-- Credentials: Username='user' | Email='user@gymtron.local' | Password='password'

INSERT INTO `users` (`id`, `username`, `email`, `password_hash`, `is_active`, `created_at`)
VALUES 
    (1, 'user', 'user@gymtron.local', 'GkRdnMxtr1jvD87kEmZPfw==:wRrYCQAK2CyLUFDru+eXqIK1QBi5sOXhTvuP1AhWIBc=:100000:SHA256', 1, NOW())
ON DUPLICATE KEY UPDATE `password_hash` = VALUES(`password_hash`);

-- Assign existing seed routines, trainings, and body weights to user 1
UPDATE `routines` SET `user_id` = 1 WHERE `user_id` IS NULL;
UPDATE `trainings` SET `user_id` = 1 WHERE `user_id` IS NULL;
UPDATE `body_weights` SET `user_id` = 1 WHERE `user_id` IS NULL;
