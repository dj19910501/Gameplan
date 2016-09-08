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
/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 09/08/2016 12:46:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spViewByDropDownList] AS' 
END
GO

-- =============================================
-- Author:		<Author,,JZhang>
-- Create date: <Create Date,05-July-2016,>
-- Description:	<This is a rewrite of the orginal proc for performance reason. Using in memory table reduces time from 900 ms to 40 ms on average>
-- =============================================
ALTER PROCEDURE  [dbo].[spViewByDropDownList] 
	-- Add the parameters for the stored procedure here
	@PlanId NVARCHAR(max),
	@ClientId NVARCHAR(50),
	@UserId NVARCHAR(50)=null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tblCustomerFieldIDs TABLE ( EntityType NVARCHAR(120), EntityID INT, CustomFieldID INT) 

	INSERT INTO @tblCustomerFieldIDs SELECT 'Campaign', A.PlanCampaignId, B.CustomFieldId 
	FROM Plan_Campaign A CROSS APPLY (  SELECT B.CustomFieldId 
										FROM CustomField_Entity B 
										WHERE B.EntityId = A.PlanCampaignId AND A.IsDeleted=0 AND A.PlanId in ( SELECT val FROM dbo.comma_split(@Planid, ','))) B

	INSERT INTO @tblCustomerFieldIDs SELECT 'Program', A.PlanProgramId, B.CustomFieldId 
	FROM Plan_Campaign_Program A CROSS APPLY (	SELECT B.CustomFieldId  
												FROM CustomField_Entity B 
												WHERE B.EntityId = A.PlanProgramId AND A.IsDeleted=0 AND A.PlanCampaignId in(SELECT EntityId FROM @tblCustomerFieldIDs WHERE EntityType = 'Campaign')) B 

	INSERT INTO @tblCustomerFieldIDs SELECT 'Tactic', A.PlanTacticId, B.CustomFieldId 
	FROM Plan_Campaign_Program_Tactic A CROSS APPLY (	SELECT B.CustomFieldId 
														FROM CustomField_Entity B 
														WHERE B.EntityId = A.PlanTacticId AND A.IsDeleted=0 AND A.PlanProgramId in(SELECT EntityId FROM @tblCustomerFieldIDs WHERE EntityType = 'Program')) B

	SELECT DISTINCT(A.Name) AS [Text],A.EntityType +'Custom'+ Cast(A.CustomFieldId as nvarchar(50)) as Value  ,'id1' OrderKey
	FROM CustomField A CROSS APPLY (	SELECT B.CustomFieldTypeId,B.Name 
										FROM CustomFieldType B 
										WHERE A.CustomFieldTypeId = B.CustomFieldTypeId) B CROSS APPLY (SELECT C.CustomFieldID 
																										FROM @tblCustomerFieldIDs C 
																										WHERE C.CustomFieldID = A.CustomFieldId) C 
										
	WHERE A.ClientId=@ClientId AND A.IsDeleted=0 AND A.IsDisplayForFilter=1 AND A.EntityType IN ('Tactic','Campaign','Program') and B.Name='DropDownList' 
	--ORDER BY Value DESC 
	union
	select ViewName AS [Text],'ColumnCustom'+Cast(ViewId as nvarchar(50)) as Value,'id2' OrderKey
	from User_CoulmnView where CreatedBy=@UserId
	ORDER BY OrderKey,Value DESC 

 END


GO

/* Start - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

-- DROP AND CREATE STORED PROCEDURE dbo.LineItem_Cost_Allocation
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'LineItem_Cost_Allocation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.LineItem_Cost_Allocation
END
GO

CREATE PROCEDURE [dbo].[LineItem_Cost_Allocation]
( 
	@LineItemId INT,
	@UserId NVARCHAR(36)
)
AS
BEGIN
	-- Calculate line item planned cost allocated by monthly/quarterly
	SELECT Id,ActivityId
	,ActivityName
	,ActivityType
	,ParentActivityId
	,MainBudgeted
	,IsOwner
	,CreatedBy
	,IsEditable
	,Cost
	,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
	,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
	,0 TotalCostSum 
	,LineItemTypeId
	FROM
	(
		SELECT 
			CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
			,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
			,PL.Title as ActivityName
			,'lineitem' ActivityType
			,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
			,PL.Cost
			,0 MainBudgeted
			,CASE WHEN PL.CreatedBy = @UserId THEN 1 ELSE 0 END IsOwner
			,PL.CreatedBy
			,0 as IsEditable
			,PLC.Value
			,'C'+PLC.period as period 
			,PL.LineItemTypeId as LineItemTypeId
		FROM Plan_Campaign_Program_Tactic_LineItem PL
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
		WHERE PL.PlanLineItemId IN (@LineItemId) AND PL.IsDeleted=0
	) LineItem_Main
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END
GO
/* End - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */


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
