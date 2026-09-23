
-- INITIALIZE TABLES --

-- CREATE TABLE --

DROP TABLE IF EXISTS `routines`;
CREATE TABLE `routines` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) DEFAULT NULL,
  `name` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_routines_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `exercise_parameters`;
CREATE TABLE `exercise_parameters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `description` TEXT NULL,
  `pattern` varchar(255) NOT NULL,
  `replays_in_reserve` int(11) NOT NULL,
  `type_id` smallint(6) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `trainings`;
CREATE TABLE `trainings` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) DEFAULT NULL,
  `routine_id` int(11) NOT NULL,
  `day_of_week` int(11) NOT NULL,
  `started_on` datetime NOT NULL,
  `completed_on` datetime DEFAULT NULL,
  `status` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `routine_id` (`routine_id`),
  KEY `ix_trainings_user` (`user_id`),
  CONSTRAINT `trainings_ibfk_1` FOREIGN KEY (`routine_id`) REFERENCES `routines` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `routine_items`;
CREATE TABLE `routine_items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `routine_id` int(11) NOT NULL,
  `day_of_week` int(11) NOT NULL,
  `exercise_parameters_id` int(11) NOT NULL,
  `min_rest_time_in_seconds` int(11) DEFAULT NULL,
  `max_rest_time_in_seconds` int(11) DEFAULT NULL,
  `alternating_series` bit(1) NOT NULL,
  `active` bit(1) NOT NULL,
  `position` tinyint(4) DEFAULT '0',
  `series` int(11) NOT NULL,
  `repetitions_min` int(11) DEFAULT NULL,
  `repetitions_max` int(11) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `exercise_parameters_id` (`exercise_parameters_id`),
  CONSTRAINT `routine_items_ibfk_1` FOREIGN KEY (`exercise_parameters_id`) REFERENCES `exercise_parameters` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `exercises`;
CREATE TABLE `exercises` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `training_id` int(11) NOT NULL,
  `exercise_parameters_id` int(11) NOT NULL,
  `weight` decimal(10,2) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `repetitions` int(11) DEFAULT NULL,
  `created_on` datetime NOT NULL,
  `observations` text,
  PRIMARY KEY (`id`),
  KEY `training_id` (`training_id`),
  KEY `exercise_parameters_id` (`exercise_parameters_id`),
  CONSTRAINT `exercises_ibfk_1` FOREIGN KEY (`training_id`) REFERENCES `trainings` (`id`),
  CONSTRAINT `exercises_ibfk_2` FOREIGN KEY (`exercise_parameters_id`) REFERENCES `exercise_parameters` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `body_weights`;
CREATE TABLE body_weights (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT DEFAULT NULL,
    `weight` DECIMAL(10,2) NOT NULL DEFAULT 0,
    imc DECIMAL(10,2) NOT NULL DEFAULT 0,
    created_on DATETIME NOT NULL,
    body_fat_percentage DECIMAL(10,2) DEFAULT 0,
    PRIMARY KEY (id),
    KEY `ix_bodyweights_user` (`user_id`)
) DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `logs`;
CREATE TABLE logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    created_on DATETIME,
    message VARCHAR(255)
);


DROP TABLE IF EXISTS `refresh_tokens`;
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(100) NOT NULL,
  `email` VARCHAR(255) NOT NULL,
  `password_hash` VARCHAR(255) NOT NULL,
  `is_active` BIT NOT NULL DEFAULT 1,
  `created_at` DATETIME NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_username` (`username`),
  UNIQUE KEY `uq_users_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `refresh_tokens` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `token_hash` VARCHAR(64) NOT NULL,
  `expires_at` DATETIME NOT NULL,
  `created_at` DATETIME NOT NULL,
  `revoked_at` DATETIME NULL,
  `replaced_by_token_hash` VARCHAR(64) NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_refresh_tokens_hash` (`token_hash`),
  KEY `ix_refresh_tokens_user_id` (`user_id`),
  CONSTRAINT `fk_refresh_tokens_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ENCODING --
ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE routines CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE exercise_parameters CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE trainings CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE routine_items CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE exercises CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE body_weights CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE logs CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE users CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE refresh_tokens CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- NOURISH DATA --

