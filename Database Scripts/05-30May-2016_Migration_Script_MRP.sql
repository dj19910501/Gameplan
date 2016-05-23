-- Created By Nishant Sheth
-- Created Date : 26-Apr-2016
-- Desc :: Get list of budget and line item budget list
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBudgetListAndLineItemBudgetList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBudgetListAndLineItemBudgetList]
GO
CREATE PROCEDURE GetBudgetListAndLineItemBudgetList
@ClientId nvarchar(max)=''
,@BudgetId int=0 
AS 
BEGIN
SET NOCOUNT ON;
-- Budget List
SELECT BudgetDetail.Id
,CASE WHEN BudgetDetail.ParentId IS NULL THEN 0 ELSE
(CASE WHEN BudgetDetail.Id=BudgetDetail.BudgetId THEN 0 ELSE BudgetDetail.ParentId END) END AS ParentId
,BudgetDetail.Name
,'' AS 'Weightage'
FROM Budget_Detail AS BudgetDetail WITH (NOLOCK)
CROSS APPLY(SELECT Id,BudgetId FROM Budget AS Budget WITH (NOLOCK) WHERE ClientId=@ClientId AND Budget.IsDeleted=0 AND Budget.Id=BudgetDetail.BudgetId)Budget
WHERE 
BudgetDetail.IsDeleted=0
AND
((CASE WHEN @BudgetId>0 THEN BudgetDetail.BudgetId END)=@BudgetId
OR(CASE WHEN @BudgetId=0 THEN BudgetDetail.BudgetId END)IN(BudgetDetail.BudgetId))

-- Budget Line Item List
SELECT LineItemBudget.* FROM [LineItem_Budget] LineItemBudget WITH (NOLOCK)
CROSS APPLY(SELECT Id,BudgetId FROM Budget_Detail BudgetDetail WITH (NOLOCK) WHERE 
BudgetDetail.IsDeleted=0 AND
((CASE WHEN @BudgetId>0 THEN BudgetDetail.BudgetId END)=@BudgetId
OR(CASE WHEN @BudgetId=0 THEN BudgetDetail.BudgetId END)IN(BudgetDetail.BudgetId))
AND BudgetDetail.Id=LineItemBudget.BudgetDetailId)BudgetDetail
CROSS APPLY(SELECT Id,BudgetId FROM Budget AS Budget WITH (NOLOCK) WHERE ClientId=@ClientId AND Budget.IsDeleted=0 AND Budget.Id=BudgetDetail.BudgetId)Budget
END
GO
-- Add By Nishant Sheth
-- Desc :: Add column for marketo integration
IF COL_LENGTH('Plan_Campaign_Program_Tactic', 'IsSyncMarketo') IS NULL
BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic
        ADD [IsSyncMarketo] BIT NULL
END
GO
IF COL_LENGTH('Plan_Campaign_Program_Tactic','IntegrationInstanceMarketoID') IS NULL
BEGIN
	ALTER TABLE [Plan_Campaign_Program_Tactic] ADD [IntegrationInstanceMarketoID] NVARCHAR(50) NULL
END
GO
IF COL_LENGTH('Model','IntegrationInstanceMarketoID') IS NULL
BEGIN
	ALTER TABLE [Model] ADD [IntegrationInstanceMarketoID] INT NULL -- Add Column
	-- Add reference
	ALTER TABLE [Model]  WITH CHECK ADD  CONSTRAINT [FK_Model_IntegrationInstanceMarketo] FOREIGN KEY([IntegrationInstanceMarketoID])
	REFERENCES [IntegrationInstance] ([IntegrationInstanceId])
 END
GO


