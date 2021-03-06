IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'MesureValueData') AND xtype IN (N'P'))
    DROP PROCEDURE MesureValueData
GO

CREATE PROCEDURE [MesureValueData]
	
	@ReportGraphID int,
	@IsGraph INT
AS
BEGIN
SET NOCOUNT ON;

DECLARE @Dimensionid int, @AxisName NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsPeriod BIT, @DatePart NVARCHAR(MAX),@DimensionTableName NVARCHAR(100)
DECLARE @CreateTable NVARCHAR(MAX), @Columns NVARCHAR(MAX), @SelectTable NVARCHAR(MAX), @InnerJoin NVARCHAR(MAX)
DECLARE @Columns1 NVARCHAR(MAX),@Columns2 NVARCHAR(MAX),@GroupBy NVARCHAR(MAX),@Where NVARCHAR(MAX)
DECLARE @Form NVARCHAR(MAX), @DateFilter  NVARCHAR(MAX),@ColumnPart NVARCHAR(1000)

SET @CreateTable = N'CREATE TABLE #REPORTTABLE' + CAST(@ReportGraphID AS NVARCHAR);
SET @SelectTable = N'INSERT INTO #REPORTTABLE' + CAST(@ReportGraphID AS NVARCHAR) + ' SELECT DISTINCT '
SET @Columns = '';SET @Columns1 = '';SET @Columns2= '';SET @InnerJoin = '';SET @Form = ' FROM ';SET @Count = 1;SET @GroupBy = ' GROUP BY ';
SET @Where = ' WHERE '
SET @DateFilter = ''

DECLARE @temp_Table TABLE (DimensionId INT, AxisName NVARCHAR(10),DimensionName NVARCHAR(100),IsPeriod BIT,DatePart NVARCHAR(20))
IF(@IsGraph = 1)	
BEGIN	
		INSERT @temp_Table
		SELECT A.Dimensionid, A.AxisName, '[' + REPLACE(D.Name,' ','') + ']' DimensionName,IsPeriod, DatePart FROM ReportGraph G 
		INNER JOIN ReportAxis A ON G.id = A.ReportID
		INNER JOIN Dimension D ON A.Dimensionid = D.id  
		WHERE G.id = @ReportGraphID
END
ELSE
BEGIN
		INSERT @temp_Table
		SELECT G.RowDimension Dimensionid, 'X' AxisName, '[' + REPLACE(D.Name,' ','') + ']' DimensionName,0 IsPeriod, NULL DatePart FROM ReportTable G 
		INNER JOIN Dimension D ON G.RowDimension = D.id  
		WHERE G.id = @ReportGraphID
END		

SELECT @DimensionTableName = 'd' + CAST(DimensionId AS NVARCHAR) FROM @temp_Table

