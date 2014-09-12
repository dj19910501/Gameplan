	--Create By : Kalpesh Sharma 09/11/2014
	--PL # #608 Create data model for advanced budgeting 


	IF OBJECT_ID(N'TempDB.dbo.#TempCostActual', N'U') IS NOT NULL
	BEGIN
	  DROP TABLE #TempCostActual
	END

	DECLARE @i INT
		,@TacticID INT
		,@MonthsCount INT
		,@CostActual VARCHAR(100)
		,@Values NVARCHAR(100)
		,@ReminderValues NVARCHAR(20)
		,@PeriodMonth DATE
		,@intErrorCode INT
		,@CreatedBy uniqueidentifier
		,@CurrentDate DATETIME

	SET @CreatedBy = 'E5EF88EB-4748-4436-9ACC-ABA6B2C5F6A9' 
	SET @CurrentDate = CONVERT(DATETIME, Convert(VARCHAR(10), GETDATE(), 111))

	CREATE TABLE #TempCostActual (
		TacticID INT
		,MonthsCount INT
		,CostActual DECIMAL
		,PeriodMonth DATE
		)

	INSERT INTO #TempCostActual (
		TacticID
		,MonthsCount
		,CostActual
		,PeriodMonth
		)
	SELECT PlanTacticId
		,(
			SELECT DATEDIFF(MONTH, StartDate, EndDate)
			) + 1 AS MonthsCount
		,CostActual
		,
		StartDate AS PeriodMonth
	FROM Plan_Campaign_Program_Tactic
	WHERE CostActual <> 0

	BEGIN TRANSACTION TranCostActual;
	 BEGIN TRY

	DECLARE myCursor CURSOR LOCAL FAST_FORWARD
	FOR
	SELECT *
	FROM #TempCostActual

	OPEN myCursor

	FETCH NEXT
	FROM myCursor
	INTO @TacticID
		,@MonthsCount
		,@CostActual
		,@PeriodMonth

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Values = FLOOR(Cast(@CostActual AS DECIMAL) / CAST(@MonthsCount AS INT))
		SET @ReminderValues = (Cast(@CostActual AS DECIMAL) - (Cast(@Values AS DECIMAL) * CAST(@MonthsCount AS INT)))

		DECLARE @RemainingMonthCount INT
		DECLARE @MONTHPERIOD NVARCHAR(20)

		SET @RemainingMonthCount = 0
		
		SET @i = 1

		WHILE (@i <= @MonthsCount)
		BEGIN
			IF (@i = @MonthsCount)
			BEGIN
				SET @Values = (Cast(@Values AS DECIMAL) + cast(@ReminderValues AS DECIMAL))
			END
			SET @MONTHPERIOD = Convert(nvarchar,CONCAT('Y', DATEPART(mm,DATEADD(mm,@RemainingMonthCount,@PeriodMonth))))
			INSERT INTO [dbo].[Plan_Campaign_Program_Tactic_Actual]
			         ([PlanTacticId]
			         ,[StageTitle]
			         ,[Period]
			         ,[Actualvalue]
			         ,[CreatedDate]
			         ,[CreatedBy]
			         ,[ModifiedDate]
			         ,[ModifiedBy])
			   VALUES(@TacticID,'Cost',@MONTHPERIOD,CONVERT(float,@Values),@CurrentDate,@CreatedBy,null,null)

			--PRINT (
			--		Convert(NVARCHAR, @TacticID) + ' Cost ' + @MONTHPERIOD + ' Value ' + @Values  + 'null' + 'null'
			--		)

			IF EXISTS(
						SELECT tactic.PlanLineItemId
						FROM Plan_Campaign_Program_Tactic_LineItem tactic
						WHERE tactic.PlanTacticId = @TacticID
							AND tactic.LineItemTypeId IS NULL
					)
			BEGIN
				DECLARE @LineItemID INT

				SET @LineItemID = (
						SELECT tactic.PlanLineItemId
						FROM Plan_Campaign_Program_Tactic_LineItem tactic
						WHERE tactic.PlanTacticId = @TacticID
							AND tactic.LineItemTypeId IS NULL
						)
						
				INSERT INTO [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]
				         ([PlanLineItemId]
				         ,[Period]
				         ,[Value]
				         ,[CreatedDate]
				         ,[CreatedBy])
				   VALUES(@LineItemID,@MONTHPERIOD,CONVERT(float,@Values),@CurrentDate,@CreatedBy)
			
				--PRINT (
				--		Convert(NVARCHAR, @LineItemID) + CONCAT (
				--			' Y'
				--			,DATEPART(mm, DATEADD(mm, @RemainingMonthCount, @PeriodMonth))
				--			) + ' Value ' + @Values + ' Created Date ' + Convert(VARCHAR(50), CONVERT(DATETIME, Convert(VARCHAR(10), GETDATE(), 111))) 
				--		)

			END
			
			update Plan_Campaign_Program_Tactic 
			set  CostActual = 0
			Where PlanTacticId = @TacticID
			

			SET @RemainingMonthCount = @RemainingMonthCount + 1
			SET @i = @i + 1
		END
		--print(' CostActual ' + Convert(varchar,@CostActual) + ' Months ' + Convert(varchar,@MonthsCount) +' Value ' + Convert(varchar,@Values) + ' Reminders ' +  Convert(varchar,@ReminderValues) )
		FETCH NEXT
		FROM myCursor
		INTO @TacticID
			,@MonthsCount
			,@CostActual
			,@PeriodMonth
	END

	print('Completed')

	CLOSE myCursor -- close the cursor

	DEALLOCATE myCursor -- Deallocate the cursor

	  ---Successfully deleted
	COMMIT TRANSACTION TranCostActual;
	 END TRY
		BEGIN CATCH
		  ---Unsuccess
			ROLLBACK TRANSACTION TranCostActual;
		END CATCH 