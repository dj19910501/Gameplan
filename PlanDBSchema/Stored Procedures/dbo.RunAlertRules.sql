-- ===================================================
-- Author:		Arpita Soni
-- Create date: 08/08/2016
-- Description:	Run alert rules and generate alerts
-- ===================================================
-- DROP AND CREATE STORED PROCEDURE [dbo].[RunAlertRules]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[RunAlertRules]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[RunAlertRules]
END
GO
CREATE PROCEDURE [dbo].[RunAlertRules]
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Constant variables
		DECLARE @txtLessThan	NVARCHAR(20) = 'less than',
				@txtGreaterThan NVARCHAR(20) = 'greater than',
				@txtEqualTo		NVARCHAR(20) = 'equal to'

		DECLARE @TacticsDataForRules			NVARCHAR(MAX) = '',
				@UPDATEQUERYCOMMON				NVARCHAR(MAX) = '',
				@UpdatePlanQuery				NVARCHAR(MAX) = '',
				@UpdateProjectedValuesForPlan	NVARCHAR(MAX) = '',
				@UpdateCampaignQuery			NVARCHAR(MAX) = '',
				@UpdateProgramQuery				NVARCHAR(MAX) = '',
				@UpdateTacticQuery				NVARCHAR(MAX) = '',
				@UpdateLineItemQuery			NVARCHAR(MAX) = '',
				@CalculatePercentGoalQuery		NVARCHAR(MAX) = '',
				@INSERTALERTQUERYCOMMON			NVARCHAR(MAX) = '',
				@InsertQueryForLT				NVARCHAR(MAX) = '',
				@InsertQueryForGT				NVARCHAR(MAX) = '',
				@InsertQueryForEQ				NVARCHAR(MAX) = '',
				@CommonQueryToIgnoreDuplicate	NVARCHAR(MAX) = ''
				

		-- Get projected and actual values of tactic belongs to plan/campaign/program
		SET @TacticsDataForRules = 'DECLARE @TempEntityTable [TacticForRuleEntities];

									-- Get entities from the rule which have reached the completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],
																 [Frequency],[DayOfWeek],[DateOfMonth],[UserId],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0)
										FROM @TempEntityTable A INNER JOIN  
										#TacticsDataForAllRuleEntities B
										ON A.EntityId = B.EntityId AND A.EntityType = B.EntityType AND A.Indicator = B.Indicator
										'
		-- Update IndicatorTitle based on Indicator Code
		DECLARE @UpdateIndicatorTitle NVARCHAR(MAX) = ' 
														UPDATE A SET A.IndicatorTitle = dbo.GetIndicatorTitle(A.Indicator,B.ClientId,A.EntityType)
														FROM @TempEntityTable A 
														INNER JOIN vClientWise_EntityList B ON A.EntityId = B.EntityId AND A.EntityType = B.Entity
														'

		-- For plan update projected value using different calculation rest of PLANNEDCOST
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												AND A.Indicator != ''PLANNEDCOST''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) != 0) THEN 100 
												 ELSE ISNULL(ActualStageValue,0) * 100 / ProjectedStageValue END ;
										   SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT RuleId, ##DESCRIPTION## AS [Description], UserId,
										(CASE WHEN Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) END
											,GETDATE()) 
										WHEN Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate										
										FROM @TempEntityTable '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal < IndicatorGoal AND IndicatorComparision = ''LT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal > IndicatorGoal AND IndicatorComparision = ''GT'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal = IndicatorGoal AND IndicatorComparision = ''EQ'' AND 
															(ISNULL(ProjectedStageValue,0) != 0 OR ISNULL(ActualStageValue,0) != 0) '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere
		
		SET @CommonQueryToIgnoreDuplicate = '	MERGE INTO [dbo].Alerts AS T1
												USING
												(##INSERTQUERY##) AS T2
												ON (T2.RuleId = T1.RuleId AND T2.Description = T1.Description AND T2.UserId = T1.UserId)
												WHEN NOT MATCHED THEN  
												INSERT ([RuleId],[Description],[UserId],[DisplayDate])
												VALUES ([RuleId],[Description],[UserId],[DisplayDate]) ; '

		SET @InsertQueryForLT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForLT)
		SET @InsertQueryForGT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForGT)
		SET @InsertQueryForEQ = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForEQ)
		
		EXEC (@TacticsDataForRules + @UPDATEQUERYCOMMON + @UpdateIndicatorTitle + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ)
		
	END TRY
	BEGIN CATCH
		--Get the details of the error
		 DECLARE   @ErMessage NVARCHAR(2048),
				   @ErSeverity INT,
				   @ErState INT
 
		 SELECT @ErMessage = ERROR_MESSAGE(), @ErSeverity = ERROR_SEVERITY(), @ErState = ERROR_STATE()
 
		 RAISERROR (@ErMessage, @ErSeverity, @ErState)
	END CATCH 
END
GO
