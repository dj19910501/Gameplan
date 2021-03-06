IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetGraphMeasure') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetGraphMeasure
GO
CREATE FUNCTION [dbo].[GetGraphMeasure](@ReportGraphID INT, @ViewByValue NVARCHAR(15))
RETURNS @measureTable TABLE(MeasureName NVARCHAR(50),Measureid INT,MeasureTableName NVARCHAR(50), AggregationType NVARCHAR(50),
		SymbolType NVARCHAR(50),ColumnOrder NVARCHAR(50),DisplayInTable NVARCHAR(50),DrillDownWhereClause NVARCHAR(500))
AS
BEGIN
	
	INSERT INTO @measureTable
	SELECT M.Name,
		   M.id,
		   M.MeasureTableName,
		   M.AggregationType,
		   RGC.SymbolType,
		   RGC.ColumnOrder,
		   RGC.DisplayInTable,
		   M.DrillDownWhereClause
	FROM ReportGraphColumn RGC 
	INNER JOIN Measure M ON M.id =  CASE 
									WHEN @ViewByValue = 'Q' OR @ViewByValue = 'FQ' THEN ISNULL(RGC.QuarterMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'Y' OR @ViewByValue = 'FY' THEN ISNULL(RGC.YearMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'M' OR @ViewByValue = 'FM' THEN ISNULL(RGC.MonthMeasureId,RGC.MeasureId) 
									WHEN @ViewByValue = 'W' THEN ISNULL(RGC.WeekMeasureId,RGC.MeasureId) 
									END
	WHERE RGC.ReportGraphId = @ReportGraphID
	RETURN 
END;
/*End of #530 */

