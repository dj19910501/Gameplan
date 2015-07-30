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
-- Added by Brad Gray 07-30-2015.

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND TABLE_NAME = 'IntegrationInstanceLogDetails' AND TABLE_SCHEMA ='dbo' )
BEGIN
ALTER TABLE [dbo].[IntegrationInstanceLogDetails]
ADD CONSTRAINT [PK_IntegrationInstanceLogDetails]
    PRIMARY KEY CLUSTERED ([EntityId], [LogTime] ASC);
END
GO
