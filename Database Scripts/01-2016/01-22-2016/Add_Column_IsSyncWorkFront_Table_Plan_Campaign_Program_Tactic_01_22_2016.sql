/* --------- Start Script of PL ticket #1922 --------- */
-- Created by : Brad Gray
-- Created On : 1/22/2016
-- Description : Add column IsSyncWorkFront - To save Integration settings from Review tab

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncWorkFront' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncWorkFront bit
END
GO