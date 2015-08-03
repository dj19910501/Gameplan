IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstanceLogDetails]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstanceLogDetails](
	[EntityId] [int] NOT NULL,
	[IntegrationInstanceLogId] [int] NULL,
	[LogTime] [datetime] NOT NULL,
	[LogDescription] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


END
GO


-- Creating primary key on [EntityId], [LogTime] in table 'IntegrationInstanceLogDetails'
-- Added by Brad Gray 07-30-2015 and modified by Brad Gray on 08-03-2015.

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND TABLE_NAME = 'IntegrationInstanceLogDetails' AND TABLE_SCHEMA ='dbo' )
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

ALTER TABLE [dbo].[IntegrationInstanceLogDetails]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstanceLogID] FOREIGN KEY([IntegrationInstanceLogId])
REFERENCES [dbo].[IntegrationInstanceLog] ([IntegrationInstanceLogId])

ALTER TABLE [dbo].[IntegrationInstanceLogDetails] CHECK CONSTRAINT [FK_IntegrationInstanceLogID]

END
GO
