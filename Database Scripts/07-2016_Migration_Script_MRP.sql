IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX-ChangeLog-TableName-ClientID-IsDeleted' AND object_id = OBJECT_ID('[ChangeLog]'))

/****** Object:  Index [NonClusteredIndex-20160711-140054]    Script Date: 7/11/2016 2:35:15 PM ******/
DROP INDEX [IX-ChangeLog-TableName-ClientID-IsDeleted] ON [dbo].[ChangeLog]
GO

/****** Object:  Index [NonClusteredIndex-20160711-140054]    Script Date: 7/11/2016 2:35:15 PM ******/
CREATE NONCLUSTERED INDEX [IX-ChangeLog-TableName-ClientID-IsDeleted] ON [dbo].[ChangeLog]
(
	[TableName] ASC,
	[ClientId] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

------ NOTE: Execute 'MediaCodes' acitivity insert attached in BDSAuth DatabaseScript folder prior to execute below script and pick ApplicationActivityId by above BDSAuth script and replace @applicationActivityId variable value with picked value.

-- Add by Viral Kadiya
-- Created Date: 07/11/2016
-- Desc: Insert 'Media Codes' permission to Client_Activity table in Plan database.

------ START: Please modify the below variable value as per requirement.
Declare @clientId uniqueidentifier ='464EB808-AD1F-4481-9365-6AADA15023BD'
Declare @applicationActivityId int = 50  -- Set 'Media Codes' application activity Id from Application_Activity table in BDSAuth db.
Declare @createdBy uniqueidentifier ='D3238077-161A-405F-8F0E-10F4D6E50631'
------------ END ------------ 

IF NOT EXISTS(Select 1 from Client_Activity where ClientId=@clientId and ApplicationActivityId=@applicationActivityId)
BEGIN
	INSERT INTO Client_Activity(ClientId,ApplicationActivityId,CreatedBy,CreatedDate) VALUES(@clientId,@applicationActivityId,@createdBy,GETDATE())
END
GO


-- Add by Viral Kadiya
-- Created Date: 07/11/2016
/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CheckExisting_MediaCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
GO

/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]'))
DROP VIEW [dbo].[vClientWise_Tactic]
GO

/****** Object:  Table [dbo].[MediaCodes_CustomField_Configuration]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediaCodes_CustomField_Configuration]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MediaCodes_CustomField_Configuration](
	[MediaConfId] [int] IDENTITY(1,1) NOT NULL,
	[CustomFieldId] [int] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[Sequence] [int] NULL,
	[Length] [int] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_MediaCodes_CustomField_Configuration] PRIMARY KEY CLUSTERED 
(
	[MediaConfId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode](
	[MediaCodeId] [int] NOT NULL,
	[StageTitle] [varchar](50) NOT NULL,
	[Period] [varchar](5) NOT NULL,
	[Actualvalue] [float] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[MediaCodeId] ASC,
	[StageTitle] ASC,
	[Period] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Tactic_MediaCodes]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Tactic_MediaCodes](
	[MediaCodeId] [int] IDENTITY(1,1) NOT NULL,
	[TacticId] [int] NOT NULL,
	[MediaCode] [varchar](max) NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[CreatedDate] [datetime] NULL,
	[LastModifiedBy] [uniqueidentifier] NULL,
	[LastModifiedDate] [datetime] NULL,
	[IsDeleted] [bit] NULL CONSTRAINT [DF_Tactic_MediaCodes_IsDeleted]  DEFAULT ((0)),
 CONSTRAINT [PK_Tactic_MediaCodes] PRIMARY KEY CLUSTERED 
(
	[MediaCodeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Tactic_MediaCodes_CustomFieldMapping]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TacticId] [int] NOT NULL,
	[MediaCodeId] [int] NOT NULL,
	[CustomFieldId] [int] NULL,
	[CustomFieldValue] [varchar](max) NULL,
 CONSTRAINT [PK_Tactic_MediaCodes_CustomFieldMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vClientWise_Tactic] AS
SELECT  a.PlanTacticId, a.Title, me.*, e.ClientId from Plan_Campaign_Program_Tactic a
left join Tactic_MediaCodes me on me.TacticId=a.PlanTacticId
inner join Plan_Campaign_Program b on a.PlanProgramId=b.PlanProgramId
inner join Plan_Campaign c on b.PlanCampaignId=c.PlanCampaignId
inner join [Plan] d on c.PlanId=d.PlanId
inner join Model e on d.ModelId=e.ModelId
where a.IsDeleted=0  and b.IsDeleted=0 and c.IsDeleted=0 and e.IsDeleted=0' 
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MediaCodes_CustomField_Configuration_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[MediaCodes_CustomField_Configuration]'))
ALTER TABLE [dbo].[MediaCodes_CustomField_Configuration]  WITH CHECK ADD  CONSTRAINT [FK_MediaCodes_CustomField_Configuration_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MediaCodes_CustomField_Configuration_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[MediaCodes_CustomField_Configuration]'))
ALTER TABLE [dbo].[MediaCodes_CustomField_Configuration] CHECK CONSTRAINT [FK_MediaCodes_CustomField_Configuration_CustomField]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes] FOREIGN KEY([MediaCodeId])
REFERENCES [dbo].[Tactic_MediaCodes] ([MediaCodeId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping]  WITH CHECK ADD  CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_CustomField] FOREIGN KEY([CustomFieldId])
REFERENCES [dbo].[CustomField] ([CustomFieldId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping] CHECK CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_CustomField]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping]  WITH CHECK ADD  CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes] FOREIGN KEY([MediaCodeId])
REFERENCES [dbo].[Tactic_MediaCodes] ([MediaCodeId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping] CHECK CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes]
GO
/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CheckExisting_MediaCode]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SP_CheckExisting_MediaCode] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 08-07-2016
-- Description:	method to check whether the media code already exist or not
-- =============================================
ALTER PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
	-- Add the parameters for the stored procedure here
	@ClientId uniqueidentifier ,
	@MediaCode nvarchar(max),
	@IsExists int Output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	set @IsExists=(Select count(*) from [vClientWise_Tactic] where ClientId=@ClientId
	and MediaCode is not null and mediacode=@MediaCode and IsDeleted=0)

END

GO



-- Add By Nishant Sheth
-- Created Date : 07-Jul-2016 
-- Desc :: Check [spViewByDropDownList] stored procedure is exist or not exist.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spViewByDropDownList] AS' 
END
/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 7/5/2016 2:11:36 PM ******/
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
	@ClientId NVARCHAR(50)
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

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Budget_Cost_Actual_Detail]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[Plan_Budget_Cost_Actual_Detail] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 29th Jun 2016
-- Description:	Sp return datatable which contains plan,Campaign,Program,tactic and line item details with respective budget, cost and actual valus 
-- =============================================
ALTER PROCEDURE [dbo].[Plan_Budget_Cost_Actual_Detail]
( 
@PlanId INT ,
@UserId NVARCHAR(36),
@SelectedTab NVARCHAR(50)
)
AS
BEGIN
	

