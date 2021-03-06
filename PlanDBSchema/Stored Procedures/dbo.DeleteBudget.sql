
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteBudget]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[DeleteBudget]
END
GO

/****** Object:  StoredProcedure [dbo].[DeleteBudget]    Script Date: 2/22/2016 4:20:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteBudget]
@BudgetDetailId int = 0,
@ClientId varchar(200)= null
AS 
BEGIN
 SET NOCOUNT ON
Declare @ErrorMeesgae varchar(3000) = ''
Declare @OtherBudgetDetailId int = 0
Declare @ParentIdCount int = 0
Declare @RowCount int = 0
BEGIN TRY
IF OBJECT_ID(N'tempdb..#BudgetDetailData') IS NOT NULL
BEGIN
  PRINT 'Table Exists'
  DROP TABLE #BudgetDetailData
END

Select @OtherBudgetDetailId = ChildDetail.Id From Budget_Detail ChildDetail
CROSS APPLY (Select * from Budget ParentDetail where ParentDetail.Id= ChildDetail.BudgetId) ParentDetail
Where (ParentDetail.IsDeleted=0 OR ParentDetail.IsDeleted is null) and (ChildDetail.IsDeleted=0  OR ChildDetail.IsDeleted is null)
and ParentDetail.IsOther=1
and ParentDetail.ClientId=@ClientId

;WITH BudgetCTE AS
( 
SELECT Id
--,Name 
,ParentId
,BudgetId
,IsDeleted
FROM [dbo].[Budget_Detail] 
WHERE ParentId = @BudgetDetailId OR Id = @BudgetDetailId
AND IsDeleted=0 OR IsDeleted is null
UNION ALL

SELECT a.Id
--, a.Name
, a.ParentId
,a.BudgetId
,a.IsDeleted
FROM [dbo].[Budget_Detail] a 
CROSS APPLY (Select * From BudgetCTE s Where a.ParentId=s.Id ) s
where a.IsDeleted=0 OR a.IsDeleted is null and s.IsDeleted=0 and s.IsDeleted is null
)
 
SELECT  Distinct Id,ParentId,BudgetId into #BudgetDetailData FROM BudgetCTE  Order by ID 

Select @RowCount = COUNT(Id) From #BudgetDetailData 
 IF @RowCount > 0 
 BEGIN
-- if parent then delete from budget table
Select @ParentIdCount=Count(*)  From #BudgetDetailData
where ParentId is null

If @ParentIdCount > 0 
BEGIN
	UPDATE [dbo].[Budget] SET IsDeleted=1 Where Id = (Select Top(1) BudgetId From #BudgetDetailData)

END

-- delete budget details
UPDATE [dbo].[Budget_Detail] SET IsDeleted=1 Where Id in (Select Id From #BudgetDetailData)

-- Update Line item details with other budget 
Update [dbo].[LineItem_Budget] SET BudgetDetailId=@OtherBudgetDetailId
	Where Id In(
	Select Id From [dbo].[LineItem_Budget] Where BudgetDetailId in (
		Select Id from #BudgetDetailData))
	END

Select @ErrorMeesgae as 'ErrorMessage'
--Select * From #BudgetDetailData

END TRY
BEGIN CATCH
	SET @ErrorMeesgae= 'Object=DeleteBudget , ErrorMessage='+ERROR_MESSAGE()
	Select @ErrorMeesgae as 'ErrorMessage'
END CATCH
END
