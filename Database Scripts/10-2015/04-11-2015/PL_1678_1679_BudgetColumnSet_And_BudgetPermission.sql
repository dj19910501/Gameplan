GO
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Budget_ColumnSet]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Budget_ColumnSet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](4000) NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Budget_ColumnSet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END
GO

IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Budget_Columns]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[Budget_Columns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Column_SetId] [int] NOT NULL,
	[CustomFieldId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsTimeFrame] [bit] NULL,
	[IsDeleted] [bit] NULL,
	[MapTableName] [nvarchar](100) NULL,
	[ValueOnEditable] [int] NULL,
	[ValidationType] [nvarchar](100) NULL,
 CONSTRAINT [PK_Budget_Columns] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[Budget_Columns]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Columns_Budget_ColumnSet] FOREIGN KEY([Column_SetId])
REFERENCES [dbo].[Budget_ColumnSet] ([Id])

ALTER TABLE [dbo].[Budget_Columns] CHECK CONSTRAINT [FK_Budget_Columns_Budget_ColumnSet]


ALTER TABLE [dbo].[Budget_Columns]  WITH CHECK ADD  CONSTRAINT [FK_Budget_Columns_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

ALTER TABLE [dbo].[Budget_Columns] CHECK CONSTRAINT [FK_Budget_Columns_CustomField]

END
GO



IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[Budget_Permission]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[Budget_Permission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[BudgetDetailId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[PermisssionCode] [int] NOT NULL,
 CONSTRAINT [PK_Budget_Permission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



ALTER TABLE [dbo].[Budget_Permission]  WITH CHECK ADD  CONSTRAINT [BudgetID_FK] FOREIGN KEY([BudgetDetailId])
REFERENCES [dbo].[Budget_Detail] ([Id])

ALTER TABLE [dbo].[Budget_Permission] CHECK CONSTRAINT [BudgetID_FK]

END
GO