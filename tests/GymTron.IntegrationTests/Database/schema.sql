CREATE TABLE routines (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NULL,
    name VARCHAR(50) NOT NULL,
    PRIMARY KEY (id),
    INDEX ix_routines_user (user_id)
) ENGINE=InnoDB;

CREATE TABLE exercise_parameters (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    pattern VARCHAR(255) NOT NULL,
    type_id SMALLINT NOT NULL,
    replays_in_reserve INT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB;

CREATE TABLE routine_items (
    id INT NOT NULL AUTO_INCREMENT,
    routine_id INT NOT NULL,
    day_of_week INT NOT NULL,
    exercise_parameters_id INT NOT NULL,
    series INT NOT NULL,
    repetitions_min INT NULL,
    repetitions_max INT NULL,
    duration INT NULL,
    min_rest_time_in_seconds INT NOT NULL,
    max_rest_time_in_seconds INT NULL,
    alternating_series BIT NOT NULL,
    `position` INT NOT NULL,
    active BIT NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_routine_items_routine FOREIGN KEY (routine_id) REFERENCES routines (id),
    CONSTRAINT fk_routine_items_parameter FOREIGN KEY (exercise_parameters_id) REFERENCES exercise_parameters (id)
) ENGINE=InnoDB;

CREATE TABLE trainings (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NULL,
    routine_id INT NOT NULL,
    day_of_week INT NOT NULL,
    started_on DATETIME(6) NOT NULL,
    completed_on DATETIME(6) NULL,
    status INT NOT NULL,
    PRIMARY KEY (id),
    INDEX ix_trainings_current (status, completed_on, started_on),
    INDEX ix_trainings_user (user_id),
    CONSTRAINT fk_trainings_routine FOREIGN KEY (routine_id) REFERENCES routines (id)
) ENGINE=InnoDB;

CREATE TABLE exercises (
    id INT NOT NULL AUTO_INCREMENT,
    training_id INT NOT NULL,
    exercise_parameters_id INT NOT NULL,
    weight DECIMAL(10,2) NULL,
    duration INT NULL,
    repetitions INT NULL,
    created_on DATETIME(6) NOT NULL,
    observations TEXT NULL,
    PRIMARY KEY (id),
    INDEX ix_exercises_latest (exercise_parameters_id, created_on),
    CONSTRAINT fk_exercises_training FOREIGN KEY (training_id) REFERENCES trainings (id),
    CONSTRAINT fk_exercises_parameter FOREIGN KEY (exercise_parameters_id) REFERENCES exercise_parameters (id)
) ENGINE=InnoDB;

CREATE TABLE body_weights (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NULL,
    weight DECIMAL(10,2) NOT NULL,
    body_fat_percentage DECIMAL(10,2) NULL,
    created_on DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    INDEX ix_bodyweights_user (user_id)
) ENGINE=InnoDB;

CREATE TABLE logs (
    id INT NOT NULL AUTO_INCREMENT,
    created_on DATETIME(6) NOT NULL,
    message VARCHAR(255) NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB;
