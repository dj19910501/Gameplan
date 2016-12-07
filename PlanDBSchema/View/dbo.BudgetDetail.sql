

/****** Object:  View [dbo].[BudgetDetail]    Script Date: 12/07/2016 12:46:36 PM ******/
IF EXISTS (SELECT 1 FROM sys.views WHERE OBJECT_ID=OBJECT_ID('BudgetDetail'))
BEGIN
	DROP VIEW [dbo].[BudgetDetail]
END
GO
GO

/****** Object:  View [dbo].[BudgetDetail]    Script Date: 12/07/2016 12:46:36 PM ******/
SET ANSI_NULLS ON
GO

CREATE VIEW [dbo].[BudgetDetail]
AS
	SELECT B.Id AS BudgetDetailId, Period, Budget ,Forecast,YEAR(B.CreatedDate) AS [Year], A.Id as Id
	FROM Budget A
	INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
	INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
	WHERE A.IsDeleted = 0 


GO


