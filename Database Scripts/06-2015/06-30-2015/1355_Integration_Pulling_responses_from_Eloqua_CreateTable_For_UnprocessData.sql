GO
/****** Object:  Table [dbo].[IntegrationInstance_UnprocessData]    Script Date: 06/30/2015 16:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstance_UnprocessData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[EloquaCampaignID] [nvarchar](50) NULL,
	[ExternalCampaignID] [nvarchar](50) NULL,
	[ResponseDateTime] [datetime] NOT NULL,
	[ResponseCount] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_IntegrationInstance_UnprocessData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IntegrationInstance_UnprocessData_IntegrationInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]'))
ALTER TABLE [dbo].[IntegrationInstance_UnprocessData]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstance_UnprocessData_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IntegrationInstance_UnprocessData_IntegrationInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]'))
ALTER TABLE [dbo].[IntegrationInstance_UnprocessData] CHECK CONSTRAINT [FK_IntegrationInstance_UnprocessData_IntegrationInstance]
GO
