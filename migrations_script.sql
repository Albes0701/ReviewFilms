CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `migration_id` varchar(150) NOT NULL,
    `product_version` varchar(32) NOT NULL,
    PRIMARY KEY (`migration_id`)
);

START TRANSACTION;
CREATE TABLE `genres` (
    `id` char(36) NOT NULL,
    `name` varchar(100) NOT NULL,
    `slug` varchar(100) NOT NULL,
    `description` longtext NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `permission` (
    `id` char(36) NOT NULL,
    `code` varchar(100) NOT NULL,
    `name` varchar(100) NOT NULL,
    `module` varchar(100) NULL,
    `description` longtext NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `persons` (
    `id` char(36) NOT NULL,
    `name` varchar(255) NOT NULL,
    `original_name` varchar(255) NULL,
    `known_for_department` varchar(100) NULL,
    `gender` varchar(20) NULL,
    `birthday` date NULL,
    `deathday` date NULL,
    `place_of_birth` varchar(255) NULL,
    `biography` longtext NULL,
    `profile_url` varchar(500) NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `role` (
    `id` char(36) NOT NULL,
    `code` varchar(50) NOT NULL,
    `name` varchar(100) NOT NULL,
    `description` longtext NULL,
    `is_system` tinyint(1) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `user` (
    `id` char(36) NOT NULL,
    `username` varchar(50) NOT NULL,
    `email` varchar(255) NOT NULL,
    `password_hash` varchar(255) NOT NULL,
    `display_name` varchar(100) NOT NULL,
    `avatar_url` varchar(500) NULL,
    `bio` longtext NULL,
    `status` varchar(50) NOT NULL,
    `email_confirmed` tinyint(1) NOT NULL,
    `last_login_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    `deleted_at` datetime(6) NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `movie` (
    `id` char(36) NOT NULL,
    `title` varchar(255) NOT NULL,
    `original_title` varchar(255) NULL,
    `slug` varchar(255) NOT NULL,
    `overview` longtext NULL,
    `release_date` date NULL,
    `runtime_minutes` int NULL,
    `age_rating` varchar(20) NULL,
    `original_language` varchar(10) NULL,
    `poster_url` varchar(500) NULL,
    `backdrop_url` varchar(500) NULL,
    `trailer_url` varchar(500) NULL,
    `avg_rating` decimal(3,2) NULL,
    `rating_count` int NOT NULL,
    `comment_count` int NOT NULL,
    `status` varchar(50) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    `created_by` char(36) NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_movie_user_created_by_user_id` FOREIGN KEY (`created_by`) REFERENCES `user` (`id`)
);

CREATE TABLE `notification` (
    `id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `type` varchar(50) NOT NULL,
    `title` varchar(255) NOT NULL,
    `message` longtext NOT NULL,
    `data_json` json NULL,
    `is_read` tinyint(1) NOT NULL,
    `read_at` datetime(6) NULL,
    `expires_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_notification_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `refresh_token` (
    `id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `token_hash` varchar(500) NOT NULL,
    `jwt_id` varchar(255) NULL,
    `device_name` varchar(255) NULL,
    `ip_address` varchar(64) NULL,
    `user_agent` longtext NULL,
    `expires_at` datetime(6) NOT NULL,
    `revoked_at` datetime(6) NULL,
    `replaced_by_token_id` char(36) NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_refresh_token_refresh_token_replaced_by_token_id` FOREIGN KEY (`replaced_by_token_id`) REFERENCES `refresh_token` (`id`),
    CONSTRAINT `fk_refresh_token_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `report` (
    `id` char(36) NOT NULL,
    `reporter_user_id` char(36) NOT NULL,
    `target_type` varchar(50) NOT NULL,
    `target_id` char(36) NOT NULL,
    `reason_code` varchar(100) NOT NULL,
    `description` longtext NULL,
    `status` varchar(50) NOT NULL,
    `reviewed_by` char(36) NULL,
    `resolution_note` longtext NULL,
    `reviewed_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_report_user_reporter_user_id` FOREIGN KEY (`reporter_user_id`) REFERENCES `user` (`id`),
    CONSTRAINT `fk_report_user_reviewed_by_user_id` FOREIGN KEY (`reviewed_by`) REFERENCES `user` (`id`)
);

CREATE TABLE `role_permission` (
    `role_id` char(36) NOT NULL,
    `permission_id` char(36) NOT NULL,
    `created_by` char(36) NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`role_id`, `permission_id`),
    CONSTRAINT `fk_role_permission_permission_permission_id` FOREIGN KEY (`permission_id`) REFERENCES `permission` (`id`),
    CONSTRAINT `fk_role_permission_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`),
    CONSTRAINT `fk_role_permission_user_created_by_user_id` FOREIGN KEY (`created_by`) REFERENCES `user` (`id`)
);

CREATE TABLE `user_role` (
    `user_id` char(36) NOT NULL,
    `role_id` char(36) NOT NULL,
    `assigned_by` char(36) NULL,
    `assigned_at` datetime(6) NOT NULL,
    PRIMARY KEY (`user_id`, `role_id`),
    CONSTRAINT `fk_user_role_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`),
    CONSTRAINT `fk_user_role_user_assigned_by_user_id` FOREIGN KEY (`assigned_by`) REFERENCES `user` (`id`),
    CONSTRAINT `fk_user_role_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `comment` (
    `id` char(36) NOT NULL,
    `movie_id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `parent_id` char(36) NULL,
    `root_id` char(36) NULL,
    `content` longtext NOT NULL,
    `depth` int NOT NULL,
    `score` int NOT NULL,
    `upvote_count` int NOT NULL,
    `downvote_count` int NOT NULL,
    `reply_count` int NOT NULL,
    `is_edited` tinyint(1) NOT NULL,
    `edited_at` datetime(6) NULL,
    `status` varchar(50) NOT NULL,
    `deleted_at` datetime(6) NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_comment_comment_parent_id` FOREIGN KEY (`parent_id`) REFERENCES `comment` (`id`),
    CONSTRAINT `fk_comment_comment_root_id` FOREIGN KEY (`root_id`) REFERENCES `comment` (`id`),
    CONSTRAINT `fk_comment_movie_movie_id` FOREIGN KEY (`movie_id`) REFERENCES `movie` (`id`),
    CONSTRAINT `fk_comment_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `movie_genres` (
    `movie_id` char(36) NOT NULL,
    `genre_id` char(36) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`movie_id`, `genre_id`),
    CONSTRAINT `fk_movie_genres_genres_genre_id` FOREIGN KEY (`genre_id`) REFERENCES `genres` (`id`),
    CONSTRAINT `fk_movie_genres_movies_movie_id` FOREIGN KEY (`movie_id`) REFERENCES `movie` (`id`)
);

CREATE TABLE `movie_rating` (
    `id` char(36) NOT NULL,
    `movie_id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `score` int NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_movie_rating_movie_movie_id` FOREIGN KEY (`movie_id`) REFERENCES `movie` (`id`),
    CONSTRAINT `fk_movie_rating_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `moviecredits` (
    `id` char(36) NOT NULL,
    `movie_id` char(36) NOT NULL,
    `person_id` char(36) NOT NULL,
    `credit_type` varchar(50) NOT NULL,
    `department` varchar(100) NULL,
    `job` varchar(100) NULL,
    `character_name` varchar(255) NULL,
    `billing_order` int NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_moviecredits_movie_movie_id` FOREIGN KEY (`movie_id`) REFERENCES `movie` (`id`),
    CONSTRAINT `fk_moviecredits_persons_person_id` FOREIGN KEY (`person_id`) REFERENCES `persons` (`id`)
);

CREATE TABLE `watchlist` (
    `id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `movie_id` char(36) NOT NULL,
    `status` varchar(50) NOT NULL,
    `priority` int NULL,
    `note` longtext NULL,
    `added_at` datetime(6) NOT NULL,
    `watched_at` datetime(6) NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_watchlist_movie_movie_id` FOREIGN KEY (`movie_id`) REFERENCES `movie` (`id`),
    CONSTRAINT `fk_watchlist_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE TABLE `comment_vote` (
    `id` char(36) NOT NULL,
    `comment_id` char(36) NOT NULL,
    `user_id` char(36) NOT NULL,
    `vote_type` varchar(50) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_comment_vote_comment_comment_id` FOREIGN KEY (`comment_id`) REFERENCES `comment` (`id`),
    CONSTRAINT `fk_comment_vote_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
);

CREATE INDEX `ix_comment_movie_id` ON `comment` (`movie_id`);

CREATE INDEX `ix_comment_movie_id_root_id_created_at` ON `comment` (`movie_id`, `root_id`, `created_at`);

CREATE INDEX `ix_comment_parent_id` ON `comment` (`parent_id`);

CREATE INDEX `ix_comment_root_id` ON `comment` (`root_id`);

CREATE INDEX `ix_comment_user_id` ON `comment` (`user_id`);

CREATE INDEX `ix_comment_vote_comment_id` ON `comment_vote` (`comment_id`);

CREATE UNIQUE INDEX `ix_comment_vote_comment_id_user_id` ON `comment_vote` (`comment_id`, `user_id`);

CREATE INDEX `ix_comment_vote_user_id` ON `comment_vote` (`user_id`);

CREATE UNIQUE INDEX `ix_genres_name` ON `genres` (`name`);

CREATE UNIQUE INDEX `ix_genres_slug` ON `genres` (`slug`);

CREATE INDEX `ix_movie_created_by_user_id` ON `movie` (`created_by`);

CREATE INDEX `ix_movie_release_date` ON `movie` (`release_date`);

CREATE UNIQUE INDEX `ix_movie_slug` ON `movie` (`slug`);

CREATE INDEX `ix_movie_status` ON `movie` (`status`);

CREATE INDEX `ix_movie_title` ON `movie` (`title`);

CREATE INDEX `ix_movie_genres_genre_id` ON `movie_genres` (`genre_id`);

CREATE INDEX `ix_movie_rating_movie_id` ON `movie_rating` (`movie_id`);

CREATE INDEX `ix_movie_rating_user_id` ON `movie_rating` (`user_id`);

CREATE UNIQUE INDEX `ix_movie_rating_user_id_movie_id` ON `movie_rating` (`user_id`, `movie_id`);

CREATE INDEX `ix_moviecredits_movie_id` ON `moviecredits` (`movie_id`);

CREATE INDEX `ix_moviecredits_movie_id_person_id_credit_type_job_character_na` ON `moviecredits` (`movie_id`, `person_id`, `credit_type`, `job`, `character_name`);

CREATE INDEX `ix_moviecredits_person_id` ON `moviecredits` (`person_id`);

CREATE INDEX `ix_notification_user_id` ON `notification` (`user_id`);

CREATE INDEX `ix_notification_user_id_is_read_created_at` ON `notification` (`user_id`, `is_read`, `created_at`);

CREATE UNIQUE INDEX `ix_permission_code` ON `permission` (`code`);

CREATE INDEX `ix_persons_name` ON `persons` (`name`);

CREATE INDEX `ix_refresh_token_jwt_id` ON `refresh_token` (`jwt_id`);

CREATE INDEX `ix_refresh_token_replaced_by_token_id` ON `refresh_token` (`replaced_by_token_id`);

CREATE UNIQUE INDEX `ix_refresh_token_token_hash` ON `refresh_token` (`token_hash`);

CREATE INDEX `ix_refresh_token_user_id` ON `refresh_token` (`user_id`);

CREATE INDEX `ix_report_reporter_user_id` ON `report` (`reporter_user_id`);

CREATE INDEX `ix_report_reviewed_by_user_id` ON `report` (`reviewed_by`);

CREATE INDEX `ix_report_target_type_target_id` ON `report` (`target_type`, `target_id`);

CREATE INDEX `ix_report_target_type_target_id_status` ON `report` (`target_type`, `target_id`, `status`);

CREATE UNIQUE INDEX `ix_role_code` ON `role` (`code`);

CREATE INDEX `ix_role_permission_created_by_user_id` ON `role_permission` (`created_by`);

CREATE INDEX `ix_role_permission_permission_id` ON `role_permission` (`permission_id`);

CREATE UNIQUE INDEX `ix_user_email` ON `user` (`email`);

CREATE UNIQUE INDEX `ix_user_username` ON `user` (`username`);

CREATE INDEX `ix_user_role_assigned_by_user_id` ON `user_role` (`assigned_by`);

CREATE INDEX `ix_user_role_role_id` ON `user_role` (`role_id`);

CREATE INDEX `ix_watchlist_movie_id` ON `watchlist` (`movie_id`);

CREATE INDEX `ix_watchlist_user_id` ON `watchlist` (`user_id`);

CREATE UNIQUE INDEX `ix_watchlist_user_id_movie_id` ON `watchlist` (`user_id`, `movie_id`);

CREATE INDEX `ix_watchlist_user_id_status` ON `watchlist` (`user_id`, `status`);

INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
VALUES ('20260407022055_Initial_Create_Pomelo', '10.0.1');

COMMIT;

