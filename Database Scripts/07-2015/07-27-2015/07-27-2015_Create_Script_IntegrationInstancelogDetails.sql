-- Added by Viral Kadiya 08-04-2015.

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstanceLogDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstanceLogDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[IntegrationInstanceLogId] [int] NULL,
	[LogTime] [datetime] NOT NULL,
	[LogDescription] [nvarchar](max) NULL,
 CONSTRAINT [PK_IntegrationInstanceLogDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