INSERT INTO routines (id, `name`)
VALUES 
	(1, 'MARÇ 2024'),
	(2, 'MAIG 2024'),
	(3, 'OCTUBRE 2024'),
	(4, 'DESEMBRE 2024');
	
INSERT INTO `exercise_parameters` (`id`, `name`, `description`, `pattern`, `replays_in_reserve`, `type_id`) VALUES
(1, 'AB ROLL', 'Mantenir el core compacte i evitar arquear l esquena a la extensió màxima.', 'ABD', 2, 1),
(2, 'ABDUCCIONES DE CADERA DESDE POLEA BAJA', NULL, 'ABDUC', 2, 1),
(3, 'CRUCES AL FRENTE EN POLEA ALTA-INCLINADO A 45 GRADOS', 'Focus en la part inferior del pectoral. Mans creuades lleugerament al final.', 'PUSH/PEC', 2, 1),
(4, 'CRUCES DESDE POLEA BAJA EN BANCO A 30 GRADOS', NULL, 'PEC', 2, 1),
(5, 'CURL DE BICEPS ALTERNO DE PIE', 'Evitar el balanceig del cos. Supinació completa del canell al pujar.', 'PULL/BIC', 2, 1),
(6, 'CURL DE BICEPS CON BARRA RECTA', NULL, 'PULL/BIC', 2, 1),
(7, 'CURL DE BICEPS CON BARRA Z', 'Lleugerament més còmode per als canells que la barra recta.', 'PULL/BIC', 2, 1),
(8, 'CURL FEMORAL SENTADO', 'Prémer el tors contra el respatller i baixar fins a la flexió màxima.', 'PULL/ISQ', 2, 1),
(9, 'ELEVACION DE TALONES DE PIE UNILATERAL', NULL, 'GEM', 2, 1),
(10, 'ELEVACIONES DE PIERNAS RECTAS TUMBADO BOCARRIBA', 'Controlar la baixada lenta sense que els peus toquin terra.', 'ABD', 0, 1),
(11, 'ELEVACIONES DE RODILLAS AL PECHO COLGADO EN POLEA', NULL, 'ABD', 1, 1),
(12, 'ELEVACIONES FRONTALES EN POLEA BAJA', 'Braços gairebé rectes, pujar fins a l alçada de les espatlles.', 'PUSH/DT', 1, 1),
(13, 'ELEVACIONES LATERALES ALTERNAS EN POLEA', NULL, 'PUSH/DT', 2, 1),
(14, 'ELEVACIONES LATERALES CON MANCUERNAS', 'Mantenir una petita flexió de colze. No pujar més enllà de la horitzontal.', 'PUSH/DT', 2, 1),
(15, 'ENCOGIMIENTOS EN POLEA DE RODILLAS', NULL, 'ABD', 2, 1),
(16, 'EXTENSIONES DE CODO EN POLEA ALTA CON BARRA RECTA', 'Colzes enganxats al costat. Extensió total del tríceps.', 'PULL/TR', 2, 1),
(17, 'HIP THRUST', 'Mirada al davant, barbeta prop del pit i bloqueig gluti a dalt.', 'HIP/GLT', 2, 1),
(18, 'LEG EXTENSION SENTADO', NULL, 'QUAD', 2, 1),
(19, 'NORDIC CURL', 'Descens el més lent possible controlant amb els isquios.', 'HIP/ISQ', 2, 1),
(20, 'PATADA TRASERA DESDE POLEA BAJA', NULL, 'GLT', 1, 1),
(21, 'PATADAS DE RANA', NULL, 'GEM', 2, 1),
(22, 'PESO MUERTO RUMANO-ATRASAR CADERAS', 'La barra sempre enganxada a les cames. Baixar fins sota els genolls.', 'HIP/PULL/ISQ', 2, 1),
(23, 'PESO MUERTO SUMO, CADA REP DESDE EL SUELO', NULL, 'HIP/PULL', 4, 1),
(24, 'PESO MUERTO, CADA REP DESDE EL SUELO', 'Esquena neutra. Empènyer el terra amb els peus al pujar.', 'HIP/PULL', 3, 1),
(25, 'PLANCHA HORIZONTAL', 'Mantenir línia recta cap-talons. Contraure fort abdominals i glutis.', 'CORE', 0, 2),
(26, 'PLANCHA LATERAL', NULL, 'CORE', 0, 2),
(27, 'POLEA ALTA AGARRE ANCHO NEUTRO INCLINADO', 'Portar la barra cap a la part alta del pit. Retracció escapular.', 'PULL', 2, 1),
(28, 'POLEA ALTA AGARRE ANCHO PRONO INCLINADO', NULL, 'PULL', 2, 1),
(29, 'POLEA ALTA AGARRE INVERTIDO CERRADO', NULL, 'PULL', 1, 1),
(30, 'POLEA ALTA AGARRE SUPINO CERRADO', 'Focus en el dorsal i una mica de bíceps per l agarre supí.', 'PULL', 2, 1),
(31, 'POLEA BAJA AGARRE NEUTRO ESTRECHO INCLINADO A 45 GRADOS', NULL, 'PULL', 2, 1),
(32, 'PRENSA INCLINADA SENTADO', 'No estendre els genolls del tot (no bloquejar) al final del moviment.', 'PUSH/QUAD', 3, 1),
(33, 'PRESS BANCA, TOCAR Y SUBIR SIN REBOTAR', 'Peus clavats a terra. Retracció escapular contra el banc.', 'PUSH PEC', 3, 1),
(34, 'PRESS FRANCÉS EZ', NULL, 'PUSH/TR', 2, 1),
(35, 'PRESS MANCUERNAS A 30 GRADOS', 'Baixar les manuelles fins a l alçada del pit lateralment.', 'PUSH PEC', 2, 1),
(36, 'PRESS MANCUERNAS A 75 GRADOS', NULL, 'PUSH/DT', 2, 1),
(37, 'PRESS MILITAR SENTADO A 75 GRADOS CON BARRA', 'Baixar la barra fins a la barbeta/part alta del pit.', 'PUSH/DT', 4, 1),
(38, 'PULL THROUGH EN POLEA ALTURA DE CADERA', 'Moviment front-rere de maluc. Sentir l estirament als isquios.', 'HIP/GLT', 1, 1),
(39, 'REMO CON MANCUERNA EN BANCO A 30 GRADOS', NULL, 'PULL', 2, 1),
(40, 'REMO PENDLAY, ESPALDA PARALELA AL SUELO', 'Explosiu al pujar fins a tocar l abdomen. Tornar al terra sempre.', 'PUSH PEC', 4, 1),
(41, 'REMO SENTADO EN AGARRE ANCHO NEUTRO', NULL, 'PULL', 2, 1),
(42, 'REMO SENTADO EN AGARRE NEUTRO ESTRECHO', 'Colzes enganxats als costats. Estirar bé el dorsal al davant.', 'PULL', 3, 1),
(43, 'SENTADILLA BULGARA PIE DE ATRAS A 15CM', 'Mantenir el tors recte per focus en quàdriceps.', 'QUAD', 2, 1),
(44, 'SENTADILLA CONTRA PARED', NULL, 'QUAD', 0, 2),
(45, 'SENTADILLA HACK EN PRENSA', 'Peus a la part baixa de la plataforma per més èmfasi en quàdriceps.', 'QUAD', 2, 1),
(46, 'SENTADILLA LIBRE CON BARRA / SUBPARALELA', 'Baixar fins que el maluc superi la línia dels genolls.', 'QUAD/HIP', 3, 1),
(47, 'ZANCADAS INVERTIDAS ALTERNAS, RODILLA AL SUELO', 'Pas llarg enrere i descens vertical controlat.', 'QUAD/UNIL', 2, 1);



