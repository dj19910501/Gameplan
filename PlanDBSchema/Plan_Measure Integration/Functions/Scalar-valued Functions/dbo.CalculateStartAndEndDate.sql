IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateStartAndEndDate') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION CalculateStartAndEndDate
GO
CREATE FUNCTION [CalculateStartAndEndDate]
(
	@DateDimension NVARCHAR(500),
	@ViewByValue NVARCHAR(15),
	@STARTDATE date, 
	@ENDDATE date
)
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE @StdStartDate DATETIME, @StdEndDate DATETIME, @ReturnValue NVARCHAR(100);
	
	IF(@ViewByValue='Y')
	BEGIN
		SET @StdStartDate=CONVERT(DATETIME, CAST(@DateDimension AS NVARCHAR) + '-01-01',20)
		SET @StdEndDate= CONVERT(DATETIME, CAST(@DateDimension AS NVARCHAR)+ '-12-31',20)
	END
	ELSE IF(@ViewByValue='Q')
	BEGIN
		DECLARE @MonthDigitQ INT;
		SET @MonthDigitQ = (3 * (SUBSTRING(@DateDimension,2,1) - 1)) + 1;
		SET @StdStartDate = CONVERT(DATETIME,SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitQ AS NVARCHAR)+ '-1',20)
		SET @StdEndDate =  DATEADD(DAY,-1,DATEADD(MONTH,1,CONVERT(DATETIME,CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitQ + 2 AS NVARCHAR)+ '-01' AS NVARCHAR),20)))
	END
	ELSE IF(@ViewByValue='M')
	BEGIN
		DECLARE @MonthDigitM INT;
		SET @MonthDigitM = DATEPART(MM, LEFT(@DateDimension,3) + ' ' + SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4))
		SET @StdStartDate = CONVERT(DATETIME,SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitM AS NVARCHAR)+ '-1',20)
		SET @StdEndDate =  DATEADD(DAY,-1,DATEADD(MONTH,1,CONVERT(DATETIME,CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) + '-' + CAST(@MonthDigitM AS NVARCHAR)+ '-01' AS NVARCHAR),20)))
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		DECLARE @MonthDigitW INT, @Year INT,@Date INT;
		IF(CHARINDEX('-',@DateDimension) = 0)
		BEGIN
			SET @Year = CAST(LEFT(@STARTDATE,4) AS INT)
			SET @Date = CAST(SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,LEN(@DateDimension)-CHARINDEX(' ',@DateDimension)) AS INT)
		END
		ELSE 
		BEGIN
			SET @Year = CAST(SUBSTRING(@DateDimension,CHARINDEX('-',@DateDimension)+1,4) AS INT)
			SET @Date = SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,CHARINDEX('-',@DateDimension)-CHARINDEX(' ',@DateDimension)-1)
			END
		SET @MonthDigitW = DATEPART(MM, LEFT(@DateDimension,3) + ' 01 ' + CAST(@Year AS NVARCHAR))
		SET @StdStartDate = CONVERT(DATETIME,CAST(@Year AS NVARCHAR) + '-' + CAST(@MonthDigitW AS NVARCHAR) + '-' + CAST(@Date AS NVARCHAR),20)
		SET @StdEndDate = DATEADD(DAY,6,CONVERT(DATETIME,CAST(@Year AS NVARCHAR) + '-' + CAST(@MonthDigitW AS NVARCHAR) + '-' + CAST(@Date AS NVARCHAR),20))
	END
	ELSE IF(@ViewByValue='FQ')
	BEGIN
		SELECT @StdStartDate=StartDate, @StdEndDate=EndDate FROM ProcessedFQY WHERE [FiscalQuarter]=SUBSTRING(@DateDimension,2,1) and [FiscalYear]=SUBSTRING(@DateDimension,CHARINDEX(' ',@DateDimension)+1,4)
	END
	ELSE IF(@ViewByValue='FY')
	BEGIN
		SELECT @StdStartDate=StartDate FROM ProcessedFQY WHERE [FiscalQuarter]=1 and [FiscalYear]=@DateDimension
		SELECT @StdEndDate=EndDate FROM ProcessedFQY WHERE [FiscalQuarter]=4 and [FiscalYear]=@DateDimension
	END
	
	-- Compare with filter date preset start and end date 
	IF(@StdStartDate < @StartDate)
		SET @STARTDATE = @StdStartDate
	IF(@StdEndDate < @EndDate)
		set @ENDDATE = @StdEndDate
	
	SET @ReturnValue = CAST(@STARTDATE AS NVARCHAR) +','+ CAST(@ENDDATE AS NVARCHAR)
	RETURN @ReturnValue;
END

GO
