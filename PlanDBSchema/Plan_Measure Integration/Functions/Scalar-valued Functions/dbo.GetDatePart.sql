IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDatePart') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetDatePart
GO

CREATE FUNCTION [dbo].[GetDatePart](@DateValue nvarchar(1000),@ViewByValue NVARCHAR(10),@STARTDATE DATETIME, @ENDDATE DATETIME)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @ColumnPart NVARCHAR(200)
	SET @ColumnPart = ''
	IF(@ViewByValue = 'Q') 
				BEGIN
					SET @ColumnPart = 'Q' + CAST(DATEPART(Q,CAST(@DateValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue = 'Y') 
				BEGIN
					SET @ColumnPart = CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue = 'M') 
				BEGIN
					SET @ColumnPart = SUBSTRING(DateName(MONTH,CAST(@DateValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(@DateValue AS DATE)) AS NVARCHAR)
				END
				ELSE IF(@ViewByValue='W')
				BEGIN
					IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
					BEGIN
						SET @ColumnPart=LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR))))
					END
					ELSE 
					BEGIN
						SET @ColumnPart=LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(@DateValue AS NVARCHAR)) - 6, CAST(@DateValue AS NVARCHAR)) AS NVARCHAR)))
					END
				END
				ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
				BEGIN
					SET @ColumnPart= [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(@DateValue AS DATETIME))
				END
				ELSE 
				BEGIN
					SET @ColumnPart = @DateValue
				END
	
	RETURN @ColumnPart;

END
                
