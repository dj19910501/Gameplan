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

	@ClientId INT  ,
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
	@UserId INT,
	@CreatedBy INT,
	@ModifiedBy  INT,
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
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,GETDATE(),@CreatedBy,@UniqueRule)
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
	[CreatedBy] INT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifyBy] INT NULL,
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
@ClientId INT 
AS
BEGIN
SET NOCOUNT ON;

Declare @CustomfieldType int
set @CustomfieldType=(select TOP 1 CustomFieldTypeId from CustomFieldType where Name='DropDownList')

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
			CreatedBy		INT
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
			CreatedBy		INT
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
	Join [Plan_Campaign_Program_Tactic] as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType=@entTactic AND tac.[Status] IN (select val from comma_split(@statusIds,'','')) and  tac.[CreatedBy] IN (select val from comma_split(@ownerIds,'',''))
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
ALTER PROCEDURE [dbo].[GetPlanBudget]--[GetPlanBudget] '20212,20203,19569'
	(
	@PlanId NVARCHAR(MAX),
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)='',
	@UserID INT = 0
	)
AS
BEGIN
	
DECLARE @tmp TABLE
(
			EntityId		BIGINT,
			ParentEntityId	BIGINT,
			EntityType NVARCHAR(50)
)

INSERT INTO @tmp
--SELECT * FROM fnGetFilterEntityHierarchy( @PlanId,@ownerIds,@tactictypeIds,@statusIds)
SELECT EntityId,ParentEntityId,EntityType FROM fnGetEntitieHirarchyByPlanId(@PlanId)

SELECT ActivityId
			,ActivityType
			,Title
			,ParentActivityId
			,CreatedBy
			,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
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
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
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
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
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
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
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
		,CASE WHEN CONVERT(VARCHAR(50),CreatedBy)=@UserID THEN 1 ELSE 0 END IsOwner
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
	@ClientId INT
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


-- Add by Nishant Sheth
-- Below Scripting for grid view data
--Function fnGetEntitieHirarchyByPlanId
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
--This function will return all the enties with hirarchy
--Multiple plan ids can be passed saperated by comma
--If we pass null then it will retuen all plans hirarchy data
ALTER FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId] ( @PlanIds NVARCHAR(MAX))
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
			CreatedBy		UNIQUEIDENTIFIER,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT
		)
AS
BEGIN

	;WITH FilteredPlan AS(
		SELECT ''Plan'' EntityType,''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId,P.PlanId EntityId, P.Title EntityTitle,NULL ParentEntityId,NULL ParentUniqueId, P.Status, NULL StartDate, NULL EndDate,P.CreatedBy 
		,CAST(P.PlanId AS NVARCHAR(50)) AS AltId
		,''L''+CAST(P.PlanId AS NVARCHAR(50)) AS TaskId
		,NULL AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM [Plan] P 
			--INNER JOIN Model M ON M.ModelId = P.ModelId AND M.ClientId = @ClientId
		WHERE P.IsDeleted = 0 
			AND (
					@PlanIds IS NULL 
					OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
				)
	),
	Campaigns AS (
		SELECT ''Campaign'' EntityType,''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId,C.PlanCampaignId EntityId, C.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, C.Status, C.StartDate StartDate, C.EndDate EndDate,C.CreatedBy 
		,CAST(P.AltId AS NVARCHAR(500))+''_''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_C''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS TaskId
		,''L''+CAST(C.PlanId  AS NVARCHAR(500)) AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign C
			INNER JOIN FilteredPlan P ON P.EntityId = C.PlanId 
		WHERE C.IsDeleted = 0 
	),
	Programs AS (
		SELECT ''Program'' EntityType,''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy 
		,CAST(C.AltId AS NVARCHAR(500))+''_''+CAST(P.PlanProgramId AS NVARCHAR(50)) As AltId
		,CAST(C.TaskId AS NVARCHAR(500))+''_P''+CAST(P.PlanProgramId AS NVARCHAR(50)) As TaskId
		,CAST(C.ParentTaskId AS NVARCHAR(500))+''_C''+CAST(P.PlanCampaignId AS NVARCHAR(50)) As ParentTaskId
		,C.PlanId
		,C.ModelId
		FROM Plan_Campaign_Program P
			INNER JOIN Campaigns C ON C.EntityId = P.PlanCampaignId
		WHERE P.IsDeleted = 0 
	),
	Tactics AS (
		SELECT ''Tactic'' EntityType,''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy 
		,CAST(P.AltId AS NVARCHAR(500))+''_''+CAST(T.PlanTacticId AS NVARCHAR(50)) As AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_T''+CAST(T.PlanTacticId AS NVARCHAR(50)) As TaskId
		,CAST(P.ParentTaskId AS NVARCHAR(500))+''_P''+CAST(T.PlanProgramId AS NVARCHAR(50)) As ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Programs P ON P.EntityId = T.PlanProgramId
		WHERE T.IsDeleted = 0 
	),
	LineItems AS (
		SELECT ''LineItem'' EntityType,''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId, NULL Status, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy 
		,CAST(T.AltId AS NVARCHAR(500))+''_''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As AltId
		,CAST(T.TaskId AS NVARCHAR(500))+''_X''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As TaskId
		,CAST(T.ParentTaskId AS NVARCHAR(500))+''_T''+CAST(L.PlanTacticId AS NVARCHAR(50)) As ParentTaskId
		,T.PlanId
		,T.ModelId
		FROM Plan_Campaign_Program_Tactic_LineItem L
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
	INSERT INTO @Entities (UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,Status,StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)
	SELECT E.UniqueId, E.EntityId,E.EntityTitle, E.ParentEntityId,E.ParentUniqueId,E.EntityType, C.ColorCode,E.Status,E.StartDate,E.EndDate,E.CreatedBy,E.AltId,E.TaskId,E.ParentTaskId,E.PlanId,E.ModelId FROM AllEntities E
	LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType

	RETURN
END'
END
ELSE 
BEGIN
execute dbo.sp_executesql @statement = N'
--This function will return all the enties with hirarchy
--Multiple plan ids can be passed saperated by comma
--If we pass null then it will retuen all plans hirarchy data
ALTER FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId] ( @PlanIds NVARCHAR(MAX))
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
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT
		)
