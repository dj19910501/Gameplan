/* --------- Start Script of PL ticket #2006 --------- */
-- Added by Viral Kadiya on 02/19/2016
-- Increase 'ErrorDescription' column size from '8000' to 'MAX'

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ErrorDescription' AND OBJECT_ID = OBJECT_ID(N'IntegrationInstancePlanEntityLog'))
BEGIN
ALTER TABLE [IntegrationInstancePlanEntityLog]
ALTER COLUMN [ErrorDescription] NVARCHAR(MAX)
END  
GO
/* --------- End Script of PL ticket #2006 --------- */
