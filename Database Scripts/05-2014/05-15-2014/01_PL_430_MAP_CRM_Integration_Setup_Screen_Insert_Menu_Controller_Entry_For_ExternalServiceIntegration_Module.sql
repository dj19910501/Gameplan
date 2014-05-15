-- Use BDSAuth DB

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Menu_Application')
BEGIN
	IF NOT EXISTS (SELECT * FROM [dbo].[Menu_Application] WHERE [ControllerName] = 'ExternalService' AND  [ActionName] = 'Index' AND ISNULL(IsDeleted,0) = 0)
	BEGIN
		INSERT [dbo].[Menu_Application] 
		([ApplicationId], [Code], [Name], [IsDisplayInMenu], [SortOrder], [ControllerName], [ActionName], [CreatedDate], [CreatedBy], [IsDeleted]) 
		VALUES 
		(N'1c10d4b9-7931-4a7c-99e9-a158ce158951', N'EXTERNALSERVICE', N'EXTERNALSERVICE', 0, 7, N'ExternalService', N'Index', GETDATE(), N'f37a855c-9bf4-4a1f-ab7f-b21af43eb2af', 0)
	END
END
