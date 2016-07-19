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
                       UPDATE EntityTypeColor SET ColorCode='3A585E' WHERE EntityType='Plan'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Plan','3A585E')
                     END
END

--Campaign color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Campaign')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='62939E' WHERE EntityType='Campaign'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Campaign','62939E')
                     END
END

--Program color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Program')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='79B6C4' WHERE EntityType='Program'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Program','79B6C4')
                     END
END

--Tactic color change in calender
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='EntityTypeColor')
BEGIN
        IF EXISTS(SELECT * FROM EntityTypeColor WHERE EntityType='Tactic')
                     BEGIN
                       UPDATE EntityTypeColor SET ColorCode='727272' WHERE EntityType='Tactic'
                     END
              ELSE
                     BEGIN
                           INSERT INTO EntityTypeColor(EntityType,ColorCode) VALUES('Tactic','727272')
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
			ELSE 0 END <> ['+(@UpdateColumn)+']
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
