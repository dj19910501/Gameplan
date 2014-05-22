-- Use BDSAuth DB

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Application')
BEGIN
	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Application' AND COLUMN_NAME = 'ReleaseVersion')
	BEGIN
		ALTER TABLE [dbo].[Application] ADD ReleaseVersion NVARCHAR(25) NULL
	END
	ELSE
		PRINT('ReleaseVersion column already exists in Application table.')
END
ELSE
	PRINT('Application table does not exists in this DB.')