-- Created by Komal Rawal 
-- Created on :: 02-May-2016
-- Desc :: Delete LastViewedData

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteLastViewedData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteLastViewedData]
GO
CREATE PROCEDURE DeleteLastViewedData
@UserId nvarchar(max) = null,
@PreviousIds nvarchar(max) = null
AS
BEGIN
SET NOCOUNT ON;
DECLARE @CheckIsAlreadyDeleted  INT = 0
SELECT @CheckIsAlreadyDeleted  = COUNT(*) FROM [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL AND Id IN(SELECT CONVERT(INT,val) From [dbo].[comma_split](@PreviousIds,','))
IF(@CheckIsAlreadyDeleted>0)
BEGIN

DELETE FROM  [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL AND Id IN(SELECT CONVERT(INT,val) From [dbo].[comma_split](@PreviousIds,','))

END
ELSE
BEGIN
	DELETE FROM  [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL 	
END
END
GO

-- Add By Nishant Sheth
-- Desc : Create Table Type for pass table as argument in stored procedure
-- Created Date: 16-May-2016
IF TYPE_ID(N'SalesforceType') IS  NULL 
BEGIN
/* Create a table type. */
CREATE TYPE SalesforceType AS TABLE
(
Id NVARCHAR(MAX) NULL,
Name NVARCHAR(MAX) NULL
)
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateSalesforceIdForMarketoTactic]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateSalesforceIdForMarketoTactic]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nihshant Sheth
-- Create date: 16-May-2016
-- Description:	Update SalesforceId for marketo's tactic
-- =============================================
CREATE PROCEDURE UpdateSalesforceIdForMarketoTactic
@SalesForce SalesforceType ReadOnly
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Tactic SET Tactic.IntegrationInstanceTacticId=Salesforce.Id FROM Plan_Campaign_Program_Tactic Tactic
	CROSS APPLY (SELECT Id,Name FROM @SalesForce Salesforce WHERE Salesforce.Name=Tactic.TacticCustomName) Salesforce
	WHERE Tactic.IsDeleted=0 AND Tactic.IntegrationInstanceMarketoID IS NOT NULL
	AND Tactic.IsSyncSalesForce=1 AND Tactic.IntegrationInstanceTacticId IS NULL
	AND Tactic.IsDeployedToIntegration=1
	AND Tactic.Status IN('Approved','In-Progress','Complete')
END
GO
-- =============================================
-- Author:		Rahul Shah
-- Create date: 19-May-2016
-- Description:	insert Markto type in integration type table
-- =============================================
GO
DECLARE @Title nvarchar (50) = 'Marketo'
DECLARE @Description nvarchar (50) = null
DECLARE @isDeleted  bit = 0
DECLARE @APIVersion nvarchar (50) = null
DECLARE @APIURL nvarchar (50) = 'https://138-cmd-587.mktorest.com/'
DECLARE @Code nvarchar (50) = 'Marketo'
DECLARE @FrontEndUrl nvarchar (50) = 'https://login.marketo.com'

IF (NOT EXISTS(SELECT * FROM [IntegrationType] WHERE Code like @Code and isDeleted = @isDeleted)) 
BEGIN 

	INSERT INTO [IntegrationType] (Title,Description,IsDeleted,APIVersion,APIURL,Code,FrontEndUrl) VALUES (@Title,@Description,@isDeleted,@APIVersion,@APIURL,@Code,@FrontEndUrl);
End
ElSE
BEGIN 

	UPDATE [IntegrationType] SET Title = @Title,Description = @Description,APIVersion = @APIVersion,APIURL = @APIURL,FrontEndUrl= @FrontEndUrl WHERE Code like @Code and isDeleted = @isDeleted;
End
GO
-- =============================================
-- Author:		Rahul Shah
-- Create date: 19-May-2016
-- Description:	insert Markto Attributes in integration type Attribute table
-- =============================================
GO
--Please do not make any change below variables and its values.    
DECLARE @IntegrationRow int=0
DECLARE @RowCount as int=1
DECLARE @IntegrationTypeIdValue int
DECLARE @Code nvarchar (50) = 'Marketo'
DECLARE @Attribute1 nvarchar (50) = 'ClientId'
DECLARE @Attribute2 nvarchar (50) = 'ClientSecret'
DECLARE @AttributeType nvarchar (50) = 'textbox'
DECLARE @isDeleted  bit = 0
SELECT @IntegrationTypeIdValue = IntegrationTypeId FROM IntegrationType WHERE Code=@Code
	IF NOT EXISTS (SELECT * FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @IntegrationTypeIdValue AND Attribute = @Attribute1 AND isDeleted = @isDeleted)
	BEGIN
		INSERT INTO IntegrationTypeAttribute (IntegrationTypeId,Attribute,AttributeType,IsDeleted) 
			VALUES(@IntegrationTypeIdValue,@Attribute1,@AttributeType,@isDeleted)
	END
	
	IF NOT EXISTS (SELECT * FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @IntegrationTypeIdValue AND Attribute = @Attribute2 AND isDeleted = @isDeleted)
	BEGIN

		INSERT INTO IntegrationTypeAttribute (IntegrationTypeId,Attribute,AttributeType,IsDeleted) 
			 VALUES(@IntegrationTypeIdValue,@Attribute2,@AttributeType,@isDeleted)
	END
GO
-- =============================================
-- Author:		Rahul Shah
-- Create date: 19-May-2016
-- Description:	insert Markto Data in GameplanDatatypeTable
-- =============================================
GO
--Please do not make any change below variables and its values.    
DECLARE @IntegrationRow int=0
DECLARE @RowCount as int=1
DECLARE @IntegrationTypeIdValue int
DECLARE @Code nvarchar (50) = 'Marketo'
DECLARE @TableName_Global nvarchar (50) = 'Global'
DECLARE @TableName_Tactic nvarchar (50) = 'Plan_Campaign_Program_Tactic'
DECLARE @ExternaleName_Actual nvarchar (50) = 'Name' 
DECLARE @ExternaleName_Display nvarchar (50) = 'External Name'
DECLARE @Description nvarchar (50) = 'Description' 
DECLARE @ProgramTypeActual nvarchar (50) = 'ProgramType' 
DECLARE @ProgramTypeDisplay nvarchar (50) = 'Program Type' 
DECLARE @Channel nvarchar (50) = 'Channel'         
DECLARE @status nvarchar (50) = 'Status'
DECLARE @ownerActual nvarchar (50) = 'CreatedBy'
DECLARE @ownerDisplay nvarchar (50) = 'Owner'
DECLARE @ActivityTypeActual nvarchar (50) = 'ActivityType'
DECLARE @ActivityTypeDisplay nvarchar (50) = 'Activity Type'
DECLARE @planNameActual nvarchar (50) = 'PlanName'
DECLARE @planNameDisplay nvarchar (50) = 'Plan Name'
DECLARE @costPlanned_Actual nvarchar (50) = 'Cost'
DECLARE @costPlanned_Display nvarchar (50) = 'Cost(Planned)'
DECLARE @tacticTypeActual nvarchar (50) = 'TacticType'
DECLARE @tacticTypeDisplay nvarchar (50) = 'Tactic Type'

DECLARE @isGet  bit = 0
DECLARE @isDeleted  bit = 0
DECLARE @isImprovement  bit = 0

SELECT @IntegrationTypeIdValue = IntegrationTypeId FROM IntegrationType WHERE Code=@Code
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @ExternaleName_Actual AND DisplayFieldName = @ExternaleName_Display AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@ExternaleName_Actual,@ExternaleName_Display,@isGet,@isDeleted,@isImprovement)
	END
	---Insert script for Channel 2 
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @Description AND DisplayFieldName = @Description AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@Description,@Description,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for Progran Type 1
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @ProgramTypeActual AND DisplayFieldName = @ProgramTypeDisplay AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@ProgramTypeActual,@ProgramTypeDisplay,@isGet,@isDeleted,@isImprovement)
	END
	---Insert script for Channel 2 
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @Channel AND DisplayFieldName = @Channel AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@Channel,@Channel,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for status 3 
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @status AND DisplayFieldName = @status AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@status,@status,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for owner 4
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @ownerActual AND DisplayFieldName = @ownerDisplay AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@ownerActual,@ownerDisplay,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for Activity Type 5
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @ActivityTypeActual AND DisplayFieldName = @ActivityTypeDisplay AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@ActivityTypeActual,@ActivityTypeDisplay,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for Plan Name 6
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Global AND ActualFieldName = @planNameActual AND DisplayFieldName = @planNameDisplay AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Global,@planNameActual,@planNameDisplay,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for Tactic Planned Cost 7
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Tactic AND ActualFieldName = @costPlanned_Actual AND DisplayFieldName = @costPlanned_Display AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN	
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Tactic,@costPlanned_Actual,@costPlanned_Display,@isGet,@isDeleted,@isImprovement)
	END

	---Insert script for Tactic Activity Type 8
	IF NOT EXISTS (SELECT * FROM GameplanDataType WHERE IntegrationTypeId = @IntegrationTypeIdValue AND TableName = @TableName_Tactic AND ActualFieldName = @tacticTypeActual AND DisplayFieldName = @tacticTypeDisplay AND IsGet = @isGet AND isDeleted = @isDeleted AND IsImprovement = @isImprovement)
	BEGIN	
		INSERT INTO GameplanDataType (IntegrationTypeId,TableName,ActualFieldName,DisplayFieldName,IsGet,IsDeleted,IsImprovement) 
			VALUES(@IntegrationTypeIdValue,@TableName_Tactic,@tacticTypeActual,@tacticTypeDisplay,@isGet,@isDeleted,@isImprovement)
	END
