IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CalculateFiscalYearQuarter') AND xtype IN (N'P'))
    DROP PROCEDURE CalculateFiscalYearQuarter
GO
CREATE PROCEDURE [dbo].[CalculateFiscalYearQuarter]
AS
BEGIN

IF EXISTS (SELECT TOP 1 * FROM FiscalQuarterYear)
BEGIN
--do what you need if exists


IF EXISTS(SELECT * FROM FiscalQuarterYear WHERE EndDate IS NULL)
BEGIN
	UPDATE FiscalQuarterYear SET EndDate = DATEADD(DAY,-1,DATEADD(mm, DATEDIFF(m,0,StartDate)+1,0)) WHERE EndDate IS NULL
END

DECLARE @FQ INT, @FY INT, @Date DATETIME
DECLARE FQY_Cursor CURSOR FOR
SELECT DISTINCT FiscalYear FROM FiscalQuarterYear ORDER BY FiscalYear
OPEN FQY_Cursor
FETCH NEXT FROM FQY_Cursor
INTO @FY
       WHILE @@FETCH_STATUS = 0
       BEGIN
              DECLARE @RecordCount INT, @FirstQ INT, @MaxQ INT, @MaxStartDate datetime, @MaxEndDate datetime
              SELECT @RecordCount = COUNT(*) FROM FiscalQuarterYear FY1 WHERE FY1.FiscalYear = @FY

              IF(@RecordCount > 0 AND @RecordCount < 12)
              BEGIN
					SELECT @MaxQ = MAX(FiscalQuarter) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @FirstQ = MAX(FiscalQuarter) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @MaxStartDate = MAX(StartDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SELECT @MaxEndDate = MAX(EndDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY

					DECLARE @EnteredMonth INT
					DECLARE @intFlag INT
					DECLARE @TotalRow INT

					SELECT @EnteredMonth = MAX([Month]) FROM FiscalQuarterYear WHERE FiscalYear = @FY
					SET @intFlag = 1
					SET @TotalRow = 12
					DECLARE @IncrCnt int
					SET @IncrCnt = 2
					WHILE (@intFlag <= @TotalRow)
					BEGIN
						DECLARE @MonthId int
						SET @MonthId = @EnteredMonth-@intFlag
						IF (@EnteredMonth > @intFlag)
						BEGIN
							IF(@intFlag = 1)
							BEGIN
								IF(@MaxQ <> 1)
								BEGIN
									SET @MaxQ  = @MaxQ - 1
								END
								ELSE
								BEGIN
									SET @MaxQ = 4
								END
							END
							IF NOT EXISTS (SELECT * FROM FiscalQuarterYear WHERE FiscalYear = @FY AND [Month] = @MonthId)
							BEGIN
								IF((SELECT COUNT(*) FROM FiscalQuarterYear WHERE FiscalQuarter = @MaxQ) > 2)
								BEGIN
									IF(@MaxQ <> 1)
									BEGIN
										SET @MaxQ  = @MaxQ - 1
									END
									ELSE
									BEGIN
										SET @MaxQ = 4
									END
								END
							DECLARE @MaxStDt datetime
								SELECT @MaxStDt = MIN(StartDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
								INSERT INTO FiscalQuarterYear (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month]) 
								SELECT @MaxQ,@FY, DATEADD(MONTH, -1, @MaxStDt), DATEADD(DAY, -1, @MaxStDt),@MonthId
							END
						END
						ELSE IF (@EnteredMonth < @intFlag)
						BEGIN
							IF(@IncrCnt = 2)
							BEGIN
								SET @MaxQ = @FirstQ
							END
							IF((SELECT COUNT(*) FROM FiscalQuarterYear WHERE FiscalQuarter = @MaxQ) > 2)
							BEGIN
								IF(@MaxQ <> 4)
								BEGIN
									SET @MaxQ  = @MaxQ + 1
								END
								ELSE
								BEGIN
									SET @MaxQ = 1
								END
							END
							IF NOT EXISTS (SELECT * FROM FiscalQuarterYear WHERE FiscalYear = @FY AND [Month] = @MonthId)
								DECLARE @MaxEnDt datetime
							SELECT @MaxEnDt = MAX(EndDate) FROM FiscalQuarterYear WHERE FiscalYear = @FY
							INSERT INTO FiscalQuarterYear (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month]) 
							SELECT @MaxQ ,@FY, DATEADD(DAY, 1, @MaxEnDt), DATEADD(DAY, -1, DATEADD(MONTH, 1, DATEADD(DAY, 1, @MaxEnDt))),@intFlag
							SET @IncrCnt = @IncrCnt + 1
						END

						SET @intFlag = @intFlag + 1
					END
              END

       FETCH NEXT FROM FQY_Cursor
       INTO @FY
       END
Close FQY_Cursor
Deallocate FQY_Cursor

DELETE FROM ProcessedFQY;

;WITH CalculateEndDate AS(
	SELECT 
		FQY.FiscalQuarter,
		FQY.FiscalYear,
		FQY.StartDate,
		EndDate = 
			CASE WHEN FQY.EndDate IS NOT NULL THEN FQY.EndDate 
				ELSE (
					CASE WHEN FQY.FiscalQuarter = 4 THEN (SELECT TOP 1 DATEADD(DAY,-1,T1.StartDate) FROM FiscalQuarterYear T1 WHERE T1.FiscalYear = FQY.FiscalYear + 1 AND T1.FiscalQuarter = 1)
						ELSE (SELECT TOP 1 DATEADD(DAY,-1,T1.StartDate) FROM FiscalQuarterYear T1 WHERE T1.FiscalYear = FQY.FiscalYear AND T1.FiscalQuarter = FQY.FiscalQuarter + 1) 
					END)
			END,
		FQY.[Month]
	FROM FiscalQuarterYear FQY
)

INSERT INTO ProcessedFQY
SELECT FiscalQuarter, FiscalYear, StartDate, EndDate = CASE WHEN EndDate IS NULL THEN DATEADD(DAY,-1,DATEADD(MONTH,3,StartDate)) ELSE EndDate END, [Month], UPPER(SUBSTRING(DateName( month , DateAdd( month , [Month] , 0 ) - 1 ),1,3)) FROM CalculateEndDate  ORDER BY [Month]

--calcluate the remaining year and quarter
DECLARE @MinYear INT, @MaxYear INT, @Year INT;
DECLARE @StartDate DATE,@EndDate DATE;


SET @MinYear = 1999; SET @MaxYear = 2029;

--Minimum  to 2000
SELECT @Year = MIN(FiscalYear)  FROM ProcessedFQY 
SET @Year = @Year - 1
WHILE @Year > @MinYear
BEGIN
	DECLARE @intPFlag INT
	DECLARE @TotalPRow INT
	DECLARE @MonthNo int
	DECLARE @FisQtr int
	SET @intPFlag = 1
	SET @TotalPRow = 12

	SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year + 1 AND [Month] = 1

	WHILE (@intPFlag <= @TotalPRow)
	BEGIN
		SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)))

		INSERT INTO ProcessedFQY VALUES 
		(@FisQtr,@Year, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)),DATEADD(DAY, -1, DATEADD(MONTH, 1, DATEADD(YEAR, -1, DATEADD(MONTH,(@intPFlag - 1),@StartDate)))),@MonthNo, UPPER(SUBSTRING(DateName( month , DateAdd( month , @MonthNo , 0 ) - 1 ),1,3)))

		SET @intPFlag = @intPFlag + 1
	END

	SET @Year = @Year - 1
