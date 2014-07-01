--==================Application_Activity=====================
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Application_Activity')
BEGIN

CREATE TABLE [dbo].[Application_Activity](
	[ApplicationActivityId] [int] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[ParentId] [int] NULL,
	[ActivityTitle] [nvarchar](255) NOT NULL,
	[Code] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Application_Activity] PRIMARY KEY CLUSTERED 
(
	[ApplicationActivityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Application_Activity]  WITH CHECK ADD  CONSTRAINT [FK_Application_Activity_Application] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Application] ([ApplicationId])
ALTER TABLE [dbo].[Application_Activity] CHECK CONSTRAINT [FK_Application_Activity_Application]

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('1',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'User Administration'
		   ,'UserAdministration'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
            ('2',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'1'
           ,'User Administration'
		   ,'UserAdmin'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('3',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Integration Credentials'
		   ,'IntegrationCredential'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('4',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'3'
           ,'Create/Edit'
		   ,'IntegrationCredentialCreateEdit'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
            ('5',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Model'
		   ,'Model'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('6',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'5'
           ,'Create/Edit Models'
		   ,'ModelCreateEdit'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('7',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Plan'
		   ,'Plan'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('8',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'7'
           ,'Create New Plans'
		   ,'PlanCreate'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('9',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'7'
           ,'Edit Own and Subordinates Plans'
		   ,'PlanEditOwnAndSubordinates'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('10',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'7'
           ,'Approve Tactics for Peers'
		   ,'TacticApproveForPeers'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('11',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'7'
           ,'Edit All Plans <br><em> *Subject to restrictions Below</em>'
		   ,'PlanEditAll'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
         ('12',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'7'
           ,'Add/Edit Actuals'
		   ,'TacticActualsAddEdit'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('13',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Boost'
		   ,'Boost'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('14',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'13'
           ,'Create/Edit Improvement Tactics'
		   ,'BoostImprovementTacticCreateEdit'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
           ('15',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'13'
           ,'Edit Best-in-Class Numbers'
		   ,'BoostBestInClassNumberEdit'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('16',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Report'
		   ,'Report'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('17',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'16'
           ,'View Reports'
		   ,'ReportView'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('18',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,NULL
           ,'Comments'
		   ,'Comments'
           ,GETDATE())

INSERT INTO [dbo].[Application_Activity]
           ([ApplicationActivityId]
           ,[ApplicationId]
           ,[ParentId]
           ,[ActivityTitle]
           ,[Code]
           ,[CreatedDate])
     VALUES
          ('19',
           '1C10D4B9-7931-4A7C-99E9-A158CE158951'
           ,'18'
		   ,'View/Edit Comments'
		   ,'CommentsViewEdit'
           ,GETDATE())
END
--=================End Application_Activity===================

--===================[User_Activity_Permission]==================
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User_Activity_Permission')
BEGIN

CREATE TABLE [dbo].[User_Activity_Permission](
	[UserActivityPermissionId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ApplicationActivityId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_User_Activity_Permission] PRIMARY KEY CLUSTERED 
(
	[UserActivityPermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[User_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_User_Activity_Permission_Application_Activity] FOREIGN KEY([ApplicationActivityId])
REFERENCES [dbo].[Application_Activity] ([ApplicationActivityId])
ALTER TABLE [dbo].[User_Activity_Permission] CHECK CONSTRAINT [FK_User_Activity_Permission_Application_Activity]


ALTER TABLE [dbo].[User_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_User_Activity_Permission_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
ALTER TABLE [dbo].[User_Activity_Permission] CHECK CONSTRAINT [FK_User_Activity_Permission_User]

ALTER TABLE [dbo].[User_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_User_Activity_Permission_User1] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[User] ([UserId])
ALTER TABLE [dbo].[User_Activity_Permission] CHECK CONSTRAINT [FK_User_Activity_Permission_User1]

END
--===================End [User_Activity_Permission]==================


--===================Role_Activity_Permission================================
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Role_Activity_Permission')
BEGIN

CREATE TABLE [dbo].[Role_Activity_Permission](
	[RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationActivityId] [int] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Role_Activity_Permission] PRIMARY KEY CLUSTERED 
(
	[RolePermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Role_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Activity_Permission_Application_Activity] FOREIGN KEY([ApplicationActivityId])
REFERENCES [dbo].[Application_Activity] ([ApplicationActivityId])
ALTER TABLE [dbo].[Role_Activity_Permission] CHECK CONSTRAINT [FK_Role_Activity_Permission_Application_Activity]

ALTER TABLE [dbo].[Role_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Activity_Permission_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([RoleId])
ALTER TABLE [dbo].[Role_Activity_Permission] CHECK CONSTRAINT [FK_Role_Activity_Permission_Role]

ALTER TABLE [dbo].[Role_Activity_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Activity_Permission_User] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[User] ([UserId])
ALTER TABLE [dbo].[Role_Activity_Permission] CHECK CONSTRAINT [FK_Role_Activity_Permission_User]

END
--======================End Role_Activity_Permission====================


--======================CustomRestriction===============================
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomRestriction')
BEGIN

CREATE TABLE [dbo].[CustomRestriction](
	[CustomRestrictionId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CustomField] [nvarchar](50) NOT NULL,
	[CustomFieldId] [nvarchar](50) NOT NULL,
	[Permission] [smallint] DEFAULT 0 NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CustomRestriction] PRIMARY KEY CLUSTERED 
(
	[CustomRestrictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User')
BEGIN


ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_User]


ALTER TABLE [dbo].[CustomRestriction]  WITH CHECK ADD  CONSTRAINT [FK_CustomRestriction_User1] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[User] ([UserId])
ALTER TABLE [dbo].[CustomRestriction] CHECK CONSTRAINT [FK_CustomRestriction_User1]


END


END
--======================End CustomRestriction===========================


--======================Add ColorCode column to Role table======
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Role')
BEGIN
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Role' AND COLUMN_NAME = 'ColorCode')
BEGIN
ALTER TABLE dbo.Role
ADD ColorCode NVARCHAR(10) DEFAULT '#fffff' NOT NULL
END
END
--====================End Add ColorCode column to Role table=====


--======================Add ManagerId column to User table=======
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User_Application')
BEGIN
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User_Application' AND COLUMN_NAME = 'ManagerId')
BEGIN
ALTER TABLE dbo.[User_Application]
ADD ManagerId UNIQUEIDENTIFIER NULL

ALTER TABLE [dbo].[User_Application]  WITH CHECK ADD  CONSTRAINT [FK_User_Application_User_ManagerId] FOREIGN KEY([ManagerId])
REFERENCES [dbo].[User] ([UserId])

ALTER TABLE [dbo].[User_Application] CHECK CONSTRAINT [FK_User_Application_User_ManagerId]

END
END
--======================End Add ManagerId column to User table=======

--======================Insert default rights for System Admin, Client Admin, Director and Planner=======
IF NOT EXISTS (SELECT * FROM Role_Activity_Permission)
BEGIN
	DECLARE @roleid uniqueidentifier, @code nvarchar(50)
	DECLARE Role_cursor CURSOR FOR 
	SELECT RoleId, Code
	FROM [Role]
	OPEN Role_cursor

	FETCH NEXT FROM Role_cursor 
	INTO @roleid, @code

	WHILE @@FETCH_STATUS = 0
	BEGIN
		--PRINT cast(@roleid as varchar(max)) + '----- Procession Role ----- ' + @code
		IF(@code='SA' or @code='CA')
		BEGIN
	
			 INSERT INTO [dbo].[Role_Activity_Permission]
					   ([ApplicationActivityId]
					   ,[RoleId]
					   ,[CreatedDate]
					   ,[CreatedBy])
			  SELECT [ApplicationActivityId]
				  ,@RoleId
				  ,GetDate()
				  ,'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL

			  IF  (@code='CA')
			  BEGIN
					UPDATE ROLE SET ColorCode='#7AC943' WHERE CODE = @code
			  END
			  ELSE
			  BEGIN
					UPDATE ROLE SET ColorCode='#FF931E' WHERE CODE = @code
			  END
		END
		ELSE IF (@code='D')
		BEGIN 
		INSERT INTO [dbo].[Role_Activity_Permission]
					   ([ApplicationActivityId]
					   ,[RoleId]
					   ,[CreatedDate]
					   ,[CreatedBy])
			  SELECT [ApplicationActivityId]
				  ,@RoleId
				  ,GetDate()
				  ,'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL AND Code NOT IN ('UserAdmin')

			  	UPDATE ROLE SET ColorCode='#3FA9F5' WHERE CODE = @code
		END
		ELSE IF (@code='P')
		BEGIN 
			  INSERT INTO [dbo].[Role_Activity_Permission]
							   ([ApplicationActivityId]
							   ,[RoleId]
							   ,[CreatedDate]
							   ,[CreatedBy])
			  SELECT [ApplicationActivityId]
				  ,@RoleId
				  ,GetDate()
				  ,'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL AND CODE IN ('PlanCreate', 'PlanEditOwnAndSubordinates', 'PlanEditAll', 'TacticActualsAddEdit')

			  UPDATE ROLE SET ColorCode='#3F3F3F' WHERE CODE = @code
		END

			-- Get the next ROLE.
		FETCH NEXT FROM Role_cursor 
		INTO @roleid, @code
	END 
	CLOSE Role_cursor;
	DEALLOCATE Role_cursor;
END

--======================Insert default rights for user with System Admin, Client Admin, Director and Planner=======
IF NOT EXISTS (SELECT * FROM User_Activity_Permission)
BEGIN
	DECLARE @userroleid uniqueidentifier,  @userid uniqueidentifier, @rolecode nvarchar(50)
	DECLARE User_cursor CURSOR FOR 
	SELECT [UserId]
      ,[RoleId]
    FROM [BDSAuthQA].[dbo].[User_Application] where ApplicationId in (select ApplicationId from Application where Code='MRP') and UserId in (select userid from [user] where IsDeleted=0)
 	OPEN User_cursor

	FETCH NEXT FROM User_cursor 
	INTO @userid, @userroleid

	WHILE @@FETCH_STATUS = 0
	BEGIN
		select @rolecode = Code from [Role] where RoleId=@userroleid
		--PRINT '----- Procession Role ----- ' + @code
		IF(@rolecode='SA' or @rolecode='CA')
		BEGIN
			 INSERT INTO [dbo].[User_Activity_Permission]
							   ([UserId]
							   ,[ApplicationActivityId]
							   ,[CreatedDate]
							   ,[CreatedBy])
			  SELECT @userid,
					 [ApplicationActivityId],
					 GetDate(),
				    'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL
		END
		ELSE IF (@rolecode='D')
		BEGIN 
			 INSERT INTO [dbo].[User_Activity_Permission]
							   ([UserId]
							   ,[ApplicationActivityId]
							   ,[CreatedDate]
							   ,[CreatedBy])
			  SELECT @userid,
					 [ApplicationActivityId],
					 GetDate(),
				    'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL AND Code NOT IN ('UserAdmin')
		END
		ELSE IF (@rolecode='P')
		BEGIN 
			 INSERT INTO [dbo].[User_Activity_Permission]
							   ([UserId]
							   ,[ApplicationActivityId]
							   ,[CreatedDate]
							   ,[CreatedBy])
			  SELECT @userid,
					 [ApplicationActivityId],
					 GetDate(),
				    'e5ef88eb-4748-4436-9acc-aba6b2c5f6a9'
			  FROM [dbo].[Application_Activity] WHERE ParentId IS NOT NULL AND CODE IN ('PlanCreate', 'PlanEditOwnAndSubordinates', 'PlanEditAll', 'TacticActualsAddEdit')

			  UPDATE ROLE SET ColorCode='#3F3F3F' WHERE CODE = @rolecode
		END

			-- Get the next user.
		FETCH NEXT FROM User_cursor 
	INTO @userid, @userroleid
	END 
	CLOSE User_cursor;
	DEALLOCATE User_cursor;
END
