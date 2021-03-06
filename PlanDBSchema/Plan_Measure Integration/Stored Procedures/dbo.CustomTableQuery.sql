IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CustomTableQuery') AND xtype IN (N'P'))
    DROP PROCEDURE CustomTableQuery
GO

CREATE PROCEDURE [dbo].[CustomTableQuery]  
@ReportTableID INT, 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100',
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@DimensionTableName NVARCHAR(100)
AS
	
	BEGIN

	DECLARE @DIMENSIONGROUP NVARCHAR(MAX)
	DECLARE @DimensionIndex NVARCHAR(50)='d1.DimensionValue'
	DECLARE @DimensionId Nvarchar(20)=''
	DECLARE @WhereCondition NVARCHAR(MAX)=' '
	DECLARE @DimensionValues NVARCHAR(MAX)
	DECLARE @Query NVARCHAR(MAX)
	DECLARE @CustomFilter NVARCHAR(MAX)
	SET @DIMENSIONGROUP=' '
	--SET @DimensionId=(SELECT TOP 1 DimensionId FROM ReportTableDimension INNER JOIN Dimension D On ReportTableDimension.Dimensionid=d.id WHERE ReportTableId=@ReportTableID AND D.IsDateDimension=1  )
	--SET @DimensionId=62
	SET @DimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)
	Select @Query=CustomQuery,@customFilter=CustomFilter from ReportTable where Id=@ReportTableID;
	SET @CustomFilter=(Select CustomFilter from ReportTable where Id=@ReportTableID);
	SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+'''),DV.OrderValue '

	IF(@FilterValues IS NULL)
	BEGIN

		   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
		 
	END
	ELSE 
	BEGIN
	
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

--IF CHARINDEX('#dimensiongroup#',LOWER(@Query)) <= 0
IF(@FilterValues IS NOT NULL AND (select IsICustomFilterApply from ReportTable where id=@ReportTableID)=1 )
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
SET @WhereCondition=@WhereCondition+@FinalFilterString

IF CHARINDEX('#dimensiongroup#',LOWER(@Query)) <= 0
BEGIN

SET @QUERY=REPLACE(@Query,'#dimensionwhere#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
--IF(@CustomFilter IS NOT NULL AND LEN(@CustomFilter)>0 AND @FilterValues IS NOT NULL )
--	SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),@FinalFilterString)
--	ELSE
--	SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),'')

exec(@Query)
END
ELSE
BEGIN
	Declare @Table nvarchar(Max)='DECLARE @TABLE Table(Name NVARCHAR(1000),Measure Nvarchar(1000),Date Nvarchar(1000),OrderValue1 int)'
	DECLARE @SelectTable NVARCHAR(MAx) =' Select * from @Table'
	SET @SelectTable='; SELECT * FROM ( ' +
	'SELECT ' + 'Name' + ',' + 'Date' + ',[' + 'Measure' + '],MAX(OrderValue1) OVER (PARTITION BY '+ 'Name' +') OrderValue FROM @Table' + 
	') P PIVOT ('+
	'MAX(['+ 'Measure' +']) FOR ' + 'Date' + ' IN ('+ @DimensionValues +')'+
	') AS PVT ORDER BY OrderValue' 
	SET @QUERY=REPLACE(LOWER(@Query),'#dimensiongroup#',''+@DIMENSIONGROUP)
	
	SET @QUERY=REPLACE(LOWER(@Query),'#dimensionwhere#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
	--IF(@CustomFilter IS NOT NULL AND LEN(@CustomFilter)>0 AND @FilterValues IS NOT NULL)
	--	SET @QUERY=REPLACE(@Query,LOWER('#FILTERS#'),@FinalFilterString)
	--ELSE
	--SET @QUERY=REPLACE(LOWER(@Query),LOWER('#FILTERS#'),'')
	
	--print @Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable
	exec(@Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable)
	
	END
	
END
