USE [Hive9ProdGP]
GO
/****** Object:  StoredProcedure [dbo].[DeleteMarketingBudget]    Script Date: 12/07/2016 15:04:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Rahul Shah
-- Create date: 11/30/2016
-- Description:	SP to Delete Marketing Budget data
-- =============================================
--DeleteMarketingBudget 1011750,4
ALTER  PROCEDURE [dbo].[DeleteMarketingBudget]
@BudgetDetailId int = 0,
@ClientId INT = 0
AS 

BEGIN
	SET NOCOUNT ON
	--Check the cross client and thorws an error 
	Declare @TempClientId INT=0
	SELECT @TempClientId= ClientId FROM Budget B INNER JOIN Budget_Detail bd ON bd.BudgetId=b.Id
	WHERE bd.Id=@BudgetDetailId
	--print(@TempClientId)
	IF(@ClientId<>@TempClientId)
	BEGIN
		RAISERROR ('You don''t have permission to delete this budget.', 16, 1);
	END

	Declare @ErrorMeesgae varchar(3000) = ''
	Declare @OtherBudgetId int = 0
	Declare @ParentIdCount int = 0
	Declare @LineItemBudgetCount int = 0
	Declare @NextBudgetId int = 0
	Declare @RowCount int = 0
	Declare @BudgetDetailData Table (Id int, ParentId int null, BudgetId int )
	BEGIN TRY
		-- GET 'N' Level Heirarchy for selected budget from budget detail table
		;WITH BudgetCTE AS
		( 
			SELECT Id
			,ParentId
			,BudgetId
			,IsDeleted
			FROM [dbo].[Budget_Detail] 
			WHERE ParentId = @BudgetDetailId OR Id = @BudgetDetailId
			AND IsDeleted=0 OR IsDeleted is null
			UNION ALL
			SELECT a.Id
			,a.ParentId
			,a.BudgetId
			,a.IsDeleted
			FROM [dbo].[Budget_Detail] a 
			INNER JOIN BudgetCTE s on a.ParentId=s.Id
			where Isnull(a.IsDeleted,0) = 0 and Isnull(s.IsDeleted,0) = 0
		)
		 
		INSERT INTO @BudgetDetailData SELECT Distinct Id,ParentId,BudgetId FROM BudgetCTE Order by ID asc	
		Select @RowCount = COUNT(Id) From @BudgetDetailData 
		IF @RowCount > 0 
		BEGIN
		-- check if any of the selected budgets is a root budget
		Select @ParentIdCount=Count(*) From @BudgetDetailData where ParentId is null

		--- if there is a parent/root budget found than delete that from budget table and get the next id of next budget that we should show on UI after deletion of root budget
		If @ParentIdCount > 0 
		BEGIN
			UPDATE [dbo].[Budget] SET IsDeleted=1 Where Id = (Select Top(1) BudgetId From @BudgetDetailData)	
			SELECT Top(1) @NextBudgetId = Id from Budget where ClientId=@ClientId and IsDeleted = 0 order by Name asc 	
		END
		
		-- delete budget from budget details table
		UPDATE [dbo].[Budget_Detail] SET IsDeleted=1 Where Id in (Select Id From @BudgetDetailData)
		
		Select @LineItemBudgetCount = Count(*) From [LineItem_Budget] where BudgetDetailId in (
				Select Id from @BudgetDetailData)
		
		-- If any of the selected budgets are linked to a line item, update respective Line item detail records with other budget id
		if @LineItemBudgetCount > 0
		BEGIN
		
		-- get Other budget Ids
			Select @OtherBudgetId = ChildDetail.Id From Budget_Detail ChildDetail
			INNER JOIN Budget ParentDetail on  ParentDetail.Id = ChildDetail.BudgetId
			where (ParentDetail.IsDeleted=0 OR ParentDetail.IsDeleted is null) and (ChildDetail.IsDeleted=0  OR ChildDetail.IsDeleted is null)
			and ParentDetail.IsOther=1
			and ParentDetail.ClientId=@ClientId
			IF(@OtherBudgetId Is Not Null)
			BEGIN
				Update [dbo].[LineItem_Budget] SET BudgetDetailId=@OtherBudgetId
					Where Id In(
					Select Id From [dbo].[LineItem_Budget] Where BudgetDetailId in (
						Select Id from @BudgetDetailData))
			END
		END
		
	END
	RETURN @NextBudgetId -- return next budget Id.
	
END TRY
BEGIN CATCH
	SET @ErrorMeesgae= 'Object=DeleteBudget , ErrorMessage='+ERROR_MESSAGE()
	SELECT @ErrorMeesgae as 'ErrorMessage'
END CATCH
END

