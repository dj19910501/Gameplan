/****** Object:  Table [dbo].[Client_Integration_Permission]    Script Date: 01/09/2015 4:36:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Client_Integration_Permission](
	[ClientIntegrationPermissionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[IntegrationTypeId] [int] NOT NULL,
	[PermissionCode] [varchar](255) NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_Client_Integration_Permission] PRIMARY KEY CLUSTERED 
(
	[ClientIntegrationPermissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Client_Integration_Permission_IntegrationType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]'))
ALTER TABLE [dbo].[Client_Integration_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Client_Integration_Permission_IntegrationType] FOREIGN KEY([IntegrationTypeId])
REFERENCES [dbo].[IntegrationType] ([IntegrationTypeId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Client_Integration_Permission_IntegrationType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Client_Integration_Permission]'))
ALTER TABLE [dbo].[Client_Integration_Permission] CHECK CONSTRAINT [FK_Client_Integration_Permission_IntegrationType]
GO
