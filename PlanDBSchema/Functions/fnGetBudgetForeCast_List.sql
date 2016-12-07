
/****** Object:  UserDefinedFunction [dbo].[fnGetBudgetForeCast_List]    Script Date: 12/01/2016 12:47:57 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetBudgetForeCast_List]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
DROP FUNCTION [dbo].[fnGetBudgetForeCast_List]
GO

/****** Object:  UserDefinedFunction [dbo].[fnGetBudgetForeCast_List]    Script Date: 12/01/2016 12:47:57 PM ******/
SET ANSI_NULLS ON
GO

-- =============================================
-- Author:		Devanshi
-- Create date: 01 Dec 2016
-- Description:	Function to get list of all childs for marketing budget
-- =============================================
CREATE FUNCTION [dbo].[fnGetBudgetForeCast_List]
(	
	@BudgetId int,
	@ClientId int
)
RETURNS TABLE 
AS
RETURN 
(
	WITH ForeCastBudgetDetail as
	(
	SELECT Budget_Detail.id,parentid from Budget_Detail
	INNER JOIN Budget on Budget_Detail.BudgetId=Budget.Id
	WHERE Budget.ClientId=@ClientId and Budget_Detail.IsDeleted=0 and
	 Budget_Detail.Id=@BudgetId and ParentID is null

    UNION ALL

    SELECT BD.id,BD.ParentID  FROM Budget_Detail BD 
	INNER JOIN Budget on BD.BudgetId=Budget.Id
	INNER JOIN ForeCastBudgetDetail FBD	  ON FBD.Id = BD.ParentID
	WHERE Budget.ClientId=@ClientId and BD.IsDeleted=0 
	)
	SELECT convert(nvarchar(20),a.id) as Id From ForeCastBudgetDetail a
	WHERE   NOT EXISTS ( SELECT *   FROM   ForeCastBudgetDetail b  WHERE  b.parentid = a.Id )
)


GO


