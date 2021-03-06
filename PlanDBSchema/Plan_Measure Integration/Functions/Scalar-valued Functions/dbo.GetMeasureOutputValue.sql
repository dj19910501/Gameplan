IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasureOutputValue') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasureOutputValue
GO
CREATE FUNCTION [GetMeasureOutputValue]
(
	@OriginalValue decimal(18,2),
	@MeasureId INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @OutputValue NVARCHAR(50);
	SET @OutputValue = '';
	SELECT TOP 1 @Outputvalue=ISNULL(Value,CAST(@OriginalValue AS NVARCHAR)) FROM MeasureOutputValue
	WHERE MeasureId=@MeasureId and @OriginalValue BETWEEN LowerLimit AND UpperLimit
	IF(@OutputValue='')
		SET @OutputValue = CAST(@OriginalValue AS NVARCHAR)

	RETURN @OutputValue
END

GO
