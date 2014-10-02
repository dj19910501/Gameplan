
--Create By : Kalpesh Sharma 10-02-2014
--PL #822 Plan cost does not roll up

IF OBJECT_ID(N'TempDB.dbo.#TempTacticCost', N'U') IS NOT NULL
BEGIN
	DROP TABLE #TempTacticCost
END

DECLARE @i INT
	,@TacticID INT
	,@Cost FLOAT
	,@RemainingCost FLOAT
	,@OperationModule NVARCHAR(30)
	,@CreatedBy uniqueidentifier
	,@CurrentDate DATETIME

	SET @CurrentDate = CONVERT(DATETIME, Convert(VARCHAR(10), GETDATE(), 111))

CREATE TABLE #TempTacticCost (
	TacticID INT
	,Cost FLOAT
	,RemainingCost FLOAT
	,OperationModule NVARCHAR(30)
	,CreatedBy uniqueidentifier 
	)

INSERT INTO #TempTacticCost (
	TacticID
	,Cost
	,RemainingCost
	,OperationModule
	,CreatedBy
	)

--Fetch all the tactic who dosent't have any line item in respective table
SELECT PTactic.PlanTacticId
	,PTactic.Cost
	,COALESCE((PTactic.Cost), 0) AS RemainnigCost
	,'Created' AS OperationModule,
	PTactic.CreatedBy
FROM Plan_Campaign_Program_Tactic PTactic
WHERE PTactic.IsDeleted = 'false' and PTactic.Cost > 0
	AND PTactic.PlanTacticId NOT IN (
		SELECT PlanTacticId
		FROM Plan_Campaign_Program_Tactic_LineItem where IsDeleted = 'false'
		)

UNION
--Fetch Other line items value and get the difference of Actual cost (Tactic Cost -  Sum of Line item cost ) 
select pcpto.PlanTacticId,
	   pcpto.Cost, 
	   (pcpto.Cost - pcpti.LineItemCost) RemainnigCost ,
	   'Created' AS OperationModule,
	   pcpto.CreatedBy 
from Plan_Campaign_Program_Tactic pcpto,
(select pcptl.PlanTacticId, 
		sum(cost) LineItemCost
 from Plan_Campaign_Program_Tactic_LineItem pcptl 
 where IsDeleted=0
 group by pcptl.PlanTacticId) pcpti
where pcpto.PlanTacticId not in (select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where LineItemTypeId is null) and pcpto.PlanTacticId = pcpti.PlanTacticId
and (pcpto.Cost - pcpti.LineItemCost)  > 0 and IsDeleted = 0

UNION

SELECT PTactic.PlanTacticId
	,PTactic.Cost
	,(
		SELECT COALESCE((PTactic.Cost - SUM(Cost)), 0)
		FROM Plan_Campaign_Program_Tactic_LineItem
		WHERE PlanTacticId = PTactic.PlanTacticId and IsDeleted = 'false'
		) AS RemainnigCost
	,'Update' AS OperationModule,
	PTactic.CreatedBy
FROM Plan_Campaign_Program_Tactic PTactic
WHERE PTactic.IsDeleted = 'false'
	AND PTactic.Cost > 0
	AND PTactic.PlanTacticId IN (
		SELECT PlanTacticId
		FROM Plan_Campaign_Program_Tactic_LineItem
		WHERE LineItemTypeId IS NULL
		)

-- Now if Remaining Cost is equal to 0 at that time we will delete it from the Temp table
DELETE
FROM #TempTacticCost
WHERE RemainingCost <= 0
	
Select * from #TempTacticCost

-- Intialize Transaction and prevent it from any error replication
BEGIN TRANSACTION TranCostActual;

BEGIN TRY
	DECLARE myCursor CURSOR LOCAL FAST_FORWARD
	FOR
	SELECT *
	FROM #TempTacticCost

	OPEN myCursor

	FETCH NEXT
	FROM myCursor
	INTO @TacticID
		,@Cost
		,@RemainingCost
		,@OperationModule,
		@CreatedBy

	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		IF (@RemainingCost > 0)
		BEGIN
		IF (@OperationModule = 'Created')
		BEGIN
			INSERT INTO [dbo].[Plan_Campaign_Program_Tactic_LineItem]
			         ([PlanTacticId]
			         ,[LineItemTypeId]
			         ,[Title]
			         ,[Description]
			         ,[StartDate]
			         ,[EndDate]
			         ,[Cost]
			         ,[IsDeleted]
			         ,[CreatedDate]
			         ,[CreatedBy]
			         ,[ModifiedDate]
			         ,[ModifiedBy])
			VALUES
			         ( @TacticID,null,'Other',null,null,null,@RemainingCost,0,@CurrentDate,@CreatedBy,null,null)
			--PRINT ('TacticId ' + convert(NVARCHAR, @TacticID) + ' Cost ' + convert(NVARCHAR, @RemainingCost) + ' Created')
		END
		ELSE
		BEGIN
			UPDATE [dbo].[Plan_Campaign_Program_Tactic_LineItem]
			SET Cost =  COALESCE((Cost + @RemainingCost),0) , IsDeleted = 'false' 
			WHERE PlanTacticId = @TacticID and LineItemTypeId Is null
			--PRINT ('TacticId ' + convert(NVARCHAR, @TacticID) + ' Cost ' + convert(NVARCHAR, @RemainingCost) + ' Updated')
		END
		END

		FETCH NEXT
		FROM myCursor
		INTO @TacticID
			,@Cost
			,@RemainingCost
			,@OperationModule,@CreatedBy
	END

	CLOSE myCursor -- close the cursor

	DEALLOCATE myCursor -- Deallocate the cursor

	---Successfully 
	COMMIT TRANSACTION TranCostActual;
END TRY

BEGIN CATCH
	---Unsuccess
	ROLLBACK TRANSACTION TranCostActual;
END CATCH