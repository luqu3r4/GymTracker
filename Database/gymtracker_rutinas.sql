-- ============================================================
--  GymTracker - Migración: módulo de rutinas
--  Ejecutar DESPUÉS de gymtracker_db.sql
-- ============================================================

USE gymtracker;

-- ============================================================
-- 1. TABLAS NUEVAS
-- ============================================================

CREATE TABLE rutinas (
    id_rutina     INT           NOT NULL AUTO_INCREMENT,
    nombre        VARCHAR(100)  NOT NULL,
    id_entrenador INT           NOT NULL,
    CONSTRAINT pk_rutinas PRIMARY KEY (id_rutina),
    CONSTRAINT fk_rutina_entrenador
        FOREIGN KEY (id_entrenador) REFERENCES entrenadores(id_entrenador)
        ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE rutina_ejercicios (
    id_rutina     INT NOT NULL,
    id_ejercicio  INT NOT NULL,
    series        INT NOT NULL DEFAULT 3,
    reps_objetivo INT NOT NULL DEFAULT 10,
    CONSTRAINT pk_rutina_ejercicios PRIMARY KEY (id_rutina, id_ejercicio),
    CONSTRAINT fk_re_rutina
        FOREIGN KEY (id_rutina) REFERENCES rutinas(id_rutina)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_re_ejercicio
        FOREIGN KEY (id_ejercicio) REFERENCES ejercicios(id_ejercicio)
        ON DELETE CASCADE ON UPDATE CASCADE
);

-- ============================================================
-- 2. MODIFICAR TABLA CLIENTES
-- ============================================================

ALTER TABLE clientes
    ADD COLUMN id_rutina_actual INT NULL,
    ADD CONSTRAINT fk_cliente_rutina
        FOREIGN KEY (id_rutina_actual) REFERENCES rutinas(id_rutina)
        ON DELETE SET NULL ON UPDATE CASCADE;

-- ============================================================
-- 3. ACTUALIZAR SP EXISTENTE (devuelve id_rutina_actual)
-- ============================================================

DROP PROCEDURE IF EXISTS sp_leer_clientes_por_entrenador;

DELIMITER $$

CREATE PROCEDURE sp_leer_clientes_por_entrenador (
    IN p_id_entrenador INT
)
BEGIN
    SELECT id_cliente, nombre, id_rutina_actual
    FROM   clientes
    WHERE  id_entrenador = p_id_entrenador
    ORDER  BY nombre;
END$$

-- ============================================================
-- 4. NUEVOS STORED PROCEDURES
-- ============================================================

CREATE PROCEDURE sp_crear_rutina (
    IN p_nombre        VARCHAR(100),
    IN p_id_entrenador INT
)
BEGIN
    INSERT INTO rutinas (nombre, id_entrenador) VALUES (p_nombre, p_id_entrenador);
    SELECT LAST_INSERT_ID() AS id_rutina;
END$$

CREATE PROCEDURE sp_listar_rutinas_por_entrenador (
    IN p_id_entrenador INT
)
BEGIN
    SELECT id_rutina, nombre
    FROM   rutinas
    WHERE  id_entrenador = p_id_entrenador
    ORDER  BY nombre;
END$$

CREATE PROCEDURE sp_eliminar_rutina (
    IN p_id_rutina INT
)
BEGIN
    DELETE FROM rutinas WHERE id_rutina = p_id_rutina;
END$$

CREATE PROCEDURE sp_agregar_ejercicio_rutina (
    IN p_id_rutina    INT,
    IN p_id_ejercicio INT,
    IN p_series       INT,
    IN p_reps         INT
)
BEGIN
    INSERT INTO rutina_ejercicios (id_rutina, id_ejercicio, series, reps_objetivo)
    VALUES (p_id_rutina, p_id_ejercicio, p_series, p_reps)
    ON DUPLICATE KEY UPDATE series = p_series, reps_objetivo = p_reps;
END$$

CREATE PROCEDURE sp_quitar_ejercicio_rutina (
    IN p_id_rutina    INT,
    IN p_id_ejercicio INT
)
BEGIN
    DELETE FROM rutina_ejercicios
    WHERE id_rutina = p_id_rutina AND id_ejercicio = p_id_ejercicio;
END$$

CREATE PROCEDURE sp_listar_ejercicios_rutina (
    IN p_id_rutina INT
)
BEGIN
    SELECT re.id_ejercicio, e.nombre, re.series, re.reps_objetivo
    FROM   rutina_ejercicios re
    JOIN   ejercicios e ON re.id_ejercicio = e.id_ejercicio
    WHERE  re.id_rutina = p_id_rutina
    ORDER  BY e.nombre;
END$$

CREATE PROCEDURE sp_asignar_rutina_cliente (
    IN p_id_cliente INT,
    IN p_id_rutina  INT
)
BEGIN
    UPDATE clientes SET id_rutina_actual = p_id_rutina WHERE id_cliente = p_id_cliente;
END$$

CREATE PROCEDURE sp_quitar_rutina_cliente (
    IN p_id_cliente INT
)
BEGIN
    UPDATE clientes SET id_rutina_actual = NULL WHERE id_cliente = p_id_cliente;
END$$

DELIMITER ;

-- ============================================================
-- 5. NUEVA VISTA
-- ============================================================

CREATE VIEW v_rutina_cliente AS
SELECT
    c.id_cliente,
    c.nombre        AS cliente,
    r.id_rutina,
    r.nombre        AS rutina,
    e.nombre        AS ejercicio,
    re.series,
    re.reps_objetivo
FROM clientes           c
JOIN rutinas            r  ON c.id_rutina_actual = r.id_rutina
JOIN rutina_ejercicios  re ON r.id_rutina        = re.id_rutina
JOIN ejercicios         e  ON re.id_ejercicio    = e.id_ejercicio;

-- ============================================================
-- 6. DATOS DE EJEMPLO
-- ============================================================

INSERT INTO rutinas (nombre, id_entrenador) VALUES
    ('Fuerza A', 1),
    ('Fuerza B', 1),
    ('Hipertrofia Full Body', 2);

INSERT INTO rutina_ejercicios (id_rutina, id_ejercicio, series, reps_objetivo) VALUES
    (1, 1, 5, 5),   -- Fuerza A: Press banca 5x5
    (1, 2, 5, 5),   -- Fuerza A: Sentadilla 5x5
    (1, 3, 3, 5),   -- Fuerza A: Peso muerto 3x5
    (2, 4, 4, 6),   -- Fuerza B: Press militar 4x6
    (2, 5, 3, 8),   -- Fuerza B: Dominadas 3x8
    (3, 1, 4, 10),  -- Hipertrofia: Press banca 4x10
    (3, 2, 4, 10),  -- Hipertrofia: Sentadilla 4x10
    (3, 6, 3, 12),  -- Hipertrofia: Curl biceps 3x12
    (3, 7, 3, 12);  -- Hipertrofia: Extension triceps 3x12

-- Asignar rutinas a clientes de ejemplo
UPDATE clientes SET id_rutina_actual = 1 WHERE id_cliente = 1;
UPDATE clientes SET id_rutina_actual = 2 WHERE id_cliente = 2;
UPDATE clientes SET id_rutina_actual = 3 WHERE id_cliente = 3;
