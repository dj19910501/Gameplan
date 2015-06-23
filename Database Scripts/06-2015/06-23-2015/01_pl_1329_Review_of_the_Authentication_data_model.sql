--Created By : Ravindra Sisodiya
--Created Date : 23-06-2015
--Description :  Add new script for f_key for GamePlan Database. 
----------------------------------------------------------------------------------------

DECLARE @JobTitle VARCHAR(100)
DECLARE @Application_ActivityCode VARCHAR(100)
DECLARE @ClientDataBaseCode VARCHAR(100)
DECLARE @User_ApplicationCode VARCHAR(100)

SET @JobTitle='Admin'
SET @Application_ActivityCode='MRP'
SET @ClientDataBaseCode='BLD'
SET @User_ApplicationCode='SA'
-----------------------------------------------------------------------------------------
--Application Table add new foreign_keys CreatedBy in Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Application_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Application')
)
 BEGIN
 IF EXISTS (SELECT * FROM [Application] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
	 BEGIN
	  IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
			 UPDATE [Application] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) where CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	 END
		
 ALTER TABLE [dbo].[Application]  WITH CHECK ADD  CONSTRAINT [FK_Application_User] FOREIGN KEY([CreatedBy])
   REFERENCES [dbo].[User] ([UserId])
 END
END
-----------------------------------------
--add new foreign_keys ModifiedBy in Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Application_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Application')
)
 BEGIN	
 IF EXISTS (SELECT * FROM [Application] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
	 BEGIN
		  IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
			UPDATE [Application] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U)  
		  END
	 END
		
	ALTER TABLE [dbo].[Application]  WITH CHECK ADD  CONSTRAINT [FK_Application_User1] FOREIGN KEY([ModifiedBy])
		REFERENCES [dbo].[User] ([UserId])
	 END
END

------------------------------------------------------------------------------------------------
--Application_Activity add new foreign_keys Application id in Application_Activity table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Application_Activity')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Application_Activity_Application')
   AND parent_object_id = OBJECT_ID(N'dbo.Application_Activity')
)
 BEGIN

 IF EXISTS (SELECT * FROM Application_Activity WHERE ApplicationId NOT IN(SELECT A.ApplicationId FROM [Application] AS A))
	 BEGIN
	  IF EXISTS (SELECT TOP 1 A.ApplicationId FROM [Application] A WHERE Code=@Application_ActivityCode)
		  BEGIN
			 UPDATE Application_Activity SET ApplicationId=( SELECT TOP 1 A.ApplicationId FROM [Application] A WHERE Code=@Application_ActivityCode) WHERE ApplicationId NOT IN(SELECT A.ApplicationId FROM [Application] AS A)
		   END
	 END
		ALTER TABLE [dbo].[Application_Activity]  WITH CHECK ADD  CONSTRAINT [FK_Application_Activity_Application] FOREIGN KEY([ApplicationId])
		 REFERENCES [dbo].[Application] ([ApplicationId])
 END
END

----------------------------------------------------------------------------------
-----Application_Role add new foreign_keys CreatedBy in Application_Role table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Application_Role')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Application_Role_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Application_Role')
)
 BEGIN		
 IF EXISTS (SELECT * FROM [Application_Role] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	  IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Application_Role] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U) 
		   END
	  END

 ALTER TABLE [dbo].[Application_Role]  WITH CHECK ADD  CONSTRAINT [FK_Application_Role_User] FOREIGN KEY([CreatedBy])
   REFERENCES [dbo].[User] ([UserId])
 END
END
-------------------------------------------------------------
--add new foreign_keys ModifiedBy in Application_Role table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Application_Role')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Application_Role_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Application_Role')
)
 BEGIN
 IF EXISTS (SELECT * FROM [Application_Role] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	  IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Application_Role] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	  END
	  		
 ALTER TABLE [dbo].[Application_Role]  WITH CHECK ADD  CONSTRAINT [FK_Application_Role_User1] FOREIGN KEY([ModifiedBy])
   REFERENCES [dbo].[User] ([UserId])
 END
END

----------------------------------------------------------------------------------
-----Client add new foreign_keys CreatedBy in Client table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Client')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Client_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Client')
)
 BEGIN
  IF EXISTS (SELECT * FROM [Client] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	    IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
			 UPDATE [Client] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)  WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	  END
		
	ALTER TABLE [dbo].[Client]  WITH CHECK ADD  CONSTRAINT [FK_Client_User] FOREIGN KEY([CreatedBy])
	   REFERENCES [dbo].[User] ([UserId])
	 END