INSERT INTO `routine_items` (
    `id`, `routine_id`, `day_of_week`, `exercise_parameters_id`, 
    `min_rest_time_in_seconds`, `max_rest_time_in_seconds`, `alternating_series`, 
    `active`, `position`, `series`, `repetitions_min`, 
    `repetitions_max`, `duration`
) VALUES
(1, 1, 1, 46, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(2, 1, 1, 33, 120, 180, 0, 1, 2, 4, 7, 10, NULL),
(3, 1, 1, 41, 120, NULL, 0, 1, 3, 3, 7, 10, NULL),
(4, 1, 1, 32, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(5, 1, 1, 8, 60, NULL, 1, 1, 5, 3, 8, 12, NULL),
(6, 1, 1, 14, 60, NULL, 1, 1, 6, 3, 8, 15, NULL),
(7, 1, 1, 9, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(8, 1, 1, 25, 30, NULL, 1, 1, 8, 3, NULL, NULL, 30),
(9, 1, 2, 37, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(10, 1, 2, 22, 120, NULL, 0, 1, 2, 3, 7, 10, NULL),
(11, 1, 2, 47, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(12, 1, 2, 31, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(13, 1, 2, 17, 120, NULL, 0, 1, 5, 3, 8, 12, NULL),
(14, 1, 2, 16, 30, NULL, 1, 1, 6, 3, 8, 12, NULL),
(15, 1, 2, 7, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(16, 1, 2, 21, 30, NULL, 1, 1, 8, 3, 8, 15, NULL),
(17, 1, 3, 24, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(18, 1, 3, 43, 120, NULL, 0, 1, 2, 3, 8, 12, NULL),
(19, 1, 3, 27, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(20, 1, 3, 36, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(21, 1, 3, 18, 60, NULL, 1, 1, 5, 3, 8, 15, NULL),
(22, 1, 3, 4, 60, NULL, 1, 1, 6, 3, 8, 15, NULL),
(23, 1, 3, 9, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(24, 1, 3, 26, 30, NULL, 1, 1, 8, 3, NULL, NULL, 30),
(25, 2, 1, 46, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(26, 2, 1, 37, 120, 180, 0, 1, 2, 4, 7, 10, NULL),
(27, 2, 1, 47, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(28, 2, 1, 17, 120, 180, 0, 1, 4, 3, 8, 12, NULL),
(29, 2, 1, 35, 60, NULL, 1, 1, 5, 3, 8, 12, NULL),
(30, 2, 1, 14, 60, NULL, 1, 1, 6, 3, 8, 15, NULL),
(31, 2, 1, 2, 60, NULL, 1, 1, 7, 3, 8, 12, NULL),
(32, 2, 1, 21, 60, NULL, 1, 1, 8, 3, 8, 15, NULL),
(33, 2, 2, 23, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(34, 2, 2, 22, 120, 180, 0, 1, 2, 3, 7, 10, NULL),
(35, 2, 2, 42, 120, NULL, 0, 1, 3, 3, 7, 10, NULL),
(36, 2, 2, 8, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(37, 2, 2, 28, 60, NULL, 1, 1, 5, 3, 8, 12, NULL),
(38, 2, 2, 8, 60, NULL, 1, 1, 6, 3, 8, 12, NULL),
(39, 2, 2, 5, 30, NULL, 1, 1, 7, 3, 8, 12, NULL),
(40, 2, 2, 25, 30, NULL, 1, 1, 8, 3, NULL, NULL, 30),
(41, 2, 3, 33, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(42, 2, 3, 40, 120, 180, 0, 1, 2, 3, 7, 10, NULL),
(43, 2, 3, 32, 120, 180, 0, 1, 3, 3, 8, 12, NULL),
(44, 2, 3, 36, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(45, 2, 3, 18, 120, NULL, 0, 1, 5, 3, 8, 15, NULL),
(46, 2, 3, 16, 30, NULL, 1, 1, 6, 3, 8, 12, NULL),
(47, 2, 3, 9, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(48, 2, 3, 15, 30, NULL, 1, 1, 8, 3, 8, 15, NULL),
(49, 3, 1, 46, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(50, 3, 1, 17, 60, NULL, 0, 1, 2, 3, 8, 12, NULL),
(51, 3, 1, 47, 60, NULL, 0, 1, 3, 3, 8, 12, NULL),
(52, 3, 1, 22, 120, 180, 0, 1, 4, 3, 7, 10, NULL),
(53, 3, 1, 18, 60, NULL, 1, 1, 5, 3, 8, 15, NULL),
(54, 3, 1, 19, 60, NULL, 1, 1, 6, 3, 8, 15, NULL),
(55, 3, 1, 20, 30, NULL, 1, 1, 7, 3, 8, 12, NULL),
(56, 3, 1, 1, 30, NULL, 1, 1, 8, 3, 8, 15, NULL),
(57, 3, 2, 37, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(58, 3, 2, 33, 120, 180, 0, 1, 2, 4, 7, 10, NULL),
(59, 3, 2, 29, 60, NULL, 0, 1, 3, 3, 8, 12, NULL),
(60, 3, 2, 35, 60, NULL, 0, 1, 4, 3, 8, 12, NULL),
(61, 3, 2, 16, 60, NULL, 0, 1, 5, 3, 8, 12, NULL),
(62, 3, 2, 12, 60, NULL, 1, 1, 6, 3, 8, 12, NULL),
(63, 3, 2, 15, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(64, 3, 2, 9, 30, NULL, 1, 1, 8, 3, 8, 15, NULL),
(65, 3, 3, 23, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(66, 3, 3, 32, 60, NULL, 0, 1, 2, 3, 8, 12, NULL),
(67, 3, 3, 8, 60, NULL, 0, 1, 3, 3, 8, 12, NULL),
(68, 3, 3, 43, 60, NULL, 0, 1, 4, 3, 8, 12, NULL),
(69, 3, 3, 38, 60, NULL, 1, 1, 5, 3, 8, 12, NULL),
(70, 3, 3, 2, 60, NULL, 1, 1, 6, 3, 8, 12, NULL),
(71, 3, 3, 9, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(72, 3, 3, 11, 30, NULL, 1, 1, 8, 3, 8, 15, NULL),
(73, 4, 1, 46, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(74, 4, 1, 22, 120, NULL, 0, 1, 2, 3, 7, 10, NULL),
(75, 4, 1, 32, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(76, 4, 1, 44, 60, NULL, 1, 1, 4, 3, NULL, NULL, 30),
(77, 4, 1, 19, 60, NULL, 1, 1, 5, 3, 8, 15, NULL),
(78, 4, 1, 21, 30, NULL, 1, 1, 6, 3, 8, 15, NULL),
(79, 4, 1, 9, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(80, 4, 2, 33, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(81, 4, 2, 40, 120, NULL, 0, 1, 2, 3, 7, 10, NULL),
(82, 4, 2, 3, 60, NULL, 1, 1, 3, 3, 7, 10, NULL),
(83, 4, 2, 28, 60, NULL, 1, 1, 4, 3, 8, 12, NULL),
(84, 4, 2, 34, 30, NULL, 1, 1, 5, 3, 8, 12, NULL),
(85, 4, 2, 6, 30, NULL, 1, 1, 6, 3, 8, 12, NULL),
(86, 4, 2, 1, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(87, 4, 3, 24, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(88, 4, 3, 44, 120, NULL, 0, 1, 2, 3, NULL, NULL, 30),
(89, 4, 3, 17, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(90, 4, 3, 45, 120, NULL, 0, 1, 4, 3, 8, 12, NULL),
(91, 4, 3, 8, 30, NULL, 1, 1, 5, 3, 8, 12, NULL),
(92, 4, 3, 9, 30, NULL, 1, 1, 6, 3, 8, 15, NULL),
(93, 4, 3, 15, 30, NULL, 1, 1, 7, 3, 8, 15, NULL),
(94, 4, 4, 37, 120, 180, 0, 1, 1, 4, 7, 10, NULL),
(95, 4, 4, 39, 120, NULL, 0, 1, 2, 3, 8, 12, NULL),
(96, 4, 4, 35, 120, NULL, 0, 1, 3, 3, 8, 12, NULL),
(97, 4, 4, 30, 60, NULL, 1, 1, 4, 3, 8, 12, NULL),
(98, 4, 4, 4, 60, NULL, 1, 1, 5, 3, 8, 15, NULL),
(99, 4, 4, 13, 30, NULL, 1, 1, 6, 3, 7, 10, NULL),
(100, 4, 4, 25, 30, NULL, 1, 1, 7, 3, NULL, NULL, 30);
	
	
	
	
	
	
INSERT INTO body_weights (id, `weight`, imc, created_on, body_fat_percentage)
VALUES 
    (1, 108.1, 0, '2024-02-29 18:00:00', 0),
    (2, 112, 0, '2024-04-03 18:00:00', 0),
    (3, 110, 0, '2024-05-06 18:00:00', 0),
    (4, 107.2, 0, '2024-06-06 18:00:00', 0),
    (5, 104.7, 0, '2024-07-04 18:00:00', 0),
    (6, 102.6, 0, '2024-09-02 18:00:00', 0),
    (7, 102.1, 0, '2024-10-02 18:00:00', 0),
    (8, 102.3, 0, '2024-11-06 18:00:00', 0),
    (9, 101.3, 0, '2024-12-10 18:00:00', 0),
    (10, 101.2, 36.1, '2025-02-10 18:00:00', 0);

-- SEED TEST USER (Local Environment) --
-- Credentials: Username='user' | Email='user@gymtron.local' | Password='password'
INSERT INTO `users` (`id`, `username`, `email`, `password_hash`, `is_active`, `created_at`)
VALUES 
    (1, 'user', 'user@gymtron.local', 'GkRdnMxtr1jvD87kEmZPfw==:wRrYCQAK2CyLUFDru+eXqIK1QBi5sOXhTvuP1AhWIBc=:100000:SHA256', 1, '2026-01-01 00:00:00')
ON DUPLICATE KEY UPDATE `password_hash` = VALUES(`password_hash`);

-- Assign initial seed routines, trainings, and body weights to test user 1
UPDATE `routines` SET `user_id` = 1 WHERE `user_id` IS NULL;
UPDATE `trainings` SET `user_id` = 1 WHERE `user_id` IS NULL;
UPDATE `body_weights` SET `user_id` = 1 WHERE `user_id` IS NULL;

-- SEED SAMPLE COMPLETED TRAININGS & EXERCISES (Local Environment) --
INSERT INTO `trainings` (`id`, `user_id`, `routine_id`, `day_of_week`, `started_on`, `completed_on`, `status`)
VALUES
    (1, 1, 1, 1, '2026-03-02 09:00:00', '2026-03-02 10:15:00', 6),
    (2, 1, 1, 3, '2026-03-04 09:00:00', '2026-03-04 10:20:00', 6),
    (3, 1, 1, 5, '2026-03-06 09:00:00', '2026-03-06 10:10:00', 6)
ON DUPLICATE KEY UPDATE `status` = VALUES(`status`);

INSERT INTO `exercises` (`id`, `training_id`, `exercise_parameters_id`, `weight`, `duration`, `repetitions`, `created_on`, `observations`)
VALUES
    (1, 1, 17, 80.00, NULL, 10, '2026-03-02 09:20:00', ''),
    (2, 1, 18, 55.00, NULL, 12, '2026-03-02 09:35:00', ''),
    (3, 1, 5, 14.00, NULL, 10, '2026-03-02 09:50:00', ''),
    (4, 2, 17, 82.50, NULL, 10, '2026-03-04 09:25:00', ''),
    (5, 2, 18, 60.00, NULL, 12, '2026-03-04 09:40:00', ''),
    (6, 2, 5, 14.00, NULL, 12, '2026-03-04 09:55:00', ''),
    (7, 3, 17, 85.00, NULL, 10, '2026-03-06 09:20:00', ''),
    (8, 3, 18, 62.50, NULL, 12, '2026-03-06 09:35:00', ''),
    (9, 3, 5, 16.00, NULL, 10, '2026-03-06 09:50:00', '')
ON DUPLICATE KEY UPDATE `weight` = VALUES(`weight`);


	