END

--Maximum to 2029
SELECT @Year = MAX(FiscalYear)  FROM ProcessedFQY 
--Modified by Arpita Soni for ticket #531 on 09/28/2015
WHILE @Year < @MaxYear
BEGIN
	DECLARE @intPMaxFlag INT
	DECLARE @TotalPMaxRow INT
	SET @intPMaxFlag = 1
	SET @TotalPMaxRow = 12

	SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year AND [Month] = 12

	WHILE (@intPMaxFlag <= @TotalPMaxRow)
	BEGIN
		SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH,DATEADD(MONTH,((@intPMaxFlag+1) - 1),@StartDate))

		INSERT INTO ProcessedFQY VALUES 
		(@FisQtr,@Year+1,DATEADD(MONTH,((@intPMaxFlag+1) - 1),@StartDate),DATEADD(MONTH,((@intPMaxFlag+1) - 1),@EndDate),@MonthNo, UPPER(SUBSTRING(DateName( month , DateAdd( month , @MonthNo , 0 ) - 1 ),1,3)))

		SET @intPMaxFlag = @intPMaxFlag + 1
	END

	SET @Year = @Year +1
END

--All remaining years
DECLARE @intPRemFlag INT
DECLARE @TotalPRemRow INT
SET @Year = 2000;
WHILE @Year < @MaxYear + 1
BEGIN
	--DECLARE @RecordCount INT;
	SELECT @RecordCount = COUNT(*) FROM ProcessedFQY WHERE FiscalYear = @Year;
	IF(@RecordCount = 0)
	BEGIN
		IF(@Year = 2000)
		BEGIN
				SET @intPRemFlag = 1
				SET @TotalPRemRow = 12
				--Q1
				SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year + 1 AND [Month] = 1;
		END
		ELSE 
		BEGIN
				SET @intPRemFlag = 1
				SET @TotalPRemRow = 12

				SELECT @StartDate = StartDate, @EndDate = EndDate FROM ProcessedFQY WHERE FiscalYear = @Year - 1 AND [Month] = 12;
		END

		WHILE (@intPRemFlag <= @TotalPRemRow)
		BEGIN
			SELECT TOP 1 @MonthNo = [Month], @FisQtr = FiscalQuarter FROM ProcessedFQY WHERE DATEPART(MONTH, StartDate) = DATEPART(MONTH,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@StartDate)))

			INSERT INTO ProcessedFQY (FiscalQuarter,FiscalYear,StartDate,EndDate,[Month],[MonthName]) VALUES
			--SELECT 1,@Year,DATEADD(DAY,1,@EndDate),DATEADD(MONTH,3,DATEADD(DAY,1,@EndDate))
			(@FisQtr,@Year,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@StartDate)),DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@EndDate)),@MonthNo, UPPER(SUBSTRING(DATENAME(MONTH,DATEADD(YEAR, 1, DATEADD(MONTH,(@intPRemFlag - 1),@EndDate))),1,3)))

			SET @intPRemFlag = @intPRemFlag + 1
		END
	END
	SET @Year = @Year + 1
END
--UPDATE ProcessedFQY SET [MonthName] = UPPER(SUBSTRING(DATENAME(MONTH,DATEADD(MONTH, [Month], -1 )),1,3))
END
END
GO