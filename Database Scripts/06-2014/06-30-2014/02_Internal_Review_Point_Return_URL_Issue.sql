IF NOT EXISTS (SELECT 1 FROM Menu_Application WHERE Code='ORGANIZATION')
BEGIN
INSERT INTO [dbo].[Menu_Application]
           ([ParentApplicationId]
           ,[ApplicationId]
           ,[Code]
           ,[Name]
           ,[Description]
           ,[IsDisplayInMenu]
           ,[SortOrder]
           ,[ControllerName]
           ,[ActionName]
           ,[CreatedDate]
           ,[CreatedBy]
           ,[ModifiedDate]
           ,[ModifiedBy]
           ,[IsDeleted])
     VALUES
           (null
           ,'1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'ORGANIZATION'
           ,'ORGANIZATION'
           ,NULL
           ,0
           ,8
           ,'Organization'
           ,'vieweditpermission'
           ,'2014-06-30 00:00:00.000'
           ,'F37A855C-9BF4-4A1F-AB7F-B21AF43EB2AF'
           ,null
           ,null
           ,0)
END


