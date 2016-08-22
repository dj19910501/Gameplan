USE [Hive9GameplanJuly2016]
GO
/****** Object:  UserDefinedFunction [INT].[Period]    Script Date: 8/22/2016 4:14:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [INT].[Period](@tacticStartDate DATE, @actualDate DATE)
RETURNS VARCHAR(10)
AS 
BEGIN

    RETURN 
	CASE WHEN DATEPART(YEAR, @tacticStartDate)<DATEPART(YEAR, @actualDate)
		 THEN 'Y' + CAST(((DATEPART(YEAR, @actualDate) - DATEPART(YEAR, @tacticStartDate))*12) AS VARCHAR(10)) + CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
		 ELSE 'Y' +  CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
	END
END