AS
BEGIN

	;WITH FilteredPlan AS(
		SELECT ''Plan'' EntityType,''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId,P.PlanId EntityId, P.Title EntityTitle,NULL ParentEntityId,NULL ParentUniqueId, P.Status, NULL StartDate, NULL EndDate,P.CreatedBy 
		,CAST(P.PlanId AS NVARCHAR(50)) AS AltId
		,''L''+CAST(P.PlanId AS NVARCHAR(50)) AS TaskId
		,NULL AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM [Plan] P 
			--INNER JOIN Model M ON M.ModelId = P.ModelId AND M.ClientId = @ClientId
		WHERE P.IsDeleted = 0 
			AND (
					@PlanIds IS NULL 
					OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
				)
	),
	Campaigns AS (
		SELECT ''Campaign'' EntityType,''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId,C.PlanCampaignId EntityId, C.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, C.Status, C.StartDate StartDate, C.EndDate EndDate,C.CreatedBy 
		,CAST(P.AltId AS NVARCHAR(500))+''_''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_C''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS TaskId
		,''L''+CAST(C.PlanId  AS NVARCHAR(500)) AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign C
			INNER JOIN FilteredPlan P ON P.EntityId = C.PlanId 
		WHERE C.IsDeleted = 0 
	),
	Programs AS (
		SELECT ''Program'' EntityType,''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy 
		,CAST(C.AltId AS NVARCHAR(500))+''_''+CAST(P.PlanProgramId AS NVARCHAR(50)) As AltId
		,CAST(C.TaskId AS NVARCHAR(500))+''_P''+CAST(P.PlanProgramId AS NVARCHAR(50)) As TaskId
		,CAST(C.ParentTaskId AS NVARCHAR(500))+''_C''+CAST(P.PlanCampaignId AS NVARCHAR(50)) As ParentTaskId
		,C.PlanId
		,C.ModelId
		FROM Plan_Campaign_Program P
			INNER JOIN Campaigns C ON C.EntityId = P.PlanCampaignId
		WHERE P.IsDeleted = 0 
	),
	Tactics AS (
		SELECT ''Tactic'' EntityType,''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy 
		,CAST(P.AltId AS NVARCHAR(500))+''_''+CAST(T.PlanTacticId AS NVARCHAR(50)) As AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_T''+CAST(T.PlanTacticId AS NVARCHAR(50)) As TaskId
		,CAST(P.ParentTaskId AS NVARCHAR(500))+''_P''+CAST(T.PlanProgramId AS NVARCHAR(50)) As ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Programs P ON P.EntityId = T.PlanProgramId
		WHERE T.IsDeleted = 0 
	),
	LineItems AS (
		SELECT ''LineItem'' EntityType,''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId, NULL Status, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy 
		,CAST(T.AltId AS NVARCHAR(500))+''_''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As AltId
		,CAST(T.TaskId AS NVARCHAR(500))+''_X''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As TaskId
		,CAST(T.ParentTaskId AS NVARCHAR(500))+''_T''+CAST(L.PlanTacticId AS NVARCHAR(50)) As ParentTaskId
		,T.PlanId
		,T.ModelId
		FROM Plan_Campaign_Program_Tactic_LineItem L
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
	INSERT INTO @Entities (UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,Status,StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)
	SELECT E.UniqueId, E.EntityId,E.EntityTitle, E.ParentEntityId,E.ParentUniqueId,E.EntityType, C.ColorCode,E.Status,E.StartDate,E.EndDate,E.CreatedBy,E.AltId,E.TaskId,E.ParentTaskId,E.PlanId,E.ModelId FROM AllEntities E
	LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType

	RETURN
END'
END

--Function fnGetMqlByEntityTypeAndEntityId
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetMqlByEntityTypeAndEntityId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[fnGetMqlByEntityTypeAndEntityId](
	 @EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value bigint
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	DECLARE @value INT = 0 
	IF (@EntityType=''Tactic'')
	BEGIN
		SELECT @AggregateValue *= (Ms.Value/100) FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE S.[Level] >= @StageMinLevel AND S.[Level] < @StageMaxLevel 
							AND S.ClientId=@ClientId
							AND S.StageId = MS.StageId) S
				WHERE Ms.ModelId=@ModelId
						AND StageType=''CR''
		SET @value = (@ProjectedStageValue * @AggregateValue)
	END

	INSERT INTO @RevenueTbl VALUES(@value)
	RETURN
END
'
END
ELSE 
BEGIN
	execute dbo.sp_executesql @statement = N'ALTER FUNCTION [dbo].[fnGetMqlByEntityTypeAndEntityId](
	 @EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value bigint
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	DECLARE @value INT = 0 
	IF (@EntityType=''Tactic'')
	BEGIN
		SELECT @AggregateValue *= (Ms.Value/100) FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE S.[Level] >= @StageMinLevel AND S.[Level] < @StageMaxLevel 
							AND S.ClientId=@ClientId
							AND S.StageId = MS.StageId) S
				WHERE Ms.ModelId=@ModelId
						AND StageType=''CR''
		SET @value = (@ProjectedStageValue * @AggregateValue)
	END

	INSERT INTO @RevenueTbl VALUES(@value)
	RETURN
END
'
END
GO

