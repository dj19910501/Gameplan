-- =============================================
-- Author: Viral
-- Create date: 09/19/2016
-- Description:	Get Plan Calendar Start & End Date
-- =============================================


/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 09/22/2016 1:48:03 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetPlanGanttStartEndDate]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 09/22/2016 1:48:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'


CREATE FUNCTION [dbo].[fnGetPlanGanttStartEndDate]
(
	@timeframe varchar(255)
)
RETURNS @start_endDate Table(
	startdate datetime,
	enddate datetime
)
AS
BEGIN
	
	--SELECT * from fnGetPlanGanttStartEndDate(''thisyear'')

	Declare @varThisYear varchar(10)=''thisyear''
	Declare @varThisQuarter varchar(15)=''thisquarter''
	Declare @varThisMonth varchar(10)=''thismonth''
	Declare @Start_Date datetime
	Declare @End_Date datetime
	
	Declare @startDate datetime
	Declare @endDate datetime

	IF(IsNull(@timeframe,'''')='''')
	BEGIN
		SET @timeframe =''thisyear''
	END

	-- Get PlanYear by timeframe.
	BEGIN

	Declare @planYear varchar(255)
	Declare @Fyear varchar(10)
	BEGIN
		IF(ISNUMERIC(@timeframe) = 1)
		BEGIN
			SET @planYear = @timeframe 
		END
		ELSE
		BEGIN
			Select  @Fyear = val from [dbo].comma_split(@timeframe,''-'')
			IF(ISNUMERIC(@Fyear) = 1)
			BEGIN
				SET @planYear = @Fyear 
			END
			ELSE
			BEGIN
				SET @planYear = CAST(DATEPART(YYYY,GETDATE()) as varchar(10))
			END
		END
	END

	END

	SET @startDate = GETDATE()
	SET @endDate = GETDATE()

	IF(@timeframe = @varThisYear)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 1, 1)
		SET @End_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 12, 31)	-- .AddTicks(-1);
	END
	
	ELSE IF(@timeframe = @varThisQuarter)
	BEGIN

		Declare @currentQuarter int 
		SET @currentQuarter = ((DATEPART(MM,@startDate) - 1) / 3) + 1
		
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,@startDate), (@currentQuarter - 1) * 3 + 1, 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,3,@Start_Date))		-- .AddTicks(-1); 
	END
	
	ELSE IF(@timeframe = @varThisMonth)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,@startDate), DATEPART(MM,GETDATE()), 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,1,@Start_Date))
	END

	ELSE IF(ISNUMERIC(@timeframe) = 1)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (CAST(@timeframe as int), 1, 1)
		SET @End_Date = DATEFROMPARTS (CAST(@timeframe as int), 12, 31)		-- .AddTicks(-1);
	END
	
	ELSE
	BEGIN

		Declare @Year1 int
		Declare @Year2 int
		
		SELECT  @Year1 = MIN(Cast(val as int)), @Year2 = MAX(Cast(val as int)) FROM [dbo].comma_split(@timeframe,''-'')
		
		SET @Start_Date = DATEFROMPARTS (@Year1, 1, 1)
		SET @End_Date = DATEFROMPARTS (@Year2 , 12, 31)		-- .AddTicks(-1);
	END

	INSERT INTO @start_endDate  SELECT @Start_Date ,@End_Date 
	RETURN 
END
' 
END

GO
