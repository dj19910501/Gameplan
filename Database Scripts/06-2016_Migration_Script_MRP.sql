--Start Integration Script for Gameplan
--Insertation Start 07/06/2016 kausha #2258
--Table Creation
IF OBJECT_ID('CK_FiscalQuarterYear') IS NOT NULL 
	ALTER TABLE [FiscalQuarterYear] DROP CONSTRAINT [CK_FiscalQuarterYear]
GO
IF OBJECT_ID('FK_User_RestrictedDimensionValues_Dimension') IS NOT NULL 
	ALTER TABLE [User_RestrictedDimensionValues] DROP CONSTRAINT [FK_User_RestrictedDimensionValues_Dimension]
GO
IF OBJECT_ID('FK_User_Permission_Dashboard') IS NOT NULL 
	ALTER TABLE [User_Permission] DROP CONSTRAINT [FK_User_Permission_Dashboard]
GO
IF OBJECT_ID('FK_Role_RestrictedDimensionValues_Dimension') IS NOT NULL 
	ALTER TABLE [Role_RestrictedDimensionValues] DROP CONSTRAINT [FK_Role_RestrictedDimensionValues_Dimension]
GO
IF OBJECT_ID('FK_Role_Permission_Dashboard') IS NOT NULL 
	ALTER TABLE [Role_Permission] DROP CONSTRAINT [FK_Role_Permission_Dashboard]
GO
IF OBJECT_ID('FK_ReportTableRowExclude_ReportTable') IS NOT NULL 
	ALTER TABLE [ReportTableRowExclude] DROP CONSTRAINT [FK_ReportTableRowExclude_ReportTable]
GO
IF OBJECT_ID('FK_ReportTableDimension_ReportTable') IS NOT NULL 
	ALTER TABLE [ReportTableDimension] DROP CONSTRAINT [FK_ReportTableDimension_ReportTable]
GO
IF OBJECT_ID('FK_ReportTableDimension_Dimension') IS NOT NULL 
	ALTER TABLE [ReportTableDimension] DROP CONSTRAINT [FK_ReportTableDimension_Dimension]
GO
IF OBJECT_ID('FK_ReportTableColumn_ReportTable') IS NOT NULL 
	ALTER TABLE [ReportTableColumn] DROP CONSTRAINT [FK_ReportTableColumn_ReportTable]
GO
IF OBJECT_ID('FK_ReportTableColumn_Measure') IS NOT NULL 
	ALTER TABLE [ReportTableColumn] DROP CONSTRAINT [FK_ReportTableColumn_Measure]
GO
IF OBJECT_ID('FK_ReportTableColumn_Dimension') IS NOT NULL 
	ALTER TABLE [ReportTableColumn] DROP CONSTRAINT [FK_ReportTableColumn_Dimension]
GO
IF OBJECT_ID('FK_ReportTable_Dashboard') IS NOT NULL 
	ALTER TABLE [ReportTable] DROP CONSTRAINT [FK_ReportTable_Dashboard]
GO
IF OBJECT_ID('FK_ReportGraphColumn_YearMeasure') IS NOT NULL 
ALTER TABLE [ReportGraphColumn] DROP CONSTRAINT [FK_ReportGraphColumn_YearMeasure]
GO
IF OBJECT_ID('FK_ReportGraphColumn_WeekMeasure') IS NOT NULL 
ALTER TABLE [ReportGraphColumn] DROP CONSTRAINT [FK_ReportGraphColumn_WeekMeasure]
GO
IF OBJECT_ID('FK_ReportGraphColumn_QuarterMeasure') IS NOT NULL 
ALTER TABLE [ReportGraphColumn] DROP CONSTRAINT [FK_ReportGraphColumn_QuarterMeasure]
GO
IF OBJECT_ID('FK_ReportGraphColumn_MonthMeasure') IS NOT NULL 
ALTER TABLE [ReportGraphColumn] DROP CONSTRAINT [FK_ReportGraphColumn_MonthMeasure]
GO
IF OBJECT_ID('FK_ReportGraphColumn_Measure') IS NOT NULL 
ALTER TABLE [ReportGraphColumn] DROP CONSTRAINT [FK_ReportGraphColumn_Measure]
GO
IF OBJECT_ID('FK_ReportAxis_Dimension') IS NOT NULL 
ALTER TABLE [ReportAxis] DROP CONSTRAINT [FK_ReportAxis_Dimension]
GO
IF OBJECT_ID('FK_MenuItems_Homepage') IS NOT NULL 
ALTER TABLE [MenuItems] DROP CONSTRAINT [FK_MenuItems_Homepage]
GO
IF OBJECT_ID('FK_MenuItems_Dashboard') IS NOT NULL 
ALTER TABLE [MenuItems] DROP CONSTRAINT [FK_MenuItems_Dashboard]
GO
IF OBJECT_ID('FK_MeasureOutputValue_Measure') IS NOT NULL 
ALTER TABLE [MeasureOutputValue] DROP CONSTRAINT [FK_MeasureOutputValue_Measure]
GO
IF OBJECT_ID('FK_KeyDataDimension_KeyData') IS NOT NULL 
ALTER TABLE [KeyDataDimension] DROP CONSTRAINT [FK_KeyDataDimension_KeyData]
GO
IF OBJECT_ID('FK_KeyDataDimension_Dimension') IS NOT NULL 
ALTER TABLE [KeyDataDimension] DROP CONSTRAINT [FK_KeyDataDimension_Dimension]
GO
IF OBJECT_ID('FK_KeyData_Measure') IS NOT NULL 
ALTER TABLE [KeyData] DROP CONSTRAINT [FK_KeyData_Measure]
GO
IF OBJECT_ID('FK_KeyData_HelpText') IS NOT NULL 
ALTER TABLE [KeyData] DROP CONSTRAINT [FK_KeyData_HelpText]
GO
IF OBJECT_ID('FK_KeyData_Dashboard') IS NOT NULL 
ALTER TABLE [KeyData] DROP CONSTRAINT [FK_KeyData_Dashboard]
GO
IF OBJECT_ID('FK_HomepageDimension_Homepage') IS NOT NULL 
ALTER TABLE [HomepageDimension] DROP CONSTRAINT [FK_HomepageDimension_Homepage]
GO
IF OBJECT_ID('FK_HomepageDimension_DashboardDimension') IS NOT NULL 
ALTER TABLE [HomepageDimension] DROP CONSTRAINT [FK_HomepageDimension_DashboardDimension]
GO
IF OBJECT_ID('FK_HomepageContent_KeyData') IS NOT NULL 
ALTER TABLE [HomepageContent] DROP CONSTRAINT [FK_HomepageContent_KeyData]
GO
IF OBJECT_ID('FK_HomepageContent_Homepage') IS NOT NULL 
ALTER TABLE [HomepageContent] DROP CONSTRAINT [FK_HomepageContent_Homepage]
GO
IF OBJECT_ID('FK_HomepageContent_Dashboard') IS NOT NULL 
ALTER TABLE [HomepageContent] DROP CONSTRAINT [FK_HomepageContent_Dashboard]
GO
IF OBJECT_ID('FK_Homepage_HelpText') IS NOT NULL 
ALTER TABLE [Homepage] DROP CONSTRAINT [FK_Homepage_HelpText]
GO
IF OBJECT_ID('FK_GraphRelation_ReportGraph_1') IS NOT NULL 
ALTER TABLE [GraphRelation] DROP CONSTRAINT [FK_GraphRelation_ReportGraph_1]
GO
IF OBJECT_ID('FK_GraphRelation_ReportGraph') IS NOT NULL 
ALTER TABLE [GraphRelation] DROP CONSTRAINT [FK_GraphRelation_ReportGraph]
GO
IF OBJECT_ID('FK_GraphRelation_ReportGraph') IS NOT NULL 
ALTER TABLE [GoalDistribution] DROP CONSTRAINT [FK_GoalDistribution_Measure]
GO
IF OBJECT_ID('FK_GoalDimension_Goal') IS NOT NULL 
ALTER TABLE [GoalDimension] DROP CONSTRAINT [FK_GoalDimension_Goal]
GO
IF OBJECT_ID('FK_GoalDimension_Dimension') IS NOT NULL 
ALTER TABLE [GoalDimension] DROP CONSTRAINT [FK_GoalDimension_Dimension]
GO
IF OBJECT_ID('FK_Goal_Measure') IS NOT NULL 
ALTER TABLE [Goal] DROP CONSTRAINT [FK_Goal_Measure]
GO
IF OBJECT_ID('FK_DrillDataConfig_DashboardContents') IS NOT NULL 
ALTER TABLE [DrillDataConfig] DROP CONSTRAINT [FK_DrillDataConfig_DashboardContents]
GO
IF OBJECT_ID('FK_DrillDataConfig_Dashboard') IS NOT NULL 
ALTER TABLE [DrillDataConfig] DROP CONSTRAINT [FK_DrillDataConfig_Dashboard]
GO
IF OBJECT_ID('FK_Dimension_RestrictedDimensionValues_Dimension1') IS NOT NULL 
ALTER TABLE [Dimension_RestrictedDimensionValues] DROP CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension1]
GO
IF OBJECT_ID('FK_Dimension_RestrictedDimensionValues_Dimension') IS NOT NULL 
ALTER TABLE [Dimension_RestrictedDimensionValues] DROP CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension]
GO
IF OBJECT_ID('FK_DashboardPage_Dashboard') IS NOT NULL 
ALTER TABLE [DashboardPage] DROP CONSTRAINT [FK_DashboardPage_Dashboard]
GO
IF OBJECT_ID('FK_DashboardDimension_Dimension') IS NOT NULL 
ALTER TABLE [DashboardDimension] DROP CONSTRAINT [FK_DashboardDimension_Dimension]
GO
IF OBJECT_ID('FK_DashboardDimension_Dashboard') IS NOT NULL 
ALTER TABLE [DashboardDimension] DROP CONSTRAINT [FK_DashboardDimension_Dashboard]
GO
IF OBJECT_ID('FK_DashboardContents_KeyData') IS NOT NULL 
ALTER TABLE [DashboardContents] DROP CONSTRAINT [FK_DashboardContents_KeyData]
GO
IF OBJECT_ID('FK_dashboardcontents_HelpText') IS NOT NULL 
ALTER TABLE [DashboardContents] DROP CONSTRAINT [FK_dashboardcontents_HelpText]
GO
IF OBJECT_ID('FK_dashboard_HelpText') IS NOT NULL 
ALTER TABLE [Dashboard] DROP CONSTRAINT [FK_dashboard_HelpText]
GO
IF OBJECT_ID('FK_Dashboard_Dashboard') IS NOT NULL 
ALTER TABLE [Dashboard] DROP CONSTRAINT [FK_Dashboard_Dashboard]
GO
IF OBJECT_ID('FK_AttrPositionConfig_Dimension') IS NOT NULL 
ALTER TABLE [AttrPositionConfig] DROP CONSTRAINT [FK_AttrPositionConfig_Dimension]
GO
IF OBJECT_ID('UserSettings') IS NOT NULL 
DROP TABLE [UserSettings]
GO
IF OBJECT_ID('User_RestrictedDimensionValues') IS NOT NULL 
DROP TABLE [User_RestrictedDimensionValues]
GO
IF OBJECT_ID('User_Permission') IS NOT NULL 
DROP TABLE [User_Permission]
GO
IF OBJECT_ID('TValues') IS NOT NULL 
DROP TABLE [TValues]
GO
IF OBJECT_ID('Settings') IS NOT NULL 
DROP TABLE [Settings]
GO
IF OBJECT_ID('Role_RestrictedDimensionValues') IS NOT NULL 
DROP TABLE [Role_RestrictedDimensionValues]
GO
IF OBJECT_ID('Role_Permission') IS NOT NULL 
DROP TABLE [Role_Permission]
GO
IF OBJECT_ID('ReportTableRowExclude') IS NOT NULL 
DROP TABLE [ReportTableRowExclude]
GO
IF OBJECT_ID('ReportTableDimension') IS NOT NULL 
DROP TABLE [ReportTableDimension]
GO
IF OBJECT_ID('ReportTableColumn') IS NOT NULL 
DROP TABLE [ReportTableColumn]
GO
IF OBJECT_ID('ReportTable') IS NOT NULL 
DROP TABLE [ReportTable]
GO
IF OBJECT_ID('ReportGraphRowExclude') IS NOT NULL 
DROP TABLE [ReportGraphRowExclude]
GO
IF OBJECT_ID('ReportGraphColumn') IS NOT NULL 
DROP TABLE [ReportGraphColumn]
GO
IF OBJECT_ID('ReportGraph') IS NOT NULL 
DROP TABLE [ReportGraph]
GO
IF OBJECT_ID('ReportAxis') IS NOT NULL 
DROP TABLE [ReportAxis]
GO
IF OBJECT_ID('ProcessedFQY') IS NOT NULL 
DROP TABLE [ProcessedFQY]
GO
IF OBJECT_ID('MenuItems') IS NOT NULL 
DROP TABLE [MenuItems]
GO
IF OBJECT_ID('MeasureValue') IS NOT NULL 
DROP TABLE [MeasureValue]
GO
IF OBJECT_ID('MeasureOutputValue') IS NOT NULL 
DROP TABLE [MeasureOutputValue]
GO

IF OBJECT_ID('Logging') IS NOT NULL 
DROP TABLE [Logging]
GO
IF OBJECT_ID('KeyDataDimension') IS NOT NULL 
DROP TABLE [KeyDataDimension]
GO
IF OBJECT_ID('KeyData') IS NOT NULL 
DROP TABLE [KeyData]
GO
IF OBJECT_ID('HomepageDimension') IS NOT NULL 
DROP TABLE [HomepageDimension]
GO
IF OBJECT_ID('HomepageContent') IS NOT NULL 
DROP TABLE [HomepageContent]
GO
IF OBJECT_ID('Homepage') IS NOT NULL 
DROP TABLE [Homepage]
GO
IF OBJECT_ID('HelpText') IS NOT NULL 
DROP TABLE [HelpText]
GO
IF OBJECT_ID('GraphRelation') IS NOT NULL 
DROP TABLE [GraphRelation]
GO
IF OBJECT_ID('GoalDistribution') IS NOT NULL 
DROP TABLE [GoalDistribution]
GO
IF OBJECT_ID('GoalDimension') IS NOT NULL 
DROP TABLE [GoalDimension]
GO
IF OBJECT_ID('Goal') IS NOT NULL 
DROP TABLE [Goal]
GO
IF OBJECT_ID('FiscalQuarterYear') IS NOT NULL 
DROP TABLE [FiscalQuarterYear]
GO
IF OBJECT_ID('DynamicDimension') IS NOT NULL 
DROP TABLE [DynamicDimension]
GO
IF OBJECT_ID('DrillDataConfig') IS NOT NULL 
DROP TABLE [DrillDataConfig]
GO
IF OBJECT_ID('DimTime') IS NOT NULL 
DROP TABLE [DimTime]
GO

IF OBJECT_ID('Dimension_RestrictedDimensionValues') IS NOT NULL 
DROP TABLE [Dimension_RestrictedDimensionValues]
GO
IF OBJECT_ID('Dimension') IS NOT NULL 
DROP TABLE [Dimension]
GO
IF OBJECT_ID('DashboardPage') IS NOT NULL 
DROP TABLE [DashboardPage]
GO
IF OBJECT_ID('DashboardDimension') IS NOT NULL 
DROP TABLE [DashboardDimension]
GO
IF OBJECT_ID('DashboardContents') IS NOT NULL 
DROP TABLE [DashboardContents]
GO
IF OBJECT_ID('Dashboard') IS NOT NULL 
DROP TABLE [Dashboard]
GO
IF OBJECT_ID('ConfigurationTestCases') IS NOT NULL 
DROP TABLE [ConfigurationTestCases]
GO
IF OBJECT_ID('ColorSequence') IS NOT NULL 
DROP TABLE [ColorSequence]
GO
IF OBJECT_ID('ChartOptionAttribute') IS NOT NULL 
DROP TABLE [ChartOptionAttribute]
GO
IF OBJECT_ID('AttrPositionConfig') IS NOT NULL 
DROP TABLE [AttrPositionConfig]
GO
IF OBJECT_ID('AggregationSteps') IS NOT NULL 
DROP TABLE [AggregationSteps]
GO
IF OBJECT_ID('AggregationStatus') IS NOT NULL 
DROP TABLE [AggregationStatus]
GO
IF OBJECT_ID('AggregationQueries') IS NOT NULL 
DROP TABLE [AggregationQueries]
GO
IF OBJECT_ID('AggregationProcessLog') IS NOT NULL 
DROP TABLE [AggregationProcessLog]
GO

IF OBJECT_ID('DimensionValue') IS NOT NULL 
DROP TABLE [DimensionValue]
Go
IF OBJECT_ID('Measure') IS NOT NULL 
DROP TABLE [Measure]
GO
GO
CREATE TABLE [AggregationProcessLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[ErrorDesription] [nvarchar](4000) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ProcedureName] [nvarchar](100) NULL,
	[QueryText] [nvarchar](max) NULL,
 CONSTRAINT [PK_AggregationProcessLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [AggregationQueries](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[QueryToRun] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [AggregationStatus](
	[StatusCode] [nvarchar](100) NOT NULL
) ON [PRIMARY]

GO

CREATE TABLE [AggregationSteps](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[StepName] [nvarchar](255) NOT NULL,
	[LogStart] [bit] NOT NULL,
	[LogEnd] [bit] NOT NULL,
	[QueryText] [varchar](max) NOT NULL,
	[StepOrder] [float] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[isDeleted] [bit] NOT NULL,
	[StatusCode] [varchar](100) NULL,
	[PartialQueryText] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [AttrPositionConfig](
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[XValue] [float] NOT NULL,
	[YValue] [float] NOT NULL,
	[DimensionId] [int] NULL,
	[DimensionValue] [nvarchar](max) NULL,
	[Weight] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [ChartOptionAttribute](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportGraphId] [int] NOT NULL,
	[AttributeKey] [nvarchar](255) NOT NULL,
	[AttributeValue] [nvarchar](255) NULL,
 CONSTRAINT [PK_ChartOptionAttribute] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_ReportGraphId_AttributeKey] UNIQUE NONCLUSTERED 
(
	[ReportGraphId] ASC,
	[AttributeKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ColorSequence](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ColorCode] [nvarchar](50) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[SequenceNumber] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_ColorSequence] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ConfigurationTestCases](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[TestCaseName] [nvarchar](100) NOT NULL,
	[TestCaseQuery] [nvarchar](max) NULL,
	[TestStoredProcedure] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](100) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[isDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ConfigurationTestCases] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [Dashboard](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DisplayName] [nvarchar](250) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[CustomCSS] [nvarchar](250) NULL,
	[Rows] [int] NULL,
	[Columns] [int] NULL,
	[ParentDashboardId] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[IsComparisonDisplay] [bit] NULL,
	[HelpTextId] [int] NULL,
 CONSTRAINT [PK_Dashboard] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DashboardContents](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](50) NOT NULL,
	[DashboardId] [int] NOT NULL,
	[DisplayOrder] [float] NOT NULL,
	[ReportTableId] [int] NULL,
	[ReportGraphId] [int] NULL,
	[Height] [int] NULL,
	[Width] [int] NULL,
	[Position] [nvarchar](50) NULL,
	[IsCumulativeData] [bit] NULL,
	[IsCommunicativeData] [bit] NULL,
	[DashboardPageID] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DisplayIfZero] [nvarchar](20) NULL,
	[KeyDataId] [int] NULL,
	[HelpTextId] [int] NULL,
 CONSTRAINT [PK_DashboardContents] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DashboardDimension](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardId] [int] NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DimensionType] [nvarchar](50) NOT NULL,
	[DisplayOrder] [float] NOT NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[DashboardPageID] [int] NULL,
 CONSTRAINT [PK_DashboardDimension] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DashboardPage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardID] [int] NOT NULL,
	[PageName] [nvarchar](50) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_DashboardPage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Dimension](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[ColumnName] [nvarchar](50) NOT NULL,
	[Formula] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsDateDimension] [bit] NULL,
	[ValueFormula] [nvarchar](1000) NULL,
	[ComputeAllValues] [bit] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
 CONSTRAINT [PK_Dimension] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [Dimension_RestrictedDimensionValues](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DimensionValue] [nvarchar](1000) NOT NULL,
	[RestrictedDimentionId] [int] NOT NULL,
	[RestrictedDimensionValue] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK_Dimension_RestrictedDimensionValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DimensionValue](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DimensionID] [int] NOT NULL,
	[Value] [nvarchar](1000) NULL,
	[DisplayValue] [nvarchar](1000) NULL,
	[OrderValue] [nvarchar](1000) NULL,
 CONSTRAINT [PK_DimensionValue_New_41709329] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DimTime](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[Year] [int] NOT NULL,
	[Quarter] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[Monthname] [nvarchar](15) NOT NULL,
	[Day] [int] NOT NULL,
 CONSTRAINT [PK_DimTime] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [DrillDataConfig](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardId] [int] NOT NULL,
	[DisplayByText] [nvarchar](250) NOT NULL,
	[DisplayByQuery] [nvarchar](max) NOT NULL,
	[PrimaryColumns] [nvarchar](1000) NULL,
	[DisplayMeasureValue] [bit] NULL,
	[DashboardContentId] [int] NULL,
 CONSTRAINT [PK_DrillDataConfig] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [DynamicDimension](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](500) NOT NULL,
	[Dimensions] [int] NULL,
	[DimensionTableName] [nvarchar](50) NULL,
	[ComputeAllValues] [bit] NULL,
	[DimensionValueTableName] [nvarchar](100) NULL,
	[ContainsDateDimension] [bit] NULL
) ON [PRIMARY]

GO

CREATE TABLE [FiscalQuarterYear](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FiscalQuarter] [int] NOT NULL,
	[FiscalYear] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
	[Month] [int] NULL,
 CONSTRAINT [PK_FiscalQuarterYear] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Goal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Month] [int] NULL,
	[Quarter] [int] NULL,
	[Year] [int] NOT NULL,
	[MeasureId] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL CONSTRAINT [DF_Goal_CreatedDate]  DEFAULT (getdate()),
	[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_Goal_IsDeleted]  DEFAULT ((0)),
	[DateDeleted] [datetime] NULL,
	[IsFiscalGoal] [bit] NULL DEFAULT (NULL),
 CONSTRAINT [PK__Goal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [GoalDimension](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GoalId] [int] NOT NULL,
	[GoalValue] [float] NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DisplayValue] [nvarchar](1000) NULL,
 CONSTRAINT [PK__GoalDime__3214EC07707B64D2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [GoalDistribution](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MeasureId] [int] NOT NULL,
	[DistributionType] [int] NOT NULL,
	[DistributionPeriodNumber] [int] NOT NULL,
	[DistributionPercentage] [float] NOT NULL,
	[Period] [int] NULL,
 CONSTRAINT [PK_GoalDistribution] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [GraphRelation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentGraphId] [int] NOT NULL,
	[ChildGraphId] [int] NOT NULL,
	[CenterPie] [nvarchar](10) NULL,
	[SizePie] [int] NULL,
	[IsYAxisDisplayRight] [bit] NULL,
	[DisplayOrder] [int] NULL,
 CONSTRAINT [PK_GraphRelation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [HelpText](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Help_Text] [nvarchar](150) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_HelpText] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [Homepage](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[DisplayName] [nvarchar](250) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[CustomCSS] [nvarchar](250) NULL,
	[Rows] [int] NULL,
	[Columns] [int] NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT ((0)),
	[IsComparisonDisplay] [bit] NULL,
	[HelpTextId] [int] NULL,
 CONSTRAINT [PK_Homepage] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [HomepageContent](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardContentId] [int] NULL,
	[KeyDataId] [int] NULL,
	[DashboardId] [int] NOT NULL,
	[RowNumber] [int] NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[DisplayTitle] [bit] NOT NULL,
	[Height] [int] NOT NULL,
	[Width] [int] NOT NULL,
	[DisplayDateStamp] [bit] NOT NULL,
	[HomepageID] [int] NULL,
 CONSTRAINT [PK_HomepageContent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [HomepageDimension](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardDimensionId] [int] NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[HomepageID] [int] NULL,
 CONSTRAINT [PK_HomepageDimension] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [KeyData](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[KeyDataName] [nvarchar](50) NULL,
	[DashboardId] [int] NULL,
	[MeasureId] [int] NULL,
	[OrderValue] [int] NULL,
	[Prefix] [nvarchar](10) NULL,
	[Suffix] [nvarchar](10) NULL,
	[DisplayIfZero] [nvarchar](20) NULL,
	[NoOfDecimal] [int] NULL DEFAULT ((0)),
	[DashboardPageID] [int] NULL,
	[IsComparisonDisplay] [bit] NULL,
	[ComparisonType] [nvarchar](10) NULL,
	[IsIndicatorDisplay] [bit] NOT NULL DEFAULT ((0)),
	[IsGoalDisplayForFilterCriteria] [bit] NOT NULL DEFAULT ((0)),
	[DateRangeGoalOption] [int] NOT NULL DEFAULT ((1)),
	[MagnitudeValue] [nvarchar](10) NULL,
	[HelpTextId] [int] NULL,
 CONSTRAINT [PK_KeyData] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [KeyDataDimension](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[KeyDataId] [int] NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DimensionValue] [nvarchar](1000) NULL,
 CONSTRAINT [PK_KeyDataDimension] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Logging](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Time] [datetime] NOT NULL,
	[Description] [nvarchar](1000) NOT NULL
) ON [PRIMARY]

GO

CREATE TABLE [Measure](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[AggregationQuery] [nvarchar](4000) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[MeasureTableName] [nvarchar](100) NULL,
	[AggregationType] [nvarchar](50) NULL,
	[DisplayColorIndication] [bit] NULL,
	[ComputeAllValues] [bit] NULL,
	[ComputeAllValuesFormula] [nvarchar](1000) NULL,
	[DrillDownWhereClause] [nvarchar](1000) NULL,
	[UseRowCountFromFormula] [bit] NULL,
 CONSTRAINT [PK_Measure] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [MeasureOutputValue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MeasureId] [int] NOT NULL,
	[LowerLimit] [float] NOT NULL,
	[UpperLimit] [float] NOT NULL,
	[Value] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_MeasureOutputValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [Unique_Measure_Values] UNIQUE NONCLUSTERED 
(
	[MeasureId] ASC,
	[LowerLimit] ASC,
	[UpperLimit] ASC,
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [MeasureValue](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Measure] [int] NOT NULL,
	[DimensionValue] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[RecordCount] [float] NULL,
 CONSTRAINT [PK_MeasureValue_New41709329] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [MenuItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HomepageId] [int] NULL,
	[DashboardId] [int] NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_MenuItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ProcessedFQY](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FiscalQuarter] [int] NOT NULL,
	[FiscalYear] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[Month] [int] NULL,
	[MonthName] [nvarchar](50) NULL,
 CONSTRAINT [PK_ProcessedFQY] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportAxis](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportGraphId] [int] NOT NULL,
	[AxisName] [nvarchar](2) NOT NULL,
	[Dimensionid] [int] NOT NULL,
	[GroupDimensionValue] [nvarchar](1000) NULL,
 CONSTRAINT [PK_ReportAxis] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportGraph](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[GraphType] [nvarchar](20) NULL,
	[IsLableDisplay] [bit] NULL,
	[LabelPosition] [nvarchar](20) NULL,
	[IsLegendVisible] [bit] NULL,
	[LegendPosition] [nvarchar](20) NULL,
	[IsDataLabelVisible] [bit] NULL,
	[DataLablePosition] [nvarchar](20) NULL,
	[DefaultRows] [int] NULL,
	[ChartAttribute] [nvarchar](max) NULL,
	[ConfidenceLevel] [float] NULL,
	[IsGoalDisplay] [bit] NOT NULL DEFAULT ((0)),
	[IsGoalDisplayForFilterCritaria] [bit] NOT NULL DEFAULT ((0)),
	[DateRangeGoalOption] [int] NOT NULL DEFAULT ((1)),
	[IsComparisonDisplay] [bit] NULL,
	[CustomQuery] [nvarchar](max) NULL,
	[IsIndicatorDisplay] [bit] NULL,
	[IsSortByValue] [bit] NOT NULL DEFAULT ((0)),
	[SortOrder] [nvarchar](5) NOT NULL DEFAULT ('asc'),
	[DisplayGoalAsLine] [bit] NULL,
	[CustomFilter] [nvarchar](max) NULL DEFAULT ((0)),
	[DrillDownCustomQuery] [nvarchar](max) NULL,
	[DrillDownXFilter] [nvarchar](max) NULL,
	[TotalDecimalPlaces] [int] NULL,
	[MagnitudeValue] [nvarchar](10) NULL,
	[IsICustomFilterApply] [bit] NULL,
	[IsTrendLineDisplay] [bit] NULL,
 CONSTRAINT [PK_ReportGraph] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [ReportGraphColumn](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportGraphId] [int] NOT NULL,
	[MeasureId] [int] NOT NULL,
	[ColumnOrder] [int] NULL,
	[SymbolType] [nvarchar](50) NULL,
	[DisplayInTable] [bit] NULL,
	[PrevMeasureCalculation] [bit] NULL,
	[DisplayAsNumerator] [bit] NULL,
	[DisplayAsDenominator] [bit] NULL,
	[WeekMeasureId] [int] NULL,
	[MonthMeasureId] [int] NULL,
	[QuarterMeasureId] [int] NULL,
	[YearMeasureId] [int] NULL,
	[TotalDecimalPlaces] [int] NULL,
	[MagnitudeValue] [nvarchar](10) NULL,
 CONSTRAINT [PK_ReportGraphColumn] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportGraphRowExclude](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportGraphID] [int] NOT NULL,
	[Exclude] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ReportGraphRowExclude] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportTable](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[DashboardId] [int] NULL,
	[ShowFooterRow] [bit] NULL,
	[IsLastDateOnly] [bit] NULL,
	[DefaultRows] [int] NULL,
	[SortColumnNumber] [int] NULL,
	[SortDirection] [nvarchar](15) NULL,
	[IsGoalDisplay] [bit] NOT NULL DEFAULT ((0)),
	[IsGoalDisplayForFilterCritaria] [bit] NOT NULL DEFAULT ((0)),
	[DateRangeGoalOption] [int] NOT NULL DEFAULT ((1)),
	[IsComparisonDisplay] [bit] NOT NULL DEFAULT ((0)),
	[CustomQuery] [nvarchar](max) NULL,
	[ComparisonType] [nvarchar](10) NULL,
	[IsGoalDisplayAsColumn] [bit] NOT NULL DEFAULT ((0)),
	[CustomFilter] [nvarchar](max) NULL DEFAULT ((0)),
	[IsICustomFilterApply] [bit] NULL,
	[TotalDecimalPlaces] [int] NULL,
	[MagnitudeValue] [nvarchar](10) NULL,
 CONSTRAINT [PK_ReportTable] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [ReportTableColumn](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportTableId] [int] NOT NULL,
	[MeasureId] [int] NOT NULL,
	[ColumnOrder] [int] NOT NULL,
	[SymbolType] [nvarchar](50) NULL,
	[DimensionId] [int] NULL,
	[ComparisonType] [nvarchar](10) NULL,
	[MagnitudeValue] [nvarchar](10) NULL,
	[TotalDecimalPlaces] [int] NULL,
 CONSTRAINT [PK_ReportTableColumn] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportTableDimension](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportTableId] [int] NULL,
	[DimensionId] [int] NULL,
	[DisplayAsColumn] [bit] NULL,
	[DisplayOrder] [int] NULL,
 CONSTRAINT [PK_ReportTableDimension] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ReportTableRowExclude](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ReportTableID] [int] NOT NULL,
	[Exclude] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_ReportTableRowExclude] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Role_Permission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardId] [int] NULL,
	[ApplicationActivityId] [int] NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[PermissionType] [nvarchar](255) NULL,
	[HomePageId] [int] NULL,
 CONSTRAINT [PK_Role_Permission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Role_RestrictedDimensionValues](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DimensionValue] [nvarchar](1000) NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Role_RestrictedDimensionValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [Settings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](255) NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [TValues](
	[Degrees] [int] NULL,
	[Probability] [float] NULL,
	[TStat] [float] NULL
) ON [PRIMARY]

GO

CREATE TABLE [User_Permission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DashboardId] [int] NULL,
	[ApplicationActivityId] [int] NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[PermissionType] [nvarchar](255) NULL,
	[HomePageId] [int] NULL,
 CONSTRAINT [PK_User_Permission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [User_RestrictedDimensionValues](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DimensionId] [int] NOT NULL,
	[DimensionValue] [nvarchar](1000) NULL,
	[UserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_User_RestrictedDimensionValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [UserSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Key] [nvarchar](500) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
	[isDeleted] [bit] NOT NULL CONSTRAINT [DF_UserSettings_isDeleted]  DEFAULT ((0)),
 CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF OBJECT_ID('FK_AttrPositionConfig_Dimension') IS NULL 
BEGIN
ALTER TABLE [AttrPositionConfig]  WITH CHECK ADD  CONSTRAINT [FK_AttrPositionConfig_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [AttrPositionConfig] CHECK CONSTRAINT [FK_AttrPositionConfig_Dimension]
END
GO
IF OBJECT_ID('FK_Dashboard_Dashboard') IS NULL 
BEGIN
ALTER TABLE [Dashboard]  WITH CHECK ADD  CONSTRAINT [FK_Dashboard_Dashboard] FOREIGN KEY([ParentDashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [Dashboard] CHECK CONSTRAINT [FK_Dashboard_Dashboard]
END
GO

IF OBJECT_ID('FK_dashboard_HelpText') IS NULL 
BEGIN
ALTER TABLE [Dashboard]  WITH CHECK ADD  CONSTRAINT [FK_dashboard_HelpText] FOREIGN KEY([HelpTextId])
REFERENCES [HelpText] ([Id])

ALTER TABLE [Dashboard] CHECK CONSTRAINT [FK_dashboard_HelpText]
END
GO

IF OBJECT_ID('FK_dashboardcontents_HelpText') IS NULL 
BEGIN
ALTER TABLE [DashboardContents]  WITH CHECK ADD  CONSTRAINT [FK_dashboardcontents_HelpText] FOREIGN KEY([HelpTextId])
REFERENCES [HelpText] ([Id])

ALTER TABLE [DashboardContents] CHECK CONSTRAINT [FK_dashboardcontents_HelpText]
END
GO

IF OBJECT_ID('FK_DashboardContents_KeyData') IS NULL 
BEGIN
ALTER TABLE [DashboardContents]  WITH CHECK ADD  CONSTRAINT [FK_DashboardContents_KeyData] FOREIGN KEY([KeyDataId])
REFERENCES [KeyData] ([id])

ALTER TABLE [DashboardContents] CHECK CONSTRAINT [FK_DashboardContents_KeyData]
END
GO
IF OBJECT_ID('FK_DashboardDimension_Dashboard') IS NULL 
BEGIN
ALTER TABLE [DashboardDimension]  WITH CHECK ADD  CONSTRAINT [FK_DashboardDimension_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [DashboardDimension] CHECK CONSTRAINT [FK_DashboardDimension_Dashboard]
END
GO

IF OBJECT_ID('FK_DashboardDimension_Dimension') IS NULL 
BEGIN
ALTER TABLE [DashboardDimension]  WITH CHECK ADD  CONSTRAINT [FK_DashboardDimension_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [DashboardDimension] CHECK CONSTRAINT [FK_DashboardDimension_Dimension]
END
GO

IF OBJECT_ID('FK_DashboardPage_Dashboard') IS NULL 
BEGIN
ALTER TABLE [DashboardPage]  WITH CHECK ADD  CONSTRAINT [FK_DashboardPage_Dashboard] FOREIGN KEY([DashboardID])
REFERENCES [Dashboard] ([id])

ALTER TABLE [DashboardPage] CHECK CONSTRAINT [FK_DashboardPage_Dashboard]
END
GO

IF OBJECT_ID('FK_Dimension_RestrictedDimensionValues_Dimension') IS NULL 
BEGIN
ALTER TABLE [Dimension_RestrictedDimensionValues]  WITH CHECK ADD  CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [Dimension_RestrictedDimensionValues] CHECK CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension]
END
GO

IF OBJECT_ID('FK_Dimension_RestrictedDimensionValues_Dimension1') IS NULL 
BEGIN
ALTER TABLE [Dimension_RestrictedDimensionValues]  WITH CHECK ADD  CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension1] FOREIGN KEY([RestrictedDimentionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [Dimension_RestrictedDimensionValues] CHECK CONSTRAINT [FK_Dimension_RestrictedDimensionValues_Dimension1]
END
GO

IF OBJECT_ID('FK_DrillDataConfig_Dashboard') IS NULL 
BEGIN
ALTER TABLE [DrillDataConfig]  WITH CHECK ADD  CONSTRAINT [FK_DrillDataConfig_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [DrillDataConfig] CHECK CONSTRAINT [FK_DrillDataConfig_Dashboard]
END
GO

IF OBJECT_ID('FK_DrillDataConfig_DashboardContents') IS NULL 
BEGIN
ALTER TABLE [DrillDataConfig]  WITH CHECK ADD  CONSTRAINT [FK_DrillDataConfig_DashboardContents] FOREIGN KEY([DashboardContentId])
REFERENCES [DashboardContents] ([id])

ALTER TABLE [DrillDataConfig] CHECK CONSTRAINT [FK_DrillDataConfig_DashboardContents]
END
GO

IF OBJECT_ID('FK_Goal_Measure') IS NULL 
BEGIN
ALTER TABLE [Goal]  WITH CHECK ADD  CONSTRAINT [FK_Goal_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [Goal] CHECK CONSTRAINT [FK_Goal_Measure]
END
GO

IF OBJECT_ID('FK_GoalDimension_Dimension') IS NULL 
BEGIN
ALTER TABLE [GoalDimension]  WITH CHECK ADD  CONSTRAINT [FK_GoalDimension_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [GoalDimension] CHECK CONSTRAINT [FK_GoalDimension_Dimension]
END
GO


IF OBJECT_ID('FK_GoalDimension_Goal') IS NULL 
BEGIN
ALTER TABLE [GoalDimension]  WITH CHECK ADD  CONSTRAINT [FK_GoalDimension_Goal] FOREIGN KEY([GoalId])
REFERENCES [Goal] ([Id])

ALTER TABLE [GoalDimension] CHECK CONSTRAINT [FK_GoalDimension_Goal]
END
GO

IF OBJECT_ID('FK_GoalDistribution_Measure') IS NULL 
BEGIN
ALTER TABLE [GoalDistribution]  WITH CHECK ADD  CONSTRAINT [FK_GoalDistribution_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [GoalDistribution] CHECK CONSTRAINT [FK_GoalDistribution_Measure]
END
GO

IF OBJECT_ID('FK_GraphRelation_ReportGraph') IS NULL 
BEGIN
ALTER TABLE [GraphRelation]  WITH CHECK ADD  CONSTRAINT [FK_GraphRelation_ReportGraph] FOREIGN KEY([ParentGraphId])
REFERENCES [ReportGraph] ([id])

ALTER TABLE [GraphRelation] CHECK CONSTRAINT [FK_GraphRelation_ReportGraph]
END
GO

IF OBJECT_ID('FK_GraphRelation_ReportGraph_1') IS NULL 
BEGIN
ALTER TABLE [GraphRelation]  WITH CHECK ADD  CONSTRAINT [FK_GraphRelation_ReportGraph_1] FOREIGN KEY([ChildGraphId])
REFERENCES [ReportGraph] ([id])

ALTER TABLE [GraphRelation] CHECK CONSTRAINT [FK_GraphRelation_ReportGraph_1]
END
GO

IF OBJECT_ID('FK_Homepage_HelpText') IS NULL 
BEGIN
ALTER TABLE [Homepage]  WITH CHECK ADD  CONSTRAINT [FK_Homepage_HelpText] FOREIGN KEY([HelpTextId])
REFERENCES [HelpText] ([Id])

ALTER TABLE [Homepage] CHECK CONSTRAINT [FK_Homepage_HelpText]
END
GO

IF OBJECT_ID('FK_HomepageContent_Dashboard') IS NULL 
BEGIN
ALTER TABLE [HomepageContent]  WITH CHECK ADD  CONSTRAINT [FK_HomepageContent_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [HomepageContent] CHECK CONSTRAINT [FK_HomepageContent_Dashboard]
END
GO

IF OBJECT_ID('FK_HomepageContent_Homepage') IS NULL 
BEGIN
ALTER TABLE [HomepageContent]  WITH CHECK ADD  CONSTRAINT [FK_HomepageContent_Homepage] FOREIGN KEY([HomepageID])
REFERENCES [Homepage] ([id])

ALTER TABLE [HomepageContent] CHECK CONSTRAINT [FK_HomepageContent_Homepage]
END
GO

IF OBJECT_ID('FK_HomepageContent_KeyData') IS NULL 
BEGIN
ALTER TABLE [HomepageContent]  WITH CHECK ADD  CONSTRAINT [FK_HomepageContent_KeyData] FOREIGN KEY([KeyDataId])
REFERENCES [KeyData] ([id])

ALTER TABLE [HomepageContent] CHECK CONSTRAINT [FK_HomepageContent_KeyData]
END
GO

IF OBJECT_ID('FK_HomepageDimension_DashboardDimension') IS NULL 
BEGIN
ALTER TABLE [HomepageDimension]  WITH CHECK ADD  CONSTRAINT [FK_HomepageDimension_DashboardDimension] FOREIGN KEY([DashboardDimensionId])
REFERENCES [DashboardDimension] ([id])

ALTER TABLE [HomepageDimension] CHECK CONSTRAINT [FK_HomepageDimension_DashboardDimension]
END
GO

IF OBJECT_ID('FK_HomepageDimension_Homepage') IS NULL 
BEGIN
ALTER TABLE [HomepageDimension]  WITH CHECK ADD  CONSTRAINT [FK_HomepageDimension_Homepage] FOREIGN KEY([HomepageID])
REFERENCES [Homepage] ([id])

ALTER TABLE [HomepageDimension] CHECK CONSTRAINT [FK_HomepageDimension_Homepage]
END
GO

IF OBJECT_ID('FK_KeyData_Dashboard') IS NULL 
BEGIN
ALTER TABLE [KeyData]  WITH CHECK ADD  CONSTRAINT [FK_KeyData_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [KeyData] CHECK CONSTRAINT [FK_KeyData_Dashboard]
END
GO

IF OBJECT_ID('FK_KeyData_HelpText') IS NULL 
BEGIN
ALTER TABLE [KeyData]  WITH CHECK ADD  CONSTRAINT [FK_KeyData_HelpText] FOREIGN KEY([HelpTextId])
REFERENCES [HelpText] ([Id])

ALTER TABLE [KeyData] CHECK CONSTRAINT [FK_KeyData_HelpText]
END
GO

IF OBJECT_ID('FK_KeyData_Measure') IS NULL 
BEGIN
ALTER TABLE [KeyData]  WITH CHECK ADD  CONSTRAINT [FK_KeyData_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [KeyData] CHECK CONSTRAINT [FK_KeyData_Measure]
END
GO

IF OBJECT_ID('FK_KeyDataDimension_Dimension') IS NULL 
BEGIN
ALTER TABLE [KeyDataDimension]  WITH CHECK ADD  CONSTRAINT [FK_KeyDataDimension_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [KeyDataDimension] CHECK CONSTRAINT [FK_KeyDataDimension_Dimension]
END
GO

IF OBJECT_ID('FK_KeyDataDimension_KeyData') IS NULL 
BEGIN
ALTER TABLE [KeyDataDimension]  WITH CHECK ADD  CONSTRAINT [FK_KeyDataDimension_KeyData] FOREIGN KEY([KeyDataId])
REFERENCES [KeyData] ([id])

ALTER TABLE [KeyDataDimension] CHECK CONSTRAINT [FK_KeyDataDimension_KeyData]
END
GO

IF OBJECT_ID('FK_MeasureOutputValue_Measure') IS NULL 
BEGIN
ALTER TABLE [MeasureOutputValue]  WITH CHECK ADD  CONSTRAINT [FK_MeasureOutputValue_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [MeasureOutputValue] CHECK CONSTRAINT [FK_MeasureOutputValue_Measure]
END
GO

IF OBJECT_ID('FK_MenuItems_Dashboard') IS NULL 
BEGIN
ALTER TABLE [MenuItems]  WITH CHECK ADD  CONSTRAINT [FK_MenuItems_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [MenuItems] CHECK CONSTRAINT [FK_MenuItems_Dashboard]
END
GO

IF OBJECT_ID('FK_MenuItems_Homepage') IS NULL 
BEGIN
ALTER TABLE [MenuItems]  WITH CHECK ADD  CONSTRAINT [FK_MenuItems_Homepage] FOREIGN KEY([HomepageId])
REFERENCES [Homepage] ([id])

ALTER TABLE [MenuItems] CHECK CONSTRAINT [FK_MenuItems_Homepage]
END
GO

IF OBJECT_ID('FK_ReportAxis_Dimension') IS NULL 
BEGIN
ALTER TABLE [ReportAxis]  WITH CHECK ADD  CONSTRAINT [FK_ReportAxis_Dimension] FOREIGN KEY([Dimensionid])
REFERENCES [Dimension] ([id])

ALTER TABLE [ReportAxis] CHECK CONSTRAINT [FK_ReportAxis_Dimension]
END
GO

IF OBJECT_ID('FK_ReportGraphColumn_Measure') IS NULL 
BEGIN
ALTER TABLE [ReportGraphColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportGraphColumn_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportGraphColumn] CHECK CONSTRAINT [FK_ReportGraphColumn_Measure]
END
GO

IF OBJECT_ID('FK_ReportGraphColumn_MonthMeasure') IS NULL 
BEGIN
ALTER TABLE [ReportGraphColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportGraphColumn_MonthMeasure] FOREIGN KEY([MonthMeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportGraphColumn] CHECK CONSTRAINT [FK_ReportGraphColumn_MonthMeasure]
END
GO

IF OBJECT_ID('FK_ReportGraphColumn_QuarterMeasure') IS NULL 
BEGIN
ALTER TABLE [ReportGraphColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportGraphColumn_QuarterMeasure] FOREIGN KEY([QuarterMeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportGraphColumn] CHECK CONSTRAINT [FK_ReportGraphColumn_QuarterMeasure]
END
GO

IF OBJECT_ID('FK_ReportGraphColumn_WeekMeasure') IS NULL 
BEGIN
ALTER TABLE [ReportGraphColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportGraphColumn_WeekMeasure] FOREIGN KEY([WeekMeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportGraphColumn] CHECK CONSTRAINT [FK_ReportGraphColumn_WeekMeasure]
END
GO

IF OBJECT_ID('FK_ReportGraphColumn_YearMeasure') IS NULL 
BEGIN
ALTER TABLE [ReportGraphColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportGraphColumn_YearMeasure] FOREIGN KEY([YearMeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportGraphColumn] CHECK CONSTRAINT [FK_ReportGraphColumn_YearMeasure]
END
GO

IF OBJECT_ID('FK_ReportTable_Dashboard') IS NULL 
BEGIN
ALTER TABLE [ReportTable]  WITH CHECK ADD  CONSTRAINT [FK_ReportTable_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [ReportTable] CHECK CONSTRAINT [FK_ReportTable_Dashboard]
END
GO

IF OBJECT_ID('FK_ReportTableColumn_Dimension') IS NULL 
BEGIN
ALTER TABLE [ReportTableColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableColumn_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [ReportTableColumn] CHECK CONSTRAINT [FK_ReportTableColumn_Dimension]
END
GO

IF OBJECT_ID('FK_ReportTableColumn_Measure') IS NULL 
BEGIN
ALTER TABLE [ReportTableColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableColumn_Measure] FOREIGN KEY([MeasureId])
REFERENCES [Measure] ([id])

ALTER TABLE [ReportTableColumn] CHECK CONSTRAINT [FK_ReportTableColumn_Measure]
END
GO

IF OBJECT_ID('FK_ReportTableColumn_ReportTable') IS NULL 
BEGIN
ALTER TABLE [ReportTableColumn]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableColumn_ReportTable] FOREIGN KEY([ReportTableId])
REFERENCES [ReportTable] ([id])

ALTER TABLE [ReportTableColumn] CHECK CONSTRAINT [FK_ReportTableColumn_ReportTable]
END
GO

IF OBJECT_ID('FK_ReportTableDimension_Dimension') IS NULL 
BEGIN
ALTER TABLE [ReportTableDimension]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableDimension_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [ReportTableDimension] CHECK CONSTRAINT [FK_ReportTableDimension_Dimension]
END
GO
IF OBJECT_ID('FK_ReportTableDimension_ReportTable') IS NULL 
BEGIN
ALTER TABLE [ReportTableDimension]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableDimension_ReportTable] FOREIGN KEY([ReportTableId])
REFERENCES [ReportTable] ([id])

ALTER TABLE [ReportTableDimension] CHECK CONSTRAINT [FK_ReportTableDimension_ReportTable]
END
GO

IF OBJECT_ID('FK_ReportTableRowExclude_ReportTable') IS NULL 
BEGIN
ALTER TABLE [ReportTableRowExclude]  WITH CHECK ADD  CONSTRAINT [FK_ReportTableRowExclude_ReportTable] FOREIGN KEY([ReportTableID])
REFERENCES [ReportTable] ([id])

ALTER TABLE [ReportTableRowExclude] CHECK CONSTRAINT [FK_ReportTableRowExclude_ReportTable]
END

GO
IF OBJECT_ID('FK_Role_Permission_Dashboard') IS NULL 
BEGIN
ALTER TABLE [Role_Permission]  WITH CHECK ADD  CONSTRAINT [FK_Role_Permission_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [Role_Permission] CHECK CONSTRAINT [FK_Role_Permission_Dashboard]
END
GO
IF OBJECT_ID('FK_Role_RestrictedDimensionValues_Dimension') IS NULL 
BEGIN
ALTER TABLE [Role_RestrictedDimensionValues]  WITH CHECK ADD  CONSTRAINT [FK_Role_RestrictedDimensionValues_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [Role_RestrictedDimensionValues] CHECK CONSTRAINT [FK_Role_RestrictedDimensionValues_Dimension]
END
GO
IF OBJECT_ID('FK_User_Permission_Dashboard') IS NULL 
BEGIN
ALTER TABLE [User_Permission]  WITH CHECK ADD  CONSTRAINT [FK_User_Permission_Dashboard] FOREIGN KEY([DashboardId])
REFERENCES [Dashboard] ([id])

ALTER TABLE [User_Permission] CHECK CONSTRAINT [FK_User_Permission_Dashboard]
END
GO
IF OBJECT_ID('FK_User_RestrictedDimensionValues_Dimension') IS NULL 
BEGIN
ALTER TABLE [User_RestrictedDimensionValues]  WITH CHECK ADD  CONSTRAINT [FK_User_RestrictedDimensionValues_Dimension] FOREIGN KEY([DimensionId])
REFERENCES [Dimension] ([id])

ALTER TABLE [User_RestrictedDimensionValues] CHECK CONSTRAINT [FK_User_RestrictedDimensionValues_Dimension]
END
GO
IF OBJECT_ID('CK_FiscalQuarterYear') IS NULL 
BEGIN
ALTER TABLE [FiscalQuarterYear]  WITH CHECK ADD  CONSTRAINT [CK_FiscalQuarterYear] CHECK  (([FiscalQuarter]=(4) OR [FiscalQuarter]=(3) OR [FiscalQuarter]=(2) OR [FiscalQuarter]=(1)))

ALTER TABLE [FiscalQuarterYear] CHECK CONSTRAINT [CK_FiscalQuarterYear]
END
GO

--Insert data

DELETE FROM AggregationSteps
INSERT INTO AggregationSteps VALUES ('PopulateDynamicDimension'	,0,0,'exec dbo.PopulateDynamicDimension @DEBUG=1'	,20,'BCG',GETDATE(),0,'','exec dbo.PopulateDynamicDimensionPartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('BaseDynamicColumnCreation',0,0,'exec dbo.BaseDynamicColumnCreation @DEBUG=1'	,30,'BCG',GETDATE(),0,'','exec dbo.BaseDynamicColumnCreationPartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('DimensionValuePopulate'	,0,0,'exec dbo.DimensionValuePopulate @DEBUG=1'		,40,'BCG',GETDATE(),0,'','exec dbo.DimensionValuePopulatePartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('DynamicTableCreation'		,0,0,'exec dbo.DynamicTableCreation @DEBUG=1'		,50,'BCG',GETDATE(),0,'','exec dbo.DynamicTableCreationPartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('CreateDynamicColumns'		,0,0,'exec dbo.CreateDynamicColumns @DEBUG=1'		,60,'BCG',GETDATE(),0,'','exec dbo.CreateDynamicColumns @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('PopulateDynamicColumns'	,0,0,'exec dbo.PopulateDynamicColumns @DEBUG=1'		,70,'BCG',GETDATE(),0,'','exec dbo.PopulateDynamicColumnsPartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('Copy Over'				,0,0,'exec dbo.CopyOverAggregation @DEBUG=1'		,80,'BCG',GETDATE(),0,'','exec dbo.CopyOverAggregationPartial @DEBUG=1')
INSERT INTO AggregationSteps VALUES ('Rebuild Indexes'			,0,0,'exec dbo.RebuildIndexes @DEBUG=1'				,90,'BCG',GETDATE(),0,'',NULL)
INSERT INTO AggregationSteps VALUES ('Create Triggers'			,0,0,'exec dbo.CreateBaseTableTriggers @DEBUG=1'	,91,'BCG',GETDATE(),0,'',NULL)


DELETE FROM ColorSequence
INSERT INTO ColorSequence VALUES ('#5F91B3',1,1)
INSERT INTO ColorSequence VALUES ('#4d0e00',2,1)
INSERT INTO ColorSequence VALUES ('#C7C9BE',3,1)
INSERT INTO ColorSequence VALUES ('#E2502D',4,1)
INSERT INTO ColorSequence VALUES ('#6e6259',5,1)
INSERT INTO ColorSequence VALUES ('#004E7B',6,1)
INSERT INTO ColorSequence VALUES ('#65190e',7,1)
INSERT INTO ColorSequence VALUES ('#D5ECFF',8,1)
INSERT INTO ColorSequence VALUES ('#F8710E',9,1)
INSERT INTO ColorSequence VALUES ('#3E3F37',10,1)
INSERT INTO ColorSequence VALUES ('#4C4E7B',11,1)
INSERT INTO ColorSequence VALUES ('#7D0E00',12,1)
INSERT INTO ColorSequence VALUES ('#b5d9e7',13,1)
INSERT INTO ColorSequence VALUES ('#B63115',14,1)
INSERT INTO ColorSequence VALUES ('#636f79',15,1)
INSERT INTO ColorSequence VALUES ('#524e7b',16,1)
INSERT INTO ColorSequence VALUES ('#B63115',17,1)
INSERT INTO ColorSequence VALUES ('#89CEDE',18,1)
INSERT INTO ColorSequence VALUES ('#E2B22C',19,1)
INSERT INTO ColorSequence VALUES ('#637877',20,1)

DELETE FROM DimTime
DECLARE @StartDate DATETIME = '1/1/1900',@EndDate DATETIME = '12/31/2100'
;WITH AllDates AS(
	SELECT @StartDate [DateTime], DATEPART(YEAR,@StartDate) [Year],DATEPART(QUARTER,@StartDate) [Quarter],DATEPART(MONTH,@StartDate) [Month],DATENAME(MONTH, @StartDate) MonthName,DATEPART(DAY,@StartDate) [Day]

	UNION ALL

	SELECT DATEADD(Day,1,[DateTime]) [DateTime], DATEPART(YEAR,DATEADD(Day,1,[DateTime])) [Year],DATEPART(QUARTER,DATEADD(Day,1,[DateTime])) [Quarter],DATEPART(MONTH,DATEADD(Day,1,[DateTime])) [Month],DATENAME(MONTH, DATEADD(Day,1,[DateTime])) MonthName,DATEPART(DAY,DATEADD(Day,1,[DateTime])) [Day]
	FROM AllDates WHERE DateTime <  @EndDate
)
INSERT INTO DimTime
SELECT * FROM AllDates
OPTION (MAXRECURSION 0);
Go
---Scalar Value Function
--1
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateFiscalQuarterYear') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION CalculateFiscalQuarterYear
GO
--Create procedure and function

-- =============================================
-- Author:		Arpita Soni
-- Create date: 05/01/2015
-- Description:	Calculate fiscal quarter and year values
-- =============================================
-- SELECT [dbo].[CalculateFiscalQuarterYear]('FQ','01/14/2015')
CREATE FUNCTION [dbo].[CalculateFiscalQuarterYear]
(	
	-- Add the parameters for the function here
	@ViewByValue varchar(10),
	@Date datetime
)
RETURNS NVARCHAR(10)
AS
BEGIN

	IF (@ViewByValue='FQ')
	BEGIN
		RETURN (SELECT TOP 1 + 'Q' + CONVERT(NVARCHAR,FiscalQuarter) + ' ' + CONVERT(NVARCHAR,FiscalYear) FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
	ELSE IF (@ViewByValue='FM')
	BEGIN
		RETURN (SELECT TOP 1 + CONVERT(NVARCHAR, UPPER(LEFT([MonthName], 1)) + LOWER(RIGHT([MonthName], LEN([MonthName]) - 1))) + '-' + CONVERT(NVARCHAR,fiscalyear) FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
	ELSE 
	BEGIN
		RETURN (SELECT TOP 1 FiscalYear FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
return 0
END

Go
--2
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateStartAndEndDate') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION CalculateStartAndEndDate
GO
CREATE FUNCTION [CalculateStartAndEndDate]
(
	@DateDimension NVARCHAR(500),
	@ViewByValue NVARCHAR(15),
	@STARTDATE date, 
	@ENDDATE date
)
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE @StdStartDate DATETIME, @StdEndDate DATETIME, @ReturnValue NVARCHAR(100);
	
	IF(@ViewByValue='Y')
	BEGIN
		SET @StdStartDate=CONVERT(DATETIME, CAST(@DateDimension AS NVARCHAR) + '-01-01',20)
		SET @StdEndDate= CONVERT(DATETIME, CAST(@DateDimension AS NVARCHAR)+ '-12-31',20)
	END
	ELSE IF(@ViewByValue='Q')
	BEGIN
		DECLARE @MonthDigitQ INT;
		SET @MonthDigitQ = (3 * (SUBSTRING(@DateDimension,2,1) - 1)) + 1;
		SET @StdStartDate = CONVERT(DATETIME,SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitQ AS NVARCHAR)+ '-1',20)
		SET @StdEndDate =  DATEADD(DAY,-1,DATEADD(MONTH,1,CONVERT(DATETIME,CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitQ + 2 AS NVARCHAR)+ '-01' AS NVARCHAR),20)))
	END
	ELSE IF(@ViewByValue='M')
	BEGIN
		DECLARE @MonthDigitM INT;
		SET @MonthDigitM = DATEPART(MM, LEFT(@DateDimension,3) + ' ' + SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4))
		SET @StdStartDate = CONVERT(DATETIME,SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitM AS NVARCHAR)+ '-1',20)
		SET @StdEndDate =  DATEADD(DAY,-1,DATEADD(MONTH,1,CONVERT(DATETIME,CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitM AS NVARCHAR)+ '-01' AS NVARCHAR),20)))
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		DECLARE @MonthDigitW INT, @Year INT,@Date INT;
		IF(CHARINDEX('-',@DateDimension) = 0)
		BEGIN
			SET @Year = CAST(LEFT(@STARTDATE,4) AS INT)
			SET @Date = CAST(SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,LEN(@DateDimension)-CHARINDEX(' ',@DateDimension)) AS INT)
		END
		ELSE 
		BEGIN
			SET @Year = CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) AS INT)
			SET @Date = SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,CHARINDEX('-',@DateDimension)-CHARINDEX(' ',@DateDimension)-1)
			END
		SET @MonthDigitW = DATEPART(MM, LEFT(@DateDimension,3) + ' 01 ' + CAST(@Year AS NVARCHAR))
		SET @StdStartDate = CONVERT(DATETIME,CAST(@Year AS NVARCHAR) + '-' + CAST(@MonthDigitW AS NVARCHAR) + '-' + CAST(@Date AS NVARCHAR),20)
		SET @StdEndDate = DATEADD(DAY,6,CONVERT(DATETIME,CAST(@Year AS NVARCHAR) + '-' + CAST(@MonthDigitW AS NVARCHAR) + '-' + CAST(@Date AS NVARCHAR),20))
	END
	ELSE IF(@ViewByValue='FQ')
	BEGIN
		SELECT @StdStartDate=StartDate, @StdEndDate=EndDate FROM ProcessedFQY WHERE [FiscalQuarter]=SUBSTRING(@DateDimension,2,1) and [FiscalYear]=SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,4)
	END
	ELSE IF(@ViewByValue='FY')
	BEGIN
		SELECT @StdStartDate=StartDate FROM ProcessedFQY WHERE [FiscalQuarter]=1 and [FiscalYear]=@DateDimension
		SELECT @StdEndDate=EndDate FROM ProcessedFQY WHERE [FiscalQuarter]=4 and [FiscalYear]=@DateDimension
	END
	
	-- Compare with filter date preset start and end date 
	IF(@StdStartDate < @StartDate)
		SET @STARTDATE = @StdStartDate
	IF(@StdEndDate < @EndDate)
		set @ENDDATE = @StdEndDate
	
	SET @ReturnValue = CAST(@STARTDATE AS NVARCHAR) +','+ CAST(@ENDDATE AS NVARCHAR)
	RETURN @ReturnValue;
END

GO
--3
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ComputeErrorBar') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION ComputeErrorBar
GO
CREATE Function [ComputeErrorBar] (@PopulationRate float, @NumberTests float, @ConfidenceLevel float, @NumberOfTails int)
returns float
as
BEGIN
	--Function will return the upper range of the confidence interval for the selected proportion
	if @ConfidenceLevel <= 0 or @ConfidenceLevel >=1
	BEGIN
		return null
	END
	if @NumberOfTails<>1 and @NumberOfTails<>2
	BEGIN
		return null
	END
	DECLARE @SD float = sqrt( @NumberTests*@PopulationRate*(1-@PopulationRate))
	DECLARE @Z float = dbo.normsinv((1-@ConfidenceLevel)/@NumberOfTails)

	return @NumberTests*@PopulationRate-(@Z*@SD)

END

GO
--4
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionBaseQuery') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION DimensionBaseQuery
GO

CREATE FUNCTION [DimensionBaseQuery] 
( 
    @DIMENSIONTABLENAME NVARCHAR(1000), 
	@STARTDATE date='01/01/2014', 
	@ENDDATE date='12/31/2014',
	@ReportGraphId int ,
	@FilterValues nvarchar(MAX) =null ,
	@IsOnlyDateDimension bit =0,
	@IsDimension bit=0,
	@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
	@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
) 
RETURNS  NVARCHAR(MAX)
BEGIN
DECLARE @OrderByCount int=1
DECLARE @DimensionCount int=0
DECLARE @Dimensionid int,@DimensionIndex NVARCHAR(100),@IsDateDimension BIT, @i INT =3
DECLARE @ExcludeQuery nvarchar(MAX)
SET @ExcludeQuery=''

DECLARE @QueryToRun NVARCHAR(MAX) 
--Chek is only Date dimension then it will get values from dimension value table
IF(@IsOnlyDateDimension=1 AND @FilterValues IS NULL)
BEGIN
SET @DimensionId=(SELECT TOP 1 DimensionId from ReportAxis where reportgraphid=@ReportGraphID )
SET @QueryToRun='';
IF(@IsDimension=0)
SET @QueryToRun =  'SELECT ' + 'DISTINCT #COLUMNS# '  + ' FROM MeasureValue D1 INNER JOIN DimensionValue D3 ON D1.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','') 
ELSE
SET @QueryToRun ='SELECT '+'DISTINCT #COLUMNS# '+'  from dimensionvalue D3 where DimensionID=' + REPLACE(@DIMENSIONTABLENAME,'D','')+' and CAST(DisplayValue AS DATE) between '''+CAST(@STARTDATE AS NVARCHAR)+''' and '''+CAST(@ENDDATE AS NVARCHAR)+''' order by ordervalue1'
END
ELSE
BEGIN
SET @QueryToRun= 'SELECT distinct '+' #COLUMNS# '+' FROM ' + @DIMENSIONTABLENAME + 'Value D1'
       SET @QueryToRun = @QueryToRun + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' D2 ON D2.id = D1.' + @DIMENSIONTABLENAME 

DECLARE Column_Cursor CURSOR FOR
SELECT D.id,D.IsDateDimension,FD.cnt
FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D') FD
INNER JOIN Dimension D ON D.id = FD.dimension
OPEN Column_Cursor
       FETCH NEXT FROM Column_Cursor
       INTO @Dimensionid,@IsDateDimension, @DimensionIndex
       WHILE @@FETCH_STATUS = 0
       BEGIN
	    
		/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
		-- Restrict dimension values as per UserId and RoleId
		--Insertation Start 24/02/2016 Kausha Added if conditionDue to resolve performance issue we have removed unneccesary join for ticket no #729,#730
		IF(@IsDateDimension=1 OR ((SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphID=@ReportGraphId AND DimensionId=@Dimensionid)>0))
    BEGIN
		DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
		IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
		END
		ELSE 
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
		END
		IF(CHARINDEX(',',@RestrictedDimensionValues) = 2)
			BEGIN
				SET @RestrictedDimensionValues = SUBSTRING(@RestrictedDimensionValues,4,LEN(@RestrictedDimensionValues))
			END
		/* End - Added by Arpita Soni for ticket #511 on 10/30/2015 */

	   IF(@ExcludeQuery IS NOT NULL)
	   SET @ExcludeQuery=@ExcludeQuery+' and '
       SET @ExcludeQuery=@ExcludeQuery+'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue not in (select Exclude FROM ReportGraphRowExclude WHERE ReportGraphId=' +cast(@ReportGraphId as nvarchar)+')'

	   /* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
	   IF(ISNULL(@RestrictedDimensionValues,'') != '')
			SET @ExcludeQuery = @ExcludeQuery + ' AND ' + 'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue NOT IN (''' + ISNULL(@RestrictedDimensionValues,'') + ''')'
		/* End - Added by Arpita Soni for ticket #511 on 11/02/2015 */

	   SET @QueryToRun = @QueryToRun + ' INNER JOIN DimensionValue D' + CAST(@i AS NVARCHAR) + ' ON D' + CAST(@i AS NVARCHAR) + '.id = D2.D' + CAST(@DimensionIndex AS NVARCHAR) 
                                                         + ' AND D'+ CAST(@i  AS NVARCHAR) +'.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR) 

       IF(@IsDateDimension = 1)
       BEGIN
              SET @QueryToRun = @QueryToRun + ' and CAST(d' + CAST(@i  AS NVARCHAR) + '.DisplayValue AS DATE) between '''+cast(@STARTDATE as nvarchar)+''' and '''+cast(@ENDDATE as nvarchar)+''''
			  SET @OrderbyCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId and AxisName='X'and Dimensionid=@Dimensionid );
			  SET @DimensionCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId );
			  --Following code is written to identify order by value
			  if(@OrderbyCount=0 and @DimensionCount>1)
			     SET @OrderByCount=2
				 else
				 SET @OrderByCount=1
       END
	      SET @RestrictedDimensionValues= ''
	    END
		SET @i = @i + 1
       FETCH NEXT FROM Column_Cursor
       INTO @Dimensionid,@IsDateDimension, @DimensionIndex
       END
Close Column_Cursor
Deallocate Column_Cursor
--Filters
	DECLARE @FilterCondition NVARCHAR(MAX);
	SET @FilterCondition = ''
	IF(@FilterValues IS NOT NULL)
	BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	END
	SET @FilterCondition = ISNULL(@FilterCondition,'')
	IF(@FilterCondition != '')
	BEGIN
	SET @FilterCondition=' where'+@FilterCondition
	SET @FilterCondition =  REPLACE(@FilterCondition,'#',' AND ')
	END

	IF(@FilterCondition is null and @ExcludeQuery is not null)
	SET @ExcludeQuery=' where '+@ExcludeQuery
--Exclude
--Deletion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue no need to pass filter and exclude in both dimension and measure table>
--set @QueryToRun=@QueryToRun+'  '+@FilterCondition+@ExcludeQuery
--Deletion End: <17/02/2016> <Kausha> <Ticket #729,#730>

--Insertion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue we have removed filter from dimension table and exculd from measure table>
IF(@IsDimension=1)
set @QueryToRun=@QueryToRun+'  '+@ExcludeQuery
ELSE
set @QueryToRun=@QueryToRun+'  '+@FilterCondition
--Insertion End: <17/02/2016> <Kausha> <Ticket #729,#730>

IF(@IsDimension=1)
set @QueryToRun=@QueryToRun+' order by ordervalue'+CAST(@OrderByCount as nvarchar)

END
RETURN @QueryToRun 
END

GO
--5

IF OBJECT_ID(N'dbo.FindDateDimension', N'FN') IS NOT NULL
    DROP FUNCTION dbo.FindDateDimension ;
GO

CREATE FUNCTION dbo.FindDateDimension (@tablename nvarchar(max), @Dimensions int)
RETURNS INT
AS
BEGIN
DECLARE @DIMENSIONTOCHECK int
DECLARE @dIndex int=1
DECLARE @dNextIndex int=CHARINDEX('d',@TABLENAME,@dIndex+1)

DECLARE @c int =1
DECLARE @found bit =0

while @c<=@Dimensions and @found=0
BEGIN

	SET @DIMENSIONTOCHECK=cast(SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) as int);
	SET @FOUND=(select isnull(computeAllValues,0) from Dimension where id=@DIMENSIONTOCHECK)	

	if @found=0
	BEGIN
		set @dIndex = @dNextIndex;
		set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);
		if @dNextIndex=0
		BEGIN
			SET @dNextIndex=len(@TABLENAME)
		END
		set @c=@c+1
	END

END

if @found=0
BEGIN
	return -1
END

return @c
END

--select * from dimension
Go
--6



IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'FindDeterminant') AND xtype IN (N'FN', N'IF', N'TF'))
BEGIN
    DROP FUNCTION FindDeterminant	

END
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'FindMatrixInverse') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION FindMatrixInverse
GO
IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'MatrixTable')
DROP TYPE MatrixTable;

CREATE TYPE [dbo].[MatrixTable] AS TABLE(
       [x] [int] NULL,
       [y] [int] NULL,
       [Value] [float] NULL
)
Go
CREATE Function [FindDeterminant] (
	-- Add the parameters for the stored procedure here
	@ToFind MatrixTable READONLY)
RETURNS FLOAT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @mSize int = (select max(x) from @ToFind)

	DECLARE @retval float=0
	if @mSize=2
	BEGIN

		return (select r1.Value*r2.Value-r3.Value*r4.Value from @ToFind r1
		inner join @ToFind r2 on r2.x=2 and r2.y=2
		inner join @ToFind r3 on r3.x=2 and r3.y=1
		inner join @ToFind r4 on r4.x=1 and r4.y=2
		where r1.x=1 and r1.y=1);
	END

	DECLARE @x int=1
	DECLARE @y int

	while @x<=@mSize
	BEGIN
		SET @y=1
		DECLARE @subD MatrixTable
		delete from @subD

		insert into @subD
		select case when x < @x then x else x-1 end, case when y < @y then y else y-1 end, value
		from @ToFind
		where x<>@x and y<>@y




		if (@x+@y)%2=0
		BEGIN
			set @retval = @retval + (select dbo.FindDeterminant (@subD))*(select Value from @ToFind where x=@x and y=@y)
		END
		ELSE
		BEGIN
			set @retval = @retval - (select dbo.FindDeterminant (@subD))*(select Value from @ToFind where x=@x and y=@y)
		END

		SET @x=@x+1
	END

	return @retval;

END


GO
CREATE FUNCTION [FindMatrixInverse]
( @ToFind MatrixTable READONLY
)
RETURNS 
@adjugateMatrix table (x int, y int, value float)
AS
BEGIN

	
	insert into @adjugateMatrix
	select * from @ToFind

	DECLARE @Determinant float = (select dbo.FindDeterminant(@toFind))

	DECLARE @i int
	DECLARE @j int
	DECLARE @NumX int =(select max(x) from @adjugateMatrix)

	update @adjugateMatrix set Value=0

	set @i=1
	while @i<=@NumX
	BEGIN
		set @j=1

		while @j<=@NumX
		BEGIN
			DECLARE @ValToAdd float=1

			if (@i+@j)%2=1
			BEGIN
				SET @ValToAdd=-1
			END

			DECLARE @cofactor MatrixTable
			delete from @cofactor

			insert into @cofactor
			select case when x < @i then x else x-1 end, case when y < @j then y else y-1 end, value
			from @ToFind
			where x<>@i and y<>@j

			set @ValToAdd=@ValToAdd*(select dbo.FindDeterminant(@cofactor))



			--print 'update #AdjugateMatrix set value=value - ' + cast(@finalVal as varchar) + ' where x=' + cast(@i as varchar) + ' and y=' + cast(@j as varchar)
			update @adjugateMatrix set value=@ValToAdd where x=@i and y=@j


			set @j=@j+1
		END

		set @i=@i+1
	END

	update @adjugateMatrix set value=value/@Determinant	
	
	RETURN 
END
GO

--7
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetColor') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetColor
GO
-- =============================================
-- Author:		Manoj Limbachiya
-- Create date: 04Dec2014
-- Description:	Return the color code for graph element from color sequence table
-- =============================================
CREATE FUNCTION [GetColor](@SeqColorId INT)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @aa NVARCHAR(4000)
	SET @aa = ''
	
	SELECT @aa = 
		COALESCE (CASE WHEN @aa = ''
					   THEN ColorCode
					   ELSE @aa + ',' + ColorCode
				   END
				  ,'')
	  FROM ColorSequence where SequenceNumber = 
	  CASE WHen 
	  @SeqColorId IS NULL then 1 else @SeqColorId end  ORDER BY DisplayOrder 
	  if(LEN(@aa)<=0)
		SET @aa = '#73c4ee,#5693b3,#2384b8,#1a638a,#b04499,#cc91bf,#333333,#4d4d4d,#666666,#808080,#8cc63f,#39b54a,#009245,#006837,#f0be29,#f6d87f,#ff931e,#f0be29,#c1272d,#ed1c24'
	RETURN @aa;

END


GO
--8
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDatePart') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetDatePart
GO

CREATE FUNCTION [dbo].[GetDatePart](@DateValue nvarchar(1000),@ViewByValue NVARCHAR(10),@STARTDATE DATETIME, @ENDDATE DATETIME)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @ColumnPart NVARCHAR(200)
	SET @ColumnPart = ''
	IF(@ViewByValue = 'Q') 
				BEGIN
					SET @ColumnPart = 'Q' + CAST(DATEPART(Q,CAST(@DateValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue = 'Y') 
				BEGIN
					SET @ColumnPart = CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue = 'M') 
				BEGIN
					SET @ColumnPart = SUBSTRING(DateName(MONTH,CAST(@DateValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue='W')
				BEGIN
					IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
					BEGIN
						SET @ColumnPart=LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR))))
					END
					ELSE 
					BEGIN
						SET @ColumnPart=LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR)))
					END
				END
				ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
				BEGIN
					SET @ColumnPart= [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(@DateValue AS DATETIME))
				END
				ELSE 
				BEGIN
					SET @ColumnPart = @DateValue
				END
	
	RETURN @ColumnPart;

END
                
Go
--9
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasureOutputValue') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasureOutputValue
GO
CREATE FUNCTION [GetMeasureOutputValue]
(
	@OriginalValue decimal(18,2),
	@MeasureId INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @OutputValue NVARCHAR(50);
	SET @OutputValue = '';
	SELECT TOP 1 @Outputvalue=ISNULL(Value,CAST(@OriginalValue AS NVARCHAR)) FROM MeasureOutputValue
	WHERE MeasureId=@MeasureId and @OriginalValue BETWEEN LowerLimit AND UpperLimit
	IF(@OutputValue='')
		SET @OutputValue = CAST(@OriginalValue AS NVARCHAR)

	RETURN @OutputValue
END

GO
--10
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasuresForMeasureTable') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasuresForMeasureTable
GO
CREATE FUNCTION [GetMeasuresForMeasureTable] 
(   @ReportGraphID int,
    @MeasureId int
) 
RETURNS  nvarchar(1000)
BEGIN
declare @MeasureName nvarchar(100)
declare @AggrigationType nvarchar(100)
declare @Column nvarchar(max)
declare @DimensionTableMeasure nvarchar(max)
declare @SymbolType nvarchar(max)
declare @Count int =1
     

  set @Column=''
			set @Column=',SUM(d1.Rows) as Rows'
 DECLARE @MeasureCursor CURSOR
			SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT Top 1 m.name,m.AggregationType,rc.SymbolType from reportgraphcolumn rc inner join measure m on    rc.reportgraphid=@ReportGraphId and m.id=@MeasureId
			OPEN @MeasureCursor
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			WHILE @@FETCH_STATUS = 0 
			BEGIN
			if(@AggrigationType='AVG')
			BEGIN
			IF(@SymbolType = '%'  )
			BEGIN
			set @Column=@Column+','+' ISNULL(ROUND((sum(D1.Value*d1.rows)/sum(d1.rows))*100,2),0) AS  ['+@MeasureName+']'
			
			END
			ELSE
			BEGIN
				set @Column=@Column+','+ 'ISNULL(ROUND(sum(D1.Value*d1.rows)/sum(d1.rows),2),0) AS ['+@MeasureName+']'
			END
			END
			ELSE
			set @Column=@Column+','+'ISNULL(ROUND(SUM(d1.Value),2),0) as ['+@MeasureName+']'
			--Add Rows
			set @Count=@count+1
			
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			END
			CLOSE @MeasureCursor
			DEALLOCATE @MeasureCursor
			return @Column
END

--ENDGetMesure for Measure Table

GO
--11
IF EXISTS (SELECT * FROM sys.objects WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[GetPositionBasedRevenue]') AND TYPE IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetPositionBasedRevenue]
GO

CREATE FUNCTION [dbo].[GetPositionBasedRevenue]
(
	@ActivityDate DATETIME = '01-09-2015',
	@ActualRevenue FLOAT = 24000,
	@Index INT = 1,
	@DimensionId INT = 14, 
	@DimensionValue NVARCHAR(MAX) = 'Marketing', 
	@MinValue INT = 1,
	@MaxValue INT = 5,
	@AttributionType INT = 5, -- @AttributionType = 5 - Position, 7 - W Shaped
	@OppFirstTouchDate DATETIME,
	@OppLastTouchDate DATETIME
)

RETURNS FLOAT
AS
BEGIN
	DECLARE @XValue FLOAT, 
			@YValue FLOAT,
			@EvenlyDistValue FLOAT,
			@CalculatedRevenue FLOAT
			
	-- Two or more configuration are there for a single touch/ All touches for an opportunity should satisfy the same configuration
	IF NOT EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate ) OR
				 (SELECT COUNT(*) FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate ) > 2 OR
				  (SELECT COUNT(*) FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate AND DimensionId = @DimensionId) > 1
	BEGIN
		-- Case 3: Default case
		SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
		WHERE ISNULL(StartDate,'') = '' AND ISNULL(EndDate,'') = '' 
		AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''
	END 
	ELSE
	BEGIN
		-- Case 1: Check DimensionId, DimensionValue, ActivityDate
		SELECT @XValue = XValue * 100 , @YValue = YValue * 100 FROM AttrPositionConfig 
		WHERE ISNULL(@ActivityDate,'') BETWEEN StartDate AND EndDate 
		AND ISNULL(DimensionId,0) = @DimensionId AND ISNULL(DimensionValue,'') = @DimensionValue
	
		IF(@XValue IS NULL AND @YValue IS NULL)
		BEGIN
			-- Case 2: Check ActivityDate
			SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
			WHERE ISNULL(@ActivityDate,'') BETWEEN StartDate AND EndDate 
			AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''

			IF(@XValue IS NULL AND @YValue IS NULL)
			BEGIN
				-- Case 3: Default case
				SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
				WHERE ISNULL(StartDate,'') = '' AND ISNULL(EndDate,'') = '' 
				AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''
			END
		END
	END
	IF((@XValue + @YValue) > 100)
	BEGIN
		RETURN NULL;
	END

	-- Calculation for W-Shaped attribution
	IF (@AttributionType = 7)
	BEGIN
		DECLARE @TempSumXY FLOAT =  @XValue + @YValue 
		IF(@TempSumXY > 0)
		BEGIN
			SET @XValue = ( @XValue * 100 ) / @TempSumXY
			SET @YValue = ( @YValue * 100 ) / @TempSumXY
		END
		SET @EvenlyDistValue = 0
	END
	ELSE
	BEGIN
		IF(@MaxValue > 2)
		BEGIN
			SET @EvenlyDistValue = (100 - @XValue - @YValue) / (@MaxValue - 2)
		END
		ELSE IF(@MaxValue = 2)
		BEGIN
			SET @EvenlyDistValue = (100 - @XValue - @YValue) / @MaxValue
			SET @XValue = @XValue + @EvenlyDistValue 
			SET @YValue = @YValue + @EvenlyDistValue 
		END
		ELSE
		BEGIN
			SET @XValue = 100
		END
	END

	IF(@XValue IS NOT NULL AND @YValue IS NOT NULL)
	BEGIN
		SET @CalculatedRevenue = (SELECT CASE WHEN @index = @MinValue THEN (@ActualRevenue * @XValue) / 100 
								 WHEN @index = @MaxValue THEN (@ActualRevenue * @YValue) / 100
								 ELSE (@ActualRevenue * @EvenlyDistValue) / 100 END)
	END
	RETURN @CalculatedRevenue
END
GO
--12
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestQueryForAll') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetTTestQueryForAll
GO
CREATE FUNCTION [GetTTestQueryForAll]
(
	@Dimension1 NVARCHAR(500),
	@Dimension2 NVARCHAR(500),
	@ViewByValue VARCHAR(10),
	@STARTDATE DATE='1-1-1900', 
	@ENDDATE DATE='1-1-2100', 
	@ReportGraphID INT, 
	@DIMENSIONTABLENAME NVARCHAR(100), 
	@DATEFIELD NVARCHAR(100)=NULL, 
	@FilterValues NVARCHAR(MAX)=NULL,
	@AxisCount INT
)
RETURNS NVARCHAR(4000)
AS
BEGIN
	
	DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX);
	DECLARE @TTestQuery1 NVARCHAR(MAX), @TTestQuery2 NVARCHAR(MAX);
	
	SET @InnerJoin1 = ''; SET @Count= 1; SET @TTestQuery2 = '';

	DECLARE @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere1 NVARCHAR(1000),@HeatMapWhere2 NVARCHAR(1000),@OnlyDateDimQuery NVARCHAR(MAX)
	SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = ''; SET @HeatMapWhere1 = ''; SET @HeatMapWhere2 = '';
	SET @OnlyDateDimQuery = 'SELECT #COLUMNS# FROM MeasureValue d1 INNER JOIN DimensionValue d2 ON d1.DimensionValue=d2.id AND ';

	SET @TTestQuery1 = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1';
	SET @TTestQuery1 =  @TTestQuery1 + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 

	DECLARE @DimensionNames TABLE(DimensionID INT,CountIndex INT);
	INSERT INTO @DimensionNames
	SELECT dimension, cnt FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D')

	DECLARE @IsOnlyDateDimension INT,@Select2ColumnIndex INT,@Select1ColumnIndex INT,@FinalTTestQuery NVARCHAR(MAX),@DimensionValue1 NVARCHAR(100),@DimensionValue2 NVARCHAR(100);
	SET @IsOnlyDateDimension=0;
	SET @DimensionValue1 = ''; 
	SET @DimensionValue2 = ''; 
	
	--identify about dimensions
	IF(@AxisCount = 2)
	BEGIN
		SET @IsOnlyDateDimension = 0;
	END
	ELSE 
	BEGIN
		DECLARE @datecount INT
		SELECT @datecount = COUNT(*) FROM ReportAxis INNER JOIN Dimension ON Dimension.id = ReportAxis.Dimensionid and Dimension.IsDateDimension = 1 where ReportGraphId = @ReportGraphID 
		IF(ISNULL(@datecount,0) = 1)
		BEGIN
			SET @IsOnlyDateDimension = 1;
		END
		ELSE
		BEGIN	
			SET @IsOnlyDateDimension = 0;
		END
	END

	DECLARE Column_Cursor CURSOR FOR
	SELECT DN.DimensionID AS Dimensionid, '[' + Replace(D.Name,' ','') + ']' DimensionName, D.IsDateDimension FROM @DimensionNames DN
	INNER JOIN Dimension D ON DN.DimensionID = D.id  
	OPEN Column_Cursor 
	FETCH NEXT FROM Column_Cursor 
	INTO @Dimensionid, @DimensionName,@IsDateDimension 
		WHILE @@FETCH_STATUS = 0
			BEGIN
				
				DECLARE @tempIndex INT, @sbString NVARCHAR(100), @CurrentDimension NVARCHAR(100);
				SET @tempIndex = 0;
				SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
				SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
				IF(@Dimensionid = SUBSTRING(@Dimension1,2,CHARINDEX(':',@Dimension1)-2))
						SET @CurrentDimension = @Dimension1
				ELSE IF(@Dimensionid = SUBSTRING(@Dimension2,2,CHARINDEX(':',@Dimension2)-2))
						SET @CurrentDimension = @Dimension2
				ELSE 
						SET @CurrentDimension = NULL
				
				IF(@IsOnlyDateDimension=1)
				BEGIN
					
					DECLARE @Dates1 NVARCHAR(100),@Dates2 NVARCHAR(100);
					SET @Dates1 = [dbo].[CalculateStartAndEndDate](REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':',''),@ViewByValue,@STARTDATE,@ENDDATE);
					SET @Dates2 = [dbo].[CalculateStartAndEndDate](REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':',''),@ViewByValue,@STARTDATE,@ENDDATE);
					
					SET @STARTDATE = SUBSTRING(@Dates1,1,CHARINDEX(',',@Dates1)-1)
					SET @ENDDATE = SUBSTRING(@Dates1,CHARINDEX(',',@Dates1)+1,LEN(@Dates1)-CHARINDEX(',',@Dates1))
					SET @InnerJoin1 = ISNULL(@OnlyDateDimQuery,'')  + ' d2.Dimensionid = '+ CAST(@Dimensionid AS NVARCHAR) + ' AND CAST(d2.DisplayValue AS DATE) BETWEEN ''' +CAST(@STARTDATE AS NVARCHAR) +''' AND '''+CAST(@ENDDATE AS NVARCHAR) +''''
					
					SET @STARTDATE = SUBSTRING(@Dates2,1,CHARINDEX(',',@Dates2)-1)
					SET @ENDDATE = SUBSTRING(@Dates2,CHARINDEX(',',@Dates2)+1,LEN(@Dates2)-CHARINDEX(',',@Dates2))
					SET @TTestQuery2 = ISNULL(@OnlyDateDimQuery,'')  + ' d2.Dimensionid = '+ CAST(@Dimensionid AS NVARCHAR) + ' AND CAST(d2.DisplayValue AS DATE) BETWEEN ''' +CAST(@STARTDATE AS NVARCHAR) +''' AND '''+CAST(@ENDDATE AS NVARCHAR) +''''

					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 
											+' d1.value '
				END
				ELSE IF(@IsDateDimension=1)
				BEGIN
					SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
					SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  
					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 
											+' d1.value '
				END
				ELSE 
				BEGIN
					IF(CHARINDEX('D'+CAST(@Dimensionid AS NVARCHAR)+':',@Dimension1)=1)
					BEGIN
						SET @Select1ColumnIndex = @Count;
						SET @DimensionValue1 = REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':','');
						SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + 
											  CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d#FIRSTINDEX# AND d'+CAST(@Count + 2 AS NVARCHAR) 
						SET @InnerJoin1 = @InnerJoin1 + '.DisplayValue = ''#NAME1#'''
					END
					IF(CHARINDEX('D'+CAST(@Dimensionid AS NVARCHAR)+':',@Dimension2)=1)
					BEGIN
						SET @DimensionValue2 = REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':','');
						SET @Select2ColumnIndex = @Count;
					END
					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + ' d1.value '
				END
				SET @Count = @Count + 1
		FETCH NEXT FROM Column_Cursor
		INTO @Dimensionid, @DimensionName,@IsDateDimension
		END
	Close Column_Cursor
	Deallocate Column_Cursor

	SET @HeatMapGrpuoColumn = @HeatMapDimColumn

	DECLARE @AggregationType NVARCHAR(20), @MEASURENAME NVARCHAR(200),@SymbolType NVARCHAR(100), @MEASUREID INT, @MEASURETABLENAME NVARCHAR(200), @MeCount INT;
	SET @MeCount =1; 
	SET @SymbolType = '';

	DECLARE Measure_Cursor CURSOR FOR
	select MeasureName, Measureid, MeasureTableName,ISNULL(AggregationType,'') AggregationType,ISNULL(SymbolType,'') SymbolType from dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue) ORDER BY ColumnOrder
	OPEN Measure_Cursor
	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
	WHILE @@FETCH_STATUS = 0
		BEGIN
		
			DECLARE @AggregartedMeasure NVARCHAR(200)

			IF(@MeCount = 1)
			BEGIN
					SET @AggregartedMeasure = 'a.value'
					SET @HeatMapDimColumn = @HeatMapDimColumn + ' ,' + @AggregartedMeasure --+ ','
					SET @HeatMapWhere1 = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
			END
			ELSE
			BEGIN
					SET @HeatMapWhere2 = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
			END
		
			SET @Count = @Count + 1
			SET @MeCount = @MeCount + 1
		FETCH NEXT FROM Measure_Cursor
		INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
		END
	CLOSE Measure_Cursor
	DEALLOCATE Measure_Cursor

	IF(@IsOnlyDateDimension = 1)
	BEGIN
		SET @InnerJoin1 = REPLACE(@InnerJoin1,'#COLUMNS#',@HeatMapDimColumn)
		SET @TTestQuery1 = ISNULL(@InnerJoin1,'')  --+ ' Group By ' + @HeatMapGrpuoColumn
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d2.id, d1.value ')
		SET @FinalTTestQuery = ISNULL(@TTestQuery1,'') +' FULL OUTER JOIN ( '+ISNULL(@TTestQuery2,'') +' '+ISNULL(@HeatMapWhere1,'')+' ) a ON a.id=d2.id' + ISNULL(@HeatMapWhere1,'')
	END
	ELSE
	BEGIN
		SET @TTestQuery2 = ' FULL OUTER JOIN ( ';
		
		IF(@MeCount=2)
		BEGIN
			SET @TTestQuery2 = ISNULL(@TTestQuery2,'') + ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'') + ISNULL(@HeatMapWhere1,'') 
			SET @TTestQuery2 =  ISNULL(@TTestQuery2,'') + ' ) a ON a.d1=d2.d1 ' 
			SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d2.d1, d1.value ')
		END
		ELSE
		BEGIN
			--For correlation heat maps
			SET @TTestQuery2 = ISNULL(@TTestQuery2,'') + ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'') + ISNULL(@HeatMapWhere2,'') 
			SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d1.value, d1.'+@DIMENSIONTABLENAME)
			SET @TTestQuery2 =  ISNULL(@TTestQuery2,'') + ' ) a ON a.'+@DIMENSIONTABLENAME+'=d1.'+@DIMENSIONTABLENAME+ ' ' 
		END
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#NAME1#','#NAME2#')
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#FIRSTINDEX#','#SECONDINDEX#')
		SET @TTestQuery1 = ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'');
		
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#COLUMNS#',@HeatMapDimColumn) 
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#NAME1#',@DimensionValue1)
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#FIRSTINDEX#',@Select1ColumnIndex)
		
		
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#NAME2#',@DimensionValue2)
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#SECONDINDEX#',@Select2ColumnIndex)

		SET @FinalTTestQuery = ISNULL(@TTestQuery1,'') +  ISNULL(@TTestQuery2,'') + ISNULL(@HeatMapWhere1,'') 
		
	END

	--Added by kausha somaiya for filter condition and For Exclude column
	DECLARE @FilterCondition NVARCHAR(MAX);
	SET @FilterCondition = ''
	IF(@FilterValues IS NOT NULL)
	BEGIN
		SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
		set @FinalTTestQuery=@FinalTTestQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
	END
	RETURN @FinalTTestQuery;
END

GO
--13
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestQueryForCoRelation') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetTTestQueryForCoRelation
GO
CREATE FUNCTION [GetTTestQueryForCoRelation] 
(
@Dimension1 nvarchar(500),
@Dimension2 nvarchar(500),
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,
@SubDashboardMainDimensionTable int = 0	
)
RETURNS NVARCHAR(4000)
AS
BEGIN
	
DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX)
SET @InnerJoin1 = ''; SET @Count= 1

DECLARE @HeatMapQuery NVARCHAR(MAX), @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere NVARCHAR(1000),@HeatMapWhere2 NVARCHAR(1000)
SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = '';SET @HeatMapWhere = ''
SET @HeatMapQuery = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1'
SET @HeatMapQuery =  @HeatMapQuery + ' FULL OUTER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 
SET @HeatMapWhere2 =''

DECLARE Column_Cursor CURSOR FOR
SELECT A.Dimensionid, A.AxisName, '[' + Replace(D.Name,' ','') + ']' DimensionName,D.IsDateDimension FROM ReportGraph G
INNER JOIN ReportAxis A ON G.id = A.ReportGraphId
INNER JOIN Dimension D ON A.Dimensionid = D.id  
WHERE G.id = @ReportGraphID
ORDER BY A.AxisName
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			IF ((@SubDashboardOtherDimensionTable > 0) AND (@SubDashboardMainDimensionTable = @Dimensionid))
			BEGIN
				SET @Dimensionid = @SubDashboardOtherDimensionTable
			END
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			IF(@DATEFIELD = 'D'  + CAST(@tempIndex AS NVARCHAR))
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
				SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR)  
				
				IF(@Dimensionid=SUBSTRING(@Dimension1,2,CHARINDEX(':',@Dimension1)-2))
					SET @HeatMapWhere2 = @HeatMapWhere2 + ' AND d' + CAST(@Count + 2 AS NVARCHAR)+ '.DisplayValue=''' +REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':','') +''''
				IF (@Dimensionid=SUBSTRING(@Dimension2,2,CHARINDEX(':',@Dimension2)-2))
					SET @HeatMapWhere2 = @HeatMapWhere2 + ' AND d' + CAST(@Count + 2 AS NVARCHAR)+ '.DisplayValue=''' +REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':','') +''''
			END
					
			
			SET @Count = @Count + 1

	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	END
Close Column_Cursor
Deallocate Column_Cursor

SET @HeatMapGrpuoColumn = @HeatMapDimColumn

DECLARE @AggregationType NVARCHAR(20), @MEASURENAME NVARCHAR(200),@SymbolType NVARCHAR(100), @MEASUREID INT, @MEASURETABLENAME NVARCHAR(200), @MeCount INT;
SET @MeCount =1; 
SET @SymbolType = '';

DECLARE Measure_Cursor CURSOR FOR
select MeasureName, Measureid, MeasureTableName,ISNULL(AggregationType,'') AggregationType,ISNULL(SymbolType,'') SymbolType from dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue) ORDER BY ColumnOrder
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
WHILE @@FETCH_STATUS = 0
	Begin
		
		DECLARE @AggregartedMeasure NVARCHAR(200)

		IF(@MeCount = 1)
		BEGIN
				SET @AggregartedMeasure = 'd1.value'
				SET @HeatMapDimColumn = @HeatMapDimColumn + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END  + @AggregartedMeasure + ','
				SET @HeatMapWhere = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		ELSE
		BEGIN
				SET @AggregartedMeasure = ' d' + CAST(@Count + 1 AS NVARCHAR) + '.Value '
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN ' +@DIMENSIONTABLENAME + 'Value d' + CAST(@Count + 1 AS NVARCHAR) + ' on d1.' + @DIMENSIONTABLENAME + ' = d' + CAST(@Count + 1 AS NVARCHAR) + '.'   + + @DIMENSIONTABLENAME
				SET @HeatMapDimColumn = @HeatMapDimColumn  + @AggregartedMeasure 

				SET @HeatMapWhere = @HeatMapWhere + ' AND d' + CAST(@Count + 1 AS NVARCHAR) + '.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		
		SET @Count = @Count + 1
		SET @MeCount = @MeCount + 1
	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor

SET @HeatMapQuery = REPLACE(@HeatMapQuery,'#COLUMNS#',@HeatMapDimColumn)
SET @HeatMapQuery = @HeatMapQuery + @InnerJoin1 + @HeatMapWhere + @HeatMapWhere2 --+ ' Group By ' + @HeatMapGrpuoColumn

DECLARE @CreateTable NVARCHAR(MAX)
SET @CreateTable = N'DECLARE @REPORTGRAPHHM' + CAST(@ReportGraphID AS NVARCHAR) + '  TABLE (d1 int, d2 int, val float);';

DECLARE @Retval table(d1 int, d2 int, val float) 

--Added by kausha somaiya for filter condition and For Exclude column
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	SET @HeatMapQuery=@HeatMapQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
END
--End Filtercondition

RETURN @HeatMapQuery;

END

GO
--14

IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetWeight]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetWeight]
GO

CREATE FUNCTION [dbo].[GetWeight]
(
	@ActivityDate datetime,
	@DimensionId int,
	@DimensionValue nvarchar(max)
)
 
RETURNS FLOAT
AS
BEGIN

	DECLARE @TouchWeight FLOAT

	-- Case 1: Check DimensionId, DimensionValue, ActivityDate
	IF((SELECT COUNT(*) FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL AND DimensionId IS NOT NULL AND DimensionValue IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId = @DimensionId AND DimensionValue = @DimensionValue)) > 0)
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL AND DimensionId IS NOT NULL AND DimensionValue IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId = @DimensionId AND DimensionValue = @DimensionValue)
	END
	-- Case 2: Check ActivityDate
	ELSE if((SELECT COUNT(*) FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId IS NULL AND DimensionValue IS NULL)) > 0)
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId IS NULL AND DimensionValue IS NULL)
	END
	-- Case 3: Default case
	ELSE
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE StartDate IS NULL AND EndDate IS NULL AND DimensionId IS NULL AND DimensionValue IS NULL
	END
	RETURN @TouchWeight
END
--ENd GetWeight

Go
--15
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'normsdist') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION normsdist
GO

CREATE Function [normsdist] (@x float)
returns float
as
BEGIN
	--This function approximates what is returned by the normsdist function in Excel.  It will not be exactly the same, but close enough for our purposes.
	if @x < -10
		BEGIN
		return 0
		END

	if @x > 10
		BEGIN
		return 1
		END


	--Using the 1945 Polya approximation formula as it is good enough for our purposes
	if @X < 0
		BEGIN
		RETURN 1- 0.5*(1+sqrt((1-exp(-1*sqrt(pi()/8)*power(@x,2)))))
		END

	return 0.5*(1+sqrt((1-exp(-1*sqrt(pi()/8)*power(@x,2)))))
END


GO
--16
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'normsinv') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION normsinv
GO

CREATE FUNCTION [normsinv](@p FLOAT)
RETURNS FLOAT
AS
BEGIN
DECLARE @a1 FLOAT = -39.6968302866538
DECLARE @a2 FLOAT = 220.946098424521
DECLARE @a3 FLOAT = -275.928510446969
DECLARE @a4 FLOAT = 138.357751867269
DECLARE @a5 FLOAT = -30.6647980661472
DECLARE @a6 FLOAT = 2.50662827745924
DECLARE @b1 FLOAT = -54.4760987982241
DECLARE @b2 FLOAT = 161.585836858041
DECLARE @b3 FLOAT = -155.698979859887
DECLARE @b4 FLOAT = 66.8013118877197
DECLARE @b5 FLOAT = -13.2806815528857
DECLARE @c1 FLOAT = -0.00778489400243029
DECLARE @c2 FLOAT = -0.322396458041136
DECLARE @c3 FLOAT = -2.40075827716184
DECLARE @c4 FLOAT = -2.54973253934373
DECLARE @c5 FLOAT = 4.37466414146497
DECLARE @c6 FLOAT = 2.93816398269878
DECLARE @d1 FLOAT = 0.00778469570904146
DECLARE @d2 FLOAT = 0.32246712907004
DECLARE @d3 FLOAT = 2.445134137143
DECLARE @d4 FLOAT = 3.75440866190742
DECLARE @plow FLOAT = 0.02425
DECLARE @phigh FLOAT = 1-@plow
DECLARE @q FLOAT
DECLARE @r FLOAT
DECLARE @result FLOAT
IF (@p<@plow)
BEGIN
SET @q = Sqrt(-2 * LOG(@p))
SET @result=(((((@c1 * @q + @c2) * @q + @c3) * @q + @c4) * @q + @c5) * @q + @c6) / ((((@d1 * @q + @d2) * @q + @d3) * @q + @d4) * @q + 1)
END
ELSE
BEGIN
IF (@p<@phigh)
BEGIN
SET @q =@p - 0.5
SET @r = @q * @q
SET @result= (((((@a1 * @r + @a2) * @r + @a3) * @r + @a4) * @r + @a5) * @r + @a6) * @q / (((((@b1 * @r + @b2) * @r + @b3) * @r + @b4) * @r + @b5) * @r + 1)
END
ELSE
BEGIN
SET @q = SQRT(-2 * LOG(1 - @p))
SET @result= -(((((@c1 * @q + @c2) * @q + @c3) * @q + @c4) * @q + @c5) * @q + @c6) / ((((@d1 * @q + @d2) * @q + @d3) * @q + @d4) * @q + 1)
END
END
RETURN @result
END

GO
--17
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RateProb') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION RateProb
GO


CREATE Function [RateProb] (@PopulationRate float, @NumberTests float, @TestRate float)
returns float
as
BEGIN
	--Function will return the probability that the number of tests would be at least the number passed in.  Returns null in case of degenerate data
	if @PopulationRate <= 0 or @PopulationRate >=1
	BEGIN
		return null
	END
	DECLARE @SD float = sqrt( @NumberTests*@PopulationRate*(1-@PopulationRate))
	DECLARE @Z float = (@NumberTests*@TestRate-@NumberTests*@PopulationRate)/@SD

	return dbo.normsdist(@Z)

END

GO
--18
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RestrictedDimensionValues') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION RestrictedDimensionValues
GO


CREATE FUNCTION [RestrictedDimensionValues] 
(
	@DimensionId INT,
	@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
	@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
)
RETURNS NVARCHAR(MAX) AS
BEGIN
		-- Restrict dimension values as per UserId and RoleId
		DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
		IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ',' ,'') + DimensionValue 
			--SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
		END
		ELSE 
		BEGIN
			--SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ',' ,'') + DimensionValue 
			FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
		END
	--SET @RestrictedDimensionValues = ''''+ @RestrictedDimensionValues+''''
	RETURN @RestrictedDimensionValues

END

GO
---Table Value Function
--1
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ExtractTableFromXml') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION ExtractTableFromXml
GO
CREATE FUNCTION [dbo].[ExtractTableFromXml] (@XmlString XML)
RETURNS @FilterString TABLE (
			ID NVARCHAR(MAX),
			DimensionValueID NVARCHAR(MAX)
		)
AS
BEGIN
	
	--DECLARE @FilterValues NVARCHAR(MAX)=(@XmlString)
	--DECLARE @XmlString XML
		--SET @XmlString = @FilterValues;
  		DECLARE @TempValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)
		DECLARE @FilterValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)

 
		;WITH MyData AS (
			SELECT data.col.value('(@ID)[1]', 'int') Number
				,data.col.value('(.)', 'INT') DimensionValueId
			FROM @XmlString.nodes('(/filters/filter)') AS data(col)
		)
		Insert into @TempValueTable 
		SELECT 
			 D.id,
			 DV.Id
		FROM MyData 
		INNER JOIN DimensionValue DV ON DV.Id = MyData.DimensionValueId
		INNER JOIN Dimension D ON D.Id = DV.DimensionId

		INSERT INTO @FilterString SElect * from @TempValueTable
	RETURN
END
Go
--2
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ExtractValueFromXML') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION ExtractValueFromXML
GO



-- =============================================
-- Author:		Manoj
-- Description:	Extract value from XML and make string for condition
-- =============================================
CREATE FUNCTION [ExtractValueFromXML] (@XmlString XML, @TableAlias NVARCHAR(10), @IsGraph INT)
RETURNS @FilterString TABLE (
			KeyValue NVARCHAR(MAX)
		)
AS
BEGIN
	
	--Temporary table will store the Id and values passed 
	DECLARE @temp_Table TABLE (
			ID NVARCHAR(10),
			Value NVARCHAR(1000)
		)

	INSERT INTO @temp_Table
	SELECT data.col.value('(@ID)[1]', 'int')
		,data.col.value('(.)', 'INT')
	FROM @XmlString.nodes('(/filters/filter)') AS data(col);

	--Based on the Id, combines value comma saperated
	WITH ExtractValue AS(
		SELECT ID, Value = STUFF((
				SELECT ', ' + Value
				FROM @temp_Table b
				WHERE b.ID = a.ID
				FOR XML PATH('')
				), 1, 2, '')
		FROM @temp_Table a
		GROUP BY ID
	),
	_AllFilters AS(
		SELECT 1 Id,  
		Condition = CASE 
					WHEN @IsGraph = 0 THEN  'AND '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					WHEN @IsGraph = 2 THEN  ' '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					ELSE ' left outer join DimensionValue d' + CONVERT(NVARCHAR(20),(ID + 4)) + ' on ' +  @TableAlias + '.Id = d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id and d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id in (' + Value  +')'
		END 
		FROM ExtractValue
	)
	INSERT @FilterString (KeyValue)
	SELECT DISTINCT 
    STUFF	(
				(SELECT CASE WHEN @IsGraph = 2 THEN '#' ELSE ',' END + B.Condition FROM _AllFilters B WHERE B.Id = A.Id FOR XML PATH(''))
	,1,1,''	) Condition
	FROM
    _AllFilters A;
	RETURN
END


GO
--3
/*  Insertion Start:11/04/2016 Kausha #759  Added Following function to get week list  */
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE   object_id = OBJECT_ID(N'fGetWeeksList') AND type IN (  N'FN', N'IF', N'TF', N'FS', N'FT') ) 
BEGIN
	DROP function [fGetWeeksList]
END
GO
CREATE FUNCTION [dbo].[fGetWeeksList]
(
    @StartDate DATETIME 
   ,@EndDate DATETIME 
)
RETURNS 
TABLE 
AS
RETURN
(

SELECT DATEADD(DAY,-(DATEPART(DW,DATEADD(WEEK, x.number, @StartDate))-1),DATEADD(WEEK, x.number, @StartDate)) as [StartDate]
      ,DATEADD(DAY,-(DATEPART(DW,DATEADD(WEEK, x.number + 1, @StartDate))-1) ,DATEADD(WEEK, x.number + 1, @StartDate)) AS [EndDate]
FROM master.dbo.spt_values x
WHERE x.type = 'P' AND x.number <= DATEDIFF(WEEK, @StartDate, DATEADD(WEEK,0,CAST(@EndDate AS DATE))))
/*  Insertion End:11/04/2016 Kausha #759  Added Following function to get week list  */
--Please do not add any script below this line
Go
--4
IF OBJECT_ID(N'dbo.FindDimensionsForTable', N'TF') IS NOT NULL
    DROP FUNCTION dbo.FindDimensionsForTable ;
GO

CREATE FUNCTION dbo.FindDimensionsForTable (@tablename nvarchar(max), @Dimensions int)
RETURNS @retTable TABLE
(
	id int identity(1,1),
	DimensionValue int
)
AS
BEGIN
DECLARE @DIMENSIONTOCHECK int
DECLARE @dIndex int=1
DECLARE @dNextIndex int=CHARINDEX('d',@TABLENAME,@dIndex+1)

DECLARE @c int =1

while @c<=@Dimensions 
BEGIN

	SET @DIMENSIONTOCHECK=cast(SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) as int);

	insert into @retTable
	select @DIMENSIONTOCHECK

	set @dIndex = @dNextIndex;
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);
	--Added for #711
	if @dNextIndex=0
	BEGIN
		SET @dNextIndex=len(@TABLENAME)+1
	END
	set @c=@c+1
END

return 
END

GO
--5
--IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'FindMatrixInverse') AND xtype IN (N'FN', N'IF', N'TF'))
--    DROP FUNCTION FindMatrixInverse
--GO
--CREATE FUNCTION [FindMatrixInverse]
--( @ToFind MatrixTable READONLY
--)
--RETURNS 
--@adjugateMatrix table (x int, y int, value float)
--AS
--BEGIN

	
--	insert into @adjugateMatrix
--	select * from @ToFind

--	DECLARE @Determinant float = (select dbo.FindDeterminant(@toFind))

--	DECLARE @i int
--	DECLARE @j int
--	DECLARE @NumX int =(select max(x) from @adjugateMatrix)

--	update @adjugateMatrix set Value=0

--	set @i=1
--	while @i<=@NumX
--	BEGIN
--		set @j=1

--		while @j<=@NumX
--		BEGIN
--			DECLARE @ValToAdd float=1

--			if (@i+@j)%2=1
--			BEGIN
--				SET @ValToAdd=-1
--			END

--			DECLARE @cofactor MatrixTable
--			delete from @cofactor

--			insert into @cofactor
--			select case when x < @i then x else x-1 end, case when y < @j then y else y-1 end, value
--			from @ToFind
--			where x<>@i and y<>@j

--			set @ValToAdd=@ValToAdd*(select dbo.FindDeterminant(@cofactor))



--			--print 'update #AdjugateMatrix set value=value - ' + cast(@finalVal as varchar) + ' where x=' + cast(@i as varchar) + ' and y=' + cast(@j as varchar)
--			update @adjugateMatrix set value=@ValToAdd where x=@i and y=@j


--			set @j=@j+1
--		END

--		set @i=@i+1
--	END

--	update @adjugateMatrix set value=value/@Determinant	
	
--	RETURN 
--END


GO
--6
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'fnSplitString') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION fnSplitString
GO
GO
CREATE FUNCTION [fnSplitString] 
( 
    @string NVARCHAR(MAX), 
    @delimiter CHAR(1) 
) 
RETURNS @output TABLE(dimension NVARCHAR(MAX),cnt int 
) 
BEGIN 
    DECLARE @start INT, @end INT ,@index INT 
    SELECT @start = 1, @end = CHARINDEX(@delimiter, @string) ,@index=0
    WHILE @start < LEN(@string) + 1 BEGIN 
        IF @end = 0  
            SET @end = LEN(@string) + 1
       IF(SUBSTRING(@string, @start, @end - @start)!='')
	   BEGIN
	 
        INSERT INTO @output (dimension,cnt)  
        VALUES(SUBSTRING(@string, @start, @end - @start),@index) 
		END
		  set @index  =@index + 1
        SET @start = @end + 1 
        SET @end = CHARINDEX(@delimiter, @string, @start)
        
    END 
    RETURN 
END

GO
--7
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDimensions') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetDimensions
GO

CREATE FUNCTION [dbo].[GetDimensions] 
(   @ReportGraphID INT, 
    @ViewByValue NVARCHAR(10),
    @DIMENSIONTABLENAME NVARCHAR(100)='d46D47', 
	@STARTDATE DATE='01/01/2014', 
    @ENDDATE DATE='12/31/2014',
	@GType nvarchar(20),
	@SubDashboardOtherDimensionTable int,
	@SubDashboardMainDimensionTable int,
	@DisplayStatSignificance NVARCHAR(15) = NULL -- Added by Arpita Soni on 08/19/2015 for PL ticket #427
) 
RETURNS @DimensionTable TABLE(SelectDimension NVARCHAR(2000),
CreateTableColumn nvarchar(2000),--2
MeasureTableColumn nvarchar(2000),--3
MeasureSelectColumn nvarchar(2000),--4
UpdateCondition nvarchar(2000),--5
GroupBy nvarchar(2000),--6
Totaldimensioncount nvarchar(2000),--7
DimensionName1 nvarchar(50),--8
DimensionName2 nvarchar(50),--9
DimensionId1 nvarchar(10),--10
DimensionId2 nvarchar(10),--11
IsDateDImensionExist nvarchar(10),--12
IsDateOnYAxis nvarchar(10),--13
IsOnlyDateDImensionExist nvarchar(10),--14


cnt int) 
BEGIN
	--Start Get the column names as per configured in Report Axis
	DECLARE @DimensionName NVARCHAR(100),@Column NVARCHAR(MAX),@CreateTableColumn NVARCHAR(MAX),@MeasureTableColumn NVARCHAR(MAX),@MeasureSelectColumn NVARCHAR(MAX),@UpdateCondition NVARCHAR(MAX),@GroupBy NVARCHAR(500),@IsDateDimension BIT,@DimensionIndex NVARCHAR(10),@DimensionId INT,@Count INT,@IsDateDImensionExist bit=0,@IsDateOnYAxis bit=0,@DimensionName1 Nvarchar(50),@DimensionName2 Nvarchar(50),@DimensionId1 INT,@DimensionId2 INT,@DateDImensionId INT,@IsOnlyDateDImensionExist bit=0
	SET @Column='';SET @CreateTableColumn=''; set @MeasureTableColumn='';set @MeasureSelectColumn=''; SET @UpdateCondition=''; SET @GroupBy='';SET @Count = 1
	 
	DECLARE @DimensionCursor CURSOR
	SET @DimensionCursor = CURSOR FAST_FORWARD FOR SELECT D.id, '[' + Replace(D.Name,' ','') + ']',D.IsDateDimension ,'D' + CAST(D1.cnt + 2 AS NVARCHAR) 
												    FROM ReportAxis rx 
													INNER JOIN Dimension d ON rx.Dimensionid=d.id
													LEFT JOIN dbo.fnSplitString(@DIMENSIONTABLENAME,'D') D1 ON D1.dimension = D.id --to get the index of dimension in combination table
													WHERE rx.ReportGraphId = @ReportGraphID
													ORDER BY rx.AxisName 
	OPEN @DimensionCursor
	FETCH NEXT FROM @DimensionCursor
	INTO @DimensionId,@DimensionName,@IsDateDimension,@DimensionIndex
	WHILE @@FETCH_STATUS = 0 
	BEGIN
		-- =============================================================================================
			-- Added by Sohel Pathan on 20/02/2015 for PL ticket #129
			IF ((@SubDashboardOtherDimensionTable > 0) AND (@SubDashboardMainDimensionTable = @Dimensionid))
			BEGIN
				SET @Dimensionid = @SubDashboardOtherDimensionTable
			END
		-- =============================================================================================
		IF(LEN(@Column) > 0)
		BEGIN
			SET @Column = @Column + ',' --to add the comma after each column, here when there is second dimension
			SET @CreateTableColumn = @CreateTableColumn + ',' --to add the comma after each column, here when there is second dimension
			SET @MeasureTableColumn = @MeasureTableColumn + ',' --to add the comma after each column, here when there is second dimension
			SET @MeasureSelectColumn=@MeasureSelectColumn+','
			SET @GroupBy=@GroupBy+','
			SET @UpdateCondition=@UpdateCondition+' AND '

			END
			DEclare @Index NVARCHAR(100)=' INDEX '+@DimensionName+' CLUSTERED'
			if(@Count>1)
			SET @Index=''
		SET @CreateTableColumn = @CreateTableColumn + @DimensionName + ' NVARCHAR(1000)'+@Index+',OrderValue' + CAST(@Count AS NVARCHAR) + ' NVARCHAR(1000) '
		SET @MeasureTableColumn = @MeasureTableColumn + @DimensionName + ' NVARCHAR(1000) '+@Index
		SET @UpdateCondition=@UpdateCondition+'D.'+@DimensionName+'=M.'+@DimensionName

			
		IF(@IsDateDimension=0)
			BEGIN
				SET @Column = @Column + @DimensionIndex+'.DisplayValue as '+ @DimensionName + ','+ @DimensionIndex +'.OrderValue AS OrderValue' + CAST(@Count AS NVARCHAR) + ' '
				SET @MeasureSelectColumn=@MeasureSelectColumn+@DimensionIndex+'.DisplayValue as '+@DimensionName+' '
				SET @GroupBy=@GroupBy+@DimensionIndex+'.DisplayValue'
				
			END
		ELSE
			BEGIN
			SET @IsDateDImensionExist=1
			SET @DateDImensionId=@DimensionId
			if(@Count=2)
			SET @IsDateOnYAxis=1;
				--SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			--	SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			if(LOWER(@GType)='errorbar')
			BEGIN
			SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			/*  Insertion start:04/01/2016 Kausha #782   */
			SET @MeasureSelectColumn=@MeasureSelectColumn+ 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS '+@DimensionName+' '
			SET @GroupBy=@GroupBy+'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''')' 
			/*  Insertion end:04/01/2016 Kausha #782   */
			END
			ELSE
			BEGIN

				IF(@ViewByValue = 'Q') 
				BEGIN
					SET @Column = @Column +'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS ' +@DimensionName
					SET @GroupBy=@GroupBy++'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) '
				END
				ELSE IF(@ViewByValue = 'Y') 
				BEGIN
					SET @Column = @Column+ 'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR)  '
				END
				ELSE IF(@ViewByValue = 'M') 
				BEGIN
					SET @Column = @Column+ 'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) '
				END
				/* Start - Added by Arpita Soni for ticket #191 on 03/19/2015 */
				ELSE IF(@ViewByValue='W')
				BEGIN
					IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
					BEGIN
					SET @Column = @Column+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) '
					END
					ELSE 
					BEGIN
						SET @Column = @Column+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
						SET @MeasureSelectColumn=@MeasureSelectColumn+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))) AS '+@DimensionName
						SET @GroupBy=@GroupBy+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))) '
					END
				END
				/* End - Added by Arpita Soni for ticket #191 on 03/19/2015 */
				/* Start - Added by Arpita Soni for ticket #244 on 05/04/2015 */
				ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
				BEGIN
					SET @Column = @Column+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)) '
				END
				END

				if(@GType='columnrange')
				SET @Column = @Column+','+'cast(' + CAST(@DimensionIndex AS NVARCHAR) + '.displayvalue as datetime)'

				--SET @MeasureSelectColumn=@MeasureSelectColumn+ 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS '+@DimensionName+' '
--				SET @GroupBy=@GroupBy+'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''')' 
			END
	      --Total dimension count
		 if(@Count=1)
		 BEGIN
			
			SET @DimensionName1=@DimensionName
			SET @DimensionId1=@DimensionId
			
		END
		else

		 BEGIN
		 SET @DimensionName2=@DimensionName
		 SET @DimensionId2=@DimensionId
		
		 END
		SET @Count = @Count+1
		
		
	FETCH NEXT FROM @DimensionCursor
	INTO @DimensionId,@DimensionName,@IsDateDimension,@DimensionIndex
	END
	CLOSE @DimensionCursor
	DEALLOCATE @DimensionCursor
	if(@GType='ColumnRange')
	SET @CreateTableColumn = @CreateTableColumn + ',DislayValue NVARCHAR(1000)'

		IF(LOWER(@DisplayStatSignificance)='rate')
		BEGIN
			DECLARE @ConfidenceLevel FLOAT = (SELECT ConfidenceLevel FROM ReportGraph WHERE id=@ReportGraphId)
			SET @CreateTableColumn = @CreateTableColumn + ',PopulationRate NVARCHAR(1000),[LowerLimit-'+CAST((@ConfidenceLevel*100) AS NVARCHAR)+'%] NVARCHAR(1000),[UpperLimit-'+CAST((@ConfidenceLevel*100) AS NVARCHAR)+'%]  NVARCHAR(1000)'	
			SET @Column = @Column+','+'0,0,0 '
		END
		ELSE IF(@GType='errorbar')
		BEGIN
		SET @CreateTableColumn = @CreateTableColumn + ',PopulationRate NVARCHAR(1000),LowerLimit NVARCHAR(1000),UpperLimit NVARCHAR(1000)'	
		SET @Column = @Column+','+'0,0,0 '
		END

		IF(@IsDateDImensionExist=1 and (@Count-1=1 or @Count=1) )
			SET @IsonlyDateDimensionExist=1

	Insert into @DimensionTable(SelectDimension,CreateTableColumn,MeasureTableColumn,MeasureSelectColumn,UpdateCondition,GroupBy,Totaldimensioncount,DimensionName1,
	DimensionName2,DimensionId1,DimensionId2,IsDateDImensionExist,IsDateOnYAxis,IsonlyDateDimensionExist,cnt)values(@column ,@CreateTableColumn,@MeasureTableColumn,@MeasureSelectColumn,@UpdateCondition,@GroupBy,CAST(@Count-1 as nvarchar),@DimensionName1,
	@DimensionName2,@DimensionId1,@DimensionId2,@IsDateDImensionExist,@IsDateOnYAxis,@IsonlyDateDimensionExist,1)

	RETURN 
END
Go
--8
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetGraphMeasure') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetGraphMeasure
GO
CREATE FUNCTION [dbo].[GetGraphMeasure](@ReportGraphID INT, @ViewByValue NVARCHAR(15))
RETURNS @measureTable TABLE(MeasureName NVARCHAR(50),Measureid INT,MeasureTableName NVARCHAR(50), AggregationType NVARCHAR(50),
		SymbolType NVARCHAR(50),ColumnOrder NVARCHAR(50),DisplayInTable NVARCHAR(50),DrillDownWhereClause NVARCHAR(500))
AS
BEGIN
	
	INSERT INTO @measureTable
	SELECT M.Name,
		   M.id,
		   M.MeasureTableName,
		   M.AggregationType,
		   RGC.SymbolType,
		   RGC.ColumnOrder,
		   RGC.DisplayInTable,
		   M.DrillDownWhereClause
	FROM ReportGraphColumn RGC 
	INNER JOIN Measure M ON M.id =  CASE 
									WHEN @ViewByValue = 'Q' OR @ViewByValue = 'FQ' THEN ISNULL(RGC.QuarterMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'Y' OR @ViewByValue = 'FY' THEN ISNULL(RGC.YearMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'M' OR @ViewByValue = 'FM' THEN ISNULL(RGC.MonthMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'W' THEN ISNULL(RGC.WeekMeasureId,RGC.MeasureId) 
									END
	WHERE RGC.ReportGraphId = @ReportGraphID
	RETURN 
END;
/*End of #530 */

Go
--9
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasures') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasures
GO

CREATE FUNCTION [GetMeasures] 
(   @ReportGraphID int,
 @ViewByValue nvarchar(10),
  @DimensionCount int,
 @GType NVARCHAR(100),
 @DisplayStatSignificance NVARCHAR(15) = NULL
) 
RETURNS @MeasureTable TABLE(SelectTableColumn NVARCHAR(2000),--Select Column
CreateTableColumn NVARCHAR(2000),--CreateColumn
MeasureName NVARCHAR(2000),--MeasureName
SymbolType NVARCHAR(50)--MeasureName
 )
BEGIN
  declare @MeasureName nvarchar(100)
  declare @Column nvarchar(max)
  set @Column=''
  declare @SymbolType nvarchar(50)
  declare @MeasureTableColumn nvarchar(max)
  set @MeasureTableColumn=''

  set @Column=''
			set @Column=@Column+','+'0 as Rows'
			set @MeasureTableColumn=@MeasureTableColumn+','+'Rows INT'
 DECLARE @MeasureCursor CURSOR
			SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT Measurename,SymbolTyPe FROM dbo.GetGraphMeasure(@ReportGraphId,@ViewByValue) order by columnorder
			OPEN @MeasureCursor
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@SymbolType
			WHILE @@FETCH_STATUS = 0 
			BEGIN
			IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND  LOWER(@DisplayStatSignificance)!='rate')
			SET @MeasureName='Measure_'+@MeasureName
			set @Column=@Column+',0 as ['+@MeasureName+']'
			set @MeasureTableColumn=@MeasureTableColumn+', ['+@MeasureName+'] FLOAT'
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@SymbolType
			END
			CLOSE @MeasureCursor
			DEALLOCATE @MeasureCursor
			Insert into @MeasureTable(SelectTableColumn,CreateTableColumn,MeasureName,SymbolType)values(@column,@MeasureTableColumn,@MeasureName,@SymbolType)
			return 
END
--ENd GetMeasure

GO
--10
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'IdentifyDimensions') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION IdentifyDimensions
GO
-- =============================================
-- Author:		Manoj
-- Create date: 20Jun2014
-- Description:	Identify the dimensions and order them
-- =============================================
CREATE FUNCTION [IdentifyDimensions] 
(	
	@ObjectId INT,
	@IsGraph INT,
	@String NVARCHAR(1000)
)
RETURNS @tmpTable TABLE (Number INT,DimensionId INT)
AS
BEGIN
	DECLARE @temp_Table TABLE (DimensionId INT)
	DECLARE @Delimiter NVARCHAR(2);
	DECLARE @ConfiguredDimensions NVARCHAR(1000);
	DECLARE @Id INT
	SET @Delimiter = 'd';
	SET @ConfiguredDimensions = '';
	IF(@IsGraph = 1)	--Graph
	BEGIN	
			INSERT @temp_Table
			SELECT D.id FROM ReportGraph RG 
			INNER JOIN ReportGraphColumn GC ON GC.ReportGraphId = RG.id
			INNER JOIN Measure M ON M.id = GC.MeasureId
			INNER JOIN ReportAxis RA ON RA.ReportGraphId = RG.id
			INNER JOIN Dimension D ON D.id = RA.Dimensionid
			WHERE RG.id = @ObjectId
	END
	ELSE IF(@IsGraph = 2) -- Key data
	BEGIN
			INSERT @temp_Table
			SELECT DimensionId FROM KeyDataDimension WHERE KeyDataId = @ObjectId
	END		
	ELSE IF (@IsGraph = 3) -- Table
	BEGIN
			INSERT @temp_Table
			select DimensionId from ReportTableDimension WHERE ReportTableId = @ObjectId 
	END		
	ELSE
	BEGIN
			INSERT @temp_Table
			select DimensionId from ReportTableDimension WHERE ReportTableId = @ObjectId 
	END		
	DECLARE Column_Cursor CURSOR FOR
	SELECT DimensionId id FROM @temp_Table
	ORDER BY DimensionId
	OPEN Column_Cursor
	FETCH NEXT FROM Column_Cursor INTO @Id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @ConfiguredDimensions = @ConfiguredDimensions + 'd' + CAST(@Id AS NVARCHAR)
		FETCH NEXT FROM Column_Cursor INTO @Id
		END
	Close Column_Cursor
	Deallocate Column_Cursor
	SET @String = @String + @ConfiguredDimensions;
	;WITH Split(stpos,endpos) 
		AS(
			SELECT 0 AS stpos, CHARINDEX(@Delimiter,@String) AS endpos
			UNION ALL
			SELECT endpos+1, CHARINDEX(@Delimiter,@String,endpos+1)
				FROM Split
				WHERE endpos > 0
		),AllData AS(
			
			SELECT DISTINCT 
				'DimensionId' = SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos)
			FROM Split
			WHERE ISNULL(SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos),'') <> ''
		)
		INSERT @tmpTable 
		SELECT 'Number' = ROW_NUMBER() OVER (ORDER BY (SELECT 1)),* FROM AllData ORDER BY CAST (AllData.DimensionId AS INT) 
RETURN 
END



GO
--11
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'UF_CSVToTable') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION UF_CSVToTable
GO

CREATE FUNCTION [UF_CSVToTable]
(
 @psCSString VARCHAR(MAX)
)
RETURNS @otTemp TABLE(sID VARCHAR(MAX))
AS
BEGIN
 DECLARE @sTemp VARCHAR(MAX)

 WHILE LEN(@psCSString) > 0
 BEGIN
  SET @sTemp = LEFT(@psCSString, ISNULL(NULLIF(CHARINDEX(',', @psCSString) - 1, -1),
                    LEN(@psCSString)))
  SET @psCSString = SUBSTRING(@psCSString,ISNULL(NULLIF(CHARINDEX(',', @psCSString), 0),
                               LEN(@psCSString)) + 1, LEN(@psCSString))
  INSERT INTO @otTemp VALUES (@sTemp)
 END

RETURN
END

GO

--store procedure
--1
Go
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[Attribution_PositionBased]') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Attribution_PositionBased]
END
GO

CREATE PROCEDURE [dbo].[Attribution_PositionBased]
@AttrQuery NVARCHAR(MAX),
@AttrWhereQuery NVARCHAR(MAX),
@BaseTblName NVARCHAR(MAX),
@AttributionType INT
AS
BEGIN
BEGIN TRY
	DECLARE @Query NVARCHAR(MAX) ='',
			@TouchDateFieldName DATETIME, 
			@Revenue FLOAT, 
			@Index INT, 
			@DimensionId INT, 
			@DimensionValue NVARCHAR(MAX), 
			@RowNumber INT,
			@MinValue INT, 
			@MaxValue INT,
			@TouchId INT,
			@OppFirstTouchDate DATETIME,
			@OppLastTouchDate DATETIME,
			@OpportunityFieldname NVARCHAR(MAX),
			@DenseRank INT,
			@WhereClause NVARCHAR(MAX)

	-- Create temp table 
	CREATE TABLE #TempData (id INT,
							OpportunityFieldname NVARCHAR(100),
							TouchDateFieldName DATE,
							Revenue FLOAT,
							IndexNo INT,
							CampaignId NVARCHAR(100),
							DimensionId INT,
							DimensionValue NVARCHAR(MAX),
							TouchWeight INT,
							RevunueAfterWeight FLOAT,
							idDenseRank INT)

	SET @Query = ' DECLARE @TempTouchData TABLE(id INT NOT NULL PRIMARY KEY IDENTITY, OpportunityFieldname NVARCHAR(100), TouchDateFieldName DATE, '+
				 ' Revenue FLOAT, IndexNo INT, CampaignId NVARCHAR(100), DimensionId INT, DimensionValue NVARCHAR(MAX), TouchWeight INT, '+
				 ' RevunueAfterWeight FLOAT, idDenseRank INT) '+
				 ' INSERT INTO @TempTouchData (idDenseRank,OpportunityFieldname,TouchDateFieldName,Revenue,IndexNo,CampaignId)'

	-- Set base table name
	SET @BaseTblName = @BaseTblName + '_Base'

	-- If base table exist then apply inner join of base table and dimension value table
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @BaseTblName)
	BEGIN
		DECLARE @ConfigDimensionId INT, @ConfigDimensionValue NVARCHAR(MAX)
		IF ((SELECT COUNT(DISTINCT DimensionId) FROM AttrPositionConfig WHERE DimensionId IS NOT NULL) = 1)
		BEGIN
			SET @ConfigDimensionId = (SELECT TOP 1 DimensionId FROM AttrPositionConfig WHERE DimensionId IS NOT NULL)
			DECLARE @ColumnName NVARCHAR(MAX) = 'DimensionValue' + CAST(@ConfigDimensionId AS NVARCHAR)
			IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = @ColumnName AND OBJECT_ID = OBJECT_ID(@BaseTblName))
			BEGIN
				SET @Query = REPLACE(@Query, 'INSERT INTO @TempTouchData (',' INSERT INTO @TempTouchData (DV.DimensionId,DV.DimensionValue,')
				SET @Query = @Query + REPLACE(@AttrQuery,'SELECT',' SELECT DV.DimensionId, DV.DisplayValue AS DimensionValue, ')
			
				SET @Query = @Query + ' INNER JOIN  ' + @BaseTblName + ' WB' +' ON objTouches.id = WB.id INNER JOIN DimensionValue DV ON WB.' + @ColumnName + ' = DV.id '	
				SET @WhereClause = @AttrWhereQuery + ' AND DV.DimensionId = ' + CAST(@ConfigDimensionId AS NVARCHAR)
			END
			ELSE
			BEGIN
				SET @WhereClause = @AttrWhereQuery
				SET @Query = @Query + @AttrQuery
			END
		END
		ELSE
		BEGIN
			SET @WhereClause = @AttrWhereQuery
			SET @Query = @Query + @AttrQuery
		END
	END
	ELSE
	BEGIN
		SET @WhereClause = @AttrWhereQuery
		SET @Query = @Query + @AttrQuery
	END

	SET @Query = @Query + ISNULL(@WhereClause,'') + ' ;SELECT * FROM @TempTouchData '
	
	INSERT INTO #TempData EXEC(@Query)
	
	-- Get position based revenue value
	DECLARE @TouchRowCursor CURSOR
	SET @TouchRowCursor = CURSOR FAST_FORWARD FOR SELECT Id,TouchDateFieldName,Revenue,[IndexNo],DimensionId,DimensionValue,OpportunityFieldname FROM #TempData
	OPEN @TouchRowCursor
	FETCH NEXT FROM @TouchRowCursor
	INTO @TouchId, @TouchDateFieldName, @Revenue, @Index, @DimensionId, @DimensionValue,@OpportunityFieldname
	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- Get minimum and maximum index values and dates of first and last touch for an opportunity
		SELECT @MaxValue = MAX([IndexNo]),@MinValue = MIN([IndexNo]), @OppFirstTouchDate = MIN(TouchDateFieldName), @OppLastTouchDate = MAX(TouchDateFieldName) 
		FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname
		GROUP BY OpportunityFieldname 

		-- Update touch value as per configuration 
		UPDATE #TempData 
		SET Revenue = dbo.GetPositionBasedRevenue(@TouchDateFieldName,@Revenue,@Index,@DimensionId,@DimensionValue,@MinValue,@Maxvalue,@AttributionType,@OppFirstTouchDate,@OppLastTouchDate) 
		WHERE Id = @TouchId
		
		FETCH NEXT FROM @TouchRowCursor
		INTO @TouchId, @TouchDateFieldName,@Revenue,@Index,@DimensionId,@DimensionValue,@OpportunityFieldname
	END
	CLOSE @TouchRowCursor
	DEALLOCATE @TouchRowCursor

	-- Update average revenue in case when multiple records in first/last touches
	DECLARE @RevenueCursor CURSOR
	SET @RevenueCursor = CURSOR FAST_FORWARD FOR SELECT DISTINCT idDenseRank, OpportunityFieldname FROM #TempData
	OPEN @RevenueCursor 
	FETCH NEXT FROM @RevenueCursor 
	INTO @DenseRank, @OpportunityFieldname 
	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #TempData 
		SET Revenue = ROUND((SELECT AVG(Revenue) FROM #TempData WHERE idDenseRank = @DenseRank AND OpportunityFieldname = @OpportunityFieldname 
						GROUP BY OpportunityFieldname, idDenseRank),2)
		WHERE idDenseRank = @DenseRank AND OpportunityFieldname = @OpportunityFieldname
	
		FETCH NEXT FROM @RevenueCursor 
		INTO @DenseRank, @OpportunityFieldname
	END
	CLOSE @RevenueCursor 
	DEALLOCATE @RevenueCursor 
	
	SELECT OpportunityFieldname,TouchDateFieldName,Revenue,CampaignId,DimensionId,DimensionValue FROM #TempData 

END TRY
BEGIN CATCH
	PRINT ERROR_MESSAGE()
END CATCH
END
GO
--2
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[AttributionCalculation_TimeDecay]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[AttributionCalculation_TimeDecay]  
END
GO

CREATE PROCEDURE [dbo].[AttributionCalculation_TimeDecay]

 @TouchTableName Nvarchar(255),
 @OpportunityFieldname  Nvarchar(255),
 @TouchDateFieldName Nvarchar(255),
 @OpportunityTableName Nvarchar(255),
 @OpportunityRevenue Nvarchar(255) , --
 @OpportunityCloseDate Nvarchar(255),
 @TouchWhereClause NVARCHAR(MAX),
 @AttributionType int,  /*  @AttributionType 1 - First Touch,  2 - Last Touch, 3 - Evenly Distributed,  4 - Time Decay,  5 - Position,  6 - Interaction, 7 - W Shaped  */
 @MaximumDays INT,
 @HalfLife INT,--Need to write 0 here for linear method 
 @Query NVARCHAR(MAX)

 AS
BEGIN


BEGIN TRY

--CREATE temp table and insert base query data in to it for further calculation
DECLARE @tempTable NVARCHAR(2000)='Declare @temptable Table('+@opportunityfieldname+' NVARCHAR(500),'+@TouchDateFieldName+' Date,Amount float,[Index] INT,CampaignID NVARCHAR(500),
'+@OpportunityCloseDate+' Date, [daysDifference] Float,[Weight] DECIMAL(18,2),Revenue Float ) INSERT INTO @temptable '


DECLARE @tempQuery NVARCHAR(MAX)=''

--CURSOR for Linear and half life method
SET @tempQuery='
DECLARE @OpportunityId NVARCHAR(500)
DECLARE @Amount FLOAT
DECLARE @Index INT

--------------------------------------------------------
DECLARE @WeightageCursor CURSOR
SET @WeightageCursor = CURSOR FAST_FORWARD
FOR
SELECT  '+@OpportunityFieldname+','+@OpportunityRevenue+',[INDEX] from @temptable

OPEN @WeightageCursor
FETCH NEXT FROM @WeightageCursor 
INTO @OpportunityId,@Amount,@Index
WHILE @@FETCH_STATUS = 0
BEGIN
Declare @Sum Float
IF(@Amount!=0)
BEGIN
IF('+CAST(@HalfLife AS NVARCHAR(50))+' =0)
BEGIN
SET @Sum=(SELECT SUM(('+(CAST(@MaximumDays AS NVARCHAR(50)))+'-daysDifference)) From @temptable  where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@OpportunityId)
IF(@Sum>0)
BEGIN
UPDATE @temptable SET Weight=(@Amount/@Sum) WHERE '+@OpportunityFieldname+'=@OpportunityId
UPDATE @temptable SET Revenue=Weight*('+CAST(@MaximumDays AS NVARCHAR)+'-daysDifference) WHERE '+@OpportunityFieldname+'=@OpportunityId
END
END
ELSE
BEGIN

UPDATE @temptable SET weight=('+CAST(@MaximumDays AS NVARCHAR)+'/(POWER(2.00,daysdifference/'+CAST(@HalfLife AS NVARCHAR(50))+'))) where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@OpportunityId AND [INDEX]=@Index
END

END

 
FETCH NEXT FROM @WeightageCursor
INTO @OpportunityId,@Amount,@Index
END
CLOSE @WeightageCursor
DEALLOCATE @WeightageCursor 

IF('+CAST(@HalfLife AS NVARCHAR(50))+'!=0)
BEGIN
DECLARE @InnerOpportunityId NVARCHAR(500)
DECLARE @InnerAmount FLOAT
DECLARE @InnerIndex INt

DECLARE @WeightageInnerCursor CURSOR
SET @WeightageInnerCursor = CURSOR FAST_FORWARD
FOR
SELECT  OpportunityId,Amount,[INDEX] from @temptable


OPEN @WeightageInnerCursor
FETCH NEXT FROM @WeightageInnerCursor
INTO @InnerOpportunityId,@InnerAmount,@InnerIndex
WHILE @@FETCH_STATUS = 0
BEGIN
IF(@InnerAmount!=0)
BEGIN
SET @Sum=(SELECT SUM(weight) From @temptable  where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@InnerOpportunityId)
IF(@Sum>0)
BEGIN
UPDATE @temptable SET Revenue=(Weight*Amount/@SUM) WHERE '+@OpportunityFieldname+'=@InnerOpportunityId AND [INDEX]=@InnerIndex
END
END

 
FETCH NEXT FROM @WeightageInnerCursor
INTO @InnerOpportunityId,@InnerAmount,@InnerIndex
END
CLOSE @WeightageInnerCursor
DEALLOCATE @WeightageInnerCursor 
END
'

DECLARE @SelectQuery NVARCHAR(2000)='SELECT '+@OpportunityFieldname+','+@TouchDateFieldName+','+'Revenue,CampaignID FROM @temptable WHERE DaysDifference<= '+CAST(@MaximumDays AS NVARCHAR)
EXEC(@tempTable+ @Query+@tempQuery+@selectquery)

END TRY
BEGIN CATCH
		PRINT ERROR_MESSAGE();
	END CATCH


END
GO
--3
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'AttributionCalculation') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[AttributionCalculation]
END
GO

CREATE PROCEDURE [dbo].[AttributionCalculation]

 @TouchTableName NVARCHAR(255)= '_WaterFall',
 @OpportunityFieldname  NVARCHAR(255)= 'OpportunityId',
 @TouchDateFieldName NVARCHAR(255)= 'ActivityDate',
 @OpportunityTableName NVARCHAR(255)= '_Opportunities',
 @OpportunityRevenue NVARCHAR(255) ='Amount', --
 @OpportunityCloseDate NVARCHAR(255)= 'CloseDate',
 @TouchWhereClause NVARCHAR(MAX)= 'SELECT ID FROM _Waterfall WHERE ID IN (510074,333900,347622,510091,719583,140549,140567,333880,347568,716203,717655,717825) ',
 @AttributionType INT = 5,  /*  @AttributionType 1 - First Touch,  2 - Last Touch, 3 - Evenly Distributed,  4 - Time Decay,  5 - Position,  6 - Interaction, 7 - W Shaped  */
 @MaximumDays INT=400,
 @HalfLife INT=0--Need to write 0 here for linear method 

AS
BEGIN
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TouchTableName) AND 
   EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @OpportunityTableName) AND 
   EXISTS (SELECT * FROM sys.columns WHERE Name = @TouchDateFieldName AND Object_ID = Object_ID(@TouchTableName)) AND 
   EXISTS (SELECT * FROM sys.columns WHERE Name = @OpportunityFieldname AND Object_ID = Object_ID(@OpportunityTableName))
BEGIN
	BEGIN TRY
		DECLARE @BaseQuery NVARCHAR(MAX)='',
				@WhereQuery NVARCHAR(MAX)='',
				@Attribution NVARCHAR(50)='',
				@TempOpportunityRevenue NVARCHAR(50)='',
				@Revenue NVARCHAR(50)='Revenue',
				@IndexNo NVARCHAR(50)='IndexNo',
				@index NVARCHAR(2000) = '',
				@ClosingQuery NVARCHAR(1000)=') DatTable ',
				@IndexWhere Nvarchar(500)='',
				@DenseRank NVARCHAR(2000) = '',
				@DenseRankNo NVARCHAR(50)='idDenseRank'

		--Use For Evenly Weighted Attribution
		DECLARE @TmpTbl TABLE (OpportunityId NVARCHAR(MAX), ActivityDate NVARCHAR(MAX), Revenue FLOAT, IndexNo INT, CampaignId NVARCHAR(MAX))

		IF(@AttributionType = 1) 
		BEGIN
			SET @Attribution = 'ASC'
		END  
		ELSE IF(@AttributionType = 2)
		BEGIN
			SET @Attribution = 'DESC'   
		END
		ELSE IF(@AttributionType = 3)
		BEGIN 
			SET @Attribution = ''
		END
		
		SET @TempOpportunityRevenue = @OpportunityRevenue

		IF(@AttributionType = 5 OR @AttributionType = 7)
		BEGIN
			SET @DenseRank = 'DENSE_RANK() OVER (PARTITION BY objOpportunities.' + @OpportunityFieldname + ' ORDER BY objOpportunities.' + @OpportunityFieldname + ', ' + @TouchDateFieldName + ' ASC ) AS '+ @DenseRankNo + ', '	
		END
		ELSE IF(@AttributionType = 6)
		BEGIN
			SET @DenseRank = 'DENSE_RANK() OVER (ORDER BY objOpportunities.' + @OpportunityFieldname + ') AS ' + @DenseRankNo + ', '
		END

		SET @index = ',ROW_NUMBER() OVER (PARTITION BY objOpportunities.' + @OpportunityFieldname + ' ORDER BY ['+@TouchDateFieldName+'] ' + @Attribution + ') AS ' + @IndexNo

		SET @TempOpportunityRevenue = @TempOpportunityRevenue + ' AS ' + @Revenue

		IF(@AttributionType = 1 OR @AttributionType = 2)
		BEGIN
			--Table Join
			SET @BaseQuery='SELECT '+@OpportunityFieldname+','

			IF(@AttributionType=3)
			BEGIN
				SET @BaseQuery= @BaseQuery + '(' + @Revenue + ' / MAX(' + @IndexNo + ')' + ') AS ' + @Revenue
			END
			ELSE
			BEGIN
				SET @BaseQuery= @BaseQuery + @TouchDateFieldName+','+@Revenue
			END

			SET @BaseQuery= @BaseQuery + ',CampaignId FROM ('
		END
	
		SET @BaseQuery= @BaseQuery + 'SELECT '

		IF(@AttributionType = 5 OR @AttributionType = 6 OR @AttributionType = 7)
		BEGIN
			SET @BaseQuery= @BaseQuery + @DenseRank
		END
		DECLARE @TimeDecayFields NVARCHAR(1000)
		SET @TimeDecayFields='';

		IF(@AttributionType=4)
		SET @TimeDecayFields=','+@OpportunityCloseDate+',DATEDIFF(day,'+@TouchDateFieldName+','+@OpportunityCloseDate+'),0,0'

		SET @BaseQuery= @BaseQuery + 'objOpportunities.'+@OpportunityFieldname+','+@TouchDateFieldName+','+@TempOpportunityRevenue+''+@index+',objOpportunities.CampaignId '+@TimeDecayFields+' FROM '+@TouchTableName +' objTouches'

		IF(LEN(@OpportunityTableName) > 0)
		BEGIN
			SET @BaseQuery=@BaseQuery+ ' Inner join '+@OpportunityTableName+' objOpportunities ON objOpportunities.'+@OpportunityFieldname+' =objTouches.'+@OpportunityFieldname+' '
		END
		--Touch Where Clause
		IF(LEN(@TouchWhereClause) > 0)
		BEGIN
			SET @WhereQuery=' WHERE  objTouches.Id IN('+@TouchWhereClause+') AND '
		END
		ELSE
		BEGIN
			SET @WhereQuery= @WhereQuery+' WHERE '
		END
		--Opportunity Close Date Where Clause 
		SET @WhereQuery=@WhereQuery+'objOpportunities.'+@OpportunityCloseDate+'<=GETDATE()'
		
		SET @IndexWhere = ' WHERE ' + @IndexNo + ' = 1'

		SET @WhereQuery=@WhereQuery + ' AND '+@TouchDateFieldName+' < '+@OpportunityCloseDate
		IF (@AttributionType = 1 OR @AttributionType = 2)
		BEGIN
			EXEC(@BaseQuery+@WhereQuery+@CLosingQuery+@IndexWhere)
		END
		ELSE IF(@AttributionType=3)
		BEGIN
			INSERT INTO @TmpTbl
			EXEC (@BaseQuery+@WhereQuery)
			Update A set A.IndexNo = B.IndNo FROM @TmpTbl A INNER JOIN (
			SELECT OpportunityId, MAX(IndexNo) AS IndNo FROM @TmpTbl A GROUP BY OpportunityId) B ON A.OpportunityId = B.OpportunityId

			SELECT OpportunityId, ActivityDate, (Revenue / IndexNo) AS Revenue , CampaignId FROM @TmpTbl
		END
		ELSE IF(@AttributionType = 4) 
		BEGIN
			 IF( LEN(@HalfLife) IS NULL)
			 SET @HalfLife=0
			  --Half life and maximum days can not less then zero
		 		 IF(@HalfLife<0)
				 BEGIN
				 PRINT 'Half life can not be nagative.'
				 RETURN
				 END
			 IF(LEN(@MaximumDays) IS NULL OR @MaximumDays<=0)
				 BEGIN
				 PRINT 'Maximum Days must be grater then 0.  '
				 RETURN
				 END
				 ELSE
				 BEGIN
					DECLARE @FinalQuery NVARCHAR(MAX)=@BaseQuery+@WhereQuery
					EXEC AttributionCalculation_TimeDecay @TouchTableName,@OpportunityFieldname,@TouchDateFieldName,@OpportunityTableName,@OpportunityRevenue,@OpportunityCloseDate,@TouchWhereClause,@AttributionType,@MaximumDays,@HalfLife,@FinalQuery
				 END
		END  
		ELSE IF(@AttributionType = 5 OR @AttributionType = 7)
		BEGIN
			IF EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE XValue IS NOT NULL AND YValue IS NOT NULL)
			BEGIN
				EXEC Attribution_PositionBased @BaseQuery, @WhereQuery, @TouchTableName, @AttributionType
			END
			ELSE
			BEGIN
				PRINT 'Please enter valid configuration in AttrPositionConfig Table.'
			END
		END
		ELSE IF(@AttributionType = 6)
		BEGIN
			IF EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE [Weight] IS NOT NULL)
			BEGIN
				EXEC InteractionBasedAttrCalc @BaseQuery, @WhereQuery, @TouchTableName
			END
			ELSE
			BEGIN
				PRINT 'Please enter valid configuration in AttrPositionConfig Table.'
			END
		END
	
	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE();
	END CATCH
END
ELSE
BEGIN
	PRINT 'Table name / column names passed as parameter are not exit in the database, please check and pass the proper parameters.'
END	

END


GO

--4
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'BaseDynamicColumnCreation') AND xtype IN (N'P'))
    DROP PROCEDURE BaseDynamicColumnCreation
GO


CREATE PROCEDURE [BaseDynamicColumnCreation]  @DEBUG bit = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

	--select TableName, DimensionTableName from DynamicDimension

	DECLARE @COLUMNNAME varchar(200);
	DECLARE @CONSTRAINTTABLENAME varchar(200);
	DECLARE @DIMENSIONTABLENAME varchar(200);
	DECLARE @DIMENSIONCOLUMNNAME varchar(200);
	DECLARE @DIMENSIONID varchar(20);

		CREATE TABLE #Keys
		(
			PKTABLE_QUALIFIER varchar(150),
			PKTABLE_OWNER varchar(150),
			PKTABLE_NAME varchar(150),
			PKCOLUMN_NAME varchar(150),
			FKTABLE_QUALIFIER varchar(150),
			FKTABLE_OWNER varchar(150),
			FKTABLE_NAME varchar(150),
			FKCOLUMN_NAME varchar(150),
			KEY_SEQ int,
			UPDATE_RULE int,
			DELETE_RULE int,
			FK_NAME varchar(150),
			PK_NAME varchar(150),
			DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= 'DimensionValue_New'

		DECLARE @FKTABLE varchar(150);
		DECLARE @FKNAME varchar(150);
		DECLARE @FKCOLUMN varchar(150);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME, @FKCOLUMN
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(1000);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;


			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END

			exec(@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME, @FKCOLUMN
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor

		drop table #Keys

	DECLARE @TableDropCreate nvarchar(max) 
	
	SET @TableDropCreate=' IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[DimensionValue_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE DimensionValue_New 
		END
		CREATE TABLE [dbo].[DimensionValue_New](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[DimensionID] [int] NOT NULL,
			[Value] [nvarchar](1000) NULL,
			[DisplayValue] [nvarchar](1000) NULL,
			[OrderValue] [nvarchar](1000) NULL,
		 CONSTRAINT [PK_DimensionValue_New_' + cast(DATEDIFF(second, '2015-01-01', GETDATE()) as varchar) + '] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
		) ON [PRIMARY]'

	exec(@TableDropCreate)

	DECLARE Base_Table_Cursor CURSOR FOR
	select distinct TableName from dimension where IsDeleted=0
	OPEN Base_Table_Cursor
	FETCH NEXT FROM Base_Table_Cursor
	INTO @DIMENSIONTABLENAME
	while @@FETCH_STATUS = 0
		BEGIN

		--We need to create the _base tables
		exec('IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[' + @DIMENSIONTABLENAME + '_Base_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE ' + @DIMENSIONTABLENAME + '_Base_New 
		END
		CREATE TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New](
		[id] [bigint] NOT NULL
		) ON [PRIMARY]')



		FETCH NEXT FROM Base_Table_Cursor
		INTO @DIMENSIONTABLENAME
		END
	Close Base_Table_Cursor
	Deallocate Base_Table_Cursor



	DECLARE Dimension_Cursor CURSOR FOR
	select 'DimensionValue'+ cast(id as varchar), 'DimensionValue', TableName, ColumnName, id from dimension where IsDeleted=0
	OPEN Dimension_Cursor
	FETCH NEXT FROM Dimension_Cursor
	INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
	while @@FETCH_STATUS = 0
		Begin

		DECLARE @ADDQUERY varchar(1000);
		set @ADDQUERY = N'ALTER TABLE ' + @DIMENSIONTABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		SET @CURQUERYFORERROR=@ADDQUERY
		if @DEBUG=1
		BEGIN
			print @ADDQUERY
		END
		exec(@ADDQUERY)

		DECLARE @ADDCONSTRAINT varchar(1000);
		set @ADDCONSTRAINT = N'ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New]  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + '] FOREIGN KEY([' + @COLUMNNAME + '])
								REFERENCES [dbo].[' + @CONSTRAINTTABLENAME + '_New] ([id])
								ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New] CHECK CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + ']'

		SET @CURQUERYFORERROR=@ADDCONSTRAINT
		if @DEBUG=1
		BEGIN
			print @ADDCONSTRAINT
		END
		exec(@ADDCONSTRAINT)


		--Need to populate the column now!!!!!!!!!!!!!

		--DECLARE @POPULATEQUERY nvarchar(4000);
	
		--SET @POPULATEQUERY= N'Update ' + @DIMENSIONTABLENAME + ' SET ' + @COLUMNNAME + '=did from (select d.id as did, c.id as cid from DimensionValue d inner join ' + @DIMENSIONTABLENAME + ' c on c.' + @DIMENSIONCOLUMNNAME + '=d.Value where DimensionID=' + @DIMENSIONID + ') A where cid=id'

		--select @POPULATEQUERY

		--print @POPULATEQUERY

		--execute sp_executesql @POPULATEQUERY


		FETCH NEXT FROM Dimension_Cursor
		INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
		End
	Close Dimension_Cursor
	Deallocate Dimension_Cursor
END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','BaseDynamicColumnCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END


GO
--5
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[BaseDynamicColumnCreationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[BaseDynamicColumnCreationPartial]  
END
GO


CREATE PROCEDURE [dbo].[BaseDynamicColumnCreationPartial]  @DEBUG bit = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

	--select TableName, DimensionTableName from DynamicDimension

	DECLARE @COLUMNNAME varchar(200);
	DECLARE @CONSTRAINTTABLENAME varchar(200);
	DECLARE @DIMENSIONTABLENAME varchar(200);
	DECLARE @DIMENSIONCOLUMNNAME varchar(200);
	DECLARE @DIMENSIONID varchar(20);

		CREATE TABLE #Keys
		(
			PKTABLE_QUALIFIER varchar(150),
			PKTABLE_OWNER varchar(150),
			PKTABLE_NAME varchar(150),
			PKCOLUMN_NAME varchar(150),
			FKTABLE_QUALIFIER varchar(150),
			FKTABLE_OWNER varchar(150),
			FKTABLE_NAME varchar(150),
			FKCOLUMN_NAME varchar(150),
			KEY_SEQ int,
			UPDATE_RULE int,
			DELETE_RULE int,
			FK_NAME varchar(150),
			PK_NAME varchar(150),
			DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= 'DimensionValue_New'

		DECLARE @FKTABLE varchar(150);
		DECLARE @FKNAME varchar(150);
		DECLARE @FKCOLUMN varchar(150);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME, @FKCOLUMN
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(1000);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;


			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END

			exec(@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME, @FKCOLUMN
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor

		drop table #Keys

	DECLARE @TableDropCreate nvarchar(max) 
	
	SET @TableDropCreate=' IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[DimensionValue_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE DimensionValue_New 
		END
		CREATE TABLE [dbo].[DimensionValue_New](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[DimensionID] [int] NOT NULL,
			[Value] [nvarchar](1000) NULL,
			[DisplayValue] [nvarchar](1000) NULL,
			[OrderValue] [nvarchar](1000) NULL,
		 CONSTRAINT [PK_DimensionValue_New_' + cast(DATEDIFF(second, '2015-01-01', GETDATE()) as varchar) + '] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
		) ON [PRIMARY]'

	exec(@TableDropCreate)

	DECLARE Base_Table_Cursor CURSOR FOR
	select distinct DimensionTableName from DynamicDimension_New
	OPEN Base_Table_Cursor
	FETCH NEXT FROM Base_Table_Cursor
	INTO @DIMENSIONTABLENAME
	while @@FETCH_STATUS = 0
		BEGIN

		--We need to create the _base tables
		exec('IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[' + @DIMENSIONTABLENAME + '_Base_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE ' + @DIMENSIONTABLENAME + '_Base_New 
		END
		CREATE TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New](
		[id] [bigint] NOT NULL
		) ON [PRIMARY]')



		FETCH NEXT FROM Base_Table_Cursor
		INTO @DIMENSIONTABLENAME
		END
	Close Base_Table_Cursor
	Deallocate Base_Table_Cursor


	DECLARE @TablesToAddDirtyBitTo table (tablename nvarchar(max))


	DECLARE Dimension_Cursor CURSOR FOR
	select 'DimensionValue'+ cast(id as varchar), 'DimensionValue', TableName, ColumnName, id from dimension where IsDeleted=0 and tablename in (select dimensiontablename from DynamicDimension_New)
	OPEN Dimension_Cursor
	FETCH NEXT FROM Dimension_Cursor
	INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
	while @@FETCH_STATUS = 0
		Begin

		if (select count(*) from @TablesToAddDirtyBitTo where tablename=@DIMENSIONTABLENAME)=0
		BEGIN
			insert into @TablesToAddDirtyBitTo select @DIMENSIONTABLENAME
		END

		DECLARE @ADDQUERY varchar(1000);
		set @ADDQUERY = N'ALTER TABLE ' + @DIMENSIONTABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		SET @CURQUERYFORERROR=@ADDQUERY
		if @DEBUG=1
		BEGIN
			print @ADDQUERY
		END
		exec(@ADDQUERY)

		DECLARE @ADDCONSTRAINT varchar(1000);
		set @ADDCONSTRAINT = N'ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New]  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + '] FOREIGN KEY([' + @COLUMNNAME + '])
								REFERENCES [dbo].[' + @CONSTRAINTTABLENAME + '_New] ([id])
								ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New] CHECK CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + ']'

		SET @CURQUERYFORERROR=@ADDCONSTRAINT
		if @DEBUG=1
		BEGIN
			print @ADDCONSTRAINT
		END
		exec(@ADDCONSTRAINT)


		--Need to populate the column now!!!!!!!!!!!!!

		--DECLARE @POPULATEQUERY nvarchar(4000);
	
		--SET @POPULATEQUERY= N'Update ' + @DIMENSIONTABLENAME + ' SET ' + @COLUMNNAME + '=did from (select d.id as did, c.id as cid from DimensionValue d inner join ' + @DIMENSIONTABLENAME + ' c on c.' + @DIMENSIONCOLUMNNAME + '=d.Value where DimensionID=' + @DIMENSIONID + ') A where cid=id'

		--select @POPULATEQUERY

		--print @POPULATEQUERY

		--execute sp_executesql @POPULATEQUERY


		FETCH NEXT FROM Dimension_Cursor
		INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
		End
	Close Dimension_Cursor
	Deallocate Dimension_Cursor

	while (select count(*) from @TablesToAddDirtyBitTo) > 0
	BEGIN
		DECLARE @TableToAdd nvarchar(max)= (select top 1 tablename from @TablesToAddDirtyBitTo)

		exec('ALTER TABLE ' + @TableToAdd + '_Base_New ADD dirty bit ')

		delete from @TablesToAddDirtyBitTo where tablename=@TableToAdd
	END


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','BaseDynamicColumnCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END

GO
--6
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateFiscalYearQuarter') AND xtype IN (N'P'))
    DROP PROCEDURE CalculateFiscalYearQuarter
GO
CREATE PROCEDURE [dbo].[CalculateFiscalYearQuarter]
AS
BEGIN

IF EXISTS (SELECT TOP 1 * FROM FiscalQuarterYear)
BEGIN
--do what you need if exists


IF EXISTS(SELECT * FROM FiscalQuarterYear WHERE EndDate IS NULL)
BEGIN
	UPDATE FiscalQuarterYear SET EndDate = DATEADD(DAY,-1,DATEADD(mm, DATEDIFF(m,0,StartDate)+1,0)) WHERE EndDate IS NULL
END

DECLARE @FQ INT, @FY INT, @Date DATETIME
DECLARE FQY_Cursor CURSOR FOR
SELECT DISTINCT FiscalYear FROM FiscalQuarterYear ORDER BY FiscalYear
OPEN FQY_Cursor
FETCH NEXT FROM FQY_Cursor
INTO @FY
       WHILE @@FETCH_STATUS = 0
       BEGIN
              DECLARE @RecordCount INT, @FirstQ INT, @MaxQ INT, @MaxStartDate datetime, @MaxEndDate datetime
              SELECT @RecordCount = COUNT(*) FROM FiscalQuarterYear FY1 WHERE FY1.FiscalYear = @FY

              IF(@RecordCount > 0 AND @RecordCount < 12)
              BEGIN
					SELECT @MaxQ = MAX(FiscalQuarter) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @FirstQ = MAX(FiscalQuarter) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @MaxStartDate = MAX(StartDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @MaxEndDate = MAX(EndDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY

					DECLARE @EnteredMonth INT
					DECLARE @intFlag INT
					DECLARE @TotalRow INT

					SELECT @EnteredMonth = MAX([Month]) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SET @intFlag = 1
					SET @TotalRow = 12
					DECLARE @IncrCnt int
					SET @IncrCnt = 2
					WHILE (@intFlag <= @TotalRow)
					BEGIN
						DECLARE @MonthId int
						SET @MonthId = @EnteredMonth-@intFlag
						IF (@EnteredMonth > @intFlag)
						BEGIN
							IF(@intFlag = 1)
							BEGIN
								IF(@MaxQ <> 1)
								BEGIN
									SET @MaxQ  = @MaxQ - 1
								END
								ELSE
								BEGIN
									SET @MaxQ = 4
								END
							END
							IF NOT EXISTS (SELECT * FROM FiscalQuarterYear WHERE FiscalYear = @FY AND [Month] = @MonthId)
							BEGIN
								IF((SELECT COUNT(*) FROM FiscalQuarterYear WHERE FiscalQuarter = @MaxQ) > 2)
								BEGIN
									IF(@MaxQ <> 1)
									BEGIN
										SET @MaxQ  = @MaxQ - 1
									END
									ELSE
									BEGIN
										SET @MaxQ = 4
									END
								END
							DECLARE @MaxStDt datetime
								SELECT @MaxStDt = MIN(StartDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
								INSERT INTO FiscalQuarterYear (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month]) 
								SELECT @MaxQ,@FY, DATEADD(MONTH, -1, @MaxStDt), DATEADD(DAY, -1, @MaxStDt),@MonthId
							END
						END
						ELSE IF (@EnteredMonth < @intFlag)
						BEGIN
							IF(@IncrCnt = 2)
							BEGIN
								SET @MaxQ = @FirstQ
							END
							IF((SELECT COUNT(*) FROM FiscalQuarterYear WHERE FiscalQuarter = @MaxQ) > 2)
							BEGIN
								IF(@MaxQ <> 4)
								BEGIN
									SET @MaxQ  = @MaxQ + 1
								END
								ELSE
								BEGIN
									SET @MaxQ = 1
								END
							END
							IF NOT EXISTS (SELECT * FROM FiscalQuarterYear WHERE FiscalYear = @FY AND [Month] = @MonthId)
								DECLARE @MaxEnDt datetime
							SELECT @MaxEnDt = MAX(EndDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
							INSERT INTO FiscalQuarterYear (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month]) 
							SELECT @MaxQ ,@FY, DATEADD(DAY, 1, @MaxEnDt), DATEADD(DAY, -1, DATEADD(MONTH, 1, DATEADD(DAY, 1, @MaxEnDt))),@intFlag
							SET @IncrCnt = @IncrCnt + 1
						END

						SET @intFlag = @intFlag + 1
					END
              END

       FETCH NEXT FROM FQY_Cursor
       INTO @FY
       END
Close FQY_Cursor
Deallocate FQY_Cursor

DELETE FROM ProcessedFQY;

;WITH CalculateEndDate AS(
	SELECT 
		FQY.FiscalQuarter,
		FQY.FiscalYear,
		FQY.StartDate,
		EndDate = 
			CASE WHEN FQY.EndDate IS NOT NULL THEN FQY.EndDate 
				ELSE (
					CASE WHEN FQY.FiscalQuarter = 4 THEN (SELECT TOP 1 DATEADD(DAY,-1,T1.StartDate) FROM FiscalQuarterYear T1 WHERE T1.FiscalYear = FQY.FiscalYear + 1 AND T1.FiscalQuarter = 1)
						ELSE (SELECT TOP 1 DATEADD(DAY,-1,T1.StartDate) FROM FiscalQuarterYear T1 WHERE T1.FiscalYear = FQY.FiscalYear AND T1.FiscalQuarter = FQY.FiscalQuarter + 1) 
					END)
			END,
		FQY.[Month]
	FROM FiscalQuarterYear FQY
)

INSERT INTO ProcessedFQY
SELECT FiscalQuarter, FiscalYear, StartDate, EndDate = CASE WHEN EndDate IS NULL THEN DATEADD(DAY,-1,DATEADD(MONTH,3,StartDate)) ELSE EndDate END, [Month], UPPER(SUBSTRING(DateName( month , DateAdd( month , [Month] , 0 ) - 1 ),1,3)) FROM CalculateEndDate  ORDER BY [Month]

--calcluate the remaining year and quarter
DECLARE @MinYear INT, @MaxYear INT, @Year INT;
DECLARE @StartDate DATE,@EndDate DATE;


SET @MinYear = 1999; SET @MaxYear = 2029;

--Minimum  to 2000
SELECT @Year = MIN(FiscalYear)  FROM ProcessedFQY 
SET @Year = @Year - 1
WHILE @Year > @MinYear
BEGIN
	DECLARE @intPFlag INT
	DECLARE @TotalPRow INT
	DECLARE @MonthNo int
	DECLARE @FisQtr int
	SET @intPFlag = 1
	SET @TotalPRow = 12

	SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year + 1 AND [Month] = 1

	WHILE (@intPFlag <= @TotalPRow)
	BEGIN
		SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)))

		INSERT INTO ProcessedFQY VALUES 
		(@FisQtr,@Year, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)),DATEADD(DAY, -1, DATEADD(MONTH, 1, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)))),@MonthNo, UPPER(SUBSTRING(DateName( month , DateAdd( month , @MonthNo , 0 ) - 1 ),1,3)))

		SET @intPFlag = @intPFlag + 1
	END

	SET @Year = @Year - 1
END

--Maximum to 2029
SELECT @Year = MAX(FiscalYear)  FROM ProcessedFQY 
--Modified by Arpita Soni for ticket #531 on 09/28/2015
WHILE @Year < @MaxYear
BEGIN
	DECLARE @intPMaxFlag INT
	DECLARE @TotalPMaxRow INT
	SET @intPMaxFlag = 1
	SET @TotalPMaxRow = 12

	SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year AND [Month] = 12

	WHILE (@intPMaxFlag <= @TotalPMaxRow)
	BEGIN
		SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH,DATEADD(MONTH,((@intPMaxFlag+1) - 1),@StartDate))

		INSERT INTO ProcessedFQY VALUES 
		(@FisQtr,@Year+1,DATEADD(MONTH,((@intPMaxFlag+1) - 1),@StartDate),DATEADD(MONTH,((@intPMaxFlag+1) - 1),@EndDate),@MonthNo, UPPER(SUBSTRING(DateName( month , DateAdd( month , @MonthNo , 0 ) - 1 ),1,3)))

		SET @intPMaxFlag = @intPMaxFlag + 1
	END

	SET @Year = @Year +1
END

--All remaining years
DECLARE @intPRemFlag INT
DECLARE @TotalPRemRow INT
SET @Year = 2000;
WHILE @Year < @MaxYear + 1
BEGIN
	--DECLARE @RecordCount INT;
	SELECT @RecordCount = COUNT(*) FROM ProcessedFQY WHERE FiscalYear = @Year;
	IF(@RecordCount = 0)
	BEGIN
		IF(@Year = 2000)
		BEGIN
				SET @intPRemFlag = 1
				SET @TotalPRemRow = 12
				--Q1
				SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year + 1 AND [Month] = 1;
		END
		ELSE 
		BEGIN
				SET @intPRemFlag = 1
				SET @TotalPRemRow = 12

				SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year - 1 AND [Month] = 12;
		END

		WHILE (@intPRemFlag <= @TotalPRemRow)
		BEGIN
			SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@StartDate)))

			INSERT INTO ProcessedFQY (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month],[MonthName]) VALUES
			--SELECT 1,@Year,DATEADD(DAY,1,@EndDate),DATEADD(MONTH,3,DATEADD(DAY,1,@EndDate))
			(@FisQtr,@Year,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@StartDate)),DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@EndDate)),@MonthNo, UPPER(SUBSTRING(DATENAME(MONTH,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@EndDate))),1,3)))

			SET @intPRemFlag = @intPRemFlag + 1
		END
	END
	SET @Year = @Year + 1
END
--UPDATE ProcessedFQY SET [MonthName] = UPPER(SUBSTRING(DATENAME(MONTH,DATEADD(MONTH, [Month], -1 )),1,3))
END
END
GO
--7
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ComputeMultipleRegression') AND xtype IN (N'P'))
    DROP PROCEDURE ComputeMultipleRegression
GO


CREATE PROCEDURE [ComputeMultipleRegression]  @ValuesQuery varchar(max), @NumX int
AS
BEGIN
SET NOCOUNT ON
--set @ValuesQuery='select top 40 LOS, ARCodingDays, SurgicalDRG, AdmissionSourceCode from _MedicalData'
--set @NumX=3

--print 'Start calc: ' + cast(GETDATE() as varchar)


set @NumX=@NumX+1

IF OBJECT_ID('tempdb..#RegressionData') IS NOT NULL
BEGIN 
     DROP TABLE #RegressionData
END 
IF OBJECT_ID('tempdb..#InverseMatrix') IS NOT NULL
BEGIN 
     DROP TABLE #InverseMatrix
END 


CREATE TABLE #RegressionData (id int identity(1,1), y int)
ALTER TABLE #RegressionData ADD  CONSTRAINT [PK_RegressionTable] PRIMARY KEY CLUSTERED
([ID] ASC)
CREATE INDEX IDX_RegressionDatay ON #RegressionData(y)

DECLARE @i int=2 -- We already have a constant as the first term
DECLARE @j int=1

while @i<=@NumX
BEGIN

	DECLARE @addString varchar(500);
	SET @addString='alter table #RegressionData add x' + cast(@i as varchar) + ' float CREATE INDEX IDX_RegressionDatax' + cast(@i as varchar) + ' ON #RegressionData(x' + cast(@i as varchar) + ')'
	exec(@addString)

	SET @i=@i+1
END

--print 'Begin regression insert: ' + cast(GETDATE() as varchar)

insert into #RegressionData
exec(@ValuesQuery)

alter table #RegressionData add x1 float
update #RegressionData set x1=1

DECLARE @SquareMatrix MatrixTable

DECLARE @rCount int = (select count(*) from #RegressionData)

--Let's multiply the matrix by its transpose
--for i, 1 =1 to numx

DECLARE @SquareMatrixQuery varchar(max)=''

--print 'Start square matrix query generation: ' + cast(GETDATE() as varchar)


set @i=1
while @i <= @NumX
BEGIN
	set @j=@i
	while @j <= @NumX
	BEGIN
		DECLARE @CurCellQuery varchar(max)=''
		DECLARE @CurCellQuery2 varchar(max)=''


		DECLARE @k int=1

		set @CurCellQuery= ' select ' + cast(@i as varchar) + ',' + cast(@j as varchar) + ', sum(x' + cast(@i as varchar) + ' * x' + cast(@j as varchar) + ') from #RegressionData '
		set @CurCellQuery2= ' UNION ALL select ' + cast(@j as varchar) + ',' + cast(@i as varchar) + ', sum(x' + cast(@i as varchar) + ' * x' + cast(@j as varchar) + ') from #RegressionData '
		

		if @i<>@j
		BEGIN
			SET @SquareMatrixQuery= @SquareMatrixQuery + @CurCellQuery + @CurCellQuery2
		END
		ELSE
		BEGIN
			SET @SquareMatrixQuery= @SquareMatrixQuery + @CurCellQuery
		END

		if @i<@NumX or @j<@NumX
		BEGIN
			SET @SquareMatrixQuery = @SquareMatrixQuery + ' UNION ALL '
		END


		set @j=@j+1
	END

	set @i=@i+1
END


--print 'Start square matrix insert: ' + cast(GETDATE() as varchar)
insert into @SquareMatrix
exec(@SquareMatrixQuery)

--Now we need to find the inverse of the square matrix

--First thing we need to do is find the determinant
DECLARE @Determinant float=0

DECLARE @dTable MatrixTable;

insert into @dTable
select * from @SquareMatrix

--print 'Start find determinant: ' + cast(GETDATE() as varchar)

SET @Determinant=(select dbo.FindDeterminant(@dTable))


CREATE TABLE #inverseMatrix (x int, y int, value float)

--print 'Start find inverse: ' + cast(GETDATE() as varchar)
insert into #inverseMatrix
select * from dbo.FindMatrixInverse(@dTable)


DECLARE @Betas MatrixTable

set @i=1
set @j=1

--print 'Start beta query generation: ' + cast(GETDATE() as varchar)
while @i<=@NumX
BEGIN
	while @j<=@rCount
	BEGIN
		DECLARE @BetaCellQuery varchar(max)='select ' + cast(@i as varchar) + ', ' + cast(@j as varchar) + ', '
		DECLARE @BetaJoin varchar(max)=' '
		set @k=1
		while @k<=@NumX
		BEGIN
			set @BetaCellQuery = @BetaCellQuery + ' i' + cast(@k as varchar) + '.value*r.x' + cast(@k as varchar)

			if @k<@NumX
			BEGIN
				set @BetaCellQuery = @BetaCellQuery + ' + '
			END

			set @BetaJoin= @BetaJoin + ' inner join #inverseMatrix i' + cast(@k as varchar) + ' on i' + cast(@k as varchar) + '.x=' + cast(@k as varchar) + ' and i' + cast(@k as varchar) + '.y=' + cast(@i as varchar) + ' '

			set @k=@k+1
		END

		set @BetaCellQuery = @BetaCellQuery + ' from #RegressionData r ' + @BetaJoin + ' where r.id=' + cast(@j as varchar)

		--print @BetaCellQuery

		insert into @Betas
		exec(@BetaCellQuery)

		set @j=@j+1
	END

	set @j=1
	set @i=@i+1
END


--We have found the betas.  Now we need to find the r^2

--Finding r^2 involves finding the correlation matrix and the y correlation vector.  Let's find the correlation matrix first

DECLARE @CorrelMatrix MatrixTable
--print 'Start R^2 calc: ' + cast(GETDATE() as varchar)

SET @i=2
while @i<=@NumX --Ignore the constant x for this
BEGIN
	SET @j=2
	while @j<=@NumX
	BEGIN
		if @i<>@j
		BEGIN

			DECLARE @toRun varchar(max)='select ' +  cast((@i-1) as varchar) + ', ' + cast((@j-1) as varchar) + ', x' + cast(@i as varchar) + ', x' + cast(@j as varchar) + ' from #RegressionData'
			insert into @CorrelMatrix
			exec CorrelationCalculation @QueryToRun=@toRun

		END
		ELSE
		BEGIN
			insert into @CorrelMatrix
			select (@i-1),( @j-1), 1
		END

	
		SET @j=@j+1
	END

	SET @i=@i+1
END

--Now we need to setup the c vector
--print 'Start c vector calc: ' + cast(GETDATE() as varchar)

DECLARE @CMatrix MatrixTable
SET @i=2
while @i<=@NumX
BEGIN
	SET @toRun ='select ' +  cast((@i-1) as varchar) + ', 1, x' + cast(@i as varchar) + ', y from #RegressionData'

	insert into @CMatrix
	exec CorrelationCalculation @QueryToRun=@toRun

	SET @i=@i+1
END

--Now we have the c vector, we need to multiply it by the inverse of the correlation matrix
DECLARE @invCorrelMatrix MatrixTable

--print 'Start inverse correl calc: ' + cast(GETDATE() as varchar)
insert into @invCorrelMatrix
select * from dbo.FindMatrixInverse(@CorrelMatrix)

DECLARE @cProductMatrix MatrixTable

--print 'Start c product calc: ' + cast(GETDATE() as varchar)
SET @i=1
while @i<@NumX
BEGIN
	insert into @cProductMatrix
	select @i, 1, sum(cm.Value*c.Value) from @CMatrix c 
	inner join @invCorrelMatrix cm on c.x=cm.y
	where cm.x=@i

	SET @i=@i+1
END

--Now we need to finish the R^2 calculation

DECLARE @R2 float = (select sum(v.Value*c.Value) from @CMatrix c inner join @cProductMatrix v on v.x=c.x)

DECLARE @BetaVector MatrixTable

--print 'Start beta vector calc: ' + cast(GETDATE() as varchar)

insert into @BetaVector
select 1, b.x, sum(b.Value*r.y) as Beta from @Betas b
inner join #RegressionData r on b.y=r.id
group by b.x





--We've Found R^2, now we need to find the standard error

ALTER TABLE #RegressionData add Residual float


DECLARE @residuals MatrixTable

DECLARE @ResidualQuery varchar(max)='update #RegressionData set Residual= y - '

--print 'Start residuals calc: ' + cast(GETDATE() as varchar)
insert into @residuals
select 1, id, y from #RegressionData

set @i=1
while @i<=@NumX
BEGIN
	DECLARE @curBeta float = (select Value from @BetaVector where y=@i)

	SET @ResidualQuery = @ResidualQuery + ' x'+ cast(@i as varchar) + '*' + cast(@curBeta as varchar) + ' '

	if @i<@NumX
	BEGIN
		SET @ResidualQuery= @ResidualQuery + ' - '
	END

	SET @i=@i+1
END

exec(@ResidualQuery)


--We've found the residuals, now we need to find the standard error

--print 'Start standard error calc: ' + cast(GETDATE() as varchar)
DECLARE @StdErr float=0
DECLARE @divisor int=(select count(*) from #RegressionData)-@Numx

SET @StdErr = (select sqrt(sum(Residual*Residual)/@divisor) from #RegressionData)


--Now we need to find the standard errors for each of the x terms

DECLARE @Errors MatrixTable
--print 'Start beta errors calc: ' + cast(GETDATE() as varchar)

insert into @Errors
select 1, i.y, sqrt(@StdErr*@StdErr*i.Value) from #inverseMatrix i inner join @BetaVector b on b.y=i.y where i.x=i.y

--select * from @BetaVector
--select * from @Errors

DECLARE @RetTable table (id int identity(1,1), Beta float, RSquared float, StdError float, BetaError float, BetaUpper Float, BetaLower float)

--print 'Start final calc: ' + cast(GETDATE() as varchar)
insert into @RetTable(Beta)
select sum(b.Value*r.y) as Beta from @Betas b
inner join #RegressionData r on b.y=r.id
group by b.x

update @RetTable set RSquared = @R2 where id=1
update @RetTable set StdError = @StdErr where id=1

update @RetTable set BetaError = Value from @Errors where id=y

update @RetTable set BetaUpper = Beta + 1.96*BetaError
update @RetTable set BetaLower = Beta - 1.96*BetaError

select * from @RetTable


drop table #RegressionData
drop table #inverseMatrix
--print 'End calc: ' + cast(GETDATE() as varchar)

END


GO
--8

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ConfigurationTest') AND xtype IN (N'P'))
    DROP PROCEDURE ConfigurationTest
GO


CREATE PROCEDURE [ConfigurationTest] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	DECLARE @TestNum int;
	DECLARE @TestName nvarchar(max);
	DECLARE @TestQuery nvarchar(max);
	DECLARE @TestStoredProcedure nvarchar(max)
	CREATE TABLE #Results (TestNumber int, TestName nvarchar(max), ErrorMessage nvarchar(max), ErrorInformation nvarchar(max))

	DECLARE TestCursor cursor for
	select id, TestCaseName, TestCaseQuery, TestStoredProcedure from ConfigurationTestCases where isDeleted=0
	OPEN TestCursor
	FETCH NEXT FROM TestCursor
	INTO @TestNum, @TestName, @TestQuery, @TestStoredProcedure
	while @@FETCH_STATUS = 0
		BEGIN

		if @TestQuery is not null
			BEGIN
			DECLARE @QueryToRun nvarchar(max)

			SET @QueryToRun = 'Insert into #Results select ' + cast(@TestNum as nvarchar) + ', ''' + @TestName + ''', ErrorMessage, ErrorInformation from ( ' + @TestQuery + ') A';

			print @QueryToRun
			exec sp_executesql @QueryToRun
			END

		if @TestStoredProcedure is not null
			BEGIN
			SET @QueryToRun = 'exec ' + @TestStoredProcedure + ' @TestNumber=' + cast(@TestNum as nvarchar)  + ', @TestName=''' + @TestName + ''''

			print @QueryToRun
			exec sp_executesql @QueryToRun
			END


		FETCH NEXT FROM TestCursor
		INTO @TestNum, @TestName, @TestQuery, @TestStoredProcedure
		END

	Close TestCursor
	Deallocate TestCursor

	select * from #Results
	RETURN

	Drop Table #Results
END


GO
--9
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CopyOverAggregation') AND xtype IN (N'P'))
    DROP PROCEDURE CopyOverAggregation
GO



CREATE PROCEDURE [CopyOverAggregation] @DEBUG bit=0
AS
BEGIN
	SET NOCOUNT ON;
BEGIN TRY

DECLARE @CURQUERYFORERROR nvarchar(max)

DECLARE @VIEWNAME nvarchar(max)

DECLARE ViewCursor Cursor for 
select distinct tablename + '_VIEW' from Dimension where IsDeleted=0
OPEN ViewCursor
Fetch next from ViewCursor 
into @VIEWNAME
while @@FETCH_STATUS = 0
	BEGIN
	
	DECLARE @DROPVIEWSQL nvarchar(max)

	IF OBJECT_ID ('dbo.' + @VIEWNAME, 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)




		END

	IF OBJECT_ID ('dbo.' + @VIEWNAME + '_NEW', 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME + '_New'
		print @DROPVIEWSQL
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)


		print 'After Rename'

		END

	Fetch next from ViewCursor 
	into @VIEWNAME
	END

CLOSE ViewCursor
DEALLOCATE ViewCursor

DECLARE @TABLENAME nvarchar(max)
DECLARE ValueCursor Cursor for 
select TableName + 'Value' from DynamicDimension_New
OPEN ValueCursor
Fetch next from ValueCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPVALUESQL nvarchar(max)
		SET @DROPVALUESQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPVALUESQL
		exec(@DROPVALUESQL)
		END

	Fetch next from ValueCursor 
	into @TABLENAME
	END

CLOSE ValueCursor
DEALLOCATE ValueCursor

DECLARE BaseValueCursor Cursor for 
select distinct DimensionValueTableName from DynamicDimension_New
OPEN BaseValueCursor
Fetch next from BaseValueCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPBaseValueSQL nvarchar(max)
		SET @DROPBaseValueSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPBaseValueSQL
		exec(@DROPBaseValueSQL)
		END

	Fetch next from BaseValueCursor 
	into @TABLENAME
	END

CLOSE BaseValueCursor
DEALLOCATE BaseValueCursor


DECLARE DynamicCursor Cursor for 
select TableName from DynamicDimension_New
OPEN DynamicCursor
Fetch next from DynamicCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPDynamicSQL nvarchar(max)
		SET @DROPDynamicSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPDynamicSQL
		exec(@DROPDynamicSQL)
		END

	Fetch next from DynamicCursor 
	into @TABLENAME
	END

CLOSE DynamicCursor
DEALLOCATE DynamicCursor


DECLARE BaseCursor Cursor for 
select distinct Dimensiontablename + '_Base' from DynamicDimension_New
OPEN BaseCursor
Fetch next from BaseCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPBaseSQL nvarchar(max)
		SET @DROPBaseSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPBaseSQL
		exec(@DROPBaseSQL)
		END

	Fetch next from BaseCursor 
	into @TABLENAME
	END

CLOSE BaseCursor
DEALLOCATE BaseCursor

--Clear out foreignkeys on MeasureValue and DimensionValue (for #528)
CREATE TABLE #Keys
(
PKTABLE_QUALIFIER nvarchar(50),
PKTABLE_OWNER nvarchar(50),
PKTABLE_NAME nvarchar(50),
PKCOLUMN_NAME nvarchar(50),
FKTABLE_QUALIFIER nvarchar(50),
FKTABLE_OWNER nvarchar(50),
FKTABLE_NAME nvarchar(50),
FKCOLUMN_NAME nvarchar(50),
KEY_SEQ int,
UPDATE_RULE int,
DELETE_RULE int,
FK_NAME nvarchar(255),
PK_NAME nvarchar(255),
DEFERRABILITY int
)
insert #Keys exec sp_fkeys @pktable_name='DimensionValue'

insert #Keys exec sp_fkeys @pktable_name='MeasureValue'

DECLARE @FKTABLE nvarchar(255);
DECLARE @FKNAME nvarchar(255);

DECLARE Constraint_Cursor CURSOR FOR
select FKTABLE_NAME, FK_NAME from #Keys
OPEN Constraint_Cursor
FETCH NEXT FROM Constraint_Cursor
INTO @FKTABLE, @FKNAME
while @@FETCH_STATUS = 0
        Begin

        DECLARE @DROPCONSTRAINT nvarchar(1000);
        SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;

        SET @CURQUERYFORERROR=@DROPCONSTRAINT
        if @DEBUG=1
        BEGIN
            print @DROPCONSTRAINT
        END
        insert into logging select getDate(), left('Start: ' + @DROPCONSTRAINT,1000)
        execute sp_executesql @DROPCONSTRAINT
        insert into logging select getDate(), left('End: ' + @DROPCONSTRAINT,1000)
        --print @DROPCONSTRAINT
        --print '------------------------'

        FETCH NEXT FROM Constraint_Cursor
        INTO @FKTABLE, @FKNAME
        End
Close Constraint_Cursor
Deallocate Constraint_Cursor





IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue]') AND TYPE IN (N'U'))
	BEGIN
	drop table MeasureValue
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimensionValue]') AND TYPE IN (N'U'))
	BEGIN
	drop table DimensionValue
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension]') AND TYPE IN (N'U'))
	BEGIN
	drop table DynamicDimension
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.DynamicDimension_New', 'DynamicDimension'
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimensionValue_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.DimensionValue_New', 'DimensionValue'
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.MeasureValue_New', 'MeasureValue'
	END


DECLARE DynamicRenameCursor Cursor for 
select TableName from DynamicDimension
union
select DimensionValueTableName from DynamicDimension
union
select distinct DimensionTableName + '_Base' from DynamicDimension
OPEN DynamicRenameCursor
Fetch next from DynamicRenameCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @RenameDynamicSQL nvarchar(max)
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_New]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_New'', ''' + @TABLENAME + ''''
		SET @CURQUERYFORERROR=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_NewValue]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_NewValue'', ''' + @TABLENAME + 'Value'''
		SET @CURQUERYFORERROR=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	Fetch next from DynamicRenameCursor 
	into @TABLENAME
	END

CLOSE DynamicRenameCursor
DEALLOCATE DynamicRenameCursor


DECLARE @QueryToRun varchar(max)
DECLARE ViewCreateCursor Cursor for
select QueryToRun from AggregationQueries
OPEN ViewCreateCursor
FETCH next from ViewCreateCursor
into @QueryToRun
while @@FETCH_STATUS = 0
	BEGIN

	SET @CURQUERYFORERROR=@QueryToRun
	exec(@QueryToRun)

	FETCH next from ViewCreateCursor
	into @QueryToRun
	END
CLOSE ViewCreateCursor
DEALLOCATE ViewCreateCursor

END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CopyOverAggregation',@CURQUERYFORERROR);
	THROW;
END CATCH
END


GO
--10

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[CopyOverAggregationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[CopyOverAggregationPartial]  
END
GO

CREATE PROCEDURE [dbo].[CopyOverAggregationPartial]  @DEBUG bit = 0
AS
BEGIN

--Steps
--1)  Update values in DimensionValue/Add New ones
--2)  Update values in MeasureValue/Add New ones
--3)  Drop extended columns that aren't needed anymore
--4)  Drop extended value tables that aren't needed anymore
--5)  Drop extended tables that aren't needed anymore
--6)  Drop basevalue tables that aren't needed anymore
--7)  Remove values from extended value tables that aren't needed anymore
--8)  Remove values from extended tables that aren't needed anymore
--9)  Remove values from MeasureValue that aren't needed anymore
--10)  Create any new extended tables
--11)  Create any new extended value tables
--12)  Update/add values to extended tables
--13)  Update/add values to extended value tables
--14)  Update/add rows to _Base tables
--15)  Update/add rows to _Base_Valuex tables
--16)  Update/add/remove rows to DynamicDimension
--17)  Drop views
--18)  Create views
--19)  Mark everything as clean


DECLARE @CurQueryForError nvarchar(max)
BEGIN TRY


--1)  Update values in DimensionValue/Add New ones
update DimensionValue
set value=v, DisplayValue=dv, OrderValue=ov from
(select id as did, value as v, DisplayValue as dv, OrderValue as ov from DimensionValue_New) A
where id=did

SET IDENTITY_INSERT dbo.DimensionValue ON

insert into DimensionValue (id, DimensionID, Value, DisplayValue, OrderValue)
select id, DimensionID, Value, DisplayValue, OrderValue from DimensionValue_New
where id not in (select id from DimensionValue)

SET IDENTITY_INSERT dbo.DimensionValue OFF

--2)  Update values in MeasureValue/Add New ones
update MeasureValue
set Measure=m, DimensionValue=dv, Value=v, RecordCount=rc from
(select id as did, Measure as m, dimensionValue as dv, value as v, RecordCount as rc from MeasureValue_New) A
where Measure=m and DimensionValue=dv

SET IDENTITY_INSERT dbo.MeasureValue ON

insert into MeasureValue (id, Measure, DimensionValue, Value, RecordCount)
select id, Measure, DimensionValue, Value, RecordCount from MeasureValue_New m1
where not exists (select m2.id from MeasureValue m2 where m2.Measure=m1.Measure and m2.DimensionValue=m1.DimensionValue)

SET IDENTITY_INSERT dbo.MeasureValue OFF

--3)  Drop extended columns that aren't needed anymore
--4)  Drop extended value tables that aren't needed anymore
--5)  Drop extended tables that aren't needed anymore
DECLARE @ExtendedToDrop table (TableName nvarchar(max), DimensionValueTableName nvarchar(max))

insert into @ExtendedToDrop
select TableName, DimensionValueTableName from DynamicDimension where DimensionTableName in (select DimensionTableName from DynamicDimension_New)
and TableName not in (select TableName from DynamicDimension_New)

while (select count(*) from @ExtendedToDrop) > 0
BEGIN
	DECLARE @TableNameToDrop nvarchar(max)= (select top 1 TableName from @ExtendedToDrop)
	DECLARE @DimensionValueTableName nvarchar(max)= (select DimensionValueTableName from @ExtendedToDrop where TableName=@TableNameToDrop)

	--Three steps here
	--1) Drop the Column off of the value table
	--2) Drop the extended value table
	--3) Drop the extended table

	SET @CurQueryForError='Alter table ' + @DimensionValueTableName + ' DROP COLUMN ' + @TableNameToDrop
	exec('Alter table ' + @DimensionValueTableName + ' DROP COLUMN ' + @TableNameToDrop)
	SET @CurQueryForError='DROP TABLE ' + @TableNameToDrop + 'Value'
	exec('DROP TABLE ' + @TableNameToDrop + 'Value')
	SET @CurQueryForError='Drop table ' + @TableNameToDrop
	exec('Drop table ' + @TableNameToDrop)



	delete from @ExtendedToDrop where TableName=@TableNameToDrop
END


--6)  Drop basevalue tables that aren't needed anymore
delete from @ExtendedToDrop

insert into @ExtendedToDrop
select DimensionValueTableName, '' from DynamicDimension where DimensionValueTableName not in
(select DimensionValueTableName from DynamicDimension_New) and DimensionTableName in
(select DimensionTableName from DynamicDimension_New)

while (select count(*) from @ExtendedToDrop) > 0
BEGIN
	SET @TableNameToDrop = (select top 1 TableName from @ExtendedToDrop)

	SET @CurQueryForError='Drop table ' + @TableNameToDrop
	exec('Drop table ' + @TableNameToDrop)


	delete from @ExtendedToDrop where TableName=@TableNameToDrop
END



--7)  Remove values from extended value tables that aren't needed anymore
--8)  Remove values from extended tables that aren't needed anymore
--We need to find what values do not exist in the base tables anymore
DECLARE @ToDelete table(id int identity(1,1), TableName nvarchar(max), DimensionTableName nvarchar(max), DimensionValueTableName nvarchar(max))

insert into @ToDelete
select Tablename, DimensionTableName, DimensionValueTableName from DynamicDimension_New where containsdatedimension=1

while (select count(*) from @ToDelete)>0
BEGIN
	DECLARE @TableName nvarchar(max)
	--DECLARE @DimensionValueTableName nvarchar(max)
	DECLARE @MinID int = (select min(id) from @ToDelete)

	SET @TableName = (select TableName from @ToDelete where id=@MinID)
	SET @DimensionValueTableName = (select DimensionValueTableName from @ToDelete where id=@MinID)

	DECLARE @DeleteSubQuery nvarchar(max)='select id from ' + @TableName + ' where id not in (select ' + @TableName + ' from ' + @DimensionValueTableName + ' union select ' + @TableName + ' from ' + @DimensionValueTableName + '_New)'

	--Delete from the value table and then the regular extended table
	SET @CurQueryForError='delete from ' + @TableName + 'Value where ' + @TableName + ' in ( ' + @DeleteSubQuery + ')'
	exec('delete from ' + @TableName + 'Value where ' + @TableName + ' in ( ' + @DeleteSubQuery + ')')
	SET @CurQueryForError='delete from ' + @TableName + ' where id in ( ' + @DeleteSubQuery + ')'
	exec('delete from ' + @TableName + ' where id in ( ' + @DeleteSubQuery + ')')

	delete from @ToDelete where id=@MinID
END


--9)  Remove values from MeasureValue that aren't needed anymore
delete from MeasureValue where id in (
select m1.ID from MeasureValue m1
inner join DimensionValue d1 on m1.DimensionValue=d1.id
inner join Dimension d2 on d1.DimensionID=d2.id
where d2.IsDeleted=1)

--Also remove unneeded dimensionvalue entries
delete from DimensionValue where DimensionID in (select id from dimension where IsDeleted=1)


--10)  Create any new extended tables
--11)  Create any new extended value tables
DECLARE DynamicRenameCursor Cursor for 
select tablename from DynamicDimension_New
where tablename not in (select tablename from DynamicDimension)
OPEN DynamicRenameCursor
Fetch next from DynamicRenameCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @RenameDynamicSQL nvarchar(max)
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_New]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_New'', ''' + @TABLENAME + ''''
		SET @CurQueryForError=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_NewValue]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_NewValue'', ''' + @TABLENAME + 'Value'''
		SET @CurQueryForError=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	Fetch next from DynamicRenameCursor 
	into @TABLENAME
	END

CLOSE DynamicRenameCursor
DEALLOCATE DynamicRenameCursor

--12)  Update/add values to extended tables
--13)  Update/add values to extended value tables


--First add the values to the extended tables
DECLARE @TablesToUpdate table (id int identity(1,1), tablename nvarchar(max), dimensions int)


insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	DECLARE @Dimensions int = (select dimensions from @TablesToUpdate where id=@MinID)


	DECLARE @ToInsert nvarchar(max) = 'Insert into ' + @TableName + ' select '

	DECLARE @d int=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd' + cast(@d as nvarchar) 

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ', '
		END

		SET @d=@d+1
	END

	SET @ToInsert= @ToInsert + ' from ' + @TableName + '_New where id in ( select d1.id from ' + @TableName + '_New d1 left outer join ' + @TableName + ' d2 on '

	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd1.d' + cast(@d as nvarchar) + '=d2.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END

	SET @ToInsert = @ToInsert + ' where d2.id is null)'

	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--Add the values to the value tables
delete from @TablesToUpdate
insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	SET @Dimensions = (select dimensions from @TablesToUpdate where id=@MinID)


	SET @ToInsert  = 'Insert into ' + @TableName + 'Value select d3.id, Measure, Value, Rows from ' + @TableName + '_NewValue d1 ' +
								' inner join ' + @TableName + '_New d2 on d1.' + @TableName + '=d2.id ' +
								' inner join ' + @TableName + ' d3 on '
	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd2.d' + cast(@d as nvarchar) + '=d3.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END


	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--Now we need to update the values in the value table
delete from @TablesToUpdate
insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	SET @Dimensions = (select dimensions from @TablesToUpdate where id=@MinID)


	SET @ToInsert  = 'Update ' + @TableName + 'Value set Measure=m, Value=v, Rows=r from (select d3.id as did, Measure as m, Value as v, Rows as r from ' + @TableName + '_NewValue d1 ' +
								' inner join ' + @TableName + '_New d2 on d1.' + @TableName + '=d2.id ' +
								' inner join ' + @TableName + ' d3 on '

	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd2.d' + cast(@d as nvarchar) + '=d3.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END

	SET @ToInsert=@ToInsert + ') A where id=did'


	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--14)  Update/add rows to _Base tables
--Add new rows first
DECLARE @BaseInsert table (id int identity(1,1), tablename nvarchar(max))

insert into @BaseInsert
select distinct DimensionTableName from DynamicDimension_New

while (select count(*) from @BaseInsert) > 0
BEGIN
	DECLARE @curId int = (select min(id) from @BaseInsert)

	DECLARE @CurTable nvarchar(max)=(select tablename from @BaseInsert where id=@curId)

	DECLARE @ToRun nvarchar(max)=replace('insert into %TABLENAME%_Base select * from %TABLENAME%_Base_New where id not in (select id from %TABLENAME%_Base)','%TABLENAME%',@CurTable)

	SET @CurQueryForError=@ToRun
	exec(@ToRun)

	delete from @BaseInsert where id=@curId
END



--Now we need to update values in the _Base tables
DECLARE @BaseUpdate table(id int identity(1,1), tablename nvarchar(max), columnName nvarchar(max), alias nvarchar(max))

insert into @BaseUpdate
select distinct d1.DimensionTableName, 'DimensionValue' + cast(d2.id as nvarchar), 'd' + cast(d2.id as nvarchar) from DynamicDimension_New d1
inner join Dimension d2 on d1.DimensionTableName=d2.TableName 
where d2.IsDeleted=0
order by d1.DimensionTableName

DECLARE @Columns nvarchar(max)=''
DECLARE @ColumnsSub nvarchar(max)=''
SET @curID = (select min(id) from @BaseUpdate)
SET @CurTable =(select tablename from @BaseUpdate where id=@curID)

DECLARE @BaseColumns table(id int identity(1,1), tablename nvarchar(max), columnNames nvarchar(max), subColumns nvarchar(max))

while @curID <= (select max(id) from @BaseUpdate) + 1
BEGIN

	DECLARE @RowTable nvarchar(max)=(select tablename from @BaseUpdate where id=@curID)

	if @RowTable<>@CurTable or @CurID=(select max(id) from @BaseUpdate)
	BEGIN
		insert into @BaseColumns select @CurTable, @Columns, @ColumnsSub

		SET @CurTable=@RowTable
		SET @Columns=''
		set @ColumnsSub=''
	END

	if len(@Columns)>0
	BEGIN

		set @Columns = @Columns + ', '
		set @ColumnsSub = @ColumnsSub + ', '
	END

	SET @Columns = @Columns + (select columnName + '=' + alias from @BaseUpdate where id=@curID)
	SET @ColumnsSub = @ColumnsSub + (select columnName + ' as ' + alias from @BaseUpdate where id=@curID)

	SET @curID=@CurID+1
END


SET @curID=(select min(id) from @BaseColumns)

while @curID<=(select max(id) from @BaseColumns)
BEGIN

	DECLARE @QueryToRun nvarchar(max)='update %TABLENAME%_Base
				set %COLUMNS%
				from (select id as i, %ALIAS% from %TABLENAME%_Base_New) A
				where id=i and Dirty=1'

	SET @Columns = (select columnNames from @BaseColumns where id=@curID)
	SET @ColumnsSub = (select subColumns from @BaseColumns where id=@curID)
	set @CurTable = (select tablename from @BaseColumns where id=@curID)

	SET @QueryToRun = Replace(@QueryToRun,'%TABLENAME%',@CurTable)
	SET @QueryToRun = Replace(@QueryToRun,'%COLUMNS%',@Columns)
	SET @QueryToRun = Replace(@QueryToRun,'%ALIAS%',@ColumnsSub)

	--print @QueryToRun

	SET @CurQueryForError=@QueryToRun

	exec(@QueryToRun)

	SET @curID=@curID+1
END


--15)  Update/add rows to _Base_Valuex tables
--Let's add the new rows first
DECLARE @ExtendedInsertData table(id int identity(1,1), QueryToRun nvarchar(max))

insert into @ExtendedInsertData
select distinct 'insert into ' + DimensionValueTableName + ' (' + DimensionTableName + ') select ' + DimensionTableName + ' from ' + DimensionValueTableName + '_New where ' +
 DimensionTableName + ' not in(
select ' + DimensionTableName + ' from ' + DimensionValueTableName + ')' from DynamicDimension_New


while (select count(*) from @ExtendedInsertData)>0
BEGIN
	
	SET @CurID=(select min(id) from @ExtendedInsertData)
	SET @ToRun=(select QueryToRun from @ExtendedInsertData where id=@CurID)

	SET @CurQueryForError=@ToRun
	exec(@ToRun)

	delete from @ExtendedInsertData where id=@CurID

END


--Now update the values



DECLARE @BaseUpdate2 table(id int identity(1,1), tablename nvarchar(max), columnName nvarchar(max), alias nvarchar(max), basename nvarchar(max))

insert into @BaseUpdate2
select d1.DimensionValueTableName, d1.tablename, d1.tablename + '_updated', DimensionTableName from DynamicDimension_New d1
order by d1.DimensionValueTableName

SET @Columns =''
SET @ColumnsSub =''
DECLARE @BaseName nvarchar(max)=''
SET @curID = (select min(id) from @BaseUpdate2)
SET @CurTable =(select tablename from @BaseUpdate2 where id=@curID)

DECLARE @BaseColumns2 table(id int identity(1,1), tablename nvarchar(max), columnNames nvarchar(max), subColumns nvarchar(max), basename nvarchar(max))

SET @BaseName=(select basename from @BaseUpdate2 where id=@curID)


while @curID <= (select max(id) from @BaseUpdate2) + 1
BEGIN

	SET @RowTable =(select tablename from @BaseUpdate2 where id=@curID)

	if @RowTable<>@CurTable or @CurID=(select max(id) from @BaseUpdate)
	BEGIN
		insert into @BaseColumns2 select @CurTable, @Columns, @ColumnsSub, @BaseName

		SET @CurTable=@RowTable
		SET @Columns=''
		set @ColumnsSub=''
		SET @BaseName=(select basename from @BaseUpdate2 where id=@curID)
	END

	if len(@Columns)>0
	BEGIN

		set @Columns = @Columns + ', '
		set @ColumnsSub = @ColumnsSub + ', '
	END

	SET @Columns = @Columns + (select columnName + '= isnull(' + alias + ','+ columnName + ') ' from @BaseUpdate2 where id=@curID)
	SET @ColumnsSub = @ColumnsSub + (select columnName + ' as ' + alias from @BaseUpdate2 where id=@curID)

	SET @curID=@CurID+1
END


SET @curID=(select min(id) from @BaseColumns)

while @curID<=(select max(id) from @BaseColumns)
BEGIN

	SET @QueryToRun ='update %TABLENAME%
				set %COLUMNS%
				from (select %BASENAME% as i, %ALIAS% from %TABLENAME%_New) A
				where %BASENAME%=i'

	SET @Columns = (select columnNames from @BaseColumns2 where id=@curID)
	SET @ColumnsSub = (select subColumns from @BaseColumns2 where id=@curID)
	set @CurTable = (select tablename from @BaseColumns2 where id=@curID)
	set @BaseName = (select basename from @BaseColumns2 where id=@curID)

	SET @QueryToRun = Replace(@QueryToRun,'%TABLENAME%',@CurTable)
	SET @QueryToRun = Replace(@QueryToRun,'%COLUMNS%',@Columns)
	SET @QueryToRun = Replace(@QueryToRun,'%ALIAS%',@ColumnsSub)
	SET @QueryToRun = Replace(@QueryToRun,'%BASENAME%',@BaseName)

	--print @QueryToRun

	SET @CurQueryForError=@QueryToRun
	exec(@QueryToRun)

	SET @curID=@curID+1
END



--16)  Update/add/remove rows to DynamicDimension
insert into DynamicDimension
select TableName, Dimensions, DimensionTableName, ComputeAllValues, DimensionValueTableName, ContainsDateDimension from DynamicDimension_New where TableName not in (select TableName from DynamicDimension)

delete from DynamicDimension where id in(
select id from DynamicDimension where tablename not in (select tablename from DynamicDimension_New) and DimensionTableName in (select DimensionTableName from DynamicDimension_New)
)



--17)  Drop views
DECLARE @VIEWNAME nvarchar(max)

DECLARE ViewCursor Cursor for 
select distinct Dimensiontablename + '_VIEW' from DynamicDimension_New OPEN ViewCursor
Fetch next from ViewCursor 
into @VIEWNAME
while @@FETCH_STATUS = 0
	BEGIN
	
	DECLARE @DROPVIEWSQL nvarchar(max)

	IF OBJECT_ID ('dbo.' + @VIEWNAME, 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)




		END

	IF OBJECT_ID ('dbo.' + @VIEWNAME + '_NEW', 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME + '_New'
		print @DROPVIEWSQL
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)


		print 'After Rename'

		END

	Fetch next from ViewCursor 
	into @VIEWNAME
	END

CLOSE ViewCursor
DEALLOCATE ViewCursor



--18)  Create views
SET @QueryToRun=''
DECLARE ViewCreateCursor Cursor for
select QueryToRun from AggregationQueries
OPEN ViewCreateCursor
FETCH next from ViewCreateCursor
into @QueryToRun
while @@FETCH_STATUS = 0
	BEGIN

	SET @CURQUERYFORERROR=@QueryToRun
	exec(@QueryToRun)

	FETCH next from ViewCreateCursor
	into @QueryToRun
	END
CLOSE ViewCreateCursor
DEALLOCATE ViewCreateCursor


--19)  Mark everything as clean
SET @QueryToRun=''
DECLARE CleanCursor Cursor for
select distinct 'update ' + DimensionTableName + '_Base set dirty=0' from DynamicDimension_New
OPEN CleanCursor
FETCH next from CleanCursor
into @QueryToRun
while @@FETCH_STATUS = 0
	BEGIN

	SET @CURQUERYFORERROR=@QueryToRun
	exec(@QueryToRun)

	FETCH next from CleanCursor
	into @QueryToRun
	END
CLOSE CleanCursor
DEALLOCATE CleanCursor

END TRY

BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CopyOverAggregationPartial',@CURQUERYFORERROR);
	THROW;
END CATCH


END

GO

--11
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CoRelationReportGraphResults') AND xtype IN (N'P'))
    DROP PROCEDURE CoRelationReportGraphResults
GO


CREATE PROCEDURE [CoRelationReportGraphResults]  
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,
@SubDashboardMainDimensionTable int = 0,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511


AS
BEGIN
SET NOCOUNT ON;

DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX)
SET @InnerJoin1 = ''; SET @Count= 1

DECLARE @HeatMapQuery NVARCHAR(MAX), @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere NVARCHAR(1000)
SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = '';SET @HeatMapWhere = ''
SET @HeatMapQuery = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1'
SET @HeatMapQuery =  @HeatMapQuery + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 

DECLARE Column_Cursor CURSOR FOR
SELECT A.Dimensionid, A.AxisName, '[' + Replace(D.Name,' ','') + ']' DimensionName,D.IsDateDimension FROM ReportGraph G
INNER JOIN ReportAxis A ON G.id = A.ReportGraphId
INNER JOIN Dimension D ON A.Dimensionid = D.id  
WHERE G.id = @ReportGraphID
ORDER BY A.AxisName
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			IF ((@SubDashboardOtherDimensionTable > 0) AND (@SubDashboardMainDimensionTable = @Dimensionid))
			BEGIN
				SET @Dimensionid = @SubDashboardOtherDimensionTable
			END
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			IF(@DATEFIELD = 'D'  + CAST(@tempIndex AS NVARCHAR))
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
				SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  

				SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 'd2.d' + CAST(@tempIndex AS NVARCHAR)
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR)  

				SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 'd2.d' + CAST(@tempIndex AS NVARCHAR)
			END
			
			
			
			SET @Count = @Count + 1

	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	END
Close Column_Cursor
Deallocate Column_Cursor

SET @HeatMapGrpuoColumn = @HeatMapDimColumn

/*
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D1',2);
END
SET @FilterCondition = ISNULL(@FilterCondition,'')
IF(@FilterCondition != '')
SET @FilterCondition = ' WHERE' + REPLACE(@FilterCondition,'#',' AND ')
*/

DECLARE @AggregationType NVARCHAR(20), @MEASURENAME NVARCHAR(200),@SymbolType NVARCHAR(100), @MEASUREID INT, @MEASURETABLENAME NVARCHAR(200), @MeCount INT;
SET @MeCount =1; 
SET @SymbolType = '';


DECLARE Measure_Cursor CURSOR FOR
select MeasureName, Measureid, MeasureTableName,ISNULL(AggregationType,'') AggregationType,ISNULL(SymbolType,'') SymbolType from dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue) ORDER BY ColumnOrder
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
WHILE @@FETCH_STATUS = 0
	Begin
		
		DECLARE @AggregartedMeasure NVARCHAR(200)
		

		IF(@MeCount = 1)
		BEGIN
				SET @AggregartedMeasure = 'd1.value'
				SET @HeatMapDimColumn = @HeatMapDimColumn + ' ,' + @AggregartedMeasure + ','
				SET @HeatMapWhere = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		ELSE
		BEGIN
				SET @AggregartedMeasure = ' d' + CAST(@Count + 1 AS NVARCHAR) + '.Value '
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN ' +@DIMENSIONTABLENAME + 'Value d' + CAST(@Count + 1 AS NVARCHAR) + ' on d1.' + @DIMENSIONTABLENAME + ' = d' + CAST(@Count + 1 AS NVARCHAR) + '.'   + + @DIMENSIONTABLENAME
				SET @HeatMapDimColumn = @HeatMapDimColumn  + @AggregartedMeasure 

				SET @HeatMapWhere = @HeatMapWhere + ' AND d' + CAST(@Count + 1 AS NVARCHAR) + '.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		
		
		SET @Count = @Count + 1
		SET @MeCount = @MeCount + 1
	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor

SET @HeatMapQuery = REPLACE(@HeatMapQuery,'#COLUMNS#',@HeatMapDimColumn)
SET @HeatMapQuery = @HeatMapQuery + @InnerJoin1 + @HeatMapWhere --+ ' Group By ' + @HeatMapGrpuoColumn

DECLARE @CreateTable NVARCHAR(MAX)
SET @CreateTable = N'DECLARE @REPORTGRAPHHM' + CAST(@ReportGraphID AS NVARCHAR) + '  TABLE (d1 int, d2 int, val float);';

DECLARE @Retval table(d1 int, d2 int, val float) 

--Added by kausha somaiya for filter condition and For Exclude column
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	set @HeatMapQuery=@HeatMapQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
END
--End Filtercondition

PRINT @HeatMapQuery;

INSERT INTO  @Retval (d1,d2,val)
exec CorrelationCalculation  @HeatMapQuery ;
select 
		DisplayValue1 = CASE WHEN DD1.IsDateDimension = 1 THEN dbo.GetDatePart(d3.DisplayValue,@ViewByValue,@STARTDATE,@ENDDATE) ELSE d3.DisplayValue END,
		DisplayValue2 = CASE WHEN DD2.IsDateDimension = 1 THEN dbo.GetDatePart(d4.DisplayValue,@ViewByValue,@STARTDATE,@ENDDATE) ELSE d4.DisplayValue END,
		MeasureValue = round(r.val,2)
FROM @Retval R
INNER JOIN DimensionValue d3 ON d3.id = R.d1
LEFT JOIN Dimension DD1 ON DD1.id = d3.DimensionID
INNER JOIN DimensionValue d4 ON d4.id = R.d2
LEFT JOIN Dimension DD2 ON DD2.id = d4.DimensionID
where 
d3.DisplayValue NOT IN(SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID =@ReportGraphId) AND 
d3.DisplayValue NOT IN (select * from UF_CSVToTable(dbo.RestrictedDimensionValues(d3.dimensionId,@UserId,@RoleId))) AND
d4.DisplayValue NOT IN(SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID =@ReportGraphId) AND
d4.DisplayValue NOT IN (select * from UF_CSVToTable(dbo.RestrictedDimensionValues(d4.DimensionId,@UserId,@RoleId)))
ORDER BY d3.OrderValue, d4.OrderValue


RETURN
END

GO
--12
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CorrelationCalculation') AND xtype IN (N'P'))
    DROP PROCEDURE CorrelationCalculation
GO


CREATE Procedure [CorrelationCalculation] (@QueryToRun varchar(max))
as
BEGIN
	--Query passed in as value must return two fields (both floats) and no nulls for every row.  Procedure will return null if data is degenerate.
	DECLARE @Retval table(d1 int, d2 int, val float) 
	BEGIN TRY
		--insert into @Retval
		exec('DECLARE @Results table(d1 int, d2 int, val1 float, val2 float); insert into @Results ' + @QueryToRun + ' 	
		
		DECLARE @mu1 table (d1 int, d2 int, val float) 

insert into @mu1 select d1, d2, avg(val1) from @Results group by d1, d2;	

DECLARE @mu2 table (d1 int, d2 int, val float)  

insert into @mu2 select d1, d2, avg(val2) from @Results group by d1, d2; 	

select r.d1, r.d2, sum((val1-m1.val)*(val2- m2.val))/sqrt(sum((val1-m1.val)*(val1-m1.val))*sum((val2-m2.val)*(val2-m2.val))) from @Results r 

inner join @mu1 m1 on r.d1=m1.d1 and r.d2=m1.d2 
inner join @mu2 m2 on r.d1=m2.d1 and r.d2=m2.d2 
group by r.d1, r.d2');
		--select d1,d2,round(val,2) val from @Retval
		return
	END TRY
	BEGIN CATCH
		select null,null,null 
		
		return 
	END CATCH
END;


GO
--13
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[CreateBaseTableTriggers]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[CreateBaseTableTriggers]  
END
GO

CREATE PROCEDURE [dbo].[CreateBaseTableTriggers]    @DEBUG bit = 0 
AS
BEGIN
	DECLARE @TableList table (id int identity(1,1), TableName nvarchar(max))

	insert into @TableList
	select distinct tablename from Dimension

	DECLARE @c int=(select count(*) from @TableList)

	while @c > 0
	BEGIN
		DECLARE @CurRow int = (select min(id) from @TableList)
		DECLARE @CurTable nvarchar(max) = (select TableName from @TableList where id=@CurRow)

		--We've got the table that we want, now we just need to create the appropriate trigger for that table
		DECLARE @DropQuery nvarchar(max) = 'IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N''[dbo].[' + @CurTable + 'AfterUpdate]''))
			BEGIN
				DROP TRIGGER ' + @CurTable + 'AfterUpdate
			END'
			


			DECLARE @CreateQuery nvarchar(max)='CREATE TRIGGER ' + @CurTable + 'AfterUpdate on dbo.[' + @CurTable + ']
			FOR UPDATE
			AS
				IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = ''dbo'' 
                 AND  TABLE_NAME = ''' + @CurTable + '_Base''))
				BEGIN
					IF NOT EXISTS(SELECT * FROM sys.columns 
								WHERE Name = N''dirty'' AND Object_ID = Object_ID(''' + @CurTable + '_Base''))
					BEGIN
						ALTER TABLE ' + @CurTable + '_Base add dirty bit
					END

					update ' + @CurTable + '_Base set dirty = 1 where id in (select id from inserted)
				END'
			

		--print @DropQuery
		--print @CreateQuery

		--If the table referenced in the dimension table doesn't exist, don't try to create a trigger for it.
		IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = @CurTable))
		BEGIN
			--If there is no base table
			IF(NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = @CurTable + '_Base'))
			BEGIN
				exec ('CREATE TABLE [dbo].[' + @CurTable + '_Base](
						[id] [bigint] NOT NULL,
						[dirty] [bit] NULL
					) ON [PRIMARY]')


			END

			--If the dirty bit doesn't exist, create it
			IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'dirty' AND Object_ID = Object_ID(@CurTable + '_Base'))
			BEGIN
				exec('ALTER TABLE ' + @CurTable + '_Base add dirty bit')
			END

			exec(@DropQuery)
			exec(@CreateQuery)
		END




		delete from @TableList where id=@CurRow
		SET @c = (select count(*) from @TableList)
	END

END

Go
--14

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CreateDynamicColumns') AND xtype IN (N'P'))
    DROP PROCEDURE CreateDynamicColumns
GO


CREATE PROCEDURE [CreateDynamicColumns]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY


DECLARE @COLUMNNAME varchar(max);
DECLARE @CONSTRAINTTABLENAME varchar(max);
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @DIMENSIONVALUETABLENAME varchar(max);
DECLARE @LASTDIMENSIONVALUETABLENAME varchar(max);
DECLARE @CREATEVIEWSQL varchar(max);
DECLARE @CREATEVIEWJOINSQL varchar(max);
DECLARE @LASTTABLENAME varchar(max);
DECLARE @TOTALCREATEVIEWSQL varchar(max)
DECLARE @ADDEDBASENEW bit;


SET @ADDEDBASENEW=0
set @LASTDIMENSIONVALUETABLENAME=null;
SET @LASTTABLENAME=null;


truncate table AggregationQueries;

--We need to drop and then recreate all of the DimensionValueTables
DECLARE DimensionValue_Cursor Cursor for
select TableName, DimensionValueTableName + '_New', DimensionTableName from DynamicDimension_New where containsdatedimension=1 order by DimensionValueTableName, TableName
OPEN DimensionValue_Cursor
FETCH NEXT FROM DimensionValue_Cursor
INTO @COLUMNNAME, @DIMENSIONVALUETABLENAME, @DIMENSIONTABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	--If we have a new DimensionValueTableName
	if @LASTDIMENSIONVALUETABLENAME is null or @LASTDIMENSIONVALUETABLENAME<>@DIMENSIONVALUETABLENAME
		BEGIN

		if @LASTTABLENAME is null or @LASTTABLENAME <> @DIMENSIONTABLENAME
			BEGIN


			if LEN(@CREATEVIEWSQL) > 0
				BEGIN


				DECLARE @ColsToAdd table (ToAdd nvarchar(max))

				Delete from  @ColsToAdd
				insert into @ColsToAdd
				exec ('select TableName + ''_Base_New.[DimensionValue'' + cast(id as varchar) + '']'' from Dimension where TableName = ''' + @LASTTABLENAME + ''' and isDeleted=0')

				DECLARE @ccount int = (select count(*) from @ColsToAdd)

				while @ccount > 0
				BEGIN
					DECLARE @toAdd nvarchar(max) = (select top 1 ToAdd from @ColsToAdd)

					SET @CREATEVIEWSQL = @CREATEVIEWSQL + ',' + @toAdd + ' '

					delete from @ColsToAdd where toAdd=@ToAdd

				set @ccount = (select count(*) from @ColsToAdd)
				END


				SET @TOTALCREATEVIEWSQL = @CREATEVIEWSQL + ' from ' + @LASTTABLENAME + ' '  + @CREATEVIEWJOINSQL

				SET @CURQUERYFORERROR = @TOTALCREATEVIEWSQL
				if @DEBUG=1
				BEGIN
					print @TOTALCREATEVIEWSQL
				END
				exec(@TOTALCREATEVIEWSQL)
				SET @ADDEDBASENEW=0

				insert into AggregationQueries select replace(@TOTALCREATEVIEWSQL,'_New','')

				END

			SET @LASTTABLENAME=@DIMENSIONTABLENAME
			SET @CREATEVIEWSQL = ' CREATE VIEW [dbo].[' + @DIMENSIONTABLENAME + '_VIEW_New] AS SELECT '
			SET @CREATEVIEWJOINSQL=''

			--Need all of the columns in the base table
			DECLARE @CURCOL varchar(max);
			DECLARE @CURTABLE varchar(max);
			DECLARE @FOUNDCOL int;

			SET @FOUNDCOL=0

			DECLARE Col_Cursor Cursor for
			select COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS 
			Open Col_Cursor
			Fetch Next from Col_Cursor
			into @CURCOL, @CURTABLE
			while @@FETCH_STATUS = 0
				BEGIN
				if @CURTABLE=@DIMENSIONTABLENAME
					BEGIN
					
					if left(@CURCOL,14)<>'DimensionValue' -- We don't want the old DimensionValue fields in here
					BEGIN
										
						if @FOUNDCOL=1
							BEGIN
							SET @CREATEVIEWSQL = @CREATEVIEWSQL + ', '
							END

						SET @FOUNDCOL=1

						SET @CREATEVIEWSQL = @CREATEVIEWSQL + ' [' + @DIMENSIONTABLENAME + '].[' + @CURCOL + '] '
						END
					END

				Fetch Next from Col_Cursor
				into @CURCOL, @CURTABLE
				END

			CLOSE Col_Cursor
			DEALLOCATE Col_Cursor

			END

		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME;

		--If the table exists, we need to drop it
		DECLARE @DROPSQL varchar(max);
		SET @DROPSQL='IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ''' + @DIMENSIONVALUETABLENAME + ''' AND TABLE_SCHEMA = ''dbo'')	BEGIN DROP TABLE ' + @DIMENSIONVALUETABLENAME + ' END'

		SET @DROPSQL=  @DROPSQL + ' IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = ''' + @DIMENSIONTABLENAME + '_VIEW_New'' AND TABLE_SCHEMA = ''dbo'')	BEGIN DROP VIEW ' + @DIMENSIONTABLENAME + '_VIEW_New END'
	
		SEt @CURQUERYFORERROR = @DROPSQL
		if @DEBUG=1
		BEGIN
			print @DROPSQL
		END

		exec(@DROPSQL)

		--Now we need to create the table
		DECLARE @CREATESQL varchar(max);

		
		SET @CREATESQL='CREATE TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + '](			[id] [bigint] IDENTITY(1,1) NOT NULL, [' + @DIMENSIONTABLENAME + '] [bigint] NOT NULL 		 CONSTRAINT [PK_' + @DIMENSIONVALUETABLENAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +  '] PRIMARY KEY CLUSTERED 		(			[id] ASC		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]		) ON [PRIMARY] '


		SET @CREATESQL = @CREATESQL + ' ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME +']  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @DIMENSIONTABLENAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + '1] FOREIGN KEY([' + @DIMENSIONTABLENAME + ']) REFERENCES [dbo].[' + @DIMENSIONTABLENAME + '] ([id]) ';

		SET @CREATESQL = @CREATESQL + 
			'CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_BaseTableReference] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']
			(
			[' + @DIMENSIONTABLENAME + '] ASC
			)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY] '



		SET @CURQUERYFORERROR = @CREATESQL

		if @DEBUG=1
		BEGIN
			print @CREATESQL
		END
		exec(@CREATESQL)

		SET @CREATEVIEWJOINSQL = @CREATEVIEWJOINSQL + ' inner join ' + @DIMENSIONVALUETABLENAME + ' on ' + @DIMENSIONVALUETABLENAME + '.' + @DIMENSIONTABLENAME + '=' + @DIMENSIONTABLENAME + '.id '
		if @ADDEDBASENEW=0
		BEGIN

			SET @CREATEVIEWJOINSQL = @CREATEVIEWJOINSQL + ' inner join ' + @DIMENSIONTABLENAME + '_Base_New on ' + @DIMENSIONTABLENAME + '_Base_New.id=' + @DIMENSIONTABLENAME + '.id '
			SET @ADDEDBASENEW=1
		END

		END


	--Now we need to add the column on to the table
	DECLARE @ADDCOLUMNSQL varchar(max);

	SET @ADDCOLUMNSQL = 'ALTER TABLE ' + @DIMENSIONVALUETABLENAME + ' ADD ' + @COLUMNNAME + ' int '

	SET @ADDCOLUMNSQL = @ADDCOLUMNSQL + 
	' ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + ']  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +'] FOREIGN KEY([' + @COLUMNNAME + '])
							REFERENCES [dbo].[' + @COLUMNNAME + '_New] ([id])
							ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + '] CHECK CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +']'

	SET @CURQUERYFORERROR = @ADDCOLUMNSQL

	if @DEBUG=1
	BEGIN
		print @ADDCOLUMNSQL
	END
	exec(@ADDCOLUMNSQL)

	SET @CREATEVIEWSQL = @CREATEVIEWSQL + ', [' + @DIMENSIONVALUETABLENAME + '].[' + @COLUMNNAME + ']'

	FETCH NEXT FROM DimensionValue_Cursor
	INTO @COLUMNNAME, @DIMENSIONVALUETABLENAME, @DIMENSIONTABLENAME
	END

CLOSE DimensionValue_Cursor
DEALLOCATE DimensionValue_Cursor

--Need to add the _Base_New columns in here

Delete from  @ColsToAdd
insert into @ColsToAdd
exec ('select TableName + ''_Base_New.DimensionValue'' + cast(id as varchar) from Dimension where isDeleted=0 and TableName = ''' + @DIMENSIONTABLENAME + '''')

SET @ccount = (select count(*) from @ColsToAdd)

while @ccount > 0
BEGIN
	SET @toAdd  = (select top 1 ToAdd from @ColsToAdd)

	SET @CREATEVIEWSQL = @CREATEVIEWSQL + ',' + @toAdd + ' '

	delete from @ColsToAdd where toAdd=@ToAdd

set @ccount = (select count(*) from @ColsToAdd)
END



if LEN(@CREATEVIEWSQL) > 0
	BEGIN

	SET @TOTALCREATEVIEWSQL = @CREATEVIEWSQL + ' from ' + @DIMENSIONTABLENAME + ' '  + @CREATEVIEWJOINSQL

	SET @CURQUERYFORERROR = @TOTALCREATEVIEWSQL
	if @DEBUG=1
	BEGIN
		print @TOTALCREATEVIEWSQL
	END
	exec(@TOTALCREATEVIEWSQL)
	insert into AggregationQueries select replace(@TOTALCREATEVIEWSQL,'_New','')

	END


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CreateDynamicColumns',@CURQUERYFORERROR);
	THROW;
END CATCH

END




GO
--15
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CustomGraphQuery') AND xtype IN (N'P'))
    DROP PROCEDURE CustomGraphQuery
GO

CREATE PROCEDURE [dbo].[CustomGraphQuery]  
@ReportGraphID INT, 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100',
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@DimensionTableName NVARCHAR(100),
@DateDimensionId int,
@IsDrillDownData bit=0,
@DrillRowValue NVARCHAR(1000) = NULL,
@SortBy NVARCHAR(1000) = NULL,
@SortDirection NVARCHAR(1000) = NULL,
@PageSize INT = NULL,
@PageIndex INT = NULL,
@IsExportAll BIT=1
AS
BEGIN
	DECLARE @DIMENSIONGROUP NVARCHAR(MAX)
	DECLARE @DimensionIndex NVARCHAR(50)='d1.DimensionValue'
	DECLARE @DimensionId Nvarchar(20)=''
	DECLARE @WhereCondition NVARCHAR(2000)=' '
	DECLARE @DimensionValues NVARCHAR(MAX)
	DECLARE @Query NVARCHAR(MAX), @DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX), @CustomFilter NVARCHAR(MAX)
	SET @DIMENSIONGROUP=' '
	SET @DimensionId=CAST(@DateDimensionId as nvarchar)
	
	Select @Query=CustomQuery, @DrillDownCustomQuery=DrillDownCustomQuery, @DrillDownXFilter=DrillDownXFilter,@CustomFilter=CustomFilter from ReportGraph where Id=@ReportGraphID
	IF(@IsDrillDownData = 0)
	BEGIN	
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') ,DV.OrderValue '
	END
	ELSE
	BEGIN
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS Column1,DV.OrderValue '
	END
	IF(@FilterValues IS NULL)
	BEGIN
		SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	ELSE 
	BEGIN
	   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	Declare @DimensionValueField NVArchar(500)='DimensionValue'+@dimensionID
	IF(@ViewByValue = 'Q') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(CAst(Value AS int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
			
	ELSE IF(@ViewByValue = 'Y') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + CAST(DATEPART(YY,CAST(CAST(value AS INt) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(Ordervalue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + CAST(DATEPART(YY,CAST(CAST(value AS INT) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue = 'M') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS INT) AS DATETIME)),0,4)
			+ '-' + CAST(DATEPART(YY,CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR) + ']' DisplayValue ,
		MIN(ORDERVALUE) OrderValue  
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
		GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS int) AS DATETIME)),0,4) + '-' + CAST(DATEPART(YY,CAST(CAST(CAST(VALUE AS INT) AS INT) AS DATETIME)) AS NVARCHAR) + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6, 
				CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']' 
				DisplayValue ,MIN(ORDERVALUE) OrderValue 
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
					CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']') A
			ORDER BY OrderValue
		END
		ELSE 
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']'
				DisplayValue ,MIN(Id) OrderValue  
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']') A
			ORDER BY OrderValue
		END
	END
	ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']' DisplayValue ,
			MIN(OrderValue) OrderValue
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']') A
		ORDER BY OrderValue
	END
	/* Modified by Arpita Soni for ticket #604 on 11/19/2015 */
	IF(@FilterValues IS NOT NULL AND (select IsICustomFilterApply from ReportGraph where id=@ReportGraphId)=1 )
	BEGIN
		DECLARE @XmlString XML
		SET @XmlString = @FilterValues;
  		DECLARE @TempValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)
		DECLARE @FilterValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)

 
		;WITH MyData AS (
			SELECT data.col.value('(@ID)[1]', 'int') Number
				,data.col.value('(.)', 'INT') DimensionValueId
			FROM @XmlString.nodes('(/filters/filter)') AS data(col)
		)
		Insert into @TempValueTable 
		SELECT 
			 D.id,
			 DV.Id
		FROM MyData 
		INNER JOIN DimensionValue DV ON DV.Id = MyData.DimensionValueId
		INNER JOIN Dimension D ON D.Id = DV.DimensionId
		--select * from @TempValueTable

		INSERT INTO @FilterValueTable SELECT  ID
			   ,'AND DimensionValue'+Id+' IN('+STUFF((SELECT ',' + CAST(DimensionValueId+'' AS VARCHAR(MAX)) [text()]
				 FROM @TempValueTable 
				 WHERE ID = t.ID
				 FOR XML PATH(''), TYPE)
				.value('.','NVARCHAR(MAX)'),1,1,' ')+')' List_Output
		FROM @TempValueTable t
		GROUP BY ID
		--select * from @FilterValueTable
		--1 step
		DECLARE @DelimitedString NVARCHAR(MAX)
		DECLARE @DimensionValueTemp NVARCHAR(MAX)
		DECLARE @DimensionIdTemp NVARCHAR(MAX)
		SET @DelimitedString =@CustomFilter
		DECLARE @FinalFilterString NVARCHAR(MAX)=' '
		DECLARE @FilterCursor CURSOR
		SET @FilterCursor = CURSOR FAST_FORWARD FOR SELECT id,dimensionvalueid from @FilterValueTable
		OPEN @FilterCursor
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		WHILE @@FETCH_STATUS = 0
		BEGIN
		Declare @Flage INT=0
		Declare @Count INT=0
		Declare @Id NVARCHAR(1000)
		SET @FinalFilterString= @FinalFilterString+ ' '+ @DimensionValueTemp
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		END
		CLOSE @FilterCursor
		DEALLOCATE @FilterCursor	
	END
	IF(@FinalFilterString IS NOT NULL)
	BEGIN
		SET @WhereCondition = @WhereCondition + @FinalFilterString
	END

	/* Modified by Arpita Soni for Ticket #669 on 12/29/2015 */
	DECLARE @fromRecord INT
	DECLARE @toRecord INT 
	
	IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
	BEGIN
		SET @fromRecord = (@PageSize * @PageIndex) + 1;
		SET @toRecord = (@PageSize * @PageIndex) + 10;
	END
	IF(ISNULL(@SortBy,'') = '')
	BEGIN
		SET @SortBy = 'SELECT NULL'
	END
	IF CHARINDEX('#DIMENSIONGROUP#',@Query) <= 0
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
		END
		ELSE 
		BEGIN
			DECLARE @DrillDownWhere NVARCHAR(MAX)
			IF(@IsExportAll = 0)
			BEGIN
				SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 
			END
			SET @QUERY = REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN
				SET @Query =' SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM (' + @Query +
										' ) B ) A WHERE RowNumber BETWEEN '+CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
									'; SELECT COUNT(*) FROM (' + @Query +') C;'
			END
		END
		
		EXEC(@Query)
	END
	ELSE
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			Declare @Table nvarchar(Max)='DECLARE @TABLE Table(Name NVARCHAR(1000),Measure Nvarchar(1000),Date Nvarchar(1000),OrderValue1 int)'
			DECLARE @SelectTable NVARCHAR(MAx) =' SELECT * FROM @Table'
			SET @SelectTable='; SELECT * FROM ( ' +
			'SELECT ' + 'Name' + ',' + 'Date' + ',[' + 'Measure' + '],MAX(OrderValue1) OVER (PARTITION BY '+ 'Name' +') OrderValue FROM @Table' + 
			') P PIVOT ('+
			'MAX(['+ 'Measure' +']) FOR ' + 'Date' + ' IN ('+ @DimensionValues +')'+
			') AS PVT ORDER BY OrderValue' 
	
			SET @QUERY=REPLACE(@Query,'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
			EXEC(@Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable)
		END
		ELSE 
		BEGIN
			SET @DrillDownWhere = ''
			IF(@IsExportAll = 0)
			BEGIN
				SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 
			END
			SET @DIMENSIONGROUP = REPLACE(@DIMENSIONGROUP,',DV.ORDERVALUE','')
			SET @DrillDownCustomQuery=REPLACE((@DrillDownCustomQuery),'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			SET @DrillDownCustomQuery= REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN	
				SET @DrillDownCustomQuery ='SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM ('+ @DrillDownCustomQuery +
										') B ) A WHERE RowNumber BETWEEN '+ CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
										'; SELECT COUNT(*) FROM (' + @DrillDownCustomQuery +') C;'
			END
			
			EXEC(@DrillDownCustomQuery)
		END
	END
END
Go
--16
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CustomTableQuery') AND xtype IN (N'P'))
    DROP PROCEDURE CustomTableQuery
GO

CREATE PROCEDURE [dbo].[CustomTableQuery]  
@ReportTableID INT, 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100',
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@DimensionTableName NVARCHAR(100)
AS
	
	BEGIN

	DECLARE @DIMENSIONGROUP NVARCHAR(MAX)
	DECLARE @DimensionIndex NVARCHAR(50)='d1.DimensionValue'
	DECLARE @DimensionId Nvarchar(20)=''
	DECLARE @WhereCondition NVARCHAR(MAX)=' '
	DECLARE @DimensionValues NVARCHAR(MAX)
	DECLARE @Query NVARCHAR(MAX)
	DECLARE @CustomFilter NVARCHAR(MAX)
	SET @DIMENSIONGROUP=' '
	--SET @DimensionId=(SELECT TOP 1 DimensionId FROM ReportTableDimension INNER JOIN Dimension D On ReportTableDimension.Dimensionid=d.id WHERE ReportTableId=@ReportTableID AND D.IsDateDimension=1  )
	--SET @DimensionId=62
	SET @DimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)
	Select @Query=CustomQuery,@customFilter=CustomFilter from ReportTable where Id=@ReportTableID;
	SET @CustomFilter=(Select CustomFilter from ReportTable where Id=@ReportTableID);
	SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+'''),DV.OrderValue '

	IF(@FilterValues IS NULL)
	BEGIN

		   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
		 
	END
	ELSE 
	BEGIN
	
		   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
		
	END


	   Declare @DimensionValueField NVArchar(500)='DimensionValue'+@dimensionID
			IF(@ViewByValue = 'Q') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(CAst(Value AS int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue 
				 FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				 GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			
			ELSE IF(@ViewByValue = 'Y') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + CAST(DATEPART(YY,CAST(CAST(value AS INt) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(Ordervalue) OrderValue 
				 FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				 GROUP BY '[' + CAST(DATEPART(YY,CAST(CAST(value AS INT) AS DATETIME)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'M') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS INT) AS DATETIME)),0,4)
				 + '-' + CAST(DATEPART(YY,CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR) + ']' DisplayValue ,
				MIN(ORDERVALUE) OrderValue  
				 FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS int) AS DATETIME)),0,4) + '-' + CAST(DATEPART(YY,CAST(CAST(CAST(VALUE AS INT) AS INT) AS DATETIME)) AS NVARCHAR) + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue='W')
			BEGIN
				IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
					 CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
					 CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6, 
					 CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']' 
					 DisplayValue ,MIN(ORDERVALUE) OrderValue 
					  FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
					  GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
					   CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
					    CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
						 CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']') A
					ORDER BY OrderValue
				END
				ELSE 
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
					+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
					+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']'
					 DisplayValue ,MIN(Id) OrderValue  
					  FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
					 GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
					+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
					+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']') A
					ORDER BY OrderValue
				END
			END
			ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']' DisplayValue ,
				MIN(OrderValue) OrderValue
				  FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				  GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']') A
				ORDER BY OrderValue
			END

--IF CHARINDEX('#dimensiongroup#',LOWER(@Query)) <= 0
IF(@FilterValues IS NOT NULL AND (select IsICustomFilterApply from ReportTable where id=@ReportTableID)=1 )
BEGIN


 DECLARE @XmlString XML
 SET @XmlString = @FilterValues;
  	DECLARE @TempValueTable TABLE (
				ID NVARCHAR(MAX),
				DimensionValueID NVARCHAR(MAX)
			)
DECLARE @FilterValueTable TABLE (
				ID NVARCHAR(MAX),
				DimensionValueID NVARCHAR(MAX)
			)

 
 ;WITH MyData AS (
	SELECT data.col.value('(@ID)[1]', 'int') Number
		,data.col.value('(.)', 'INT') DimensionValueId
	FROM @XmlString.nodes('(/filters/filter)') AS data(col)
)
Insert into @TempValueTable 
SELECT 
	 D.id,
	 DV.Id
FROM MyData 
INNER JOIN DimensionValue DV ON DV.Id = MyData.DimensionValueId
INNER JOIN Dimension D ON D.Id = DV.DimensionId
--select * from @TempValueTable

INSERT INTO @FilterValueTable SELECT  ID
       ,'AND DimensionValue'+Id+' IN('+STUFF((SELECT ',' + CAST(DimensionValueId+'' AS VARCHAR(MAX)) [text()]
         FROM @TempValueTable 
         WHERE ID = t.ID
         FOR XML PATH(''), TYPE)
        .value('.','NVARCHAR(MAX)'),1,1,' ')+')' List_Output
FROM @TempValueTable t
GROUP BY ID
--select * from @FilterValueTable
--1 step
DECLARE @DelimitedString NVARCHAR(MAX)
DECLARE @DimensionValueTemp NVARCHAR(MAX)
DECLARE @DimensionIdTemp NVARCHAR(MAX)
SET @DelimitedString =@CustomFilter
DECLARE @FinalFilterString NVARCHAR(MAX)=' '
DECLARE @FilterCursor CURSOR
SET @FilterCursor = CURSOR FAST_FORWARD FOR SELECT id,dimensionvalueid from @FilterValueTable
OPEN @FilterCursor
FETCH NEXT FROM @FilterCursor
INTO @DimensionIdTemp,@DimensionValueTemp
WHILE @@FETCH_STATUS = 0
BEGIN
Declare @Flage INT=0
Declare @Count INT=0
Declare @Id NVARCHAR(1000)
SET @FinalFilterString= @FinalFilterString+ ' '+ @DimensionValueTemp
FETCH NEXT FROM @FilterCursor
INTO @DimensionIdTemp,@DimensionValueTemp
END
CLOSE @FilterCursor
DEALLOCATE @FilterCursor	
END
IF(@FinalFilterString IS NOT NULL)
SET @WhereCondition=@WhereCondition+@FinalFilterString

IF CHARINDEX('#dimensiongroup#',LOWER(@Query)) <= 0
BEGIN

SET @QUERY=REPLACE(@Query,'#dimensionwhere#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
--IF(@CustomFilter IS NOT NULL AND LEN(@CustomFilter)>0 AND @FilterValues IS NOT NULL )
--	SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),@FinalFilterString)
--	ELSE
--	SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),'')

exec(@Query)
END
ELSE
BEGIN
	Declare @Table nvarchar(Max)='DECLARE @TABLE Table(Name NVARCHAR(1000),Measure Nvarchar(1000),Date Nvarchar(1000),OrderValue1 int)'
	DECLARE @SelectTable NVARCHAR(MAx) =' Select * from @Table'
	SET @SelectTable='; SELECT * FROM ( ' +
	'SELECT ' + 'Name' + ',' + 'Date' + ',[' + 'Measure' + '],MAX(OrderValue1) OVER (PARTITION BY '+ 'Name' +') OrderValue FROM @Table' + 
	') P PIVOT ('+
	'MAX(['+ 'Measure' +']) FOR ' + 'Date' + ' IN ('+ @DimensionValues +')'+
	') AS PVT ORDER BY OrderValue' 
	SET @QUERY=REPLACE(LOWER(@Query),'#dimensiongroup#',''+@DIMENSIONGROUP)
	
	SET @QUERY=REPLACE(LOWER(@Query),'#dimensionwhere#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
	--IF(@CustomFilter IS NOT NULL AND LEN(@CustomFilter)>0 AND @FilterValues IS NOT NULL)
	--	SET @QUERY=REPLACE(@Query,LOWER('#FILTERS#'),@FinalFilterString)
	--ELSE
	--SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),'')
	
	--print @Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable
	exec(@Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable)
	
	END
	
END
Go
--17
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionInvalidFormulaTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionInvalidFormulaTest
GO


CREATE PROCEDURE [DimensionInvalidFormulaTest]
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @DimensionID int
DECLARE @DimensionFormula nvarchar(max)


DECLARE DimensionCursor Cursor for
select id, formula from dimension where ValueFormula is null
Open DimensionCursor
FETCH NEXT FROM DimensionCursor
into @DimensionID, @DimensionFormula
while @@FETCH_STATUS = 0
	BEGIN
	DECLARE @CheckSQL nvarchar(max)

	DECLARE @Valid int
	CREATE TABLE  #TempTableStore (Display nvarchar(max), Value nvarchar(max), OrderBy nvarchar(max));
	SET @Valid=1

	SEt @DimensionFormula = ' Insert into #TempTableStore ' + @DimensionFormula

	BEGIN TRY
	 exec sp_executesql @DimensionFormula
	END TRY
	BEGIN CATCH
	 SET @Valid=0
	END CATCH

	DROP TABLE #TempTableStore


	
	if @Valid=0
		BEGIN
		SET @CheckSQL = 'insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with invalid formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''');' 

		exec sp_executesql @CheckSQL
		END


	FETCH NEXT FROM DimensionCursor
	into @DimensionID, @DimensionFormula
	END


Close DimensionCursor
Deallocate DimensionCursor
RETURN
END



GO
--18
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionNonNumericDateOrderByTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionNonNumericDateOrderByTest
GO


CREATE PROCEDURE [DimensionNonNumericDateOrderByTest]
	-- Add the parameters for the stored procedure here
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @TableName nvarchar(max)
	DECLARE @Formula nvarchar(max)
	DECLARE @DimensionID int

	DECLARE QueryCursor cursor for
	select tablename, Formula, id from dimension
	where IsDateDimension=0

	OPEN QueryCursor
	FETCH NEXT from QueryCursor 
	into @TableName, @Formula, @DimensionID
	while @@FETCH_STATUS=0
		BEGIN

		DECLARE @QueryToRun nvarchar(max)

		SET @QueryToRun = 'if (select count(*) from  (' + @Formula + ') A where isnumeric(A.OrderBy=0) ) > 0 BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Date Dimension with non-numeric order by'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;'


		FETCH NEXT from QueryCursor 
		into @TableName, @Formula, @DimensionID
		END

	CLOSE QueryCursor
	DEALLOCATE QueryCursor

END




GO
--19
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionNullTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionNullTest
GO


CREATE PROCEDURE [DimensionNullTest]
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @DimensionID int
DECLARE @DimensionFormula nvarchar(max)


DECLARE DimensionCursor Cursor for
select id, formula from dimension where ValueFormula is null
Open DimensionCursor
FETCH NEXT FROM DimensionCursor
into @DimensionID, @DimensionFormula
while @@FETCH_STATUS = 0
	BEGIN
	DECLARE @CheckSQL nvarchar(max)

	
	SET @CheckSQL = 'if exists (Select 1 from (' + @DimensionFormula + ') A where value is null or display is null or orderby is null ) BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with null values from formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;' 

	print @CheckSQL
	exec sp_executesql @CheckSQL


	FETCH NEXT FROM DimensionCursor
	into @DimensionID, @DimensionFormula
	END


Close DimensionCursor
Deallocate DimensionCursor
RETURN
END


GO
--20
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionTooManyValuesTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionTooManyValuesTest
GO

CREATE PROCEDURE [DimensionTooManyValuesTest]
	-- Add the parameters for the stored procedure here
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @TableName nvarchar(max)
	DECLARE @Formula nvarchar(max)
	DECLARE @DimensionID int

	DECLARE QueryCursor cursor for
	select tablename, Formula, id from dimension
	where IsDateDimension=0

	OPEN QueryCursor
	FETCH NEXT from QueryCursor 
	into @TableName, @Formula, @DimensionID
	while @@FETCH_STATUS=0
		BEGIN

		DECLARE @QueryToRun nvarchar(max)

		SET @QueryToRun = 'if (select count(*) from  (' + @Formula + ') A ) > 20 BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with too many values from formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;'


		FETCH NEXT from QueryCursor 
		into @TableName, @Formula, @DimensionID
		END

	CLOSE QueryCursor
	DEALLOCATE QueryCursor

END



GO
--21
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionValueFormulaNullTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionValueFormulaNullTest
GO

CREATE PROCEDURE [DimensionValueFormulaNullTest]
	-- Add the parameters for the stored procedure here
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @TableName nvarchar(max)
	DECLARE @ValueFormula nvarchar(max)
	DECLARE @DimensionID int

	DECLARE QueryCursor cursor for
	select tablename, ValueFormula, id from dimension
	where ValueFormula is not null

	OPEN QueryCursor
	FETCH NEXT from QueryCursor 
	into @TableName, @ValueFormula, @DimensionID
	while @@FETCH_STATUS=0
		BEGIN

		DECLARE @QueryToRun nvarchar(max)

		SET @QueryToRun = 'if exists (select id from ' + @TableName +  ' A left outer join (' + @ValueFormula + ') B on A.id=b.id where b.id is null) BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with null values from Value formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;'


		FETCH NEXT from QueryCursor 
		into @TableName, @ValueFormula, @DimensionID
		END

	CLOSE QueryCursor
	DEALLOCATE QueryCursor

END


GO
--22
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[DimensionValuePopulate]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[DimensionValuePopulate]  
END
GO


CREATE PROCEDURE [dbo].[DimensionValuePopulate]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY


	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue_New]') AND type in (N'U'))
		BEGIN
			DROP tABLE MeasureValue_New 
		END

DECLARE @CREATEMEASUREVALUE varchar(max)

SET @CREATEMEASUREVALUE = 
'CREATE TABLE [dbo].[MeasureValue_New](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Measure] [int] NOT NULL,
	[DimensionValue] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[RecordCount] [float] NULL,
	CONSTRAINT [PK_MeasureValue_New' + cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar) + '] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]'

exec(@CREATEMEASUREVALUE)
exec('delete from dimensionvalue_New')

--reset the identity seeds
DBCC CHECKIDENT('MeasureValue_New', RESEED, 0)
DBCC CHECKIDENT('DimensionValue_New', RESEED, 0)

DECLARE @DID varchar(30), @DFORMULA varchar(max);
DECLARE @COMPUTEALLVALUES bit, @STARTDATE datetime, @ENDDATE datetime;
DECLARE @ISDATEDIMENSION bit;

DECLARE Dimension_Cursor CURSOR FOR
select dimension.id, Formula, isnull(ComputeAllValues,0), isnull(d1.DateTime,cast('1/1/2010' as datetime)), isnull(d2.DateTime,cast(cast(getdate() as date) as datetime)), isnull(isDateDimension,0) from dimension
left outer join dimtime d1 on d1.DateTime=StartDate
left outer join dimtime d2 on d2.DateTime=EndDate
where IsDeleted=0
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
while @@FETCH_STATUS = 0
	Begin
		declare @Insert_Query varchar(max);


		--We need to know if this table has a compute all values set on it and if this is a date dimension.  If so, we need to incorporate the full range of values.
		--We will require date dimensions to use the DimTime ids as values in order to be supported.

		if @ISDATEDIMENSION=0 or @COMPUTEALLVALUES=0
		BEGIN
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (' + @DFORMULA + ') B cross join (select ' + @DID + ' as ID) A;';
		END
		ELSE
		BEGIN --If this is a data dimension that we need to populate all values for
			DECLARE @SDATE datetime, @EDATE datetime;

			SET @SDATE=(select [DateTime] from dimTime where id=@STARTDATE)
			SET @EDATE=(select dateadd(day, 1, [DateTime]) from dimTime where id=@ENDDATE) -- To avoid missing the last date
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (select id,[DateTime],id as id2 from dimtime where [DateTime] between ''' + cast(cast(@SDATE as date) as varchar) + ''' and ''' + cast(cast(@EDATE as date) as varchar) + ''') B cross join (select ' + @DID + ' as ID) A;';
		END

		--print @Insert_Query;
	
		SET @CURQUERYFORERROR=@Insert_Query
		if @DEBUG=1
		BEGIN
			print @Insert_Query
		END
		exec(@Insert_Query);
		--print @Insert_Query;
		--print '--**************************'
		FETCH NEXT FROM Dimension_Cursor
		INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
	end;
Close Dimension_Cursor
Deallocate Dimension_Cursor




DECLARE @COLUMNNAME varchar(100);
DECLARE @DIMENSIONID varchar(10);
DECLARE @COLUMNVALUE varchar(100);
DECLARE @VALUEFORMULA varchar(max);
DECLARE @TABLENAME varchar(250);

DECLARE Dimension_Cursor CURSOR FOR
select distinct d2.TableName as tablename, 'DimensionValue' + cast(d2.id as varchar) as colname, d2.id as did, d2.ColumnName, d2.ValueFormula from dimensionvalue_new d1
inner join dimension d2 on d1.DimensionID=d2.id
where d2.IsDeleted=0
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @UPDATEQUERY varchar(max);


	DECLARE @rowCheckQuery varchar(max) = 'select count(*) from ' +  @TABLENAME + '_Base_New';
	DECLARE @rowCheck table (c int)
	delete from @rowCheck
	insert into @rowCheck 
	exec(@rowCheckQuery)

	DECLARE @rowCheckCount int = (select c from @rowCheck)

	if @rowCheckCount = 0
		BEGIN
		--Populate the base_new table if it hasn't been populated before
		if @DEBUG=1
		BEGIN
			print 'insert into ' + @TABLENAME + '_Base_New (id) select id from ' + @TABLENAME
		END

		exec('insert into ' + @TABLENAME + '_Base_New (id) select id from ' + @TABLENAME)
		END

	--We need to check if the column exists.  If not, we need to create it.  This should never happen.
	DECLARE @exists bit;

	set @exists= (select cast(count(*) as bit) from sys.columns 
            where Name = N'' + @COLUMNNAME + '' and Object_ID = Object_ID(N'' + @TABLENAME  + '_Base_New'));


	if @exists=0
		BEGIN
		DECLARE @ADDQUERY varchar(max);
		set @ADDQUERY = N'ALTER TABLE ' + @TABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		exec(@ADDQUERY)
		END


--select d1.id as bid, d2.id as did from DummyContacts d1
--inner join DimensionValue d2 on d1.Industry=d2.Value and d2.DimensionID=45
	
	SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New 
		set ' + @COLUMNNAME + ' = A.did
		from
		(select d1.id as bid, d2.id as did from ' + @TABLENAME + ' d1 
		inner join DimensionValue_New d2 on d1.' + @COLUMNVALUE + '=d2.Value and d2.DimensionID=' + @DIMENSIONID + '
		
		) A
		where A.bid=id'




	if @VALUEFORMULA is not null 
		BEGIN
		SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New
			set ' + @COLUMNNAME + ' = did 
			from
			(select  joinID,  D.ID as did from
			(' + @VALUEFORMULA + ') A inner join DimensionValue_New D on D.Value=A.Value
			where D.DimensionID=' + @DIMENSIONID + ') B
			where B.joinID=id'
		END

	--print @UPDATEQUERY
	SET @CURQUERYFORERROR=@UPDATEQUERY
	if @DEBUG=1
	BEGIN
		print @UPDATEQUERY
	END
	exec(@UPDATEQUERY)
	--print '--/////////////////////////'


	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor

--We need to make sure there is a dirty bit on all of the base tables so partial aggregation will run


DECLARE Dimension_Cursor CURSOR FOR
select distinct DimensiontableName + '_Base_New' from DynamicDimension_New
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	set @exists= (select cast(count(*) as bit) from sys.columns 
			where Name = N'dirty' and Object_ID = Object_ID( @TABLENAME  ));


	if @exists=0
		BEGIN
		set @ADDQUERY = N'ALTER TABLE ' + @TABLENAME + ' ADD dirty bit';
		set @CURQUERYFORERROR=@ADDQUERY
		exec(@ADDQUERY)
		SET @UPDATEQUERY = N'Update ' + @TABLENAME + ' set dirty=0';
		set @CURQUERYFORERROR=@UPDATEQUERY
		exec(@UPDATEQUERY)
		END
	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME
	END
Close Dimension_Cursor
Deallocate Dimension_Cursor


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DimensionValuePopulate',@CURQUERYFORERROR);
	THROW;
END CATCH

END


GO
--23
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[DimensionValuePopulatePartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[DimensionValuePopulatePartial]  
END
GO


CREATE PROCEDURE [dbo].[DimensionValuePopulatePartial]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY


	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue_New]') AND type in (N'U'))
		BEGIN
			DROP tABLE MeasureValue_New 
		END

DECLARE @CREATEMEASUREVALUE varchar(max)

SET @CREATEMEASUREVALUE = 
'CREATE TABLE [dbo].[MeasureValue_New](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Measure] [int] NOT NULL,
	[DimensionValue] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[RecordCount] [float] NULL,
	CONSTRAINT [PK_MeasureValue_New' + cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar) + '] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]'

exec(@CREATEMEASUREVALUE)
exec('delete from dimensionvalue_New')

--reset the identity seeds
DECLARE @Seed int = (Select IDENT_CURRENT('MeasureValue'))+1

DBCC CHECKIDENT('MeasureValue_New', RESEED, @Seed)

SET @Seed  = (Select IDENT_CURRENT('DimensionValue'))+1

DBCC CHECKIDENT('DimensionValue_New', RESEED, @Seed)

DECLARE @DID varchar(30), @DFORMULA varchar(max);
DECLARE @COMPUTEALLVALUES bit, @STARTDATE datetime, @ENDDATE datetime;
DECLARE @ISDATEDIMENSION bit;

DECLARE Dimension_Cursor CURSOR FOR
select dimension.id, Formula, isnull(ComputeAllValues,0), isnull(d1.DateTime,cast('1/1/2010' as datetime)), isnull(d2.DateTime,cast(cast(getdate() as date) as datetime)), isnull(isDateDimension,0) from dimension
left outer join dimtime d1 on d1.DateTime=StartDate
left outer join dimtime d2 on d2.DateTime=EndDate
where IsDeleted=0 and tablename in (select dimensiontablename from DynamicDimension_new) 
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
while @@FETCH_STATUS = 0
	Begin
		declare @Insert_Query varchar(max);


		--We need to know if this table has a compute all values set on it and if this is a date dimension.  If so, we need to incorporate the full range of values.
		--We will require date dimensions to use the DimTime ids as values in order to be supported.

		if @ISDATEDIMENSION=0 or @COMPUTEALLVALUES=0
		BEGIN
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (' + @DFORMULA + ') B cross join (select ' + @DID + ' as ID) A;';
		END
		ELSE
		BEGIN --If this is a date dimension that we need to populate all values for
			DECLARE @SDATE datetime, @EDATE datetime;

			SET @SDATE=(select [DateTime] from dimTime where id=@STARTDATE)
			SET @EDATE=(select dateadd(day, 1, [DateTime]) from dimTime where id=@ENDDATE) -- To avoid missing the last date
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (select id,[DateTime],id as id2 from dimtime where [DateTime] between ''' + cast(cast(@SDATE as date) as varchar) + ''' and ''' + cast(cast(@EDATE as date) as varchar) + ''') B cross join (select ' + @DID + ' as ID) A;';
		END

		--print @Insert_Query;
	
		SET @CURQUERYFORERROR=@Insert_Query
		if @DEBUG=1
		BEGIN
			print @Insert_Query
		END
		exec(@Insert_Query);
		--print @Insert_Query;
		--print '--**************************'
		FETCH NEXT FROM Dimension_Cursor
		INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
	end;
Close Dimension_Cursor
Deallocate Dimension_Cursor


--Okay, we need to update DimensionValue_New with the values that exist from DimensionValue
--This will allow us to keep computations that haven't changed

print 'Before Duplicates'

--Update any display or order values that have changed
update DimensionValue set DisplayValue=dv, OrderValue=ov from (

select d2.id as ident, d1.DisplayValue as dv, d1.OrderValue as ov from DimensionValue_New d1
inner join DimensionValue d2 on d1.dimensionid=d2.DimensionID and 
d1.Value=d2.value ) a 
where a.ident=id


--We need to store the rows that we need to get rid of as duplicates
DECLARE @ToDelete table (ident int)

--The duplicate rows
insert into @ToDelete
select d1.id as ident from DimensionValue_New d1
inner join DimensionValue d2 on d1.dimensionid=d2.DimensionID and 
d1.Value=d2.value

--Since we want to maintain the exisiting values
SET IDENTITY_INSERT DimensionValue_New ON

print 'Before insert'

--Grab the existing values
insert into DimensionValue_New (id, dimensionid, value, displayvalue, ordervalue)
select d2.id as ident, d1.dimensionid, d1.Value as v, d1.DisplayValue as dv, d1.OrderValue as ov from DimensionValue_New d1
inner join DimensionValue d2 on d1.dimensionid=d2.DimensionID and 
d1.Value=d2.value

--Get rid of the duplicates
delete from DimensionValue_New where id in (select ident from @ToDelete)

print 'After duplicates'

SET IDENTITY_INSERT DimensionValue_New OFF --Since Identity insert is a bad idea, turn it off

DECLARE @COLUMNNAME varchar(100);
DECLARE @DIMENSIONID varchar(10);
DECLARE @COLUMNVALUE varchar(100);
DECLARE @VALUEFORMULA varchar(max);
DECLARE @TABLENAME varchar(250);

DECLARE Dimension_Cursor CURSOR FOR
select distinct d2.TableName as tablename, 'DimensionValue' + cast(d2.id as varchar) as colname, d2.id as did, d2.ColumnName, d2.ValueFormula from dimensionvalue_new d1
inner join dimension d2 on d1.DimensionID=d2.id
where d2.IsDeleted=0 and tablename in (select dimensiontablename from DynamicDimension_new)
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @UPDATEQUERY varchar(max);


	DECLARE @rowCheckQuery varchar(max) = 'select count(*) from ' +  @TABLENAME + '_Base_New';
	DECLARE @rowCheck table (c int)
	delete from @rowCheck
	insert into @rowCheck 
	exec(@rowCheckQuery)

	DECLARE @rowCheckCount int = (select c from @rowCheck)

	if @rowCheckCount = 0
		BEGIN
		--Populate the base_new table if it hasn't been populated before and set the dirty bits
		if @DEBUG=1
		BEGIN
			print 'insert into ' + @TABLENAME + '_Base_New (id, dirty) select b1.id, isnull(b2.dirty, 1) from ' + @TABLENAME + ' b1 left outer join ' + 
			@TABLENAME + '_Base b2 on b1.id=b2.id '
		END

		exec('insert into ' + @TABLENAME + '_Base_New (id, dirty) select b1.id, isnull(b2.dirty, 1) from ' + @TABLENAME + ' b1 left outer join ' + 
			@TABLENAME + '_Base b2 on b1.id=b2.id ')

		END

	--We need to check if the column exists.  If not, we need to create it.  This should never happen.
	DECLARE @exists bit;

	set @exists= (select cast(count(*) as bit) from sys.columns 
            where Name = N'' + @COLUMNNAME + '' and Object_ID = Object_ID(N'' + @TABLENAME  + '_Base_New'));


	if @exists=0
		BEGIN
		DECLARE @ADDQUERY varchar(max);
		set @ADDQUERY = N'ALTER TABLE ' + @TABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		exec(@ADDQUERY)
		END


--select d1.id as bid, d2.id as did from DummyContacts d1
--inner join DimensionValue d2 on d1.Industry=d2.Value and d2.DimensionID=45
	
	SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New 
		set ' + @COLUMNNAME + ' = A.did
		from
		(select d1.id as bid, d2.id as did from ' + @TABLENAME + ' d1 
		inner join DimensionValue_New d2 on d1.' + @COLUMNVALUE + '=d2.Value and d2.DimensionID=' + @DIMENSIONID + '
		
		) A
		where A.bid=id'




	if @VALUEFORMULA is not null 
		BEGIN
		SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New
			set ' + @COLUMNNAME + ' = did 
			from
			(select  joinID,  D.ID as did from
			(' + @VALUEFORMULA + ') A inner join DimensionValue_New D on D.Value=A.Value
			where D.DimensionID=' + @DIMENSIONID + ') B
			where B.joinID=id'
		END

	--print @UPDATEQUERY
	SET @CURQUERYFORERROR=@UPDATEQUERY
	if @DEBUG=1
	BEGIN
		print @UPDATEQUERY
	END
	exec(@UPDATEQUERY)
	--print '--/////////////////////////'


	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor




END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DimensionValuePopulate',@CURQUERYFORERROR);
	THROW;
END CATCH

END


GO
--24
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DynamicTableCreation') AND xtype IN (N'P'))
    DROP PROCEDURE DynamicTableCreation
GO


CREATE PROCEDURE [DynamicTableCreation]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY




DECLARE @TABLENAME varchar(max);
DECLARE @DIMENSIONS int;
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @ComputeAllValues bit;


DECLARE Dimension_Cursor CURSOR FOR
select TableName + '_New', Dimensions, DimensionTableName, ComputeAllValues from DynamicDimension_New
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues
while @@FETCH_STATUS = 0
	Begin

	DECLARE @CREATE_QUERY varchar(max);

	DECLARE @PART2 varchar(max);
	DECLARE @PART3 varchar(max);
	DECLARE @INSERTQUERY varchar(max); --To populate the d1d2 etc. tables
	DECLARE @INSERTFROM varchar(max);
	DECLARE @INSERTWHERE varchar(max);
	DECLARE @dCount int;
	DECLARE @dIndex int;
	DECLARE @dNextIndex int;
	DECLARE @CREATEVALUEQUERY varchar(max);
	DECLARE @CurDate varchar(100) = (select cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar));



	--Need to do something different if this is a table for which all values should be populated.
	SET @INSERTQUERY = N'Insert into ' + @TABLENAME + ' select distinct ';

	if @ComputeAllValues=0
	BEGIN
		SET @INSERTFROM = N' FROM ' + @DIMENSIONTABLENAME + '_Base_New ';
	END
	ELSE
	BEGIN
		SET @INSERTFROM = N' FROM ';
	END
	SET @INSERTWHERE = N' WHERE ';
	set @PART2='';
	set @PART3='';
	set @dCount=1;
	set @dIndex = CHARINDEX('d',@TABLENAME);
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

	DECLARE @FOUNDDATEDIMENSION bit;
	DECLARE @SUBTABLENAME varchar(max);
	DECLARE @DATEDIMENSION varchar(max);

	SET @FOUNDDATEDIMENSION=0;
	SET @SUBTABLENAME='';
	SET @DATEDIMENSION='0';

	if @ComputeAllValues=1 and @DIMENSIONS>2
	BEGIN
		SET @INSERTQUERY='DECLARE @t table (dv int)

		insert into @t
		select id from DimensionValue_New where DimensionID=%DATEDIMENSION%

		DECLARE @rc int=(select count(*) from @t)


		while @rc > 0
		BEGIN

			DECLARE @u table (id int)

			delete from @u

			insert into @u
			select top 100 dv from @t

			Insert into ' + @TABLENAME + ' 
			select distinct '
	END

	while @dCount <= @DIMENSIONS
		BEGIN

			set @PART2 = @PART2 + 	'ALTER TABLE [dbo].[' + @TABLENAME + ']  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + '_DimensionValue' + cast(@dCount as nvarchar) + @CurDate + '] FOREIGN KEY([d' + cast(@dCount as nvarchar) + '])
			REFERENCES [dbo].[DimensionValue_New] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + '] CHECK CONSTRAINT [FK_' + @TABLENAME + '_DimensionValue' + cast(@dCount as nvarchar) + @CurDate + ']';

			set @PART3 = @PART3 + '[d' + cast(@dCount as nvarchar) + '] [int] NOT NULL, ';

			if @ComputeAllValues=0
			BEGIN

				set @INSERTQUERY = @INSERTQUERY + 'DimensionValue' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1) + ' '
				--set @INSERTFROM = @INSERTFROM + ' DimensionValue d' + cast(@dCount as nvarchar) + ' ';
				--set @INSERTWHERE = @INSERTWHERE + 'd' + cast(@dCount as nvarchar) + '.DimensionId =' + SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1);
			END
			else
			BEGIN

				if @DIMENSIONS=2
				BEGIN
					set @INSERTQUERY = @INSERTQUERY + 'd' + cast(@dCount as varchar) + '.id '
					set @INSERTFROM = @INSERTFROM + ' DimensionValue_New d' + cast(@dCount as varchar) + ' ';
					set @INSERTWHERE = @INSERTWHERE + 'd' + cast(@dCount as varchar) + '.DimensionId =' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1);
				END
				ELSE
				BEGIN

					if @FOUNDDATEDIMENSION=1
					BEGIN
						set @INSERTQUERY = @INSERTQUERY + 'd1.d' + cast((@dCount-1) as varchar) + ' '
						set @SUBTABLENAME= @SUBTABLENAME + 'd' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1)
					END
					else
					BEGIN
						DECLARE @DIMENSIONTOCHECK varchar(10);
						SET @DIMENSIONTOCHECK=SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1);
						SET @FOUNDDATEDIMENSION=(select isnull(computeAllValues,0) from Dimension where id=@DIMENSIONTOCHECK)	

						if @FOUNDDATEDIMENSION=0
						BEGIN
							set @INSERTQUERY = @INSERTQUERY + 'd1.d' + cast(@dCount as varchar) + ' '
							set @SUBTABLENAME= @SUBTABLENAME + 'd' + @DIMENSIONTOCHECK
						END
						ELSE
						BEGIN
							set @INSERTQUERY = @INSERTQUERY + 'd2.id '
							SET @DATEDIMENSION=@DIMENSIONTOCHECK
							SET @INSERTQUERY=Replace(@INSERTQUERY,'%DATEDIMENSION%',cast(@DIMENSIONTOCHECK as nvarchar))
						END
	
					END

					set @INSERTFROM = ' FROM ' + @SUBTABLENAME + '_New d1 cross join @u d2 '
					set @INSERTWHERE = ' 	delete from @t where dv in (select id from @u)

											set @rc= (select count(*) from @t)

											END'
				END
			END


			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@TABLENAME)+1;
				END

			if @dCount <= @DIMENSIONS
				BEGIN
				set @INSERTQUERY = @INSERTQUERY + ', ';

				END
					if @ComputeAllValues=1 and @DIMENSIONS=2
					BEGIN

						if @dCount = 2
						BEGIN
							set @INSERTFROM = @INSERTFROM + ', ';
							set @INSERTWHERE = @INSERTWHERE + ' and ';
						END
					END



		END


	SET @INSERTQUERY = @INSERTQUERY + @INSERTFROM; -- + @INSERTWHERE;

	if @ComputeAllValues=1
	BEGIN
		SET @INSERTQUERY = @INSERTQUERY + @INSERTWHERE;
	END

	--print @INSERTQUERY

	SET @CREATE_QUERY = N'

			if object_id(N''' + @TABLENAME + ''',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + ']
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + '](
				[id] [int] IDENTITY(1,1) NOT NULL, ' + @PART3 + '
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + '] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + '] REBUILD PARTITION = ALL
			WITH (DATA_COMPRESSION = ROW); 

			' + @PART2;
	
	SET @CREATEVALUEQUERY=N'
			if object_id(N''' + @TABLENAME + 'Value'',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + 'Value]
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + 'Value](
				[id] [int] IDENTITY(1,1) NOT NULL, 
				[' + left(@TABLENAME, len(@TABLENAME)-4) + '] int NOT NULL, 
				[Measure] [int] NOT NULL,
				[Value] [float] NOT NULL,
				[Rows] [float] null,
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + 'Value] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate + '] FOREIGN KEY([' + left(@TABLENAME, len(@TABLENAME)-4) + '])
			REFERENCES [dbo].[' + @TABLENAME + '] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate +']
			
			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + '] FOREIGN KEY([Measure])
			REFERENCES [dbo].[Measure] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + ']'
			
		--Before we drop the table, we need to eliminate all constraints on the table

		CREATE TABLE #Keys
		(
		 PKTABLE_QUALIFIER varchar(max),
		 PKTABLE_OWNER varchar(max),
		 PKTABLE_NAME varchar(max),
		 PKCOLUMN_NAME varchar(max),
		 FKTABLE_QUALIFIER varchar(max),
		 FKTABLE_OWNER varchar(max),
		 FKTABLE_NAME varchar(max),
		 FKCOLUMN_NAME varchar(max),
		 KEY_SEQ int,
		 UPDATE_RULE int,
		 DELETE_RULE int,
		 FK_NAME varchar(max),
		 PK_NAME varchar(max),
		 DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= @TABLENAME

		DECLARE @FKTABLE varchar(max);
		DECLARE @FKNAME varchar(max);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(max);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;

			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END
			exec (@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor



		drop table #Keys


		--select @CREATE_QUERY
		SET @CURQUERYFORERROR=@CREATE_QUERY
		if @DEBUG=1
		BEGIN
			print @CREATE_QUERY
		END
		exec (@CREATE_QUERY);

		--Okay, now we need to populate the table
		--select @INSERTQUERY

		SET @CURQUERYFORERROR=@INSERTQUERY
		if @DEBUG=1
		BEGIN
			print @INSERTQUERY
		END
		exec (@INSERTQUERY)

		--Release space used in the insert query
		dbcc shrinkdatabase (tempdb, 10) 


		SET @CURQUERYFORERROR=@CREATEVALUEQUERY
		if @DEBUG=1
		BEGIN
			print @CREATEVALUEQUERY
		END
		exec (@CREATEVALUEQUERY)

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor
END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DynamicTableCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END



GO
--25
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[DynamicTableCreationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[DynamicTableCreationPartial]  
END
GO

CREATE PROCEDURE [dbo].[DynamicTableCreationPartial]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY




DECLARE @TABLENAME varchar(max);
DECLARE @DIMENSIONS int;
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @ComputeAllValues bit;
DECLARE @DATEDIMENSION int;



DECLARE Dimension_Cursor CURSOR FOR
select TableName, Dimensions, DimensionTableName, ComputeAllValues, 
case when ComputeAllValues=1 then dbo.FindDateDimension(TableName,Dimensions) else 0 end as DateDimension
from DynamicDimension_New 
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues, @DATEDIMENSION
while @@FETCH_STATUS = 0
	Begin

	DECLARE @CREATE_QUERY varchar(max);
	DECLARE @INSERTQUERY varchar(max); --To populate the d1d2 etc. tables
	DECLARE @CREATEVALUEQUERY varchar(max);
	DECLARE @CurDate varchar(100) = (select cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar));

	--There are two cases
	--1) Non compute all values
	--2) Compute All Values

	--The table and value creates are the same for both cases

	--In the case of non compute all values we just need to get all of the right values, here are the steps
	--1) Turn Identity insert on
	--2) Run a query to find the values that exist in the current table and insert with those identities
	--3) Turn Identity insert off
	--4) Set the identity seed to one more than the one from the old table
	--5) Insert any new rows

	DECLARE @DimensionList nvarchar(max) = ''--'d1,d2'
	DECLARE @DimensionValueWhere nvarchar(max) = ''--'d1.d1=d2.DimensionValue67 and d1.d2=d2.DimensionValue68'
	DECLARE @DimensionSelect nvarchar(max) = ''--'d1.DimensionValue67, d1.DimensionValue68'
	DECLARE @DimensionSelect2 nvarchar(max) = ''--'d2.DimensionValue67, d2.DimensionValue68'

	DECLARE @ComputeAllValuesWhere nvarchar(max)=''--'d1.d2=d2.DimensionValue68 and d3.dimensionID=67 and d1.d1=d3.id'
	DECLARE @ComputeAllValuesSelect nvarchar(max)=''--'d3.id, d1.DimensionValue68'
	DECLARE @DateDimensionNumber nvarchar(max)=''--'67'
	DECLARE @ComputeAllValuesNewWhere nvarchar(max)=''--'d2.d1=d3.id and d2.d2=d1.DimensionValue68'

	DECLARE @Part2 nvarchar (max)=''
	DECLARE @Part3 nvarchar (max)=''



	DECLARE @DimensionValues table(dv int, val int)

	delete from @DimensionValues

	insert into @DimensionValues
	select * from dbo.FindDimensionsForTable(@TABLENAME,@DIMENSIONS)


	DECLARE @i int=1
	while @i<=@DIMENSIONS
	BEGIN

		set @PART2 = @PART2 + 	'ALTER TABLE [dbo].[' + @TABLENAME + '_New]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + '_New_DimensionValue' + cast(@i as nvarchar) + @CurDate + '] FOREIGN KEY([d' + cast(@i as nvarchar) + '])
		REFERENCES [dbo].[DimensionValue_New] ([id])

		ALTER TABLE [dbo].[' + @TABLENAME + '_New] CHECK CONSTRAINT [FK_' + @TABLENAME + '_New_DimensionValue' + cast(@i as nvarchar) + @CurDate + ']';

		set @PART3 = @PART3 + '[d' + cast(@i as nvarchar) + '] [int] NOT NULL, ';



		SET @DimensionList = @DimensionList + 'd' + cast(@i as nvarchar)
		DECLARE @curDimensionNum int = (select val from @DimensionValues where dv=@i)
		SET @DimensionValueWhere = @DimensionValueWhere + 'd1.d' + cast(@i as nvarchar) + '=d2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
		SET @DimensionSelect = @DimensionSelect + 'd1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
		SET @DimensionSelect2 = @DimensionSelect2 + 'd2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '

		if @i<>@DateDimension
		BEGIN
			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + 'd1.d' + cast(@i as nvarchar) + '=d2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + 'd1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + 'd2.d' + cast(@i as nvarchar) + '=d1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '

		END
		else
		BEGIN
			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + 'd3.ID=' + cast(@curDimensionNum as nvarchar) + ' and d1.d' + cast(@i as nvarchar) + '=d3.id '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + 'd3.id '
			SET @DateDimensionNumber=cast(@curDimensionNum as nvarchar)
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + 'd2.d' + cast(@i as nvarchar) + '=d3.id '
		END

		if @i<@DIMENSIONS
		BEGIN
			SET @DimensionList = @DimensionList + ', '
			SET @DimensionValueWhere = @DimensionValueWhere + ' and '
			SET @DimensionSelect = @DimensionSelect + ', '
			SET @DimensionSelect2 = @DimensionSelect2 + ', '

			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + ' and '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + ', '
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + ' and '
		END

		SET @i=@i+1
	END

	--Union added for ticket #699
	DECLARE @NonComputeAllValuesUnion nvarchar(max)='	union 
	select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
	where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base d2 where ' + @DimensionValueWhere + ' and d2.dirty=1)
	'
	IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = @TABLENAME AND Object_ID = Object_ID(@DIMENSIONTABLENAME + '_Base'))
	BEGIN
		--If this is a new dimension and it didn't exist before
		SET @NonComputeAllValuesUnion=''
	END



	DECLARE @NonComputeAllValuesQuery nvarchar(max)=

	'SET IDENTITY_INSERT ' + @TABLENAME + '_New on

	insert into ' + @TABLENAME + '_New (id, ' + @DimensionList + ')

	select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
	where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base_New d2 where ' + @DimensionValueWhere + ')

	' + @NonComputeAllValuesUnion + '

	SET IDENTITY_INSERT ' + @TABLENAME + '_New off

	DECLARE @Seed int = (Select IDENT_CURRENT(''' + @TABLENAME + '''))+1

	DBCC CHECKIDENT(''' + @TABLENAME + '_New'', RESEED, @Seed)

	insert into ' + @TABLENAME + '_New 

	select distinct ' + @DimensionSelect2 + ' from ' + @DIMENSIONTABLENAME + '_Base_New d2
	where not exists (select d1.id from ' + @TABLENAME + '_New d1 where ' + @DimensionValueWhere + ')'


	--In the case of compute all values we just need to get all of the right values, here are the steps
	--1) Turn Identity insert on
	--2) Run a query to find the values that exist in the current table and insert with those identities
	--3) Turn Identity insert off
	--4) Set the identity seed to one more than the one from the old table
	--5) Insert any new rows

	--Union added for ticket #699
	DECLARE @ComputeAllValuesUnion nvarchar(max)='
			union
		select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
		where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base d2 
					cross join @cVals d3
		where ' + @ComputeAllValuesWhere + ' and d2.dirty=1)'

	IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = @TABLENAME AND Object_ID = Object_ID(@DIMENSIONTABLENAME + '_Base'))
	BEGIN
		--If this is a new dimension and it didn't exist before
		SET @ComputeAllValuesUnion=''
	END


	DECLARE @ComputeAllValuesQuery nvarchar(max)=

	'DECLARE @dVals table(id int)
	DECLARE @cVals table(id int)

	insert into @dVals
	select id from DimensionValue where dimensionID=' + @DATEDIMENSIONNUMBER + '

	while (select count(*) from @dVals) > 0
	BEGIN
		DELETE from @cVals
		insert into @cVals
		select top 100 id from @dVals

		SET IDENTITY_INSERT ' + @TABLENAME + '_New on

		insert into ' + @TABLENAME + '_New (id, ' + @DimensionList +')

		select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
		where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base_New d2 
					cross join @cVals d3
		where ' + @ComputeAllValuesWhere + ')

		' + @ComputeAllValuesUnion + '

		SET IDENTITY_INSERT ' + @TABLENAME + '_New off

		DECLARE @Seed int = (Select IDENT_CURRENT(''' + @TABLENAME + '''))+1

		DBCC CHECKIDENT(''' + @TABLENAME + '_New'', RESEED, @Seed)

		insert into ' + @TABLENAME + '_New 

		select distinct ' + @ComputeAllValuesSelect + ' from ' + @DIMENSIONTABLENAME + '_Base_New d1
		cross join @cVals d3
		where d3.ID=' + @DateDimensionNumber + ' and
		not exists (select d2.id from ' + @TABLENAME + '_New d2 where ' + @ComputeAllValuesNewWhere + ')

		delete from @dVals where id in (select id from @cVals)
	END
	'




	if @ComputeAllValues=0
	BEGIN
		SET @INSERTQUERY = @NonComputeAllValuesQuery;
	END
	ELSE
	BEGIN
		SET @INSERTQUERY = @ComputeAllValuesQuery;
	END


	SET @TABLENAME=@TABLENAME+'_New'

	SET @CREATE_QUERY = N'

			if object_id(N''' + @TABLENAME + ''',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + ']
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + '](
				[id] [int] IDENTITY(1,1) NOT NULL, ' + @PART3 + '
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + '] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + '] REBUILD PARTITION = ALL
			WITH (DATA_COMPRESSION = ROW); 

			' + @PART2;
	
	SET @CREATEVALUEQUERY=N'
			if object_id(N''' + @TABLENAME + 'Value'',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + 'Value]
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + 'Value](
				[id] [int] IDENTITY(1,1) NOT NULL, 
				[' + left(@TABLENAME, len(@TABLENAME)-4) + '] int NOT NULL, 
				[Measure] [int] NOT NULL,
				[Value] [float] NOT NULL,
				[Rows] [float] null,
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + 'Value] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate + '] FOREIGN KEY([' + left(@TABLENAME, len(@TABLENAME)-4) + '])
			REFERENCES [dbo].[' + @TABLENAME + '] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate +']
			
			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + '] FOREIGN KEY([Measure])
			REFERENCES [dbo].[Measure] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + ']'
			
		--Before we drop the table, we need to eliminate all constraints on the table

		CREATE TABLE #Keys
		(
		 PKTABLE_QUALIFIER varchar(max),
		 PKTABLE_OWNER varchar(max),
		 PKTABLE_NAME varchar(max),
		 PKCOLUMN_NAME varchar(max),
		 FKTABLE_QUALIFIER varchar(max),
		 FKTABLE_OWNER varchar(max),
		 FKTABLE_NAME varchar(max),
		 FKCOLUMN_NAME varchar(max),
		 KEY_SEQ int,
		 UPDATE_RULE int,
		 DELETE_RULE int,
		 FK_NAME varchar(max),
		 PK_NAME varchar(max),
		 DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= @TABLENAME

		DECLARE @FKTABLE varchar(max);
		DECLARE @FKNAME varchar(max);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(max);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;

			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END
			exec (@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor



		drop table #Keys


		--select @CREATE_QUERY
		SET @CURQUERYFORERROR=@CREATE_QUERY
		if @DEBUG=1
		BEGIN
			print @CREATE_QUERY
		END
		exec (@CREATE_QUERY);

		--Okay, now we need to populate the table
		--select @INSERTQUERY

		SET @CURQUERYFORERROR=@INSERTQUERY
		if @DEBUG=1
		BEGIN
			print @INSERTQUERY
		END
		exec (@INSERTQUERY)

		--Release space used in the insert query
		dbcc shrinkdatabase (tempdb, 10) 


		SET @CURQUERYFORERROR=@CREATEVALUEQUERY
		if @DEBUG=1
		BEGIN
			print @CREATEVALUEQUERY
		END
		exec (@CREATEVALUEQUERY)

		print 'After everything'

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues, @DATEDIMENSION
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor
END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DynamicTableCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END


GO
--26

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'EmailStatisticsByDivision_Comcast') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE EmailStatisticsByDivision_Comcast
END
GO

CREATE PROCEDURE EmailStatisticsByDivision_Comcast
AS
BEGIN

IF OBJECT_ID('EmailStatisticsByDivision_Comcast_Table', 'U') IS NOT NULL 
  DROP TABLE EmailStatisticsByDivision_Comcast_Table; 

CREATE TABLE EmailStatisticsByDivision_Comcast_Table 
(AssetName NVARCHAR(255),
Division NVARCHAR(255),
Sends INT,
OpenRate FLOAT,
ClickThruRate FLOAT,
BounceRate FLOAT,
UnsubscribeRate FLOAT)

INSERT INTO EmailStatisticsByDivision_Comcast_Table
SELECT ea.assetName
	, ISNULL(ec.Division, '(blank)') AS Division
	, COUNT(*) AS Sends
	, CAST(SUM(CAST(ea.isOpened AS INT)) AS FLOAT)/COUNT(*) AS OpenRate
	, CAST(SUM(CAST(ea.isClickThrough AS INT)) AS float)/CASE WHEN SUM(CAST(isOpened AS INT)) = 0 THEN 0.000000001 ELSE SUM(CAST(isOpened AS INT)) END AS ClickThruRate
	, CAST(SUM(CAST(ea.isBounceback AS INT)) AS float)/COUNT(*) AS BounceRate
	, CAST(SUM(CAST(ea.isunsubscribed AS INT)) AS float)/COUNT(*) AS UnsubscribeRate
FROM _Emails_View d1
INNER JOIN _EmailActivity ea ON (d1.EmailName = ea.AssetName)
LEFT JOIN _Eloqua_Contacts ec ON (ea.EmailAddress = ec.EmailAddress)
GROUP BY ea.assetName, ec.Division

END
Go
--27
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDashboardContent') AND xtype IN (N'P'))
    DROP PROCEDURE [GetDashboardContent]
GO

CREATE PROCEDURE [dbo].[GetDashboardContent]
(
	@HomepageId INT = 0,
	@DashboardId INT = 0,
	@DashboardPageId INT = 0,
	@UserId UNIQUEIDENTIFIER = '14d7d588-cf4d-46be-b4ed-a74063b67d66'
)
AS
BEGIN
	DECLARE @ColumnNames NVARCHAR(MAX) 
	SELECT @ColumnNames = COALESCE(@ColumnNames + ', ', '') +  AttributeKey FROM (SELECT DISTINCT  '[' + AttributeKey + ']' AttributeKey FROM ChartOptionAttribute)  A
	IF(@ColumnNames IS NULL OR @ColumnNames ='')
		SET @ColumnNames = 'X'
		IF(@HomepageId != 0)
		BEGIN
			--First Query
			SELECT HC.Id, HC.DashboardContentId, HC.KeyDataId, HC.DashboardId, HC.RowNumber, HC.DisplayOrder, HC.DisplayTitle, HC.Height, HC.Width, HC.DisplayDateStamp, D.* 
			FROM Dashboard D INNER JOIN HomePageContent HC ON HC.DashboardId = D.id  LEFT JOIN DashboardContents DC ON DC.id=HC.DashboardContentId  
			WHERE HC.HomepageID = @HomepageId AND D.IsDeleted = 0 AND ISNULL(DC.IsDeleted,0)=0  ORDER BY D.DisplayOrder;
				
			--Second Query
			;WITH Data AS( 
				SELECT 
						D.Rows,
						D.Columns,
						DC.ReportTableId,
						ISNULL(DC.HelpTextId, 0) HelpTextId,
						ISNULL(T.ShowFooterRow,0) ShowFooterRow,
						ISNULL(T.IsGoalDisplay,0) IsGoalDisplay,
						ISNULL(T.IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,
						ISNULL(T.DateRangeGoalOption,1) DateRangeGoalOption,
						ISNULL(T.IsComparisonDisplay,0) IsComparisonDisplay,
						ISNULL(T.ComparisonType,0) ComparisonType,
						ISNULL(T.IsGoalDisplayAsColumn,0) IsGoalDisplayAsColumn,
						DC.ReportGraphId,
						DC.DisplayName,
						DC.Height,
						DC.Width, 
						ROW_NUMBER() OVER (PARTITION BY D.id ORDER BY Dc.DisplayOrder) DisplayOrder,
						D.id DashboardId,
						IsSubDashboard = CASE WHEN D.ParentDashboardId IS NULL THEN 1 ELSE 0 END,
						DC.id DashboardContentId,
						DisplayIfZero = ISNULL(DC.DisplayIfZero,''),
						SortColumnNumber = ISNULL(T.SortColumnNumber,1),
						SortDirection = UPPER(ISNULL(T.SortDirection,'ASC')) 
				FROM Dashboard D 
					INNER JOIN  DashboardContents DC on D.id = DC.DashboardId 
					INNER JOIN  HomepageContent HC on DC.id = HC.DashboardContentId 
					LEFT JOIN ReportTable T ON T.id = DC.ReportTableId 
				WHERE 
					D.IsDeleted=0 AND 
					DC.IsDeleted=0
				),
				RowNumData AS ( 
					SELECT 
							*,
							RowNum = CASE WHEN DisplayOrder % Columns <> 0 THEN (DisplayOrder / Columns) + 1  ELSE (DisplayOrder / Columns) END FROM Data 
				),
				CalculatedData AS (
					SELECT 
							*,
							CalculatedWidth1 = CASE WHEN Width IS NOT NULL THEN Width ELSE CAST((100 - SUM(Width) OVER (PARTITION BY RowNum)) AS DECIMAL)/CAST((COUNT(*) OVER (PARTITION BY RowNum) - COUNT(Width) OVER (PARTITION BY RowNum)) AS DECIMAL) END,
							CalculatedHeight1 = MAX(Height) OVER (PARTITION BY RowNum) 
					FROM RowNumData
				)
				SELECT 
						*,
						CalculatedHeight = CASE WHEN CalculatedHeight1 IS NULL THEN 350 ELSE CalculatedHeight1 END, 
						CalculatedWidth = CASE WHEN CalculatedWidth1 IS NULL THEN CAST(100/Columns AS DECIMAL) ELSE CalculatedWidth1 END ,
						AxisCount = CASE WHEN ReportTableId IS NOT NULL THEN  (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId) ELSE  (SELECT COUNT(*) FROM ReportAxis WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) END,
						IsDateDimensionOnly = CASE WHEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN  CASE WHEN (SELECT ISNULL(Dimension.IsDateDimension,0) FROM ReportAxis INNER JOIN Dimension ON Dimension.Id = ReportAxis.Dimensionid WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN 1 ELSE 0 END ELSE 0 END,
						DisplayAsColumn = (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId AND ISNULL(DisplayAsColumn,0) = 1),
						DashboardContentId 
				FROM CalculatedData ORDER BY DisplayOrder


				exec('
						;WITH ChartAttrb AS
						(
							SELECT * FROM (
									SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
									LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
							) AS R
							PIVOT 
							(
								MIN(AttributeValue)
								FOR AttributeKey IN ('+ @ColumnNames +')
							) AS A
						)

						SELECT G.id,G.id ReportGraphId,DC.DashboardId,ISNULL(DC.HelpTextId, 0) HelpTextId,HC.DashboardContentId, G.GraphType ChartType,(select top 1 symboltype from ReportGraphColumn where ReportGraphId=G.ID ) AS SymbolType,
						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) > 1) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) = 0) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							ELSE
								(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1)
							END)) AS TotalDecimalPlaces, 

						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') > 1) 								
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN									''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
									from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') = 0)
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
								from ReportGraph where id=G.ID)	
							ELSE		
								(select top 1 (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(MagnitudeValue, ''0'') <> ''0'' and									LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15''))
							END)) AS MagnitudeValue,

						 G.IsLableDisplay,G.LabelPosition,G.IsLegendVisible,G.LegendPosition,G.IsDataLabelVisible,G.DataLablePosition,G.DefaultRows, G.ChartAttribute,G.IsComparisonDisplay,IsNULL(G.IsIndicatorDisplay,0) IsIndicatorDisplay,G.CustomQuery,ISNULL(G.IsGoalDisplay,0) IsGoalDisplay,ISNULL(G.IsTrendLineDisplay,0) IsTrendLineDisplay,ISNULL(IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,DateRangeGoalOption, A.*, ChartColor = dbo.GetColor(A.ColorSequenceNo), ISNULL(G.IsSortByValue,0) AS IsSortByValue, G.SortOrder,ISNULL(G.DisplayGoalAsLine,0) AS DisplayGoalAsLine
						FROM HomepageContent HC
						INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id 
						INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId
						LEFT OUTER JOIN ChartAttrb A ON A.ReportGraphId1 = G.id 
						WHERE HC.HomepageId = '+ @HomepageId +'
				')

				IF ((SELECT COUNT(*) FROM HomepageContent HC INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId WHERE HC.HomepageId = @HomepageId					and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet') > 0)
				BEGIN
					SELECT RG.id, (case when RGC.TotalDecimalPlaces > 0 then RGC.TotalDecimalPlaces else RG.TotalDecimalPlaces end) AS TotalDecimalPlaces,

					(CASE WHEN RGC.MagnitudeValue IS NOT NULL THEN 
					/* In Case of NOT NULL value in ReportTableColumn table */
					(CASE 
						WHEN LOWER(RGC.MagnitudeValue) = 'k' THEN 3 
						WHEN LOWER(RGC.MagnitudeValue) = 'm' THEN 6 
						WHEN LOWER(RGC.MagnitudeValue) = 'b' THEN 9 
						WHEN LOWER(RGC.MagnitudeValue) = 't' THEN 12 
						WHEN LOWER(RGC.MagnitudeValue) = 'q' THEN 15 
						WHEN (LOWER(RGC.MagnitudeValue) IN ('3','6','9','12','15')) THEN RGC.MagnitudeValue 
						ELSE 
							/* In Case of Wrong value in ReportTableColumn table */
							(CASE 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
								WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
								ELSE 0 
							END)
						END) 
						ELSE 
						/* In Case of NULL value in ReportTableColumn table */
						(CASE 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
							WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
							ELSE 0 
						END) 
					END) AS MagnitudeValue
					FROM HomepageContent HC 
					INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id 
					INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
					inner join ReportGraphColumn RGC on (RGC.ReportGraphId = RG.id)
					WHERE HC.HomepageId = @HomepageId
				END
		END
		ELSE IF(@DashboardId != 0)
		BEGIN
			--First Query
			SELECT D.Id,D.Name,D.DisplayName,D.Rows,D.Columns,D.ParentDashboardId FROM Dashboard D WHERE D.Id = @DashboardId AND D.IsDeleted=0 ORDER BY D.DisplayOrder;
					
			--Second Query
			;WITH Data AS( 
			SELECT 
					D.Rows,
					D.Columns,
					DC.ReportTableId,
					ISNULL(DC.HelpTextId, 0) HelpTextId,
					DC.KeyDataId,
					ISNULL(T.ShowFooterRow,0) ShowFooterRow,
					ISNULL(T.IsGoalDisplay,0) IsGoalDisplay,
					ISNULL(T.IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,
					ISNULL(T.DateRangeGoalOption,1) DateRangeGoalOption,
					ISNULL(T.IsComparisonDisplay,0) IsComparisonDisplay,
				    ISNULL(T.ComparisonType,0) ComparisonType,
					ISNULL(T.IsGoalDisplayAsColumn,0) IsGoalDisplayAsColumn,
					DC.ReportGraphId,
					DC.DisplayName,
					DC.Height,
					DC.Width,
					DC.IsCumulativeData, 
					ROW_NUMBER() OVER (PARTITION BY D.id ORDER BY Dc.DisplayOrder) 
					DisplayOrder,
					D.id DashboardId,
					IsSubDashboard = CASE WHEN D.ParentDashboardId IS NULL THEN 1 ELSE 0 END,
					DC.id DashboardContentId,
					DisplayIfZero = ISNULL(DC.DisplayIfZero,''),
					SortColumnNumber = ISNULL(T.SortColumnNumber,1),
					SortDirection = UPPER(ISNULL(T.SortDirection,'ASC')) 
			FROM Dashboard D 
				INNER JOIN  DashboardContents DC on D.id = DC.DashboardId  
				--LEFT JOIN ReportGraph RG ON RG.id = DC.ReportGraphId 
				LEFT JOIN ReportTable T ON T.id = DC.ReportTableId 
			WHERE 
					(D.id = @DashboardId OR D.ParentDashboardid = @DashboardId) AND 
					ISNULL(DC.DashboardPageID,0)= @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0
			),
			RowNumData AS ( 
				SELECT 
						*,
						RowNum = CASE WHEN DisplayOrder % Columns <> 0 THEN (DisplayOrder / Columns) + 1  ELSE (DisplayOrder / Columns) END FROM Data ),
						CalculatedData AS (SELECT *,CalculatedWidth1 = CASE WHEN Width IS NOT NULL THEN Width ELSE CAST((100 - SUM(Width) OVER (PARTITION BY RowNum)) AS DECIMAL)/CAST((COUNT(*) OVER (PARTITION BY RowNum) - COUNT(Width) OVER (PARTITION BY RowNum)) AS DECIMAL) END,
						CalculatedHeight1 = MAX(Height) OVER (PARTITION BY RowNum) 
				FROM RowNumData
			)
			SELECT 
					*,
					CalculatedHeight = CASE WHEN CalculatedHeight1 IS NULL THEN 350 ELSE CalculatedHeight1 END, 
					CalculatedWidth = CASE WHEN CalculatedWidth1 IS NULL THEN CAST(100/Columns AS DECIMAL) ELSE CalculatedWidth1 END ,
					AxisCount = CASE WHEN ReportTableId IS NOT NULL THEN  (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId) ELSE  (SELECT COUNT(*) FROM ReportAxis WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) END,
					IsDateDimensionOnly = CASE WHEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN  CASE WHEN (SELECT ISNULL(Dimension.IsDateDimension,0) FROM ReportAxis INNER JOIN Dimension ON Dimension.Id = ReportAxis.Dimensionid WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN 1 ELSE 0 END ELSE 0 END, 
					DisplayAsColumn = (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId AND ISNULL(DisplayAsColumn,0) = 1),
					DashboardContentId 
			FROM CalculatedData 
			ORDER BY 
					DisplayOrder;
			
			--Third Query
			SELECT D.Id, D.Name, D.DisplayName, D.Rows, D.Columns, D.ParentDashboardId, 0 IsSubDashboard, ISNULL(D.HelpTextId, 0) HelpTextId FROM Dashboard D  
			INNER JOIN	(
							SELECT DISTINCT DashboardId,UserId FROM User_Permission
						) UP ON UP.DashboardId = D.id 
			WHERE ParentDashboardId = @DashboardId AND UP.UserId = @UserId AND D.IsDeleted=0 ORDER BY DisplayOrder

			SET @DashboardPageId = ISNULL(@DashboardPageId,0)
			--Forth Query
			exec('
						;WITH ChartAttrb AS
						(
							SELECT * FROM (
									SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
									LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
							) AS R
							PIVOT 
							(
								MIN(AttributeValue)
								FOR AttributeKey IN ('+ @ColumnNames +')
							) AS A
						)

						SELECT  G.id,G.id ReportGraphId,D.id DashboardId, DC.Id DashboardContentId,ISNULL(DC.HelpTextId, 0) HelpTextId, G.GraphType ChartType,(select top 1 symboltype from ReportGraphColumn where ReportGraphId=G.Id ) AS SymbolType,
						--(select (CASE 
						--	WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0) > 1) 
						--		THEN (select TotalDecimalPlaces from ReportGraph where id=G.ID)
						--	WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0) = 0) 
						--		THEN (select TotalDecimalPlaces from ReportGraph where id=G.ID)
						--	ELSE
						--		(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0)
						--	END)) AS TotalDecimalPlaces, 
						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) > 1) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) = 0) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							ELSE
								(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1)
							END)) AS TotalDecimalPlaces, 

						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') > 1) 								
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN									''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
									from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') = 0)
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
								from ReportGraph where id=G.ID)	
							ELSE		
								(select top 1 (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(MagnitudeValue, ''0'') <> ''0'' and									LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15''))
							END)) AS MagnitudeValue,

						 G.IsLableDisplay,G.LabelPosition,G.IsLegendVisible,G.LegendPosition,G.IsDataLabelVisible,G.DataLablePosition,G.DefaultRows, G.ChartAttribute,G.IsComparisonDisplay,IsNULL(G.IsIndicatorDisplay,0) IsIndicatorDisplay,ISNULL(G.IsTrendLineDisplay,0) IsTrendLineDisplay,G.CustomQuery,ISNULL(G.IsGoalDisplay,0) IsGoalDisplay,ISNULL(IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,DateRangeGoalOption, A.*, ChartColor = dbo.GetColor(A.ColorSequenceNo), ISNULL(G.IsSortByValue,0) AS IsSortByValue, G.SortOrder, ISNULL(G.DisplayGoalAsLine,0) AS DisplayGoalAsLine
						FROM Dashboard D
						INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
						INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId
						LEFT OUTER JOIN ChartAttrb A ON A.ReportGraphId1 = G.id 
						WHERE (D.Id  = '+ @DashboardId +' OR D.ParentDashboardId  = '+ @DashboardId +') AND ISNULL(DC.DashboardPageID,0) = '+ @DashboardPageId +' AND 
						D.IsDeleted=0 AND DC.IsDeleted=0
				')
			IF ((SELECT COUNT(*) FROM Dashboard D 
				INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
				INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
				WHERE (D.Id  = @DashboardId OR D.ParentDashboardId  = @DashboardId) AND ISNULL(DC.DashboardPageID,0) = @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0) > 0)
			BEGIN
				SELECT RG.id, (case when RGC.TotalDecimalPlaces > 0 then RGC.TotalDecimalPlaces else RG.TotalDecimalPlaces end) AS TotalDecimalPlaces,

				(CASE WHEN RGC.MagnitudeValue IS NOT NULL THEN 
				/* In Case of NOT NULL value in ReportTableColumn table */
				(CASE 
					WHEN LOWER(RGC.MagnitudeValue) = 'k' THEN 3 
					WHEN LOWER(RGC.MagnitudeValue) = 'm' THEN 6 
					WHEN LOWER(RGC.MagnitudeValue) = 'b' THEN 9 
					WHEN LOWER(RGC.MagnitudeValue) = 't' THEN 12 
					WHEN LOWER(RGC.MagnitudeValue) = 'q' THEN 15 
					WHEN (LOWER(RGC.MagnitudeValue) IN ('3','6','9','12','15')) THEN RGC.MagnitudeValue 
					ELSE 
						/* In Case of Wrong value in ReportTableColumn table */
						(CASE 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
							WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
							ELSE 0 
						END)
					END) 
					ELSE 
					/* In Case of NULL value in ReportTableColumn table */
					(CASE 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
						WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
						ELSE 0 
					END) 
				END) AS MagnitudeValue
				FROM Dashboard D 
				INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
				INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
				inner join ReportGraphColumn RGC on (RGC.ReportGraphId = RG.id)
				WHERE (D.Id  = @DashboardId OR D.ParentDashboardId  = @DashboardId) AND ISNULL(DC.DashboardPageID,0) = @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0
			END
		END
END

Go
--28
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetKeyDataDetails') AND xtype IN (N'P'))
    DROP PROCEDURE GetKeyDataDetails
GO


-- ==========================================================
-- Author:		Nandish Shah
-- Create date: 12/14/2015
-- Description:	Add Magnitude Value in KeyData 
-- ==========================================================
CREATE PROCEDURE [dbo].[GetKeyDataDetails]
@KeyDataId INT, 
@DimensionTableName NVARCHAR(100), 
@StartDate DATE='1-1-1900', 
@EndDate DATE='1-1-2100', 
@CompStartDate DATE='1-1-1900', 
@CompEndDate DATE='1-1-2100', 
@DateField nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15)=null,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
AS
BEGIN
	DECLARE @KeyDataActual FLOAT,
			@KeyDataComp FLOAT,
			@TempValue FLOAT,
			@FormattedKeyDataActual NVARCHAR(50),
			@FormattedKeyDataTooltip NVARCHAR(50),
			@FormattedKeyDataComp NVARCHAR(50),
			@KDActualOutput NVARCHAR(50),
			@KDCompOutput NVARCHAR(50),
			@CompDataTooltip NVARCHAR(50),
			@ComparisonColor NVARCHAR(20),
			@ComparisonDiffValue FLOAT;
	
	-- get and set properties of key data
	DECLARE @KeyDataName NVARCHAR(50),
			@MeasureId INT,
			@Prefix NVARCHAR(10),
			@Suffix NVARCHAR(10),
			@DisplayIfZero NVARCHAR(20),
			@NoOfDecimal INT,
			@ComparisonType NVARCHAR(10),
			@IsComparisonDisplay BIT,
			@IsIndicatorDisplay BIT,
			@IsGoalDisplayForFilterCriteria BIT,
			@DateRangeGoalOption INT,
			@MagnitudeValue nvarchar(10),
			@HelpTextId int
	
	SELECT  @KeyDataName = KeyDataName,
			@MeasureId = MeasureId,
			@Prefix = Prefix ,
			@Suffix = Suffix,
			@DisplayIfZero = DisplayIfZero,
			@NoOfDecimal = (CASE WHEN ISNULL(NoOfDecimal , -1) < -1 THEN 2 ELSE NoOfDecimal END),
			@ComparisonType = ISNULL(ComparisonType,'ABSOLUTE'),
			@IsComparisonDisplay = ISNULL(IsComparisonDisplay,0),
			@IsIndicatorDisplay = ISNULL(IsIndicatorDisplay,0),
			@IsGoalDisplayForFilterCriteria = ISNULL(IsGoalDisplayForFilterCriteria,0),
			@DateRangeGoalOption = DateRangeGoalOption,
			@MagnitudeValue = (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'k' THEN 3 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'm' THEN 6 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'b' THEN 9 WHEN LOWER(ISNULL									(MagnitudeValue,0)) = 't' THEN 12 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'q' THEN 15 WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN MagnitudeValue ELSE 0 END),
			@HelpTextId = ISNULL(HelpTextId, 0) 
	FROM KeyData WHERE id = @KeyDataId

	-- Get actual key data value
	SET @TempValue = 0;
	EXECUTE KeyDataGet @KeyDataId,@DimensionTableName,@StartDate,@EndDate,@DateField,@FilterValues,@ViewByValue,@UserId,@RoleId,@TempValue OUTPUT;
	SET @KeyDataActual = @TempValue;

	DECLARE @MagSymbol nvarchar(10)
	SET @MagSymbol = (CASE WHEN @MagnitudeValue = '3' THEN 'k' WHEN @MagnitudeValue = '6' THEN 'M' WHEN @MagnitudeValue = '9' THEN 'B' WHEN @MagnitudeValue = '12' THEN 'T' WHEN @MagnitudeValue = '15' THEN 'Q' ELSE '0' END)
	
	IF(ISNUMERIC(@KeyDataActual) = 1)
	BEGIN
		BEGIN /* Region of Actual Key Data Value */
			-- Replace DisplayIfZero value 
			IF(ISNULL(@DisplayIfZero,'') != '' AND @KeyDataActual = 0)
			BEGIN
				SET @FormattedKeyDataActual = @DisplayIfZero
			END
			ELSE
			BEGIN
				-- Replace actual key data value with measure output value from MeasureOutputValue table
				SET @KDActualOutput = dbo.GetMeasureOutputValue(@KeyDataActual,@MeasureId)
				IF(ISNUMERIC(@KDActualOutput) = 1)
				BEGIN
					-- Format key data value as per NoOfDecimal property

					DECLARE @StrFormat NVARCHAR(20) = ''
					DECLARE @TooltipStrFormat NVARCHAR(20) = ''
					DECLARE @MagDivVal NVARCHAR(20) = ''
					DECLARE @MagDivValActual FLOAT
					IF(@MagnitudeValue != 0)
					begin
						SET @MagDivVal = '1' + REPLACE(STR('0',CAST(@MagnitudeValue AS int)),' ','0')
						SET @MagDivValActual = CAST(@KDActualOutput AS FLOAT) / CAST(@MagDivVal AS float)

						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@MagDivValActual AS FLOAT)) > 0)
							BEGIN
								SET @StrFormat = REPLACE(STR('0',2),' ','0')
							END
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @TooltipStrFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
							SET @TooltipStrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrFormat,'') != '')
						begin
							SET @StrFormat = '#,##0.' + @StrFormat 
							SET @TooltipStrFormat = '#,##0.' + @TooltipStrFormat 
							end
						ELSE
						begin
							SET @StrFormat = '#,##0'
							SET @TooltipStrFormat = '#,##0'
						end
						
						SET @FormattedKeyDataActual = FORMAT(@MagDivValActual,@StrFormat) + @MagSymbol
						SET @FormattedKeyDataTooltip = FORMAT(@KeyDataActual,@TooltipStrFormat)
					end
					else
					begin
						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @StrFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrFormat,'') != '')
						begin
							SET @StrFormat = '#,##0.' + @StrFormat 
							end
						ELSE
						begin
							SET @StrFormat = '#,##0'
						end
						SET @FormattedKeyDataActual = FORMAT(CAST(@KDActualOutput AS FLOAT),@StrFormat)
						SET @FormattedKeyDataTooltip = FORMAT(@KeyDataActual,@StrFormat)
					end
				END
				ELSE
				BEGIN
					SET @FormattedKeyDataActual = @KDActualOutput
				END
			END
			-- Add prefix and suffix to key data
			SET @FormattedKeyDataActual = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataActual +
					       				  CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	

			SET @FormattedKeyDataTooltip = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataTooltip +
											CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	
		END /* Region of Actual Key Data Value */

		BEGIN /* Region of Key Data Comparison Value */
			IF(@IsComparisonDisplay = 1)
			BEGIN
				-- Get comparison value for key data
				SET @TempValue = 0;
				EXECUTE KeyDataGet @KeyDataId,@DimensionTableName,@CompStartDate,@CompEndDate,@DateField,@FilterValues,@ViewByValue,@UserId,@RoleId ,@TempValue OUTPUT;
				SET @KeyDataComp = @TempValue;

				IF(ISNUMERIC(@KeyDataComp) = 1)
				BEGIN
					-- Set color of the comparison value
					IF(@KeyDataActual < @KeyDataComp)
						SET @ComparisonColor = 'red'
					ELSE
						SET @ComparisonColor = 'green'

					-- Get comparison value of key data
					IF(UPPER(@ComparisonType) = 'RATE')
					BEGIN
						IF(@KeyDataComp = 0)
							SET @ComparisonDiffValue = 100;
						ELSE IF(@KeyDataActual = @KeyDataComp)
						BEGIN
							SET @ComparisonDiffValue = 0;
							--SET @ComparisonColor = 'blue'
						END
						ELSE
							SET @ComparisonDiffValue = ((@KeyDataActual - @KeyDataComp) * 100) / @KeyDataComp;
					END
					ELSE 
					BEGIN
						SET @ComparisonDiffValue = @KeyDataActual - @KeyDataComp;
					END
					IF(@ComparisonDiffValue < 0)
						SET @ComparisonDiffValue = @ComparisonDiffValue * -1;


					-- Format key data comparison value to default decimal format
					DECLARE @StrCompFormat NVARCHAR(20) = ''
					DECLARE @TooltipStrCompFormat NVARCHAR(20) = ''

					DECLARE @MagDivValComp FLOAT
					IF(@MagnitudeValue != 0)
					BEGIN
						SET @MagDivValComp = CAST(@ComparisonDiffValue AS FLOAT) / CAST(@MagDivVal AS float)

						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@MagDivValComp AS FLOAT)) > 0)
							BEGIN
								SET @StrCompFormat = REPLACE(STR('0',2),' ','0')
							END
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @TooltipStrCompFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
							SET @TooltipStrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrCompFormat,'') != '')
						begin
							SET @StrCompFormat = '#,##0.' + @StrCompFormat 
							SET @TooltipStrCompFormat = '#,##0.' + @TooltipStrCompFormat 
							end
						ELSE
						begin
							SET @StrCompFormat = '#,##0'
							SET @TooltipStrCompFormat = '#,##0'
						end

						SET @FormattedKeyDataComp = FORMAT(@MagDivValComp,@StrCompFormat) + @MagSymbol
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @TooltipStrCompFormat)
					END
					ELSE
						BEGIN
						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@ComparisonDiffValue AS FLOAT)) > 0)
							BEGIN
								SET @StrCompFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrCompFormat,'') != '')
						begin
							SET @StrCompFormat = '#,##0.' + @StrCompFormat 
							end
						ELSE
						begin
							SET @StrCompFormat = '#,##0'
						end
						SET @FormattedKeyDataComp = FORMAT(@ComparisonDiffValue, @StrCompFormat)
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @StrCompFormat)
					END
					IF(UPPER(@ComparisonType) = 'RATE')
					BEGIN
						SET @FormattedKeyDataComp = @FormattedKeyDataComp + '%'
						SET @CompDataTooltip = CAST(@ComparisonDiffValue AS nvarchar(50)) + '%'
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @StrCompFormat) + '%'
					END
					ELSE
					BEGIN
						-- Add prefix and suffix to key data comparison
						SET @FormattedKeyDataComp = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataComp +
												CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END
						
						SET @CompDataTooltip = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @CompDataTooltip +
												CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END
					END
				END
			END
		END /* Region of Key Data Comparison Value */
	END
	ELSE 
	BEGIN
		SET @KeyDataActual = 0
		SET @FormattedKeyDataActual = @DisplayIfZero
		SET @FormattedKeyDataActual = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataActual +
				       				  CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	
	END

	SELECT @KeyDataName AS KeyDataName,
		   @KeyDataActual AS KeyDataActual, 
		   @FormattedKeyDataActual AS FormattedKeyDataValue, 
		   @FormattedKeyDataTooltip AS FormattedKeyDataTooltip,
		   @FormattedKeyDataComp AS ComparisonValue, 
		   @IsComparisonDisplay AS IsComparisonDisplay,
		   @CompDataTooltip AS  ComparisonTooltip,	
		   @ComparisonColor AS ComparisonColor,
		   @IsIndicatorDisplay AS IsIndicatorDisplay,		
		   @IsGoalDisplayForFilterCriteria AS IsGoalDisplayForFilterCriteria,
		   @DateRangeGoalOption AS DateRangeGoalOption,
		   @HelpTextId AS HelpTextId 
END
Go
--29
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetRestrictedValues') AND xtype IN (N'P'))
    DROP PROCEDURE GetRestrictedValues
GO
CREATE PROCEDURE [dbo].[GetRestrictedValues]

@SelectedDiemsnionId INT=12,
@DashboardId INT=3,
@dashBoardPageId INT=0,
@Filters NVARCHAR(MAX)='<filters><filter ID="2">2554</filter><filter ID="2">2556</filter></filters>'
AS

BEGIN
DECLARE @DynamicFilters TABLE
(
 DiemnsionId INt,
 RestrictedDiemnsionId INT,
 RestrictedDiemnsionValue NVARCHAR(1000)   
)
BEGIN TRY
DECLARE @TableName NVARCHAR(1000)=''
Declare @SelectedDiemsnionCount INT=0
Declare @SelectedDiemsnionNo INT=0
DECLARE @DimensionIdTemp INT
--Following is cursor to prepare Table Name
DECLARE @TablNameCursor CURSOR
SET @TablNameCursor = CURSOR FOR
SELECT D.id from Dimension D
INNER JOIN Dimension D1 On D1.TableName=D.TableName
where D1.id =@SelectedDiemsnionId Order By id
OPEN @TablNameCursor
FETCH NEXT
FROM @TablNameCursor INTO @DimensionIdTemp
WHILE @@FETCH_STATUS = 0
BEGIN
--PRINT @DimensionId
SET @SelectedDiemsnionNo=@SelectedDiemsnionNo+1;
 SET @TableName=@TableName+'D'+CAST(@DimensionIdTemp AS NVARCHAR)
 IF(@DimensionIdTemp=@SelectedDiemsnionId)
 SET @SelectedDiemsnionCount=@SelectedDiemsnionNo

FETCH NEXT
FROM @TablNameCursor INTO @DimensionIdTemp
END
CLOSE @TablNameCursor
PRINT @TableName
DEALLOCATE @TablNameCursor
--End TableName Cursor

DECLARE @DimensionId INT
DECLARE @IsDateDiemnsion Bit=0
DECLARE @Count INT=0

DECLARE @FilterCursor CURSOR
SET @FilterCursor = CURSOR FOR
SELECT D.id,D.IsDateDimension from Dimension D
INNER JOIN Dimension D1 On D1.TableName=D.TableName
where D1.id =@SelectedDiemsnionId Order By id
OPEN @FilterCursor
FETCH NEXT
FROM @FilterCursor INTO @DimensionId,@IsDateDiemnsion
WHILE @@FETCH_STATUS = 0
BEGIN
--PRINT @DimensionId
SET @Count=@Count+1

IF(@IsDateDiemnsion=0 AND @DimensionId!=@SelectedDiemsnionId )

BEGIN

DECLARE @Query NVARCHAR(MAX)='SELECT '+CAST(@SelectedDiemsnionId AS NVARCHAR)+',DimensionId,Value from dimensionValue WHere Dimensionid in ('+CAST(@DimensionId as NVARCHAR) +' ) 
AND Value not in(
select dv2.value from '+@TableName+' dv 
Inner Join Dimensionvalue Dv2 on dv2.id=dv.d'+CAST(@Count AS NVARCHAR)+ '
Inner Join Dimension Di on di.id=dv2.DimensionID  
AND di.id='+CAST(@DimensionId AS nvarchar)+' WHERE dv.d'+CAST(@SelectedDiemsnionCount AS NVARCHAR)+' In(select DimensionValueId from dbo.ExtractTableFromXml('''+@Filters+''')))'


INSERT INTO @DynamicFilters EXEC(@Query)
END

FETCH NEXT
FROM @FilterCursor INTO @DimensionId,@IsDateDiemnsion
END
CLOSE @FilterCursor
DEALLOCATE @FilterCursor

SELECT * FROM @DynamicFilters
END TRY


BEGIN CATCH

SELECT * FROM @DynamicFilters

END CATCH
END
GO
--30
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestSignificance') AND xtype IN (N'P'))
    DROP PROCEDURE GetTTestSignificance
GO


CREATE PROCEDURE [GetTTestSignificance]  
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,	
@SubDashboardMainDimensionTable int = 0,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'

AS
BEGIN
DECLARE @Dimensions NVARCHAR(MAX),@IsDateDimensionExist BIT,@IsDateOnYAxis BIT,@TotalDimensionCount INT, @TOTALSQL NVARCHAR(MAX), 
		@DimensionId1 INT,@DimensionId2 INT, @Columns NVARCHAR(1000), @BaseQuery NVARCHAR(1000),@DimensionName1 NVARCHAR(500), @DimensionName2 NVARCHAR(500),
		@CLOSINGSQL NVARCHAR(MAX),@DistinctOnly VARCHAR(20),@CreateDimensionTable NVARCHAR(1000), @GType NVARCHAR(100),
		@IsOnlyDateDimension BIT=0;

SET @Columns='' ;SET @BaseQuery='';SET @DistinctOnly=' DISTINCT '
SELECT @GType =LOWER (GraphType) FROM ReportGraph WHERE id = @ReportGraphID

DECLARE @TTestTable TABLE(Dimension1 NVARCHAR(400) INDEX Dimension1 CLUSTERED,ColumnOrder1 NVARCHAR(500), Dimension2 NVARCHAR(500),ColumnOrder2 NVARCHAR(500), TTestValue FLOAT); 
DECLARE @TTestTableTemp TABLE(Dimension1 NVARCHAR(400) INDEX Dimension1 CLUSTERED,ColumnOrder1 NVARCHAR(500), Dimension2 NVARCHAR(500),ColumnOrder2 NVARCHAR(500), TTestValue FLOAT); 

 --Get Dimension Table 
SELECT 
		@Dimensions				= selectdimension, 
		@Columns				= CreateTableColumn,
		@IsDateDimensionExist	= IsDateDimensionExist, 
		@IsDateOnYAxis			= IsDateOnYAxis,
		@DimensionName1			= DimensionName1, 
		@DimensionName2			= DimensionName2, 
		@DimensionId1			= DimensionId1,
		@DimensionId2			= DimensionId2,
		@TotalDimensionCount	= Totaldimensioncount,
		@IsOnlyDateDimension	= CAST(IsOnlyDateDImensionExist AS BIT) 
FROM dbo.GetDimensions(@ReportGraphID,@ViewByValue,@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@GType,@SubDashboardOtherDimensionTable,@SubDashboardMainDimensionTable,'')

SET @BaseQuery=(dbo.DimensionBaseQuery(@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@ReportGraphID,@FilterValues,@IsOnlyDateDimension,1,@UserId,@RoleId))
SET @BaseQuery=REPLACE(@BaseQuery,'#COLUMNS#',@Dimensions)

-- Create dimension table
SET @CreateDimensionTable='DECLARE @DimensionTable TABLE( '+@Columns+' )'
SET @CreateDimensionTable=@CreateDimensionTable+'INSERT INTO @DimensionTable '+@BaseQuery+'';

		IF(@TotalDimensionCount > 1)
		BEGIN
			IF(@IsDateDimensionExist = 0)
			BEGIN
				SET @CLOSINGSQL = N'SELECT T1.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName2+ ',T1.OrderValue2 FROM @DimensionTable T1 ORDER BY T1.OrderValue1,T1.OrderValue2' 
			END
			ELSE
			BEGIN
				IF(@IsDateOnYAxis = 1)
				BEGIN
					SET @DimensionId2 = @DimensionId1
					SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName1+ ',T1.OrderValue1 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
				END
				ELSE
				BEGIN
					SET @DimensionId1 = @DimensionId2
					SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName2+ ',T2.OrderValue2,T1.' +@DimensionName2+ ',T1.OrderValue2 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
				END
			END
			
		END
		ELSE 
		BEGIN
			SET @DimensionId2 = @DimensionId1
			SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName1+ ',T1.OrderValue1 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
		END
		SET @TOTALSQL = @CreateDimensionTable + ' ' + @CLOSINGSQL

		INSERT INTO @TTestTableTemp(Dimension1, ColumnOrder1, Dimension2, ColumnOrder2)
		EXEC(@TOTALSQL)
		
		--Get distinct dimension value pair with order by 
		INSERT INTO @TTestTable(Dimension1, ColumnOrder1, Dimension2, ColumnOrder2)
		SELECT	DISTINCT
			Dimension1, 
			ColumnOrder1 = MIN(ColumnOrder1) OVER (PARTITION BY Dimension1), 
			Dimension2, 
			ColumnOrder2 = MIN(ColumnOrder2) OVER (PARTITION BY Dimension2)
		FROM @TTestTableTemp ORDER BY ColumnOrder1,ColumnOrder2
		
		DECLARE @Dimension1 NVARCHAR(500), @ColumnOrder1 NVARCHAR(500), @Dimension2 NVARCHAR(500), @ColumnOrder2 NVARCHAR(500),
				@TTestQuery NVARCHAR(MAX), @IsTTestValueExist float;
		
		DECLARE TTestCursor CURSOR FOR
		SELECT Dimension1,ColumnOrder1,Dimension2,ColumnOrder2 FROM @TTestTable ORDER BY ColumnOrder1
		OPEN TTestCursor
		FETCH NEXT FROM TTestCursor
		INTO @Dimension1, @ColumnOrder1, @Dimension2,@ColumnOrder2 
			WHILE @@FETCH_STATUS = 0
				BEGIN
				
				SET @IsTTestValueExist= (SELECT TOP 1 TTestValue FROM @TTestTable WHERE Dimension1=@Dimension1 AND Dimension2=@Dimension2)
				
				-- Check if TTestValue already exist
				IF(@IsTTestValueExist IS NULL)
				BEGIN
					SET @Dimension1 = 'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':' + @Dimension1;
					SET @Dimension2 = 'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':' + @Dimension2;
				
					DECLARE @PValue FLOAT;
					IF(@Dimension1 != @Dimension2)
					BEGIN
					BEGIN TRY
						IF(@GType='coheatmap')
							SET @TTestQuery = [dbo].[GetTTestQueryForCoRelation] (@Dimension1,@Dimension2,@ReportGraphID,@DIMENSIONTABLENAME,@STARTDATE ,@ENDDATE ,@DATEFIELD ,@FilterValues ,@ViewByValue ,@SubDashboardOtherDimensionTable ,@SubDashboardMainDimensionTable )
						ELSE
							SET @TTestQuery = [dbo].[GetTTestQueryForAll](@Dimension1,@Dimension2,@ViewByValue,@STARTDATE,@ENDDATE,@ReportGraphID,@DIMENSIONTABLENAME,@DATEFIELD,@FilterValues,@TotalDimensionCount) 
						
						-- call TTestCalculation stored procedure to get @PValue
						EXECUTE TTestCalculation @TTestQuery ,@PValue OUTPUT
					END TRY
					BEGIN CATCH
						SET @PValue = NULL
					END CATCH
					END
					ELSE
					BEGIN
						SET @PValue = NULL
					END
					
					--If t-test value is lesser than 0.001 set it to zero
					IF(@PValue < 0.001)
						SET @PValue = 0;

					UPDATE @TTestTable SET TTestValue = @PValue 
					WHERE (Dimension1 = REPLACE(@Dimension1,'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':','') 
						   AND Dimension2 = REPLACE(@Dimension2,'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':','')) 
						OR 
						   (Dimension2 = REPLACE(@Dimension1,'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':','') 
						   AND Dimension1 = REPLACE(@Dimension2,'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':',''))	
				END
				FETCH NEXT FROM TTestCursor
				INTO @Dimension1, @ColumnOrder1, @Dimension2,@ColumnOrder2 
			END
		CLOSE TTestCursor
		DEALLOCATE TTestCursor
		SELECT Dimension1,Dimension2,TTestValue AS MeasureValue FROM @TTestTable ORDER BY ColumnOrder1;
END

GO
--31
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'InteractionBasedAttrCalc') AND xtype IN (N'P'))
    DROP PROCEDURE [InteractionBasedAttrCalc]
GO

CREATE PROCEDURE [dbo].[InteractionBasedAttrCalc]
@AttrQuery NVARCHAR(MAX),
@AttrWhereQuery NVARCHAR(MAX),
@BaseTblName NVARCHAR(MAX)
AS
BEGIN
	BEGIN TRY
		DECLARE @Query NVARCHAR(MAX) ='',
				@WhereClause NVARCHAR(MAX)
		-- Create temp table 
		CREATE TABLE #TempData (id INT,
								OpportunityFieldname NVARCHAR(100),
								TouchDateFieldName DATE,
								Revenue FLOAT,
								IndexNo INT,
								CampaignId NVARCHAR(100),
								DimensionId INT,
								DimensionValue NVARCHAR(MAX),
								TouchWeight INT,
								RevunueAfterWeight FLOAT,
								idDenseRank INT)

		SET @Query = ' DECLARE @TempTouchData TABLE(id int NOT NULL PRIMARY KEY IDENTITY,OpportunityFieldname NVARCHAR(100),TouchDateFieldName DATE,Revenue FLOAT,IndexNo INT,  CampaignId NVARCHAR(100), DimensionId INT,								DimensionValue NVARCHAR(MAX), TouchWeight int, RevunueAfterWeight Float, idDenseRank int) '+
					' INSERT INTO @TempTouchData (idDenseRank,OpportunityFieldname,TouchDateFieldName,Revenue,IndexNo,CampaignId)'


		-- Set base table name
		SET @BaseTblName = @BaseTblName + '_Base'

		-- If base table exist then apply inner join of base table and dimension value table
		IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @BaseTblName)
		BEGIN
			DECLARE @ConfigDimensionId INT, @ConfigDimensionValue NVARCHAR(MAX)
			IF ((SELECT COUNT(DISTINCT DimensionId) FROM AttrPositionConfig WHERE DimensionId IS NOT NULL) = 1)
			BEGIN
				SET @ConfigDimensionId = (SELECT TOP 1 DimensionId FROM AttrPositionConfig WHERE DimensionId IS NOT NULL)
				DECLARE @ColumnName NVARCHAR(MAX) = 'DimensionValue' + CAST(@ConfigDimensionId AS NVARCHAR)
				IF EXISTS(SELECT * FROM sys.columns WHERE Name = @ColumnName AND Object_ID = Object_ID(@BaseTblName))
				BEGIN
					SET @Query = REPLACE(@Query, 'INSERT INTO @TempTouchData (',' INSERT INTO @TempTouchData (DV.DimensionId,DV.DimensionValue,')
					SET @Query = @Query + REPLACE(@AttrQuery,'SELECT',' SELECT DV.DimensionId, DV.DisplayValue AS DimensionValue, ')

					SET @Query = @Query + ' INNER JOIN  ' + @BaseTblName + ' WB' + ' ON objTouches.id = WB.id INNER JOIN DimensionValue DV ON WB.' + @ColumnName + ' = DV.id '	
					SET @WhereClause = @AttrWhereQuery + ' AND DV.DimensionId = ' + CAST(@ConfigDimensionId AS NVARCHAR)
				END
				ELSE
				BEGIN
					SET @WhereClause = @AttrWhereQuery
					SET @Query = @Query + @AttrQuery
				END
			END
			ELSE
			BEGIN
				SET @WhereClause = @AttrWhereQuery
				SET @Query = @Query + @AttrQuery
			END
		END
		ELSE
		BEGIN
			SET @WhereClause = @AttrWhereQuery
			SET @Query = @Query + @AttrQuery
		END

		SET @Query = @Query + ISNULL(@WhereClause,'')
		SET @Query = @Query + ' ;SELECT * FROM @TempTouchData '

		Insert INTO #TempData EXEC(@Query)

	
		DECLARE @intFlag INT
		DECLARE @TotalRow INT

		SET @intFlag = 1
		SET @TotalRow = (select count(*) from #TempData)
		-- Get Interaction based revenue value
		WHILE (@intFlag <= @TotalRow)
		BEGIN
			DECLARE @TouchDateFieldName DATETIME
			DECLARE @DimensionId INT
			DECLARE @DimensionValue NVARCHAR(MAX)
			SET @TouchDateFieldName = (SELECT TouchDateFieldName FROM #TempData WHERE id = @intFlag)
			SET @DimensionId = (SELECT DimensionId FROM #TempData WHERE id = @intFlag)
			SET @DimensionValue = (SELECT DimensionValue FROM #TempData WHERE id = @intFlag)

			UPDATE #TempData SET TouchWeight = dbo.[GetWeight](@TouchDateFieldName,@DimensionId,@DimensionValue) where id = @intFlag

			SET @intFlag = @intFlag + 1
		end

		DECLARE @intFlag2 INT
		DECLARE @TotalRow2 INT

		SET @intFlag2 = 1
		SET @TotalRow2 = (SELECT COUNT(DISTINCT idDenseRank) FROM #TempData)
		-- Calculate Revenue based on weight
		WHILE (@intFlag2 <= @TotalRow2)
		BEGIN
			DECLARE @OpportunityFieldname nvarchar(max)
			DECLARE @TotalRevenue FLOAT
			DECLARE @TotalTouchWeight FLOAT
			DECLARE @AvgTouchWeight FLOAT
			SET @OpportunityFieldname = (SELECT TOP 1 OpportunityFieldname FROM #TempData WHERE idDenseRank = @intFlag2)
			SET @TotalRevenue = (SELECT TOP 1 Revenue FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname)
			SET @TotalTouchWeight = (SELECT SUM(TouchWeight) FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname)
			IF(@TotalRevenue <> 0)
			BEGIN
				set @AvgTouchWeight = (select (@TotalRevenue / @TotalTouchWeight))
			END
			ELSE
			BEGIN
				set @AvgTouchWeight = 0
			END

			SET @intFlag2 = @intFlag2 + 1

			UPDATE #TempData set RevunueAfterWeight = (@AvgTouchWeight * TouchWeight) WHERE OpportunityFieldname = @OpportunityFieldname
		END

		SELECT OpportunityFieldname, TouchDateFieldName, CampaignId,	DimensionValue	,RevunueAfterWeight AS Revunue FROM #TempData
	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE()
	END CATCH
END
Go
--32
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'KeyDataGet') AND xtype IN (N'P'))
    DROP PROCEDURE KeyDataGet
GO


CREATE PROCEDURE [dbo].[KeyDataGet]  
@KeyDataId int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15)=null,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F',
@KEYDATAVALUE FLOAT OUTPUT
AS
BEGIN

DECLARE @Dimensionid INT, @DimensionName NVARCHAR(100), @Count INT, 
		@IsDateDimension BIT,@CreateTable NVARCHAR(MAX),
		@CreateColTable NVARCHAR(MAX), @Columns NVARCHAR(MAX),
		@ColColumns NVARCHAR(MAX), @SelectTable NVARCHAR(MAX),
		@SelectColumns NVARCHAR(MAX), @InnerJoin NVARCHAR(MAX),
		@InnerJoin1 NVARCHAR(MAX), @Columns1 NVARCHAR(MAX),
		@GroupBy NVARCHAR(MAX),@Where NVARCHAR(MAX),
		@DateFilter NVARCHAR(MAX),@ColumnPart NVARCHAR(1000),
		@ExcludeWhere NVARCHAR(MAX),@AxisCount INT, 
		@IsDateDimensionExist BIT,@IsDateDimensionOnly BIT,
		@DATEDIMENSION int,@DimensionValue NVARCHAR(1000)

SET @ExcludeWhere = '';
SET @Columns = '';SET @ColColumns = '';SET @SelectColumns = ''; SET @Columns1 = '';SET @InnerJoin = '';SET @InnerJoin1 = '';
SET @DateFilter = ''

SET @CreateTable = N'DECLARE @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) + '  TABLE ';
SET @CreateColTable = N'DECLARE @COLTABLE' + CAST(@KeyDataId AS NVARCHAR) + '  TABLE ';

SET @SelectTable = N'INSERT INTO @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) + ' SELECT DISTINCT '
SET @Count = 1;
SET @GroupBy = ' GROUP BY ';
SET @Where = ' WHERE '

SET @IsDateDimensionOnly = 0;
SELECT @AxisCount = COUNT(*) FROM KeyDataDimension WHERE KeyDataId = @KeyDataId;
SELECT @IsDateDimensionExist = ISNULL(IsDateDimension,0) FROM Dimension WHERE Id IN (SELECT DimensionId FROM KeyDataDimension WHERE KeyDataId = @KeyDataId)
							   AND IsDateDimension = 1;
If(@AxisCount = 1 AND @IsDateDimensionExist = 1) 
BEGIN
	SET @IsDateDimensionOnly = 1
END

SELECT @DATEDIMENSION = dimension FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D') WHERE cnt = REPLACE(@DATEFIELD,'D','');
DECLARE @ColumnDatePart NVARCHAR(MAX) = '';
	IF(@ViewByValue = 'Q') 
	BEGIN
		SET @ColumnDatePart = '''Q'' + CAST(DATEPART(Q,CAST(#NAME# AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue = 'Y') 
	BEGIN
		SET @ColumnDatePart = 'CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue = 'M') 
	BEGIN
		SET @ColumnDatePart = 'SUBSTRING(DateName(MONTH,CAST(#NAME# AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		IF(YEAR(@STARTDATE) = YEAR(@ENDDATE))
		BEGIN
			SET @ColumnDatePart ='CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)))) + '' '' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR),3)'
		END
		ELSE 
		BEGIN
			SET @ColumnDatePart='CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)))) + '' '' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR),3) + ''-''+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR)))'
		END
	END
	ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
	BEGIN
		SET @ColumnDatePart= '[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(#NAME# AS DATETIME))'
	END
	ELSE 
	BEGIN
		SET @ColumnDatePart = '#NAME#'
	END
DECLARE Column_Cursor CURSOR FOR
select KDD.Dimensionid, '[' + Replace(D.Name,' ','') +']'   DimensionName,D.IsDateDimension,KDD.DimensionValue	from KeyData KD 
INNER JOIN KeyDataDimension KDD ON KD.id = KDD.KeyDataId
INNER JOIN Dimension D ON KDD.Dimensionid = D.id  
WHERE KD.id = @KeyDataId
ORDER BY KD.OrderValue
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @DimensionName,@IsDateDimension,@DimensionValue
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF(ISNULL(@IsDateDimension,0) = 1)
			BEGIN
				SET @ColumnPart = @ColumnDatePart
			END
			ELSE 
			BEGIN
				SET @ColumnPart = '#NAME#'
			END
			
			/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
			-- Restrict dimension values as per UserId and RoleId
			DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
			IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
			BEGIN
				SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
				FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
			END
			ELSE 
			BEGIN
				SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
				FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
			END
			IF(CHARINDEX(',',@RestrictedDimensionValues) = 2)
			BEGIN
				SET @RestrictedDimensionValues = SUBSTRING(@RestrictedDimensionValues,4,LEN(@RestrictedDimensionValues))
			END
			/* End - Added by Arpita Soni for ticket #511 on 10/30/2015 */

			--Blank value insertion
			SET @Columns = @Columns + @DimensionName + ' NVARCHAR(1000), '
			SET @ColColumns = @ColColumns + 'DV' + CAST(@Count AS NVARCHAR) + ' NVARCHAR(1000), '
			SET @SelectTable = @SelectTable + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') + ', ' 
			SET @ExcludeWhere = CASE WHEN ISNULL(@ExcludeWhere,'') != '' THEN @ExcludeWhere + ' AND ' ELSE '' END 

			-- filter keydata as per dimension value in KeyDataDimension
			IF(ISNULL(@IsDateDimension,0) = 0)
			BEGIN
				SET @ExcludeWhere = @ExcludeWhere + CASE WHEN ISNULL(@DimensionValue,'') != '' THEN 
									' d' + CAST(@Count AS NVARCHAR) + '.DisplayValue IN ('''+ISNULL(@DimensionValue,'')+''') AND ' 
									ELSE '' END 
			END
			ELSE
			BEGIN
				SET @ExcludeWhere = @ExcludeWhere + CASE WHEN ISNULL(@DimensionValue,'') != '' THEN 
									REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') +' IN ('''+ISNULL(@DimensionValue,'')+''') AND ' 
									ELSE '' END 
			END
			SET @ExcludeWhere = @ExcludeWhere + ' d' + CAST(@Count AS NVARCHAR) + '.DisplayValue NOT IN ('''+ISNULL(@RestrictedDimensionValues,'')+''')' 
			SET @InnerJoin = @InnerJoin + ' INNER JOIN DimensionValue d' + CAST(@Count AS NVARCHAR) + ' ON d' + CAST(@Count AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
			
			--Measure value update
			SET @Columns1 = @Columns1 + REPLACE(@ColumnPart,'#NAME#',' d' + CAST((@Count + 2) AS NVARCHAR) + '.DisplayValue') + ' DV' +  CAST(@Count AS NVARCHAR)  + ','
			
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			IF(@DATEFIELD = 'D'  + CAST(@tempIndex AS NVARCHAR))
			BEGIN
				DECLARE @tmpDateId INT
				SELECT @tmpDateId = @DATEDIMENSION
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d1.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@tmpDateId AS NVARCHAR)
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d1.d'  + CAST(@tempIndex AS NVARCHAR) 
			END
			SET @GroupBy = @GroupBy + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count + 2 AS NVARCHAR) + '.DisplayValue') + ','

			SET @Where = @Where + ' dv' + CAST(@Count AS NVARCHAR) + ' = ' + @DimensionName + ' AND'
			SET @Count = @Count + 1
			SET @RestrictedDimensionValues= ''
	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @DimensionName,@IsDateDimension,@DimensionValue
	END
Close Column_Cursor
Deallocate Column_Cursor
--create temporary table
SET @CreateTable = @CreateTable + '(' + @Columns
SET @CreateColTable = @CreateColTable + '(' + @ColColumns
--measure
SET @GroupBy = SUBSTRING(@GroupBy,0 ,LEN(@GroupBy))
SET @Where = SUBSTRING(@Where,0 ,LEN(@Where)-3)
--Setting Date Filter
DECLARE @DateDimensionId INT,@DateDimensionCondition NVARCHAR(100)
SET @DateDimensionId = 0; SET @DateDimensionCondition = ''
IF(@DATEFIELD IS NOT NULL)
BEGIN
	SELECT @DateDimensionId =  @DATEDIMENSION 
	SET @DateDimensionCondition = ' AND d'+ CAST(@Count + 3  AS NVARCHAR) +'.DimensionId = ' + CAST(@DateDimensionId  AS NVARCHAR)
END
SET @DateFilter = CASE WHEN @DATEFIELD IS NULL THEN '' ELSE 
				+ ' INNER JOIN DimensionValue d' +  CAST(@Count + 3  AS NVARCHAR) + ' ON d1.' + @DATEFIELD + ' = d' +  CAST(@Count + 3  AS NVARCHAR) + '.id '
				+ 'and CAST(d'+  CAST(@Count + 3  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '   + @DateDimensionCondition
				END;

--setting where condition
DECLARE @FilterCondition NVARCHAR(4000);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D1',2);
END
SET @FilterCondition = ISNULL(@FilterCondition,'')
IF(@FilterCondition != '')
SET @FilterCondition = ' WHERE' + REPLACE(@FilterCondition,'#',' AND ')


DECLARE @TEMPTABLEUPDATE NVARCHAR(MAX),@AggregationType NVARCHAR(20), @TEMPTABLEUPDATE1 NVARCHAR(MAX), @TEMPTABLEUPDATE2 NVARCHAR(MAX),@FilterConditionMeasure NVARCHAR(MAX);
DECLARE @MEASURENAME NVARCHAR(100),@SymbolType NVARCHAR(100);
DECLARE @MEASUREID INT;
DECLARE @MEASURETABLENAME NVARCHAR(100);
SET @FilterConditionMeasure = '';
SET @TEMPTABLEUPDATE = ''
SET @TEMPTABLEUPDATE1 = ''
SET @TEMPTABLEUPDATE2 = ''
DECLARE Measure_Cursor CURSOR FOR
select Measure.[Name], Measure.id, MeasureTableName,AggregationType,ISNULL(Suffix,'') SymbolType from KeyData inner join Measure on Measure.id=KeyData.MeasureId where KeyData.id= @KeyDataId
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
WHILE @@FETCH_STATUS = 0
	Begin
		DECLARE @tmpColumn1 NVARCHAR(MAX)
		SET @tmpColumn1 = ''
		IF(@FilterCondition = '')
			SET @FilterConditionMeasure = ' WHERE d2.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		ELSE
			SET @FilterConditionMeasure = ' AND d2.Measure = ' + CAST(@MEASUREID AS NVARCHAR)

		DECLARE @AggregartedMeasure NVARCHAR(200)
		IF(LOWER(@AggregationType) = 'avg')
		BEGIN
			IF(@SymbolType != '%')
			BEGIN
				SET @AggregartedMeasure = ' ROUND(sum(D2.Value*d2.rows)/sum(d2.rows),2) AS CalculatedValue,sum(d2.rows) RecordCountSum '
			END
			ELSE
			BEGIN
				SET @AggregartedMeasure = ' ROUND((sum(D2.Value*d2.rows)/sum(d2.rows))*100,2) AS CalculatedValue,sum(d2.rows) RecordCountSum '
			END
		END
		ELSE 
		BEGIN
			SET @AggregartedMeasure = ' SUM(d2.Value) AS CalculatedValue,sum(d2.rows) RecordCountSum '
		END
			
		IF (@IsDateDimensionOnly = 1 )
		BEGIN
			SET @tmpColumn1 =  'SELECT ' + @Columns1 + REPLACE(@AggregartedMeasure,'rows','RecordCount') + ' FROM MeasureValue D2 INNER JOIN DimensionValue D3 ON D2.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','')  + ' WHERE D2.Measure =  ' + CONVERT(NVARCHAR,@MEASUREID) + ' ' +  @GroupBy
		END
		ELSE 
		BEGIN
			SET @tmpColumn1 =  'SELECT ' + @Columns1 + @AggregartedMeasure + ' FROM '+ @DimensionTableName +' d1 INNER JOIN ' + @DimensionTableName  +'Value d2 ON d1.id = ' + 'd2.' +  @DimensionTableName  + @InnerJoin1 +  @DateFilter + @FilterCondition + @FilterConditionMeasure + @GroupBy
		END
		SET @CreateTable = @CreateTable + '[' + @MEASURENAME + '] FLOAT ,RecordCountSum FLOAT, '
		SET @CreateColTable = @CreateColTable + '[' + @MEASURENAME + '] FLOAT ,RecordCountSum FLOAT, '
		
		IF(@SelectColumns = '')
		BEGIN
			SET @SelectColumns =  @SelectColumns + '0,0'
		END
		ELSE
		BEGIN
			SET @SelectColumns =  @SelectColumns + ',0,0'
		END
		
		--Manoj Start changes to table variable
		SET @TEMPTABLEUPDATE1 = @TEMPTABLEUPDATE + N'INSERT INTO @COLTABLE'+ CAST(@KeyDataId AS NVARCHAR) + ' ' +  @tmpColumn1 
		SET @TEMPTABLEUPDATE1 = @TEMPTABLEUPDATE1  + @TEMPTABLEUPDATE + N' UPDATE @KeyDataTable' + cast(@KeyDataId AS NVARCHAR) + ' SET ['+ @MEASURENAME +'] = A.['+ @MEASURENAME + '], RecordCountSum=A.RecordCountSum FROM ( SELECT * FROM @COLTABLE'+ cast(@KeyDataId AS NVARCHAR) +') A ' + @Where
		SET @TEMPTABLEUPDATE2 =  @TEMPTABLEUPDATE2 + @TEMPTABLEUPDATE1
		--Manoj End changes to table variable

	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME, @AggregationType, @SymbolType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor
SET @TEMPTABLEUPDATE  = @TEMPTABLEUPDATE2

SET @SelectTable = @SelectTable + @SelectColumns + '  FROM KeyData ' + @InnerJoin + ' WHERE KeyData.id = ' + CAST(@KeyDataId AS NVARCHAR) +' AND ' +@ExcludeWhere

SET @CreateTable = SUBSTRING(@CreateTable, 0 , LEN(@CreateTable))
SET @CreateTable = @CreateTable + ');'

SET @CreateColTable = SUBSTRING(@CreateColTable, 0 , LEN(@CreateColTable))
SET @CreateColTable = @CreateColTable + ');'

DECLARE @CLOSINGSQL NVARCHAR(1000);
DECLARE @TOTALSQL NVARCHAR(MAX);
 
SET @CLOSINGSQL= N'DECLARE @SUM FLOAT,@WEIGHTEDSUM FLOAT,@TOTALRECORDS FLOAT; ' +
				' SELECT @SUM = SUM(['+@MEASURENAME+']),@TOTALRECORDS = SUM(RecordCountSum),@WEIGHTEDSUM = SUM(['+@MEASURENAME+'] * RecordCountSum) ' +
				' FROM @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) 

IF(@AggregationType='avg' OR (SELECT COUNT(*) FROM KeyDataDimension WHERE KeyDataId = @KeyDataId AND DimensionValue IS NOT NULL) > 0)
	SET @CLOSINGSQL = @CLOSINGSQL + ';IF(@TOTALRECORDS = 0) SET @TOTALRECORDS = 1; set @FINALVALUE = @WEIGHTEDSUM / @TOTALRECORDS;'
ELSE
	SET @CLOSINGSQL = @CLOSINGSQL + '; SEt @FINALVALUE = @SUM; '

SET @TOTALSQL = @CreateTable + ' ' + @SelectTable + ' ' + ' ' +  @CreateColTable + ' ' + @TEMPTABLEUPDATE + ' ' + @CLOSINGSQL
--PRINT (@TOTALSQL)

EXECUTE SP_EXECUTESQL @TOTALSQL,N'@FINALVALUE FLOAT OUTPUT',@FINALVALUE = @KEYDATAVALUE OUTPUT
--RETURN @KEYDATAVALUE
END

Go
--33
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'LinearRegression') AND xtype IN (N'P'))
    DROP PROCEDURE LinearRegression
GO

CREATE procedure [LinearRegression] (@QueryToRun varchar(max))
as
BEGIN
	--Query passed in as value must return two fields (both floats) and no nulls for every row.  Procedure will return null if regression model is degenerate.
	DECLARE @values table(val1 float, val2 float)
	insert into @values
	exec(@QueryToRun)

	BEGIN try
		DECLARE @muxy float = (select avg(val1*val2) from @values)
		DECLARE @mux float = (select avg(val1) from @values)
		DECLARE @muy float = (select avg(val2) from @values)
		DECLARE @mux2 float = (select avg(val1*val1) from @values)
		DECLARE @Beta float = (@muxy - @mux*@muy)/(@mux2-@mux*@mux)
		DECLARE @Alpha float = @muy - @Beta*@mux
		DECLARE @SigmaError float = (select sqrt((STDEV(val2 - (@Beta*val1+ @Alpha))*STDEV(val2 - (@Beta*val1+ @Alpha)))/sum((val1-@mux)*(val1-@mux))) from @values)
		DECLARE @R2 float = (select (@Alpha * sum(val2) + @Beta * sum(val1*val2) - power(sum(val2),2) / count(*)) / (sum(val2*val2) - power(sum(val2),2) / count(*)) from @values)




		select @Alpha as Alpha, @Beta as Beta, @SigmaError as StandardError, @Beta-1.96025*@SigmaError as Lower95, @Beta+1.96025*@SigmaError as Upper95, @R2 as RSquared
	END TRY
	BEGIN CATCH
		select null
	END CATCH
END


GO


--removed 34 measure value data sp


--35
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'NightlyAdobeAnalyics') AND xtype IN (N'P'))
    DROP PROCEDURE NightlyAdobeAnalyics
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [NightlyAdobeAnalyics]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_WebPageAnalytics_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_WebPageAnalytics_Value0]

	DELETE FROM _WebPageAnalytics WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer3]  WHERE datestamp >= '2015-10-28';


	/** Populate all of the values found in the rel tables **/
	UPDATE w
	SET w.PlatformType = rel.PlatformType
	FROM _WebPageAnalytics w
	INNER JOIN [rel_PlatformType] rel ON (w.ElementValue = rel.OperatingSystem)
	;

	UPDATE w
	SET w.DeviceType = rel.DeviceType
	FROM _WebPageAnalytics w
	INNER JOIN [rel_DeviceType] rel ON (w.ElementValue = rel.OperatingSystem)
	;


	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_WebsitePageAnalytics_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_WebsitePageAnalytics_Value0];

	DELETE FROM _WebsitePageAnalytics WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage1 WHERE datestamp >= '2015-10-28'
	;

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage2 WHERE datestamp >= '2015-10-28'
	;

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage3 WHERE datestamp >= '2015-10-28'
	;

END

GO
--36
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'NightlyRefreshDatabase') AND xtype IN (N'P'))
    DROP PROCEDURE NightlyRefreshDatabase
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [NightlyRefreshDatabase] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value0]

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value1]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value1]

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value2]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value2]

	DELETE FROM _Database;

	INSERT INTO [_Database] ([Email],[SalesRegion],[FirmType],[AUMLevel],[Marketable],[DateCreated])
	SELECT ec.[EmailAddress]
		,ec.[SalesRegion]
		,ec.[FirmType]
		, rel.Id
		, 'Marketable'
		,CAST(ec.[DateCreated] AS date)
	FROM raw_Eloqua_Contacts ec
	INNER JOIN rel_AUMs rel ON (ec.TDAEstimatedAUM >= CAST(rel.[MIN] as float) AND ec.TDAEstimatedAUM <= CAST(rel.[MAX] as float))
	;

	UPDATE _Database SET SalesRegion = '(blank)' WHERE SalesRegion = '';
	UPDATE _Database SET FirmType = '(blank)' WHERE FirmType = '';

	DELETE FROM _Activity
	WHERE ActivityDate >= '2015-10-28'
	;

	INSERT INTO _Activity ([Email],[ActivityType],[ActivityDate]) 
	SELECT Email, 'Created', CAST(DateCreated as date) FROM _Database
	WHERE DateCreated >= '2015-10-28';

	INSERT INTO _Activity ([Email],[ActivityType],[ActivityDate]) 
	SELECT EmailAddress, ActivityType, CAST(ActivityDate as date) FROM raw_eloqua_Activities
	WHERE ActivityType IN ('Bounceback', 'Unsubscribe')
	AND ActivityDate  >= '2015-10-28';

	UPDATE [_Database] SET Marketable = 'Unmarketable'
	WHERE Email IN (SELECT Email FROM _Activity WHERE ActivityType IN ('Bounceback', 'Unsubscribe'))
	;

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_EmailActivityNew_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_EmailActivityNew_Value0]

	DELETE FROM _EmailActivityNew WHERE SendDate >= '2015-10-28';
	--DBCC CHECKIDENT (_EmailActivityNew, reseed, SELECT MAX(ID) FROM _EmailActivityNew);

	INSERT INTO _EmailActivityNew ([SendDate],[ContactID],[EmailAddress],[AssetName], isOpened, isClickThrough, IsBounceback)
	SELECT CAST([ActivityDate] as date),[ContactID],[EmailAddress],[AssetName], 0, 0, 0
	FROM raw_eloqua_Activities
	WHERE ActivityType IN ('EmailSend')
	AND ActivityDate >= '2015-10-28'
	;

	UPDATE e1
	SET e1.isOpened = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'EmailOpen'
	;

	UPDATE e1
	SET e1.isClickThrough = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'EmailClickthrough'
	;

	UPDATE e1
	SET e1.IsBounceback = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'Bounceback'
	;


END

GO
--37
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'PopulateDynamicColumns') AND xtype IN (N'P'))
    DROP PROCEDURE PopulateDynamicColumns
GO


CREATE PROCEDURE [PopulateDynamicColumns]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY




DECLARE @TABLENAME varchar(max);
DECLARE @DIMENSIONS int;
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @DIMENSIONVALUETABLENAME varchar(max);
DECLARE @LASTDIMENSIONVALUETABLENAME varchar(max);

SET @LASTDIMENSIONVALUETABLENAME=null;


DECLARE Dimension_Cursor CURSOR FOR
select TableName, Dimensions, DimensionTableName, DimensionValueTableName + '_New' from DynamicDimension_New
where containsDateDimension=1
 order by DimensionValueTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
while @@FETCH_STATUS = 0
	Begin

	--If this is the first row with the DimensionValueTableName value, we need to populate that table
	if @LASTDIMENSIONVALUETABLENAME is null or @LASTDIMENSIONVALUETABLENAME<>@DIMENSIONVALUETABLENAME
		BEGIN
		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME

		DECLARE @POPSQL varchar(max);
		SET @POPSQL = 'Insert into ' + @DIMENSIONVALUETABLENAME + ' ( ' + @DIMENSIONTABLENAME + ' ) select id from ' + @DIMENSIONTABLENAME + ';';
		
		SET @CURQUERYFORERROR = @POPSQL

		exec(@POPSQL)
		

		END

	DECLARE @POPULATEQUERY varchar(max); -- To populate the target table (e.g. CampaignValue0)
	DECLARE @POPULATELASTJOIN varchar(max);
	DECLARE @dCount int;
	DECLARE @dIndex int;
	DECLARE @dNextIndex int;

	set @POPULATEQUERY = N'Update ' + @DIMENSIONVALUETABLENAME + ' set ' + @TABLENAME + '=' + @TABLENAME + 'Val from (select c1.ID as CID, d1.id as ' + @TABLENAME + 'Val from ' + 
		@DIMENSIONTABLENAME + '_Base_New c1'
	set @POPULATELASTJOIN = N' inner join ' + @TABLENAME + '_New d1 on ';


	set @dCount=1;
	set @dIndex = CHARINDEX('d',@TABLENAME);
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

	while @dCount <= @DIMENSIONS
		BEGIN
			--select @TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1, @dIndex, @dNextIndex
			set @POPULATELASTJOIN = @POPULATELASTJOIN + ' c1.DimensionValue' + SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) + '=d1.d' + cast(@dCount as varchar) + ' '


			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@TABLENAME)+1;
				END

			if @dCount <= @DIMENSIONS
				BEGIN
				set @POPULATELASTJOIN = @POPULATELASTJOIN + ' and ';

				END
		END
	set @POPULATEQUERY = @POPULATEQUERY + @POPULATELASTJOIN + ') a where a.CID=' + @DIMENSIONTABLENAME;

	--select @POPULATEQUERY

	
	SET @CURQUERYFORERROR=@POPULATEQUERY
	if @DEBUG=1
	BEGIN
		print @POPULATEQUERY
	END

	exec(@POPULATEQUERY)

	--print @POPULATEQUERY
	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor


delete from MeasureValue_New

--Let's populate the MeasureValue table now
DECLARE @MEASUREID int;
DECLARE @AGGREGATIONQUERY varchar(max);
DECLARE @MEASURETABLENAME varchar(max);
DECLARE @DIMENSIONID varchar(max);
DECLARE @COMPUTEALLVALUES bit;
DECLARE @COMPUTEALLVALUESFORMULA varchar(max);
--reuse DIMENSIONID



DECLARE MEASURE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW') , MeasureTableName, Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW') from Measure
inner join Dimension on MeasureTableName=TableName
where Dimension.IsDeleted=0 and Measure.IsDeleted=0
OPEN MEASURE_CURSOR
FETCH NEXT FROM MEASURE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA

while @@FETCH_STATUS = 0
	BEGIN

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END
	ELSE  --We need to deal with the computeallvalues stuff here as well
	BEGIN
		

		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END


		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#', 'DimensionValue_New')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#', 'id')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#', 'd1.DimensionID=' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END

	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASURE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA
	END;
CLOSE MEASURE_CURSOR
DEALLOCATE MEASURE_CURSOR



--Now we need to populate the extended dimension value tables
--We need to deal with the computeallvalues stuff here as well

DECLARE @LastDimensionTableName varchar(500)=null;
DECLARE @UseRowCountFromFormula bit=0;

DECLARE MEASUREVALUE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW'), D1.TableName + '_New', Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW'), D1.Dimensions, D1.DimensionValueTableName + '_New', UseRowCountFromFormula from Measure
inner join DynamicDimension_New d1 on MeasureTableName=DimensionTableName
left outer join Dimension  on Dimension.TableName=DimensionTableName and Dimension.ComputeAllValues=1 and Dimension.IsDeleted=0
where containsdatedimension=1 and not exists (select d2.id from DynamicDimension_New d2 where d1.DimensionTableName=d2.DimensionTableName and d2.Dimensions > d1.Dimensions)
and Measure.IsDeleted=0
order by D1.Dimensions, D1.TableName
OPEN MEASUREVALUE_CURSOR
FETCH NEXT FROM MEASUREVALUE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
while @@FETCH_STATUS = 0
	BEGIN


	DECLARE @shortTableName nvarchar(max)= (left(@MEASURETABLENAME, len(@MEASURETABLENAME)-4)) 

	if @LastDimensionTableName is null or @LastDimensionTableName<> @MEASURETABLENAME
	BEGIN
		if @LastDimensionTableName is not null
		BEGIN
			SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

			exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
		END


		if @DEBUG=1
		BEGIN
			print 'CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'
		END

		SET @CURQUERYFORERROR='CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'

		exec('CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)')

		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME
		SET @LastDimensionTableName=@MEASURETABLENAME
	END

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  @shortTableName)
		--SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  '')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END
	ELSE
	BEGIN

		--select d1.id, count(*) as Inventory, count(*) as RecordCount from #DIMENSIONTABLE# d1 
		--inner join DEXAllLeadHistory d3 on d3.#DIMENSIONFIELD#<=d1.#DIMENSIONTABLEFIELD# 
		--where  #DIMENSIONWHERE# d1.d1=d3.DimensionValue12 and not exists ( 
		--select d2.LeadID from DEXAllLeadHistory d2 
		--where d2.OldValue=d3.NewValue and d2.OldValue<>d2.NewValue and cast(d2.CreatedDate as datetime) > cast(d3.CreatedDate as datetime) and d2.LeadID=d3.LeadID) group by d1.id


		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END
		
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  'DimensionValue' + @DIMENSIONID)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#',  @MEASURETABLENAME)

		--Need to loop through the dimensions in the table with the exception of the computeallvalues dimension
		DECLARE @DIMENSIONTABLEFIELD varchar(max);
		DECLARE @DIMENSIONWHERE varchar(max);

		DECLARE @OLDMEASURETABLENAME varchar(max) = left(@MEASURETABLENAME,LEN(@MEASURETABLENAME)-4) --Strip the _New off the name

		set @dCount=1;
		set @dIndex = CHARINDEX('d',@OLDMEASURETABLENAME);
		set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);
		set @DIMENSIONWHERE=''
		set @DIMENSIONTABLEFIELD='d1'


		while @dCount <= @DIMENSIONS
		BEGIN
			DECLARE @CURDIMENSION varchar(10);

			set @CURDIMENSION=SUBSTRING(@OLDMEASURETABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1);

			if @CURDIMENSION<>@DIMENSIONID
			BEGIN

				if len(@DIMENSIONWHERE) > 0
				BEGIN
					set @DIMENSIONWHERE= @DIMENSIONWHERE + ' and '
				END

				set @DIMENSIONWHERE = @DIMENSIONWHERE + ' d1.d' + cast(@dCount as varchar) + '=d2.DimensionValue' + cast(@CURDIMENSION as varchar) + ' '; 
			END
			ELSE
			BEGIN
				set @DIMENSIONTABLEFIELD = 'd' + cast(@dCount as varchar) + ' '
			END

			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@OLDMEASURETABLENAME)+1;
				END
		END

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#',  @DIMENSIONTABLEFIELD)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#',  @DIMENSIONWHERE)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  ', 1 as RecordCount')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
			--update d38d45d53d65d66Value
			--set rows=rcount from (
			--select d38d45d53d65d66 as d, count(*) as rcount from DummyContacts_Value0
			--group by d38d45d53d65d66) A
			--where d38d45d53d65d66=d

		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END
	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASUREVALUE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
	END;
CLOSE MEASUREVALUE_CURSOR
DEALLOCATE MEASUREVALUE_CURSOR

--Clean up from Aggreation process 
--Fix to issue #310
if @LastDimensionTableName is not null
BEGIN
	SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

	exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
END


--For performance Improvement

DECLARE @MasterTable varchar(max);

--SET @MasterTable=(select top 1 tablename from DynamicDimension where DimensionTableName='DummyContacts'
--order by Dimensions desc)


DECLARE @dtable table (id int, did int)

--insert into @dtable select id from Dimension where IsDeleted=0 and TableName='DummyContacts'

--select * from @dtable

DECLARE @ctable table (id int, did int)
SET @LASTDIMENSIONTABLENAME ='';

DECLARE @InsertQuery varchar(max);
DECLARE @iCount int;


DECLARE Dimension_Cursor CURSOR FOR
select TableName+'_New', Dimensions, DimensionTableName from DynamicDimension_New 
where ContainsDateDimension=1
order by DimensionTableName, Dimensions desc
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
while @@FETCH_STATUS = 0
	BEGIN
	delete from @ctable

	if @LASTDIMENSIONTABLENAME<>@DIMENSIONTABLENAME
	BEGIN
		SET @MasterTable = @TABLENAME
		insert into @dtable select 0, id from Dimension where IsDeleted=0 and TableName=@DIMENSIONTABLENAME order by id
		SET @LASTDIMENSIONTABLENAME = @DIMENSIONTABLENAME

		SET @iCount=1

		while (select count(*) from @dtable where id=0) > 0
		BEGIN
			update @dtable set id=@iCount where did=(select min(did) from @dtable where id=0)
			set @iCount = @iCount + 1
		END

	END


	if @TABLENAME<>@MasterTable
	BEGIN

		set @dCount=1;
		set @dIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4));
		set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);
		while @dCount <= @DIMENSIONS
			BEGIN

				--if @DEBUG=1
				--BEGIN
				--	print SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1)
				--END

				insert into @ctable select @dCount, cast(SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1) as int)

				set @dCount = @dCount + 1;
				set @dIndex = @dNextIndex;
				set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);

				if @dNextIndex = 0
					BEGIN
					set @dNextIndex = LEN(left(@TABLENAME, len(@TABLENAME)-4))+1;
					END

			END

		--select d3.id, m1.id,
		--case 
			--when m1.AggregationType='AVG' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			--else sum(Value)
		--END as Generated,
		--sum(d1.rows) as recordCount
		 --from d38d45d53d65d66Value d1
		--inner join d38d45d53d65d66 d2 on d1.d38d45d53d65d66=d2.id
		--inner join d38d45d53d65 d3 on d2.d1=d3.d1 and d2.d2=d3.d2 and d2.d3=d3.d3 and d2.d4=d3.d4
		--inner join Measure m1 on d1.Measure=m1.id
		--group by d3.id, m1.id, m1.AggregationType


		set @InsertQuery='truncate table ' + @TABLENAME + 'VALUE insert into ' + @TABLENAME + 'VALUE select d3.id, m1.id, case 
			when m1.AggregationType=''AVG'' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			else sum(Value)
		END as Generated, sum(rows) as recordCount from ' + @MasterTable + 'Value d1 '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @MasterTable + ' d2 on d1.' + left(@MasterTable, len(@MasterTable)-4) + '=d2.id '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @TABLENAME + ' d3 on '

		while (select count(*) from @ctable) > 0
		BEGIN

			SET @InsertQuery = @InsertQuery +  (select 'd3.d' + cast(c1.id as varchar) + '=d2.d' + cast(d1.id as varchar) from @ctable c1
				inner join @dtable d1 on c1.did=d1.did
				where c1.id=(select min(id) from @ctable))
			delete from @ctable where id=(select min(id) from @ctable)
			if (select count(*) from @ctable) > 0
			BEGIN
				SET @InsertQuery = @InsertQuery + ' AND '
			END
		END

		SET @InsertQuery = @InsertQuery + ' inner join Measure m1 on d1.Measure=m1.id Group by d3.id, m1.id, m1.AggregationType'

		if @DEBUG=1
		BEGIN
			print @InsertQuery
		END

		SET @CURQUERYFORERROR=@InsertQuery

		exec( @InsertQuery)
	END

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
	END

close Dimension_Cursor
Deallocate Dimension_Cursor

--END INSERT of performance code





END TRY

BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicColumns',@CURQUERYFORERROR);
	THROW;
END CATCH

END






GO
--38
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[PopulateDynamicColumnsPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[PopulateDynamicColumnsPartial]  
END
GO

CREATE PROCEDURE [dbo].[PopulateDynamicColumnsPartial]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY




DECLARE @TABLENAME varchar(max);
DECLARE @DIMENSIONS int;
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @DIMENSIONVALUETABLENAME varchar(max);
DECLARE @LASTDIMENSIONVALUETABLENAME varchar(max);

SET @LASTDIMENSIONVALUETABLENAME=null;


DECLARE Dimension_Cursor CURSOR FOR
select TableName, Dimensions, DimensionTableName, DimensionValueTableName + '_New' from DynamicDimension_New
where containsDateDimension=1
 order by DimensionValueTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
while @@FETCH_STATUS = 0
	Begin

	--If this is the first row with the DimensionValueTableName value, we need to populate that table
	if @LASTDIMENSIONVALUETABLENAME is null or @LASTDIMENSIONVALUETABLENAME<>@DIMENSIONVALUETABLENAME
		BEGIN
		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME

		DECLARE @POPSQL varchar(max);
		SET @POPSQL = 'Insert into ' + @DIMENSIONVALUETABLENAME + ' ( ' + @DIMENSIONTABLENAME + ' ) select id from ' + @DIMENSIONTABLENAME + ';';
		
		SET @CURQUERYFORERROR = @POPSQL

		exec(@POPSQL)
		

		END

	DECLARE @POPULATEQUERY varchar(max); -- To populate the target table (e.g. CampaignValue0)
	DECLARE @POPULATELASTJOIN varchar(max);
	DECLARE @dCount int;
	DECLARE @dIndex int;
	DECLARE @dNextIndex int;

	set @POPULATEQUERY = N'Update ' + @DIMENSIONVALUETABLENAME + ' set ' + @TABLENAME + '=' + @TABLENAME + 'Val from (select c1.ID as CID, d1.id as ' + @TABLENAME + 'Val from ' + 
		@DIMENSIONTABLENAME + '_Base_New c1'
	set @POPULATELASTJOIN = N' inner join ' + @TABLENAME + '_New d1 on ';


	set @dCount=1;
	set @dIndex = CHARINDEX('d',@TABLENAME);
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

	while @dCount <= @DIMENSIONS
		BEGIN
			--select @TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1, @dIndex, @dNextIndex
			set @POPULATELASTJOIN = @POPULATELASTJOIN + ' c1.DimensionValue' + SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) + '=d1.d' + cast(@dCount as varchar) + ' '


			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@TABLENAME)+1;
				END

			if @dCount <= @DIMENSIONS
				BEGIN
				set @POPULATELASTJOIN = @POPULATELASTJOIN + ' and ';

				END
		END
	set @POPULATEQUERY = @POPULATEQUERY + @POPULATELASTJOIN + ') a where a.CID=' + @DIMENSIONTABLENAME;

	--select @POPULATEQUERY

	
	SET @CURQUERYFORERROR=@POPULATEQUERY
	if @DEBUG=1
	BEGIN
		print @POPULATEQUERY
	END

	exec(@POPULATEQUERY)

	--print @POPULATEQUERY
	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor


delete from MeasureValue_New

--Let's populate the MeasureValue table now
DECLARE @MEASUREID int;
DECLARE @AGGREGATIONQUERY varchar(max);
DECLARE @MEASURETABLENAME varchar(max);
DECLARE @DIMENSIONID varchar(max);
DECLARE @COMPUTEALLVALUES bit;
DECLARE @COMPUTEALLVALUESFORMULA varchar(max);
--reuse DIMENSIONID



DECLARE MEASURE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW') , MeasureTableName, Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW') from Measure
inner join Dimension on MeasureTableName=TableName
where Dimension.IsDeleted=0 and Measure.IsDeleted=0
and MeasureTableName in (select dimensiontablename from DynamicDimension_New)
OPEN MEASURE_CURSOR
FETCH NEXT FROM MEASURE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA

while @@FETCH_STATUS = 0
	BEGIN

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END
	ELSE  --We need to deal with the computeallvalues stuff here as well
	BEGIN
		

		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END


		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#', 'DimensionValue_New')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#', 'id')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#', 'd1.DimensionID=' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END

	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASURE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA
	END;
CLOSE MEASURE_CURSOR
DEALLOCATE MEASURE_CURSOR



--Now we need to populate the extended dimension value tables
--We need to deal with the computeallvalues stuff here as well

DECLARE @LastDimensionTableName varchar(500)=null;
DECLARE @UseRowCountFromFormula bit=0;

DECLARE MEASUREVALUE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW'), D1.TableName + '_New', Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW'), D1.Dimensions, D1.DimensionValueTableName + '_New', UseRowCountFromFormula from Measure
inner join DynamicDimension_New d1 on MeasureTableName=DimensionTableName
left outer join Dimension  on Dimension.TableName=DimensionTableName and Dimension.ComputeAllValues=1 and Dimension.IsDeleted=0
where containsdatedimension=1 and not exists (select d2.id from DynamicDimension_New d2 where d1.DimensionTableName=d2.DimensionTableName and d2.Dimensions > d1.Dimensions)
and Measure.IsDeleted=0
order by D1.Dimensions, D1.TableName
OPEN MEASUREVALUE_CURSOR
FETCH NEXT FROM MEASUREVALUE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
while @@FETCH_STATUS = 0
	BEGIN


	DECLARE @shortTableName nvarchar(max)= (left(@MEASURETABLENAME, len(@MEASURETABLENAME)-4)) 

	if @LastDimensionTableName is null or @LastDimensionTableName<> @MEASURETABLENAME
	BEGIN
		if @LastDimensionTableName is not null
		BEGIN
			SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

			exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
		END


		if @DEBUG=1
		BEGIN
			print 'CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'
		END

		SET @CURQUERYFORERROR='CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'

		exec('CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)')

		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME
		SET @LastDimensionTableName=@MEASURETABLENAME
	END

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  @shortTableName)
		--SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  '')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A where A.' + @shortTableName + ' is not null ';
		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END
	ELSE
	BEGIN

		--select d1.id, count(*) as Inventory, count(*) as RecordCount from #DIMENSIONTABLE# d1 
		--inner join DEXAllLeadHistory d3 on d3.#DIMENSIONFIELD#<=d1.#DIMENSIONTABLEFIELD# 
		--where  #DIMENSIONWHERE# d1.d1=d3.DimensionValue12 and not exists ( 
		--select d2.LeadID from DEXAllLeadHistory d2 
		--where d2.OldValue=d3.NewValue and d2.OldValue<>d2.NewValue and cast(d2.CreatedDate as datetime) > cast(d3.CreatedDate as datetime) and d2.LeadID=d3.LeadID) group by d1.id


		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END
		
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  'DimensionValue' + @DIMENSIONID)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#',  @MEASURETABLENAME)

		--Need to loop through the dimensions in the table with the exception of the computeallvalues dimension
		DECLARE @DIMENSIONTABLEFIELD varchar(max);
		DECLARE @DIMENSIONWHERE varchar(max);

		DECLARE @OLDMEASURETABLENAME varchar(max) = left(@MEASURETABLENAME,LEN(@MEASURETABLENAME)-4) --Strip the _New off the name

		set @dCount=1;
		set @dIndex = CHARINDEX('d',@OLDMEASURETABLENAME);
		set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);
		set @DIMENSIONWHERE=''
		set @DIMENSIONTABLEFIELD='d1'


		while @dCount <= @DIMENSIONS
		BEGIN
			DECLARE @CURDIMENSION varchar(10);

			set @CURDIMENSION=SUBSTRING(@OLDMEASURETABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1);

			if @CURDIMENSION<>@DIMENSIONID
			BEGIN

				if len(@DIMENSIONWHERE) > 0
				BEGIN
					set @DIMENSIONWHERE= @DIMENSIONWHERE + ' and '
				END

				set @DIMENSIONWHERE = @DIMENSIONWHERE + ' d1.d' + cast(@dCount as varchar) + '=d2.DimensionValue' + cast(@CURDIMENSION as varchar) + ' '; 
			END
			ELSE
			BEGIN
				set @DIMENSIONTABLEFIELD = 'd' + cast(@dCount as varchar) + ' '
			END

			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@OLDMEASURETABLENAME)+1;
				END
		END

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#',  @DIMENSIONTABLEFIELD)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#',  @DIMENSIONWHERE)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  ', 1 as RecordCount')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A where A.id is not null ';
			--update d38d45d53d65d66Value
			--set rows=rcount from (
			--select d38d45d53d65d66 as d, count(*) as rcount from DummyContacts_Value0
			--group by d38d45d53d65d66) A
			--where d38d45d53d65d66=d

		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END
	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASUREVALUE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
	END;
CLOSE MEASUREVALUE_CURSOR
DEALLOCATE MEASUREVALUE_CURSOR

--Clean up from Aggreation process 
--Fix to issue #310
if @LastDimensionTableName is not null
BEGIN
	SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

	exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
END


--For performance Improvement

DECLARE @MasterTable varchar(max);

--SET @MasterTable=(select top 1 tablename from DynamicDimension where DimensionTableName='DummyContacts'
--order by Dimensions desc)


DECLARE @dtable table (id int, did int)

--insert into @dtable select id from Dimension where IsDeleted=0 and TableName='DummyContacts'

--select * from @dtable

DECLARE @ctable table (id int, did int)
SET @LASTDIMENSIONTABLENAME ='';

DECLARE @InsertQuery varchar(max);
DECLARE @iCount int;


DECLARE Dimension_Cursor CURSOR FOR
select TableName+'_New', Dimensions, DimensionTableName from DynamicDimension_New 
where ContainsDateDimension=1
order by DimensionTableName, Dimensions desc
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
while @@FETCH_STATUS = 0
	BEGIN
	delete from @ctable

	if @LASTDIMENSIONTABLENAME<>@DIMENSIONTABLENAME
	BEGIN
		SET @MasterTable = @TABLENAME
		insert into @dtable select 0, id from Dimension where IsDeleted=0 and TableName=@DIMENSIONTABLENAME order by id
		SET @LASTDIMENSIONTABLENAME = @DIMENSIONTABLENAME

		SET @iCount=1

		while (select count(*) from @dtable where id=0) > 0
		BEGIN
			update @dtable set id=@iCount where did=(select min(did) from @dtable where id=0)
			set @iCount = @iCount + 1
		END

	END


	if @TABLENAME<>@MasterTable
	BEGIN

		set @dCount=1;
		set @dIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4));
		set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);
		while @dCount <= @DIMENSIONS
			BEGIN

				--if @DEBUG=1
				--BEGIN
				--	print SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1)
				--END

				insert into @ctable select @dCount, cast(SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1) as int)

				set @dCount = @dCount + 1;
				set @dIndex = @dNextIndex;
				set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);

				if @dNextIndex = 0
					BEGIN
					set @dNextIndex = LEN(left(@TABLENAME, len(@TABLENAME)-4))+1;
					END

			END

		--select d3.id, m1.id,
		--case 
			--when m1.AggregationType='AVG' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			--else sum(Value)
		--END as Generated,
		--sum(d1.rows) as recordCount
		 --from d38d45d53d65d66Value d1
		--inner join d38d45d53d65d66 d2 on d1.d38d45d53d65d66=d2.id
		--inner join d38d45d53d65 d3 on d2.d1=d3.d1 and d2.d2=d3.d2 and d2.d3=d3.d3 and d2.d4=d3.d4
		--inner join Measure m1 on d1.Measure=m1.id
		--group by d3.id, m1.id, m1.AggregationType


		set @InsertQuery='truncate table ' + @TABLENAME + 'VALUE insert into ' + @TABLENAME + 'VALUE select d3.id, m1.id, case 
			when m1.AggregationType=''AVG'' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			else sum(Value)
		END as Generated, sum(rows) as recordCount from ' + @MasterTable + 'Value d1 '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @MasterTable + ' d2 on d1.' + left(@MasterTable, len(@MasterTable)-4) + '=d2.id '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @TABLENAME + ' d3 on '

		while (select count(*) from @ctable) > 0
		BEGIN

			SET @InsertQuery = @InsertQuery +  (select 'd3.d' + cast(c1.id as varchar) + '=d2.d' + cast(d1.id as varchar) from @ctable c1
				inner join @dtable d1 on c1.did=d1.did
				where c1.id=(select min(id) from @ctable))
			delete from @ctable where id=(select min(id) from @ctable)
			if (select count(*) from @ctable) > 0
			BEGIN
				SET @InsertQuery = @InsertQuery + ' AND '
			END
		END

		SET @InsertQuery = @InsertQuery + ' inner join Measure m1 on d1.Measure=m1.id Group by d3.id, m1.id, m1.AggregationType'

		if @DEBUG=1
		BEGIN
			print @InsertQuery
		END

		SET @CURQUERYFORERROR=@InsertQuery

		exec( @InsertQuery)
	END

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
	END

close Dimension_Cursor
Deallocate Dimension_Cursor

--END INSERT of performance code





END TRY

BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicColumns',@CURQUERYFORERROR);
	THROW;
END CATCH

END

GO
--39
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'PopulateDynamicDimension') AND xtype IN (N'P'))
    DROP PROCEDURE PopulateDynamicDimension
GO


CREATE PROCEDURE [PopulateDynamicDimension] @DEBUG bit = 0
AS
BEGIN

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

	--truncate table DynamicDimension

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension_New]') AND type in (N'U'))
		CREATE TABLE [dbo].[DynamicDimension_New](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[TableName] [nvarchar](500) NOT NULL,
		[Dimensions] [int] NULL,
		[DimensionTableName] [nvarchar](50) NULL,
		[ComputeAllValues] [bit] NULL,
		[DimensionValueTableName] [nvarchar](100) NULL,
		[ContainsDateDimension] [bit] NULL
	) ON [PRIMARY]

	truncate table DynamicDimension_New
	truncate table AggregationQueries
	DBCC CHECKIDENT ('dbo.AggregationQueries', RESEED, 1);
	



	DECLARE @DCount int;
	set @DCOUNT=(select count(*) from dimension);

	DECLARE @c int;
	set @c=2

	while @c<= @DCount
		Begin
		
			DECLARE @CurQuery varchar(MAX);
		
			DECLARE @Part1 varchar(MAX);
			DECLARE @Part2 varchar(MAX);
			DECLARE @Part3 varchar(MAX);
			DECLARE @Part4 varchar(MAX);
			DECLARE @Part5 varchar(MAX);

			DECLARE @Column_Drop varchar(MAX);
			DECLARE @Column_Add varchar(MAX);

			set @Part1=''
			set @Part2=''
			set @Part3=''
			set @Part4=''
			set @Part5=''

			set @Column_Drop=''
			set @Column_Add=''

			DECLARE @sCount int;
			set @sCount=1
			while @sCount <= @c
				Begin
					set @Part1 = @Part1 + ' ''d'' + cast(d' + cast(@sCount as varchar) + '.id as varchar)';
					set @Part2 = @Part2 + ' dimension d' + cast(@sCount as varchar) + ' ';
					set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.IsDeleted=0 '
					if @sCount < @c
						BEGIN
						set @Part1 = @Part1 + ' + '
						set @Part2 = @Part2 + ', '
						set @Part3 = @Part3 + ' and d' + cast(@sCount as varchar) + '.id<d' + cast((@sCount+1) as varchar) + '.id and '
						set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.TableName=d' + cast((@sCount+1) as varchar) + '.TableName '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) + '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) + '
						set @Part3 = @Part3 + ' and ';
						END
					ELSE
						BEGIN
						set @Part1 = @Part1 + ', d' + cast(@sCount as varchar) + '.TableName ';
						set @Part2 = @Part2 + ' '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) '
						END

					set @sCount = @sCount + 1
				end;




			set @CurQuery = N'insert into DynamicDimension_New (TableName, DimensionTableName, Dimensions, ComputeAllValues, containsDateDimension) select ' + @Part1 + ', ' + cast(@c as varchar) + ', case when ' + @Part4 + '=0 then 0 else 1 end, case when ' + @Part5 + '=0 then 0 else 1 end from ' + @Part2 + ' where ' + @Part3;
		
			--print @CurQuery + ' '  +  @Part1  + ' ' +  @Part2 + ' ' +  @Part3


			set @CURQUERYFORERROR=@CurQuery
			if @DEBUG=1
			BEGIN
				print @CurQuery
			END
			exec (@CurQuery);
			--print @CurQuery;

			set @c=@c+1
		end;

--Added for work on #189 by BCG
DECLARE @Count int;
DECLARE @CurID int;
DECLARE @DimensionTableName varchar(MAX);
DECLARE @DimensionValueTableName varchar(MAX);
DECLARE @LastDimensionTableName varchar(MAX);

SET @Count=0;
SET @LastDimensionTableName=null;

DECLARE Dimension_Cursor CURSOR FOR
select DimensionTableName, id from DynamicDimension_New
order by DimensionTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DimensionTableName, @CurID
while @@FETCH_STATUS = 0
	BEGIN
	if @LastDimensionTableName is null or @LastDimensionTableName<>@DimensionTableName
		BEGIN
		SET @LastDimensionTableName=@DimensionTableName
		SET @Count=0;
		END
	
	SET @DimensionValueTableName = @DimensionTableName + '_Value' + cast(@Count/250 as varchar)

	DECLARE @CurSQL varchar(max);

	SET @CurSQL='Update DynamicDimension_New set DimensionValueTableName=''' + @DimensionValueTableName + ''' where ID =' + cast(@CurID as varchar)


	set @CURQUERYFORERROR=@CurSQL
	if @DEBUG=1
	BEGIN
		print @CurSQL
	END
	exec(@CurSQL)

	SET @Count=@Count+1

	FETCH NEXT FROM Dimension_Cursor
	INTO @DimensionTableName, @CurID
	END

Close Dimension_Cursor
Deallocate Dimension_Cursor





END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicDimension',@CURQUERYFORERROR);
	THROW;
END CATCH
END



GO
--40
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[PopulateDynamicDimensionPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[PopulateDynamicDimensionPartial]  
END
GO

CREATE PROCEDURE [dbo].[PopulateDynamicDimensionPartial] @DEBUG bit = 0
AS
BEGIN

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

	--truncate table DynamicDimension

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension_New]') AND type in (N'U'))
		CREATE TABLE [dbo].[DynamicDimension_New](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[TableName] [nvarchar](500) NOT NULL,
		[Dimensions] [int] NULL,
		[DimensionTableName] [nvarchar](50) NULL,
		[ComputeAllValues] [bit] NULL,
		[DimensionValueTableName] [nvarchar](100) NULL,
		[ContainsDateDimension] [bit] NULL
	) ON [PRIMARY]

	truncate table DynamicDimension_New
	truncate table AggregationQueries
	DBCC CHECKIDENT ('dbo.AggregationQueries', RESEED, 1);
	

	--Work for #638
	declare @QueriesToRun table (id int identity(1,1), query nvarchar(max))

	--Make sure there is the dirty bit on all base tables first
	insert into @QueriesToRun
	select distinct 'IF NOT EXISTS(SELECT * FROM sys.columns 
				WHERE Name = N''dirty'' AND Object_ID = Object_ID(''' + tablename + '_Base''))
	BEGIN
		DECLARE @AddColumn nvarchar(max)=''ALTER TABLE ' + tablename + '_Base add dirty bit''
		exec(@AddColumn)
	END' from Dimension

	while (select count(*) from @QueriesToRun) > 0
	BEGIN
		DECLARE @ToRun nvarchar(max)=(select query from @QueriesToRun where id=(select min(id) from @QueriesToRun))

		exec(@ToRun)

		delete from @QueriesToRun where id=(select min(id) from @QueriesToRun)
	END

	DECLARE @TablesWithChanges table(id int identity(1,1), tablename nvarchar(max))

	delete from @QueriesToRun

	--Now let's check if there is a dirty bit on any base table

	insert into @QueriesToRun
	select distinct 'select distinct ''' + tablename + ''' from ' + tablename + '_base where dirty=1' from dimension


	--Now let's check to see if there are new rows in the tables

	insert into @QueriesToRun
	select distinct 'select distinct ''' + tablename + ''' from ' + tablename + ' d1 where not exists (select d2.id from ' + tablename + '_base d2 where d2.id=d1.id)' from dimension

	--For ticket #705
	insert into @TablesWithChanges
	select distinct d.tablename from dimension d 
		inner join Measure m on d.TableName=m.MeasureTableName
		where d.ComputeAllValues=1 and m.ComputeAllValues=1


	--Now let's run the queries


	while (select count(*) from @QueriesToRun) > 0
	BEGIN
		SET @ToRun=(select query from @QueriesToRun where id=(select min(id) from @QueriesToRun))

		insert into @TablesWithChanges
		exec(@ToRun)

		delete from @QueriesToRun where id=(select min(id) from @QueriesToRun)
	END


	DECLARE @DCount int;
	set @DCOUNT=(select count(*) from dimension where TableName in (
				select distinct tablename from @TablesWithChanges))


	DECLARE @TableList nvarchar(max)=''

	while (select count(*) from @TablesWithChanges) > 0
	BEGIN
	
		DECLARE @CurTable nvarchar(max)=(select tablename from @TablesWithChanges where id=(select min(id) from @TablesWithChanges))

		SET @TableList = @TableList + '''' + @CurTable + ''''

		delete from @TablesWithChanges where tablename=(select tablename from @TablesWithChanges where id=(select min(id) from @TablesWithChanges))

		if (select count(*) from @TablesWithChanges) > 0
		BEGIN
			SET @TableList=@TableList + ', '
		END
	END

	-- End insert for #638

	DECLARE @c int;
	set @c=2

	while @c<= @DCount
		Begin
		
			DECLARE @CurQuery varchar(MAX);
		
			DECLARE @Part1 varchar(MAX);
			DECLARE @Part2 varchar(MAX);
			DECLARE @Part3 varchar(MAX);
			DECLARE @Part4 varchar(MAX);
			DECLARE @Part5 varchar(MAX);

			DECLARE @Column_Drop varchar(MAX);
			DECLARE @Column_Add varchar(MAX);

			set @Part1=''
			set @Part2=''
			set @Part3=''
			set @Part4=''
			set @Part5=''

			set @Column_Drop=''
			set @Column_Add=''

			DECLARE @sCount int;
			set @sCount=1
			while @sCount <= @c
				Begin
					set @Part1 = @Part1 + ' ''d'' + cast(d' + cast(@sCount as varchar) + '.id as varchar)';
					set @Part2 = @Part2 + ' dimension d' + cast(@sCount as varchar) + ' ';
					set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.IsDeleted=0 '
					if @sCount < @c
						BEGIN
						set @Part1 = @Part1 + ' + '
						set @Part2 = @Part2 + ', '
						set @Part3 = @Part3 + ' and d' + cast(@sCount as varchar) + '.id<d' + cast((@sCount+1) as varchar) + '.id and '
						set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.TableName=d' + cast((@sCount+1) as varchar) + '.TableName '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) + '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) + '
						set @Part3 = @Part3 + ' and ';
						END
					ELSE
						BEGIN
						set @Part1 = @Part1 + ', d' + cast(@sCount as varchar) + '.TableName ';
						set @Part2 = @Part2 + ' '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) '
						END

					set @sCount = @sCount + 1
				end;




			set @CurQuery = N'insert into DynamicDimension_New (TableName, DimensionTableName, Dimensions, ComputeAllValues, containsDateDimension) select ' + @Part1 + ', ' + cast(@c as varchar) + ', case when ' + @Part4 + '=0 then 0 else 1 end, case when ' + @Part5 + '=0 then 0 else 1 end from ' + @Part2 + ' where ' + @Part3 + ' and d1.Tablename in (' + @TableList + ')';
		
			--print @CurQuery + ' '  +  @Part1  + ' ' +  @Part2 + ' ' +  @Part3


			set @CURQUERYFORERROR=@CurQuery
			if @DEBUG=1
			BEGIN
				print @CurQuery
			END
			exec (@CurQuery);
			--print @CurQuery;

			set @c=@c+1
		end;

--Added for work on #189 by BCG
DECLARE @Count int;
DECLARE @CurID int;
DECLARE @DimensionTableName varchar(MAX);
DECLARE @DimensionValueTableName varchar(MAX);
DECLARE @LastDimensionTableName varchar(MAX);

SET @Count=0;
SET @LastDimensionTableName=null;

DECLARE Dimension_Cursor CURSOR FOR
select DimensionTableName, id from DynamicDimension_New
order by DimensionTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DimensionTableName, @CurID
while @@FETCH_STATUS = 0
	BEGIN
	if @LastDimensionTableName is null or @LastDimensionTableName<>@DimensionTableName
		BEGIN
		SET @LastDimensionTableName=@DimensionTableName
		SET @Count=0;
		END
	
	SET @DimensionValueTableName = @DimensionTableName + '_Value' + cast(@Count/250 as varchar)

	DECLARE @CurSQL varchar(max);

	SET @CurSQL='Update DynamicDimension_New set DimensionValueTableName=''' + @DimensionValueTableName + ''' where ID =' + cast(@CurID as varchar)


	set @CURQUERYFORERROR=@CurSQL
	if @DEBUG=1
	BEGIN
		print @CurSQL
	END
	exec(@CurSQL)

	SET @Count=@Count+1

	FETCH NEXT FROM Dimension_Cursor
	INTO @DimensionTableName, @CurID
	END

Close Dimension_Cursor
Deallocate Dimension_Cursor





END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicDimension',@CURQUERYFORERROR);
	THROW;
END CATCH
END

Go
--41
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'proc_auditDatabase') AND xtype IN (N'P'))
    DROP PROCEDURE proc_auditDatabase
GO


-- =============================================
-- Author:		Nate Lee
-- Create date: 2011 September 15
-- Description:	This procedure will poll a table loaded with data and run the standard data audit metrics
-- =============================================
CREATE PROCEDURE [proc_auditDatabase]
	-- Add the parameters for the stored procedure here
	@tableName nvarchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @columnName nvarchar(255);
	DECLARE @unique int;
	DECLARE @singleValue int;
	DECLARE @top20 int;
	DECLARE @notNull int;
	
	DECLARE @uniqueTable table (
		tempColumnName nvarchar(255)
	)
	
	DECLARE @singleValueTable table (
		tempColumnName nvarchar(255)
		, tempAmount int
	)

	DECLARE @top20Table table (
		tempColumnName nvarchar(255)
		, tempAmount int
	)

	DECLARE @notNullTable table (
		tempColumnName nvarchar(255)
	)	

	-- get the column names
	DECLARE cur CURSOR FOR
	SELECT syscolumns.name AS ColumnName
	FROM sysobjects 
	JOIN syscolumns ON sysobjects.id = syscolumns.id
	WHERE sysobjects.xtype = 'U'
	AND sysobjects.name = @tableName
	ORDER BY sysobjects.name, syscolumns.colid
	
	OPEN cur
	
	FETCH NEXT FROM cur INTO @columnName;
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		DECLARE @sql1 nvarchar(4000)
		DECLARE @ParmDefinition1 nvarchar(500);
		DECLARE @sql2 nvarchar(4000)
		DECLARE @ParmDefinition2 nvarchar(500);
		DECLARE @sql3 nvarchar(4000)
		DECLARE @ParmDefinition3 nvarchar(500);
		DECLARE @sql4 nvarchar(4000)
		DECLARE @ParmDefinition4 nvarchar(500);
		
		-- Unique Values
		SET @sql1 = N'SELECT @singleValueOUT = COUNT(*) FROM (
				SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field, Count(*) AS Amounta
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				GROUP BY RTRIM(LTRIM([' + @columnName + ']))
				) as x'
		SET @ParmDefinition1 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql1, @ParmDefinition1, @singleValueOUT=@unique OUTPUT;

		---- Single Values
		--SET @sql2 = N'SELECT @singleValueOUT = COUNT(*) FROM (
		--		SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field, Count(*) AS Amounta
		--		FROM Nate..[' + @tableName + ']
		--		WHERE [' + @columnName + '] IS NOT NULL
		--		AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
		--		GROUP BY RTRIM(LTRIM([' + @columnName + ']))
		--		HAVING COUNT(*) = 1 ) as x'
		--SET @ParmDefinition2 = N'@singleValueOUT int OUTPUT';			 
		--EXEC sp_executesql @sql2, @ParmDefinition2, @singleValueOUT=@singleValue OUTPUT;

		-- Not NULL
		SET @sql3 = N'SELECT @singleValueOUT = COUNT(*) FROM (
				SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				) as x'
		SET @ParmDefinition3 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql3, @ParmDefinition3, @singleValueOUT=@notNull OUTPUT;
		
		-- Top 20
		SET @sql4 = N'SELECT @singleValueOUT = SUM(x.AMT) FROM (
				SELECT TOP 20 RTRIM(LTRIM([' + @columnName + '])) AS Field, COUNT(*) AS AMT
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				GROUP BY RTRIM(LTRIM([' + @columnName + ']))
				ORDER BY AMT DESC
				) as x'
		SET @ParmDefinition4 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql4, @ParmDefinition4, @singleValueOUT=@top20 OUTPUT;

		
		--INSERT INTO @notNullTable (tempColumnName)
		--	SELECT @columnName
		--	FROM Nate..[ZEBRA - Middle Tier]
		--	WHERE @columnName IS NOT NULL
		--	AND @columnName <> ''

		--SET @unique = (SELECT COUNT(*) FROM @uniqueTable);
		--SET @singleValue = (SELECT COUNT(*) FROM @singleValueTable WHERE tempAmount = 1);
		--SET @notNull = (SELECT COUNT(*) FROM @notNullTable);
		
		-- Insert statements for procedure here
		-- How many unique records?
		--INSERT INTO DataAuditResults ([Dataset], [column Name], [Unique Values], [Single Value], [Not Null], [Top 20 Coverage]) 
		--VALUES (@tableName, @columnName, @unique, @singleValue, @notNull, @top20);	
		INSERT INTO DataAuditResults ([Dataset], [column Name], [Unique Values], [Not Null], [Top 20 Coverage]) 
		VALUES (@tableName, @columnName, @unique, @notNull, @top20);	
		
		DELETE FROM @uniqueTable
--		DELETE FROM @singleValueTable
		DELETE FROM @notNullTable
		DELETE FROM @top20Table
		
		FETCH NEXT FROM cur INTO @columnName
	END
	
	CLOSE cur
	DEALLOCATE cur

END


GO
--42
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RebuildIndexes') AND xtype IN (N'P'))
    DROP PROCEDURE RebuildIndexes
GO

CREATE PROCEDURE [RebuildIndexes] @DEBUG bit=0
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TableName varchar(255) 
	DECLARE TableCursor CURSOR FOR 
	--added braces "[" and "]" to the table name PL Ticket # 812 Issue in RebuildIndexes stored procedure in Mongo database
	SELECT '[' + table_name + ']' FROM information_schema.tables 
	WHERE table_type = 'base table' and (table_name like 'd%d%' or TABLE_NAME in ('DimensionValue','DynamicDimension','MeasureValue') or table_name like '%_Value%') 
	OPEN TableCursor 
 
	FETCH NEXT FROM TableCursor INTO @TableName 
	WHILE @@FETCH_STATUS = 0 
	BEGIN 
		--added try catch PL Ticket # 812 Issue in RebuildIndexes stored procedure in Mongo database
		BEGIN TRY
			DBCC DBREINDEX(@TableName,' ',90) 
			exec ('alter table ' +  @TableName + ' REBUILD')
		END TRY
		BEGIN CATCH
		END CATCH
		FETCH NEXT FROM TableCursor INTO @TableName 
	END 
 
	CLOSE TableCursor 
	DEALLOCATE TableCursor 
	exec sp_updatestats
END

GO

--43
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ReportGraphResultsNew') AND xtype IN (N'P'))
    DROP PROCEDURE ReportGraphResultsNew
GO


CREATE PROCEDURE [dbo].[ReportGraphResultsNew] -- exec ReportGraphResultsNew 9,'D21D22','01/01/2015','12/31/2015','D1',null,'Q',0,0,NULL,'14d7d588-cf4d-46be-b4ed-a74063b67d66','504f5e26-2208-44c2-a78f-4bdf4bab703f'
@ReportGraphID INT, 
@DIMENSIONTABLENAME NVARCHAR(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD NVARCHAR(100)=null, 
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@SubDashboardOtherDimensionTable INT = 0,	-- Added by Sohel Pathan on 20/02/2015 for PL ticket #129
@SubDashboardMainDimensionTable INT = 0,	-- Added by Sohel Pathan on 20/02/2015 for PL ticket #129
@DisplayStatSignificance NVARCHAR(15) = NULL, -- Added by Arpita Soni on 08/19/2015 for PL ticket #427
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511

AS
BEGIN
SET NOCOUNT ON;
DECLARE @DateDimensionID int
 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

--For issue #609
if @FilterValues='''<filters></filters>'''
BEGIN
	SET @FilterValues=null
END

Declare @CustomQuery NVARCHAR(MAX)
SET @CustomQuery=(SELECT ISNULL(CustomQuery,'') FROM ReportGraph where Id=@ReportGraphID)
IF(@CustomQuery!='')
BEGIN
EXEC CustomGraphQuery @ReportGraphID,@STARTDATE,@ENDDATE,@FilterValues,@ViewByValue,@DIMENSIONTABLENAME,@DateDimensionID
END
ELSE


BEGIN
SET @DisplayStatSignificance=ISNULL(@DisplayStatSignificance,'')
--to do please add report id to all the table variables 
DECLARE @Dimensions NVARCHAR(MAX), @TempString NVARCHAR(MAX), @Count INT, @Measures NVARCHAR(1000), @GroupBy NVARCHAR(1000), @FirstTable NVARCHAR(MAX), @SymbolType NVARCHAR(50) ,@ConfidenceLevel FLOAT
DECLARE @SecondTable NVARCHAR(MAX)  DECLARE @MeasureCreateTableDimension NVARCHAR(MAX) DECLARE @MeasureSelectTableDimension NVARCHAR(1000), @IsDateOnYaixs BIT=0, @DimensionCount INT=0
DECLARE @UpdateTableCondition NVARCHAR(500),  @DimensionName1 NVARCHAR(50), @DimensionName2 NVARCHAR(50), @Measure NVARCHAR(50), @DimensionId1 INT, @DimensionId2 INT, @IsDateDimension BIT=0 ,@IsOnlyDateDimension BIT=0
DECLARE @FinalTable NVARCHAR(MAX), @MeasureCount INT DECLARE @SQL NVARCHAR(MAX), @Columns NVARCHAR(1000) DECLARE @BaseQuery NVARCHAR(MAX), @GType NVARCHAR(100)

SET @FirstTable='';  SET @SymbolType=''; SET @Columns='' ; SET @MeasureCreateTableDimension='';SET @MeasureSelectTableDimension='';SET @UpdateTableCondition='';SET @BaseQuery='';SET @SecondTable=''
SET @SQL=''; SET @DimensionName1=''; SET @DimensionName2=''; SET @Measure=''; SET @GroupBy='';SET @FinalTable='';SET @TempString=''
if @FilterValues='<filters></filters>'
BEGIN
SET @FilterValues=null
END	
		
SELECT @GType =LOWER (GraphType),@ConfidenceLevel=ISNULL(ConfidenceLevel,0) FROM ReportGraph WHERE id = @ReportGraphID

 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

SELECT 
		@Dimensions						= selectdimension, 
		@Columns						= CreateTableColumn,
		@MeasureCreateTableDimension	= MeasureTableColumn, 
		@MeasureSelectTableDimension	= MeasureSelectColumn, 
		@UpdateTableCondition			= UpdateCondition,
		@GroupBy						= GroupBy,
		@DimensionCount					= CAST(Totaldimensioncount AS INT), 
		@DimensionName1					= DimensionName1, 
		@DimensionName2					= DimensionName2, 
		@DimensionId1					= CAST(DimensionId1 AS INT), 
		@DimensionId2					= CAST(DimensionId2 AS INT),
		@IsDateDimension				= CAST(IsDateDImensionExist AS BIT), 
		@IsDateOnYaixs					= CAST(IsDateOnYAxis AS BIT), 
		@IsOnlyDateDimension			= CAST(IsOnlyDateDImensionExist AS BIT) 
FROM dbo.GetDimensions(@ReportGraphID,@ViewByValue,@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@GType,@SubDashboardOtherDimensionTable,@SubDashboardMainDimensionTable,@DisplayStatSignificance)

--Measure Table
 SELECT 
		@Measures	= SelectTableColumn, 
		@Columns	= @Columns + CreateTableColumn, 
		@Measure	= MeasureName,
		@SymbolType	= SymbolType 
FROM dbo.GetMeasures(@ReportGraphID,@ViewByValue,@DimensionCount,@GTYPe,@DisplayStatSignificance)

--1 means creating base query for dimension - only order by cluase will be added
SET @BaseQuery = dbo.DimensionBaseQuery(@DIMENSIONTABLENAME, @STARTDATE, @ENDDATE, @ReportGraphID, @FilterValues,@IsOnlyDateDimension, 1,@UserId,@RoleId)
--print @basequery
--
SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#',@Dimensions + @Measures)
--print @Columns
DECLARE @Table NVARCHAR(1000)
SET @Table = ''
SET @Table = 'DECLARE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' TABLE('
SET @Table = @Table+' '+ @Columns + ')'
SET @FirstTable = @Table + 'INSERT INTO @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' ' + @SQL + ''; --insert in to dimension table

SET @BaseQuery= (dbo.DimensionBaseQuery(@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@ReportGraphID,@FilterValues,@IsOnlyDateDimension,0,@UserId,@RoleId))
--print @basequery
DECLARE @MeasureId INT, @AggregationType NVARCHAR(50), @MeasureName NVARCHAR(100)
SET @Table=' DECLARE @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' table(' + ' ' + @MeasureCreateTableDimension + ',Rows float, Measure float'+')'

SET @SecondTable = @SecondTable + @Table

--print @MeasureSelectTableDimension + @Measures

DECLARE @MeasureCursor CURSOR
SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT MeasureId,Measurename,AggregationType  FROM dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue)
OPEN @MeasureCursor
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
WHILE @@FETCH_STATUS = 0 
BEGIN
  IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND  LOWER(@DisplayStatSignificance)!='rate')
   SET @MeasureName='Measure_'+@MeasureName
	SET @Measures = dbo.GetMeasuresForMeasureTable(@ReportGraphID,@MeasureId)
	IF(@IsOnlyDateDImension=1 AND @FilterValues IS NULL )
	SET @Measures=REPLACE(@Measures,'rows','RecordCount')
	SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#', @MeasureSelectTableDimension + @Measures )
	--SET SQL

	--In base query if there are row excluded or filter is passed as parameter then there is already where cluase so we have to check condition
	IF CHARINDEX('where',LOWER(@SQL)) <= 0
		SET @SQL=@SQL+' WHERE '
	ELSE
		SET @SQL=@SQL+' AND '
	
	SET @SQL = @SQL + ' D1.Measure=' + CAST(@MeasureId AS NVARCHAR) + ' GROUP BY ' + @GroupBy
			
	SET @SecondTable=@SecondTable+' INSERT INTO @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' '+@SQL
	
		IF(LOWER(@GType)='errorbar' OR LOWER(@DisplayStatSignificance)='rate')	
		BEGIN
			DECLARE @ErrorbarQuery NVARCHAR(2000)='', @UpdateErrorbarQuery NVARCHAR(2000)=' ', @PopulationRate FLOAT, @NoOfTails FLOAT=2
			
			IF( @Confidencelevel < 0 OR @Confidencelevel > 1)
				SET @ConfidenceLevel=0

		IF(@DimensionCount = 1)
			BEGIN
			IF(LOWER(@AggregationType) = 'avg')
			BEGIN
			SET @PopulationRate=(SELECT ROUND((SUM(value*RecordCount)/SUM(RecordCount)),2) FROM MeasureValue WHERE DimensionValue in (SELECT id FROM DimensionValue where DimensionID in(select DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
				
			END
			ELSE
			BEGIN
				SET @PopulationRate=(SELECT ROUND(AVG(value),2) FROM MeasureValue WHERE DimensionValue IN (SELECT id FROM DimensionValue where DimensionID IN(SELECT DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
			END
		
			IF(@PopulationRate>1 OR @PopulationRate<0)
				SET @PopulationRate = 1;
				SET @ErrorbarQuery =' Update @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+CAST(@PopulationRate AS NVARCHAR)
				SET @SecondTable = @SecondTable + @ErrorbarQuery + ';'
			END

		ELSE -- @DimensionCount = 2
			BEGIN
				DECLARE @ExistDimensionTableName NVARCHAR(100), @Pr_DimensionId INT, @SeriesDimension INT, @SeriesDimensionIndex INT, @YaxisIndex INT
				DECLARE @SeriesDimensionName NVARCHAR(500), @DateAdded bit =0, @IsDateDimensionOnYAxis INT, @Pr_IsDateDimensionExist INT
			
				SELECT @Pr_IsDateDimensionExist = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID) and IsDateDimension=1;
				SET @SeriesDimension=(select dimensionid FROM ReportAxis WHERE  ReportGraphId = @ReportGraphID and axisname='Y')
				
				SET @SeriesDimensionName=(select columnname FROM Dimension where id=@SeriesDimension)
			
				SET @YaxisIndex=2
				SELECT @IsDateDimensionOnYAxis = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID and axisname='Y') and IsDateDimension=1;
			
				DECLARE @DimensionCursor CURSOR
				SET @DimensionCursor = CURSOR FAST_FORWARD FOR SELECT dimensionid FROM   reportaxis where reportgraphid=@ReportGraphID order by dimensionid 
				OPEN @DimensionCursor
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				WHILE @@FETCH_STATUS = 0
					BEGIN

						IF(@Pr_IsDateDimensionExist=0 and @DateDimensionId < @Pr_DimensionId and @DateAdded=0)
						BEGIN
							SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@DateDimensionId as NVARCHAR)) 
							SET @DateAdded=1
						END

					SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@Pr_DimensionId as NVARCHAR)) 
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				END
				CLOSE @DimensionCursor
				DEALLOCATE @DimensionCursor	
					SET @SeriesDimensionIndex=1
				IF(@DateAdded=1)
				SET @SeriesDimensionIndex= (select cnt FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'d') where Dimension=CAST(@SeriesDimension as NVARCHAR))
				ELSE
				IF((select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='Y')>(select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='X'))
				SET @SeriesDimensionIndex=2
				DECLARE @DisplayValue NVARCHAR(500), @Dimension NVARCHAR(50), @Populationratestring NVARCHAR(1000)
			
				IF(@YaxisIndex=1)
					SET @Dimension = @DimensionName1
				ELSE
					SET @Dimension = @DimensionName2
					SET @DisplayValue='DisplayValue'
					IF(@IsDateDimensionOnYAxis>0)
						SET @DisplayValue=' dimensionid='+CAST(@DateDimensionId as NVARCHAR)+' and CAST(dbo.GetDatePart(CAST(DisplayValue AS DATE),'''+@ViewByValue+''','''+CAST(@STARTDATE as NVARCHAR)+''','''+CAST(@Enddate as NVARCHAR)+''') as NVARCHAR) '
							IF(LOWER(@AggregationType)='avg')
							BEGIN
						
							SET @PopulationRateString='((select (SUM(Value*Rows)/SUM(rows)) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
						
							END
							ELSE
							BEGIN
							SET @PopulationRateString='((select AVG(value) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
								
							
							END
							SET @ErrorbarQuery=' UPDATE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+@PopulationRateString
							SET @SecondTable=@SEcondTable+@ErrorbarQuery+';'
		END -- Error graph @DimensionCount End
		IF(LOWER(@DisplayStatSignificance)='rate')
		BEGIN
		IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
		ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END
		END
		ELSE IF(LOWER(@GType)='errorbar')
		BEGIN
	    IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
			ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END

		END -- Error graph complete
		END

		 IF(@gtype!='errorbar' AND LOWER(@DisplayStatSignificance)!='rate')
				SET @UpdateErrorBarQuery=' '
				SET @SecondTable=@SecondTable+' UPDATE @dimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET Rows=M.Rows,['+@MeasureName+']=M.[Measure]'+@UpdateErrorBarQuery+' FROM @dimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' D INNER JOIN @MeasureTable'+CAST(@ReportGraphID AS NVARCHAR)+' M ON '+@UpdateTableCondition+' delete FROM @Measuretable'+CAST(@ReportGraphID as NVARCHAR)
			 
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
END
CLOSE @MeasureCursor
DEALLOCATE @MeasureCursor
DECLARE @SelectTable NVARCHAR(MAX)


IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND LOWER(@DisplayStatSignificance)!='rate')
BEGIN

	DECLARE @DimensionValues NVARCHAR(MAX)
	IF(@IsDateOnYaixs=0)
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + '[' + DisplayValue + ']'  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) ORDER BY OrderValue
	ELSE
	BEGIN
			IF(@ViewByValue = 'Q') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'Y') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'M') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue='W')
			BEGIN
				IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']') A
					ORDER BY OrderValue
				END
				ELSE 
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']') A
					ORDER BY OrderValue
				END
			END
			ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']') A
				ORDER BY OrderValue
			END
	END
	
	--SET @SelectTable='; SELECT '+ @DimensionName1 + ',' + @DimensionValues + ' FROM ( ' +
	SET @SelectTable='; SELECT * FROM ( ' +
	'SELECT ' + @DimensionName1 + ',' + @DimensionName2 + ',[' + @Measure + '],MAX(OrderValue1) OVER (PARTITION BY '+ @DimensionName1 +') OrderValue FROM @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' ' + 
	') P PIVOT ('+
	'MAX(['+ @Measure +']) FOR ' + @DimensionName2 + ' IN ('+ @DimensionValues +')'+
	') AS PVT ORDER BY OrderValue' 
	END
	ELSE
	BEGIN
		DECLARE @OrderBy NVARCHAR(500) 
		IF(@DimensionCount>1)
			SET @OrderBy=' ORDER BY OrderValue1,OrderValue2'
		ELSE
			SET @OrderBy=' ORDER BY OrderValue1  '
		SET @SelectTable=';SELECT * FROM @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' '+@OrderBy
	END
	print 	@FirstTable
	print @SecondTable
	print @finaltable
	print @SelectTable 
	EXEC(@FirstTable+@SecondTable+@finaltable+@SelectTable )
END

END
Go
--44

IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[RunAggregation]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[RunAggregation]  
END
GO



CREATE PROCEDURE [dbo].[RunAggregation] @DEBUG bit = 0, @Partial bit =0, @ReturnStatus int = 0 output
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE StepCursor CURSOR FOR
	select stepName, LogStart, LogEnd, QueryText, StatusCode, PartialQueryText from AggregationSteps where isDeleted=0 order by StepOrder
	BEGIN TRY

	DECLARE @StepName varchar(max);
	DECLARE @LogStart bit;
	DECLARE @LogEnd bit;
	DECLARE @QueryText varchar(max);
	DECLARE @PartialQueryText varchar(max);
	DECLARE @StatusCode varchar(max);


	OPEN StepCursor
	FETCH NEXT FROM StepCursor INTO @StepName, @LogStart, @LogEnd, @QueryText, @StatusCode, @PartialQueryText
	WHILE @@FETCH_STATUS =0
	BEGIN

		if @StatusCode is null 
		BEGIN
			SET @StatusCode='RUNNING'
		END

		update AggregationStatus set StatusCode=@StatusCode

		if @LogStart=1 
		BEGIN
			insert into Logging select getDate(), @StepName + ' Start'
		END

		if @Partial=1 and @PartialQueryText is not null and len(@PartialQueryText) > 0
		BEGIN
			exec(@PartialQueryText)
		END
		ELSE
		BEGIN
			exec(@QueryText)
		END

		if @LogEnd=1
		BEGIN
			insert into Logging select getDate(), @StepName + ' End'
		END

		FETCH NEXT FROM StepCursor INTO @StepName, @LogStart, @LogEnd, @QueryText, @StatusCode, @PartialQueryText
	END

	CLOSE StepCursor
	DEALLOCATE StepCursor



	insert into Logging select GETDATE(), 'End Aggregation'
	SET @ReturnStatus = 1
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'SUCCESS','',GETDATE(),'bcg',null,null)
	END TRY
	BEGIN CATCH
		CLOSE StepCursor
		DEALLOCATE StepCursor
		SET @ReturnStatus = 0
		INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','RunAggregation',null)
	END CATCH;
	update AggregationStatus set StatusCode='NOTRUNNING'
	return @ReturnStatus
END

GO
--45
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'TTestCalculation') AND xtype IN (N'P'))
    DROP PROCEDURE TTestCalculation
GO

CREATE PROCEDURE [TTestCalculation] 
(
@QueryToRun VARCHAR(MAX),
@Pvalue FLOAT OUTPUT
)
AS
BEGIN
	--Query passed in as value must return two fields (both floats).  Procedure will return null if data is degenerate.
	DECLARE @DataValues TABLE(d1 FLOAT, d2 FLOAT) 
	BEGIN TRY
		
		INSERT INTO @DataValues
		EXEC(@QueryToRun);
		
		DECLARE @Var1 FLOAT = (SELECT POWER( STDEV(d1),2) FROM @DataValues);
		DECLARE @Var2 FLOAT = (SELECT POWER( STDEV(d2),2) FROM @DataValues);
		DECLARE @Mu1 FLOAT = (SELECT AVG(d1) FROM @DataValues);
		DECLARE @Mu2 FLOAT = (SELECT AVG(d2) FROM @DataValues);
		DECLARE @Count1 FLOAT = (SELECT COUNT(*) FROM @DataValues WHERE d1 IS NOT NULL);
		DECLARE @Count2 FLOAT = (SELECT COUNT(*) FROM @DataValues WHERE d2 IS NOT NULL);

		DECLARE @TStat FLOAT = (@Mu1-@Mu2)/SQRT(@Var1/@Count1+@Var2/@Count2);
		DECLARE @DegsFree FLOAT = (POWER(@Var1/@Count1+@Var2/@Count2,2)/(POWER(@Var1,2)/(@Count1*@Count1*(@Count1-1))+POWER(@Var2,2)/(@Count2*@Count2*(@Count2-1))));
		DECLARE @VValue INT;

		IF @DegsFree < 100.5
		BEGIN
			SET @VValue=ROUND(@DegsFree,0);
		END

		IF @DegsFree>=100.5
		BEGIN
			SET @PValue=EXP(-1*POWER(@TStat,2)/2)/SQRT(2*PI());
		END
		ELSE
		BEGIN
			SET @PValue=(SELECT Probability FROM TValues WHERE DEGREES=@VValue AND ABS(Tstat-ABS(@Tstat)) IN (SELECT MIN(ABS(Tstat-ABS(@Tstat))) FROM TValues WHERE DEGREES=@VValue))
		END
		RETURN @PValue

	END TRY
	BEGIN CATCH
		RETURN NULL
	END CATCH
END;

GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE   object_id = OBJECT_ID(N'GetFilterXmlString') AND type IN (  N'FN', N'IF', N'TF', N'FS', N'FT') ) 
BEGIN
	DROP function [GetFilterXmlString]
END
GO
CREATE FUNCTION [dbo].[GetFilterXmlString] (	
	-- Add the parameters for the function here
	@FilterVAlues NVARCHAR(MAX),
	@DimensionName NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @FilterXML NVARCHAR(MAX)
	DECLARE @TempDimensionTable AS TABLE (Id INT, DimensionIds INT)
	INSERT INTO @TempDimensionTable
	select ROW_NUMBER() OVER (ORDER BY dimension) AS RowNo,dimension from [dbo].[fnSplitString](@DimensionName, 'D')

	DECLARE @TempDimensionValueTable AS TABLE (Id INT, DimensionValues NVARCHAR(MAX))
	INSERT INTO @TempDimensionValueTable
	select ROW_NUMBER() OVER (ORDER BY dimension) AS RowNo,dimension from [dbo].[fnSplitString](@FilterVAlues, ',')

	DECLARE @intFlag INT
	DECLARE @TotalRow INT

	SET @intFlag = 1
	SET @TotalRow = (select COUNT(dimension) from [dbo].[fnSplitString](@FilterVAlues, ','))

	WHILE (@intFlag <= @TotalRow)
	BEGIN
		DECLARE @DimId INT
		DECLARE @DimVal INT
		DECLARE @FilterIndex INT

		SET @DimId = (SELECT LEFT(DimensionValues, CHARINDEX(':',DimensionValues)-1) FROM @TempDimensionValueTable WHERE Id = @intFlag)
		SET @DimVal = (SELECT RIGHT(DimensionValues,LEN(DimensionValues)-CHARINDEX(':',DimensionValues)) FROM @TempDimensionValueTable WHERE Id = @intFlag)
		SET @FilterIndex = (SELECT Id FROM @TempDimensionTable WHERE DimensionIds = @DimId)

		IF EXISTS (SELECT * FROM @TempDimensionTable WHERE DimensionIds = @DimId)
		BEGIN
			SET @FilterXML = CONCAT(@FilterXML, '<filter ID="' + CAST(@FilterIndex AS NVARCHAR(MAX)) + '">' + CAST(@DimVal AS NVARCHAR(MAX)) + '</filter>')
		END

		SET @intFlag = @intFlag + 1
	END
	RETURN @FilterXML
END
Go
--46
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'WebApiGetReportRawData') AND xtype IN (N'P'))
    DROP PROCEDURE WebApiGetReportRawData
GO
--WebApiGetReportRawData 96564698,0,'Q','1/1/2014','12/31/2014'

CREATE PROCEDURE [dbo].[WebApiGetReportRawData] 
(
	@Id INT, @TopOnly BIT = 1, @ViewBy NVARCHAR(2) = 'Q',@StartDate DATETIME= '1/1/1900', @EndDate DATETIME = '1/1/2100', @FilterValues NVARCHAR(MAX) = NULL
)
AS 
BEGIN
	SET NOCOUNT ON;
	--Identify if it is a custom query or not
	DECLARE @CustomQuery NVARCHAR(MAX),@DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX),@CustomFilter NVARCHAR(MAX), @GT NVARCHAR(100)
	SELECT TOP 1 
			@GT						= ISNULL(G.GraphType,''),
			@CustomQuery			= ISNULL(G.CustomQuery,''),
			@DrillDownCustomQuery	= ISNULL(G.DrillDownCustomQuery,''),
			@DrillDownXFilter		= ISNULL(G.DrillDownXFilter,''),
			@CustomFilter			= ISNULL(G.CustomFilter,'')
	FROM ReportGraph G 
		LEFT JOIN ReportAxis A			ON G.id = A.ReportGraphId 
		WHERE G.Id = @Id

	IF(@GT != 'bar' AND @GT != 'column' AND @GT != 'pie' AND @GT != 'donut' AND @GT != 'line' AND @GT != 'stackbar' AND @GT != 'stackcol')
	BEGIN
			SET @GT = 'Currently we are not supporting graph type "'+ @GT +'"  in Measure Report Web API'
			RAISERROR(@GT,16,1) 
	END


	--Identify if there is only date dimension is configured and will return with the chart attribute
	DECLARE @IsDateDimensionOnly BIT = 0
	IF(@CustomQuery != '') --Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
	BEGIN
		SET @IsDateDimensionOnly = 
			CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) = 1 THEN
					(SELECT COUNT(*) FROM ReportAxis  A
					INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
					WHERE ReportGraphId  = @Id)
			ELSE 0 
		END
	END

	--Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
	DECLARE @DateOnX BIT = 0
	IF(@CustomQuery = '') 
	BEGIN
		SET @DateOnX = 
			CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) > 1 THEN
					(SELECT COUNT(*) FROM ReportAxis  A
					INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
					WHERE ReportGraphId  = @Id AND AxisNAme = 'X')
			ELSE 0 
		END
	END
	
	DECLARE @ColumnNames NVARCHAR(MAX) 
	SELECT @ColumnNames = COALESCE(@ColumnNames + ', ', '') +  AttributeKey FROM (SELECT DISTINCT  '[' + AttributeKey + ']' AttributeKey FROM ChartOptionAttribute)  A
	IF(@ColumnNames IS NULL OR @ColumnNames ='')
		SET @ColumnNames = 'X'

	DECLARE @Query NVARCHAR(MAX);
	SET @Query = '
	;WITH ReportAttribute AS (
	SELECT 
			Id,
			GraphType				=	ISNULL(GraphType,''''),
			IsLableDisplay			=	ISNULL(IsLableDisplay,0),
			IsLegendVisible			= 	ISNULL(IsLegendVisible,0),
			LegendPosition			=	ISNULL(LegendPosition,''right,middle,y''),
			IsDataLabelVisible		= 	ISNULL(IsDataLabelVisible,0),
			DataLablePosition		=	ISNULL(DataLablePosition,''''),
			DefaultRows				=	ISNULL(DefaultRows,10),
			ChartAttribute			=	ISNULL(ChartAttribute,''''),
			ConfidenceLevel			=	ConfidenceLevel,
			CustomQuery				=	ISNULL(CustomQuery,''''),
			IsSortByValue			=	ISNULL(IsSortByValue,0),
			SortOrder				=	ISNULL(SortOrder,''asc''),
			DrillDownCustomQuery	=	ISNULL(DrillDownCustomQuery,''''),
			DrillDownXFilter		=	ISNULL(DrillDownXFilter,''''),
			CustomFilter			=	ISNULL(CustomFilter,''''),
			TotalDecimalPlaces		=	(SELECT TOP 1 CASE WHEN ISNULL(TotalDecimalPlaces,-1) = -1 THEN ISNULL(G.TotalDecimalPlaces,-1) ELSE TotalDecimalPlaces END FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
			MagnitudeValue			=	(SELECT TOP 1 CASE WHEN ISNULL(MagnitudeValue,'''') = '''' THEN ISNULL(G.MagnitudeValue,'''') ELSE MagnitudeValue END  FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
			DimensionCount			=   CASE WHEN ISNULL(CustomQuery,'''') = ''''
											THEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + ')
										ELSE 
											CASE WHEN CHARINDEX(''#DIMENSIONGROUP#'',CustomQuery) <= 0
												THEN 1
												ELSE 2
											END
										END,
			SymbolType = (SELECT TOP 1 ISNULL(SymbolType,'''') FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
			IsDateDimensionOnly		=   '+ CAST(@IsDateDimensionOnly AS NVARCHAR) +',
			DateOnX					=   '+ CAST(@DateOnX AS NVARCHAR) +'
			FROM ReportGraph G
			WHERE G.Id = ' + CAST(@Id AS NVARCHAR) + '
	),
	ExtendedAttribute AS
	(
	
		SELECT * FROM (
					SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
					LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
					WHERE C1.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '
			) AS R
			PIVOT 
			(
				MIN(AttributeValue)
				FOR AttributeKey IN ( '+  @ColumnNames + ')
			) AS A
				
	)
	SELECT *,ChartColor = dbo.GetColor(E.ColorSequenceNo) FROM ReportAttribute R
	LEFT JOIN ExtendedAttribute E ON R.id = E.ReportGraphId1
	'
	
	--This dynamic query will returns all the attributes of chart
	
	EXEC(@Query)

	
	DECLARE @DateDimensionId INT;
	DECLARE @DimensionName VARCHAR(8000);
	DECLARE @FilterXML NVARCHAR(MAX) = NULL
	DECLARE @FilterXMLString NVARCHAR(MAX) = NULL
	DECLARE @DimList NVARCHAR(MAX) = NULL
		IF(@CustomQuery != '') --In case of custom query is configured for the report
		BEGIN
			
			SELECT TOP 1 
				@DateDimensionId = D.Id
			FROM ReportGraph G 
				LEFT JOIN ReportAxis A			ON G.id = A.ReportGraphId 
				INNER JOIN Dimension D			ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
			WHERE G.Id = @Id

			IF (@FilterValues IS NOT NULL AND @FilterValues != '')
			BEGIN
				SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
				SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
				SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
			END
			ELSE
			BEGIN
				SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
			END

			IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
			BEGIN
				SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
			END

			IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
			BEGIN
				SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
			END
			
			IF((CHARINDEX('#DIMENSIONGROUP#',@CustomQuery) > 0 OR CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) > 0) AND ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
			BEGIN
				EXEC [CustomGraphQuery]  
					@ReportGraphID			= @Id, 
					@STARTDATE				= @StartDate, 
					@ENDDATE				= @EndDate,
					@FilterValues			= @FilterXML,
					@ViewByValue			= @ViewBy,
					@DimensionTableName		= '',
					@DateDimensionId		= @DateDimensionId,--this value must be pass , other wise CustomGraphQuery will throw an error
					@IsDrillDownData		= 0,
					@DrillRowValue			= NULL,
					@SortBy					= NULL,
					@SortDirection			= NULL,
					@PageSize				= NULL,
					@PageIndex				= NULL,
					@IsExportAll			= 0
			END
			ELSE IF(CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) <= 0)
			BEGIN
				EXEC [CustomGraphQuery]  
					@ReportGraphID			= @Id, 
					@STARTDATE				= @StartDate, 
					@ENDDATE				= @EndDate,
					@FilterValues			= @FilterXML,
					@ViewByValue			= @ViewBy,
					@DimensionTableName		= '',
					@DateDimensionId		= '', --this value is not passed here
					@IsDrillDownData		= 0,
					@DrillRowValue			= NULL,
					@SortBy					= NULL,
					@SortDirection			= NULL,
					@PageSize				= NULL,
					@PageIndex				= NULL,
					@IsExportAll			= 0
			END
			ELSE 
			BEGIN
					RAISERROR('Date Dimension is not configured for Report ',16,1) 
			END
		END
		ELSE --In case of custom query is not configured for the report, but Dimension and Measure are configured
		BEGIN
				SELECT TOP 1 @DateDimensionId = DateD.id FROM ReportAxis A 
					INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDeleted = 0
					INNER JOIN Dimension DateD ON D.TableName = DateD.TableName and DateD.IsDateDimension = 1 AND DateD.IsDeleted = 0
				WHERE A.ReportGraphId = @Id

				IF(ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
				BEGIN
						IF (@FilterValues IS NOT NULL AND @FilterValues != '')
						BEGIN
							SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
							SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
							SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
						END
						ELSE
						BEGIN
							SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
						END

						IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
						BEGIN
							SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
						END						

						IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
						BEGIN
							SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
						END
						
						EXEC [ReportGraphResultsNew]
							@ReportGraphID						= @Id, 
							@DIMENSIONTABLENAME					= @DimensionName, 
							@STARTDATE							= @StartDate, 
							@ENDDATE							= @EndDate, 
							@DATEFIELD							= @DateDimensionId, 
							@FilterValues						= @FilterXML,
							@ViewByValue						= @ViewBy,
							@SubDashboardOtherDimensionTable	= 0,
							@SubDashboardMainDimensionTable		= 0,
							@DisplayStatSignificance			= NULL,
							@UserId								= NULL,
							@RoleId								= NULL
				END
				ELSE
				BEGIN
						RAISERROR('Date Dimension is not configured for Report ',16,1) 
				END
		END
END

GO
--47
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'GetHelpText') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetHelpText]
END
GO

CREATE PROCEDURE [dbo].[GetHelpText]
	-- Add the parameters for the stored procedure here
	@ReportDashboardID int,
    @GraphType varchar(50)
AS
BEGIN
	if(@GraphType = 'Widget' OR @GraphType = 'KeydataWidget')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join DashboardContents dc on (ht.Id = dc.HelpTextId and dc.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'Keydata')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join keydata kd on (ht.Id = kd.HelpTextId and kd.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'Dashboard')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join dashboard db on (ht.Id = db.HelpTextId and db.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'HomeDashboard')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join Homepage db on (ht.Id = db.HelpTextId and db.id = @ReportDashboardID)
	End
END

Go
--48
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'StatisticallyInferredAttribution') AND xtype IN (N'P'))
    DROP PROCEDURE StatisticallyInferredAttribution
GO




CREATE PROCEDURE StatisticallyInferredAttribution 
	@StartDate Date ='1/1/1900', @EndDate Date = '1/1/2100', @BaseRateQuery varchar(max)=null,
	@CLOSED_WON varchar(max)='''Closed Won''', @CLOSED_STAGES nvarchar(255)='''Closed Lost'',''Closed Won''',
	 @SELECT_CAMPAIGN_COUNTS varchar(max)=null, @CAMPAIGN_TABLE varchar(max)='_Campaigns',
	 @CAMPAIGN_IDFIELD varchar(max)='CampaignID', @MIN_RESPONSES varchar(max)='49',
	 @CAMPAIGN_WHERE varchar(max)=null, @CAMPAIGN_START varchar(max)=null,
	 @FINAL_JOIN varchar(max)=null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @BaseRate float = 0.5
	if @BaseRateQuery<>null
	BEGIN
		SET @BaseRateQuery=replace(@BaseRateQuery,'#EndDate#',@EndDate)
		DECLARE @StorageTable table(val float)
		insert into @StorageTable
		exec(@BaseRateQuery)
		SET @BaseRate=(select top 1 val from @StorageTable)
	END

	SET @SELECT_CAMPAIGN_COUNTS=replace(@SELECT_CAMPAIGN_COUNTS, '#CLOSED_STAGES#', @CLOSED_STAGES)
	SET @SELECT_CAMPAIGN_COUNTS=replace(@SELECT_CAMPAIGN_COUNTS, '#EndDate#', @EndDate)

	SET @CAMPAIGN_WHERE=replace(@CAMPAIGN_WHERE,'#EndDate#',@EndDate)
	SET @CAMPAIGN_WHERE=replace(@CAMPAIGN_WHERE,'#StartDate#',@StartDate)

	DECLARE @CAMPAIGN_SELECT varchar(max)='
	select c2.CampaignId, o2.opportunityid, count(*) as respcount,w1.[Weight] * count(*) as totalWeight   
	from _OpportunityContactRole o1
	inner join _Opportunities o2 on o1.OpportunityId=o2.OpportunityID
	inner join _CampaignMembers c1 on o1.ContactID=c1.ContactID
	inner join _Campaigns c2 on c1.CampaignID=c2.CampaignID
	'


	DECLARE @FINAL_QUERY varchar(max)='
	select a1.CampaignID, c1.CampaignName, sum((a1.totalWeight/a2.AllWeight)*o1.Amount) as allocation from 
	(
	' + @CAMPAIGN_SELECT + '

	inner join 
	(
	select c.' + @CAMPAIGN_IDFIELD + ' as CampaignID, isnull(A.[Weight],0.5) as [weight] from ' + @CAMPAIGN_TABLE + ' c
	left outer join (

		select campaignID, 
		dbo.normsdist((sum(case when stagename=' + @CLOSED_WON + '  then campaign_count else 0 end)/cast(sum(campaign_count) as float)-' + cast(@BASERate as nvarchar) + ')/sqrt((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float))*(1-(sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)))/sum(campaign_count)))
		as [Weight]
		from (

			select campaignID, stagename, count(*) as campaign_count from (

			' + @SELECT_CAMPAIGN_COUNTS + '
			) a 
			group by campaignID, stagename
			) B



		group by campaignID
		having sum(campaign_count)>' + @MIN_RESPONSES + ') A on c.CampaignId=a.CampaignId
		where ' + @CAMPAIGN_START + '<= ''' + cast(@EndDate as nvarchar) + '''


		)
	 w1 on c2.CampaignID=w1.CampaignID

	where ' + @CAMPAIGN_WHERE + '
	group by c2.CampaignID, o2.Opportunityid, w1.[Weight]

	) A1
	inner join (

		select opportunityid, sum(totalWeight) as AllWeight from 
		(
		' + @CAMPAIGN_SELECT + '

			inner join 
			(
				select c.CampaignID, isnull(A.[Weight],0.5) as [weight] from _Campaigns c
				left outer join (

				select campaignID, 
				dbo.normsdist((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)-' + cast( @BASERate as nvarchar) + ')/sqrt((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float))*(1-(sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)))/sum(campaign_count)))
				as [Weight]
				from (

				select campaignID, stagename, count(*) as campaign_count from (

			' + @SELECT_CAMPAIGN_COUNTS + '
			) a 
			group by campaignID, stagename
			) B


		group by campaignID
		having sum(campaign_count)>' + @MIN_RESPONSES + ') A on c.CampaignId=a.CampaignId
		where ' + @CAMPAIGN_START + '<=''' + cast( @EndDate as nvarchar) + '''


		)
		 w1 on c2.CampaignID=w1.CampaignID

		where ' + @CAMPAIGN_WHERE + '
		group by c2.CampaignID, o2.Opportunityid, w1.[Weight]

		) A
		group by opportunityid 

	) A2 on A1.opportunityid=a2.opportunityid

	' + @FINAL_JOIN + '
	group by a1.CampaignID, c1.CampaignName
	order by allocation desc'


	--print @Final_Query

	exec(@Final_Query)



END
GO
--insertation end kausha #2258

--End Integration Script for GamePlan




-- Updated by Akashdeep Kadia 
-- Updated on :: 15-June-2016
-- Desc :: Special charachters replace with Hyphen in Export to CSV. 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
Create PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@ClientId nvarchar(max)=''
,@HoneyCombids nvarchar(max)=null
AS
BEGIN

SET NOCOUNT ON;
--Update CustomField set Name =REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost',null AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',[Tactic].Cost As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',[lineitem].Cost As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy Uniqueidentifier,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
		END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType <> 'Budget'
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType <> 'Budget' 
AND EntityType <> 'Lineitem'
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End

END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublishModel]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[PublishModel]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Created by Nishant Sheth
-- Created on :: 06-Jun-2016
-- Desc :: Update Plan model id and tactic's tactic type id with new publish version of model 
CREATE PROCEDURE [dbo].[PublishModel]
@NewModelId int = 0 
,@UserId uniqueidentifier=''
AS
SET NOCOUNT ON;

BEGIN
IF OBJECT_ID(N'tempdb..#tblModelids') IS NOT NULL
BEGIN
  DROP TABLE #tblModelids
END

-- Get all parents model of new model
;WITH tblParent  AS
(
    SELECT ModelId,ParentModelId
        FROM [Model] WHERE ModelId = @NewModelId
    UNION ALL
    SELECT [Model].ModelId,[Model].ParentModelId FROM [Model]  JOIN tblParent  ON [Model].ModelId = tblParent.ParentModelId
)
SELECT ModelId into #tblModelids
    FROM tblParent 
	OPTION(MAXRECURSION 0)

-- Update Tactic Type for Default saved views
DECLARE  @TacticTypeIds NVARCHAR(MAX)=''
SELECT @TacticTypeIds = FilterValues From Plan_UserSavedViews WHERE Userid=@UserId AND FilterName='TacticType'

DECLARE   @FilterValues NVARCHAR(MAX)
SELECT    @FilterValues = COALESCE(@FilterValues + ',', '') + CAST(TacticTypeId AS NVARCHAR) FROM TacticType 
WHERE PreviousTacticTypeId IN(SELECT val FROM dbo.comma_split(@TacticTypeIds,','))
AND ModelId=@NewModelId

IF @FilterValues <>'' 
BEGIN
	UPDATE Plan_UserSavedViews SET FilterValues=@FilterValues WHERE Userid=@UserId AND FilterName='TacticType'
END

-- Update Plan's ModelId with new modelid
UPDATE [Plan] SET ModelId=@NewModelId WHERE ModelId IN(SELECT ModelId FROM #tblModelids)

-- Update Tactic's Tactic Type with new model's tactic type
UPDATE Tactic SET Tactic.TacticTypeId=TacticType.TacticTypeId FROM 
Plan_Campaign_Program_Tactic Tactic 
CROSS APPLY(SELECT TacticType.TacticTypeId FROM TacticType WHERE TacticType.PreviousTacticTypeId=Tactic.TacticTypeId)TacticType
CROSS APPLY(SELECT Program.PlanProgramId,Program.PlanCampaignId FROM Plan_Campaign_Program Program WHERE Program.PlanProgramId=Tactic.PlanProgramId) Program
CROSS APPLY(SELECT Camp.PlanCampaignId,Camp.PlanId FROM Plan_Campaign Camp WHERE Camp.PlanCampaignId=Program.PlanCampaignId 
AND Camp.PlanId IN(SELECT PlanId FROM [Plan] WHERE ModelId IN(SELECT ModelId FROM #tblModelids)))Camp
WHERE Tactic.IsDeleted=0
AND Tactic.TacticTypeId IS NOT NULL

END
GO

-- Modified By Nishant Sheth
-- Date :: 16-Jun-2016
-- Desc :: To resolve the linked tactic id pass as null with store marketo url
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
	@strCreatedTacIds nvarchar(max),
	@strUpdatedTacIds nvarchar(max),
	@strUpdateComment nvarchar(max),
	@strCreateComment nvarchar(max),
	@isAutoSync bit='0',
	@userId uniqueidentifier,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'

	Declare @strAllPlanTacIds nvarchar(max)=''
	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@strCreateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@strUpdateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
	END
    
END


GO


-- ========================================================================================================

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'June.2016'
set @version = 'June.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO




-- Added by Komal Rawal
-- Added on :: 08-June-2016
-- Desc :: On creation of new item in Marketing budget give permission as per parent items.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveuserBudgetPermission]
GO
/****** Object:  StoredProcedure [dbo].[SaveuserBudgetPermission]    Script Date: 06/09/2016 15:30:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveuserBudgetPermission] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy uniqueidentifier 
AS
BEGIN
WITH CTE AS
(
 
    SELECT ParentId 
    FROM Budget_Detail
    WHERE id= @BudgetDetailId
    UNION ALL
    --This is called multiple times until the condition is met
    SELECT g.ParentId 
    FROM CTE c, Budget_Detail g
    WHERE g.id= c.parentid

)

Select * into ##tempbudgetdata from (select ParentId ,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as RN from CTE) as result
option (maxrecursion 0)

select * from ##tempbudgetdata where ParentId is not null

IF OBJECT_ID('tempdb..##AllUniqueBudgetdata') IS NOT NULL
Drop Table ##AllUniqueBudgetdata

--Get user data of all the parents
SELECT * INTO ##AllUniqueBudgetdata FROM (
select Distinct BudgetDetailId, UserId,@BudgetDetailId as bid,GETDATE() as dt,@CreatedBy as Cby,Case WHEN UserId = @CreatedBy
THEN 
@PermissionCode
 ELSE
PermisssionCode END as percode,
Case WHEN UserId = @CreatedBy
THEN 
 1
 ELSE
 0 END as usrid
from Budget_Permission where BudgetDetailId in (select ParentId from ##tempbudgetdata)) as data


-- Insert unique data of parents for new item,take users from upper level parent if user not present in immediate parent
insert into Budget_Permission select Uniquedata.UserId,Uniquedata.bid,GETDATE(),Uniquedata.Cby,Uniquedata.percode,Uniquedata.usrid from ##AllUniqueBudgetdata as Uniquedata
JOIN ##tempbudgetdata as TempBudgetTable on Uniquedata.BudgetDetailId = TempBudgetTable.ParentId and UserId NOT IN 
(
select UserId 
FROM ##AllUniqueBudgetdata as Alldata
JOIN ##tempbudgetdata as parent on Alldata.BudgetDetailId = parent.ParentId and RN < TempBudgetTable.RN 
)
UNION
select  @CreatedBy,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode,1 from Budget_Permission 

IF OBJECT_ID('tempdb..##tempbudgetdata') IS NOT NULL
Drop Table ##tempbudgetdata

IF OBJECT_ID('tempdb..##AllUniqueBudgetdata') IS NOT NULL
Drop Table ##AllUniqueBudgetdata

END

GO


IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Report_Intergration_Conf'))
BEGIN
CREATE TABLE [dbo].[Report_Intergration_Conf](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[IdentifierColumn] [nvarchar](255) NOT NULL,
	[IdentifierValue] [nvarchar](1000) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MeasureApiConfiguration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_TableName_IdentifierColumn_IdentifierValue] UNIQUE NONCLUSTERED 
(
	[TableName] ASC,
	[IdentifierColumn] ASC,
	[IdentifierValue] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboarContentData]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[GetDashboarContentData]
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboardContentData]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[GetDashboardContentData]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetDashboardContentData]
	-- Add the parameters for the stored procedure here
	@UserId varchar(max),
	@DashboardID int = 0
AS
BEGIN
	IF (@UserId IS NOT NULL AND @UserId != '')
	BEGIN
		IF (ISNULL(@DashboardID, 0) > 0)
		BEGIN
			IF EXISTS (SELECT D.id
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID)
			BEGIN
				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted,IsComparisonDisplay=ISNULL(D.IsComparisonDisplay,0), 
					ISNULL(HelpTextId,0) AS HelpTextId 
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID
                    ORDER BY D.DisplayOrder


				SELECT dc.id, dc.DisplayName, dc.DashboardId, dc.DisplayOrder, dc.ReportTableId, dc.ReportGraphId, dc.Height, dc.Width, dc.Position, dc.IsCumulativeData, dc.IsCommunicativeData, dc.DashboardPageID,					dc.IsDeleted, dc.DisplayIfZero, dc.KeyDataId, dc.HelpTextId
					FROM DashboardContents AS dc 
					INNER JOIN Dashboard AS D ON D.id = dc.DashboardId AND D.IsDeleted = 0 AND D.ParentDashboardId IS NULL
					INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE dc.IsDeleted = 0 AND dc.DashboardId = @DashboardID
					ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'User Not Authorize to Access Dashboard'
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT D.id
				FROM Dashboard D
                INNER JOIN User_Permission UP ON d.id = UP.DashboardId  AND UP.UserId = @UserId
				WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0)
			BEGIN

				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, D.[Rows], D.[Columns], D.ParentDashboardId, D.IsDeleted, D.IsComparisonDisplay, D.HelpTextId
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0
                    ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'No Dashboard Configured For User'
			END
		END
	END
	ELSE
	BEGIN
		SELECT 'Please Provide Proper UserId'
	END
END

GO

--- START: PL ticket #2251 related SPs & Functions --------------------
-- Created By: Viral 
-- Created On: 06/10/2016
-- Description: PL ticket #2251: Prepared Data before push to SFDC through Integration Web API.

/****** Object:  StoredProcedure [dbo].[spGetSalesforceMarketo3WayData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceMarketo3WayData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceMarketo3WayData]
GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticActualCostMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''101371'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementCampaign'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16404'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementProgram'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16402'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementTactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''7864'',2,1203,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		--Declare @trgtSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		--Declare @trgtSourceParentId varchar(50)=''''
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		--Declare @trgtMode varchar(255)=''''
		Declare @actObjType varchar(255)=''ObjectType''
		--Declare @trgtObjType varchar(255)=''''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		Declare @entImprvTactic varchar(255)=''ImprovementTactic''
		Declare @entImprvProgram varchar(255)=''ImprovementProgram''
		Declare @entImprvCampaign varchar(255)=''ImprovementCampaign''
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables

		 -- START:- Improvement Variable declaration
		 --Cost Field
		Declare @imprvCost varchar(20)=''ImprvCost''
		Declare @actImprvCost varchar(20)=''Cost''

		 -- Static Status
		 Declare @imprvPlannedStatus varchar(50)=''Planned''
		 Declare @tblImprvTactic varchar(200)=''Plan_Improvement_Campaign_Program_Tactic''
		 Declare @tblImprvProgram varchar(200)=''Plan_Improvement_Campaign_Program''
		 Declare @tblImprvCampaign varchar(200)=''Plan_Improvement_Campaign''

		 -- Imprv. Tactic table Actual Fields
		 Declare @actEffectiveDate varchar(50)=''EffectiveDate''
		 -- END: Improvement Variable declaration
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

	-- IF EntityType is ''Improvement Campaign, Program or Tactic'' then add respective entity related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				CASE 
					WHEN ((gpDataType.TableName=@tblImprvTactic) AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''1'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@entityType) and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
BEGIN
	-- Insert table name ''Global'' and IsImprovement flag ''1'' in case of Improvement entities
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
END
ELSE
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Campaigns
		SELECT 
			IC.ImprovementPlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign IC 
		WHERE @entityType=@entImprvCampaign and ImprovementPlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Programs
		SELECT 
			IP.ImprovementPlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program IP 
		WHERE @entityType=@entImprvProgram and ImprovementPlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Tactics
		SELECT 
			IT.ImprovementPlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program_Tactic IT 
		WHERE @entityType=@entImprvTactic and ImprovementPlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																		ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Improvement Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(Imprvcmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Imprvcmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Imprvcmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Imprvcmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvCampaign
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = T.SourceId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvCampaign)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvPrg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvPrg.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvPrg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvPrg.ImprovementPlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvPrg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvProgram
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvProgram)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvTac.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvTac.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actImprvCost THEN ISNull(Cast(ImprvTac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actEffectiveDate THEN ISNull(CONVERT(VARCHAR(19),ImprvTac.EffectiveDate),'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvTac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvTac.ImprovementPlanProgramId as nvarchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvTac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvTactic
		LEFT JOIN Plan_Improvement_Campaign_Program_Tactic as ImprvTac ON ImprvTac.ImprovementPlanTacticId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvTactic)
	)
) as result;



Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''


RETURN
END
' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData_Marketo3Way](''Program'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''32029'',2,1203,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		Declare @actObjType varchar(255)=''ObjectType''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		Declare @actsfdcParentId varchar(50)=''ParentId''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables
		
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actsfdcParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																		ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(Tac.IntegrationInstanceTacticId,'''')<>'''') THEN prg.IntegrationInstanceProgramId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(prg.IntegrationInstanceProgramId,'''')<>'''') THEN cmpgn.IntegrationInstanceCampaignId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
) as result;

Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''

RETURN
END
' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- SElect [GetSFDCTacticResultColumns] (1203,''464EB808-AD1F-4481-9365-6AADA15023BD'',2)
CREATE FUNCTION [dbo].[GetSFDCTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN
Declare @imprvCost varchar(20)=''ImprvCost''
Declare @actImprvCost varchar(20)=''Cost''
declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

	;With ResultTable as(

(
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				-- Rename actualfield ''Cost'' to ''ImprvCost''  in case of Table Name ''Plan_Improvement_Campaign_Program_Tactic'' to ignore conflict of same name ''Cost'' actual field of both Tactic & Improvement Tactic table
				CASE 
					WHEN  ((gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'') AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost 
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Plan_Campaign_Program'' OR gpDataType.TableName=''Plan_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		SELECT  mapp.IntegrationInstanceId,
				0 as GameplanDataTypeId,
				Null as TableName,
				custm.Name as ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
				
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and (custm.EntityType=''Tactic'' or custm.EntityType=''Campaign'' or custm.EntityType=''Program'')
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select Distinct ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END

' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTacticActualCostMappingData]
(
	@entIds varchar(max)=''''
)
RETURNS @tac_actualcost_mappingtbl Table(
PlanTacticId int,
ActualCost varchar(50)
)

AS
BEGIN
	Declare @costStage varchar(20)=''Cost''

	-- Get Tactic & Tactic Actual Cost Mapping data 
	-- If Tactic has lineitems then Sum up of LineItem Actual''s value else Tactic Actual''s value.

	INSERT INTO @tac_actualcost_mappingtbl
	SELECT tac.PlanTacticId,
	   	   CASE 
			WHEN COUNT(distinct line.PlanLineItemId) >0 THEN  Cast(IsNULL(SUM(lActl.Value),0) as varchar(50)) ELSE  Cast(IsNULL(SUM(tActl.Actualvalue),0) as varchar(50))
		   END as ActualCost
	FROM Plan_Campaign_Program_Tactic as tac
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem as line on tac.PlanTacticId = line.PlanTacticId and line.IsDeleted=''0''
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual as lActl on line.PlanLineItemId = lActl.PlanLineItemId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual as tActl on tac.PlanTacticId = tActl.PlanTacticId and  tActl.StageTitle=@costStage
	WHERE tac.PlanTacticId IN (select val from comma_split(@entIds,'',''))
	GROUP BY tac.PlanTacticId
	RETURN 
END
' 
END

GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
		Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
		Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Store Improvement Entities related data
	Declare @tblImprvEntity table (
									ImprovementPlanTacticId int,
									ImprovementPlanProgramId int,
									ImprovementPlanCampaignId int,
									ObjectType varchar(50)
								  )

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Campaign,Program,Tactic,Improvement Tactic or Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- Get Improvement TacticIds
							BEGIN
								Insert into @tblImprvEntity
								Select Imprvtact.ImprovementPlanTacticId,
									   Imprvtact.ImprovementPlanProgramId,
									   Imprvcampgn.ImprovementPlanCampaignId,
									   @entityTypeImprvTac as ObjectType
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Improvement_Campaign as Imprvcampgn ON Imprvcampgn.ImprovePlanId = pln.PlanId 
								join Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
								join Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
								where mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							END

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			BEGIN TRY

				-- Pick latest year tactic in case of linked Tactic and push to SFDC.
				IF EXISTS(SELECT LinkedTacticId from Plan_Campaign_Program_Tactic where PlanTacticId=@id)
				BEGIN
					
					DECLARE @tac_lnkdIds varchar(20)=''
					SELECT @tac_lnkdIds=cast(PlanTacticId as varchar)+','+Cast(ISNULL(LinkedTacticId,0) as varchar) 
					FROM Plan_Campaign_Program_Tactic where PlanTacticId=@id
					;WITH tbl as(
								SELECT tact.PlanTacticId,tact.LinkedTacticId,tact.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tact
								WHERE PlanTacticId IN (select val from comma_split(@tac_lnkdIds,',')) and tact.IsDeleted=0
								UNION ALL
								SELECT tac.PlanTacticId,tac.LinkedTacticId,tac.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tac 
								INNER JOIN tbl as lnk on tac.LinkedTacticId=lnk.PlanTacticId
								WHERE tac.PlanTacticId=@id
								)
					-- Set latest year tactic to @id variable
					SELECT TOP 1 @id=LinkedTacticId 
					FROM tbl
					INNER JOIN [Plan] as pln on tbl.LinkedPlanId = pln.PlanId and pln.IsDeleted=0
					ORDER BY [Year] DESC
				END
			
				INSERT INTO @tblTaclist 
				SELECT tact.PlanTacticId,
						tact.PlanProgramId,
						prg.PlanCampaignId,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						@tact as ObjectType,
						1 as RN
				FROM Plan_Campaign_Program_Tactic as tact 
				INNER JOIN Plan_Campaign_Program as prg on tact.PlanProgramId = prg.PlanProgramId and prg.IsDeleted='0'
				WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
				INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0
				INNER JOIN @tblTaclist as tac ON prg.PlanProgramId = tac.PlanProgramId
			END TRY
			BEGIN CATCH
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
			END CATCH

			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Tactic Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeProg))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Program Id')

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- START: Get list of Campaigns not pushed into SFDC.
							BEGIN
								Insert into @tblTaclist 
								select Null as PlanTacticId,
									   Null as PlanProgramId,
									   cmpgn.PlanCampaignId,
									   Null as LinkedTacticId ,
									   Null as LinkedPlanId,
									   tac.PlanYear,
									   @cmpgn as ObjectType,
									   RN= 1
								from @tblTaclist as tac
								INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
								INNER JOIN Plan_Campaign_Program as P on tac.PlanProgramId=P.PlanProgramId and (IsNull(P.IntegrationInstanceProgramId,'')='')
								where tac.ObjectType=@prg
							END
							-- END: Get list of Campaigns not pushed into SFDC.
							

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
					INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0 and prg.PlanProgramId=@id
					
					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Program Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeCampgn))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Campaign Id')
					
					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln 
								INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
								INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								INNER JOIN Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where pln.IsDeleted=0
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0 and cmpgn.PlanCampaignId=@id

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Campaign Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeImprvTac))
		BEGIN
			-- Get Improvement TacticIds
			BEGIN
				Insert into @tblImprvEntity
				Select Imprvtact.ImprovementPlanTacticId,
					   Imprvtact.ImprovementPlanProgramId,
					   Imprvcampgn.ImprovementPlanCampaignId,
					   @entityTypeImprvTac as ObjectType
				from Plan_Improvement_Campaign as Imprvcampgn
				INNER JOIN Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
				INNER JOIN Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.ImprovementPlanTacticId=@id and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Improvement_Campaign] as Imprvcmpgn ON pln.PlanId = Imprvcmpgn.ImprovePlanId
				INNER JOIN [Plan_Improvement_Campaign_Program] as ImprvPrg ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId 
				INNER JOIN @tblImprvEntity as ImprvTac ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
			END
		END
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: Add list of Improvement Programs not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Imprvprg.ImprovementPlanProgramId,
						   Imprvprg.ImprovementPlanCampaignId,
							@entityTypeImprvProg as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign_Program as Imprvprg on Imprvtac.ImprovementPlanProgramId = Imprvprg.ImprovementPlanProgramId and IsNull(Imprvprg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Improvement Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Null as ImprovementPlanProgramId,
						   ImprvCmpgn.ImprovementPlanCampaignId,
							@entityTypeImprvCamp as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign as ImprvCmpgn on Imprvtac.ImprovementPlanCampaignId = ImprvCmpgn.ImprovementPlanCampaignId and IsNull(ImprvCmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist)) OR (EXISTS(Select 1 from @tblImprvEntity))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids

					-- Get Improvement Ids
					BEGIN
						-- START: Get ImprvmntTacticIds
						SELECT @entImrvmntTacIds = ISNULL(@entImrvmntTacIds  + ',','') + (ImprvTac)
						FROM (Select DISTINCT Cast (ImprovementPlanTacticId as varchar(max)) ImprvTac FROM @tblImprvEntity where ObjectType=@entityTypeImprvTac) AS PlanTacticIds
						-- END: Get ImprvmntTacticIds

						-- START: Get ImprvmntCampaign Ids
						SELECT @entImrvmntCmpgnIds = ISNULL(@entImrvmntCmpgnIds  + ',','') + (ImprvCampgn)
						FROM (Select DISTINCT Cast (ImprovementPlanCampaignId as varchar(max)) ImprvCampgn FROM @tblImprvEntity where ObjectType=@entityTypeImprvCamp) AS PlanCampaignIds
						-- END: Get ImprvmntCampaign Ids

						-- START: Get ImprvmntProgram Ids
						SELECT @entImrvmntPrgIds= ISNULL(@entImrvmntPrgIds + ',','') + (ImprvPrgrm)
						FROM (Select DISTINCT Cast (ImprovementPlanProgramId as varchar(max)) ImprvPrgrm FROM @tblImprvEntity where ObjectType=@entityTypeImprvProg) AS PlanProgramIds
						-- END: Get ImprvmntProgram Ids
					END
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvCamp +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvProg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvTac +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
					--PRINT @dynResultQuery  
					--Execute the Dynamic Pivot Query
					--EXEC sp_executesql @dynResultQuery
					
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END



GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceMarketo3WayData]    Script Date: 06/21/2016 1:27:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceMarketo3WayData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceMarketo3WayData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceMarketo3WayData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Identify 3 Way integration between Marketo & SFDC - Create hierarchy in SFDC 
					BEGIN
						Declare @isSyncSFDCWithMarketo bit='1'
						--Declare @ModelIds varchar(max)=''
						--SELECT @ModelIds = ISNULL(@ModelIds  + ',','') + (mdlId)
						--FROM (Select DISTINCT Cast (ModelId as varchar(max)) mdlId from Model where ((IsNull(IsDeleted,'0')='0') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id OR IntegrationInstanceId=@id))) AS planIds
					END

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and ( (tact.IsSyncMarketo='1') OR (tact.IsSyncSalesForce='1') ) and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.ModelId IN (
														Select DISTINCT ModelId from Model where ((IsNull(IsDeleted,'0')='0') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id OR IntegrationInstanceId=@id))
													   ) and mdl.[Status]='Published' and mdl.IsActive='1'
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
										
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	
END

GO

--- END: PL ticket #2251 related SPs & Functions --------------------

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc :: Add is owner column to the table.
IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'IsOwner' AND OBJECT_ID = OBJECT_ID(N'[Budget_Permission]'))
BEGIN
ALTER TABLE [dbo].[Budget_Permission]
ADD [IsOwner] [bit] NOT NULL CONSTRAINT [DF_Budget_Permission_IsOwner]  DEFAULT 0
END 
Go

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc ::update isowner flag for existing budget.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Budget_Permission]') AND type in (N'U') )
BEGIN
	Declare @cntOwner int=0
	select @cntOwner = Count(*) FROM  [dbo].[Budget_Permission] where IsOwner = 1
	IF(@cntOwner=0)
	BEGIN
		UPDATE [dbo].[Budget_Permission]
		SET IsOwner = 1 WHERE UserId = CreatedBy 
	END
END 
Go
-- Added by Viral Kadiya
-- Added on :: 17-June-2016
-- Desc :: Insert Campaign,Program,Tactic & Improvement Tactic Comment.

/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
	@strCreatedTacIds nvarchar(max)='',
	@strUpdatedTacIds nvarchar(max)='',
	@strCrtCampaignIds nvarchar(max)='',
	@strUpdCampaignIds nvarchar(max)='',
	@strCrtProgramIds nvarchar(max)='',
	@strUpdProgramIds nvarchar(max)='',
	@strCrtImprvmntTacIds nvarchar(max)='',
	@strUpdImprvmntTacIds nvarchar(max)='',
	@isAutoSync bit='0',
	@userId uniqueidentifier,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'
	Declare @strAllPlanTacIds nvarchar(max)=''

	-- Start:- Declare comment related variables for each required entity.
	Declare @TacticSyncedComment varchar(500) = 'Tactic synced with ' + @integrationType
	Declare @TacticUpdatedComment varchar(500) = 'Tactic updated with ' + @integrationType
	Declare @ProgramSyncedComment varchar(500) = 'Program synced with ' + @integrationType
	Declare @ProgramUpdatedComment varchar(500) = 'Program updated with ' + @integrationType
	Declare @CampaignSyncedComment varchar(500) = 'Campaign synced with ' + @integrationType
	Declare @CampaignUpdatedComment varchar(500) = 'Campaign updated with ' + @integrationType
	Declare @ImprovementTacticSyncedComment varchar(500) = 'Improvement Tactic synced with ' + @integrationType
	Declare @ImprovementTacticUpdatedComment varchar(500) = 'Improvement Tactic updated with ' + @integrationType
	-- End:- Declare comment related variables for each required entity.

	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@TacticSyncedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@TacticUpdatedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Comment for Plan Campaign
		IF( (@strCrtCampaignIds <>'') OR (@strUpdCampaignIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@CampaignSyncedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strCrtCampaignIds, ','))
			UNION
			SELECT null,@CampaignUpdatedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strUpdCampaignIds, ','))
		END

		-- Insert Comment for Plan Program
		IF( (@strCrtProgramIds <>'') OR (@strUpdProgramIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@ProgramSyncedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strCrtProgramIds, ','))
			UNION
			SELECT null,@ProgramUpdatedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strUpdProgramIds, ','))
		END

		-- Insert Comment for Improvement Tactic
		IF( (@strCrtImprvmntTacIds <>'') OR (@strUpdImprvmntTacIds <>'') )
		BEGIN
			INSERT Into Plan_Improvement_Campaign_Program_Tactic_Comment
			SELECT ImprovementPlanTacticId,@ImprovementTacticSyncedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCrtImprvmntTacIds, ','))
			UNION
			SELECT ImprovementPlanTacticId,@ImprovementTacticUpdatedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdImprvmntTacIds, ','))
		END

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
	END
    
END

GO

--===========================================================================================================================================
/* Start - Added by Arpita Soni for Ticket #2279 on 06/21/2016 */

-- Add column PlanId into IntegrationWorkFrontPortfolios table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'PlanId' AND [object_id] = OBJECT_ID(N'IntegrationWorkFrontPortfolios'))
BEGIN
	ALTER TABLE dbo.IntegrationWorkFrontPortfolios ADD PlanId INT NULL
END
GO

-- Alter column PlanProgramId into IntegrationWorkFrontPortfolios table
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'PlanProgramId' AND [object_id] = OBJECT_ID(N'IntegrationWorkFrontPortfolios'))
BEGIN
	ALTER TABLE dbo.IntegrationWorkFrontPortfolios ALTER COLUMN PlanProgramId INT NULL
END
GO

-- Add column IntegrationWorkFrontProgramID into Plan_Campaign table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'IntegrationWorkFrontProgramID' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign ADD IntegrationWorkFrontProgramID NVARCHAR(50) NULL
END
GO

-- Create table IntegrationWorkFrontPortfolio_Mapping
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationWorkFrontProgram_Mapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PortfolioTableId] [int] NOT NULL,
	[ProgramId] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontProgram_Mapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

-- Create FK on PortfolioTableId column in table IntegrationWorkFrontPortfolio_Mapping  
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Program_PortfolioTableId]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]'))
ALTER TABLE [dbo].[IntegrationWorkFrontProgram_Mapping]  WITH CHECK ADD  CONSTRAINT FK_Program_PortfolioTableId FOREIGN KEY([PortfolioTableId])
REFERENCES [dbo].[IntegrationWorkFrontPortfolios] ([Id])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Program_PortfolioTableId]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]'))
ALTER TABLE [dbo].[IntegrationWorkFrontProgram_Mapping] CHECK CONSTRAINT FK_Program_PortfolioTableId
GO

/* End - Added by Arpita Soni for Ticket #2279 on 06/21/2016 */
--===========================================================================================================================================

/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/22/2016 4:08:28 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSFDCFieldMappings]
GO
/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/22/2016 4:08:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetSFDCFieldMappings] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetSFDCFieldMappings] 
	@clientId uniqueidentifier,
	@integrationTypeId int,
	@id int=0,
	@isSFDCMarketoIntegration bit='0'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Exec GetSFDCFieldMappings '464EB808-AD1F-4481-9365-6AADA15023BD',2,1203

BEGIN
	Declare @Table TABLE (sourceFieldName NVARCHAR(250),destinationFieldName NVARCHAR(250),fieldType varchar(255))
	Declare @ColumnName nvarchar(max)
	Declare @trgtCampaignFolder varchar(255)='Id'
	Declare @actMode varchar(255)='Mode'
	Declare @trgtMode varchar(255)=''
	Declare @modeCREATE varchar(20)='Create'
	Declare @modeUPDATE varchar(20)='Update'
	Declare @actCost varchar(30)='Cost'
	Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'
	Declare @tblProgram varchar(255)='Plan_Campaign_Program'
	Declare @tblCampaign varchar(255)='Plan_Campaign'
	Declare @tblImprvTactic varchar(255)='Plan_Improvement_Campaign_Program_Tactic'
	Declare @varGlobal varchar(100)='Global'
	Declare @varGlobalImprovemnt varchar(100)='GlobalImprovement'
	Declare @actActivityType varchar(255)='ActivityType'
	Declare @actCreatedBy varchar(255)='CreatedBy'
	Declare @actPlanName varchar(255)='PlanName'
	Declare @actStatus varchar(255)='Status'
	Declare @entityTypeTac varchar(255)='Tactic'
	Declare @entityTypeProg varchar(255)='Program'
	Declare @entityTypeCampgn varchar(255)='Campaign'
	Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
	Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
	Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
	Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
	Declare @actsfdcParentId varchar(50)='ParentId'
END

;With ResultTable as(

(
		-- Add Campaign,Program, Tactic, Improvement Campaign,Program & Tactic Or Global fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				CASE 
					WHEN gpDataType.TableName=@tblTactic THEN @entityTypeTac
					WHEN gpDataType.TableName=@tblProgram THEN @entityTypeProg
					WHEN gpDataType.TableName=@tblCampaign THEN @entityTypeCampgn
					WHEN gpDataType.TableName=@tblImprvTactic THEN @entityTypeImprvTac
					ELSE @varGlobal
				END as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@tblTactic OR gpDataType.TableName=@tblProgram OR gpDataType.TableName=@tblCampaign OR gpDataType.TableName=@tblImprvTactic OR gpDataType.TableName=@varGlobal) and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- Add Improvement Global Fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				 @varGlobalImprovemnt as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@varGlobal and IsImprovement='1' and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- CustomField Query
		SELECT  custm.Name as sourceFieldName,
				TargetDataType as destinationFieldName,
				custm.EntityType as fieldType
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and ((custm.EntityType=@entityTypeTac) OR (custm.EntityType=@entityTypeProg) OR (custm.EntityType=@entityTypeCampgn))
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
IF(@isSFDCMarketoIntegration='1')
BEGIN
	-- Insert ParentId field for Campaign,Program & Tactic
	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobal as fieldType

	-- Insert ParentId field for Improvement Campaign,Program & Tactic
	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobalImprovemnt as fieldType
END
select * from @Table
END

GO

