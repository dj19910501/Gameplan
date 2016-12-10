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
		INSERT INTO [MV].[PreCalculatedMarketingBudget] (BudgetDetailId, [Year],Y1_Budget,Y2_Budget,Y3_Budget,Y4_Budget,Y5_Budget,Y6_Budget,Y7_Budget,Y8_Budget,Y9_Budget,Y10_Budget,Y11_Budget,Y12_Budget
																				,Y1_Forecast,Y2_Forecast,Y3_Forecast,Y4_Forecast,Y5_Forecast,Y6_Forecast,Y7_Forecast,Y8_Forecast,Y9_Forecast,Y10_Forecast,Y11_Forecast,Y12_Forecast
																				,Y1_Planned,Y2_Planned,Y3_Planned,Y4_Planned,Y5_Planned,Y6_Planned,Y7_Planned,Y8_Planned,Y9_Planned,Y10_Planned,Y11_Planned,Y12_Planned
																				,Y1_Actual,Y2_Actual,Y3_Actual,Y4_Actual,Y5_Actual,Y6_Actual,Y7_Actual,Y8_Actual,Y9_Actual,Y10_Actual,Y11_Actual,Y12_Actual)
		SELECT Id, YEAR(CreatedDate),0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
		FROM INSERTED
	END
	ELSE
	BEGIN
		-- Delete record from pre calculate table
		DELETE P FROM [MV].[PreCalculatedMarketingBudget] P
		INNER JOIN DELETED D ON P.BudgetDetailId = D.Id
	END

END

GO
