
-- =============================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get projected value for Plan
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[PlanIndicatorProjectedValue]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[PlanIndicatorProjectedValue]
GO
CREATE FUNCTION [dbo].[PlanIndicatorProjectedValue]
(
	@PlanId INT,
	@ModelId INT,
	@PlanStageCode NVARCHAR(255),
	@IndicatorCode NVARCHAR(50),
	@ClientId UNIQUEIDENTIFIER
)
RETURNS FLOAT
AS
BEGIN
	-- Variables
	DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@ProjectedStageValue INT,
			@ResultValue FLOAT,
			@AboveStageLevels NVARCHAR(MAX),
			@AboveStageIds NVARCHAR(MAX),
			@IndicatorStageLevel INT,
			@PlanStageLevel INT,
			@MQLLevel INT,
			@CWLevel INT
	
	SELECT @ProjectedStageValue = GoalValue FROM [Plan] WHERE PlanId = @PlanId AND IsDeleted = 0

	IF(@PlanStageCode = @IndicatorCode)
	BEGIN
		RETURN @ProjectedStageValue
	END
	
	-- Fetch levels of stages
	SELECT @IndicatorStageLevel = [Level] FROM Stage WHERE Code = @IndicatorCode AND ClientId = @ClientId AND IsDeleted = 0
	SELECT @PlanStageLevel = [Level] FROM Stage WHERE Code = @PlanStageCode AND ClientId = @ClientId AND IsDeleted = 0
	
	SELECT @MQLLevel = [Level] FROM Stage WHERE Code = @MQLCode AND ClientId = @ClientId AND IsDeleted = 0
	SELECT @CWLevel = [Level] FROM Stage WHERE Code = @CWCode AND ClientId = @ClientId AND IsDeleted = 0
	
	IF(@PlanStageCode != @RevenueCode AND (@IndicatorCode = @MQLCode OR @IndicatorStageLevel < @PlanStageLevel))
	BEGIN
		DECLARE @CalStageFrom INT = @PlanStageLevel

		IF(@IndicatorStageLevel < @PlanStageLevel)
		BEGIN
			SET @CalStageFrom = @IndicatorStageLevel
		END

		-- Get stages and levels before MQL stage
		SELECT @AboveStageLevels = COALESCE(@AboveStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
				@AboveStageIds = COALESCE(@AboveStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
		FROM Stage WHERE [Level] < @MQLLevel AND [Level] >= @CalStageFrom AND ClientId = @ClientId AND IsDeleted = 0
	END
	ELSE
	BEGIN
		-- Get stages and levels upto CW stage
		SELECT @AboveStageLevels = COALESCE(@AboveStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
				@AboveStageIds = COALESCE(@AboveStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
		FROM Stage WHERE [Level] < @CWLevel AND [Level] >= ISNULL(@PlanStageLevel, @IndicatorStageLevel) AND ClientId = @ClientId AND IsDeleted = 0
	END

	IF(@PlanStageCode = @RevenueCode OR @IndicatorStageLevel < ISNULL(@PlanStageLevel,0))
	BEGIN
		-- Calculate INQ/MQL/CW from Revenue
		SET @ResultValue = (SELECT @ProjectedStageValue / Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
							FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveStageIds,',')  CSVTable
							ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR') 
		IF(@PlanStageCode = @RevenueCode)
		BEGIN
			SET @ResultValue = ISNULL(@ResultValue, @ProjectedStageValue) / (SELECT AverageDealSize FROM [Model] WHERE ModelId = @ModelId AND IsDeleted = 0)
		END
			
	END
	ELSE
	BEGIN
		-- Calculate INQ/MQL/CW/REVENUE from INQ/MQL/CW
		SET @ResultValue = (SELECT @ProjectedStageValue * 
							Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
							FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveStageIds,',') CSVTable
							ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR') 

		-- Multiply with AverageDealSize to calculate Revenue
		IF(@IndicatorCode = @RevenueCode)
		BEGIN
			SET @ResultValue = @ResultValue * (SELECT AverageDealSize FROM [Model] WHERE ModelId = @ModelId AND IsDeleted = 0)
		END
	END
	RETURN ROUND(ISNULL(@ResultValue,0),0)
	
END
GO
