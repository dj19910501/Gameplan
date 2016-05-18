-- Add By Nishant Sheth
-- Desc : Create Table Type for pass table as argument in stored procedure
-- Created Date: 16-May-2016
IF TYPE_ID(N'SalesforceType') IS  NULL 
BEGIN
/* Create a table type. */
CREATE TYPE SalesforceType AS TABLE
(
Id NVARCHAR(MAX) NULL,
Name NVARCHAR(MAX) NULL
)
END




