 ---Execute this script on BDSAuth database.
----Updated Release month.
BEGIN
	IF EXISTS(Select 1 from [dbo].[Application] Where [Code] = 'MRP')
	BEGIN
		UPDATE [dbo].[Application] set ReleaseVersion= '2015.January' Where [Code] = 'MRP'
	END
END
GO

-- 01/27/2015
---- Remove BU from User Table...
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'User'))
BEGIN
    -- Column Exists
	ALTER TABLE dbo.[User] DROP COLUMN BusinessUnitId
END

---- Add Client Id FK in UserTable
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_User_Client]') AND parent_object_id = OBJECT_ID(N'[dbo].[User]'))
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Client] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Client] ([ClientId])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_User_Client]') AND parent_object_id = OBJECT_ID(N'[dbo].[User]'))
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Client]
GO