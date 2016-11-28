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
	@BudgetId		INT,
	@ClientId		INT,
	@lstUserIds		NVARCHAR(MAX),
	@UserId			INT,
	@CurrencyRate	FLOAT
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
	LineItems		INT,
	[User]			INT
)
AS

BEGIN
	
	-- select * from [dbo].GetFinanceBasicData(2807,24,'470,308,104',470)
	
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
		FROM [dbo].Budget_Detail BD 
		INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		LEFT JOIN [dbo].[Budget_Permission] BP ON BD.Id = BP.BudgetDetailId AND BP.UserId = @UserId
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId
	END

	DECLARE @tblLineItemIds TABLE
	(
		BudgetDetailId	INT,
		LineItems		VARCHAR(MAX)
	)

	-- Get Parent & Children comma separated LineItemIds by BudgetDetailIds
	;With TblLineItem as(
		
		-- Get Children records
		SELECT R.BudgetDetailId,R.ParentId,L.PlanLineItemIds FROM @tblResult R
		JOIN [dbo].[GetLineItemIdsByBudgetDetailId](@BudgetId) L ON R.BudgetDetailId = L.BudgetDetailId
		WHERE R.BudgetDetailId NOT IN (SELECT ISNULL(ParentId,0) FROM @tblResult)

		UNION ALL 
		
		-- Get parent records
		SELECT P.BudgetDetailId,P.ParentId,c.PlanLineItemIds
		FROM @tblResult p
		JOIN TblLineItem c ON c.ParentId = p.BudgetDetailId
	)

	-- Insert Parent,child distinct lineItem count by comma separated lineItemsIds.
	INSERT INTO @tblLineItemIds(BudgetDetailId,LineItems)
	SELECT	BudgetDetailId
			,SUM(LEN(PlanLineItemIds) - LEN(REPLACE(PlanLineItemIds, ',', '')) +1) AS LineItems	-- Get distinct comma separated lineItemIds count.
	FROM
	(
		SELECT BudgetDetailId
				-- Get Distinct comma separated lineItemIds for each parent-child level record
				,REPLACE(
						CAST(
							CAST('<d>'+ REPLACE(PlanLineItemIds, ', ','</d><d>')+'</d>'  AS XML)
							.query('distinct-values(/d)') AS VARCHAR
							), ' ', ', ')AS [PlanLineItemIds]
		
		FROM (
				SELECT BudgetDetailId,PlanLineItemIds = STUFF((
				    SELECT ', ' + CAST(PlanLineItemIds AS VARCHAR) FROM TblLineItem
				    WHERE BudgetDetailId = x.BudgetDetailId
				    FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)'), 1, 2, '')
				FROM TblLineItem x
				GROUP BY BudgetDetailId
			) AS planlineIds
	) AS linecnts
	GROUP BY BudgetDetailId

	INSERT INTO @ResultFinanceData(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast,TotalPlanned,LineItems,[User])
	SELECT DISTINCT R.Permission,R.BudgetDetailId,R.ParentId,R.Name,R.[Owner],R.TotalBudget,R.TotalForecast,LineItem.TotalPlanned,L.LineItems,usrcnt.[User]
	FROM @tblResult R
	LEFT JOIN @tblLineItemIds L on R.BudgetDetailId = L.BudgetDetailId
	LEFT JOIN 
	(
		-- Get User Count
		SELECT  BD.Id as BudgetDetailId,COUNT(BP.UserId) as [User]
		FROM [dbo].[Budget_Detail] BD
		JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		JOIN [dbo].[Budget_Permission] BP ON BD.Id = BP.BudgetDetailId 
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId
		GROUP BY BD.Id
	) AS usrcnt ON R.BudgetDetailId = usrcnt.BudgetDetailId
	LEFT JOIN 
	(	
		-- Get Planned Cost values
		SELECT BD.Id AS BudgetDetailId, SUM((PCPTL.Cost * Weightage/100)) AS TotalPlanned FROM 
		[dbo].[Budget_Detail] BD 
		INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
		GROUP BY BD.Id
	) LineItem ON L.BudgetDetailId = LineItem.BudgetDetailId

	RETURN 
END
GO
