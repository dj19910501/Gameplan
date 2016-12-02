IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
   ON  [dbo].[Budget_Detail]
   AFTER INSERT, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Insert new record into pre calculate when new budget is generated
		INSERT INTO [MV].[PreCalculatedMarketingBudget] (BudgetDetailId, [Year])
		SELECT Id, YEAR(CreatedDate) FROM INSERTED
	END
	ELSE
	BEGIN
		-- Delete record from pre calculate table
		DELETE P FROM [MV].[PreCalculatedMarketingBudget] P
		INNER JOIN DELETED D ON P.BudgetDetailId = D.Id
	END

END
GO
