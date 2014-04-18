--01_PL_391_Application version should be displayed on the login screen.sql
/* To be executed on BDS Auth database */

IF EXISTS (SELECT * FROM [Application] WHERE Code = 'MRP')
BEGIN
	UPDATE [Application] SET Name ='Bulldog Gameplan', [Description] = 'Bulldog Gameplan'
	WHERE Code = 'MRP'
END

/* To be executed on BDS Auth database */

--03_PL_159_UI for Best in class data entry.sql
/* To be executed on BDS Authentication database*/

Update Role_Permission set PermissionCode = 2 where 
RoleId In (select RoleId from Role where Code='SA') and
MenuApplicationId in (select MenuApplicationId from Menu_Application where Code='BOOST' and 
ApplicationId In (select ApplicationId from [Application] where Code='MRP'))


--1_PL_326_Forgot_Password_functionality.sql
-- Execute Below Script in BDSAuth

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'SecurityQuestion')
BEGIN

CREATE TABLE [dbo].[SecurityQuestion](
	[SecurityQuestionId] [int] IDENTITY(1,1) NOT NULL,
	[SecurityQuestion] [nvarchar](255) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SecurityQuestion] PRIMARY KEY CLUSTERED 
(
	[SecurityQuestionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SecurityQuestion] ADD  CONSTRAINT [DF_SecurityQuestion_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What was your childhood nickname?', 0, CAST(0x0000A2F9010E6BF6 AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What is the name of your favorite childhood friend? ', 0, CAST(0x0000A2F9010E6BF6 AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What street did you live on in third grade?', 0, CAST(0x0000A2F9010E6BF6 AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What school did you attend for sixth grade?', 0, CAST(0x0000A2F9010E6BF6 AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What was your childhood phone number including area code? (e.g., 000-000-0000)', 0, CAST(0x0000A2F9010E6BFB AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What is your oldest cousin''s first and last name?', 0, CAST(0x0000A2F9010E6BFB AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What was the last name of your third grade teacher?', 0, CAST(0x0000A2F9010E6BFB AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What is your oldest brother’s birthday month and year? (e.g., January 1900)', 0, CAST(0x0000A2F9010E6BFB AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES (N'What is your maternal grandmother''s maiden name?', 0, CAST(0x0000A2F9010E6BFB AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES ( N'In what city or town was your first job?', 0, CAST(0x0000A2F9010E6BFD AS DateTime))
	INSERT [dbo].[SecurityQuestion] ([SecurityQuestion], [IsDeleted], [CreatedDate]) VALUES ( N'What is the name of a college you applied to but didn''t attend?', 0, CAST(0x0000A2F9010E6BFE AS DateTime))
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'PasswordResetRequest')
BEGIN

CREATE TABLE [dbo].[PasswordResetRequest](
	[PasswordResetRequestId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[AttemptCount] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PasswordResetRequest] PRIMARY KEY CLUSTERED 
(
	[PasswordResetRequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PasswordResetRequest] ADD  CONSTRAINT [DF_PasswordResetRequest_IsUsed]  DEFAULT ((0)) FOR [IsUsed]

ALTER TABLE [dbo].[PasswordResetRequest]  WITH CHECK ADD  CONSTRAINT [FK_PasswordResetRequest_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])

ALTER TABLE [dbo].[PasswordResetRequest] CHECK CONSTRAINT [FK_PasswordResetRequest_User]

END
GO

IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'SecurityQuestionId' AND [object_id] = OBJECT_ID(N'User'))
BEGIN
    Alter table [User] ADD SecurityQuestionId int NULL
END

Go
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Answer' AND [object_id] = OBJECT_ID(N'User'))
BEGIN
    Alter table [User] ADD Answer nvarchar(255) NULL
END

Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_User_SecurityQuestion]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE [User] ADD CONSTRAINT FK_User_SecurityQuestion FOREIGN KEY (SecurityQuestionId) REFERENCES SecurityQuestion(SecurityQuestionId)
END




