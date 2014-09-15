--==================CustomFieldType=====================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldType')
BEGIN
CREATE TABLE [dbo].[CustomFieldType](
       [CustomFieldTypeId] [int] IDENTITY(1,1) NOT NULL,
       [Name] [nvarchar](50) NOT NULL,
       [Description] [nvarchar](4000) NULL,
CONSTRAINT [PK_CustomFieldType] PRIMARY KEY CLUSTERED 
(
       [CustomFieldTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'CustomFieldTypeId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'Name'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description for custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'Description'
END

--==================End CustomFieldType=====================

--==================CustomField=====================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField')
BEGIN
CREATE TABLE [dbo].[CustomField](
       [CustomFieldId] [int] IDENTITY(1,1) NOT NULL,
       [Name] [nvarchar](255) NOT NULL,
       [CustomFieldTypeId] [int] NOT NULL,
       [Description] [nvarchar](4000) NULL,
       [IsRequired] [bit] NOT NULL,
       [EntityType] [nvarchar](50) NOT NULL,
       [ClientId] [uniqueidentifier] NOT NULL,
       [IsDeleted] [bit] NOT NULL,
       [CreatedDate] [datetime] NOT NULL,
       [CreatedBy] [uniqueidentifier] NOT NULL,
       [ModifiedDate] [datetime] NULL,
       [ModifiedBy] [uniqueidentifier] NULL,
CONSTRAINT [PK_CustomField] PRIMARY KEY CLUSTERED 
(
       [CustomFieldId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



ALTER TABLE [dbo].[CustomField] ADD  CONSTRAINT [DF_CustomField_IsRequired]  DEFAULT ((0)) FOR [IsRequired]


ALTER TABLE [dbo].[CustomField] ADD  CONSTRAINT [DF_CustomField_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]


ALTER TABLE [dbo].[CustomField]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_CustomFieldType] FOREIGN KEY([CustomFieldTypeId])
REFERENCES [dbo].[CustomFieldType] ([CustomFieldTypeId])

ALTER TABLE [dbo].[CustomField] CHECK CONSTRAINT [FK_CustomField_CustomFieldType]


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CustomFieldId'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'Name'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldTypeId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CustomFieldTypeId'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description for custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'Description'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify custom field is required or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'IsRequired'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type of entity. EntityType can be Plan,Campaign,Program,Tactic,LineItem.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'EntityType'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated ClientId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ClientId'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'IsDeleted'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CreatedDate'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CreatedBy'


EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

END

--==================End CustomField=====================

--==================CustomFieldOption=====================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldOption')
BEGIN

CREATE TABLE [dbo].[CustomFieldOption](
       [CustomFieldOptionId] [int] IDENTITY(1,1) NOT NULL,
       [CustomFieldId] [int] NOT NULL,
       [Value] [nvarchar](255) NOT NULL,
       [CreatedDate] [datetime] NOT NULL,
       [CreatedBy] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_CustomFieldOption] PRIMARY KEY CLUSTERED 
(
       [CustomFieldOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[CustomFieldOption]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldOption_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

ALTER TABLE [dbo].[CustomFieldOption] CHECK CONSTRAINT [FK_CustomFieldOption_CustomField]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field option.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CustomFieldOptionId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Option of custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CreatedBy'




END

--==================End CustomFieldOption=====================

--==================CustomField_Entity=====================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField_Entity')
BEGIN

CREATE TABLE [dbo].[CustomField_Entity](
       [CustomFieldEntityId] [int] IDENTITY(1,1) NOT NULL,
       [EntityId] [int] NOT NULL,
       [CustomFieldId] [int] NOT NULL,
       [Value] [nvarchar](255) NOT NULL,
       [CreatedDate] [datetime] NOT NULL,
       [CreatedBy] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_CustomField_Entity] PRIMARY KEY CLUSTERED 
(
       [CustomFieldEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[CustomField_Entity]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_Entity_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])

ALTER TABLE [dbo].[CustomField_Entity] CHECK CONSTRAINT [FK_CustomField_Entity_CustomField]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify value of custom field and entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CustomFieldEntityId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated Plan, Campaign, Program, Tactic and LineItem id.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'EntityId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CustomFieldId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value for custom field defined for an entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'Value'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CreatedDate'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CreatedBy'


END

--==================End CustomField_Entity=====================

