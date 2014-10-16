---Execute this script on BDSAuth database.

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