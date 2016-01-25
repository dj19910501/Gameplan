/* --------- Start Script of PL ticket #1921 --------- */
-- Created by : Viral Kadiya
-- Created On : 1/22/2016
-- Description : Add column IsSyncSalesForce & IsSyncEloqua column to Tactic table - To save Integration settings from Review tab

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncSalesForce' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncSalesForce bit
END
GO
IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncEloqua' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncEloqua bit
END
GO

/* --------- End Script of PL ticket #1921 --------- */