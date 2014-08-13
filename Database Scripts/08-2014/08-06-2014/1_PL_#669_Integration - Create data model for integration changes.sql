
/****** Object:  Table [dbo].[IntegrationInstanceExternalServer]    Script Date: 8/6/2014 6:24:29 PM ******/
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstanceExternalServer')
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IntegrationInstanceExternalServer](
	[IntegrationInstanceExternalServerId] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[SFTPServerName] [nvarchar](255) NOT NULL,
	[SFTPFileLocation] [nvarchar](1000) NOT NULL,
	[SFTPUserName] [nvarchar](255) NOT NULL,
	[SFTPPassword] [nvarchar](255) NOT NULL,
	[SFTPPort] [nvarchar](4) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_IntegrationInstanceExternalServer] PRIMARY KEY CLUSTERED 
(
	[IntegrationInstanceExternalServerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[IntegrationInstanceExternalServer] ADD  CONSTRAINT [DF_IntegrationInstanceExternalServer_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]

ALTER TABLE [dbo].[IntegrationInstanceExternalServer]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceExternalServer_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[IntegrationInstanceExternalServer] CHECK CONSTRAINT [FK_IntegrationInstanceExternalServer_IntegrationInstance]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify external server for a integration instance.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceExternalServerId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to the associated integration instance.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IntegrationInstanceId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPServerName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'File location of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPFileLocation'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'User name of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPUserName'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Password of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPPassword'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Port number of SFTP server.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'SFTPPort'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'IsDeleted'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IntegrationInstanceExternalServer', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdINQ' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdINQ INT
	
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceINQ] FOREIGN KEY([IntegrationInstanceIdINQ])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceINQ]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdMQL' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdMQL INT
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceMQL] FOREIGN KEY([IntegrationInstanceIdMQL])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceMQL]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IntegrationInstanceIdCW' AND OBJECT_ID = OBJECT_ID(N'Model'))
BEGIN
    ALTER TABLE [Model] ADD IntegrationInstanceIdCW INT
	ALTER TABLE [dbo].[Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceCW] FOREIGN KEY([IntegrationInstanceIdCW])
	REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
	ALTER TABLE [dbo].[Model] CHECK CONSTRAINT [FK_Model_IntegrationInstanceCW]
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ModifiedDate' AND OBJECT_ID = OBJECT_ID(N'Plan_Campaign_Program_Tactic_Actual'))
BEGIN
    ALTER TABLE [Plan_Campaign_Program_Tactic_Actual] ADD ModifiedDate datetime
END

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'ModifiedBy' AND OBJECT_ID = OBJECT_ID(N'Plan_Campaign_Program_Tactic_Actual'))
BEGIN
    ALTER TABLE [Plan_Campaign_Program_Tactic_Actual] ADD ModifiedBy uniqueidentifier
END