/*  Insertion Start:11/04/2016 Kausha #759  Added Following function to get week list  */
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE   object_id = OBJECT_ID(N'fGetWeeksList') AND type IN (  N'FN', N'IF', N'TF', N'FS', N'FT') ) 
BEGIN
	DROP function [fGetWeeksList]
END
GO
CREATE FUNCTION [dbo].[fGetWeeksList]
(
    @StartDate DATETIME 
   ,@EndDate DATETIME 
)
RETURNS 
TABLE 
AS
RETURN
(

SELECT DATEADD(DAY,-(DATEPART(DW,DATEADD(WEEK, x.number, @StartDate))-1),DATEADD(WEEK, x.number, @StartDate)) as [StartDate]
      ,DATEADD(DAY,-(DATEPART(DW,DATEADD(WEEK, x.number + 1, @StartDate))-1) ,DATEADD(WEEK, x.number + 1, @StartDate)) AS [EndDate]
FROM master.dbo.spt_values x
WHERE x.type = 'P' AND x.number <= DATEDIFF(WEEK, @StartDate, DATEADD(WEEK,0,CAST(@EndDate AS DATE))))
/*  Insertion End:11/04/2016 Kausha #759  Added Following function to get week list  */
--Please do not add any script below this line
