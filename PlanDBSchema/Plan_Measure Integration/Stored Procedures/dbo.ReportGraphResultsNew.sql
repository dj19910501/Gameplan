IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ReportGraphResultsNew') AND xtype IN (N'P'))
    DROP PROCEDURE ReportGraphResultsNew
GO


CREATE PROCEDURE [ReportGraphResultsNew]
@ReportGraphID INT, 
@DIMENSIONTABLENAME NVARCHAR(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD NVARCHAR(100)=null, 
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@SubDashboardOtherDimensionTable INT = 0,
@SubDashboardMainDimensionTable INT = 0,
@DisplayStatSignificance NVARCHAR(15) = NULL,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F',
@Rate PreferredCurrenctDetails READONLY

AS
BEGIN
SET NOCOUNT ON;
DECLARE @DateDimensionID int

 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

--For issue #609
if @FilterValues='''<filters></filters>'''
BEGIN
	SET @FilterValues=null
END

Declare @CustomQuery NVARCHAR(MAX)
SET @CustomQuery=(SELECT ISNULL(CustomQuery,'') FROM ReportGraph where Id=@ReportGraphID)
IF(@CustomQuery!='')
BEGIN
EXEC CustomGraphQuery @ReportGraphID,@STARTDATE,@ENDDATE,@FilterValues,@ViewByValue,@DIMENSIONTABLENAME,@DateDimensionID,0,NULL,NULL,NULL,NULL,NULL,1,@Rate
END
ELSE


BEGIN
SET @DisplayStatSignificance=ISNULL(@DisplayStatSignificance,'')
--to do please add report id to all the table variables 
DECLARE @Dimensions NVARCHAR(MAX), @TempString NVARCHAR(MAX), @Count INT, @Measures NVARCHAR(1000), @GroupBy NVARCHAR(1000), @FirstTable NVARCHAR(MAX), @SymbolType NVARCHAR(50) ,@ConfidenceLevel FLOAT
DECLARE @SecondTable NVARCHAR(MAX)  DECLARE @MeasureCreateTableDimension NVARCHAR(MAX) DECLARE @MeasureSelectTableDimension NVARCHAR(1000), @IsDateOnYaixs BIT=0, @DimensionCount INT=0
DECLARE @UpdateTableCondition NVARCHAR(500),  @DimensionName1 NVARCHAR(50), @DimensionName2 NVARCHAR(50), @Measure NVARCHAR(50), @DimensionId1 INT, @DimensionId2 INT, @IsDateDimension BIT=0 ,@IsOnlyDateDimension BIT=0
DECLARE @FinalTable NVARCHAR(MAX), @MeasureCount INT DECLARE @SQL NVARCHAR(MAX), @Columns NVARCHAR(1000) DECLARE @BaseQuery NVARCHAR(MAX), @GType NVARCHAR(100)
DECLARE @RateList NVARCHAR(50) = ''

SET @FirstTable='';  SET @SymbolType=''; SET @Columns='' ; SET @MeasureCreateTableDimension='';SET @MeasureSelectTableDimension='';SET @UpdateTableCondition='';SET @BaseQuery='';SET @SecondTable=''
SET @SQL=''; SET @DimensionName1=''; SET @DimensionName2=''; SET @Measure=''; SET @GroupBy='';SET @FinalTable='';SET @TempString=''
if @FilterValues='<filters></filters>'
BEGIN
SET @FilterValues=null
END	
		
SELECT @GType =LOWER (GraphType),@ConfidenceLevel=ISNULL(ConfidenceLevel,0) FROM ReportGraph WHERE id = @ReportGraphID

 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

SELECT 
		@Dimensions						= selectdimension, 
		@Columns						= CreateTableColumn,
		@MeasureCreateTableDimension	= MeasureTableColumn, 
		@MeasureSelectTableDimension	= MeasureSelectColumn, 
		@UpdateTableCondition			= UpdateCondition,
		@GroupBy						= GroupBy,
		@DimensionCount					= CAST(Totaldimensioncount AS INT), 
		@DimensionName1					= DimensionName1, 
		@DimensionName2					= DimensionName2, 
		@DimensionId1					= CAST(DimensionId1 AS INT), 
		@DimensionId2					= CAST(DimensionId2 AS INT),
		@IsDateDimension				= CAST(IsDateDImensionExist AS BIT), 
		@IsDateOnYaixs					= CAST(IsDateOnYAxis AS BIT), 
		@IsOnlyDateDimension			= CAST(IsOnlyDateDImensionExist AS BIT) 
FROM dbo.GetDimensions(@ReportGraphID,@ViewByValue,@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@GType,@SubDashboardOtherDimensionTable,@SubDashboardMainDimensionTable,@DisplayStatSignificance)

--Measure Table
 SELECT 
		@Measures	= SelectTableColumn, 
		@Columns	= @Columns + CreateTableColumn, 
		@Measure	= MeasureName,
		@SymbolType	= SymbolType 
FROM dbo.GetMeasures(@ReportGraphID,@ViewByValue,@DimensionCount,@GTYPe,@DisplayStatSignificance)


IF EXISTS (SELECT * FROM @Rate)
BEGIN
	IF OBJECT_ID('tempdb..#RateListMonthWise') IS NOT NULL
	BEGIN
		DROP TABLE #RateListMonthWise
	END
	CREATE TABLE #RateListMonthWise (StartDate DATE, EndDate DATE, Rate FLOAT)
	INSERT INTO #RateListMonthWise 
	SELECT CAST(StartDate AS DATE), CAST(EndDate AS DATE), Rate FROM @Rate
	
	IF EXISTS (SELECT * FROM Measure m 
				INNER JOIN ReportGraphColumn rgc ON (rgc.MeasureId = m.id AND rgc.ReportGraphId = @ReportGraphID) 
				WHERE ISNULL(m.IsCurrency, 0) = 1)
	BEGIN
		SET @RateList = '#RateListMonthWise'
	END
END

--1 means creating base query for dimension - only order by cluase will be added
SET @BaseQuery = dbo.DimensionBaseQuery(@DIMENSIONTABLENAME, @STARTDATE, @ENDDATE, @ReportGraphID, @FilterValues,@IsOnlyDateDimension, 1,@UserId,@RoleId,@RateList)
--print @basequery
--
SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#',@Dimensions + @Measures)
--print @Columns
DECLARE @Table NVARCHAR(1000)
SET @Table = ''
SET @Table = 'DECLARE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' TABLE('
SET @Table = @Table+' '+ @Columns + ')'
SET @FirstTable = @Table + 'INSERT INTO @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' ' + @SQL + ''; --insert in to dimension table

SET @BaseQuery= (dbo.DimensionBaseQuery(@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@ReportGraphID,@FilterValues,@IsOnlyDateDimension,0,@UserId,@RoleId,@RateList))
--print @basequery
DECLARE @MeasureId INT, @AggregationType NVARCHAR(50), @MeasureName NVARCHAR(100)
SET @Table=' DECLARE @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' table(' + ' ' + @MeasureCreateTableDimension + ',Rows float, Measure float'+')'

SET @SecondTable = @SecondTable + @Table

--print @MeasureSelectTableDimension + @Measures

DECLARE @MeasureCursor CURSOR
SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT MeasureId,Measurename,AggregationType  FROM dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue)
OPEN @MeasureCursor
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
WHILE @@FETCH_STATUS = 0 
BEGIN
  IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND  LOWER(@DisplayStatSignificance)!='rate')
   SET @MeasureName='Measure_'+@MeasureName

	IF EXISTS(SELECT * FROM Measure WHERE id = @MeasureId AND ISNULL(IsCurrency, 0) = 1)
	BEGIN
		SET @Measures = dbo.GetMeasuresForMeasureTable(@ReportGraphID,@MeasureId,@RateList)
	END
	ELSE
	BEGIN
		SET @Measures = dbo.GetMeasuresForMeasureTable(@ReportGraphID,@MeasureId,'')
	END
	IF(@IsOnlyDateDImension=1 AND @FilterValues IS NULL )
	SET @Measures=REPLACE(@Measures,'rows','RecordCount')
	SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#', @MeasureSelectTableDimension + @Measures )
	--SET SQL

	--In base query if there are row excluded or filter is passed as parameter then there is already where cluase so we have to check condition
	IF CHARINDEX('where',LOWER(@SQL)) <= 0
		SET @SQL=@SQL+' WHERE '
	ELSE
		SET @SQL=@SQL+' AND '
	
	SET @SQL = @SQL + ' D1.Measure=' + CAST(@MeasureId AS NVARCHAR) + ' GROUP BY ' + @GroupBy
			
	SET @SecondTable=@SecondTable+' INSERT INTO @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' '+@SQL
	
		IF(LOWER(@GType)='errorbar' OR LOWER(@DisplayStatSignificance)='rate')	
		BEGIN
			DECLARE @ErrorbarQuery NVARCHAR(2000)='', @UpdateErrorbarQuery NVARCHAR(2000)=' ', @PopulationRate FLOAT, @NoOfTails FLOAT=2
			
			IF( @Confidencelevel < 0 OR @Confidencelevel > 1)
				SET @ConfidenceLevel=0

		IF(@DimensionCount = 1)
			BEGIN
			IF(LOWER(@AggregationType) = 'avg')
			BEGIN
			SET @PopulationRate=(SELECT ROUND((SUM(value*RecordCount)/SUM(RecordCount)),2) FROM MeasureValue WHERE DimensionValue in (SELECT id FROM DimensionValue where DimensionID in(select DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
				
			END
			ELSE
			BEGIN
				SET @PopulationRate=(SELECT ROUND(AVG(value),2) FROM MeasureValue WHERE DimensionValue IN (SELECT id FROM DimensionValue where DimensionID IN(SELECT DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
			END
		
			IF(@PopulationRate>1 OR @PopulationRate<0)
				SET @PopulationRate = 1;
				SET @ErrorbarQuery =' Update @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+CAST(@PopulationRate AS NVARCHAR)
				SET @SecondTable = @SecondTable + @ErrorbarQuery + ';'
			END

		ELSE -- @DimensionCount = 2
			BEGIN
				DECLARE @ExistDimensionTableName NVARCHAR(100), @Pr_DimensionId INT, @SeriesDimension INT, @SeriesDimensionIndex INT, @YaxisIndex INT
				DECLARE @SeriesDimensionName NVARCHAR(500), @DateAdded bit =0, @IsDateDimensionOnYAxis INT, @Pr_IsDateDimensionExist INT
			
				SELECT @Pr_IsDateDimensionExist = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID) and IsDateDimension=1;
				SET @SeriesDimension=(select dimensionid FROM ReportAxis WHERE  ReportGraphId = @ReportGraphID and axisname='Y')
				
				SET @SeriesDimensionName=(select columnname FROM Dimension where id=@SeriesDimension)
			
				SET @YaxisIndex=2
				SELECT @IsDateDimensionOnYAxis = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID and axisname='Y') and IsDateDimension=1;
			
				DECLARE @DimensionCursor CURSOR
				SET @DimensionCursor = CURSOR FAST_FORWARD FOR SELECT dimensionid FROM   reportaxis where reportgraphid=@ReportGraphID order by dimensionid 
				OPEN @DimensionCursor
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				WHILE @@FETCH_STATUS = 0
					BEGIN

						IF(@Pr_IsDateDimensionExist=0 and @DateDimensionId < @Pr_DimensionId and @DateAdded=0)
						BEGIN
							SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@DateDimensionId as NVARCHAR)) 
							SET @DateAdded=1
						END

					SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@Pr_DimensionId as NVARCHAR)) 
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				END
				CLOSE @DimensionCursor
				DEALLOCATE @DimensionCursor	
					SET @SeriesDimensionIndex=1
				IF(@DateAdded=1)
				SET @SeriesDimensionIndex= (select cnt FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'d') where Dimension=CAST(@SeriesDimension as NVARCHAR))
				ELSE
				IF((select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='Y')>(select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='X'))
				SET @SeriesDimensionIndex=2
				DECLARE @DisplayValue NVARCHAR(500), @Dimension NVARCHAR(50), @Populationratestring NVARCHAR(1000)
			
				IF(@YaxisIndex=1)
					SET @Dimension = @DimensionName1
				ELSE
					SET @Dimension = @DimensionName2
					SET @DisplayValue='DisplayValue'
					IF(@IsDateDimensionOnYAxis>0)
						SET @DisplayValue=' dimensionid='+CAST(@DateDimensionId as NVARCHAR)+' and CAST(dbo.GetDatePart(CAST(DisplayValue AS DATE),'''+@ViewByValue+''','''+CAST(@STARTDATE as NVARCHAR)+''','''+CAST(@Enddate as NVARCHAR)+''') as NVARCHAR) '
							IF(LOWER(@AggregationType)='avg')
							BEGIN
						
							SET @PopulationRateString='((select (SUM(Value*Rows)/SUM(rows)) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
						
							END
							ELSE
							BEGIN
							SET @PopulationRateString='((select AVG(value) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
								
							
							END
							SET @ErrorbarQuery=' UPDATE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+@PopulationRateString
							SET @SecondTable=@SEcondTable+@ErrorbarQuery+';'
		END -- Error graph @DimensionCount End
		IF(LOWER(@DisplayStatSignificance)='rate')
		BEGIN
		IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
		ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END
		END
		ELSE IF(LOWER(@GType)='errorbar')
		BEGIN
	    IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
			ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END

		END -- Error graph complete
		END

		 IF(@gtype!='errorbar' AND LOWER(@DisplayStatSignificance)!='rate')
				SET @UpdateErrorBarQuery=' '
				SET @SecondTable=@SecondTable+' UPDATE @dimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET Rows=M.Rows,['+@MeasureName+']=M.[Measure]'+@UpdateErrorBarQuery+' FROM @dimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' D INNER JOIN @MeasureTable'+CAST(@ReportGraphID AS NVARCHAR)+' M ON '+@UpdateTableCondition+' delete FROM @Measuretable'+CAST(@ReportGraphID as NVARCHAR)
			 
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
END
CLOSE @MeasureCursor
DEALLOCATE @MeasureCursor
DECLARE @SelectTable NVARCHAR(MAX)


IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND LOWER(@DisplayStatSignificance)!='rate')
BEGIN

	DECLARE @DimensionValues NVARCHAR(MAX)
	IF(@IsDateOnYaixs=0)
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + '[' + DisplayValue + ']'  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) ORDER BY OrderValue
	ELSE
	BEGIN
			IF(@ViewByValue = 'Q') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'Y') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'M') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue='W')
			BEGIN
				IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']') A
					ORDER BY OrderValue
				END
				ELSE 
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']') A
					ORDER BY OrderValue
				END
			END
			ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']') A
				ORDER BY OrderValue
			END
	END
	
	--SET @SelectTable='; SELECT '+ @DimensionName1 + ',' + @DimensionValues + ' FROM ( ' +
	SET @SelectTable='; SELECT * FROM ( ' +
	'SELECT ' + @DimensionName1 + ',' + @DimensionName2 + ',[' + @Measure + '],MAX(OrderValue1) OVER (PARTITION BY '+ @DimensionName1 +') OrderValue FROM @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' ' + 
	') P PIVOT ('+
	'MAX(['+ @Measure +']) FOR ' + @DimensionName2 + ' IN ('+ @DimensionValues +')'+
	') AS PVT ORDER BY OrderValue' 
	END
	ELSE
	BEGIN
		DECLARE @OrderBy NVARCHAR(500) 
		IF(@DimensionCount>1)
			SET @OrderBy=' ORDER BY OrderValue1,OrderValue2'
		ELSE
			SET @OrderBy=' ORDER BY OrderValue1  '
		SET @SelectTable=';SELECT * FROM @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' '+@OrderBy
	END
	
	
	print 	@FirstTable
	print @SecondTable
	print @finaltable
	print @SelectTable
	EXEC(@FirstTable+@SecondTable+@finaltable+@SelectTable )
END

END