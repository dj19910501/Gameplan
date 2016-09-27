IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Plan_BudgetQuarterCalculation') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE dbo.Plan_BudgetQuarterCalculation
END
GO
CREATE PROCEDURE [dbo].[Plan_BudgetQuarterCalculation]	
	@EntityId INT,
	@Quater INT,
	@newValue FLOAT
	AS
	BEGIN
	
	DECLARE @FirstMonthofQuarter NVARCHAR(10);
	DECLARE @Sum FLOAT;
	DECLARE @SecondMonthofQuarter NVARCHAR(10);
	DECLARE @ThirdMonthofQuarter NVARCHAR(10);
	/*Following is calculation to get quarter value and onths of quarter*/
			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataPlan_Budget') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataPlan_Budget
			END 
			SELECT * INTO #tempDataPlan_Budget FROM (SELECT * from Plan_Budget where PlanId=@EntityId) a 
			IF(@Quater=1)
			BEGIN
				SELECT @Sum=SUM(value) from #tempDataPlan_Budget where Period in('Y1','Y2','Y3')	
				SET @FirstMonthofQuarter	='Y1';SET @SecondMonthofQuarter	='Y2';SET @ThirdMonthofQuarter	='Y3'
			END

            ELSE IF(@Quater=2)
			BEGIN
				SELECT @Sum=SUM(value) from #tempDataPlan_Budget where Period in('Y4','Y5','Y6')		
				SET @FirstMonthofQuarter	='Y4';SET @SecondMonthofQuarter	='Y5';SET @ThirdMonthofQuarter	='Y6'
			END

			ELSE IF(@Quater=3)
			BEGIN
				SELECT @Sum=SUM(value) from #tempDataPlan_Budget where Period in('Y7','Y8','Y9')
				SET @FirstMonthofQuarter	='Y7';SET @SecondMonthofQuarter	='Y8';SET @ThirdMonthofQuarter	='Y9'
			END

			ELSE IF(@Quater=4)
			BEGIN
				SELECT @Sum=SUM(value) from #tempDataPlan_Budget where Period in('Y10','Y11','Y12')
				SET @FirstMonthofQuarter	='Y10';SET @SecondMonthofQuarter	='Y11';SET @ThirdMonthofQuarter	='Y12'
			END

			
	BEGIN
	      /*Ex if Q1- if value is less then sum of (y1+y2+y3) then it will be deducted from y3->y2->y1 respectively */
			DECLARE @RemainingAmount FLOAT=0;
			DECLARE @DifferenceAmount FLOAT=@Sum-@newValue;			
			DECLARE @UpdateValue FLOAT=0;
			
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @ThirdMonthofQuarter)
			BEGIN		
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @ThirdMonthofQuarter)>@DifferenceAmount)
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter
				SET @UpdateValue=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @ThirdMonthofQuarter				
				END			
			END
		
			IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @SecondMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @SecondMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @SecondMonthofQuarter				
				END			
			END
			END		
		  IF(@UpdateValue<@DifferenceAmount)
			BEGIN
			IF EXISTS (SELECT * from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @FirstMonthofQuarter)
			BEGIN
				IF((SELECT Value from #tempDataPlan_Budget WHERE PlanId = @EntityId AND Period = @FirstMonthofQuarter)>(@DifferenceAmount-@UpdateValue))
				BEGIN
				UPDATE Plan_Budget  SET Value = (Value-(@DifferenceAmount-@UpdateValue))     WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter
				SET @UpdateValue+=@DifferenceAmount;
				END
				ELSE
				BEGIN
				SELECT @UpdateValue+=value from Plan_Budget WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter;
				UPDATE Plan_Budget  SET Value = 0     WHERE   PlanId = @EntityId AND Period = @FirstMonthofQuarter				
				END			
			END
			END			

			END

			END
						
GO