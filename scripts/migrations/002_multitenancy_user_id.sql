-- Migration 002: Add user_id to routines, trainings, and body_weights for multi-tenancy

ALTER TABLE `routines` ADD COLUMN `user_id` INT NULL AFTER `id`, ADD KEY `ix_routines_user` (`user_id`);
ALTER TABLE `trainings` ADD COLUMN `user_id` INT NULL AFTER `id`, ADD KEY `ix_trainings_user` (`user_id`);
ALTER TABLE `body_weights` ADD COLUMN `user_id` INT NULL AFTER `id`, ADD KEY `ix_bodyweights_user` (`user_id`);

-- Optional: Assign existing data to the first registered user if needed
-- UPDATE `routines` SET `user_id` = 1 WHERE `user_id` IS NULL;
-- UPDATE `trainings` SET `user_id` = 1 WHERE `user_id` IS NULL;
-- UPDATE `body_weights` SET `user_id` = 1 WHERE `user_id` IS NULL;
