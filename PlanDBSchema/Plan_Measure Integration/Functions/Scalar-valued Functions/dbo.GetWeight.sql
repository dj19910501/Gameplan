IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetWeight]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetWeight]
GO

CREATE FUNCTION [dbo].[GetWeight]
(
	@ActivityDate datetime,
	@DimensionId int,
	@DimensionValue nvarchar(max)
)
 
RETURNS FLOAT
AS
BEGIN

	DECLARE @TouchWeight FLOAT

	-- Case 1: Check DimensionId, DimensionValue, ActivityDate
	IF((SELECT COUNT(*) FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL AND DimensionId IS NOT NULL AND DimensionValue IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId = @DimensionId AND DimensionValue = @DimensionValue)) > 0)
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL AND DimensionId IS NOT NULL AND DimensionValue IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId = @DimensionId AND DimensionValue = @DimensionValue)
	END
	-- Case 2: Check ActivityDate
	ELSE if((SELECT COUNT(*) FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId IS NULL AND DimensionValue IS NULL)) > 0)
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE (StartDate IS NOT NULL AND EndDate IS NOT NULL) AND ((@ActivityDate BETWEEN StartDate AND EndDate) AND DimensionId IS NULL AND DimensionValue IS NULL)
	END
	-- Case 3: Default case
	ELSE
	BEGIN
		SELECT @TouchWeight = [Weight] FROM AttrPositionConfig WHERE StartDate IS NULL AND EndDate IS NULL AND DimensionId IS NULL AND DimensionValue IS NULL
	END
	RETURN @TouchWeight
END
--ENd GetWeight