GO

-- =============================================
-- Author: Komal Rawal
-- Create date: 19-May-2016
-- Description:	Create table for MarketoEntity Mapping
-- =============================================
IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'MarketoEntityValueMapping'))
BEGIN
CREATE TABLE [dbo].[MarketoEntityValueMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EntityID] [int] NULL,
	[EntityType] [nvarchar](255) NULL,
	[MarketoCampaignFolderId] [nvarchar](255) NULL,
	[ProgramType] [nvarchar](1000) NULL,
	[Channel] [nvarchar](1000) NULL,
	[IntegrationInstanceId] [int] NULL,
	[LastModifiedBy] [uniqueidentifier] NULL,
	[LastModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_MarketoEntityValueMapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[MarketoEntityValueMapping]  WITH CHECK ADD  CONSTRAINT [FK_MarketoEntityValueMapping_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[MarketoEntityValueMapping] CHECK CONSTRAINT [FK_MarketoEntityValueMapping_IntegrationInstance]

END

Go

-- =============================================
-- Created By Nishant Sheth
-- Created Date : 20-May-2016
-- Description : Remove Space and make word Captialize and remove special character
-- =============================================
IF object_id(N'RemoveSpaceAndUppercaseFirst', N'FN') IS NOT NULL
    DROP FUNCTION RemoveSpaceAndUppercaseFirst
GO
GO
/****** Object:  UserDefinedFunction [dbo].[RemoveSpaceAndUppercaseFirst]    Script Date: 05/20/2016 12:43:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[RemoveSpaceAndUppercaseFirst] ( @InputString varchar(MAX) ) 
RETURNS VARCHAR(MAX)
AS
BEGIN

DECLARE @Index          INT
DECLARE @Char           CHAR(1)
DECLARE @PrevChar       CHAR(1)
DECLARE @OutputString   VARCHAR(255)

SET @InputString=REPLACE(@InputString,'&amp;','&')
SET @InputString=REPLACE(@InputString,'&lt;','<')
SET @InputString=REPLACE(@InputString,'&gt;','>')

SET @OutputString = LOWER(@InputString)
SET @Index = 1

WHILE @Index <= LEN(@InputString)
BEGIN
    SET @Char     = SUBSTRING(@InputString, @Index, 1)
    SET @PrevChar = CASE WHEN @Index = 1 THEN ' '
                         ELSE SUBSTRING(@InputString, @Index - 1, 1)
                    END

    IF @PrevChar IN (' ', ';', ':', '!', '?', ',', '.', '_', '-', '/', '''', '(')
    BEGIN
        IF @PrevChar != '''' OR UPPER(@Char) != 'S'
            SET @OutputString = STUFF(@OutputString, @Index, 1, UPPER(@Char))
    END

    SET @Index = @Index + 1

END

SET @OutputString=REPLACE(@OutputString,' ','')

DECLARE @KeepValues AS VARCHAR(MAX)
SET @KeepValues = '%[^a-z0-9A-Z_]%'
WHILE PATINDEX(@KeepValues, @OutputString) > 0
SET @OutputString = STUFF(@OutputString, PATINDEX(@KeepValues, @OutputString), 1, '')


RETURN @OutputString

END
GO

-- Updated by Akashdeep Kadia 
-- Created on :: 21-May-2016
-- Desc :: Special charachter En Dash replace with Hyphen in Export to CSV. 

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
Update CustomField set Name =REPLACE(Name,'–','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
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

SELECT @Colname = ColName FROM #tblColName WHERE ROWNUM=@Count
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
set @release = 'May30.2016'
set @version = 'May30.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO