GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_Save_AlertRule]
GO
Go
/****** Object:  StoredProcedure [dbo].[SP_Save_AlertRule]    Script Date: 08/20/2016 4:31:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SP_Save_AlertRule] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 20-08-2016
-- Description:	method to save Alert rule 
-- =============================================
ALTER PROCEDURE [dbo].[SP_Save_AlertRule]

	@ClientId NVARCHAR(255)  ,
	@RuleId int,
	@RuleSummary nvarchar(max),
	@EntityId int,
	@EntityType nvarchar(100),
	@Indicator nvarchar(50),
	@IndicatorComparision nvarchar(10),
	@IndicatorGoal int,
	@CompletionGoal int,
	@Frequency nvarchar(50),
	@DayOfWeek tinyint=null,
	@DateOfMonth tinyint=null,
	@UserId NVARCHAR(255),
	@CreatedBy NVARCHAR(255),
	@ModifiedBy  NVARCHAR(255),
	@IsExists int Output

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @UniqueRule nvarchar(max)
	Declare @FrequencyValue nvarchar(100)=null
	if(@DayOfWeek is not null and @DateOfMonth is null)
		set @FrequencyValue=@DayOfWeek
	else if(@DayOfWeek is null and @DateOfMonth is not null)
		set @FrequencyValue=@DateOfMonth

	set @UniqueRule=CONVERT(nvarchar(15),@EntityId)+'_'+CONVERT(nvarchar(15),@Indicator)+'_'+CONVERT(nvarchar(15),@IndicatorComparision)+'_'+CONVERT(nvarchar(15),@IndicatorGoal)+'_'+CONVERT(nvarchar(15),@CompletionGoal)+'_'+CONVERT(nvarchar(15),@Frequency)
	if(@FrequencyValue is not null)
		set @UniqueRule=@UniqueRule+'_'+@FrequencyValue
	
	If(@RuleId!=0)
	Begin --Update existing rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and  RuleId!=@RuleId and UniqueRuleCode=@UniqueRule and UserId = @UserId)
		Begin
			Update Alert_Rules set EntityId=@EntityId,EntityType=@EntityType,Indicator=@Indicator,IndicatorComparision=@IndicatorComparision,IndicatorGoal=@IndicatorGoal,
			CompletionGoal=@CompletionGoal,Frequency=@Frequency,DateOfMonth=@DateOfMonth,DayOfWeek=@DayOfWeek,ModifiedBy=@ModifiedBy,ModifiedDate=GETDATE(),
			RuleSummary=@RuleSummary,LastProcessingDate=GETDATE(),UniqueRuleCode=@UniqueRule
			where RuleId=@RuleId
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	Else
	Begin -- Isert new alert rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and UniqueRuleCode=@UniqueRule and UserId = @UserId)
		Begin
			Insert into Alert_Rules (RuleSummary,EntityId,EntityType,Indicator,IndicatorComparision,IndicatorGoal,CompletionGoal,Frequency,DayOfWeek,DateOfMonth,LastProcessingDate,
				UserId,ClientId,IsDisabled,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,UniqueRuleCode)
			values(@RuleSummary,@EntityId,@EntityType,@Indicator,@IndicatorComparision,@IndicatorGoal,@CompletionGoal,@Frequency,@DayOfWeek,@DateOfMonth,GETDATE(),
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,null,null,@UniqueRule)
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	
END

--exec SP_Save_AlertRule '464eb808-ad1f-4481-9365-6aada15023bd',25,'<h4>Test Plan eCopy_Test_cases  Closed Won are greater than 50% of Goal</h4><span>Start at 50% completion</span><span>Repeat Weekly</span>',295,'Plan','CW','GT',50,50,'Weekly',4,null,'14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66',0

GO

/****** Object:  Table [dbo].[User_CoulmnView]    Script Date: 09/08/2016 12:46:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_CoulmnView]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User_CoulmnView](
	[ViewId] [int] IDENTITY(1,1) NOT NULL,
	[ViewName] [nvarchar](500) NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[CreatedDate] [datetime] NULL,
	[ModifyBy] [uniqueidentifier] NULL,
	[ModifyDate] [datetime] NULL,
	[IsDefault] [bit] NULL,
 CONSTRAINT [PK_User_CoulmnView] PRIMARY KEY CLUSTERED 
(
	[ViewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[User_CoulmnView_attribute]    Script Date: 09/08/2016 12:46:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_CoulmnView_attribute]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User_CoulmnView_attribute](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ViewId] [int] NOT NULL,
	[AttributeType] [nvarchar](50) NULL,
	[AttributeId] [nvarchar](50) NULL,
	[ColumnOrder] [int] NULL,
 CONSTRAINT [PK_User_CoulmnView_attribute] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_User_CoulmnView_attribute_User_CoulmnView]') AND parent_object_id = OBJECT_ID(N'[dbo].[User_CoulmnView_attribute]'))
ALTER TABLE [dbo].[User_CoulmnView_attribute]  WITH CHECK ADD  CONSTRAINT [FK_User_CoulmnView_attribute_User_CoulmnView] FOREIGN KEY([ViewId])
REFERENCES [dbo].[User_CoulmnView] ([ViewId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_User_CoulmnView_attribute_User_CoulmnView]') AND parent_object_id = OBJECT_ID(N'[dbo].[User_CoulmnView_attribute]'))
ALTER TABLE [dbo].[User_CoulmnView_attribute] CHECK CONSTRAINT [FK_User_CoulmnView_attribute_User_CoulmnView]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetCustomFieldList]    Script Date: 09/08/2016 12:46:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetCustomFieldList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[sp_GetCustomFieldList] AS' 
END
GO

ALTER PROCEDURE [dbo].[sp_GetCustomFieldList]
@ClientId nvarchar(250)=null
AS
BEGIN
SET NOCOUNT ON;

SELECT distinct CustomField.*,ISnull(CustomFieldDependency.ParentCustomFieldId,0) as ParentId 
FROM CustomField (NOLOCK) 
LEFT join CustomFieldDependency on
CustomField.CustomFieldId= CustomFieldDependency.ChildCustomFieldId WHERE 
CustomField.IsDeleted=0
AND ClientId= CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END
order by CustomField.Name

END



GO

-- ===========================Please put your script above this script=============================
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
set @release = 'September.2016'
set @version = 'September.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
