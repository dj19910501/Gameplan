IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CustomGraphQuery') AND xtype IN (N'P'))
    DROP PROCEDURE CustomGraphQuery
GO

CREATE PROCEDURE [CustomGraphQuery]
@ReportGraphID INT, 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100',
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@DimensionTableName NVARCHAR(100),
@DateDimensionId int,
@IsDrillDownData bit=0,
@DrillRowValue NVARCHAR(1000) = NULL,
@SortBy NVARCHAR(1000) = NULL,
@SortDirection NVARCHAR(1000) = NULL,
@PageSize INT = NULL,
@PageIndex INT = NULL,
@IsExportAll BIT=1,
@Rate PreferredCurrenctDetails READONLY
AS
BEGIN
	DECLARE @DIMENSIONGROUP NVARCHAR(MAX)
	DECLARE @DimensionIndex NVARCHAR(50)='d1.DimensionValue'
	DECLARE @DimensionId Nvarchar(20)=''
	DECLARE @WhereCondition NVARCHAR(2000)=' '
	DECLARE @DimensionValues NVARCHAR(MAX)
	DECLARE @Query NVARCHAR(MAX), @DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX), @CustomFilter NVARCHAR(MAX)

	IF OBJECT_ID('tempdb..#RateListMonthWise') IS NOT NULL
	BEGIN
		DROP TABLE #RateListMonthWise
	END
	CREATE TABLE #RateListMonthWise (StartDate DATE, EndDate DATE, Rate FLOAT)
	IF EXISTS (SELECT * FROM @Rate)
	BEGIN
		INSERT INTO #RateListMonthWise 
		SELECT CAST(StartDate AS DATE), CAST(EndDate AS DATE), Rate FROM @Rate
	END
	
	SET @DIMENSIONGROUP=' '
	SET @DimensionId=CAST(@DateDimensionId as nvarchar)
	
	Select @Query=CustomQuery, @DrillDownCustomQuery=DrillDownCustomQuery, @DrillDownXFilter=DrillDownXFilter,@CustomFilter=CustomFilter from ReportGraph where Id=@ReportGraphID
	IF NOT EXISTS (SELECT * FROM @Rate)
	BEGIN
		SET @QUERY=REPLACE(LOWER(@Query),'rt.Rate','1')
	END
	SET @QUERY=REPLACE(@Query,'#STARTDATE#','''' +CAST(@STARTDATE AS NVARCHAR)+'''')
	SET @QUERY=REPLACE(@Query,'#ENDDATE#','''' +CAST(@ENDDATE AS NVARCHAR)+'''')

	IF(@IsDrillDownData = 0)
	BEGIN	
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') ,DV.OrderValue '
	END
	ELSE
	BEGIN
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS Column1,DV.OrderValue '
	END
	IF(@FilterValues IS NULL)
	BEGIN
	--Insertation Start Kuahsha This date filter will not affact to attribution query
	   IF CHARINDEX('getattributiontouchdata',LOWER(@Query)) <= 0
	   --Insertation End Kuahsha This date filter will not affact to attribution query
		SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	ELSE 
	BEGIN
	--Insertation Start Kuahsha This date filter will not affact to attribution query
	IF CHARINDEX('getattributiontouchdata',@Query) <= 0
	--Insertation end Kuahsha This date filter will not affact to attribution query
	   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	Declare @DimensionValueField NVArchar(500)='DimensionValue'+@dimensionID
	IF(@ViewByValue = 'Q') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(CAst(Value AS int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
			
	ELSE IF(@ViewByValue = 'Y') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + CAST(DATEPART(YY,CAST(CAST(value AS INt) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(Ordervalue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + CAST(DATEPART(YY,CAST(CAST(value AS INT) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue = 'M') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS INT) AS DATETIME)),0,4)
			+ '-' + CAST(DATEPART(YY,CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR) + ']' DisplayValue ,
		MIN(ORDERVALUE) OrderValue  
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
		GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS int) AS DATETIME)),0,4) + '-' + CAST(DATEPART(YY,CAST(CAST(CAST(VALUE AS INT) AS INT) AS DATETIME)) AS NVARCHAR) + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6, 
				CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']' 
				DisplayValue ,MIN(ORDERVALUE) OrderValue 
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
					CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']') A
			ORDER BY OrderValue
		END
		ELSE 
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']'
				DisplayValue ,MIN(Id) OrderValue  
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']') A
			ORDER BY OrderValue
		END
	END
	ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']' DisplayValue ,
			MIN(OrderValue) OrderValue
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']') A
		ORDER BY OrderValue
	END
	/* Modified by Arpita Soni for ticket #604 on 11/19/2015 */
	IF(@FilterValues IS NOT NULL AND (select IsICustomFilterApply from ReportGraph where id=@ReportGraphId)=1 )
	BEGIN
		DECLARE @XmlString XML
		SET @XmlString = @FilterValues;
  		DECLARE @TempValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)
		DECLARE @FilterValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)

 
		;WITH MyData AS (
			SELECT data.col.value('(@ID)[1]', 'int') Number
				,data.col.value('(.)', 'INT') DimensionValueId
			FROM @XmlString.nodes('(/filters/filter)') AS data(col)
		)
		Insert into @TempValueTable 
		SELECT 
			 D.id,
			 DV.Id
		FROM MyData 
		INNER JOIN DimensionValue DV ON DV.Id = MyData.DimensionValueId
		INNER JOIN Dimension D ON D.Id = DV.DimensionId
		--select * from @TempValueTable

		INSERT INTO @FilterValueTable SELECT  ID
			   ,'AND DimensionValue'+Id+' IN('+STUFF((SELECT ',' + CAST(DimensionValueId+'' AS VARCHAR(MAX)) [text()]
				 FROM @TempValueTable 
				 WHERE ID = t.ID
				 FOR XML PATH(''), TYPE)
				.value('.','NVARCHAR(MAX)'),1,1,' ')+')' List_Output
		FROM @TempValueTable t
		GROUP BY ID
		--select * from @FilterValueTable
		--1 step
		DECLARE @DelimitedString NVARCHAR(MAX)
		DECLARE @DimensionValueTemp NVARCHAR(MAX)
		DECLARE @DimensionIdTemp NVARCHAR(MAX)
		SET @DelimitedString =@CustomFilter
		DECLARE @FinalFilterString NVARCHAR(MAX)=' '
		DECLARE @FilterCursor CURSOR
		SET @FilterCursor = CURSOR FAST_FORWARD FOR SELECT id,dimensionvalueid from @FilterValueTable
		OPEN @FilterCursor
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		WHILE @@FETCH_STATUS = 0
		BEGIN
		Declare @Flage INT=0
		Declare @Count INT=0
		Declare @Id NVARCHAR(1000)
		SET @FinalFilterString= @FinalFilterString+ ' '+ @DimensionValueTemp
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		END
		CLOSE @FilterCursor
		DEALLOCATE @FilterCursor	
	END
	IF(@FinalFilterString IS NOT NULL)
	BEGIN
		SET @WhereCondition = @WhereCondition + @FinalFilterString
	END

	/* Modified by Arpita Soni for Ticket #669 on 12/29/2015 */
	DECLARE @fromRecord INT
	DECLARE @toRecord INT 
	
	IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
	BEGIN
		SET @fromRecord = (@PageSize * @PageIndex) + 1;
		SET @toRecord = (@PageSize * @PageIndex) + 10;
	END
	IF(ISNULL(@SortBy,'') = '')
	BEGIN
		SET @SortBy = 'SELECT NULL'
	END
	IF CHARINDEX('#DIMENSIONGROUP#',@Query) <= 0
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			IF NOT EXISTS(SELECT * FROM #RateListMonthWise)
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
			END
			ELSE
			BEGIN
			SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' INNER JOIN #RateListMonthWise rt ON  CAST(DV.DisplayValue AS DATE) between CAST(rt.StartDate AS DATE) and CAST(rt.EndDate AS DATE) '+@WhereCondition)
			END
		END
		ELSE 
		BEGIN
			DECLARE @DrillDownWhere NVARCHAR(MAX)
			IF(@IsExportAll = 0)
			BEGIN
			SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 

			END
			SET @QUERY = REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN
				SET @Query =' SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM (' + @Query +
										' ) B ) A WHERE RowNumber BETWEEN '+CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
									'; SELECT COUNT(*) FROM (' + @Query +') C;'
			END
		END
		print @Query
		EXEC(@Query)
	END
	ELSE
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			Declare @Table nvarchar(Max)='DECLARE @TABLE Table(Name NVARCHAR(1000),Measure Nvarchar(1000),Date Nvarchar(1000),OrderValue1 int)'
			DECLARE @SelectTable NVARCHAR(MAx) =' SELECT * FROM @Table'
			SET @SelectTable='; SELECT * FROM ( ' +
			'SELECT ' + 'Name' + ',' + 'Date' + ',[' + 'Measure' + '],MAX(OrderValue1) OVER (PARTITION BY '+ 'Name' +') OrderValue FROM @Table' + 
			') P PIVOT ('+
			'MAX(['+ 'Measure' +']) FOR ' + 'Date' + ' IN ('+ @DimensionValues +')'+
			') AS PVT ORDER BY OrderValue' 
	
			SET @QUERY=REPLACE(@Query,'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			IF NOT EXISTS(SELECT * FROM #RateListMonthWise)
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
			END
			ELSE
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' INNER JOIN #RateListMonthWise rt ON  CAST(DV.DisplayValue AS DATE) between CAST(rt.StartDate AS DATE) and CAST(rt.EndDate AS DATE) '+@WhereCondition)
			END
			EXEC(@Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable)
		END
		ELSE 
		BEGIN
			SET @DrillDownWhere = ''
			IF(@IsExportAll = 0)
			BEGIN
				SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 
			END
			SET @DIMENSIONGROUP = REPLACE(@DIMENSIONGROUP,',DV.ORDERVALUE','')
			SET @DrillDownCustomQuery=REPLACE((@DrillDownCustomQuery),'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			SET @DrillDownCustomQuery= REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN	
				SET @DrillDownCustomQuery ='SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM ('+ @DrillDownCustomQuery +
										') B ) A WHERE RowNumber BETWEEN '+ CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
										'; SELECT COUNT(*) FROM (' + @DrillDownCustomQuery +') C;'
			END
			
			EXEC(@DrillDownCustomQuery)
		END
	END
END
