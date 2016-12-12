IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Tactic_CostQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Tactic_CostQuarterCalculation
END
Go
CREATE PROCEDURE [dbo].[Tactic_CostQuarterCalculation]	
	@EntityId INT,
	@Quarter INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempTacticActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempTacticActual
			END 
			SELECT * INTO #tempTacticActual FROM (SELECT * from Plan_Campaign_Program_Tactic_Cost where PlanTacticId=@EntityId ) a 
			IF(@Quarter=1)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quarter=2)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quarter=3)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quarter=4)
			BEGIN
				SELECT @Sum=ISNULL(SUM(value),0) from #tempTacticActual where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN

			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempTacticActual WHERE  PlanTacticId= @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter 
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter ;
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @ThirdMonthofQuarter 				
				END			
			END
			
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter 
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter ;
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @SecondMonthofQuarter		
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				--IF((SELECT Value from #tempTacticActual WHERE PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				--BEGIN
				UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter 
				SET @UpdateValue+=@DifferenceAmount;
				--END
				--ELSE
				--BEGIN
				--SELECT @UpdateValue+=Value from Plan_Campaign_Program_Tactic_Cost WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter ;
				--UPDATE Plan_Campaign_Program_Tactic_Cost  SET Value = 0     WHERE   PlanTacticId = @EntityId AND Period = @FirstMonthofQuarter		 
				--END			
		END
		END			
	END

END
Go