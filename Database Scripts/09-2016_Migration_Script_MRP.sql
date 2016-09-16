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

/****** Object:  Table [dbo].[User_CoulmnView]    Script Date: 09/14/2016 1:40:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_CoulmnView]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User_CoulmnView](
	[ViewId] [int] IDENTITY(1,1) NOT NULL,
	[ViewName] [nvarchar](50) NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[CreatedDate] [datetime] NULL,
	[ModifyBy] [uniqueidentifier] NULL,
	[ModifyDate] [datetime] NULL,
	[IsDefault] [bit] NULL,
	[GridAttribute] [xml] NULL,
	[BudgetAttribute] [xml] NULL,
 CONSTRAINT [PK_User_CoulmnView] PRIMARY KEY CLUSTERED 
(
	[ViewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
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

Declare @CustomfieldType int
set @CustomfieldType=(select CustomFieldTypeId from CustomFieldType where Name='DropDownList')

SELECT distinct CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,case when  CustomField.EntityType='LineItem' then 'Line Item' else CustomField.EntityType end as EntityType,ISnull(CustomFieldDependency.ParentCustomFieldId,0) as ParentId ,CustomFieldType.Name as CustomFieldType,
case when  CustomField.EntityType='Campaign' then 1
when  CustomField.EntityType='Program' then 2
when  CustomField.EntityType='Tactic' then 3
when  CustomField.EntityType='LineItem' then 4
end as entityorder
FROM CustomField (NOLOCK) 
inner join CustomFieldType on CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId
LEFT join CustomFieldDependency on
CustomField.CustomFieldId= CustomFieldDependency.ChildCustomFieldId
Left join CustomFieldOption on CustomField.CustomFieldId = CustomFieldOption.CustomFieldId 
 WHERE 
CustomField.IsDeleted=0 and (CustomField.CustomFieldTypeId <> @CustomfieldType or (CustomFieldOptionId IS NOT NULL AND CustomField.CustomFieldTypeId=@CustomfieldType ))
and ClientId= CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END and CustomField.EntityType in('Campaign','Program','Tactic','LineItem')
group by CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,CustomField.EntityType,CustomFieldDependency.ParentCustomFieldId,CustomFieldType.Name
order by entityorder

END

--select * from CustomFieldType where CustomFieldId=29
-- EXEC sp_GetCustomFieldList '464eb808-ad1f-4481-9365-6aada15023bd'
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

/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 09/13/2016 2:46:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

	SELECT DISTINCT(A.Name) AS [Text],A.EntityType +'Custom'+ Cast(A.CustomFieldId as nvarchar(50)) as Value  
	FROM CustomField A CROSS APPLY (	SELECT B.CustomFieldTypeId,B.Name 
										FROM CustomFieldType B 
										WHERE A.CustomFieldTypeId = B.CustomFieldTypeId) B CROSS APPLY (SELECT C.CustomFieldID 
																										FROM @tblCustomerFieldIDs C 
																										WHERE C.CustomFieldID = A.CustomFieldId) C 
										
	WHERE A.ClientId=@ClientId AND A.IsDeleted=0 AND A.IsDisplayForFilter=1 AND A.EntityType IN ('Tactic','Campaign','Program') and B.Name='DropDownList' 
	ORDER BY Value DESC 

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
	@PlanTacticId INT,
	@UserId NVARCHAR(36)
)
AS
BEGIN

	SELECT Id
		,ActivityId
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
		,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum
		,0 as LineItemTypeId 
	FROM
	(
	  SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
			,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
			,PT.Title AS ActivityName
			,'tactic' ActivityType
			,NULL ParentActivityId
			,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
			,PT.TacticBudget AS MainBudgeted
			,PT.CreatedBy 
			,0 as IsEditable
			,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
			,PTB.Value
			,PTB.Period
			,PT.Cost
			,'C'+PTCst.Period as CPeriod
			,PTCst.Value as CValue
	FROM 
		Plan_Campaign_Program_Tactic PT
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem PL ON PT.PlanTacticId = PL.PlanTacticId
		LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
		LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
		WHERE PT.IsDeleted=0 and pl.PlanTacticId = @PlanTacticId
	) Tactic_Main
	pivot
	(
	  sum(value)
	  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
 
	) PlanCampaignProgramTacticDetails
	Pivot
	(
	sum(CValue)
	  for CPeriod in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	)PlanCampaignProgramTacticDetails1

	UNION ALL

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
		LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId = PLC.PlanLineItemId
		WHERE PL.PlanTacticId IN (@PlanTacticId) AND PL.IsDeleted=0
	) LineItem_Main
	PIVOT
	(
		SUM (Value)
		FOR Period IN ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
	) PivotLineItem
END

GO
/* End - Added by Arpita Soni for Ticket #2612 on 09/08/2016 */

