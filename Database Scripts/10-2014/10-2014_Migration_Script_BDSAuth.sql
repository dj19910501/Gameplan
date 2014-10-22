---Execute this script on BDSAuth database.
---01_PL_795_Cannot_Log_In_After_Session_Expiration.sql
GO
IF NOT EXISTS(Select 1 from [dbo].[Menu_Application] Where [Code] = 'EXTERNALSERVICE')
BEGIN
INSERT Into [dbo].[Menu_Application] ([ParentApplicationId], [ApplicationId], [Code], [Name], [IsDisplayInMenu], [SortOrder], [ControllerName], [ActionName], [CreatedDate], [CreatedBy],[IsDeleted])
SELECT [ParentApplicationId], 
	   [ApplicationId],
	   'EXTERNALSERVICE' as [Code],
	   'EXTERNALSERVICE' as [Name],
	   0 as [IsDisplayInMenu],
	   7 as [SortOrder] ,
	   'ExternalService' as [ControllerName],
	   'Index' as [ActionName],
	   GETDATE() as [CreatedDate],
	   [CreatedBy],
	   0 as [IsDeleted]
FROM [dbo].[Menu_Application] 
WHERE [Code] = 'HOME'
END

GO

IF NOT EXISTS(Select 1 from [dbo].[Menu_Application] Where [Code] = 'ORGANIZATION')
BEGIN
INSERT Into [dbo].[Menu_Application] ([ParentApplicationId], [ApplicationId], [Code], [Name], [IsDisplayInMenu], [SortOrder], [ControllerName], [ActionName], [CreatedDate], [CreatedBy],[IsDeleted])
SELECT [ParentApplicationId], 
	   [ApplicationId],
	   'ORGANIZATION' as [Code],
	   'ORGANIZATION' as [Name],
	   0 as [IsDisplayInMenu],
	   8 as [SortOrder] ,
	   'Organization' as [ControllerName],
	   'vieweditpermission' as [ActionName],
	   GETDATE() as [CreatedDate],
	   [CreatedBy],
	   0 as [IsDeleted]
FROM [dbo].[Menu_Application] 
WHERE [Code] = 'HOME'
END
GO

IF EXISTS(Select 1 from [dbo].[Application] Where [Code] = 'MRP')
BEGIN
UPDATE [dbo].[Application] set ReleaseVersion= '2014.October' Where [Code] = 'MRP'
END
GO