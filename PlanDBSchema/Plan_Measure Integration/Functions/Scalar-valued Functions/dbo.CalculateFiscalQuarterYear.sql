IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateFiscalQuarterYear') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION CalculateFiscalQuarterYear
GO
--Create procedure and function

-- =============================================
-- Author:		Arpita Soni
-- Create date: 05/01/2015
-- Description:	Calculate fiscal quarter and year values
-- =============================================
-- SELECT [dbo].[CalculateFiscalQuarterYear]('FQ','01/14/2015')
CREATE FUNCTION [dbo].[CalculateFiscalQuarterYear]
(	
	-- Add the parameters for the function here
	@ViewByValue varchar(10),
	@Date datetime
)
RETURNS NVARCHAR(10)
AS
BEGIN

	IF (@ViewByValue='FQ')
	BEGIN
		RETURN (SELECT TOP 1 + 'Q' + CONVERT(NVARCHAR,FiscalQuarter) + ' ' + CONVERT(NVARCHAR,FiscalYear) FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
	ELSE IF (@ViewByValue='FM')
	BEGIN
		RETURN (SELECT TOP 1 + CONVERT(NVARCHAR, UPPER(LEFT([MonthName], 1)) + LOWER(RIGHT([MonthName], LEN([MonthName]) - 1))) + '-' + CONVERT(NVARCHAR,fiscalyear) FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
	ELSE 
	BEGIN
		RETURN (SELECT TOP 1 FiscalYear FROM ProcessedFQY WHERE StartDate<=@Date AND EndDate>=@Date)
	END
return 0
END