/* Start- Added by Viral for Ticket #2571 on 09/13/2016 */

/****** Object:  UserDefinedFunction [dbo].[fnGetEntitieHirarchyByPlanId]    Script Date: 09/13/2016 12:55:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'--This function will return all the enties with hirarchy
--Multiple plan ids can be passed saperated by comma
--If we pass null then it will retuen all plans hirarchy data
CREATE FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId] ( @PlanIds NVARCHAR(MAX))
RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		UNIQUEIDENTIFIER
		)
AS
BEGIN

	;WITH FilteredPlan AS(
		SELECT ''Plan'' EntityType,''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId,P.PlanId EntityId, P.Title EntityTitle,NULL ParentEntityId,NULL ParentUniqueId, P.Status, NULL StartDate, NULL EndDate,P.CreatedBy FROM [Plan] P 
			--INNER JOIN Model M ON M.ModelId = P.ModelId AND M.ClientId = @ClientId
		WHERE P.IsDeleted = 0 
			AND (
					@PlanIds IS NULL 
					OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
				)
	),
	Campaigns AS (
		SELECT ''Campaign'' EntityType,''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId,C.PlanCampaignId EntityId, C.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, C.Status, C.StartDate StartDate, C.EndDate EndDate,C.CreatedBy FROM Plan_Campaign C
			INNER JOIN FilteredPlan P ON P.EntityId = C.PlanId 
		WHERE C.IsDeleted = 0 
	),
	Programs AS (
		SELECT ''Program'' EntityType,''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy FROM Plan_Campaign_Program P
			INNER JOIN Campaigns C ON C.EntityId = P.PlanCampaignId
		WHERE P.IsDeleted = 0 
	),
	Tactics AS (
		SELECT ''Tactic'' EntityType,''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Programs P ON P.EntityId = T.PlanProgramId
		WHERE T.IsDeleted = 0 
	),
	LineItems AS (
		SELECT ''LineItem'' EntityType,''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId, NULL Status, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem L
			INNER JOIN Tactics T ON T.EntityId = L.PlanTacticId
		WHERE L.IsDeleted = 0 
	),
	AllEntities AS (    
		SELECT * FROM FilteredPlan UNION ALL
		SELECT * FROM Campaigns UNION ALL
		SELECT * FROM Programs UNION ALL
		SELECT * FROM Tactics UNION ALL
		SELECT * FROM LineItems
	)
	INSERT INTO @Entities (UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,Status,StartDate,EndDate,CreatedBy)
	SELECT E.UniqueId, E.EntityId,E.EntityTitle, E.ParentEntityId,E.ParentUniqueId,E.EntityType, C.ColorCode,E.Status,E.StartDate,E.EndDate,E.CreatedBy FROM AllEntities E
	LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType

	RETURN
END


' 
END

GO

/* End- Added by Viral for Ticket #2571 on 09/13/2016 */

/* Start- Added by Viral for Ticket #2571 on 09/13/2016 */

/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 09/13/2016 12:55:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[fnGetFilterEntityHierarchy]
(
	@planIds varchar(max)='''',
	--@customfields varchar(max)=''71_104,71_105'',
	@ownerIds nvarchar(max)='''',
	@tactictypeIds varchar(max)='''',
	@statusIds varchar(max)=''''
)

--select * from fnGetFilterEntityHierarchy(''20220'','''',''56_null'',''41F64F4B-531E-4CAA-8F5F-328E36D9B202'',''31104'',''Created'')
RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		UNIQUEIDENTIFIER
		)
AS
BEGIN


Declare @entTactic varchar(8)=''Tactic''
Declare @entLineItem varchar(10)=''LineItem''

	-- Fill the table variable with the rows for your result set
	

	;WITH FilteredEnt AS(
Select * from fnGetEntitieHirarchyByPlanId(@planIds)
)
,tac as (
	Select distinct ent.* 
	FROM FilteredEnt as ent
	Join [Plan_Campaign_Program_Tactic] as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType=@entTactic AND tac.[Status] IN (select val from comma_split(@statusIds,'','')) and  tac.[CreatedBy] IN (select case when val = '''' then null else Convert(uniqueidentifier,val) end from comma_split(@ownerIds,'',''))
	Join [TacticType] as typ on tac.TacticTypeId = typ.TacticTypeId and typ.IsDeleted=''0'' and typ.[TacticTypeId] IN (select val from comma_split(@tactictypeIds,'',''))
	where ent.EntityType = @entTactic
)
,line as (
	SELECT ent.* 
	FROM FilteredEnt as ent
	JOIN tac on ent.ParentEntityId = tac.EntityId and ent.EntityType=@entLineItem

)

