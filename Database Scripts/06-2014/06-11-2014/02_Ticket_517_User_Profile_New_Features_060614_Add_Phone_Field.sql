-- Run below script on BDSAuth


IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'User')
BEGIN
	IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'Phone' AND [object_id] = OBJECT_ID(N'User'))
	BEGIN
		ALTER TABLE [User] ADD Phone NVARCHAR(20) NULL
	END
END