/*
========================================================
PROYECTO: CONTROL IQBF - BIG BAGS
ARCHIVO: 03_seed_data.sql
OBJETIVO: DATOS INICIALES DEL SISTEMA
========================================================
*/

-- =====================================================
-- USUARIO ADMINISTRADOR INICIAL
-- =====================================================

INSERT INTO Users
(
    UserId,
    UID,
    FirstName,
    LastName,
    PasswordHash,
    Role,
    IsActive,
    CreatedAt
)
VALUES
(
    NEWID(),
    'ADMIN',
    'ADMINISTRADOR',
    'SISTEMA',

    -- Cambiar posteriormente por hash BCrypt real
    'ADMIN123',

    'ADMINISTRADOR',
    1,
    GETDATE()
);

-- =====================================================
-- PRODUCTOS BASE
-- =====================================================

INSERT INTO Products
(
    ProductId,
    Name,
    IsActive,
    CreatedAt
)
VALUES
(NEWID(), 'CARBONATO DE SODIO', 1, GETDATE()),
(NEWID(), 'CARBONATO DE SODIO DENSE', 1, GETDATE()),
(NEWID(), 'METABISULFITO DE SODIO', 1, GETDATE()),
(NEWID(), 'SULFATO DE SODIO', 1, GETDATE());

-- =====================================================
-- NAVES BASE
-- =====================================================

INSERT INTO Ships
(
    ShipId,
    Name,
    Status,
    CreatedAt
)
VALUES
(NEWID(), 'CL HEIDI', 'ACTIVE', GETDATE()),
(NEWID(), 'YIN CAI', 'ACTIVE', GETDATE()),
(NEWID(), 'FEDERAL TAMBO', 'INACTIVE', GETDATE()),
(NEWID(), 'LINDEN ARROW', 'INACTIVE', GETDATE());

-- =====================================================
-- BLS DE EJEMPLO
-- =====================================================

DECLARE @ShipId UNIQUEIDENTIFIER;
DECLARE @ProductId UNIQUEIDENTIFIER;

SELECT TOP 1
    @ShipId = ShipId
FROM Ships
WHERE Name = 'CL HEIDI';

SELECT TOP 1
    @ProductId = ProductId
FROM Products
WHERE Name = 'SULFATO DE SODIO';

INSERT INTO BLs
(
    BLId,
    Code,
    ShipId,
    ProductId,
    TotalQuantity,
    IsActive,
    CreatedAt
)
VALUES
(
    NEWID(),
    '20CL201CS006',
    @ShipId,
    @ProductId,
    1800,
    1,
    GETDATE()
);

-- =====================================================
-- VALIDACIÓN
-- =====================================================

PRINT 'Seed Data cargado correctamente.';
