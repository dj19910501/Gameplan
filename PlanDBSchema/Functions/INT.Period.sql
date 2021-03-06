IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[INT].Period') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION [INT].Period
GO

CREATE FUNCTION [INT].[Period](@tacticStartDate DATE, @actualDate DATE)
RETURNS VARCHAR(10)
AS 
BEGIN
    RETURN 
	CASE WHEN DATEPART(YEAR, @tacticStartDate)<DATEPART(YEAR, @actualDate)
		 THEN 'Y' + CAST(DATEDIFF(YEAR, @tacticStartDate, @actualDate)*12 AS VARCHAR(10)) + CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
		 ELSE 'Y' +  CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
	END
END
GO