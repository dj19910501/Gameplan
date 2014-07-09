
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
