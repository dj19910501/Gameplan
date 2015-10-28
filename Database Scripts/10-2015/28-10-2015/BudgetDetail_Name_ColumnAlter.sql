--- Created By Nishant Sheth
--- Date: 16-Oct-2015
--- Desc: Increase column size of Budget_Detail column 'Name' size
 IF (EXISTS
          (SELECT *
           FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = 'Budget_Detail')) BEGIN IF EXISTS
  (SELECT *
   FROM sys.columns
   WHERE Name = N'Name'
     AND Object_ID = Object_ID(N'Budget_Detail')) BEGIN
ALTER TABLE [dbo].[Budget_Detail]
ALTER COLUMN [Name] nvarchar(255) END END