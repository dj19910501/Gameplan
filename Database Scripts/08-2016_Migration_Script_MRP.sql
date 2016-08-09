/* Start - Added by Arpita Soni on 08/09/2016 for Ticket #2464 - Custom Alerts and Notifications */

-- Create table Alert_Rules
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alert_Rules]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Alert_Rules](
		[RuleId] [int] IDENTITY(1,1) NOT NULL,
		[RuleSummary] [nvarchar](max) NOT NULL,
		[EntityId] [int] NOT NULL,
		[EntityType] [nvarchar](50) NOT NULL,
		[Indicator] [nvarchar](50) NOT NULL,
		[IndicatorComparision] [nvarchar](10) NOT NULL,
		[IndicatorGoal] [int] NOT NULL,
		[CompletionGoal] [int] NOT NULL,
		[Frequency] [nvarchar](50) NOT NULL,
		[DayOfWeek] [tinyint] NULL,
		[DateOfMonth] [tinyint] NULL,
		[LastProcessingDate] [datetime] NULL,
		[NextProcessingDate] [datetime] NULL,
		[UserId] [uniqueidentifier] NOT NULL,
		[ClientId] [uniqueidentifier] NOT NULL,
		[IsDisabled] [bit] NOT NULL DEFAULT 0,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_Alert_Rules] PRIMARY KEY CLUSTERED 
	(
		[RuleId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- Create table Alerts
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alerts]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Alerts](
		[AlertId] [int] IDENTITY(1,1) NOT NULL,
		[RuleId] [int] NOT NULL,
		[Description] [nvarchar](max) NOT NULL,
		[IsRead] [bit] NOT NULL DEFAULT 0,
		[UserId] [uniqueidentifier] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
	 CONSTRAINT [PK_Alerts] PRIMARY KEY CLUSTERED 
	(
		[AlertId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- Create FK FK_Alerts_Alert_Rules in table Alerts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Alerts_Alert_Rules]') AND parent_object_id = OBJECT_ID(N'[dbo].[Alerts]'))
ALTER TABLE [dbo].[Alerts]  WITH CHECK ADD  CONSTRAINT [FK_Alerts_Alert_Rules] FOREIGN KEY([RuleId])
REFERENCES [dbo].[Alert_Rules] ([RuleId])
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Alerts_Alert_Rules]') AND parent_object_id = OBJECT_ID(N'[dbo].[Alerts]'))
ALTER TABLE [dbo].[Alerts] CHECK CONSTRAINT [FK_Alerts_Alert_Rules]
GO

-- Create table User_Notification_Messages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_Notification_Messages]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[User_Notification_Messages](
		[NotificationId] [int] IDENTITY(1,1) NOT NULL,
		[ComponentName] [nvarchar](50) NOT NULL,
		[ComponentId] [int] NULL,
		[EntityId] [int] NULL,
		[Description] [nvarchar](250) NULL,
		[IsRead] [bit] NOT NULL DEFAULT 0,
		[UserId] [uniqueidentifier] NULL,
		[RecipientId] [uniqueidentifier] NOT NULL,
		[TimeStamp] [datetime] NOT NULL,
		[ClientID] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_User_Notification_Detail] PRIMARY KEY CLUSTERED 
	(
		[NotificationId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

-- Alter column ObjectId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ObjectId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN ObjectId INT NULL
END
GO

-- Alter column ComponentId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ComponentId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN ComponentId INT NULL
END
GO

-- Alter column UserId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'UserId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN UserId UNIQUEIDENTIFIER NULL
END
GO

/* End - Added by Arpita Soni on 08/09/2016 for Ticket #2464 - Custom Alerts and Notifications */