--If tab is planned then planned cost value return in query

IF (@SelectedTab='Planned')

	BEGIN

	SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum 
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
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
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0
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

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
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
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem
	END

--If tab is Actual then Actual values return in query 
ELSE 

	BEGIN

		SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum 
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
		,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PT.TacticBudget AS MainBudgeted
		,PT.CreatedBy 
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PTB.Value
		,PTB.Period
		,PT.Cost
		,'C'+PTAct.Period as CPeriod
		,PTAct.ActualValue as CValue
FROM 
	Plan_Campaign_Program_Tactic PT
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual PTAct ON PT.PlanTacticId=PTAct.PlanTacticId AND PTAct.StageTitle='Cost'
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 
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

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
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
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem

	END


END

GO

-- =============================================
-- Author: Rahul Shah
-- Create date: 06th July 2016
-- Description:	increase the size of Title column of Plan_Campaign_Program_Tactic_LineItem Table
-- =============================================
GO
DECLARE @TableName nvarchar (200) = 'Plan_Campaign_Program_Tactic_LineItem'
DECLARE @ColumnName nvarchar (200) = 'Title'
IF EXISTS (SELECT column_name FROM INFORMATION_SCHEMA.columns WHERE table_name = @TableName and column_name = @ColumnName)
BEGIN
ALTER TABLE Plan_Campaign_Program_Tactic_LineItem ALTER COLUMN Title NVARCHAR(512)
END

GO
-- added by devanshi regarding PL ticket #2375 :add validation for media code on 11-7-2016


/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/08/2016 6:35:42 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]') AND type in (N'P', N'PC',N'V'))
BEGIN
DROP VIEW [dbo].[vClientWise_Tactic]
End
GO

/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/08/2016 6:35:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vClientWise_Tactic] AS
SELECT  a.PlanTacticId, a.Title, me.*, e.ClientId from Plan_Campaign_Program_Tactic a
left join Tactic_MediaCodes me on me.TacticId=a.PlanTacticId
inner join Plan_Campaign_Program b on a.PlanProgramId=b.PlanProgramId
inner join Plan_Campaign c on b.PlanCampaignId=c.PlanCampaignId
inner join [Plan] d on c.PlanId=d.PlanId
inner join Model e on d.ModelId=e.ModelId
where a.IsDeleted=0  and b.IsDeleted=0 and c.IsDeleted=0 and e.IsDeleted=0
GO



/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/08/2016 6:34:02 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CheckExisting_MediaCode]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
END
GO

/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/08/2016 6:34:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 08-07-2016
-- Description:	method to check whether the media code already exist or not
-- =============================================
CREATE PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
	-- Add the parameters for the stored procedure here
	@ClientId uniqueidentifier ,
	@MediaCode nvarchar(max),
	@IsExists int Output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	set @IsExists=(Select count(*) from [vClientWise_Tactic] where ClientId=@ClientId
	and MediaCode is not null and mediacode=@MediaCode and IsDeleted=0)

END

GO



--end



-- =============================================
-- Author:		Arpita Soni
-- Create date: 12-07-2016
-- Ticket:      #2353
-- Description:	Create table for ROI package details and some constraints 
-- =============================================
-- CREATE TABLE [dbo].[ROI_PackageDetail]
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ROI_PackageDetail]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ROI_PackageDetail](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[AnchorTacticID] [int] NOT NULL,
		[PlanTacticId] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_ROI_PackageDetail] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

-- CREATE FK ON [PlanTacticId] COLUMN
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ROIPackageDetail_Plan_Campaign_Program_Tactic]') AND parent_object_id = OBJECT_ID(N'[dbo].[ROI_PackageDetail]'))
ALTER TABLE [dbo].[ROI_PackageDetail]  WITH CHECK ADD  CONSTRAINT [FK_ROIPackageDetail_Plan_Campaign_Program_Tactic] FOREIGN KEY([PlanTacticId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ROIPackageDetail_Plan_Campaign_Program_Tactic]') AND parent_object_id = OBJECT_ID(N'[dbo].[ROI_PackageDetail]'))
ALTER TABLE [dbo].[ROI_PackageDetail] CHECK CONSTRAINT [FK_ROIPackageDetail_Plan_Campaign_Program_Tactic]
GO

-- CREATE FK ON [AnchorTacticID] COLUMN
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ROIPackageDetail_Plan_Campaign_Program_Tactic1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ROI_PackageDetail]'))
ALTER TABLE [dbo].[ROI_PackageDetail]  WITH CHECK ADD  CONSTRAINT [FK_ROIPackageDetail_Plan_Campaign_Program_Tactic1] FOREIGN KEY([AnchorTacticID])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic] ([PlanTacticId])
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ROIPackageDetail_Plan_Campaign_Program_Tactic1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ROI_PackageDetail]'))
ALTER TABLE [dbo].[ROI_PackageDetail] CHECK CONSTRAINT [FK_ROIPackageDetail_Plan_Campaign_Program_Tactic1]
GO

