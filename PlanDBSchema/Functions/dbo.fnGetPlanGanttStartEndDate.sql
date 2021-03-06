-- =============================================
-- Author: Viral
-- Create date: 09/19/2016
-- Description:	Get Plan Calendar Start & End Date
-- =============================================


/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 10/05/2016 5:17:16 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetPlanGanttStartEndDate]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetPlanGanttStartEndDate]    Script Date: 10/05/2016 5:17:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetPlanGanttStartEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
-- select * from [dbo].[fnGetPlanGanttStartEndDate](''thismonth'')

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
	
	-- This line is keep commented to test this function and verify data from SQL.
	--SELECT * from fnGetPlanGanttStartEndDate(''thisyear'') 

	-- Declare local variables
	BEGIN
		Declare @varThisYear varchar(10)=''thisyear''
		Declare @varThisQuarter varchar(15)=''thisquarter''
		Declare @varThisMonth varchar(10)=''thismonth''
		Declare @Start_Date datetime
		Declare @End_Date datetime
	END
	
	-- Check that if timeframe value is blank then set ''thisyear'' as default timeframe for Calendar
	IF(IsNull(@timeframe,'''')='''')
	BEGIN
		SET @timeframe =@varThisYear
	END


	-- Check that if timeframe value is ''thisyear'' then set Start Date & End date to ''01/01/2016'' to ''31/12/2016'' respectively
	IF(@timeframe = @varThisYear)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 1, 1)
		SET @End_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), 12, 31)	
	END
	
	-- Check that if timeframe value is ''thisquarter'' then set Start Date & End date to ''01/07/2016'' to ''30/09/2016'' respectively
	ELSE IF(@timeframe = @varThisQuarter)
	BEGIN

		Declare @currentQuarter int 
		SET @currentQuarter = ((DATEPART(MM,GETDATE()) - 1) / 3) + 1
		
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), (@currentQuarter - 1) * 3 + 1, 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,3,@Start_Date))		
	END
	
	-- Check that if timeframe value is ''thismonth'' then set Start Date & End date to ''01/09/2016'' to ''30/09/2016'' respectively
	ELSE IF(@timeframe = @varThisMonth)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (DATEPART(yyyy,GETDATE()), DATEPART(MM,GETDATE()), 1)
		SET @End_Date = DATEADD(DD,-1,DATEADD(MM,1,@Start_Date))
	END

	-- Check that if timeframe value is ''2016'' then set Start Date & End date to ''01/01/2016'' to ''31/12/2016'' respectively
	ELSE IF(ISNUMERIC(@timeframe) = 1)
	BEGIN
		SET @Start_Date = DATEFROMPARTS (CAST(@timeframe as int), 1, 1)
		SET @End_Date = DATEFROMPARTS (CAST(@timeframe as int), 12, 31)		
	END
	
	-- When timeframe multiyear(ex. 2016-2017) then set Start Date & End date to ''01/01/2016'' to ''31/12/2017'' respectively
	ELSE
	BEGIN

		Declare @Year1 int
		Declare @Year2 int
		
		SELECT  @Year1 = MIN(Cast(val as int)), @Year2 = MAX(Cast(val as int)) FROM [dbo].comma_split(@timeframe,''-'')	-- Split timeframe ''2016-2017'' value.
		
		SET @Start_Date = DATEFROMPARTS (@Year1, 1, 1)
		SET @End_Date = DATEFROMPARTS (@Year2 , 12, 31)		
	END

	INSERT INTO @start_endDate  SELECT @Start_Date ,@End_Date 
	RETURN 
END
' 
END

GO
