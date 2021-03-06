IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalUserCountMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalUserCountMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jaymin Modi
-- Create date: 02/Dec/2016
-- Description:	Trigger which Update user count into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalUserCountMarketingBudget]
       ON [dbo].[Budget_Permission]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @BudgetDetailId INT,
			@Year INT,
			@Users INT = 0
	
	--Get BudgetDetailId and User Count
	SELECT @BudgetDetailId = BP.BudgetDetailId, @Users = ISNULL(COUNT(DISTINCT BP.UserId),0)
	FROM Budget_Permission BP
	INNER JOIN DELETED I ON BP.BudgetDetailId = I.BudgetDetailId
	GROUP BY BP.BudgetDetailId 

	--Get BudgetDetailId and User Count
	SELECT @BudgetDetailId = BP.BudgetDetailId, @Users = ISNULL(COUNT(DISTINCT BP.UserId),0)
	FROM Budget_Permission BP
	INNER JOIN INSERTED I ON BP.BudgetDetailId = I.BudgetDetailId
	GROUP BY BP.BudgetDetailId 

	--Update User Count By BudgetDetailId
	UPDATE [MV].[PreCalculatedMarketingBudget] SET Users = @Users WHERE BudgetDetailId = @BudgetDetailId

	
END
