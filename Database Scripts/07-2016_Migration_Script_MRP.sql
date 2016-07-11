-- Add by Viral Kadiya
-- Created Date: 07/11/2016
-- Desc: Insert 'Media Codes' permission by client wise and for that insert 'Media Codes' activity to 'Application_Activity' table in BDSAuth database.

------- NOTE: Please execute below script to BDSAuth database and get inserted ApplicationActivityId for the same record

------- Start : PL ticket #2366: Insert 'Media Code' Application activity to Application_Activity table------- 

-- Please set the below variable value as per requirement.
Declare @ApplicationActivityId int=50	-- Add 1 to last ApplicationActivityId column value from Application_Activity table and assign it to @ApplicationActivityId variable

-- Don't make any change to below variable values.
Declare @appId uniqueidentifier
Declare @MRPCode varchar(500)='MRP'
Declare @MediaCodeActTitle varchar(500)='Media Codes'
Declare @Code_MediaCode varchar(500)='MediaCodes'
Declare @clientActivityType varchar(500)='Client'

-- Insert 'Media Codes' activity to Application_Activity table
SELECT TOP 1 @appId=ApplicationId from [Application] where Code=@MRPCode and IsDeleted='0'

IF NOT EXISTS(Select ApplicationActivityId from Application_Activity where ApplicationId=@appId and IsNull(ParentId,'') = '' and ActivityTitle=@MediaCodeActTitle and Code=@Code_MediaCode and ActivityType=@clientActivityType)
BEGIN
	INSERT INTO Application_Activity(ApplicationActivityId,ApplicationId,ParentId,ActivityTitle,Code,CreatedDate,ActivityType) VALUES(@ApplicationActivityId,@appId,null,@MediaCodeActTitle,@Code_MediaCode,GETDATE(),@clientActivityType)
END
GO
------- End : PL ticket #2366: Insert 'Media Code' Application activity to Application_Activity table------- 


------ NOTE: Execute above script to BDSAuth database prior to execute below script and pick ApplicationActivityId by above BDSAuth script and refer to below script


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
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping] DROP CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_Tactic_MediaCodes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tactic_MediaCodes_CustomFieldMapping_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]'))
ALTER TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping] DROP CONSTRAINT [FK_Tactic_MediaCodes_CustomFieldMapping_CustomField]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Actual_MediaCode_Tactic_MediaCodes]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MediaCodes_CustomField_Configuration_CustomField]') AND parent_object_id = OBJECT_ID(N'[dbo].[MediaCodes_CustomField_Configuration]'))
ALTER TABLE [dbo].[MediaCodes_CustomField_Configuration] DROP CONSTRAINT [FK_MediaCodes_CustomField_Configuration_CustomField]
GO
/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]'))
DROP VIEW [dbo].[vClientWise_Tactic]
GO
/****** Object:  Table [dbo].[Tactic_MediaCodes_CustomFieldMapping]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes_CustomFieldMapping]') AND type in (N'U'))
DROP TABLE [dbo].[Tactic_MediaCodes_CustomFieldMapping]
GO
/****** Object:  Table [dbo].[Tactic_MediaCodes]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tactic_MediaCodes]') AND type in (N'U'))
DROP TABLE [dbo].[Tactic_MediaCodes]
GO
/****** Object:  Table [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]') AND type in (N'U'))
DROP TABLE [dbo].[Plan_Campaign_Program_Tactic_Actual_MediaCode]
GO
/****** Object:  Table [dbo].[MediaCodes_CustomField_Configuration]    Script Date: 07/11/2016 2:20:05 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediaCodes_CustomField_Configuration]') AND type in (N'U'))
DROP TABLE [dbo].[MediaCodes_CustomField_Configuration]
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
