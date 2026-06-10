-- ============================================================
--  GymTracker - Script de base de datos
--  Motor: MySQL 8.x
--  Encoding: UTF-8
-- ============================================================

-- 0. CREAR Y SELECCIONAR LA BASE DE DATOS
DROP DATABASE IF EXISTS gymtracker;
CREATE DATABASE gymtracker
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_spanish_ci;

USE gymtracker;

-- ============================================================
-- 1. TABLAS
-- ============================================================

CREATE TABLE entrenadores (
    id_entrenador  INT           NOT NULL AUTO_INCREMENT,
    nombre         VARCHAR(100)  NOT NULL,
    pin            VARCHAR(4)    NOT NULL,
    CONSTRAINT pk_entrenadores PRIMARY KEY (id_entrenador)
);

CREATE TABLE clientes (
    id_cliente    INT           NOT NULL AUTO_INCREMENT,
    nombre        VARCHAR(100)  NOT NULL,
    id_entrenador INT           NOT NULL,
    CONSTRAINT pk_clientes PRIMARY KEY (id_cliente),
    CONSTRAINT fk_cliente_entrenador
        FOREIGN KEY (id_entrenador)
        REFERENCES entrenadores(id_entrenador)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

CREATE TABLE ejercicios (
    id_ejercicio INT           NOT NULL AUTO_INCREMENT,
    nombre       VARCHAR(100)  NOT NULL,
    foto         VARCHAR(255)  NULL,
    CONSTRAINT pk_ejercicios PRIMARY KEY (id_ejercicio)
);

CREATE TABLE seguimiento (
    id_seguimiento INT           NOT NULL AUTO_INCREMENT,
    id_cliente     INT           NOT NULL,
    id_ejercicio   INT           NOT NULL,
    peso           DECIMAL(5,2)  NOT NULL,
    repeticiones   INT           NOT NULL,
    fecha          DATE          NOT NULL,
    CONSTRAINT pk_seguimiento PRIMARY KEY (id_seguimiento),
    CONSTRAINT fk_seg_cliente
        FOREIGN KEY (id_cliente)
        REFERENCES clientes(id_cliente)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_seg_ejercicio
        FOREIGN KEY (id_ejercicio)
        REFERENCES ejercicios(id_ejercicio)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

-- ============================================================
-- 2. VISTAS
-- ============================================================

-- Historial completo con nombres legibles (usada en pantalla de registros)
CREATE VIEW v_seguimiento_detalle AS
SELECT
    s.id_seguimiento,
    c.id_cliente,
    c.nombre        AS cliente,
    e.id_ejercicio,
    e.nombre        AS ejercicio,
    e.foto          AS foto_ejercicio,
    s.peso,
    s.repeticiones,
    s.fecha,
    t.nombre        AS entrenador
FROM seguimiento   s
JOIN clientes      c ON s.id_cliente   = c.id_cliente
JOIN ejercicios    e ON s.id_ejercicio = e.id_ejercicio
JOIN entrenadores  t ON c.id_entrenador = t.id_entrenador;

-- Último registro por cliente y ejercicio (usada para mostrar marca actual)
CREATE VIEW v_ultimo_peso_por_ejercicio AS
SELECT
    s.id_cliente,
    c.nombre        AS cliente,
    s.id_ejercicio,
    e.nombre        AS ejercicio,
    s.peso          AS ultimo_peso,
    s.repeticiones  AS ultimas_reps,
    s.fecha         AS ultima_fecha
FROM seguimiento s
JOIN clientes    c ON s.id_cliente   = c.id_cliente
JOIN ejercicios  e ON s.id_ejercicio = e.id_ejercicio
WHERE s.id_seguimiento = (
    SELECT MAX(s2.id_seguimiento)
    FROM   seguimiento s2
    WHERE  s2.id_cliente   = s.id_cliente
    AND    s2.id_ejercicio = s.id_ejercicio
);

-- ============================================================
-- 3. STORED PROCEDURES
-- ============================================================

DELIMITER $$

-- AUTENTICACIÓN
CREATE PROCEDURE sp_login_entrenador (
    IN  p_pin     VARCHAR(4),
    OUT p_id      INT,
    OUT p_nombre  VARCHAR(100)
)
BEGIN
    SELECT id_entrenador, nombre
    INTO   p_id, p_nombre
    FROM   entrenadores
    WHERE  pin = p_pin
    LIMIT  1;
END$$

-- CRUD CLIENTES
CREATE PROCEDURE sp_crear_cliente (
    IN p_nombre        VARCHAR(100),
    IN p_id_entrenador INT
)
BEGIN
    INSERT INTO clientes (nombre, id_entrenador)
    VALUES (p_nombre, p_id_entrenador);
    SELECT LAST_INSERT_ID() AS id_cliente;
END$$

CREATE PROCEDURE sp_leer_clientes_por_entrenador (
    IN p_id_entrenador INT
)
BEGIN
    SELECT id_cliente, nombre
    FROM   clientes
    WHERE  id_entrenador = p_id_entrenador
    ORDER  BY nombre;
END$$

CREATE PROCEDURE sp_actualizar_cliente (
    IN p_id_cliente INT,
    IN p_nombre     VARCHAR(100)
)
BEGIN
    UPDATE clientes
    SET    nombre = p_nombre
    WHERE  id_cliente = p_id_cliente;
END$$

CREATE PROCEDURE sp_eliminar_cliente (
    IN p_id_cliente INT
)
BEGIN
    -- seguimiento se borra en cascada por FK
    DELETE FROM clientes
    WHERE id_cliente = p_id_cliente;
END$$

-- CRUD SEGUIMIENTO
CREATE PROCEDURE sp_crear_seguimiento (
    IN p_id_cliente   INT,
    IN p_id_ejercicio INT,
    IN p_peso         DECIMAL(5,2),
    IN p_repeticiones INT,
    IN p_fecha        DATE
)
BEGIN
    INSERT INTO seguimiento (id_cliente, id_ejercicio, peso, repeticiones, fecha)
    VALUES (p_id_cliente, p_id_ejercicio, p_peso, p_repeticiones, p_fecha);
    SELECT LAST_INSERT_ID() AS id_seguimiento;
END$$

CREATE PROCEDURE sp_leer_seguimiento_por_cliente (
    IN p_id_cliente   INT,
    IN p_id_ejercicio INT
)
BEGIN
    SELECT id_seguimiento, ejercicio, peso, repeticiones, fecha
    FROM   v_seguimiento_detalle
    WHERE  id_cliente   = p_id_cliente
    AND    id_ejercicio = p_id_ejercicio
    ORDER  BY fecha DESC, id_seguimiento DESC;
END$$

CREATE PROCEDURE sp_actualizar_seguimiento (
    IN p_id_seguimiento INT,
    IN p_peso           DECIMAL(5,2),
    IN p_repeticiones   INT,
    IN p_fecha          DATE
)
BEGIN
    UPDATE seguimiento
    SET    peso         = p_peso,
           repeticiones = p_repeticiones,
           fecha        = p_fecha
    WHERE  id_seguimiento = p_id_seguimiento;
END$$

CREATE PROCEDURE sp_eliminar_seguimiento (
    IN p_id_seguimiento INT
)
BEGIN
    DELETE FROM seguimiento
    WHERE id_seguimiento = p_id_seguimiento;
END$$

-- EJERCICIOS
CREATE PROCEDURE sp_listar_ejercicios ()
BEGIN
    SELECT id_ejercicio, nombre, foto
    FROM   ejercicios
    ORDER  BY nombre;
END$$

DELIMITER ;

-- ============================================================
-- 4. DATOS DE EJEMPLO
-- ============================================================

INSERT INTO entrenadores (nombre, pin) VALUES
    ('Carlos Ruiz',   '1234'),
    ('Laura Méndez',  '5678'),
    ('Sergio Blanco', '0000');

INSERT INTO clientes (nombre, id_entrenador) VALUES
    ('Ana García',     1),
    ('Pedro Martínez', 1),
    ('Sofía López',    2),
    ('Juan Torres',    2),
    ('Marta Díaz',     3);

INSERT INTO ejercicios (nombre, foto) VALUES
    ('Press de banca',       'press_banca.png'),
    ('Sentadilla',           'sentadilla.png'),
    ('Peso muerto',          'peso_muerto.png'),
    ('Press militar',        'press_militar.png'),
    ('Dominadas',            'dominadas.png'),
    ('Curl de bíceps',       'curl_biceps.png'),
    ('Extensión de tríceps', 'extension_triceps.png'),
    ('Hip thrust',           'hip_thrust.png');

INSERT INTO seguimiento (id_cliente, id_ejercicio, peso, repeticiones, fecha) VALUES
    (1, 1, 40.00,  10, '2026-05-01'),
    (1, 1, 42.50,  10, '2026-05-08'),
    (1, 1, 45.00,   8, '2026-05-15'),
    (1, 2, 50.00,  12, '2026-05-01'),
    (1, 2, 52.50,  12, '2026-05-08'),
    (2, 1, 80.00,   8, '2026-05-02'),
    (2, 3, 100.00,  5, '2026-05-02'),
    (2, 3, 105.00,  5, '2026-05-09'),
    (3, 4, 30.00,  10, '2026-05-03'),
    (3, 8, 60.00,  12, '2026-05-03'),
    (4, 2, 70.00,  10, '2026-05-04'),
    (4, 5,  0.00,   6, '2026-05-04'),
    (5, 6, 15.00,  12, '2026-05-05'),
    (5, 7, 20.00,  12, '2026-05-05');
