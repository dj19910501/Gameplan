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
	CROSS APPLY (SELECT Id,Name FROM @SalesForce Salesforce WHERE Salesforce.Name=Tactic.Title) Salesforce
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
	[MarketoCampaignFolderName] [nvarchar](max) NULL,
	[IsDeleted] [bit] NULL CONSTRAINT [DF_MarketoEntityValueMapping_IsDeleted]  DEFAULT ((0)),
 CONSTRAINT [PK_MarketoEntityValueMapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

Go

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