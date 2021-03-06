IF EXISTS (SELECT * FROM sys.objects WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[GetPositionBasedRevenue]') AND TYPE IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetPositionBasedRevenue]
GO

CREATE FUNCTION [dbo].[GetPositionBasedRevenue]
(
	@ActivityDate DATETIME = '01-09-2015',
	@ActualRevenue FLOAT = 24000,
	@Index INT = 1,
	@DimensionId INT = 14, 
	@DimensionValue NVARCHAR(MAX) = 'Marketing', 
	@MinValue INT = 1,
	@MaxValue INT = 5,
	@AttributionType INT = 5, -- @AttributionType = 5 - Position, 7 - W Shaped
	@OppFirstTouchDate DATETIME,
	@OppLastTouchDate DATETIME
)

RETURNS FLOAT
AS
BEGIN
	DECLARE @XValue FLOAT, 
			@YValue FLOAT,
			@EvenlyDistValue FLOAT,
			@CalculatedRevenue FLOAT
			
	-- Two or more configuration are there for a single touch/ All touches for an opportunity should satisfy the same configuration
	IF NOT EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate ) OR
				 (SELECT COUNT(*) FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate ) > 2 OR
				  (SELECT COUNT(*) FROM AttrPositionConfig WHERE ISNULL(@OppFirstTouchDate,'') BETWEEN StartDate AND EndDate 
				  AND ISNULL(@OppLastTouchDate,'') BETWEEN StartDate AND EndDate AND DimensionId = @DimensionId) > 1
	BEGIN
		-- Case 3: Default case
		SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
		WHERE ISNULL(StartDate,'') = '' AND ISNULL(EndDate,'') = '' 
		AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''
	END 
	ELSE
	BEGIN
		-- Case 1: Check DimensionId, DimensionValue, ActivityDate
		SELECT @XValue = XValue * 100 , @YValue = YValue * 100 FROM AttrPositionConfig 
		WHERE ISNULL(@ActivityDate,'') BETWEEN StartDate AND EndDate 
		AND ISNULL(DimensionId,0) = @DimensionId AND ISNULL(DimensionValue,'') = @DimensionValue
	
		IF(@XValue IS NULL AND @YValue IS NULL)
		BEGIN
			-- Case 2: Check ActivityDate
			SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
			WHERE ISNULL(@ActivityDate,'') BETWEEN StartDate AND EndDate 
			AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''

			IF(@XValue IS NULL AND @YValue IS NULL)
			BEGIN
				-- Case 3: Default case
				SELECT @XValue = XValue * 100, @YValue = YValue * 100 FROM AttrPositionConfig 
				WHERE ISNULL(StartDate,'') = '' AND ISNULL(EndDate,'') = '' 
				AND ISNULL(DimensionId,0) = '' AND ISNULL(DimensionValue,'') = ''
			END
		END
	END
	IF((@XValue + @YValue) > 100)
	BEGIN
		RETURN NULL;
	END

	-- Calculation for W-Shaped attribution
	IF (@AttributionType = 7)
	BEGIN
		DECLARE @TempSumXY FLOAT =  @XValue + @YValue 
		IF(@TempSumXY > 0)
		BEGIN
			SET @XValue = ( @XValue * 100 ) / @TempSumXY
			SET @YValue = ( @YValue * 100 ) / @TempSumXY
		END
		SET @EvenlyDistValue = 0
	END
	ELSE
	BEGIN
		IF(@MaxValue > 2)
		BEGIN
			SET @EvenlyDistValue = (100 - @XValue - @YValue) / (@MaxValue - 2)
		END
		ELSE IF(@MaxValue = 2)
		BEGIN
			SET @EvenlyDistValue = (100 - @XValue - @YValue) / @MaxValue
			SET @XValue = @XValue + @EvenlyDistValue 
			SET @YValue = @YValue + @EvenlyDistValue 
		END
		ELSE
		BEGIN
			SET @XValue = 100
		END
	END

	IF(@XValue IS NOT NULL AND @YValue IS NOT NULL)
	BEGIN
		SET @CalculatedRevenue = (SELECT CASE WHEN @index = @MinValue THEN (@ActualRevenue * @XValue) / 100 
								 WHEN @index = @MaxValue THEN (@ActualRevenue * @YValue) / 100
								 ELSE (@ActualRevenue * @EvenlyDistValue) / 100 END)
	END
	RETURN @CalculatedRevenue
END
GO