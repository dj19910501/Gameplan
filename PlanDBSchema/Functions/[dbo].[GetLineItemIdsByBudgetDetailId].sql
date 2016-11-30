-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetLineItemIdsByBudgetDetailId]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetLineItemIdsByBudgetDetailId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Viral
-- Create date: 22/11/2016
-- Description:	Returns Commaseparated LineItems group by BudgetDetailId
-- =============================================
CREATE FUNCTION [dbo].[GetLineItemIdsByBudgetDetailId]
(
	-- Add the parameters for the function here
	@BudgetId int 
)
RETURNS 
@tblLineItem TABLE 
(
	-- Add the column definitions for the TABLE variable here
	BudgetDetailId int,
	LineItemCount int,
	PlanLineItemIds varchar(max)
)
AS
BEGIN
		-- SELECT * FROM GetLineItemIdsByBudgetDetailId(2807)
	
		-- For child records, Get comma separated LineItemIds by BudgetDetailId 
		;WITH MyData AS
		(
			SELECT L.BudgetDetailId,L.PlanLineItemId 
			FROM LineItem_Budget L
			INNER JOIN Budget_Detail B ON B.Id = L.BudgetDetailId AND B.BudgetId = @BudgetId
			WHERE B.IsDeleted = 0
		)
		
		INSERT INTO @tblLineItem(BudgetDetailId,LineItemCount,PlanLineItemIds)
		SELECT BudgetDetailId,COUNT(*)as LineItemCount, PlanLineItemId = STUFF((
		    SELECT ', ' + CAST(PlanLineItemId AS VARCHAR) FROM MyData
		    WHERE BudgetDetailId = x.BudgetDetailId
		    FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)'), 1, 2, '')
		FROM MyData x
		GROUP BY BudgetDetailId;

	RETURN 
END
GO