END
-----------------------------------------------------
--add new foreign_keys ModifiedBy in Client table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Client')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Client_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Client')
)
 BEGIN
   IF EXISTS (SELECT * FROM [Client] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		     UPDATE [Client] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U) 
		   END
	  END
		
  ALTER TABLE [dbo].[Client]  WITH CHECK ADD  CONSTRAINT [FK_Client_User1] FOREIGN KEY([ModifiedBy])
    REFERENCES [dbo].[User] ([UserId])
  END
END

--------------------------------------------------------------------------------------------
--ClientDatabase add new foreign_keys ClientId in ClientDatabase table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'ClientDatabase')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_ClientDatabase_Client')
   AND parent_object_id = OBJECT_ID(N'dbo.ClientDatabase')
)
 BEGIN
   IF EXISTS (SELECT * FROM ClientDatabase WHERE ClientID NOT IN(SELECT C.ClientId FROM [Client] AS C))
     BEGIN
	    IF EXISTS (SELECT TOP 1 C.ClientId FROM Client AS C WHERE Code=@ClientDataBaseCode)
		  BEGIN
		     UPDATE [ClientDatabase] SET ClientID=(SELECT TOP 1 C.ClientId FROM Client AS C WHERE Code=@ClientDataBaseCode) WHERE ClientID NOT IN(SELECT C.ClientId FROM [Client] AS C)
		   END
	  END

  ALTER TABLE [dbo].[ClientDatabase]  WITH CHECK ADD  CONSTRAINT [FK_ClientDatabase_Client] FOREIGN KEY([ClientId])
    REFERENCES [dbo].[Client] ([ClientId])
  END
END

-----------------------------------------------------------------------------------------
--Menu_Application add new foreign_keys CreatedBy in Menu_Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Menu_Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Menu_Application_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Menu_Application')
)
 BEGIN
   IF EXISTS (SELECT * FROM [Menu_Application] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	    IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		     UPDATE [Menu_Application] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U)  
		   END
	  END

	ALTER TABLE [dbo].[Menu_Application]  WITH CHECK ADD  CONSTRAINT [FK_Menu_Application_User] FOREIGN KEY([CreatedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END
-------------------------------------------------------------
--add new foreign_keys ModifiedBy in Menu_Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Menu_Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Menu_Application_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Menu_Application')
)
 BEGIN
   IF EXISTS (SELECT * FROM [Menu_Application] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	  IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		     UPDATE [Menu_Application] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U) 
		   END
	  END

	ALTER TABLE [dbo].[Menu_Application]  WITH CHECK ADD  CONSTRAINT [FK_Menu_Application_User1] FOREIGN KEY([ModifiedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END


-----------------------------------------------------------------------------------
---Role add new foreign_keys CreatedBy in Role table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Role')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Role_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Role')
)
 BEGIN
    IF EXISTS (SELECT * FROM [Role] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Role] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)  WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	  END

	ALTER TABLE [dbo].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_User] FOREIGN KEY([CreatedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END
----------------------------------------------------------
--add new foreign_keys ModifiedBy in Role table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Role')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Role_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Role')
)
 BEGIN
  IF EXISTS (SELECT * FROM [Role] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Role] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)  WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END  
	  END

	ALTER TABLE [dbo].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_User1] FOREIGN KEY([ModifiedBy])
	  REFERENCES [dbo].[User] ([UserId])
  END
END