INSERT INTO @Entities
select * from FilteredEnt where EntityType not in (''Tactic'',''LineItem'')
union all
SELECT * FROM tac 
union all
select * from line

RETURN

END
' 
END

GO


/* End- Added by Viral for Ticket #2571 on 09/13/2016 */

Go
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPlanBudget]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetPlanBudget] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 09/08/2016
-- Description:	This store proc. return data for budget tab for repective plan, campaign, program and tactic
-- =============================================
ALTER PROCEDURE [dbo].[GetPlanBudget]
	(
	@PlanId NVARCHAR(MAX),
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)=''
	)
AS
BEGIN
	
DECLARE @tmp TABLE
(
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		UNIQUEIDENTIFIER
)

INSERT INTO @tmp
SELECT * FROM fnGetFilterEntityHierarchy( @PlanId,@ownerIds,@tactictypeIds,@statusIds)

SELECT ActivityId
			,ActivityType
			,Title
			,ParentActivityId
			,CreatedBy
			,Budget
			,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
			,(ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalAllocationBudget
			,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY1NULL], NULL [CostY11], NULL [CostY12]
			,0 TotalAllocationCost
			,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY1NULL], NULL [ActualY11], NULL [ActualY12]
			,0 TotalAllocationActual
		FROM 
				(SELECT 
					P.PlanId ActivityId
					,P.Title
					,'Plan' as ActivityType
					,H.ParentEntityId ParentActivityId
					,P.CreatedBy
					, Budget
					,PB.Value
					,PB.Period
				FROM @tmp H 
					INNER JOIN [Plan] P ON H.EntityId=P.PlanId 
					LEFT JOIN Plan_Budget PB ON P.PlanId=PB.PlanId
				WHERE H.EntityType='Plan' 
				)Pln
				PIVOT
				(
					sum(value)
					for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
				)PLNMain