-- CREATE UNIQUE CONSTRAINT ON TWO COLUMNS
IF NOT EXISTS (SELECT * FROM sysconstraints WHERE OBJECT_NAME(constid) = 'UQ_AnchorTacticID_PlanTacticID' AND OBJECT_NAME(id) = 'ROI_PackageDetail')
ALTER TABLE [ROI_PackageDetail] ADD CONSTRAINT UQ_AnchorTacticID_PlanTacticID UNIQUE(AnchortacticId, PlanTacticId)
GO

-- ADD COLUMN AssetType INTO TacticType TABLE
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AssetType' AND [object_id] = OBJECT_ID(N'TacticType'))
BEGIN
	ALTER TABLE dbo.TacticType ADD AssetType NVARCHAR(50) DEFAULT 'Promotion'
END

-- UPDATE AssetType to "Promotion" TO ASSIGN DEFAULT VALUE
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AssetType' AND [object_id] = OBJECT_ID(N'TacticType'))
BEGIN
	UPDATE dbo.TacticType SET AssetType = 'Promotion' WHERE AssetType IS NULL
END
GO
-- End 
-- ======================================================================================
-- Created By : Mitesh
-- Created Date : 07/11/2014
-- ======================================================================================

--Plan color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Plan')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='e00000' WHERE EntityType='Plan'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Plan','e00000')
                     END
END

--Campaign color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Campaign')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='00ff33' WHERE EntityType='Campaign'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Campaign','00ff33')
                     END
END

--Program color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Program')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='ff0099' WHERE EntityType='Program'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Program','ff0099')
                     END
END

--Tactic color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Tactic')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='407b22' WHERE EntityType='Tactic'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Tactic','407b22')
                     END
END


GO


