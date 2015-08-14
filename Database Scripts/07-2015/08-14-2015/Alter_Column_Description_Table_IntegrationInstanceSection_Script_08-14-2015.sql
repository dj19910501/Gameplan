IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstanceSection]') AND type in (N'U'))
BEGIN
	ALTER TABLE IntegrationInstanceSection
	ALTER COLUMN [Description] NVARCHAR(MAX)
END
