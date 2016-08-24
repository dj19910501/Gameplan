
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

									-- Get entities from the rule which are reached to completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- SELECT * INTO TempEntityTable from @TempEntityTable

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									-- SELECT * FROM #TacticsDataForAllRuleEntities 
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0)
										FROM @TempEntityTable A INNER JOIN  
										(
											SELECT B.##ENTITYIDCOLNAME##,B.INDICATOR, SUM(B.ProjectedStageValue) AS ProjectedStageValue,SUM(B.ActualStageValue) AS ActualStageValue
											FROM @TempEntityTable A
											INNER JOIN #TacticsDataForAllRuleEntities B
											ON A.EntityId = B.##ENTITYIDCOLNAME## AND A.EntityType = ##ENTITYTYPE## AND A.Indicator = B.Indicator
											GROUP BY B.##ENTITYIDCOLNAME##, B.Indicator
										) B ON A.EntityId = B.##ENTITYIDCOLNAME##  AND A.Indicator = B.Indicator AND A.EntityType = ##ENTITYTYPE## 
																				
										'
		-- Update IndicatorTitle based on Indicator Code
		DECLARE @UpdateIndicatorTitle NVARCHAR(MAX) = ' 
														UPDATE A SET A.IndicatorTitle = dbo.GetIndicatorTitle(A.Indicator,B.ClientId)
														FROM @TempEntityTable A 
														INNER JOIN vClientWise_EntityList B ON A.EntityId = B.EntityId AND A.EntityType = REPLACE(B.Entity,'' '','''')
														'

		-- Update query for plan
		SET @UpdatePlanQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanId')
		SET @UpdatePlanQuery = REPLACE(@UpdatePlanQuery,'##ENTITYTYPE##','''Plan''')
		-- Update query for campaign
		SET @UpdateCampaignQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanCampaignId')
		SET @UpdateCampaignQuery = REPLACE(@UpdateCampaignQuery,'##ENTITYTYPE##','''Campaign''')
		-- Update query for program
		SET @UpdateProgramQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanProgramId')
		SET @UpdateProgramQuery = REPLACE(@UpdateProgramQuery,'##ENTITYTYPE##','''Program''')
		-- Update query for tactic
		SET @UpdateTacticQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanTacticId')
		SET @UpdateTacticQuery = REPLACE(@UpdateTacticQuery,'##ENTITYTYPE##','''Tactic''')
		-- Update query for line item
		SET @UpdateLineItemQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanLineItemId')
		SET @UpdateLineItemQuery = REPLACE(@UpdateLineItemQuery,'##ENTITYTYPE##','''LineItem''')

		-- For plan update projected value using different calculation
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) != 0) OR (ActualStageValue * 100 / ProjectedStageValue) > 100 THEN 100 
												 ELSE ISNULL(ActualStageValue,0) * 100 / ProjectedStageValue END ;
										   SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT AR.RuleId, ##DESCRIPTION## AS [Description], AR.UserId,
										(CASE WHEN AR.Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek + 1) END
											,GETDATE()) 
										WHEN AR.Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate										
										FROM @TempEntityTable FinalTable
										INNER JOIN Alert_Rules AR ON FinalTable.RuleId = AR.RuleId '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal < AR.IndicatorGoal AND AR.IndicatorComparision = ''LT'' '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal > AR.IndicatorGoal AND AR.IndicatorComparision = ''GT'' '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal = AR.IndicatorGoal AND AR.IndicatorComparision = ''EQ'' '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere
		
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
		
		EXEC (@TacticsDataForRules + @UpdatePlanQuery + @UpdateCampaignQuery + @UpdateProgramQuery + @UpdateTacticQuery + @UpdateLineItemQuery + @UpdateIndicatorTitle + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ )
	
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