UNION ALL
SELECT 
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,CreatedBy
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,(ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalAllocationBudget
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY1NULL], NULL [CostY11], NULL [CostY12]
		,0 TotalAllocationCost
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY1NULL], NULL [ActualY11], NULL [ActualY12]
		,0 TotalAllocationActual
	 FROM
			(SELECT 
				PC.PlanCampaignId ActivityId
				,PC.Title
				,'campaign' as ActivityType
				,H.ParentEntityId ParentActivityId
				,PC.CreatedBy
				,CampaignBudget Budget
				,PCB.Value
				,PCB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign PC  ON H.EntityId=PC.PlanCampaignId 
				LEFT JOIN Plan_Campaign_Budget PCB ON PC.PlanCampaignId=PCB.PlanCampaignId  
			WHERE H.EntityType='Campaign'
			)Campaign
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)CampaignMain
UNION ALL
	SELECT 
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,CreatedBy
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,(ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalAllocationBudget
		,NULL [CostY1], NULL [CostY2], NULL [CostY3], NULL [CostY4],NULL [CostY5], NULL [CostY6], NULL [CostY7], NULL [CostY8],NULL [CostY9], NULL [CostY1NULL], NULL [CostY11], NULL [CostY12]
		,0 TotalAllocationCost
		,NULL [ActualY1], NULL [ActualY2], NULL [ActualY3], NULL [ActualY4],NULL [ActualY5], NULL [ActualY6], NULL [ActualY7], NULL [ActualY8],NULL [ActualY9], NULL [ActualY1NULL], NULL [ActualY11], NULL [ActualY12]
		,0 TotalAllocationActuals
	 FROM
			(SELECT 
				PCP.PlanProgramId ActivityId
				,PCP.Title
				,'program' as ActivityType
				,PCP.PlanCampaignId ParentActivityId
				,PCP.CreatedBy
				,PCP.ProgramBudget Budget
				,PCPB.Value
				,PCPB.Period
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program PCP ON H.EntityId=PCP.PlanProgramId 
				LEFT JOIN Plan_Campaign_Program_Budget PCPB ON PCP.PlanProgramId=PCPB.PlanProgramId
			WHERE H.EntityType='Program'
			)Program
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)ProgramMain
UNION ALL
	SELECT 
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,CreatedBy
		,Budget
		,[Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12]
		,(ISNULL([Y1],0)+ ISNULL([Y2],0)+ISNULL( [Y3],0)+ ISNULL( [Y4],0)+ISNULL( [Y5],0) +ISNULL( [Y6],0) +ISNULL( [Y7],0) +ISNULL( [Y8],0) +ISNULL( [Y9],0) +ISNULL( [Y10],0) +ISNULL( [Y11],0) +ISNULL( [Y12],0)) TotalAllocationBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,(ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0)) TotalAllocationCost
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) TotalAllocationActuals
	 FROM
			(SELECT 
				PCPT.PlanTacticId ActivityId
				,PCPT.Title
				,'tactic' as ActivityType
				,PCPT.PlanProgramId ParentActivityId
				,PCPT.CreatedBy
				,PCPT.TacticBudget Budget
				,PCPTB.Value
				,PCPTB.Period
				,PCPTC.Value as CValue
				,'Cost'+PCPTC.Period as CPeriod
				,PCPTA.Actualvalue as AValue
				,'A'+PCPTA.Period as APeriod
			FROM @tmp H
				INNER JOIN Plan_Campaign_Program_Tactic PCPT ON H.EntityId=PCPT.PlanTacticId 
				LEFT JOIN Plan_Campaign_Program_Tactic_Budget PCPTB ON PCPT.PlanTacticId=PCPTB.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Cost PCPTC ON PCPT.PlanTacticId=PCPTC.PlanTacticId
				LEFT JOIN Plan_Campaign_Program_Tactic_Actual PCPTA ON PCPT.PlanTacticId=PCPTA.PlanTacticId AND PCPTA.StageTitle='Cost'
			WHERE H.EntityType='Tactic'
			)Tactic
			PIVOT
			(
				sum(value)
				for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
			)TacticMain
			PIVOT
			(
				sum(CValue)
				for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12])
			)TacticMain1
			PIVOT
			(
				sum(AValue)
				for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12])
			)TacticMain2

UNION ALL
	SELECT 
		ActivityId
		,ActivityType
		,Title
		,ParentActivityId
		,CreatedBy
		,0 Budget
		,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12]
		,0 TotalAllocationBudget
		,[CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12]
		,(ISNULL([CostY1],0)+ ISNULL([CostY2],0)+ISNULL( [CostY3],0)+ ISNULL( [CostY4],0)+ISNULL( [CostY5],0) +ISNULL( [CostY6],0) +ISNULL( [CostY7],0) +ISNULL( [CostY8],0) +ISNULL( [CostY9],0) +ISNULL( [CostY10],0) +ISNULL( [CostY11],0) +ISNULL( [CostY12],0)) TotalAllocationCost
		,[ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12]
		,(ISNULL([ActualY1],0)+ ISNULL([ActualY2],0)+ISNULL( [ActualY3],0)+ ISNULL( [ActualY4],0)+ISNULL( [ActualY5],0) +ISNULL( [ActualY6],0) +ISNULL( [ActualY7],0) +ISNULL( [ActualY8],0) +ISNULL( [ActualY9],0) +ISNULL( [ActualY10],0) +ISNULL( [ActualY11],0) +ISNULL( [ActualY12],0)) TotalAllocationActuals
	 FROM
		 (SELECT 
					PCPTL.PlanLineItemId ActivityId
					,PCPTL.Title
					,'lineitem' as ActivityType
					,PCPTL.PlanTacticId ParentActivityId
					,PCPTL.CreatedBy
					,PCPTLC.Value as CValue
					,'Cost'+PCPTLC.Period as CPeriod
					,PCPTLA.Value as AValue
					,'A'+PCPTLA.Period as APeriod
				FROM @tmp H
					INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON H.EntityId=PCPTL.PlanLineItemId 
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTLC ON PCPTL.PlanLineItemId=PCPTLC.PlanLineItemId
					LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId=PCPTLA.PlanLineItemId
				WHERE H.EntityType='LineItem'
				)LineItem
				PIVOT
				(
				 sum(CValue)
				  for CPeriod in ([CostY1], [CostY2], [CostY3], [CostY4],[CostY5], [CostY6], [CostY7], [CostY8],[CostY9], [CostY10], [CostY11], [CostY12])
				)LineItemMain
				PIVOT
				(
				 sum(AValue)
				  for APeriod in ([ActualY1], [ActualY2], [ActualY3], [ActualY4],[ActualY5], [ActualY6], [ActualY7], [ActualY8],[ActualY9], [ActualY10], [ActualY11], [ActualY12])
				)LineItemMain
