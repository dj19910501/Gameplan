-- ===========================================01_PL_430_MAP_CRM_Integration_Setup_Screen_Insert_Menu_Controller_Entry_For_ExternalServiceIntegration_Module.sql===========================================
-- Use BDSAuth DB

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Menu_Application')
BEGIN
	IF NOT EXISTS (SELECT * FROM [dbo].[Menu_Application] WHERE [ControllerName] = 'ExternalService' AND  [ActionName] = 'Index' AND ISNULL(IsDeleted,0) = 0)
	BEGIN
		INSERT [dbo].[Menu_Application] 
		([ApplicationId], [Code], [Name], [IsDisplayInMenu], [SortOrder], [ControllerName], [ActionName], [CreatedDate], [CreatedBy], [IsDeleted]) 
		VALUES 
		(N'1c10d4b9-7931-4a7c-99e9-a158ce158951', N'EXTERNALSERVICE', N'EXTERNALSERVICE', 0, 7, N'ExternalService', N'Index', GETDATE(), N'f37a855c-9bf4-4a1f-ab7f-b21af43eb2af', 0)
	END
END

-- ===========================================01_PL_469_MAP_CRM_Release_Version_Displayed_On_Login_Page_Incorrect_Add_ReleaseVersion_Column.sql===========================================
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

-- ===========================================02_PL_469_MAP_CRM_Release_Version_Displayed_On_Login_Page_Incorrect_Update_ReleaseVersion_Value_Script.sql===========================================
-- Use BDSAuth DB
-- Please execute this script on each release.
-- Please set value for parameter @NewValue

DECLARE @NewValue NVARCHAR(25)
SET @NewValue = '2014.May'		-- Please change the new value here.

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
