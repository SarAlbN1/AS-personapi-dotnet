-- 01_ddl.sql · Esquema de persona_db (SQL Server 2025 Express)
-- Origen único del esquema para Database First: las entidades EF Core se generan
-- a partir de este script, nunca al contrario.
-- Contrato: docs/wiki/03-contratos/persistencia.md y SDD §3.
--
-- Reejecución: el script es idempotente. Crea la base y cada tabla solo si no
-- existen; si ya existen las deja intactas, con sus datos, y no informa error.
-- No altera tablas existentes: un cambio de columnas o restricciones se hace
-- recreando la base (DROP DATABASE persona_db) y volviendo a ejecutar 01 y 02.
--
-- Sin credenciales: se ejecuta con la conexión del cliente (SSMS o sqlcmd).

SET NOCOUNT ON;
GO

IF DB_ID('persona_db') IS NULL
    CREATE DATABASE persona_db;
GO
ALTER AUTHORIZATION ON DATABASE::persona_db TO sa;
GO
USE persona_db;
GO

IF OBJECT_ID('dbo.persona', 'U') IS NULL
BEGIN
    CREATE TABLE persona (
        cc        INT          NOT NULL,
        nombre    VARCHAR(45)  NOT NULL,
        apellido  VARCHAR(45)  NOT NULL,
        genero    CHAR(1)      NOT NULL,
        edad      INT          NULL,
        CONSTRAINT PK_persona PRIMARY KEY (cc),
        CONSTRAINT CK_persona_genero CHECK (genero IN ('M', 'F'))
    );
END
GO

IF OBJECT_ID('dbo.profesion', 'U') IS NULL
BEGIN
    CREATE TABLE profesion (
        id   INT           NOT NULL,
        nom  VARCHAR(90)   NOT NULL,
        des  VARCHAR(MAX)  NULL,
        CONSTRAINT PK_profesion PRIMARY KEY (id)
    );
END
GO

-- PK compuesta (id_prof, cc_per), sin identificador artificial.
-- Las FK usan NO ACTION (valor por defecto): nunca CASCADE.
IF OBJECT_ID('dbo.estudios', 'U') IS NULL
BEGIN
    CREATE TABLE estudios (
        id_prof  INT          NOT NULL,
        cc_per   INT          NOT NULL,
        fecha    DATE         NULL,
        univer   VARCHAR(50)  NULL,
        CONSTRAINT PK_estudios PRIMARY KEY (id_prof, cc_per),
        CONSTRAINT estudio_persona_fk   FOREIGN KEY (cc_per)  REFERENCES persona (cc)
            ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT estudio_profesion_fk FOREIGN KEY (id_prof) REFERENCES profesion (id)
            ON DELETE NO ACTION ON UPDATE NO ACTION
    );
    CREATE INDEX estudio_persona_fk ON estudios (cc_per);
END
GO

IF OBJECT_ID('dbo.telefono', 'U') IS NULL
BEGIN
    CREATE TABLE telefono (
        num     VARCHAR(15)  NOT NULL,
        oper    VARCHAR(45)  NOT NULL,
        duenio  INT          NOT NULL,
        CONSTRAINT PK_telefono PRIMARY KEY (num),
        CONSTRAINT telefono_persona_fk FOREIGN KEY (duenio) REFERENCES persona (cc)
            ON DELETE NO ACTION ON UPDATE NO ACTION
    );
    CREATE INDEX telefono_persona_fk ON telefono (duenio);
END
GO
