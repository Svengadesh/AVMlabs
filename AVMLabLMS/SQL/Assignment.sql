-- =============================================
-- Create Tables
-- =============================================

CREATE TABLE Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    ClientName NVARCHAR(100) NOT NULL,
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    City NVARCHAR(100),
    Country NVARCHAR(100),
    CreditLimit DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Tests (
    TestId INT IDENTITY(1,1) PRIMARY KEY,
    TestCode NVARCHAR(20) NOT NULL,
    TestName NVARCHAR(100) NOT NULL,
    SampleType NVARCHAR(50),
    TATHours INT NOT NULL,
    Rate DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE WorkOrders (
    WOId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL FOREIGN KEY REFERENCES Clients(ClientId),
    WODate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedBy NVARCHAR(50)
);

CREATE TABLE WorkOrderItems (
    WOItemId INT IDENTITY(1,1) PRIMARY KEY,
    WOId INT NOT NULL FOREIGN KEY REFERENCES WorkOrders(WOId),
    TestId INT NOT NULL FOREIGN KEY REFERENCES Tests(TestId),
    Quantity INT NOT NULL,
    Rate DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    SampleStatus NVARCHAR(20)
);

CREATE TABLE Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL FOREIGN KEY REFERENCES Clients(ClientId),
    InvoiceDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL
);

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(InvoiceId),
    PaymentDate DATETIME2 NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Mode NVARCHAR(50) NOT NULL,
    GatewayFee DECIMAL(18,2) NOT NULL,
    NetAmount DECIMAL(18,2) NOT NULL
);

GO

-- =============================================
-- Seed Data
-- =============================================
INSERT INTO Clients (ClientName, City, Country, CreditLimit, IsActive) VALUES 
('Acme Corp', 'New York', 'USA', 5000, 1),
('Global Tech', 'London', 'UK', 10000, 1),
('Local Med', 'Sydney', 'Australia', 2000, 1);

INSERT INTO Tests (TestCode, TestName, SampleType, TATHours, Rate, IsActive) VALUES 
('T01', 'Blood Sugar', 'Blood', 24, 50, 1),
('T02', 'Lipid Profile', 'Blood', 48, 150, 1);

-- =============================================
-- Required Queries
-- =============================================

-- 1. Active clients with outstanding amount
-- Calculates outstanding based on pending invoices + in-transit work orders.
SELECT c.ClientId, c.ClientName, 
       ISNULL(i.PendingInvoiceAmount, 0) + ISNULL(w.InTransitAmount, 0) AS OutstandingAmount
FROM Clients c
LEFT JOIN (
    SELECT ClientId, SUM(TotalAmount) - ISNULL(SUM(PaidAmount), 0) AS PendingInvoiceAmount
    FROM Invoices inv
    LEFT JOIN (SELECT InvoiceId, SUM(Amount) AS PaidAmount FROM Payments GROUP BY InvoiceId) p ON inv.InvoiceId = p.InvoiceId
    WHERE inv.Status <> 'Paid'
    GROUP BY ClientId
) i ON c.ClientId = i.ClientId
LEFT JOIN (
    SELECT ClientId, SUM(TotalAmount) AS InTransitAmount
    FROM WorkOrders
    WHERE Status NOT IN ('Billed', 'Reported')
    GROUP BY ClientId
) w ON c.ClientId = w.ClientId
WHERE c.IsActive = 1 AND (ISNULL(i.PendingInvoiceAmount, 0) + ISNULL(w.InTransitAmount, 0)) > 0;

-- 2. Daily work order summary for last 30 days including zero-WO days
-- Using a recursive CTE to generate last 30 days dates
WITH DateCTE AS (
    SELECT CAST(GETDATE() - 30 AS DATE) AS DateVal
    UNION ALL
    SELECT DATEADD(DAY, 1, DateVal)
    FROM DateCTE
    WHERE DateVal < CAST(GETDATE() AS DATE)
)
SELECT d.DateVal, COUNT(w.WOId) AS TotalWorkOrders, ISNULL(SUM(w.TotalAmount), 0) AS TotalRevenue
FROM DateCTE d
LEFT JOIN WorkOrders w ON CAST(w.WODate AS DATE) = d.DateVal
GROUP BY d.DateVal
ORDER BY d.DateVal DESC
OPTION (MAXRECURSION 100);

-- 3. In-transit samples older than 48 hours
SELECT w.WOId, w.WODate, c.ClientName, w.Status, w.TotalAmount
FROM WorkOrders w
JOIN Clients c ON w.ClientId = c.ClientId
WHERE w.Status NOT IN ('Reported', 'Billed') 
  AND w.WODate < DATEADD(HOUR, -48, GETDATE());


-- 5. Clients exceeding CreditLimit
SELECT c.ClientId, c.ClientName, c.CreditLimit,
       (ISNULL(i.PendingInvoiceAmount, 0) + ISNULL(w.InTransitAmount, 0)) AS OutstandingAmount
FROM Clients c
LEFT JOIN (
    SELECT ClientId, SUM(TotalAmount) - ISNULL(SUM(PaidAmount), 0) AS PendingInvoiceAmount
    FROM Invoices inv
    LEFT JOIN (SELECT InvoiceId, SUM(Amount) AS PaidAmount FROM Payments GROUP BY InvoiceId) p ON inv.InvoiceId = p.InvoiceId
    WHERE inv.Status <> 'Paid'
    GROUP BY ClientId
) i ON c.ClientId = i.ClientId
LEFT JOIN (
    SELECT ClientId, SUM(TotalAmount) AS InTransitAmount
    FROM WorkOrders
    WHERE Status NOT IN ('Billed', 'Reported')
    GROUP BY ClientId
) w ON c.ClientId = w.ClientId
WHERE (ISNULL(i.PendingInvoiceAmount, 0) + ISNULL(w.InTransitAmount, 0)) > c.CreditLimit;

GO

-- =============================================
-- GetClientLedger Stored Procedure
-- =============================================
CREATE PROCEDURE GetClientLedger 
    @ClientId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        RecordDate AS Date,
        Description,
        Debit,
        Credit,
        SUM(Debit - Credit) OVER(ORDER BY RecordDate ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS RunningBalance
    FROM (
        -- Invoices as Debits
        SELECT 
            InvoiceDate AS RecordDate,
            'Invoice #' + CAST(InvoiceId AS NVARCHAR) AS Description,
            TotalAmount AS Debit,
            0 AS Credit
        FROM Invoices
        WHERE ClientId = @ClientId
        
        UNION ALL
        
        -- Payments as Credits
        SELECT 
            p.PaymentDate AS RecordDate,
            'Payment #' + CAST(p.PaymentId AS NVARCHAR) + ' (' + p.Mode + ')' AS Description,
            0 AS Debit,
            p.Amount AS Credit
        FROM Payments p
        JOIN Invoices i ON p.InvoiceId = i.InvoiceId
        WHERE i.ClientId = @ClientId
        
        UNION ALL
        
        -- Gateway Fees as Debits
        SELECT 
            p.PaymentDate AS RecordDate,
            'Gateway Fee for Payment #' + CAST(p.PaymentId AS NVARCHAR) AS Description,
            p.GatewayFee AS Debit,
            0 AS Credit
        FROM Payments p
        JOIN Invoices i ON p.InvoiceId = i.InvoiceId
        WHERE i.ClientId = @ClientId AND p.GatewayFee > 0
    ) AS Ledger
    ORDER BY RecordDate;
END
GO
