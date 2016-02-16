/* --------- Start Script of PL ticket #1979 --------- */
-- Added by Viral Kadiya on 02/16/2016
-- Increase 'ActionSuffix' column size from '50' to 'MAX'

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ActionSuffix' AND OBJECT_ID = OBJECT_ID(N'Changelog'))
BEGIN
ALTER TABLE [Changelog]
ALTER COLUMN [ActionSuffix] NVARCHAR(MAX)
END  

/* --------- End Script of PL ticket #1979 --------- */