DECLARE Column_Cursor CURSOR FOR
SELECT Dimensionid, AxisName, DimensionName,IsPeriod, DatePart FROM @temp_Table
ORDER BY Dimensionid
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @AxisName, @DimensionName,@IsPeriod,@DatePart
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF(ISNULL(@IsPeriod,0) = 1) 
				BEGIN
				IF(@DatePart = 'QUARTER') 
				BEGIN
					SET @ColumnPart = '''Q'' + CAST(DATEPART(Q,CAST(#NAME# AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
				END
				ELSE IF(@DatePart = 'YEAR') 
				BEGIN
					SET @ColumnPart = 'CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
				END
				ELSE IF(@DatePart = 'MONTH') 
				BEGIN
					SET @ColumnPart = 'SUBSTRING(DateName(MONTH,CAST(#NAME# AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
				END
				ELSE 
				BEGIN
					SET @ColumnPart = '#NAME#'
				END
			END
			ELSE 
			BEGIN
				SET @ColumnPart = '#NAME#'
			END
			--PRINT @ColumnPart

			--Blank value insertion
			IF(@Count = 1)
			BEGIN
				SET @Columns = @Columns + @DimensionName + ' NVARCHAR(500), OrderValue NVARCHAR(500), '
				SET @SelectTable = @SelectTable + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') + ', ' + 'd' + CAST(@Count AS NVARCHAR) + '.OrderValue' +', '
				SET @InnerJoin = @InnerJoin + ' INNER JOIN DimensionValue d' + CAST(@Count AS NVARCHAR) + ' ON d' + CAST(@Count AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
			END
			ELSE
			BEGIN
				SET @Columns = @Columns + @DimensionName + ' NVARCHAR(100), '
				SET @SelectTable = @SelectTable + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') + ', '
				SET @InnerJoin = @InnerJoin + ' INNER JOIN DimensionValue d' + CAST(@Count AS NVARCHAR) + ' ON d' + CAST(@Count AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
			END
			--Measure value update
			SET @Columns1 = @Columns1 + REPLACE(@ColumnPart,'#NAME#',' d' + CAST((@Count + 1) AS NVARCHAR) + '.DisplayValue') + ' DV' +  CAST(@Count AS NVARCHAR)  + ','
			
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			
			--SET @GroupBy = @GroupBy + 'd' + CAST(@Count + 2 AS NVARCHAR) + '.DisplayValue,'
			SET @GroupBy = @GroupBy + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count + 1 AS NVARCHAR) + '.DisplayValue') + ','

			SET @Where = @Where + ' dv' + CAST(@Count AS NVARCHAR) + ' = ' + @DimensionName + ' AND'

			SET @Count = @Count + 1

	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @AxisName, @DimensionName,@IsPeriod,@DatePart
	END
Close Column_Cursor
Deallocate Column_Cursor
--create temporary table
SET @CreateTable = @CreateTable + '(' + @Columns
--insert 0 values

--measure
SET @GroupBy = SUBSTRING(@GroupBy,0 ,LEN(@GroupBy))
SET @Where = SUBSTRING(@Where,0 ,LEN(@Where)-3)
--Setting Date Filter

DECLARE @TEMPTABLEUPDATE NVARCHAR(MAX),@AggregationType NVARCHAR(20);
DECLARE @MEASURENAME NVARCHAR(100);
DECLARE @MEASUREID INT;
DECLARE @MEASURETABLENAME NVARCHAR(100);
SET @TEMPTABLEUPDATE = ''

DECLARE @temp_Table1 TABLE ([Name] NVARCHAR(100),id INT,MeasureTableName NVARCHAR(100),AggregationType NVARCHAR(50))
IF(@IsGraph = 1)	
BEGIN	
		INSERT @temp_Table1
		SELECT Measure.[Name],Measure.id,MeasureTableName,AggregationType FROM ReportGraph INNER JOIN Measure ON Measure.id= ReportGraph.Measure WHERE ReportGraph.id =  @ReportGraphID
END
ELSE
BEGIN
		INSERT @temp_Table1
		SELECT Measure.[Name],Measure.id,MeasureTableName,AggregationType FROM ReportTable 
		INNER JOIN ReportTableColumn ON ReportTable.id = ReportTableColumn.ReportTable
		INNER JOIN Measure ON Measure.id= ReportTableColumn.Measure WHERE ReportTable.id = @ReportGraphID
END		

DECLARE @MeasureColumn NVARCHAR(100)
SET @MeasureColumn = ''
DECLARE Measure_Cursor CURSOR FOR
SELECT [Name],id,MeasureTableName,AggregationType FROM @temp_Table1
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType
WHILE @@FETCH_STATUS = 0
	Begin
		SET @MeasureColumn = @MeasureColumn + '0,';
		SET @Columns2 =  'SELECT ' + @Columns1 + ' SUM(d1.Value) AS CalculatedValue FROM MeasureValue d1 INNER JOIN DimensionValue d2 ON d1.DimensionValue = ' + 'd2.id AND d2.DimensionID = ' +  REPLACE(@DimensionTableName,'d','')  + ' WHERE d1.Measure=' + CAST (@MEASUREID AS NVARCHAR) +  @GroupBy
		SET @CreateTable = @CreateTable + @MEASURENAME + ' FLOAT , '
		SET @TEMPTABLEUPDATE = @TEMPTABLEUPDATE + N'UPDATE #REPORTTABLE' + cast(@ReportGraphID AS NVARCHAR) + ' SET ['+ @MEASURENAME +'] = CalculatedValue FROM ('+ @Columns2 +') A ' + @Where
	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor
SET @CreateTable = SUBSTRING(@CreateTable, 0 , LEN(@CreateTable))
SET @CreateTable = @CreateTable + ');'

SET @MeasureColumn = SUBSTRING(@MeasureColumn,0,LEN(@MeasureColumn))
IF(@IsGraph = 1)	
	SET @SelectTable = @SelectTable + @MeasureColumn + ' FROM ReportGraph ' + @InnerJoin + ' WHERE ReportGraph.id = ' + CAST(@ReportGraphID AS NVARCHAR)
ELSE
	SET @SelectTable = @SelectTable + @MeasureColumn + ' FROM ReportTable ' + @InnerJoin + ' WHERE ReportTable.id = ' + CAST(@ReportGraphID AS NVARCHAR)
--PRINT @CreateTable  
--PRINT @SelectTable
--print @FilterCondition
--PRINT @Columns1

DECLARE @CLOSINGSQL nvarchar(1000);
DECLARE @TOTALSQL nvarchar(4000);
SET @CLOSINGSQL= N'SELECT * from #REPORTTABLE' + CAST(@ReportGraphID AS NVARCHAR) + ' ORDER BY OrderValue ; drop table #REPORTTABLE' + CAST(@ReportGraphID AS NVARCHAR) + ''
SET @TOTALSQL = @CreateTable + ' ' + @SelectTable + ' ' + @TEMPTABLEUPDATE + ' ' + @CLOSINGSQL

exec (@TOTALSQL)
END



GO
