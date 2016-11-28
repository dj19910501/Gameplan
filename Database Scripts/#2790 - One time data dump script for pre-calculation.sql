
    BEGIN
	-- Update Budget values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Budget = [Y1],Y2_Budget = [Y2],
					  Y3_Budget = [Y3],Y4_Budget = [Y4],
					  Y5_Budget = [Y5],Y6_Budget = [Y6],
					  Y7_Budget = [Y7],Y8_Budget = [Y8],
					  Y9_Budget = [Y9],Y10_Budget = [Y10],
					  Y11_Budget = [Y11],Y12_Budget = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly budget amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Budget 
			FROM Budget A
			INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
			WHERE A.IsDeleted = 0
		) P
		PIVOT
		(
			MIN(BUDGET)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END

BEGIN
	-- Insert Budget records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Budget, Y2_Budget, Y3_Budget, Y4_Budget, Y5_Budget, 
													Y6_Budget, Y7_Budget,Y8_Budget, Y9_Budget, Y10_Budget, Y11_Budget, Y12_Budget)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly budget amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget 
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

BEGIN
	-- Update Forecast values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Forecast = [Y1],Y2_Forecast = [Y2],
					  Y3_Forecast = [Y3],Y4_Forecast = [Y4],
					  Y5_Forecast = [Y5],Y6_Forecast = [Y6],
					  Y7_Forecast = [Y7],Y8_Forecast = [Y8],
					  Y9_Forecast = [Y9],Y10_Forecast = [Y10],
					  Y11_Forecast = [Y11],Y12_Forecast = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal
	INNER JOIN 
	(
		-- Get monthly forecast amount with pivoting
		SELECT * FROM 
		(
			SELECT B.Id AS BudgetDetailId, Period, Forecast FROM Budget A
			INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
			INNER JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
		) P
		PIVOT
		(
			MIN(Forecast)
			FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
		) AS Pvt
	) ExistingFinanceData ON PreCal.BudgetDetailId = ExistingFinanceData.BudgetDetailId
END

BEGIN
	-- Insert Forecast records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Forecast, Y2_Forecast, Y3_Forecast, Y4_Forecast, Y5_Forecast, 
													Y6_Forecast, Y7_Forecast,Y8_Forecast, Y9_Forecast, Y10_Forecast, Y11_Forecast, Y12_Forecast)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly forecast amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, Budget FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN Budget_DetailAmount C ON B.Id = C.BudgetDetailId
	) P
	PIVOT
	(
		MIN(BUDGET)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

BEGIN
	-- Update Planned values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Planned = [Y1],Y2_Planned = [Y2],
					  Y3_Planned = [Y3],Y4_Planned = [Y4],
					  Y5_Planned = [Y5],Y6_Planned = [Y6],
					  Y7_Planned = [Y7],Y8_Planned = [Y8],
					  Y9_Planned = [Y9],Y10_Planned = [Y10],
					  Y11_Planned = [Y11],Y12_Planned = [Y12]
	FROM 
	[MV].PreCalculatedMarketingBudget PreCal 
	INNER JOIN 
	(
		-- Get monthly planned cost amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalPlanned
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			SELECT BD.Id AS BudgetDetailId,PCPTL.Period, SUM((ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100)) AS TotalPlanned FROM 
			[dbo].[Budget_Detail] BD 
			INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
			GROUP BY BD.Id, PCPTL.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalPlanned)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
END

BEGIN
	-- Insert Planned records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Planned, Y2_Planned, Y3_Planned, Y4_Planned, Y5_Planned, 
													Y6_Planned, Y7_Planned,Y8_Planned, Y9_Planned, Y10_Planned, Y11_Planned, Y12_Planned)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly planned cost amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalPlanned
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on planned cost and sum up costs for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTL.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalPlanned FROM 
			[dbo].[Budget_Detail] BD 
			INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
			GROUP BY BD.Id, PCPTL.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalPlanned)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END

BEGIN
	-- Update Actual values into [MV].[PreCalculatedMarketingBudget] table
	UPDATE PreCal SET Y1_Actual = [Y1],Y2_Actual = [Y2],
					  Y3_Actual = [Y3],Y4_Actual = [Y4],
					  Y5_Actual = [Y5],Y6_Actual = [Y6],
					  Y7_Actual = [Y7],Y8_Actual = [Y8],
					  Y9_Actual = [Y9],Y10_Actual = [Y10],
					  Y11_Actual = [Y11],Y12_Actual = [Y12]
	FROM [MV].PreCalculatedMarketingBudget PreCal 
	INNER JOIN 
	(
		-- Get monthly actuals amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalActual
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTL.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalActual FROM 
			[dbo].[Budget_Detail] BD 
			INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
			GROUP BY BD.Id, PCPTL.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalActual)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
END

BEGIN
	-- Insert Actual records into [MV].[PreCalculatedMarketingBudget] table
	INSERT INTO [MV].PreCalculatedMarketingBudget (BudgetDetailId, [Year], Y1_Actual, Y2_Actual, Y3_Actual, Y4_Actual, Y5_Actual, 
													Y6_Actual, Y7_Actual,Y8_Actual, Y9_Actual, Y10_Actual, Y11_Actual, Y12_Actual)
	SELECT Pvt.BudgetDetailId,Pvt.[Year],[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12] FROM 
	(
		-- Get monthly actuals amount with pivoting
		SELECT B.Id AS BudgetDetailId,YEAR(B.CreatedDate) AS [Year], Period, TotalActual
		FROM Budget A
		INNER JOIN Budget_Detail B ON A.Id = B.BudgetId AND B.IsDeleted = 0
		LEFT JOIN 
		(
			-- Apply weightage on actual and sum up actuals for all line items associated to the single budget 
			SELECT BD.Id AS BudgetDetailId,PCPTL.Period, SUM(ISNULL(Value,0) * CAST(Weightage AS FLOAT)/100) AS TotalActual FROM 
			[dbo].[Budget_Detail] BD 
			INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
			INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
			GROUP BY BD.Id, PCPTL.Period
		) LineItems ON B.Id = LineItems.BudgetDetailId
		WHERE A.IsDeleted = 0
	) P
	PIVOT
	(
		MIN(TotalActual)
		FOR Period IN ([Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12])
	) AS Pvt
	LEFT JOIN [MV].PreCalculatedMarketingBudget PreCal ON PreCal.BudgetDetailId = Pvt.BudgetDetailId
	WHERE PreCal.Id IS NULL
END
