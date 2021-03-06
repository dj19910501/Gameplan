-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'GetFinanceBasicData') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetFinanceBasicData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFinanceBasicData]
(
	@BudgetId		INT,			-- Budget Id
	@lstUserIds		NVARCHAR(MAX),	-- List of all users for this client
	@UserId			INT				-- Logged in user id 
)
RETURNS 
@ResultFinanceData TABLE 
(
	Permission		VARCHAR(20),
	BudgetDetailId	INT,
	ParentId		INT,
	Name			VARCHAR(MAX),
	[Owner]			INT,
	TotalBudget		FLOAT,
	TotalForecast	FLOAT,
	TotalPlanned	FLOAT,
	LineItems	INT
)
AS

BEGIN
	
	-- SELECT * FROM [dbo].[GetFinanceBasicData](1,'2,4,6,12,13,17,22,24,27,30,32,37,38,42,43,49,53,58,59,641,62,66,71,77,79,80,82,643,85,86,87,88,89,92,96,97,101,106,107,617,646,110,113,114,624,120,125,129,131,133,619,134,136,137,139,647,142,144,146,150,155,157,158,160,163,165,166,645,169,174,175,176,179,186,623,188,190,191,192,194,195,208,210,211,213,214,219,220,221,223,224,621,243,245,250,626,254,256,258,259,260,625,261,262,269,270,272,273,275,276,277,281,288,289,291,292,293,295,298,299,301,303,304,313,314,315,318,320,322,324,327,330,339,341,342,344,349,355,359,361,362,365,366,370,374,378,382,387,389,393,394,398,399,403,406,407,408,412,415,419,426,427,428,431,434,435,440,446,447,449,450,451,454,457,459,462,465,466,467,468,470,471,472,475,481,485,486,487,488,492,493,495,497,503,504,505,512,514,516,517,518,519,520,521,526,527,533,536,537,542,544,545,546,549,551,555,556,561,566,573,574,578,584,585,586,588,590,649,593,595,602,608',549)
	
	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	BEGIN
		DECLARE @tblResult TABLE
		(
			Permission		VARCHAR(20),
			BudgetDetailId	INT,
			ParentId		INT,
			Name			VARCHAR(MAX),
			[Owner]			INT,
			TotalBudget		FLOAT,
			TotalForecast	FLOAT
		)

		-- Get Budget details based on permissions
		INSERT INTO @tblResult(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast)
		SELECT
				CASE WHEN BP.PermisssionCode = 0 THEN 'Edit'
					  WHEN BP.PermisssionCode = 1 THEN 'View'
					  ELSE 'None' 
				END AS Permission	
				,BD.Id AS BudgetDetailId
				,BD.ParentId
				,BD.Name
				,U.UserId as '[Owner]'
				,BD.TotalBudget
				,BD.TotalForecast
		FROM [dbo].Budget_Detail(NOLOCK) BD 
		INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		LEFT JOIN [dbo].[Budget_Permission](NOLOCK) BP ON BD.Id = BP.BudgetDetailId AND BP.UserId = @UserId
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId 
	END
	
	INSERT INTO @ResultFinanceData(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast,TotalPlanned,LineItems)
	SELECT DISTINCT R.Permission,R.BudgetDetailId,R.ParentId,R.Name,R.[Owner],R.TotalBudget,R.TotalForecast,LineItem.TotalPlanned,ISNULL(LC.LineItemCount,0) LineItems
	FROM @tblResult R
	LEFT JOIN 
	(
		-- Get line item counts
		SELECT LB.BudgetDetailId, COUNT(LB.PlanLineItemId) AS LineItemCount 
		FROM LineItem_Budget(NOLOCK) LB 
		INNER JOIN Budget_Detail BD ON LB.BudgetDetailId = BD.Id AND BD.IsDeleted = 0
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PL ON LB.PlanLineItemId = PL.PlanLineItemId AND PL.IsDeleted=0 AND PL.LineItemTypeId IS NOT NULL 
		WHERE BD.BudgetId = @BudgetId 
		GROUP BY LB.BudgetDetailId
	) LC ON R.BudgetDetailId = LC.BudgetDetailId
	LEFT JOIN 
	(	
		-- Get Planned Cost values
		SELECT BD.Id AS BudgetDetailId, SUM((PCPTL.Cost * CAST(Weightage AS FLOAT)/100)) AS TotalPlanned FROM 
		[dbo].[Budget_Detail](NOLOCK) BD 
		INNER JOIN LineItem_Budget(NOLOCK) LB ON BD.Id = LB.BudgetDetailId
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem(NOLOCK) PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
		WHERE BD.IsDeleted=0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL and PCPTL.IsDeleted = 0
		GROUP BY BD.Id
	) LineItem ON R.BudgetDetailId = LineItem.BudgetDetailId

	RETURN 
END

GO