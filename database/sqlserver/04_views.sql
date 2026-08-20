/* =========================================================
   VIEW: vw_VesselSummary
   Resumen por nave
   ========================================================= */
CREATE VIEW vw_VesselSummary AS
SELECT
    v.Id,
    v.Name,
    v.IsActive,
    ISNULL(SUM(r.Quantity), 0) AS TotalReceived,
    ISNULL(SUM(d.Quantity), 0) AS TotalDispatched,
    ISNULL(SUM(r.Quantity), 0) - ISNULL(SUM(d.Quantity), 0) AS CurrentStock
FROM Vessels v
LEFT JOIN Receptions r
    ON v.Id = r.VesselId
LEFT JOIN Dispatches d
    ON v.Id = d.VesselId
GROUP BY
    v.Id,
    v.Name,
    v.IsActive;
GO


/* =========================================================
   VIEW: vw_BLBalance
   Balance por BL
   ========================================================= */
CREATE VIEW vw_BLBalance AS
SELECT
    b.Id,
    b.BlCode,
    pc.Name AS ProductName,
    b.LotTotal,

    ISNULL(SUM(ri.Quantity), 0) AS ReceivedQty,

    b.LotTotal - ISNULL(SUM(ri.Quantity), 0)
        AS RemainingQty

FROM BLs b

LEFT JOIN ProductCatalog pc
    ON pc.Id = b.ProductCatalogId

LEFT JOIN ReceptionItems ri
    ON ri.BlId = b.Id

GROUP BY
    b.Id,
    b.BlCode,
    pc.Name,
    b.LotTotal;
GO


/* =========================================================
   VIEW: vw_DailyReception
   Recepción diaria
   ========================================================= */
CREATE VIEW vw_DailyReception AS
SELECT
    CAST(CreatedAt AS DATE) AS OperationDate,
    COUNT(*) AS Trucks,
    SUM(Quantity) AS TotalQuantity
FROM Receptions
GROUP BY CAST(CreatedAt AS DATE);
GO


/* =========================================================
   VIEW: vw_DailyDispatch
   Despacho diario
   ========================================================= */
CREATE VIEW vw_DailyDispatch AS
SELECT
    CAST(CreatedAt AS DATE) AS OperationDate,
    COUNT(*) AS Trucks,
    SUM(Quantity) AS TotalQuantity
FROM Dispatches
GROUP BY CAST(CreatedAt AS DATE);
GO


/* =========================================================
   VIEW: vw_CurrentStock
   Stock operacional
   ========================================================= */
CREATE VIEW vw_CurrentStock AS
SELECT
    v.Id AS VesselId,
    v.Name AS VesselName,

    ISNULL(r.TotalReceived,0) AS TotalReceived,
    ISNULL(d.TotalDispatched,0) AS TotalDispatched,

    ISNULL(r.TotalReceived,0) -
    ISNULL(d.TotalDispatched,0) AS CurrentStock

FROM Vessels v

LEFT JOIN
(
    SELECT
        VesselId,
        SUM(Quantity) AS TotalReceived
    FROM Receptions
    GROUP BY VesselId
) r
ON v.Id = r.VesselId

LEFT JOIN
(
    SELECT
        VesselId,
        SUM(Quantity) AS TotalDispatched
    FROM Dispatches
    GROUP BY VesselId
) d
ON v.Id = d.VesselId;
GO


/* =========================================================
   VIEW: vw_ReceptionDetails
   Detalle completo de recepciones
   ========================================================= */
CREATE VIEW vw_ReceptionDetails AS
SELECT
    r.Id,
    r.TerminalTruck,
    r.Quantity,
    r.OperatorName,
    r.CreatedAt,

    v.Name AS Vessel,

    b.BlCode,

    pc.Name AS Product

FROM Receptions r

LEFT JOIN Vessels v
    ON v.Id = r.VesselId

LEFT JOIN ReceptionItems ri
    ON ri.ReceptionId = r.Id

LEFT JOIN BLs b
    ON b.Id = ri.BlId

LEFT JOIN ProductCatalog pc
    ON pc.Id = b.ProductCatalogId;
GO
