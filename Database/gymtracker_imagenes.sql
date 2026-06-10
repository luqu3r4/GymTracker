-- Migración: soporte de imágenes BLOB para ejercicios
-- Ejecutar: mysql -u root -palumno gymtracker < gymtracker_imagenes.sql

USE gymtracker;

-- 1. Cambiar foto de VARCHAR(255) a MEDIUMBLOB
ALTER TABLE ejercicios MODIFY COLUMN foto MEDIUMBLOB NULL;

-- 2. Limpiar los valores de string que ya no son válidos como imagen
UPDATE ejercicios SET foto = NULL;

-- 3. Actualizar sp_listar_ejercicios_rutina para incluir la foto del ejercicio
DROP PROCEDURE IF EXISTS sp_listar_ejercicios_rutina;

DELIMITER $$
CREATE PROCEDURE sp_listar_ejercicios_rutina(IN p_id_rutina INT)
BEGIN
    SELECT re.id_ejercicio, e.nombre, re.series, re.reps_objetivo, e.foto
    FROM rutina_ejercicios re
    JOIN ejercicios e ON re.id_ejercicio = e.id_ejercicio
    WHERE re.id_rutina = p_id_rutina
    ORDER BY re.id_ejercicio;
END$$
DELIMITER ;

-- 4. SP para crear ejercicio con imagen BLOB
DROP PROCEDURE IF EXISTS sp_crear_ejercicio;

DELIMITER $$
CREATE PROCEDURE sp_crear_ejercicio(IN p_nombre VARCHAR(100), IN p_foto MEDIUMBLOB)
BEGIN
    INSERT INTO ejercicios (nombre, foto) VALUES (p_nombre, p_foto);
    SELECT LAST_INSERT_ID() AS id_ejercicio;
END$$
DELIMITER ;

-- 5. SP para actualizar ejercicio (nombre + foto)
DROP PROCEDURE IF EXISTS sp_actualizar_ejercicio;

DELIMITER $$
CREATE PROCEDURE sp_actualizar_ejercicio(IN p_id INT, IN p_nombre VARCHAR(100), IN p_foto MEDIUMBLOB)
BEGIN
    UPDATE ejercicios SET nombre = p_nombre, foto = p_foto WHERE id_ejercicio = p_id;
END$$
DELIMITER ;

-- 6. SP para eliminar ejercicio
DROP PROCEDURE IF EXISTS sp_eliminar_ejercicio;

DELIMITER $$
CREATE PROCEDURE sp_eliminar_ejercicio(IN p_id INT)
BEGIN
    DELETE FROM ejercicios WHERE id_ejercicio = p_id;
END$$
DELIMITER ;
