IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestQueryForCoRelation') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetTTestQueryForCoRelation
GO
CREATE FUNCTION [GetTTestQueryForCoRelation] 
(
@Dimension1 nvarchar(500),
@Dimension2 nvarchar(500),
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,
@SubDashboardMainDimensionTable int = 0	
)
RETURNS NVARCHAR(4000)
AS
BEGIN
	
DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX)
SET @InnerJoin1 = ''; SET @Count= 1

DECLARE @HeatMapQuery NVARCHAR(MAX), @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere NVARCHAR(1000),@HeatMapWhere2 NVARCHAR(1000)
SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = '';SET @HeatMapWhere = ''
SET @HeatMapQuery = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1'
SET @HeatMapQuery =  @HeatMapQuery + ' FULL OUTER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 
SET @HeatMapWhere2 =''

DECLARE Column_Cursor CURSOR FOR
SELECT A.Dimensionid, A.AxisName, '[' + Replace(D.Name,' ','') + ']' DimensionName,D.IsDateDimension FROM ReportGraph G
INNER JOIN ReportAxis A ON G.id = A.ReportGraphId
INNER JOIN Dimension D ON A.Dimensionid = D.id  
WHERE G.id = @ReportGraphID
ORDER BY A.AxisName
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	WHILE @@FETCH_STATUS = 0
		BEGIN
			
			IF ((@SubDashboardOtherDimensionTable > 0) AND (@SubDashboardMainDimensionTable = @Dimensionid))
			BEGIN
				SET @Dimensionid = @SubDashboardOtherDimensionTable
			END
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			IF(@DATEFIELD = 'D'  + CAST(@tempIndex AS NVARCHAR))
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
				SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR)  
				
				IF(@Dimensionid=SUBSTRING(@Dimension1,2,CHARINDEX(':',@Dimension1)-2))
					SET @HeatMapWhere2 = @HeatMapWhere2 + ' AND d' + CAST(@Count + 2 AS NVARCHAR)+ '.DisplayValue=''' +REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':','') +''''
				IF (@Dimensionid=SUBSTRING(@Dimension2,2,CHARINDEX(':',@Dimension2)-2))
					SET @HeatMapWhere2 = @HeatMapWhere2 + ' AND d' + CAST(@Count + 2 AS NVARCHAR)+ '.DisplayValue=''' +REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':','') +''''
			END
					
			
			SET @Count = @Count + 1

	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	END
Close Column_Cursor
Deallocate Column_Cursor

SET @HeatMapGrpuoColumn = @HeatMapDimColumn

DECLARE @AggregationType NVARCHAR(20), @MEASURENAME NVARCHAR(200),@SymbolType NVARCHAR(100), @MEASUREID INT, @MEASURETABLENAME NVARCHAR(200), @MeCount INT;
SET @MeCount =1; 
SET @SymbolType = '';

DECLARE Measure_Cursor CURSOR FOR
select MeasureName, Measureid, MeasureTableName,ISNULL(AggregationType,'') AggregationType,ISNULL(SymbolType,'') SymbolType from dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue) ORDER BY ColumnOrder
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
WHILE @@FETCH_STATUS = 0
	Begin
		
		DECLARE @AggregartedMeasure NVARCHAR(200)

		IF(@MeCount = 1)
		BEGIN
				SET @AggregartedMeasure = 'd1.value'
				SET @HeatMapDimColumn = @HeatMapDimColumn + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END  + @AggregartedMeasure + ','
				SET @HeatMapWhere = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		ELSE
		BEGIN
				SET @AggregartedMeasure = ' d' + CAST(@Count + 1 AS NVARCHAR) + '.Value '
				SET @InnerJoin1 = @InnerJoin1 + ' FULL OUTER JOIN ' +@DIMENSIONTABLENAME + 'Value d' + CAST(@Count + 1 AS NVARCHAR) + ' on d1.' + @DIMENSIONTABLENAME + ' = d' + CAST(@Count + 1 AS NVARCHAR) + '.'   + + @DIMENSIONTABLENAME
				SET @HeatMapDimColumn = @HeatMapDimColumn  + @AggregartedMeasure 

				SET @HeatMapWhere = @HeatMapWhere + ' AND d' + CAST(@Count + 1 AS NVARCHAR) + '.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		
		SET @Count = @Count + 1
		SET @MeCount = @MeCount + 1
	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor

SET @HeatMapQuery = REPLACE(@HeatMapQuery,'#COLUMNS#',@HeatMapDimColumn)
SET @HeatMapQuery = @HeatMapQuery + @InnerJoin1 + @HeatMapWhere + @HeatMapWhere2 --+ ' Group By ' + @HeatMapGrpuoColumn

DECLARE @CreateTable NVARCHAR(MAX)
SET @CreateTable = N'DECLARE @REPORTGRAPHHM' + CAST(@ReportGraphID AS NVARCHAR) + '  TABLE (d1 int, d2 int, val float);';

DECLARE @Retval table(d1 int, d2 int, val float) 

--Added by kausha somaiya for filter condition and For Exclude column
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	SET @HeatMapQuery=@HeatMapQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
END
--End Filtercondition

RETURN @HeatMapQuery;

END

GO
