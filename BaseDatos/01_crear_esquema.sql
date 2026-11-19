-- Etapa 1. Ejecutar una sola vez en una base PostgreSQL vacía con el propietario.
-- No crea ni elimina la base de datos. Si falla, toda la instalación se revierte.
BEGIN;
CREATE SCHEMA facturacion;

CREATE TABLE facturacion.usuario (
    idusuario SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE CHECK (nombre = lower(btrim(nombre)) AND nombre <> ''),
    nombrecompleto VARCHAR(150) NOT NULL CHECK (btrim(nombrecompleto) <> ''),
    passwordhash VARCHAR(64) NOT NULL CHECK (passwordhash ~ '^[0-9a-f]{64}$'),
    passwordsalt VARCHAR(44) NOT NULL CHECK (length(passwordsalt) = 44),
    fecharegistro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE facturacion.producto (
    idproducto SERIAL PRIMARY KEY,
    descripcion VARCHAR(200) NOT NULL CHECK (btrim(descripcion) <> ''),
    cantidad INTEGER NOT NULL DEFAULT 0 CHECK (cantidad >= 0),
    precio NUMERIC(10,2) NOT NULL CHECK (precio >= 0 AND precio <> 'NaN'::numeric)
);

CREATE TABLE facturacion.cliente (
    idcliente SERIAL PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL CHECK (btrim(nombre) <> ''),
    telefono VARCHAR(30) NOT NULL DEFAULT '',
    email VARCHAR(254) NOT NULL DEFAULT '',
    direccion VARCHAR(300) NOT NULL DEFAULT ''
);

CREATE TABLE facturacion.factura (
    idfactura SERIAL PRIMARY KEY,
    idcliente INTEGER NOT NULL REFERENCES facturacion.cliente(idcliente) ON DELETE RESTRICT,
    cliente VARCHAR(150) NOT NULL CHECK (btrim(cliente) <> ''),
    fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    subtotal NUMERIC(10,2) NOT NULL CHECK (subtotal >= 0 AND subtotal <> 'NaN'::numeric),
    iva NUMERIC(10,2) NOT NULL CHECK (iva = round(subtotal * 0.13, 2)),
    total NUMERIC(10,2) NOT NULL CHECK (total = subtotal + iva)
);

CREATE TABLE facturacion.detallefactura (
    iddetalle SERIAL PRIMARY KEY,
    idfactura INTEGER NOT NULL REFERENCES facturacion.factura(idfactura) ON DELETE RESTRICT,
    idproducto INTEGER NOT NULL REFERENCES facturacion.producto(idproducto) ON DELETE RESTRICT,
    cantidad INTEGER NOT NULL CHECK (cantidad > 0),
    precio NUMERIC(10,2) NOT NULL CHECK (precio >= 0 AND precio <> 'NaN'::numeric),
    total NUMERIC(10,2) NOT NULL CHECK (total = round(cantidad * precio, 2)),
    UNIQUE (idfactura, idproducto)
);

CREATE INDEX ix_factura_fecha ON facturacion.factura(fecha);
CREATE INDEX ix_factura_cliente ON facturacion.factura(idcliente);
CREATE INDEX ix_detallefactura_producto ON facturacion.detallefactura(idproducto);

-- Usuario de arranque: admin / AdminInicio2026!
-- Salt Base64 de 32 bytes; hash hexadecimal de SHA256(UTF8(contraseña + saltBase64)).
-- Cambiar la contraseña cuando se implemente el CRUD de usuarios.
INSERT INTO facturacion.usuario (nombre, nombrecompleto, passwordhash, passwordsalt)
SELECT 'admin', 'Administrador',
       encode(sha256(convert_to('AdminInicio2026!' || salt, 'UTF8')), 'hex'), salt
FROM (VALUES ('YjVUZDdtOTNjS3hSU0ExOGVtaUp2NlFhNHpOMHdYZUc=')) AS inicial(salt);
COMMIT;