/****** Object:  Table [dbo].[Report_Intergration_Conf]    Script Date: 07/11/2016 2:20:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Report_Intergration_Conf]') AND type in (N'U'))
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

Go
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[GetCustomDashboardsClientwise]') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetCustomDashboardsClientwise]
END
GO

CREATE PROCEDURE [dbo].[GetCustomDashboardsClientwise] 
(
	@UserId UNIQUEIDENTIFIER, @ClientId UNIQUEIDENTIFIER
)
AS 
BEGIN
	SELECT dash.DisplayName,dash.id as DashboardId,ISNULL(up.PermissionType, '') as PermissionType
	FROM Dashboard dash
	INNER JOIN Report_Intergration_Conf ric ON (dash.id = ric.IdentifierValue AND ric.TableName = 'Dashboard' AND ric.IdentifierColumn = 'id' AND ric.ClientId = @ClientId)
	LEFT JOIN User_Permission up ON (dash.id = up.DashboardId and up.UserId = @UserId)
END
GO

--Modified BY : Komal rawal
--Date :15-7-16
--Desc : To get Tactic Type listing after we delete tactic type from model
/****** Object:  StoredProcedure [dbo].[GetTacticTypeList]    Script Date: 07/15/2016 14:33:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticTypeList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTacticTypeList]
GO
/****** Object:  StoredProcedure [dbo].[GetTacticTypeList]    Script Date: 07/15/2016 14:33:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticTypeList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetTacticTypeList] AS' 
END
GO
-- Created BY Nisahnt Sheth
-- Desc :: get list of tactic type 
-- Created Date : 04-Mar-2016
ALTER PROCEDURE [dbo].[GetTacticTypeList]
@TacticIds nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;
SELECT [TacticType].Title,[TacticType].TacticTypeId,Count([TacticType].TacticTypeId) As Number FROM TacticType WITH (NOLOCK) 
CROSS APPLY (SELECT PlanTacticId,TacticTypeId FROM Plan_Campaign_Program_Tactic As Tactic WITH (NOLOCK)
WHERE TacticType.TacticTypeId=Tactic.TacticTypeId AND PlanTacticId in (Select val From dbo.comma_split(@TacticIds,',')) AND IsDeleted=0) Tactic
GROUP BY [TacticType].TacticTypeId,[TacticType].Title
ORDER BY [TacticType].Title
END


GO
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]'))
DROP VIEW [dbo].[vClientWise_Tactic]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vClientWise_Tactic] AS
SELECT  a.PlanTacticId, a.Title, me.*, e.ClientId from Plan_Campaign_Program_Tactic a
inner join Tactic_MediaCodes me on me.TacticId=a.PlanTacticId
inner join Plan_Campaign_Program b on a.PlanProgramId=b.PlanProgramId
inner join Plan_Campaign c on b.PlanCampaignId=c.PlanCampaignId
inner join [Plan] d on c.PlanId=d.PlanId
inner join Model e on d.ModelId=e.ModelId
where a.IsDeleted=0  and b.IsDeleted=0 and c.IsDeleted=0 and e.IsDeleted=0' 
GO


-- =============================================
-- Author:		Arpita Soni
-- Create date: 15-07-2016
-- Ticket:      #2357
-- Description:	Get package information in tactic details
-- =============================================
-- DROP AND CREATE STORED PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetListPlanCampaignProgramTactic]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
END
GO
-- Created by nishant Sheth
-- Created on :: 03-Feb-2016
-- Desc :: Get list of Plans,Campaigns,Prgorams,Tactics
-- EXEC [dbo].[GetListPlanCampaignProgramTactic] '19071','464EB808-AD1F-4481-9365-6AADA15023BD'
CREATE PROCEDURE [dbo].[GetListPlanCampaignProgramTactic]
 @PlanId NVARCHAR(MAX) = NULL,
 @ClientId NVARCHAR(MAX) = NULL
AS
BEGIN
SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tempPlanId') IS NOT NULL
    DROP TABLE #tempPlanId
SELECT val into #tempPlanId FROM dbo.comma_split(@Planid, ',')

IF OBJECT_ID('tempdb..#tempClientId') IS NOT NULL
    DROP TABLE #tempClientId
SELECT val into #tempClientId FROM dbo.comma_split(@ClientId, ',')

-- Plan Details
SELECT * FROM [Plan] AS [Plan] WITH (NOLOCK) 
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)
 AND [Plan].IsDeleted=0 

-- Campaign Details
SELECT Campaign.* FROM Plan_Campaign Campaign
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE Campaign.IsDeleted=0

-- Program Details
SELECT Program.*,[Plan].PlanId FROM Plan_Campaign_Program Program WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val From #tempClientId)) [Model]
WHERE Program.IsDeleted=0

-- Tactic Details
SELECT Tactic.*,[Plan].PlanId,[Campaign].PlanCampaignId,[TacticType].[Title] AS 'TacticTypeTtile',[TacticType].[ColorCode],[Plan].[Year] AS 'PlanYear',[Plan].ModelId, 
[Campaign].Title AS 'CampaignTitle',[Program].Title AS 'ProgramTitle',[Plan].Title AS 'PlanTitle', [Stage].Title AS 'StageTitle',[Plan].[Status] AS 'PlanStatus', [Package].[AnchorTacticId] AS 'AnchorTacticId', [Package].[Title] AS 'PackageTitle'
,[TacticType].[AssetType] AS 'AssetType'
FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId=Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
CROSS APPLY (SELECT PlanId,ModelId,[Year],Title,[Status] From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId IN (SELECT val From #tempPlanId)  AND [Plan].PlanId=Campaign.PlanId AND [Plan].IsDeleted=0 ) [Plan]
CROSS APPLY (SELECT ModelId,ClientId From [Model]  WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND ClientId IN (SELECT val FROM #tempClientId)) [Model]
OUTER APPLY (SELECT [TacticTypeId],[Title],[ColorCode],[AssetType] FROM [TacticType] WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND IsDeleted=0) [TacticType]
OUTER APPLY (SELECT [StageId],[Title] FROM Stage WHERE [Tactic].StageId=Stage.StageId) Stage
OUTER APPLY (SELECT [AnchorTacticId],[Title] FROM [ROI_PackageDetail] AS Package
			 CROSS APPLY (SELECT [Title] FROM [Plan_Campaign_Program_Tactic] AS AnchorTactic 
			 WHERE [Package].[AnchorTacticID] = [AnchorTactic].[PlanTacticId]) AnchorTactic
			 WHERE [Tactic].[PlanTacticId] = [Package].[PlanTacticId]
			 ) Package
WHERE Tactic.IsDeleted = 0
END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ImportMarketingBudgetMonthly') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].ImportMarketingBudgetMonthly
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ImportMarketingBudgetQuarter') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].ImportMarketingBudgetQuarter
GO

-- ================================
-- Create User-defined Table Type
-- Created By Nishant Sheth
-- ================================
IF TYPE_ID('[MarketingBudgetColumns]') IS NOT NULL
   BEGIN
     DROP TYPE [MarketingBudgetColumns]
   END
GO  
-- Create the data type
CREATE TYPE [dbo].[MarketingBudgetColumns] AS TABLE(
	[Month] [nvarchar](15) NULL,
	[ColumnName] [nvarchar](255) NULL,
	[ColumnIndex] [bigint] NULL
)
GO

-- Created BY Nishant Sheth
-- Created Date 06-Jul-2016
-- Desc :: Import Data from excel data in marketing budget with monthly
CREATE PROCEDURE ImportMarketingBudgetMonthly
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@ClientId NVARCHAR(MAX)
,@UserId NVARCHAR(MAX)
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;

CREATE TABLE #tmpXmlData (ROWNUM BIGINT)
CREATE TABLE #tmpCustomDeleteDropDown (EntityId BIGINT,CustomFieldId BIGINT)
CREATE TABLE #tmpCustomDeleteTextBox (EntityId BIGINT,CustomFieldId BIGINT)

DECLARE @Textboxcol nvarchar(max)=''
DECLARE @UpdateColumn NVARCHAR(255)
DECLARE @CustomEntityDeleteDropdownCount BIGINT
DECLARE @CustomEntityDeleteTextBoxCount BIGINT
DECLARE @IsCutomFieldDrp BIT
DECLARE @GetBudgetAmoutData NVARCHAR(MAX)=''
DECLARE @Count Int = 1;
DECLARE @RowCount INT;
DECLARE @ColName nvarchar(100)
DECLARE @IsMonth nvarchar(100)

SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol

DECLARE @XmldataQuery NVARCHAR(MAX)=''

DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

SET @XmldataQuery += '
	SELECT 
	ROW_NUMBER() OVER(ORDER BY(SELECT 100)),
	'
DECLARE @ConcatColumns NVARCHAR(MAX)=''

WHILE(@Count<=@RowCount)
BEGIN
SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

SET @ConcatColumns  += '
	pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],'

	SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) '
	SET @Count=@Count+1;
END
SELECT @ConcatColumns=LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)

SET @XmldataQuery+= @ConcatColumns+' FROM  
	
	@XmlData.nodes(''/data/row'') AS People(pref);'

EXEC(@tmpXmlDataAlter)

;WITH tblChild AS
(
    SELECT Id,ParentId,BudgetId
        FROM Budget_Detail WHERE Id = @BudgetDetailId 
    UNION ALL
    SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
	CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
)
SELECT  * Into #tmpChildBudgets
    FROM tblChild
OPTION(MAXRECURSION 0)

INSERT INTO #tmpXmlData EXECUTE sp_executesql @XmldataQuery, N'@XmlData XML OUT', @XmlData = @XmlData  OUT
	
	-- Remove Other Child items which are not related to parent
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	OUTER APPLY(
	SELECT * FROM #tmpChildBudgets tmpChildBudgets
	WHERE CAST(tmpXmlData.[Id#1] AS INT)= tmpChildBudgets.Id
	) tmpChildBudgets WHERE tmpChildBudgets.Id IS NULL

	-- Remove View/None Permission budgets
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	OUTER APPLY(
	SELECT * FROM Budget_Permission BudgetPermission
	WHERE CAST(tmpXmlData.[Id#1] AS INT)=BudgetPermission.BudgetDetailId AND UserId=@UserId
	AND (BudgetPermission.IsOwner=1 OR BudgetPermission.PermisssionCode=0)
	) BudgetPermission WHERE BudgetPermission.Id IS NULL

-- Update Process
DECLARE @GetBudgetAmoutData2 NVARCHAR(MAX)=''
DECLARE @MonthNumber varchar(2)
SET @Count=2;
WHILE(@Count<=@RowCount)
BEGIN

	SELECT @UpdateColumn=[ColumnName],@Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE [Month] END FROM @ImportBudgetCol WHERE ColumnIndex=@Count
	
	-- Insert/Update values for budget and forecast
	 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
	 BEGIN
		IF(@Ismonth!='' AND @Ismonth!='Total')
		BEGIN
			SELECT  @MonthNumber = CAST(DATEPART(MM,''+@IsMonth+' 01 1990') AS varchar(2))
			DECLARE @temp nvarchar(max)=''
			SET @GetBudgetAmoutData+=' 
			-- Update the Budget Detail amount table for Forecast and Budget values
			UPDATE BudgetDetailAmount SET ['+(@UpdateColumn)+']=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END FROM 
			Budget_DetailAmount BudgetDetailAmount
			CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE BudgetDetailAmount.BudgetDetailId=CAST([Id#1] AS INT) 
			AND BudgetDetailAmount.Period=''Y'+@MonthNumber+''' AND ISNULL(['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')<>'''' 
			AND CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END <> ISNULL(['+(@UpdateColumn)+'],0)
			) tmpXmlData
			
			-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
			INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,['+@UpdateColumn+'])
			SELECT  tmpXmlData.[Id#1] 
			,''Y'+@MonthNumber+'''
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END
			FROM #tmpXmlData  tmpXmlData
			OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
			WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
			AND A.Period=''Y'+@MonthNumber+''')
			A WHERE A.Id IS NULL 
			
			 '
			 
		END
	END

	-- Custom Columns
	 IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
	 BEGIN
		IF(@Ismonth='' AND @Ismonth!='Total')
		BEGIN

			SELECT @IsCutomFieldDrp = CASE WHEN CustomFieldType.Name='TextBox' THEN 0 ELSE 1 END FROM CustomField 
			CROSS APPLY(SELECT CustomFieldType.Name,CustomFieldType.CustomFieldTypeId FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) CustomFieldType
			WHERE CustomField.Name=''+@UpdateColumn+'' AND CustomField.ClientId=''+@ClientId+'' AND CustomField.IsDeleted=0 AND CustomField.EntityType='Budget'
			
			-- Insert/Update/Delete values for custom field as dropdown
			IF(@IsCutomFieldDrp=1)
			BEGIN

				SET @GetBudgetAmoutData+=' 
				-- Get List of record which need to delete from CustomField Entity Table

				INSERT INTO #tmpCustomDeleteDropDown 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteDropdownCount=COUNT(*) FROM #tmpCustomDeleteDropDown tmpCustomDelete

				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteDropdownCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM #tmpCustomDeleteDropDown)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM #tmpCustomDeleteDropDown)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,CustOpt.CustomFieldOptionId,'''+@UserId+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+']) CustOpt
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE CustomFieldEntity.CustomFieldEntityId IS NULL
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=CustOpt.CustomFieldOptionId FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+']) CustOpt 
				WHERE CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				'
			END

			-- Insert/Update/Delete values for custom field as Textbox
			IF(@IsCutomFieldDrp<>1)
			BEGIN

				SET @GetBudgetAmoutData+='  
				-- Get List of record which need to delete from CustomField Entity Table
				INSERT INTO #tmpCustomDeleteTextBox 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteTextBoxCount=COUNT(*) FROM #tmpCustomDeleteTextBox tmpCustomDelete
				
				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteTextBoxCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM #tmpCustomDeleteTextBox)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM #tmpCustomDeleteTextBox)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''+@UserId+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] IS NOT NULL AND CustomFieldEntity.CustomFieldEntityId IS NULL 
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField 
				WHERE tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] IS NOT NULL
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
				'

			END
			
		END
	END
	SET @Ismonth=''
	SET @Count=@Count+1;
	SET @MonthNumber=0;
END

EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT
END
GO

-- Created BY Nishant Sheth
-- Created Date 06-Jul-2016
-- Desc :: Import Data from excel data in marketing budget with quartely
CREATE PROCEDURE ImportMarketingBudgetQuarter
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@ClientId NVARCHAR(MAX)
,@UserId NVARCHAR(MAX)
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY

CREATE TABLE #tmpXmlData (ROWNUM BIGINT)
CREATE TABLE #tmpCustomDeleteDropDown (EntityId BIGINT,CustomFieldId BIGINT)
CREATE TABLE #tmpCustomDeleteTextBox (EntityId BIGINT,CustomFieldId BIGINT)
CREATE TABLE #tmpQuarters ([Quarter] NVARCHAR(4))

DECLARE @Textboxcol nvarchar(max)=''
DECLARE @UpdateColumn NVARCHAR(255)
DECLARE @CustomEntityDeleteDropdownCount BIGINT
DECLARE @CustomEntityDeleteTextBoxCount BIGINT
DECLARE @IsCutomFieldDrp BIT
DECLARE @GetBudgetAmoutData NVARCHAR(MAX)=''
DECLARE @Count Int = 1;
DECLARE @RowCount INT;
DECLARE @ColName nvarchar(100)
DECLARE @IsMonth nvarchar(100)

-- Declare variable for time frame
DECLARE @QFirst NVARCHAR(MAX)=''
DECLARE @QSecond NVARCHAR(MAX)=''
DECLARE @QThird NVARCHAR(MAX)=''
-- Declare Variable for forecast/budget column is exist or not

DECLARE @BudgetOrForecastIndex INT

SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol

DECLARE @XmldataQuery NVARCHAR(MAX)=''

DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

SET @XmldataQuery += '
	SELECT 
	ROW_NUMBER() OVER(ORDER BY(SELECT 100)),
	'
DECLARE @ConcatColumns NVARCHAR(MAX)=''

WHILE(@Count<=@RowCount)
BEGIN
SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

SET @ConcatColumns  += '
	pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],'

	SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) '
	SET @Count=@Count+1;
END
SELECT @ConcatColumns=LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)

SET @XmldataQuery+= @ConcatColumns+' FROM  
	
	@XmlData.nodes(''/data/row'') AS People(pref);'

EXEC(@tmpXmlDataAlter)

;WITH tblChild AS
(
    SELECT Id,ParentId,BudgetId
        FROM Budget_Detail WHERE Id = @BudgetDetailId 
    UNION ALL
    SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
	CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
)
SELECT  * Into #tmpChildBudgets
    FROM tblChild
OPTION(MAXRECURSION 0)


INSERT INTO #tmpXmlData EXECUTE sp_executesql @XmldataQuery, N'@XmlData XML OUT', @XmlData = @XmlData  OUT


	-- Remove Other Child items which are not related to parent
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	OUTER APPLY(
	SELECT * FROM #tmpChildBudgets tmpChildBudgets
	WHERE CAST(tmpXmlData.[Id#1] AS INT)= tmpChildBudgets.Id
	) tmpChildBudgets WHERE tmpChildBudgets.Id IS NULL
	
	-- Remove View/None Permission budgets
	DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
	OUTER APPLY(
	SELECT * FROM Budget_Permission BudgetPermission
	WHERE CAST(tmpXmlData.[Id#1] AS INT)=BudgetPermission.BudgetDetailId AND UserId=@UserId
	AND (BudgetPermission.IsOwner=1 OR BudgetPermission.PermisssionCode=0)
	) BudgetPermission WHERE BudgetPermission.Id IS NULL
	
-- Update Process
DECLARE @GetBudgetAmoutData2 NVARCHAR(MAX)=''
DECLARE @MonthNumber varchar(2)
SET @Count=3;
WHILE(@Count<=@RowCount)
BEGIN

	SELECT  @Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE LTRIM(RTRIM([Month])) END
		,@UpdateColumn = CASE  WHEN  ISNULL(ColumnName,'')='' THEN '' ELSE [ColumnName] END
		,@BudgetOrForecastIndex = ColumnIndex FROM @ImportBudgetCol WHERE ColumnIndex=@Count
		

	IF (@Ismonth<>'')
	BEGIN
		-- Set Time frame based on Quarters
		IF(@IsMonth='Q1')
		BEGIN	
			SET @QFirst ='Y1'
			SET @QSecond ='Y2'
			SET @QThird ='Y3'
		END

		IF(@IsMonth='Q2')
		BEGIN	
			SET @QFirst ='Y4'
			SET @QSecond ='Y5'
			SET @QThird ='Y6'
		END

		IF(@IsMonth='Q3')
		BEGIN	
			SET @QFirst ='Y7'
			SET @QSecond ='Y8'
			SET @QThird ='Y9'
		END

		IF(@IsMonth='Q4')
		BEGIN	
			SET @QFirst ='Y10'
			SET @QSecond ='Y11'
			SET @QThird ='Y12'
		END

	 -- Insert/Update values for budget and forecast
	 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
	 BEGIN
		IF(@Ismonth!='' AND @Ismonth!='Total')
		BEGIN
			--SELECT @MonthNumber = CAST(DATEPART(MM,''+@IsMonth+' 01 1990') AS varchar(2))
			
			SET @GetBudgetAmoutData=' 
			-- Update the Budget Detail amount table for Forecast and Budget values
			UPDATE BudgetDetailAmount
			SET BudgetDetailAmount.['+@UpdateColumn+']=TableData.['+@UpdateColumn+']
			FROM Budget_DetailAmount BudgetDetailAmount
			CROSS APPLY(
			SELECT * FROM (
			SELECT [Id#1]
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',['+@QThird+'] = CASE WHEN SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+']) > MIN(['+@IsMonth+@UpdateColumn+'])
			THEN 
				CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
				THEN 0
			ELSE 
				SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
			END
			ELSE SUM(['+@QThird+@UpdateColumn+'])
			END  
			,['+@QSecond+'] = CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
			THEN 
				CASE WHEN SUM(['+@QSecond+@UpdateColumn+']) < (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+'])))
			THEN 0
			WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
				THEN 0
			ELSE SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))) 
			END
			ELSE SUM(['+@QSecond+@UpdateColumn+'])
			END
			,['+@QFirst+'] = CASE WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
						THEN SUM(['+@QFirst+@UpdateColumn+']) + (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
				  ELSE 
						CASE WHEN SUM(['+@QFirst+@UpdateColumn+']) > SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
						THEN SUM(['+@QFirst+@UpdateColumn+'])
						ELSE
						SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
						END
					END
			' END +'

			FROM(
			SELECT [Id#1]
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',SUM(ISNULL(['+@QFirst+@UpdateColumn+'],0)) AS '+@QFirst+@UpdateColumn+'
			,SUM(ISNULL(['+@QSecond+@UpdateColumn+'],0)) AS '+@QSecond+@UpdateColumn+'
			,SUM(ISNULL(['+@QThird+@UpdateColumn+'],0)) AS '+@QThird+@UpdateColumn+'
			,MIN(ISNULL(['+@IsMonth+@UpdateColumn+'],0)) AS '+@IsMonth+@UpdateColumn+'' END +'
			FROM
			(
			-- First Month Of Quarter
			SELECT tmpXmlData.[Id#1]
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN '
			,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN CASE WHEN ['+@UpdateColumn+'] > 0 THEN ['+@UpdateColumn+']
			ELSE 0 END ELSE 0 END AS '+@QFirst+@UpdateColumn+'
			,NULL AS '+@QSecond+@UpdateColumn+'
			,NULL AS '+@QThird+@UpdateColumn+'
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
			FROM #tmpXmlData tmpXmlData
			CROSS APPLY(SELECT BudgetDetailId,Period
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
			FROM Budget_DetailAmount BudgetDetailAmount WHERE 
			tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QFirst+''') BudgetDetailAmount 
			-- Second Month Of Quarter
			UNION ALL
			SELECT tmpXmlData.[Id#1]
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
			,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN CASE WHEN ['+@UpdateColumn+'] > 0 THEN ['+@UpdateColumn+']
			ELSE 0 END ELSE 0 END AS '+@QSecond+@UpdateColumn+'
			,NULL AS '+@QThird+@UpdateColumn+'
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
			FROM #tmpXmlData tmpXmlData
			CROSS APPLY(SELECT BudgetDetailId,Period
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
			FROM Budget_DetailAmount BudgetDetailAmount WHERE 
			tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QSecond+''') BudgetDetailAmount 
			-- Third Month Of Quarter
			UNION ALL
			SELECT tmpXmlData.[Id#1]
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
			,NULL AS '+@QSecond+@UpdateColumn+'
			,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN CASE WHEN ['+@UpdateColumn+'] > 0 THEN ['+@UpdateColumn+']
			ELSE 0 END ELSE 0 END AS '+@QThird+@UpdateColumn+'
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
			FROM #tmpXmlData tmpXmlData
			CROSS APPLY(SELECT BudgetDetailId,Period
			'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
			FROM Budget_DetailAmount BudgetDetailAmount WHERE 
			tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QThird+''') BudgetDetailAmount 
			) AS A
			GROUP BY [Id#1]
			) AS QuarterData
			GROUP BY Id#1
			) As P
			UNPIVOT(
			['+@UpdateColumn+'] FOR Period
			IN(['+@QFirst+'],['+@QSecond+'],['+@QThird+'])
			) as TableData
			WHERE TableData.[Id#1]=BudgetDetailAmount.BudgetDetailId
			AND TableData.Period=BudgetDetailAmount.Period			
			) TableData

			-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
			INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,'+@UpdateColumn+')
			SELECT tmpXmlData.[Id#1],'''+@QFirst+'''
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
			THEN 
			CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
			THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) ELSE 0 END
			ELSE 0 END
			FROM #tmpXmlData tmpXmlData
			OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
			WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
			AND A.Period IN('''+@QFirst+''','''+@QSecond+''','''+@QThird+''')
			) A WHERE A.Id IS NULL 
			'
			
			
			EXECUTE sp_executesql @GetBudgetAmoutData
			SET @GetBudgetAmoutData=''
		END
	END
	--END
	END
	-- Custom Columns
	IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
	 BEGIN
		IF(@Ismonth='' AND @Ismonth!='Total')
		BEGIN

			SELECT @IsCutomFieldDrp = CASE WHEN CustomFieldType.Name='TextBox' THEN 0 ELSE 1 END FROM CustomField 
			CROSS APPLY(SELECT CustomFieldType.Name,CustomFieldType.CustomFieldTypeId FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) CustomFieldType
			WHERE CustomField.Name=''+@UpdateColumn+'' AND CustomField.ClientId=''+@ClientId+'' AND CustomField.IsDeleted=0 AND CustomField.EntityType='Budget'
			
			-- Insert/Update/Delete values for custom field as dropdown
			IF(@IsCutomFieldDrp=1)
			BEGIN
				SET @GetBudgetAmoutData+=' 
				-- Get List of record which need to delete from CustomField Entity Table

				INSERT INTO #tmpCustomDeleteDropDown 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteDropdownCount=COUNT(*) FROM #tmpCustomDeleteDropDown tmpCustomDelete

				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteDropdownCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM #tmpCustomDeleteDropDown)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM #tmpCustomDeleteDropDown)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,CustOpt.CustomFieldOptionId,'''+@UserId+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+']) CustOpt
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE CustomFieldEntity.CustomFieldEntityId IS NULL
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=CustOpt.CustomFieldOptionId FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+']) CustOpt 
				WHERE CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
				'
				
			END

			-- Insert/Update/Delete values for custom field as Textbox
			IF(@IsCutomFieldDrp<>1)
			BEGIN
				SET @GetBudgetAmoutData+=' 
				-- Get List of record which need to delete from CustomField Entity Table
				INSERT INTO #tmpCustomDeleteTextBox 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteTextBoxCount=COUNT(*) FROM #tmpCustomDeleteTextBox tmpCustomDelete
				
				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteTextBoxCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM #tmpCustomDeleteTextBox)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM #tmpCustomDeleteTextBox)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''+@UserId+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] IS NOT NULL AND CustomFieldEntity.CustomFieldEntityId IS NULL 

				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+@ClientId+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField 
				WHERE tmpXmlData.['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'] IS NOT NULL
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
				'

			END
			
		END
	END

	SET @Ismonth=''
	SET @Count=@Count+1;
	SET @MonthNumber=0;
END


EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT

END TRY
BEGIN CATCH
	
END CATCH
END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ExportToCSV') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].[ExportToCSV]
GO

CREATE PROCEDURE [dbo].[ExportToCSV]
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
AND EntityType IN('Campaign','Program','Tactic','Lineitem')
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
AND EntityType IN('Campaign','Program','Tactic')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End

END
GO

-- Created By: Viral Kadiya
-- Created On: 19th July 2016
-- Desc: Make changes regarding PL ticket #2424.

/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 07/19/2016 5:15:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 07/19/2016 5:15:16 PM ******/
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

