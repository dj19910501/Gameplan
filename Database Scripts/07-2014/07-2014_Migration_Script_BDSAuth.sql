-- Run This Script in BDSAuth


-- Created by : Kalpesh Sharma 
-- Ticket : Internal Reviews Point  
-- This script will be check that ApplicationID field is exists or not in CustomRestriction Table . if it will be not in that table at that time we have to insert that 
   --field with default data 

if not EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'CustomRestriction' AND COLUMN_NAME = 'ApplicationId')
  ALTER TABLE CustomRestriction   ADD ApplicationId uniqueidentifier CONSTRAINT ApplicationId_fk REFERENCES dbo.Application(ApplicationId)  
GO
if EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'CustomRestriction' AND COLUMN_NAME = 'ApplicationId') 
 update CustomRestriction set ApplicationId = (Select ApplicationId from Application where Name = 'Bulldog Gameplan')


 Go 

 
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ManagerId' AND OBJECT_ID = OBJECT_ID(N'[User]'))
BEGIN
    ALTER TABLE [User] ADD ManagerId UNIQUEIDENTIFIER
END
GO
IF NOT EXISTS (SELECT * FROM SYS.FOREIGN_KEYS WHERE OBJECT_ID = OBJECT_ID(N'FK_User_User') AND PARENT_OBJECT_ID = OBJECT_ID(N'[User]'))
BEGIN
    ALTER TABLE [User] WITH CHECK ADD CONSTRAINT FK_User_User FOREIGN KEY (ManagerId) REFERENCES [User] (UserId)
END
GO
DECLARE @ApplicationId UNIQUEIDENTIFIER
--Get the application id
SELECT @ApplicationId = ApplicationId FROM Application WHERE Code = 'MRP'
--Update Manager Id
UPDATE [User]  
SET [User].ManagerId = UA.ManagerId
FROM [User], User_Application UA
WHERE [User].UserId = UA.UserId
AND UA.ApplicationId = @ApplicationId AND UA.IsDeleted = 0
GO
IF EXISTS (SELECT * FROM SYS.FOREIGN_KEYS WHERE OBJECT_ID = OBJECT_ID(N'FK_User_Application_User_ManagerId') AND PARENT_OBJECT_ID = OBJECT_ID(N'[User_Application]'))
BEGIN
    ALTER TABLE [User_Application] DROP CONSTRAINT FK_User_Application_User_ManagerId
END
GO
IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ManagerId' AND OBJECT_ID = OBJECT_ID(N'[User_Application]'))
BEGIN
    ALTER TABLE [User_Application] DROP COLUMN ManagerId
END
GO

-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/18/2014
-- Description :- Drop GeographyId field from User table
-- NOTE :- Run this script on 'BDSAuth' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User')
BEGIN
	
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'GeographyId')
	BEGIN
		ALTER TABLE [User] DROP COLUMN GeographyId
	END
END