--Function fnGetRevueneByEntityTypeAndEntityId
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetRevueneByEntityTypeAndEntityId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[fnGetRevueneByEntityTypeAndEntityId](
	@EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	 ,@ADS decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value decimal(38,2)
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	--DECLARE @ADS float = 1
	DECLARE @value decimal = 0

	IF (@EntityType=''Tactic'')
	BEGIN

		SELECT @AggregateValue *= (Ms.Value/100)
		 FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE 
							(S.[Level] IS NULL AND S.ClientId=@ClientId)
							OR (S.[Level] >= @StageMinLevel AND S.[Level] <= @StageMaxLevel 
							AND S.ClientId=@ClientId)
							AND S.IsDeleted=0
							) S
			WHERE Ms.ModelId=@ModelId
				AND S.StageId=MS.StageId
						AND (StageType=''CR'' OR StageType=''Size'')

		SET @value = ((@ProjectedStageValue * @AggregateValue)* @ADS)
		INSERT INTO @RevenueTbl VALUES(@value)
	END
	
	RETURN 
END
'
END
ELSE 
BEGIN
	execute dbo.sp_executesql @statement = N'ALTER FUNCTION [dbo].[fnGetRevueneByEntityTypeAndEntityId](
	@EntityType NVARCHAR(100)=''''
	 ,@ClientId INT = 0
	 ,@StageMinLevel INT = 0
	 ,@StageMaxLevel INT = 0
	 ,@ModelId INT = 0
	 ,@ProjectedStageValue decimal=0
	 ,@ADS decimal=0
	)
RETURNS @RevenueTbl TABLE(
	Value decimal(38,2)
)
AS
BEGIN
	DECLARE @AggregateValue float = 1
	DECLARE @value decimal = 0

	IF (@EntityType=''Tactic'')
	BEGIN

		SELECT @AggregateValue *= (Ms.Value/100)
		 FROM Model_Stage MS WITH (NOLOCK)
			CROSS APPLY (SELECT S.StageId FROM Stage S WITH (NOLOCK) WHERE 
							(S.[Level] IS NULL AND S.ClientId=@ClientId)
							OR (S.[Level] >= @StageMinLevel AND S.[Level] <= @StageMaxLevel 
							AND S.ClientId=@ClientId)
							AND S.IsDeleted=0
							) S
			WHERE Ms.ModelId=@ModelId
				AND S.StageId=MS.StageId
						AND (StageType=''CR'' OR StageType=''Size'')

		SET @value = ((@ProjectedStageValue * @AggregateValue)* @ADS)
		INSERT INTO @RevenueTbl VALUES(@value)
	END
	
	RETURN 
END
'
END
GO

-- View Plan_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Plan_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Plan_PlannedCost]
END
GO

 CREATE VIEW [dbo].[Plan_PlannedCost] WITH SCHEMABINDING
	AS 
   SELECT [dbo].[Plan_Campaign].[PlanId], 
   SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
   FROM  [dbo].[Plan_Campaign], [dbo].[Plan_Campaign_Program],[dbo].[Plan_Campaign_Program_Tactic]
		WHERE [dbo].[Plan_Campaign].[IsDeleted] = 0 
		 AND [dbo].[Plan_Campaign_Program].[IsDeleted]  = 0
			AND  [dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  				
			  AND  [dbo].[Plan_Campaign].[PlanCampaignId] = [dbo].[Plan_Campaign_Program].[PlanCampaignId] 
			    AND  [dbo].[Plan_Campaign_Program].[PlanProgramId] = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId] 
			      --AND [Plan_Campaign].PlanId=19569
			 GROUP BY  [dbo].[Plan_Campaign].[PlanId]  
GO

-- View Campaign_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Campaign_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Campaign_PlannedCost]
END
GO

CREATE VIEW [dbo].[Campaign_PlannedCost] WITH SCHEMABINDING
	AS 
SELECT  [dbo].[Plan_Campaign_Program].[PlanCampaignId] as [PlanCampaignId], 
	SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
	FROM  [dbo].[Plan_Campaign_Program_Tactic],  
	[dbo].[Plan_Campaign_Program]   
	WHERE  
	[dbo].[Plan_Campaign_Program].[IsDeleted] = 0  AND  
	[dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  AND  
	[dbo].[Plan_Campaign_Program].[PlanProgramId]  = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId]
	GROUP BY  [dbo].[Plan_Campaign_Program].[PlanCampaignId]  

GO

-- View Program_PlannedCost
IF EXISTS(select * FROM sys.views where name = 'Program_PlannedCost')
BEGIN
	DROP VIEW [dbo].[Program_PlannedCost]
END
GO

   CREATE VIEW [dbo].[Program_PlannedCost] WITH SCHEMABINDING
	AS 
   SELECT [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId], 
   SUM([dbo].[Plan_Campaign_Program_Tactic].[Cost]) AS PlannedCost
   FROM [dbo].[Plan_Campaign_Program],[dbo].[Plan_Campaign_Program_Tactic]
		WHERE [dbo].[Plan_Campaign_Program].[IsDeleted]  = 0
			AND  [dbo].[Plan_Campaign_Program_Tactic].[IsDeleted] = 0  				
			 AND   [dbo].[Plan_Campaign_Program].[PlanProgramId] = [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId] 
			 GROUP BY  [dbo].[Plan_Campaign_Program_Tactic].[PlanProgramId]  
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GridCustomFieldData]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GridCustomFieldData] AS' 
END
GO

-- =============================================
-- Author:		Nishant Sheth
-- Create date: 16-Sep-2016
-- Description:	Get home grid customfields and it's values
-- =============================================
ALTER PROCEDURE [dbo].[GridCustomFieldData]
	@PlanId	 NVARCHAR(MAX)=''
	,@ClientId INT = 0
AS
BEGIN

SET NOCOUNT ON;

	SELECT CustomFieldId
			,Name AS 'CustomFieldName' 
			,CustomFieldTypeId 
			,IsRequired
			,EntityType
			,AbbreviationForMulti
			FROM CustomField 
				WHERE ClientId=@ClientId
					AND IsDeleted=0
					AND EntityType NOT IN('Budget','MediaCode')

	SELECT Hireachy.EntityId,Hireachy.EntityType,C.CustomFieldId,C.CustomFieldEntityId,C.Value
		 FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
			CROSS APPLY (SELECT C.CustomFieldId
								,C.EntityType
								,CE.CustomFieldEntityId
								,Ce.Value
							 FROM CustomField C
							 CROSS APPLY(SELECT CE.CustomFieldEntityId
												,CE.CustomFieldId
												,CE.Value FROM CustomField_Entity CE
											WHERE C.CustomFieldId = CE.CustomFieldId
													AND Hireachy.EntityId = CE.EntityId
										)CE
								WHERE C.ClientId=@ClientId
									AND C.IsDeleted=0
									AND C.EntityType NOT IN('Budget','MediaCode')
									AND C.EntityType = Hireachy.EntityType) C

END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridData] AS' 
END
GO

-- =============================================
-- Author:		Nishant Sheth
-- Create date: 09-Sep-2016
-- Description:	Get home grid data with custom field 19910781.11
-- =============================================
ALTER PROCEDURE GetGridData
	-- Add the parameters for the stored procedure here
	@PlanId NVARCHAR(MAX) = ''
	,@ClientId INT = 0
	,@OwnerIds NVARCHAR(MAX) = ''
	,@TacticTypeIds varchar(max)=''
	,@StatusIds varchar(max)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	DECLARE @StageMqlMaxLevel INT = 1
	DECLARE @StageRevenueMaxLevel INT = 1

	SELECT @StageMqlMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='MQL'

	SELECT @StageRevenueMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='CW'

	SELECT Hireachy.*,
				TacticType.AssetType,
				TacticType.Title AS TacticType,
				Tactic.TacticTypeId,
				LineItem.LineItemTypeId,
				LineItem.LineItemType,
				CASE WHEN EntityType = 'Tactic'
						THEN Tactic.Cost 
							WHEN EntityType = 'LineItem' 
								THEN LineItem.Cost
							WHEN EntityType='Program'
								THEN ProgramPlannedCost.PlannedCost
							WHEN EntityType='Campaign'
								THEN CampaignPlannedCost.PlannedCost
							WHEN EntityType='Plan'
								THEN PlanPlannedCost.PlannedCost
					END AS PlannedCost
				,Tactic.ProjectedStageValue 
				,Stage.Title AS 'ProjectedStage'
				,MQL.Value as MQL
				,Revenue.Value as Revenue
				,Tactic.TacticCustomName AS 'MachineName'
				,Tactic.LinkedPlanId
				,Tactic.LinkedTacticId
				,P.PlanName AS 'LinkedPlanName'
				,ROI.AnchorTacticID
				,(SELECT SUBSTRING((	
						SELECT ',' + CAST(PlanTacticId AS VARCHAR) FROM ROI_PackageDetail R
						WHERE ROI.AnchorTacticID = R.AnchorTacticID
						FOR XML PATH('')), 2,900000
					))AS PackageTacticIds
				FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
	OUTER APPLY (SELECT M.AverageDealSize FROM Model M WITH (NOLOCK)
					WHERE M.IsDeleted = 0
							AND Hireachy.ModelId = M.ModelId
							) M
				--FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds) Hireachy 
	OUTER APPLY (SELECT Tactic.PlanTacticId,
						Tactic.TacticTypeId,
						Tactic.Cost,
						Tactic.StageId,
						Tactic.ProjectedStageValue,
						Tactic.PlanProgramId,
						Tactic.LinkedPlanId,
						Tactic.LinkedTacticId,
						Tactic.TacticCustomName
						FROM Plan_Campaign_Program_Tactic Tactic WITH (NOLOCK)
							WHERE Hireachy.EntityType='Tactic'
						AND Hireachy.EntityId = Tactic.PlanTacticId) Tactic
	OUTER APPLY (SELECT ROI.PlanTacticId
						,ROI.AnchorTacticID FROM ROI_PackageDetail ROI
						WHERE Tactic.PlanTacticId = ROI.PlanTacticId) ROI
	OUTER APPLY (SELECT Title AS 'PlanName' FROM [Plan] P WITH (NOLOCK)
					WHERE Tactic.LinkedPlanId = P.PlanId) P
	OUTER APPLY(SELECT TacticType.TacticTypeId,
						TacticType.AssetType,
						TacticType.Title  
						FROM TacticType WITH (NOLOCK)
						WHERE Tactic.TacticTypeId = TacticType.TacticTypeId) TacticType
	OUTER APPLY (SELECT LineItem.LineItemTypeId,
						LineItem.PlanLineItemId,
						LineItem.Cost,
						LT.Title AS 'LineItemType'
						FROM Plan_Campaign_Program_Tactic_LineItem LineItem WITH (NOLOCK)
						CROSS APPLY(SELECT LT.LineItemTypeId,LT.Title FROM LineItemType LT
									WHERE LineItem.LineItemTypeId = LT.LineItemTypeId
									AND LT.IsDeleted = 0)LT
						WHERE Hireachy.EntityType = 'LineItem'
						AND Hireachy.EntityId = LineItem.PlanLineItemId) LineItem
	OUTER APPLY (SELECT Stage.Title,Stage.StageId,Stage.[Level] FROM Stage WITH (NOLOCK) WHERE Tactic.StageId = Stage.StageId AND Stage.IsDeleted=0) Stage
	OUTER APPLY (SELECT Value FROM dbo.fnGetMqlByEntityTypeAndEntityId(Hireachy.EntityType,@ClientId,Stage.[Level],@StageMqlMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue) MQL
					WHERE Hireachy.EntityType='Tactic') AS MQL
	OUTER APPLY (SELECT Value FROM dbo.fnGetRevueneByEntityTypeAndEntityId(Hireachy.EntityType,@ClientId,Stage.[Level],@StageRevenueMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue,M.AverageDealSize) Revenue
					WHERE Hireachy.EntityType='Tactic') AS Revenue
	OUTER APPLY (SELECT PlanPlannedCost.PlanId
						,PlanPlannedCost.PlannedCost FROM Plan_PlannedCost PlanPlannedCost WHERE 
						Hireachy.EntityType='Plan'
							AND Hireachy.EntityId=PlanPlannedCost.PlanId)PlanPlannedCost
	OUTER APPLY (SELECT CampaignPlannedCost.PlanCampaignId
							,CampaignPlannedCost.PlannedCost FROM Campaign_PlannedCost CampaignPlannedCost WHERE 
							Hireachy.EntityType='Campaign'
								AND Hireachy.EntityId=CampaignPlannedCost.PlanCampaignId)CampaignPlannedCost
	OUTER APPLY (SELECT ProgramPlannedCost.PlanProgramId
							,ProgramPlannedCost.PlannedCost FROM Program_PlannedCost ProgramPlannedCost WHERE 
							Hireachy.EntityType='Program'
								AND Hireachy.EntityId=ProgramPlannedCost.PlanProgramId)ProgramPlannedCost
	
END
GO

-- Indexes
IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Campaign_Plan' AND object_id = OBJECT_ID('Plan_Campaign'))
BEGIN
	DROP INDEX [IX_Campaign_Plan] ON [dbo].[Plan_Campaign]

	CREATE NONCLUSTERED INDEX [IX_Campaign_Plan] ON [dbo].[Plan_Campaign]
	(
		[PlanId] ASC
	)
	INCLUDE ([PlanCampaignId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign' AND object_id = OBJECT_ID('Plan_Campaign_Program'))
BEGIN
	DROP INDEX [IX_Program_Campaign] ON [dbo].[Plan_Campaign_Program]

	
	CREATE NONCLUSTERED INDEX [IX_Program_Campaign] ON [dbo].[Plan_Campaign_Program]
	(
		[PlanCampaignId] ASC
	)
	INCLUDE ([PlanProgramId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	DROP INDEX [IX_Program_Campaign_Tactic] ON [dbo].[Plan_Campaign_Program_Tactic]

	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[PlanProgramId] ASC
	)
	INCLUDE ([PlanTacticId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_TacticType' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	DROP INDEX [IX_Program_Campaign_Tactic_TacticType] ON [dbo].[Plan_Campaign_Program_Tactic]

	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_TacticType] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[TacticTypeId] ASC
	)
	INCLUDE ([PlanTacticId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_LineItem' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem'))
BEGIN
	DROP INDEX [IX_Program_Campaign_Tactic_LineItem] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]

	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_LineItem] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]
	(
		[PlanTacticId] ASC
	)
	INCLUDE ([PlanLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_Program_Campaign_Tactic_LineItem_LineItemType' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic_LineItem'))
BEGIN
	DROP INDEX [IX_Program_Campaign_Tactic_LineItem_LineItemType] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]

	CREATE NONCLUSTERED INDEX [IX_Program_Campaign_Tactic_LineItem_LineItemType] ON [dbo].[Plan_Campaign_Program_Tactic_LineItem]
	(
		[LineItemTypeId] ASC
	)
	INCLUDE ([PlanLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_CustomField_Entity_EntityId' AND object_id = OBJECT_ID('CustomField_Entity'))
BEGIN
	DROP INDEX [IX_CustomField_Entity_EntityId] ON [dbo].[CustomField_Entity]

	CREATE NONCLUSTERED INDEX [IX_CustomField_Entity_EntityId] ON [dbo].[CustomField_Entity]
	(
		[EntityId] ASC,
		[CustomFieldId] ASC
	)
	INCLUDE ( 	[CustomFieldEntityId],
		[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_32_2101582525__K2_K17_K1' AND object_id = OBJECT_ID('Plan_Campaign'))
BEGIN
	DROP INDEX [_dta_index_Plan_Campaign_32_2101582525__K2_K17_K1] ON [dbo].[Plan_Campaign]

	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_32_2101582525__K2_K17_K1] ON [dbo].[Plan_Campaign]
	(
		[PlanId] ASC,
		[IsDeleted] ASC,
		[PlanCampaignId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_Program_32_2133582639__K2_K17_K1' AND object_id = OBJECT_ID('Plan_Campaign_Program'))
BEGIN
	DROP INDEX [_dta_index_Plan_Campaign_Program_32_2133582639__K2_K17_K1] ON [dbo].[Plan_Campaign_Program]
	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_Program_32_2133582639__K2_K17_K1] ON [dbo].[Plan_Campaign_Program]
	(
		[PlanCampaignId] ASC,
		[IsDeleted] ASC,
		[PlanProgramId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_Program_32_2133582639__K2_K17_K1' AND object_id = OBJECT_ID('Plan_Campaign_Program'))
BEGIN
	DROP INDEX [_dta_index_Plan_Campaign_Program_32_2133582639__K17_K2_K1] ON [dbo].[Plan_Campaign_Program]
	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_Program_32_2133582639__K17_K2_K1] ON [dbo].[Plan_Campaign_Program]
	(
		[IsDeleted] ASC,
		[PlanCampaignId] ASC,
		[PlanProgramId] ASC
	)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Plan_Campaign_Program_Tactic_32_56387270__K15_K2_K1_8' AND object_id = OBJECT_ID('Plan_Campaign_Program_Tactic'))
BEGIN
	DROP INDEX [_dta_index_Plan_Campaign_Program_Tactic_32_56387270__K15_K2_K1_8] ON [dbo].[Plan_Campaign_Program_Tactic]
	CREATE NONCLUSTERED INDEX [_dta_index_Plan_Campaign_Program_Tactic_32_56387270__K15_K2_K1_8] ON [dbo].[Plan_Campaign_Program_Tactic]
	(
		[IsDeleted] ASC,
		[PlanProgramId] ASC,
		[PlanTacticId] ASC
	)
	INCLUDE ( 	[Cost]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='_dta_index_Model_Stage_32_1330103779__K13_K4_K3_K1_5' AND object_id = OBJECT_ID('Model_Stage'))
BEGIN
	DROP INDEX [_dta_index_Model_Stage_32_1330103779__K13_K4_K3_K1_5] ON [dbo].[Model_Stage]
	CREATE NONCLUSTERED INDEX [_dta_index_Model_Stage_32_1330103779__K13_K4_K3_K1_5] ON [dbo].[Model_Stage]
	(
		[ModelId] ASC,
		[StageType] ASC,
		[StageId] ASC,
		[ModelStageId] ASC
	)
	INCLUDE ( 	[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
END
GO

-- STATISTICS
IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2101582525_17_1')
BEGIN
	CREATE STATISTICS [_dta_stat_2101582525_17_1] ON [dbo].[Plan_Campaign]([IsDeleted], [PlanCampaignId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2101582525_1_2_17')
BEGIN	
	CREATE STATISTICS [_dta_stat_2101582525_1_2_17] ON [dbo].[Plan_Campaign]([PlanCampaignId], [PlanId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_2133582639_1_17')
BEGIN	
	CREATE STATISTICS [_dta_stat_2133582639_1_17] ON [dbo].[Plan_Campaign_Program]([PlanProgramId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_56387270_15_1')
BEGIN	
	CREATE STATISTICS [_dta_stat_56387270_15_1] ON [dbo].[Plan_Campaign_Program_Tactic]([IsDeleted], [PlanTacticId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_56387270_1_2_15')
BEGIN	
	CREATE STATISTICS [_dta_stat_56387270_1_2_15] ON [dbo].[Plan_Campaign_Program_Tactic]([PlanTacticId], [PlanProgramId], [IsDeleted])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1330103779_4_1_3_13')
BEGIN
	CREATE STATISTICS [_dta_stat_1330103779_4_1_3_13] ON [dbo].[Model_Stage]([StageType], [ModelStageId], [StageId], [ModelId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.stats WHERE name='_dta_stat_1330103779_13_3')
BEGIN
	CREATE STATISTICS [_dta_stat_1330103779_13_3] ON [dbo].[Model_Stage]([ModelId], [StageId])
END
GO
-- End By Nishant Sheth


/* Start - Added by Dhvani Raval for Ticket #2534*/
-----------------------* Add Columns in Tables *-----------------------
--Add Column in Alert Rule Table
IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'UserEmail' AND Object_ID = Object_ID(N'Alert_Rules'))
BEGIN
  ALTER TABLE Alert_Rules  Add UserEmail NVARCHAR(255)
END
GO

--Add Columns in Alert Table 
IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsEmailSent' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add IsEmailSent NVARCHAR(50)
END
GO

IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'CurrentGoal' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add CurrentGoal FLOAT
END
GO

IF Not EXISTS(SELECT * FROM sys.columns WHERE Name = N'ActualGoal' AND Object_ID = Object_ID(N'Alerts'))
BEGIN
  ALTER TABLE Alerts  Add ActualGoal FLOAT
END
GO

-----------------------*SP RunAlertRules *-----------------------

IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'RunAlertRules') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE dbo.RunAlertRules
END
GO

/****** Object:  StoredProcedure [dbo].[RunAlertRules]    Script Date: 9/17/2016 5:23:06 PM ******/
CREATE PROCEDURE [dbo].[RunAlertRules]
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Constant variables
		DECLARE @txtLessThan	NVARCHAR(20) = 'less than',
				@txtGreaterThan NVARCHAR(20) = 'greater than',
				@txtEqualTo		NVARCHAR(20) = 'equal to'

		DECLARE @TacticsDataForRules			NVARCHAR(MAX) = '',
				@UPDATEQUERYCOMMON				NVARCHAR(MAX) = '',
				@UpdatePlanQuery				NVARCHAR(MAX) = '',
				@UpdateProjectedValuesForPlan	NVARCHAR(MAX) = '',
				@UpdateCampaignQuery			NVARCHAR(MAX) = '',
				@UpdateProgramQuery				NVARCHAR(MAX) = '',
				@UpdateTacticQuery				NVARCHAR(MAX) = '',
				@UpdateLineItemQuery			NVARCHAR(MAX) = '',
				@CalculatePercentGoalQuery		NVARCHAR(MAX) = '',
				@INSERTALERTQUERYCOMMON			NVARCHAR(MAX) = '',
				@InsertQueryForLT				NVARCHAR(MAX) = '',
				@InsertQueryForGT				NVARCHAR(MAX) = '',
				@InsertQueryForEQ				NVARCHAR(MAX) = '',
				@CommonQueryToIgnoreDuplicate	NVARCHAR(MAX) = '',
				@EmailBody	NVARCHAR(MAX) = '',
				@EmailSubject	NVARCHAR(MAX) = ''
				

		-- Get projected and actual values of tactic belongs to plan/campaign/program
		SET @TacticsDataForRules = 'DECLARE @TempEntityTable [TacticForRuleEntities];

									-- Get entities from the rule which have reached the completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],
																 [Frequency],[DayOfWeek],[DateOfMonth],[UserId],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0)
										FROM @TempEntityTable A INNER JOIN  
										#TacticsDataForAllRuleEntities B
										ON A.EntityId = B.EntityId AND A.EntityType = B.EntityType AND A.Indicator = B.Indicator
										'
		-- Update IndicatorTitle based on Indicator Code
		DECLARE @UpdateIndicatorTitle NVARCHAR(MAX) = ' 
														UPDATE A SET A.IndicatorTitle = dbo.GetIndicatorTitle(A.Indicator,B.ClientId,A.EntityType)
														FROM @TempEntityTable A 
														INNER JOIN vClientWise_EntityList B ON A.EntityId = B.EntityId AND A.EntityType = B.Entity
														'

		-- For plan update projected value using different calculation rest of PLANNEDCOST
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												AND A.Indicator != ''PLANNEDCOST''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) != 0) THEN 100 
												 ELSE ISNULL(ActualStageValue,0) * 100 / ProjectedStageValue END ;
										   SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT RuleId, ##DESCRIPTION## AS [Description], UserId,
										(CASE WHEN Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) END
											,GETDATE()) 
										WHEN Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate,  ProjectedStageValue , ActualStageValue
										FROM @TempEntityTable '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal < IndicatorGoal AND IndicatorComparision = ''LT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal > IndicatorGoal AND IndicatorComparision = ''GT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal = IndicatorGoal AND IndicatorComparision = ''EQ'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere

    

		SET @CommonQueryToIgnoreDuplicate = '	MERGE INTO [dbo].Alerts AS T1
												USING
												(##INSERTQUERY##) AS T2
												ON (T2.RuleId = T1.RuleId AND T2.Description = T1.Description AND T2.UserId = T1.UserId)
												WHEN NOT MATCHED THEN  
												INSERT ([RuleId],[Description],[UserId],[DisplayDate],[CurrentGoal],[ActualGoal])
												VALUES ([RuleId],[Description],[UserId],[DisplayDate],[ActualStageValue],[ProjectedStageValue]);'
 
		SET @InsertQueryForLT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForLT)
		SET @InsertQueryForGT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForGT)
		SET @InsertQueryForEQ = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForEQ)
		
		EXEC (@TacticsDataForRules + @UPDATEQUERYCOMMON + @UpdateIndicatorTitle + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ)
		
	END TRY
	BEGIN CATCH
		--Get the details of the error
		 DECLARE   @ErMessage NVARCHAR(2048),
				   @ErSeverity INT,
				   @ErState INT
 
		 SELECT @ErMessage = ERROR_MESSAGE(), @ErSeverity = ERROR_SEVERITY(), @ErState = ERROR_STATE()
 
		 RAISERROR (@ErMessage, @ErSeverity, @ErState)
	END CATCH 
END
GO




-----------------------*SP SP_Save_AlertRule *-----------------------

IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'SP_Save_AlertRule') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE [dbo].[SP_Save_AlertRule]
END
GO

CREATE PROCEDURE [dbo].[SP_Save_AlertRule]

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
	@UserId INT,
	@CreatedBy NVARCHAR(255),
	@ModifiedBy  NVARCHAR(255),
	@UserEmail NVARCHAR(255),
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
				UserId,ClientId,IsDisabled,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,UniqueRuleCode,UserEmail)
			values(@RuleSummary,@EntityId,@EntityType,@Indicator,@IndicatorComparision,@IndicatorGoal,@CompletionGoal,@Frequency,@DayOfWeek,@DateOfMonth,GETDATE(),
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,null,null,@UniqueRule,@UserEmail)
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	
END
GO

-----------------------*View vClientWise_EntityList *-----------------------

IF EXISTS (SELECT 1 FROM sys.views WHERE OBJECT_ID=OBJECT_ID('vClientWise_EntityList'))
BEGIN
	DROP view vClientWise_EntityList
END
GO

CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, 'Plan' Entity,P.CreatedDate,P.Title as PlanTitle, P.PlanId As PlanId  ,1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, 'Campaign' Entity,P.CreatedDate,C.PlanTitle as PlanTitle, c.PlanId As PlanId, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, 'Program' Entity,P.CreatedDate, C.PlanTitle as PlanTitle, c.PlanId As PlanId, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllLinkedTactic as
(
SELECT P.LinkedTacticId 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and P.LinkedTacticId is not null
	   and (DATEPART(year,P.EndDate)-DATEPART(year,P.StartDate))>0
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, 'Tactic' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
			  LEFT OUTER JOIN AllLinkedTactic L on P.PlanTacticId=L.LinkedTacticId
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and L.LinkedTacticId is null
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, 'Line Item' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  5 EntityOrder 
       FROM Plan_Campaign_Program_Tactic_LineItem P
              INNER JOIN AllTactic C ON P.PlanTacticId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.LineItemTypeId is not null
)
SELECT * FROM AllPlans
UNION ALL 
SELECT * FROM AllCampaigns
UNION ALL 
SELECT * FROM AllProgram
UNION ALL 
SELECT * FROM AllTactic
UNION ALL 
SELECT * FROM AllLineitem
GO

-----------------------* Email Configuration *-----------------------
sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'Ole Automation Procedures', 1;
GO
RECONFIGURE;
GO


-----------------------* SP sp_Send_Mail_For_Alerts *-----------------------
IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'sp_Send_Mail_For_Alerts') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE dbo.sp_Send_Mail_For_Alerts
END
GO

CREATE PROCEDURE [dbo].[sp_Send_Mail_For_Alerts]
        @from Nvarchar(500),
		@pwd Nvarchar(30),
		@Url Nvarchar(max)
	
AS
BEGIN

  DECLARE @imsg int
  DECLARE @hr int
  DECLARE @source varchar(255)
  DECLARE @description varchar(500)
  DECLARE @bodytype varchar(10)
  DECLARE @to varchar(500) 
  DECLARE @Comparision varchar(500)
  DECLARE @body varchar(max) 
  DECLARE @subject varchar(max) 
  DECLARE @output_desc varchar(1000)
  DECLARE @Result varchar(1000)
  DECLARE @UrlString varchar(max)
  --DECLARE @IndicatorTitle nvarchar(1000)

  DECLARE @SubjectComparision varchar(500)
  DECLARE @Query Nvarchar(max)
   
  SET @bodytype = 'htmlbody'


	declare @temptable table (test nvarchar(50))
	insert into @temptable EXEC(@query)
 
  
  BEGIN
  DECLARE @AlertId int, @AlertDescription nvarchar(max), @Email  nvarchar(255),@Indicator nvarchar(50), @IndicatorComparision nvarchar(10),@IndicatorGoal int, @EntityTitle nvarchar(500),@DisplayDate DateTime, @ActualGoal float, @CurrentGoal float,@PlanName nvarchar(255),@Entity nvarchar(255),@PlanId int,@EntityId int
  			
  DECLARE Cur_Mail CURSOR FOR	
    SELECT  al.AlertId, al.Description, ar.UserEmail, 
				CASE WHEN ar.Indicator = 'PLANNEDCOST' 
					THEN 'PLANNED COST'
					when ar.Indicator = 'REVENUE' 
					THEN 'REVENUE'
					ELSE Stage.Title END AS
					 Indicator, ar.IndicatorComparision,ar.IndicatorGoal, vw.EntityTitle,al.DisplayDate,al.ActualGoal,al.CurrentGoal, vw.PlanTitle,vw.Entity, vw.PlanId,vw.EntityId
			   FROM  [dbo].Alerts al
			   INNER JOIN dbo.[Alert_Rules] AS ar ON ar.RuleId = al.RuleId  and ar.UserId = al.UserId
			   left JOIN dbo.Stage ON Stage.Code = ar.Indicator  and ar.ClientId = Stage.ClientId
			   left JOIN dbo.vClientWise_EntityList as vw on  vw.EntityId = ar.EntityId 
			   WHERE al.DisplayDate <= GETDATE() and (al.IsEmailSent Is null or  al.IsEmailSent <> 'Success')
   
    
  OPEN Cur_Mail
  
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate,@ActualGoal,@CurrentGoal,@PlanName,@Entity,@PlanId,@EntityId
        
        WHILE @@FETCH_STATUS=0
        BEGIN
        
        Begin
        EXEC @hr = sp_oacreate 'cdo.message', @imsg out
        
        --SendUsing Specifies Whether to send using port (2) or using pickup directory (1)
        EXEC @hr = sp_oasetproperty @imsg,
        'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusing").value','2'
        
        --SMTP Server
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserver").value','pod51022.outlook.com' 
        
        --UserName
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusername").value',@from
        
        --Password
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendpassword").value',@pwd
        
        --UseSSL
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpusessl").value','True' 
        
        --PORT 
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserverport").value','25' 
        
        --Requires Aunthentication None(0) / Basic(1)
        EXEC @hr = sp_oasetproperty @imsg, 
          'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate").value','1' 	
        
        End
      
        IF @IndicatorComparision = 'GT'
		BEGIN
              set  @Comparision = '<b> greater than </b>'
			  set @SubjectComparision = 'above goal'
			  END
        ELSE 	
		BEGIN
		      IF @IndicatorComparision = 'LT'
			  	 BEGIN
                  set @Comparision = '<b>  less than </b>' 
				  set @SubjectComparision = 'below goal'
				 END
              ELSE 
			     BEGIN
                  set @Comparision = '<b> equal to </b>' 
				  set @SubjectComparision = 'equal goal'
				END
		END

	


        Set @subject = @EntityTitle + ' is performing ' + @SubjectComparision 

		SET @body = @EntityTitle + '''s <b>' + @Indicator +' </b> is '+ @Comparision + ' ' + CONVERT(nvarchar(50),@IndicatorGoal) +'% of the goal as of <b>'+ CONVERT(VARCHAR(11),@DisplayDate,106)  + '</b><br><br>Item : ' +  @EntityTitle + '<br>Plan Name : '+ @PlanName 
	    
		IF @ActualGoal is not null
		SET @body = @body +  '<br>Goal : '+ cast(Format(@ActualGoal, '##,##0') as varchar) 

		IF @CurrentGoal is not null
		SET @body = @body + '<br>Current : ' + cast(Format(@CurrentGoal, '##,##0') as varchar)
		 
		 IF @Entity <> 'Plan' 
		 BEGIN
		 SET @UrlString = @Url +'/home?currentPlanId='+convert(nvarchar(max), @planId)+'&plan'+@Entity+'Id='+convert(nvarchar(max),@EntityId)+'&activeMenu=Plan'
		 END
		 ELSE
		 BEGIN
		 SET @UrlString = @Url +'/home?currentPlanId='+convert(nvarchar(max), @planId)+'&activeMenu=Plan'
		 End

		set @body = @body + '<br><br><html><body> URL : <a href=' + @UrlString +'>'+@UrlString+'</a></body></html>' 

        
        EXEC @hr = sp_oamethod @imsg, 'configuration.fields.update', null
        EXEC @hr = sp_oasetproperty @imsg, 'to', @Email
        EXEC @hr = sp_oasetproperty @imsg, 'from', @from
        EXEC @hr = sp_oasetproperty @imsg, 'subject', @subject
        EXEC @hr = sp_oasetproperty @imsg, @bodytype, @body
        EXEC @hr = sp_oamethod @imsg, 'send',null        
	

        -- sample error handling.	
        IF @hr <> 0   
        	BEGIN
			     		EXEC @hr = sp_oageterrorinfo null, out, @description out
			    IF @hr = 0
        		BEGIN					
        			set @output_desc =  @description
					update Alerts
            		set IsEmailSent = 'Not Sent'
        			where AlertId = @AlertId
					print @description
				End
				ELSE
				BEGIN
        			set @output_desc =   ' sp_oageterrorinfo failed'
					update Alerts
            		set IsEmailSent = 'FAIL'
        			where AlertId = @AlertId
				END
			END
        ELSE
        	BEGIN
        		SET @output_desc = ' sp_oageterrorinfo failed'
				update  Alerts
        		set IsEmailSent = 'Success'
        		where AlertId = @AlertId
        	END

             
        EXEC @hr = sp_oadestroy @imsg
        
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate, @ActualGoal, @CurrentGoal, @PlanName,@Entity,@PlanId,@EntityId
        END
  CLOSE Cur_Mail
  DEALLOCATE Cur_Mail
  END
  END
  
Go

/* End - Added by Dhvani Raval for Ticket #2534*/
--Added by Manoj - Stage object in TacticType somehow didn’t get populated from data by EF 
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_TacticType_Stage')AND parent_object_id = OBJECT_ID(N'dbo.TacticType'))
BEGIN
	ALTER TABLE TacticType ADD FOREIGN KEY (StageId) REFERENCES Stage(StageId) 
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_ImprovementTacticType_Metric_ImprovementTacticType')AND parent_object_id = OBJECT_ID(N'dbo.ImprovementTacticType_Metric'))
BEGIN
	ALTER TABLE ImprovementTacticType_Metric ADD FOREIGN KEY (ImprovementTacticTypeId) REFERENCES ImprovementTacticType(ImprovementTacticTypeId) 
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