-----------------------------------------------------------------------------------------------------
--add new foreign_keys CreatedBy in Role_Permission table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Role_Permission')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Role_Permission_User')
   AND parent_object_id = OBJECT_ID(N'dbo.Role_Permission')
)
 BEGIN
   IF EXISTS (SELECT * FROM [Role_Permission] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Role_Permission] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)  WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	  END

	ALTER TABLE [dbo].[Role_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Permission_User] FOREIGN KEY([CreatedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END
---------------------------------------------------------------------
--add new foreign_keys ModifiedBy in Role_Permission table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Role_Permission')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Role_Permission_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.Role_Permission')
)
 BEGIN
  IF EXISTS (SELECT * FROM [Role_Permission] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		    UPDATE [Role_Permission] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U)
		   END
	  END

	ALTER TABLE [dbo].[Role_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Permission_User1] FOREIGN KEY([ModifiedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END


--------------------------------------------------------------------------------------------------
--User_Application add new foreign_keys CreatedBy in User_Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'User_Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_User_Application_User1')
   AND parent_object_id = OBJECT_ID(N'dbo.User_Application')
)
 BEGIN
   IF EXISTS (SELECT * FROM [User_Application] WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	    IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
		     UPDATE [User_Application] SET CreatedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE CreatedBy NOT IN(SELECT U.UserId FROM [User] AS U) 
		   END
	  END

	ALTER TABLE [dbo].[User_Application]  WITH CHECK ADD  CONSTRAINT [FK_User_Application_User1] FOREIGN KEY([CreatedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
END
---------------------------------------------------------------------------
--add new foreign_keys ModifiedBy in User_Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'User_Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_User_Application_User2')
   AND parent_object_id = OBJECT_ID(N'dbo.User_Application')
)
 BEGIN
 IF EXISTS (SELECT * FROM [User_Application] WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	 	    IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
			  BEGIN
			     UPDATE [User_Application] SET ModifiedBy=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)  WHERE ModifiedBy NOT IN(SELECT U.UserId FROM [User] AS U)  
			   END
	  END
	ALTER TABLE [dbo].[User_Application]  WITH CHECK ADD  CONSTRAINT [FK_User_Application_User2] FOREIGN KEY([ModifiedBy])
	 REFERENCES [dbo].[User] ([UserId])
  END
 END
 -------------------------------------------------------------------------------------------
 --add new foreign_keys RoleId in User_Application table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'User_Application')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_User_Application_Role')
   AND parent_object_id = OBJECT_ID(N'dbo.User_Application')
)
 BEGIN
   IF EXISTS (SELECT * FROM [User_Application] WHERE RoleId NOT IN(SELECT R.RoleId FROM [Role] AS R))
     BEGIN
	  IF EXISTS (SELECT TOP 1 R.RoleId FROM [User_Application] UA INNER JOIN [User] U ON U.UserId=UA.UserId INNER JOIN [Client] C ON C.ClientId=U.ClientId  INNER JOIN [Role] R ON R.ClientId=C.ClientId WHERE R.Code=@User_ApplicationCode AND R.IsDeleted=0)
		  BEGIN
			  UPDATE [User_Application] SET RoleId=(SELECT TOP 1 R.RoleId FROM [User_Application] UA INNER JOIN [User] U ON U.UserId=UA.UserId INNER JOIN [Client] C ON C.ClientId=U.ClientId
			  INNER JOIN [Role] R ON R.ClientId=C.ClientId WHERE R.Code=@User_ApplicationCode AND R.IsDeleted=0) WHERE RoleId NOT IN(SELECT R.RoleId FROM [Role] AS R)
		  END
	  END

	ALTER TABLE [dbo].[User_Application]  WITH CHECK ADD  CONSTRAINT [FK_User_Application_Role] FOREIGN KEY([RoleId])
	 REFERENCES [dbo].[Role] ([RoleId])
  END
END


---------------------------------------------------------------------------------------------------------------
 --UserClientDatabase add new foreign_keys UserId in UserClientDatabase table.
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'UserClientDatabase')
BEGIN
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_UserClientDatabase_User')
   AND parent_object_id = OBJECT_ID(N'dbo.UserClientDatabase')
)
 BEGIN
   IF EXISTS (SELECT * FROM [UserClientDatabase] WHERE UserID NOT IN(SELECT U.UserId FROM [User] AS U))
     BEGIN
	   IF EXISTS (SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0)
		  BEGIN
			  UPDATE [UserClientDatabase] SET UserID=(SELECT TOP 1 U.UserId FROM [User] AS U WHERE JobTitle=@JobTitle AND IsDeleted=0) WHERE UserID NOT IN(SELECT U.UserId FROM [User] AS U)
			END
	  END

	ALTER TABLE [dbo].[UserClientDatabase]  WITH CHECK ADD  CONSTRAINT [FK_UserClientDatabase_User] FOREIGN KEY([UserID])
	  REFERENCES [dbo].[User] ([UserId])
  END
END
-------------------------------------------------------------------------------------------------------------------
