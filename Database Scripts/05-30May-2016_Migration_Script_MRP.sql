-- ===================================================================================================
-- Added By : Viral Kadiya
-- Added Date : 05/26/2016
-- ===========================================================================================================

-- Note: Please update ClientId,Sequence & length variable value as per requirement

DECLARE @MarketoClientId NVARCHAR(100)='464EB808-AD1F-4481-9365-6AADA15043BD'
DECLARE @sequence int=1
DECLARE @length int=5
IF NOT EXISTS(SELECT CampaignNameConventionId FROM CampaignNameConvention WHERE [TableName]='Plan_Campaign_Program_Tactic' AND [FieldName]='PlanTacticId' AND ClientId=@MarketoClientId AND IsDeleted=0)
BEGIN

       UPDATE CampaignNameConvention SET [Sequence] = [Sequence]+1
       WHERE ClientId=@MarketoClientId

       Insert Into CampaignNameConvention Values('Plan_Campaign_Program_Tactic','PlanTacticId',null,@sequence,@MarketoClientId,GetDate(),@MarketoClientId,0,@length)
END
GO


-- ==========================================================================================================

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
-- Updated on :: 01-June-2016
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
Update CustomField set Name =REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
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

-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================

/****** Object:  UserDefinedFunction [dbo].[GetTacticResultColumns]    Script Date: 05/24/2016 6:59:20 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticResultColumns]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[GetTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN

declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

	;With ResultTable as(

(
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
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
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=''Tactic''
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select DISTINCT ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END
' 
END

GO
-- ========================================================================================================
-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================
/****** Object:  UserDefinedFunction [dbo].[GetTacCustomNameMappingList]    Script Date: 05/26/2016 9:45:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacCustomNameMappingList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacCustomNameMappingList]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacCustomNameMappingList]    Script Date: 05/26/2016 9:45:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacCustomNameMappingList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetTacCustomNameMappingList]
(
	@entityType varchar(255)=''Tactic'',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)=''''
)

--SELECT * from  GetTacCustomNameMappingList(''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''94028,94016'')
RETURNS @tac_Type_Cust Table(
PlanTacticId int,
CustomName varchar(max)
)
AS
BEGIN
	
--	Declare @entityType varchar(255)=''Tactic''
--Declare @clientId uniqueidentifier=''464EB808-AD1F-4481-9365-6AADA15023BD''
--Declare @TacticIds varchar(max)=''''
Declare @actTitleField varchar(50)=''Title''
Declare @actPlanTacticIdField varchar(50)=''PlanTacticId''

Declare @tbl_Tac_Custm_Type table(
PlanTacticId int,
TacticTitle varchar(max),
CustomFieldId int,
CustomFieldValue varchar(max),
TacticType varchar(max),
TableName varchar(1000),
[Sequence] int
)


;WITH tacTitle as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''Plan_Campaign_Program_Tactic'' as TableName
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
tacType as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''TacticType'' as TableName,
	T.TacticTypeId
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
EntityTableWithValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as ValueV, 
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
INSERT INTO @tbl_Tac_Custm_Type
SElect * from (

-- START: Tactic Title ---
(SELECT 
	T.PlanTacticId,
	CASE 
		WHEN CNC.CustomNameCharNo is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](CASE WHEN FieldName=@actPlanTacticIdField THEN Cast(T.PlanTacticId as varchar(10)) ELSE T.Title END) +''_'' 
		ELSE SUBSTRING([dbo].[RemoveSpaceAndUppercaseFirst](CASE WHEN FieldName=@actPlanTacticIdField THEN Cast(T.PlanTacticId as varchar(10)) ELSE T.Title END),1,CNC.CustomNameCharNo) +''_'' 
	END as ''TacticTitle'',
	Null as CustomFieldId,
	Null as CustomFieldValue,
	Null as TacticType,
	CNC.TableName,
	CNC.[Sequence]
FROM tacTitle as T
Inner JOIN CampaignNameConvention as CNC on T.TableName = CNC.TableName and CNC.IsDeleted=0 and CNC.ClientId=@clientId
where T.PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
)
-- END: Tactic Title ---
UNION 
-- START: Tactic Type ---
(SELECT 
	T.PlanTacticId,
	NULL as ''TacticTitle'',
	Null as CustomFieldId,
	Null as CustomFieldValue,
	CASE 
		WHEN CNC.CustomNameCharNo is null THEN 
											CASE 
												WHEN TP.Abbreviation is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](TP.Title) +''_'' 
												ELSE [dbo].[RemoveSpaceAndUppercaseFirst](TP.Abbreviation) +''_''
											END
		ELSE SUBSTRING(
						CASE 
							WHEN TP.Abbreviation is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](TP.Title)
							ELSE [dbo].[RemoveSpaceAndUppercaseFirst](TP.Abbreviation)
						END
					   ,1,CNC.CustomNameCharNo) +''_'' 
	END as TacticType,
	CNC.TableName,
	CNC.[Sequence]
FROM tacType as T
Inner JOIN TacticType as TP ON T.TacticTypeId = TP.TacticTypeId and TP.IsDeleted=0
Inner JOIN CampaignNameConvention as CNC on T.TableName = CNC.TableName and CNC.IsDeleted=0 and CNC.ClientId=@clientId
where T.PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
)
-- END: Tactic Type ---
UNION 

--order by keyv
--select * from EntityTableWithValues;

(SELECT 
	    E.EntityId as PlanTacticId,
		NULL as ''TacticTitle'',
		E.CustomFieldId as CustomFieldId,
		CASE 
			WHEN CNC.CustomNameCharNo is null THEN E.CustomName +''_'' 
			ELSE SUBSTRING(E.CustomName,1,CNC.CustomNameCharNo) +''_'' 
		END as CustomFieldValue,
		Null as TacticType,
		CNC.TableName,
		CNC.[Sequence]
FROM EntityTableWithValues as E
Inner JOIN CampaignNameConvention as CNC on E.CustomFieldId = CNC.CustomFieldId and CNC.IsDeleted=0 and CNC.ClientId=@clientId
WHERE E.EntityId IN (select val from comma_split(@EntityIds,'',''))
)
) as tac_type_custm
Order by PlanTacticId,[Sequence]
;
--select * from @tbl_Tac_Custm_Type
INSERT INTO @tac_Type_Cust
Select Main.PlanTacticId,
       Left(Main.[subcustm],Len(Main.[subcustm])-1) As CustomName
From
    (
 SELECT distinct ST2.PlanTacticId, 
            (
                SELECT ST1.CustomName + '''' AS [text()]
                FROM (
						SELECT T1.PlanTacticId,
							CASE 
								WHEN T1.TableName =''CustomField'' THEN T1.CustomFieldValue 
								WHEN T1.TableName =''Plan_Campaign_Program_Tactic'' THEN T1.TacticTitle 
								WHEN T1.TableName =''TacticType'' THEN T1.TacticType 
							 END  as CustomName	
						FROM @tbl_Tac_Custm_Type T1
					) ST1
                Where ST1.PlanTacticId = ST2.PlanTacticId
                ORDER BY ST1.PlanTacticId
                For XML PATH ('''')
            ) [subcustm]
        From (
				SELECT T1.PlanTacticId,
							CASE 
								WHEN T1.TableName =''CustomField'' THEN T1.CustomFieldValue 
								WHEN T1.TableName =''Plan_Campaign_Program_Tactic'' THEN T1.TacticTitle 
								WHEN T1.TableName =''TacticType'' THEN T1.TacticType 
							 END  as CustomName	
					from @tbl_Tac_Custm_Type T1
) ST2
)Main
	
	RETURN 
END
' 
END

GO

---=========================================================================================
-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================
/****** Object:  UserDefinedFunction [dbo].[GetSourceTargetMappingData]    Script Date: 05/24/2016 6:59:20 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSourceTargetMappingData]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSourceTargetMappingData]
(
	@entityType varchar(255)=''Tactic'',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255	-- default value 255
)

--SELECT * from  [GetSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''94016,94028,94029,94030'',3,1176,255)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS
BEGIN

Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
Declare @ColumnName nvarchar(max)
Declare @actCampaignFolder varchar(255)=''CampaignFolder''
Declare @trgtCampaignFolder varchar(255)=''Id''
Declare @actMarketoProgramId varchar(255)=''MarketoProgramId''
Declare @trgtMarketoProgramId varchar(255)=''''
Declare @actMode varchar(255)=''Mode''
Declare @trgtMode varchar(255)=''''
Declare @modeCREATE varchar(20)=''Create''
Declare @modeUPDATE varchar(20)=''Update''
Declare @actNote varchar(255)=''Note''
Declare @trgtNote varchar(255)=''note''
Declare @actCostStartDate varchar(255)=''CostStartDate''
Declare @trgtCostStartDate varchar(255)=''CostStartDate''
Declare @actCreatedBy varchar(255)=''CreatedBy''
Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	
	(
			Select  IntegrationInstanceID,
					IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
					TableName,
					gpDataType.ActualFieldName,
					TargetDataType,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId
			FROM GamePlanDataType as gpDataType
			JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
			Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
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
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	
	)
	insert into @Table 
	select * from ResultTable
END
-------- END: Get Standard & CustomField Mappings data --------
-------- START: Insert fixed Marketo fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMarketoProgramId as ActualFieldName,@trgtMarketoProgramId as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,@trgtMode as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCampaignFolder as ActualFieldName,@trgtCampaignFolder as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actNote as ActualFieldName,@trgtNote as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCostStartDate as ActualFieldName,@trgtCostStartDate as TargetDataType,0 as CustomFieldId
END
-------- END: Insert fixed Marketo fields to Mapping list. -------- 

;WITH tact as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''Tactic_Mapp'' as Link
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Tactic_Mapp'' as Link
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
select Mapp.ActualFieldName,
		Mapp.CustomFieldId,
		 CASE 
			WHEN Mapp.ActualFieldName=@actMarketoProgramId THEN ISNull(Tac.IntegrationInstanceMarketoID,'''')
			WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
														WHEN ISNULL(Tac.IntegrationInstanceMarketoID,'''')='''' THEN @modeCREATE 
														ELSE @modeUPDATE 
													 END)  
			WHEN Mapp.ActualFieldName=''ProgramType'' THEN ISNull(marktType.ProgramType,'''')
			WHEN Mapp.ActualFieldName=''Channel'' THEN ISNull(marktType.Channel,'''')
			WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
			WHEN Mapp.ActualFieldName=''ActivityType'' THEN ''Tactic''
			WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
			WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
			WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
			WHEN Mapp.ActualFieldName=''Name'' THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
			WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
			WHEN Mapp.ActualFieldName=@actCampaignFolder THEN ISNull(marktFolder.MarketoCampaignFolderId,'''')
			WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,''None'')
			WHEN Mapp.ActualFieldName=@actNote THEN ''Planned Cost''
			WHEN Mapp.ActualFieldName=@actCostStartDate THEN ISNull(CONVERT(VARCHAR(19),Tac.StartDate),'''')
			WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
		END AS TacValue,
		T.PlanTacticId as SourceId
from IntegMapp as Mapp
INNER JOIN tact as T ON Mapp.Link = T.Link
LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.PlanTacticId = Tac.PlanTacticId
LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.PlanTacticId = custm.EntityId
LEFT JOIN MarketoEntityValueMapping as marktFolder ON pln.PlanId = marktFolder.EntityID and marktFolder.EntityType=''Plan'' and marktFolder.IntegrationInstanceId =@id 
LEFT JOIN MarketoEntityValueMapping as marktType ON TT.TacticTypeId = marktType.EntityID and marktType.EntityType=''TacticType'' 
	RETURN 
END' 
END

GO
----=========================================================================================
-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================

/****** Object:  StoredProcedure [dbo].[spGetMarketoData]    Script Date: 05/27/2016 8:38:56 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetMarketoData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetMarketoData]
GO
/****** Object:  StoredProcedure [dbo].[spGetMarketoData]    Script Date: 05/27/2016 8:38:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetMarketoData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetMarketoData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetMarketoData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
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
		Declare @colCampgnFolder varchar(255)='CampaignFolder'
		Declare @actNote varchar(255)='Note'
		Declare @actCostStartDate varchar(255)='CostStartDate'
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables
	END
	-- END: Declare local variables
	

	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								TacticCustomName varchar(max),
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Tactic or Integration Instance')
		
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
									tact.TacticCustomName,
									tact.LinkedTacticId ,
									tact.LinkedPlanId,
									pln.[Year] as PlanYear
							from [Model] as mdl
							join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
							Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
							join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
							join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncMarketo='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
							where  mdl.IntegrationInstanceMarketoID=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
						),
						 tactList AS (
							(
								SELECT tact1.PlanTacticId,
										tact1.PlanProgramId,
										tact1.TacticCustomName,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											RN= 1
								FROM tblTact as tact1 
								WHERE IsNull(Tact1.LinkedTacticId,0) <=0
							 )
							 UNION
							 (
								SELECT tact1.PlanTacticId,
										tact1.PlanProgramId,
										tact1.TacticCustomName,
										tact1.LinkedTacticId ,
										tact1.LinkedPlanId,
										tact1.PlanYear,
										-- Get latest year tactic
										RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																				WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																				ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																			 END 
																ORDER BY PlanYear DESC) 
								FROM tblTact as tact1 
								WHERE (tact1.LinkedTacticId > 0)
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
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			BEGIN TRY
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
						tact.TacticCustomName,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						1 as RN
				FROM Plan_Campaign_Program_Tactic as tact 
				WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncMarketo='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceMarketoID
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

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId

			-- Update LastSync Status of Integration Instance
			--Update IntegrationInstance set LastSyncStatus = @syncStatusInProgress where IntegrationInstanceId=@instanceId
			
		END
		-- END: Get IntegrationTypeId

		

		-- START: GET result data based on Mapping fields
		BEGIN
			IF EXISTS(Select PlanTacticId from @tblTaclist)
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						select  @ColumnName = dbo.GetTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					-- START: Get TacticIds
					SELECT @entIds= ISNULL(@entIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist) AS PlanTacticIds
					-- END: Get TacticIds
					

					----- START: Get Updte CustomName TacIds -----
					SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where IsNull(TacticCustomName,'')='') AS PlanTacticIds
					----- END: Get Updte CustomName TacIds -----
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

					BEGIN TRY
						-- START: Update Tactic Name --
						UPDATE Plan_Campaign_Program_Tactic 
						SET TacticCustomName = T1.CustomName 
						FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
						INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId
						-- END: Update Tactic Name --
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')

					--SELECT * from  [GetSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'94016,94028,94029,94030',3,1176)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT MarketoProgramId,SourceId,Mode,[CampaignFolder],'+@actNote+','+@actCostStartDate+','+@ColumnName+' 
																FROM (
																		SELECT ActualFieldName,TacValue,SourceId 
																		FROM [GetSourceTargetMappingData](''Tactic'','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''')) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (MarketoProgramId,Mode,[CampaignFolder],'+@actNote+','+@actCostStartDate+','+@ColumnName+')
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
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for marketo instance')
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
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push marketo')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END


GO


----===============================================================================================================

-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================

/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/30/2016 7:56:15 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/30/2016 7:56:15 PM ******/
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

