-- Use BDSAuth DB
-- Please execute this script on each release.
-- Please set value for parameter @NewValue

DECLARE @NewValue NVARCHAR(25)
SET @NewValue = '2014 .May'		-- Please change the new value here.

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Application')
BEGIN
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Application' AND COLUMN_NAME = 'ReleaseVersion')
	BEGIN
		UPDATE [dbo].[Application] SET ReleaseVersion = @NewValue where Code = 'MRP'
		SELECT ReleaseVersion from Application where Code = 'MRP'
	END
	ELSE
		PRINT('ReleaseVersion column does not exists in Application table.')
END
ELSE
	PRINT('Application table does not exists in this DB.')
