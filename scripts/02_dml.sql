-- 02_dml.sql · Datos de demostración de persona_db
-- Requiere haber ejecutado 01_ddl.sql.
--
-- Reejecución: el script es idempotente. Cada fila se inserta solo si su clave
-- no existe; no duplica filas ni sobrescribe las que se hayan modificado desde
-- la aplicación. Para volver al estado inicial exacto se recrea la base y se
-- ejecutan 01 y 02 de nuevo.
--
-- Las tres personas tienen teléfono y estudio, de modo que borrar cualquiera de
-- ellas, o cualquier profesión con estudios, demuestra el bloqueo por FK (409).

SET NOCOUNT ON;
GO
USE persona_db;
GO

INSERT INTO persona (cc, nombre, apellido, genero, edad)
SELECT v.cc, v.nombre, v.apellido, v.genero, v.edad
FROM (VALUES
    (1010101010, N'Ana',    N'Gómez',    'F', 28),
    (1020202020, N'Carlos', N'Pérez',    'M', 35),
    (1030303030, N'Laura',  N'Martínez', 'F', 22)
) AS v (cc, nombre, apellido, genero, edad)
WHERE NOT EXISTS (SELECT 1 FROM persona p WHERE p.cc = v.cc);

INSERT INTO profesion (id, nom, des)
SELECT v.id, v.nom, v.des
FROM (VALUES
    (1, N'Ingeniería de Sistemas', N'Diseño y desarrollo de sistemas de información'),
    (2, N'Medicina',               N'Diagnóstico y tratamiento de enfermedades'),
    (3, N'Derecho',                N'Asesoría y representación legal')
) AS v (id, nom, des)
WHERE NOT EXISTS (SELECT 1 FROM profesion p WHERE p.id = v.id);

INSERT INTO estudios (id_prof, cc_per, fecha, univer)
SELECT v.id_prof, v.cc_per, CAST(v.fecha AS DATE), v.univer
FROM (VALUES
    (1, 1010101010, '2020-12-15', N'Pontificia Universidad Javeriana'),
    (2, 1020202020, '2015-06-20', N'Universidad Nacional de Colombia'),
    (1, 1030303030, '2024-07-01', N'Universidad de los Andes')
) AS v (id_prof, cc_per, fecha, univer)
WHERE NOT EXISTS (
    SELECT 1 FROM estudios e WHERE e.id_prof = v.id_prof AND e.cc_per = v.cc_per
);

INSERT INTO telefono (num, oper, duenio)
SELECT v.num, v.oper, v.duenio
FROM (VALUES
    ('3001234567', N'Claro',    1010101010),
    ('3109876543', N'Movistar', 1020202020),
    ('3205551234', N'Tigo',     1030303030)
) AS v (num, oper, duenio)
WHERE NOT EXISTS (SELECT 1 FROM telefono t WHERE t.num = v.num);
GO
