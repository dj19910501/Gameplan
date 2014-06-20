-- Run below script on BDSAuth

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'User_Application')
BEGIN

	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'User_Application' AND COLUMN_NAME = 'ManagerId')
	BEGIN
		ALTER TABLE User_Application ADD ManagerId UNIQUEIDENTIFIER NULL

		IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_User_Application_User_ManagerId' AND CONSTRAINT_SCHEMA = 'dbo')
		BEGIN
			ALTER TABLE [dbo].[User_Application]  WITH CHECK ADD  CONSTRAINT [FK_User_Application_User_ManagerId] FOREIGN KEY([ManagerId])
			REFERENCES [dbo].[User] ([UserId])
			
			ALTER TABLE [dbo].[User_Application] CHECK CONSTRAINT [FK_User_Application_User_ManagerId]
		END
	END

END
