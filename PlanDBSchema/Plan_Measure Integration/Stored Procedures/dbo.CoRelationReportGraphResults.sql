IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CoRelationReportGraphResults') AND xtype IN (N'P'))
    DROP PROCEDURE CoRelationReportGraphResults
GO


CREATE PROCEDURE [CoRelationReportGraphResults]  
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,
@SubDashboardMainDimensionTable int = 0,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'	-- Added by Arpita Soni on 11/02/2015 for PL ticket #511


AS
BEGIN
SET NOCOUNT ON;

DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX)
SET @InnerJoin1 = ''; SET @Count= 1

DECLARE @HeatMapQuery NVARCHAR(MAX), @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere NVARCHAR(1000)
SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = '';SET @HeatMapWhere = ''
SET @HeatMapQuery = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1'
SET @HeatMapQuery =  @HeatMapQuery + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 

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
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
				SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  

				SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 'd2.d' + CAST(@tempIndex AS NVARCHAR)
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR)  

				SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 'd2.d' + CAST(@tempIndex AS NVARCHAR)
			END
			
			
			
			SET @Count = @Count + 1

	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @IsColumn, @DimensionName,@IsDateDimension
	END
Close Column_Cursor
Deallocate Column_Cursor

SET @HeatMapGrpuoColumn = @HeatMapDimColumn

/*
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D1',2);
END
SET @FilterCondition = ISNULL(@FilterCondition,'')
IF(@FilterCondition != '')
SET @FilterCondition = ' WHERE' + REPLACE(@FilterCondition,'#',' AND ')
*/

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
				SET @HeatMapDimColumn = @HeatMapDimColumn + ' ,' + @AggregartedMeasure + ','
				SET @HeatMapWhere = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		END
		ELSE
		BEGIN
				SET @AggregartedMeasure = ' d' + CAST(@Count + 1 AS NVARCHAR) + '.Value '
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN ' +@DIMENSIONTABLENAME + 'Value d' + CAST(@Count + 1 AS NVARCHAR) + ' on d1.' + @DIMENSIONTABLENAME + ' = d' + CAST(@Count + 1 AS NVARCHAR) + '.'   + + @DIMENSIONTABLENAME
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
SET @HeatMapQuery = @HeatMapQuery + @InnerJoin1 + @HeatMapWhere --+ ' Group By ' + @HeatMapGrpuoColumn

DECLARE @CreateTable NVARCHAR(MAX)
SET @CreateTable = N'DECLARE @REPORTGRAPHHM' + CAST(@ReportGraphID AS NVARCHAR) + '  TABLE (d1 int, d2 int, val float);';

DECLARE @Retval table(d1 int, d2 int, val float) 

--Added by kausha somaiya for filter condition and For Exclude column
DECLARE @FilterCondition NVARCHAR(MAX);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	set @HeatMapQuery=@HeatMapQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
END
--End Filtercondition

PRINT @HeatMapQuery;

INSERT INTO  @Retval (d1,d2,val)
exec CorrelationCalculation  @HeatMapQuery ;
select 
		DisplayValue1 = CASE WHEN DD1.IsDateDimension = 1 THEN dbo.GetDatePart(d3.DisplayValue,@ViewByValue,@STARTDATE,@ENDDATE) ELSE d3.DisplayValue END,
		DisplayValue2 = CASE WHEN DD2.IsDateDimension = 1 THEN dbo.GetDatePart(d4.DisplayValue,@ViewByValue,@STARTDATE,@ENDDATE) ELSE d4.DisplayValue END,
		MeasureValue = round(r.val,2)
FROM @Retval R
INNER JOIN DimensionValue d3 ON d3.id = R.d1
LEFT JOIN Dimension DD1 ON DD1.id = d3.DimensionID
INNER JOIN DimensionValue d4 ON d4.id = R.d2
LEFT JOIN Dimension DD2 ON DD2.id = d4.DimensionID
where 
d3.DisplayValue NOT IN(SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID =@ReportGraphId) AND 
d3.DisplayValue NOT IN (select * from UF_CSVToTable(dbo.RestrictedDimensionValues(d3.dimensionId,@UserId,@RoleId))) AND
d4.DisplayValue NOT IN(SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID =@ReportGraphId) AND
d4.DisplayValue NOT IN (select * from UF_CSVToTable(dbo.RestrictedDimensionValues(d4.DimensionId,@UserId,@RoleId)))
ORDER BY d3.OrderValue, d4.OrderValue


RETURN
END

GO
