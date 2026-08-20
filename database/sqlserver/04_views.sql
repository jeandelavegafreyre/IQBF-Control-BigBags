/*
========================================================
PROYECTO: CONTROL IQBF - BIG BAGS
ARCHIVO: 04_views.sql
OBJETIVO: VISTAS PARA DASHBOARD Y REPORTES
========================================================
*/

-- =====================================================
-- RESUMEN GENERAL POR BL
-- =====================================================

CREATE VIEW vw_BLSummary
AS
SELECT
    b.BLId,
    b.Code AS BL,
    p.Name AS Product,
    s.Name AS Ship,

    ISNULL(r.TotalReceived, 0) AS TotalReceived,
    ISNULL(d.TotalDispatched, 0) AS TotalDispatched,

    ISNULL(r.TotalReceived, 0) -
    ISNULL(d.TotalDispatched, 0) AS Balance

FROM BLs b

INNER JOIN Products p
    ON b.ProductId = p.ProductId

INNER JOIN Ships s
    ON b.ShipId = s.ShipId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalReceived
    FROM ReceptionItems
    GROUP BY BLId
) r
ON b.BLId = r.BLId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalDispatched
    FROM DispatchItems
    GROUP BY BLId
) d
ON b.BLId = d.BLId;
GO

-- =====================================================
-- RESUMEN POR NAVE
-- =====================================================

CREATE VIEW vw_ShipSummary
AS
SELECT
    s.ShipId,
    s.Name AS Ship,

    COUNT(DISTINCT b.BLId) AS TotalBLs,

    SUM(ISNULL(r.TotalReceived,0)) AS TotalReceived,

    SUM(ISNULL(d.TotalDispatched,0)) AS TotalDispatched,

    SUM(ISNULL(r.TotalReceived,0)) -
    SUM(ISNULL(d.TotalDispatched,0)) AS Balance

FROM Ships s

LEFT JOIN BLs b
    ON s.ShipId = b.ShipId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalReceived
    FROM ReceptionItems
    GROUP BY BLId
) r
ON b.BLId = r.BLId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalDispatched
    FROM DispatchItems
    GROUP BY BLId
) d
ON b.BLId = d.BLId

GROUP BY
    s.ShipId,
    s.Name;
GO

-- =====================================================
-- RESUMEN POR TURNO
-- =====================================================

CREATE VIEW vw_ShiftSummary
AS
SELECT

    sh.ShiftId,
    sh.ShiftDate,
    sh.ShiftType,
    sh.Status,

    sp.Name AS Ship,

    COUNT(DISTINCT r.ReceptionId) AS TotalReceptions,
    COUNT(DISTINCT dp.DispatchId) AS TotalDispatches

FROM Shifts sh

INNER JOIN Ships sp
    ON sh.ShipId = sp.ShipId

LEFT JOIN Receptions r
    ON sh.ShiftId = r.ShiftId

LEFT JOIN Dispatches dp
    ON sh.ShiftId = dp.ShiftId

GROUP BY
    sh.ShiftId,
    sh.ShiftDate,
    sh.ShiftType,
    sh.Status,
    sp.Name;
GO

-- =====================================================
-- DASHBOARD RECEPCION VS DESPACHO
-- =====================================================

CREATE VIEW vw_Dashboard
AS
SELECT

    b.Code AS BL,

    p.Name AS Product,

    s.Name AS Ship,

    ISNULL(r.TotalReceived,0) AS Reception,

    ISNULL(d.TotalDispatched,0) AS Dispatch,

    ISNULL(r.TotalReceived,0) -
    ISNULL(d.TotalDispatched,0) AS Balance

FROM BLs b

INNER JOIN Products p
    ON b.ProductId = p.ProductId

INNER JOIN Ships s
    ON b.ShipId = s.ShipId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalReceived
    FROM ReceptionItems
    GROUP BY BLId
) r
ON b.BLId = r.BLId

LEFT JOIN
(
    SELECT
        BLId,
        SUM(Quantity) AS TotalDispatched
    FROM DispatchItems
    GROUP BY BLId
) d
ON b.BLId = d.BLId;
GO
