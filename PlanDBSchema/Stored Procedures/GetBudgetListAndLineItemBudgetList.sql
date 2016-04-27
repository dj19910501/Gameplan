-- EXEC GetBudgetListAndLineItemBudgetList '464EB808-AD1F-4481-9365-6AADA15023BD',0
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