END

GO


-- =============================================
-- Author: Viral Kadiya
-- Create date: 09/16/2016
-- Description:	This store proc. return Goal Header section data.
-- =============================================

/****** Object:  StoredProcedure [dbo].[spGetGoalValuesForPlan]    Script Date: 09/16/2016 12:26:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetGoalValuesForPlan]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetGoalValuesForPlan] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetGoalValuesForPlan]
	@PlanIds VARCHAR(max),
	@ClientId UNIQUEIDENTIFIER
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--EXEC spGetGoalValuesForPlan '20220','464eb808-ad1f-4481-9365-6aada15023bd'

	SET NOCOUNT ON;

    DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@INQValue FLOAT = 0,
			@MQLValue FLOAT = 0,
			@CWValue FLOAT = 0,
			@RevenueValue FLOAT = 0,
			@INQTotal FLOAT = 0,
			@MQLTotal FLOAT = 0,
			@CWTotal FLOAT = 0,
			@RevenueTotal FLOAT = 0,
			@PlanId INT,
			@ModelId INT,
			@PlanStageCode VARCHAR(20),
			@inqTitle varchar(255)='',
			@mqlTitle varchar(255)='',
			@cwTitle varchar(255)='',
			@revenue varchar(20)='Revenue'
	
	Declare @plans TABLE(
		PlanId INT,
		ModelId INT,
		PlanStageCode varchar(10)
	)


	INSERT INTO @plans 
	SELECT P.PlanId,P.ModelId,P.GoalType
	FROM [dbo].[comma_split](@PlanIds,',') as V
	INNER JOIN [Plan] as P on Cast(V.val as INT) = P.PlanId

	BEGIN
		DECLARE planGoal CURSOR FOR  
		SELECT PlanId,ModelId,PlanStageCode 
		FROM @plans 

		OPEN planGoal   
		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode  
		
		WHILE @@FETCH_STATUS = 0   
		BEGIN   

		SELECT @INQValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@INQCode,@ClientId)
		SELECT @MQLValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@MQLCode,@ClientId)
		SELECT @CWValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@CWCode,@ClientId)
		SELECT @RevenueValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@RevenueCode,@ClientId)
		
		SET @INQTotal = @INQTotal + @INQValue
		SET @MQLTotal = @MQLTotal + @MQLValue
		SET @CWTotal = @CWTotal + @CWValue
		SET @RevenueTotal = @RevenueTotal + @RevenueValue

		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode 
		END   
		
		CLOSE planGoal   
		DEALLOCATE planGoal
	END
		
	Select @inqTitle = Title from Stage where ClientId=@ClientId and Code=@INQCode and IsDeleted='0'
	Select @mqlTitle = Title from Stage where ClientId=@ClientId and Code=@MQLCode and IsDeleted='0'
	Select @cwTitle = Title from Stage where ClientId=@ClientId and Code=@CWCode and IsDeleted='0'


	SELECT @inqTitle as Title, @INQTotal as Value, @INQCode as StageCode
	UNION ALL
	SELECT @mqlTitle as Title, @MQLTotal as Value, @MQLCode as StageCode
	UNION ALL
	SELECT @cwTitle as Title, @CWTotal as Value, @CWCode as StageCode
	UNION ALL
	SELECT @revenue as Title, @RevenueTotal as Value, @revenue as StageCode
	
END

GO

--Index Add to improve performance for Getbudget store proc.
IF NOT EXISTS (  SELECT top 1 * 
FROM sys.indexes 
WHERE name='IX_Plan_Campaign_Program_Tactic_Budget_1' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_Budget'))
BEGIN
CREATE NONCLUSTERED INDEX [IX_Plan_Campaign_Program_Tactic_Budget_1] ON [dbo].[Plan_Campaign_Program_Tactic_Budget]
(
	[PlanTacticId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
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