--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''105406'',2,1203,255,0,0)
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
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016
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
		Declare @actIsSyncedSFDC varchar(100)=''IsSyncedSFDC''				-- Added on 06/23/2016
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
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement		-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement			-- Added on 06/23/2016
END
ELSE
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement		-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
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
																		WHEN(Tac.IsSyncMarketo=''1'') THEN IsNUll(Tac.TacticCustomName,'''') ELSE		-- if 3way Marketo-SFDC related data then pass TacticCustomName as ''Title''
																		CASE
																			WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																			ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																		END
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(prg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(acost.ActualCost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN Cast(ISNULL(Tac.IsSyncMarketo,''0'') as varchar(50))		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(cmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''		-- Added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(Imprvcmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(ImprvPrg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
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

/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 07/19/2016 5:22:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 07/19/2016 5:22:23 PM ******/
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

--SELECT * from  [GetSFDCSourceTargetMappingData_Marketo3Way](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''105314'',2,1211,255,0,0)
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
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016
		Declare @actIsSyncedSFDC varchar(100)=''IsSyncedSFDC''				-- Added on 06/23/2016
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
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
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
																		WHEN(Tac.IsSyncMarketo=''1'') THEN IsNUll(Tac.TacticCustomName,'''') ELSE		-- if 3way Marketo-SFDC related data then pass TacticCustomName as ''Title''
																		CASE
																			WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																			ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																		END
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(prg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(acost.ActualCost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN Cast(ISNULL(Tac.IsSyncMarketo,''0'') as varchar(50))		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(cmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''												-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
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
-- added by devanshi on 25-7-2016 for pl ticket #2429


IF  EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes]')  AND name = 'MediaCode')
ALTER TABLE Tactic_MediaCodes DROP COLUMN MediaCode


go
IF Not EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes]')  AND name = 'MediaCode')
	ALTER TABLE Tactic_MediaCodes 
    ADD MediaCode bigint
GO 

IF Not EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes]')  AND name = 'MediaCode')
	ALTER TABLE Tactic_MediaCodes 
    ADD MediaCodeValue nvarchar(max)
Go
-- end

IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'WebApiGetReportRawData') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[WebApiGetReportRawData]
END
GO
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

	IF(@GT != 'bar' AND @GT != 'column' AND @GT != 'pie' AND @GT != 'donut' AND @GT != 'line' AND @GT != 'stackbar' AND @GT != 'stackcol' AND @GT != 'area' AND @GT != 'bubble' AND @GT != 'scatter' AND @GT != 'columnrange' AND @GT != 'negative' AND @GT != 'solidgauge' AND @GT != 'gauge')
	BEGIN
			SET @GT = 'Currently we are not supporting graph type "'+ @GT +'"  in Measure Report Web API'
			RAISERROR(@GT,16,1) 
	END
	ELSE
	BEGIN
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
		DECLARE @ColDimLst NVARCHAR(MAX)
			IF(@CustomQuery != '') --In case of custom query is configured for the report
			BEGIN
			
				SELECT TOP 1 
					@DateDimensionId = D.Id
				FROM ReportGraph G 
					LEFT JOIN ReportAxis A			ON G.id = A.ReportGraphId 
					INNER JOIN Dimension D			ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
				WHERE G.Id = @Id

				IF(@GT = 'columnrange')
				BEGIN
					IF (@FilterValues IS NOT NULL AND @FilterValues != '')
					BEGIN
						SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
						SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
						SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
					END
					ELSE
					BEGIN
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
					END

					IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
					BEGIN
						SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
					END	
				END
				ELSE
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
						IF(@GT = 'columnrange')
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END	
						END
						ELSE
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END						
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
END

GO



-- ===========================Please put your script above this script=============================
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
set @release = 'July.2016'
set @version = 'July.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
