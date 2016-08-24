
-- =============================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get projected value for tactic
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[TacticIndicatorProjectedValue]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[TacticIndicatorProjectedValue]
GO
CREATE FUNCTION [dbo].[TacticIndicatorProjectedValue]
(
	@TacticId INT,
	@ModelId INT,
	@TacticStageCode NVARCHAR(255),
	@IndicatorCode NVARCHAR(255),
	@ClientId UNIQUEIDENTIFIER
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(20) = 'REVENUE',
			@PlannedCostCode NVARCHAR(20) = 'PLANNEDCOST',
			@ProjectedStageValue FLOAT,
			@TacticStageLevel INT
					
	
	SET @ProjectedStageValue = (SELECT ProjectedStageValue FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @TacticId)
	SET @TacticStageLevel = (SELECT [Level] FROM Stage WHERE Code = @TacticStageCode AND ClientId = @ClientId AND IsDeleted = 0) 

	-- Calculate projected revenue for the tactic
	IF(@IndicatorCode = @RevenueCode)
	BEGIN
		
		SELECT @ProjectedStageValue = @ProjectedStageValue * MIN(M.AverageDealSize) * 
										Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
										FROM Model_Stage MS
										INNER JOIN Stage S ON MS.StageId = S.StageId
										INNER JOIN [Model] M ON MS.ModelId = M.ModelId
										WHERE MS.ModelId = @ModelId AND MS.StageType ='CR' AND S.[Level] >= @TacticStageLevel 
											  AND S.ClientId = @ClientId AND S.IsDeleted = 0
		
	END
	-- Calculate projected planned cost for the tactic
	ELSE IF(@IndicatorCode = @PlannedCostCode)
	BEGIN
		SELECT @ProjectedStageValue = Cost FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @TacticId 
	END
	-- Calculate projected stage values for the tactic
	ELSE
	BEGIN
		IF (@IndicatorCode = @TacticStageCode)
		BEGIN
			SELECT @ProjectedStageValue = @ProjectedStageValue
		END
		ELSE
		BEGIN
			-- Variables
			DECLARE @AboveMQLStageLevels NVARCHAR(50),
					@AboveMQLStageIds NVARCHAR(50),
					@IndicatorStageLevel INT,
					@MQLLevel INT,
					@ProjectedMQLValue INT = 0

			-- Fetch levels of stages
			SET @IndicatorStageLevel = (SELECT [Level] FROM Stage WHERE Code = @IndicatorCode AND ClientId = @ClientId AND IsDeleted = 0) 
			
			SET @MQLLevel = (SELECT [Level] FROM Stage WHERE Code = @MQLCode AND ClientId = @ClientId AND IsDeleted = 0) 

			IF(@TacticStageLevel > @IndicatorStageLevel OR @IndicatorCode = @INQCode)
			BEGIN
				SELECT @ProjectedStageValue = 0 
			END
			ELSE
			BEGIN
				-- Get stages and levels before MQL stage
				SELECT @AboveMQLStageLevels = COALESCE(@AboveMQLStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
					   @AboveMQLStageIds = COALESCE(@AboveMQLStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
				FROM Stage WHERE [Level] < @MQLLevel AND [Level] >= @TacticStageLevel AND ClientId = @ClientId AND IsDeleted = 0

				-- Calculate MQL from any stage
				IF(@TacticStageLevel = @MQLLevel)
				BEGIN
					SET @ProjectedMQLValue = @ProjectedStageValue
				END
				ELSE IF(@TacticStageLevel < @MQLLevel)
				BEGIN
					-- Aggregate all stages which are above MQL
					SET @ProjectedMQLValue = (SELECT ROUND(@ProjectedStageValue * Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1),0)
											  FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveMQLStageIds,',') CSVTable
											  ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR')
				END
			
				-- Calculation for final projected stage value
				IF (@IndicatorCode = @MQLCode)
				BEGIN
					SELECT @ProjectedStageValue = @ProjectedMQLValue 
				END
				-- Calculate CW from any stage
				ELSE IF (@IndicatorCode = @CWCode)
				BEGIN
					-- If level of tactic stage is more than MQL then calculate from that level itself to CW
					IF (@TacticStageLevel > @MQLLevel)
					BEGIN
						SET @ProjectedMQLValue = @ProjectedStageValue
						SET @MQLLevel = @TacticStageLevel
					END

					-- If level of tactic stage is less or equal to MQL then calculate from MQL to CW
					SELECT @ProjectedStageValue = ROUND(@ProjectedMQLValue * Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1),0) 
					FROM Model_Stage M INNER JOIN Stage S
					ON M.StageId = S.StageId WHERE ModelId = @ModelId AND StageType ='CR' 
					AND S.[Level] < @IndicatorStageLevel AND S.[Level] >= @MQLLevel AND S.ClientId = @ClientId AND S.IsDeleted = 0
				END
			END
		END
	END

	RETURN @ProjectedStageValue
END
GO