-- ===================================================================================================
-- Added By : Viral Kadiya
-- Added Date : 05/24/2016
-- ===========================================================================================================
/****** Object:  StoredProcedure [dbo].[GetFieldMappings]    Script Date: 05/25/2016 4:42:53 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFieldMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFieldMappings]
GO
/****** Object:  StoredProcedure [dbo].[GetFieldMappings]    Script Date: 05/25/2016 4:42:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFieldMappings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetFieldMappings] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetFieldMappings] 
	@entityType varchar(255)='Tactic',
	@clientId uniqueidentifier,
	@integrationTypeId int,
	@id int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Exec GetFieldMappings 'Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',3,1190

BEGIN
	Declare @Table TABLE (sourceFieldName NVARCHAR(250),destinationFieldName NVARCHAR(250),marketoFieldType varchar(255))
	Declare @ColumnName nvarchar(max)
	Declare @actCampaignFolder varchar(255)='CampaignFolder'
	Declare @trgtCampaignFolder varchar(255)='Id'
	Declare @actNote varchar(255)='Note'
	Declare @trgtNote varchar(255)='note'
	Declare @actMarketoProgramId varchar(255)='MarketoProgramId'
	Declare @trgtMarketoProgramId varchar(255)=''
	Declare @actCostStartDate varchar(255)='CostStartDate'
	Declare @trgtCostStartDate varchar(255)='CostStartDate'
	Declare @actMode varchar(255)='Mode'
	Declare @trgtMode varchar(255)=''
	Declare @modeCREATE varchar(20)='Create'
	Declare @modeUPDATE varchar(20)='Update'
	Declare @actCost varchar(30)='Cost'
	Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'
	Declare @varGlobal varchar(100)='Global'
	Declare @costFieldType varchar(255)='costs'
	Declare @tagsFieldType varchar(255)='tags'
	Declare @folderFieldType varchar(255)='Folder'
	Declare @actActivityType varchar(255)='ActivityType'
	Declare @actCreatedBy varchar(255)='CreatedBy'
	Declare @actPlanName varchar(255)='PlanName'
	Declare @actStatus varchar(255)='Status'

END

;With ResultTable as(

(
		-- Actualfield Query
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				CASE 
					WHEN gpDataType.ActualFieldName=@actCost THEN @costFieldType
					WHEN gpDataType.ActualFieldName=@actActivityType THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actCreatedBy THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actPlanName THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actStatus THEN @tagsFieldType
				END as marketoFieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@tblTactic OR gpDataType.TableName=@varGlobal) and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- CustomField Query
		SELECT  custm.Name as sourceFieldName,
				TargetDataType as destinationFieldName,
				@tagsFieldType as marketoFieldType
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
--INSERT INTO @Table SELECT @actMarketoProgramId as sourceFieldName,@trgtMarketoProgramId as destinationFieldName,Null as marketoFieldType

IF EXISTS (Select sourceFieldName from @Table where sourceFieldName =@actCost)
BEGIN
	INSERT INTO @Table SELECT @actNote as sourceFieldName,@trgtNote as destinationFieldName,@costFieldType as marketoFieldType
	INSERT INTO @Table SELECT @actCostStartDate as sourceFieldName,@trgtCostStartDate as destinationFieldName,@costFieldType as marketoFieldType
END
INSERT INTO @Table SELECT @actCampaignFolder as sourceFieldName,@trgtCampaignFolder as destinationFieldName,@folderFieldType as marketoFieldType

select * from @Table
END


GO
-- =========================================
-- Add By Nishant Sheth
-- Description :  Add Host field for marketo instance
-- Created Date : 26-May-2016
-- =========================================
 IF EXISTS(SELECT [IntegrationTypeId] FROM [IntegrationType] WHERE [Title]='Marketo' AND [IsDeleted]=0)
  BEGIN 
	DECLARE @IntegrationTypeId INT
	SELECT @IntegrationTypeId =[IntegrationTypeId] FROM [IntegrationType] WHERE [Title]='Marketo' AND [IsDeleted]=0 
	
	IF NOT EXISTS(SELECT [IntegrationTypeAttributeId] FROM [IntegrationTypeAttribute] WHERE [Attribute]='Host' AND [IsDeleted]=0 AND AttributeType='textbox' AND [IntegrationTypeId]=@IntegrationTypeId)
	BEGIN
		INSERT INTO [IntegrationTypeAttribute] (IntegrationTypeId,Attribute,AttributeType,IsDeleted)
		VALUES (@IntegrationTypeId,'Host','textbox',0)

		DECLARE @IntegrationTypeAttributeId INT

		SELECT @IntegrationTypeAttributeId=
		IntegrationTypeAttributeId FROM IntegrationTypeAttribute
		WHERE Attribute='Host' AND AttributeType='textbox' AND IsDeleted=0
		AND IntegrationTypeId=@IntegrationTypeId

		INSERT INTO IntegrationInstance_Attribute
		SELECT DISTINCT(IntegrationInstance_Attribute.IntegrationInstanceId)
		,@IntegrationTypeAttributeId,'',GETDATE(),IntegrationInstance_Attribute.CreatedBy 
		FROM IntegrationInstance_Attribute
		INNER JOIN IntegrationInstance ON IntegrationInstance.IntegrationInstanceId=IntegrationInstance_Attribute.IntegrationInstanceId
		WHERE  IntegrationInstance.IntegrationTypeId=@IntegrationTypeId
		AND IntegrationInstance_Attribute.IntegrationInstanceId NOT IN (SELECT DISTINCT(IntegrationInstance_Attribute.IntegrationInstanceId)
		FROM IntegrationInstance_Attribute
		WHERE IntegrationInstance_Attribute.IntegrationTypeAttributeId  = @IntegrationTypeAttributeId) 

	END
  END

  Go

-- =============================================
-- Author: Nishant Sheth
-- Create date: 27-May-2016
-- Description:	Create table for Integration Attributr
-- =============================================
IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'EntityIntegration_Attribute'))
BEGIN
CREATE TABLE [dbo].[EntityIntegration_Attribute](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EntityId] [int] NOT NULL,
	[EntityType] [nvarchar](255) NOT NULL,
	[IntegrationinstanceId] [int] NOT NULL,
	[AttrType] [nvarchar](255) NOT NULL,
	[AttrValue] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_EntityIntegration_Attribute] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



ALTER TABLE [dbo].[EntityIntegration_Attribute]  WITH CHECK ADD  CONSTRAINT [FK_EntityIntegration_Attribute_IntegrationInstance] FOREIGN KEY([IntegrationinstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])

ALTER TABLE [dbo].[EntityIntegration_Attribute] CHECK CONSTRAINT [FK_EntityIntegration_Attribute_IntegrationInstance]

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
set @release = 'May30.2016'
set @version = 'May30.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO