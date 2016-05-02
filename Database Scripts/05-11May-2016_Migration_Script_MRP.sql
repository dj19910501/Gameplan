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
set @release = 'May11.2016'
set @version = 'May11.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO