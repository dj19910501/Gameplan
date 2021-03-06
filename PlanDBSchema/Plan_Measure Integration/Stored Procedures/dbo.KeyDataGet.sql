IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'KeyDataGet') AND xtype IN (N'P'))
    DROP PROCEDURE KeyDataGet
GO


CREATE PROCEDURE [dbo].[KeyDataGet]  
@KeyDataId int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15)=null,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F',
@KEYDATAVALUE FLOAT OUTPUT
AS
BEGIN

DECLARE @Dimensionid INT, @DimensionName NVARCHAR(100), @Count INT, 
		@IsDateDimension BIT,@CreateTable NVARCHAR(MAX),
		@CreateColTable NVARCHAR(MAX), @Columns NVARCHAR(MAX),
		@ColColumns NVARCHAR(MAX), @SelectTable NVARCHAR(MAX),
		@SelectColumns NVARCHAR(MAX), @InnerJoin NVARCHAR(MAX),
		@InnerJoin1 NVARCHAR(MAX), @Columns1 NVARCHAR(MAX),
		@GroupBy NVARCHAR(MAX),@Where NVARCHAR(MAX),
		@DateFilter NVARCHAR(MAX),@ColumnPart NVARCHAR(1000),
		@ExcludeWhere NVARCHAR(MAX),@AxisCount INT, 
		@IsDateDimensionExist BIT,@IsDateDimensionOnly BIT,
		@DATEDIMENSION int,@DimensionValue NVARCHAR(1000)

SET @ExcludeWhere = '';
SET @Columns = '';SET @ColColumns = '';SET @SelectColumns = ''; SET @Columns1 = '';SET @InnerJoin = '';SET @InnerJoin1 = '';
SET @DateFilter = ''

SET @CreateTable = N'DECLARE @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) + '  TABLE ';
SET @CreateColTable = N'DECLARE @COLTABLE' + CAST(@KeyDataId AS NVARCHAR) + '  TABLE ';

SET @SelectTable = N'INSERT INTO @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) + ' SELECT DISTINCT '
SET @Count = 1;
SET @GroupBy = ' GROUP BY ';
SET @Where = ' WHERE '

SET @IsDateDimensionOnly = 0;
SELECT @AxisCount = COUNT(*) FROM KeyDataDimension WHERE KeyDataId = @KeyDataId;
SELECT @IsDateDimensionExist = ISNULL(IsDateDimension,0) FROM Dimension WHERE Id IN (SELECT DimensionId FROM KeyDataDimension WHERE KeyDataId = @KeyDataId)
							   AND IsDateDimension = 1;
If(@AxisCount = 1 AND @IsDateDimensionExist = 1) 
BEGIN
	SET @IsDateDimensionOnly = 1
END

SELECT @DATEDIMENSION = dimension FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D') WHERE cnt = REPLACE(@DATEFIELD,'D','');
DECLARE @ColumnDatePart NVARCHAR(MAX) = '';
	IF(@ViewByValue = 'Q') 
	BEGIN
		SET @ColumnDatePart = '''Q'' + CAST(DATEPART(Q,CAST(#NAME# AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue = 'Y') 
	BEGIN
		SET @ColumnDatePart = 'CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue = 'M') 
	BEGIN
		SET @ColumnDatePart = 'SUBSTRING(DateName(MONTH,CAST(#NAME# AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(#NAME# AS DATE)) AS NVARCHAR)'
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		IF(YEAR(@STARTDATE) = YEAR(@ENDDATE))
		BEGIN
			SET @ColumnDatePart ='CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)))) + '' '' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR),3)'
		END
		ELSE 
		BEGIN
			SET @ColumnDatePart='CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)))) + '' '' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR),3) + ''-''+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(#NAME# AS NVARCHAR)) - 6, CAST(#NAME# AS NVARCHAR)) AS NVARCHAR)))'
		END
	END
	ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
	BEGIN
		SET @ColumnDatePart= '[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(#NAME# AS DATETIME))'
	END
	ELSE 
	BEGIN
		SET @ColumnDatePart = '#NAME#'
	END
DECLARE Column_Cursor CURSOR FOR
select KDD.Dimensionid, '[' + Replace(D.Name,' ','') +']'   DimensionName,D.IsDateDimension,KDD.DimensionValue	from KeyData KD 
INNER JOIN KeyDataDimension KDD ON KD.id = KDD.KeyDataId
INNER JOIN Dimension D ON KDD.Dimensionid = D.id  
WHERE KD.id = @KeyDataId
ORDER BY KD.OrderValue
OPEN Column_Cursor
FETCH NEXT FROM Column_Cursor
INTO @Dimensionid, @DimensionName,@IsDateDimension,@DimensionValue
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF(ISNULL(@IsDateDimension,0) = 1)
			BEGIN
				SET @ColumnPart = @ColumnDatePart
			END
			ELSE 
			BEGIN
				SET @ColumnPart = '#NAME#'
			END
			
			/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
			-- Restrict dimension values as per UserId and RoleId
			DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
			IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
			BEGIN
				SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
				FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
			END
			ELSE 
			BEGIN
				SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
				FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
			END
			IF(CHARINDEX(',',@RestrictedDimensionValues) = 2)
			BEGIN
				SET @RestrictedDimensionValues = SUBSTRING(@RestrictedDimensionValues,4,LEN(@RestrictedDimensionValues))
			END
			/* End - Added by Arpita Soni for ticket #511 on 10/30/2015 */

			--Blank value insertion
			SET @Columns = @Columns + @DimensionName + ' NVARCHAR(1000), '
			SET @ColColumns = @ColColumns + 'DV' + CAST(@Count AS NVARCHAR) + ' NVARCHAR(1000), '
			SET @SelectTable = @SelectTable + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') + ', ' 
			SET @ExcludeWhere = CASE WHEN ISNULL(@ExcludeWhere,'') != '' THEN @ExcludeWhere + ' AND ' ELSE '' END 

			-- filter keydata as per dimension value in KeyDataDimension
			IF(ISNULL(@IsDateDimension,0) = 0)
			BEGIN
				SET @ExcludeWhere = @ExcludeWhere + CASE WHEN ISNULL(@DimensionValue,'') != '' THEN 
									' d' + CAST(@Count AS NVARCHAR) + '.DisplayValue IN ('''+ISNULL(@DimensionValue,'')+''') AND ' 
									ELSE '' END 
			END
			ELSE
			BEGIN
				SET @ExcludeWhere = @ExcludeWhere + CASE WHEN ISNULL(@DimensionValue,'') != '' THEN 
									REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count AS NVARCHAR) + '.DisplayValue') +' IN ('''+ISNULL(@DimensionValue,'')+''') AND ' 
									ELSE '' END 
			END
			SET @ExcludeWhere = @ExcludeWhere + ' d' + CAST(@Count AS NVARCHAR) + '.DisplayValue NOT IN ('''+ISNULL(@RestrictedDimensionValues,'')+''')' 
			SET @InnerJoin = @InnerJoin + ' INNER JOIN DimensionValue d' + CAST(@Count AS NVARCHAR) + ' ON d' + CAST(@Count AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
			
			--Measure value update
			SET @Columns1 = @Columns1 + REPLACE(@ColumnPart,'#NAME#',' d' + CAST((@Count + 2) AS NVARCHAR) + '.DisplayValue') + ' DV' +  CAST(@Count AS NVARCHAR)  + ','
			
			DECLARE @tempIndex INT, @sbString NVARCHAR(100);
			SET @tempIndex = 0;
			SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
			SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
			IF(@DATEFIELD = 'D'  + CAST(@tempIndex AS NVARCHAR))
			BEGIN
				DECLARE @tmpDateId INT
				SELECT @tmpDateId = @DATEDIMENSION
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d1.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@tmpDateId AS NVARCHAR)
			END
			ELSE 
			BEGIN
				SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d1.d'  + CAST(@tempIndex AS NVARCHAR) 
			END
			SET @GroupBy = @GroupBy + REPLACE(@ColumnPart,'#NAME#','d' + CAST(@Count + 2 AS NVARCHAR) + '.DisplayValue') + ','

			SET @Where = @Where + ' dv' + CAST(@Count AS NVARCHAR) + ' = ' + @DimensionName + ' AND'
			SET @Count = @Count + 1
			SET @RestrictedDimensionValues= ''
	FETCH NEXT FROM Column_Cursor
	INTO @Dimensionid, @DimensionName,@IsDateDimension,@DimensionValue
	END
Close Column_Cursor
Deallocate Column_Cursor
--create temporary table
SET @CreateTable = @CreateTable + '(' + @Columns
SET @CreateColTable = @CreateColTable + '(' + @ColColumns
--measure
SET @GroupBy = SUBSTRING(@GroupBy,0 ,LEN(@GroupBy))
SET @Where = SUBSTRING(@Where,0 ,LEN(@Where)-3)
--Setting Date Filter
DECLARE @DateDimensionId INT,@DateDimensionCondition NVARCHAR(100)
SET @DateDimensionId = 0; SET @DateDimensionCondition = ''
IF(@DATEFIELD IS NOT NULL)
BEGIN
	SELECT @DateDimensionId =  @DATEDIMENSION 
	SET @DateDimensionCondition = ' AND d'+ CAST(@Count + 3  AS NVARCHAR) +'.DimensionId = ' + CAST(@DateDimensionId  AS NVARCHAR)
END
SET @DateFilter = CASE WHEN @DATEFIELD IS NULL THEN '' ELSE 
				+ ' INNER JOIN DimensionValue d' +  CAST(@Count + 3  AS NVARCHAR) + ' ON d1.' + @DATEFIELD + ' = d' +  CAST(@Count + 3  AS NVARCHAR) + '.id '
				+ 'and CAST(d'+  CAST(@Count + 3  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '   + @DateDimensionCondition
				END;

--setting where condition
DECLARE @FilterCondition NVARCHAR(4000);
SET @FilterCondition = ''
IF(@FilterValues IS NOT NULL)
BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D1',2);
END
SET @FilterCondition = ISNULL(@FilterCondition,'')
IF(@FilterCondition != '')
SET @FilterCondition = ' WHERE' + REPLACE(@FilterCondition,'#',' AND ')


DECLARE @TEMPTABLEUPDATE NVARCHAR(MAX),@AggregationType NVARCHAR(20), @TEMPTABLEUPDATE1 NVARCHAR(MAX), @TEMPTABLEUPDATE2 NVARCHAR(MAX),@FilterConditionMeasure NVARCHAR(MAX);
DECLARE @MEASURENAME NVARCHAR(100),@SymbolType NVARCHAR(100);
DECLARE @MEASUREID INT;
DECLARE @MEASURETABLENAME NVARCHAR(100);
SET @FilterConditionMeasure = '';
SET @TEMPTABLEUPDATE = ''
SET @TEMPTABLEUPDATE1 = ''
SET @TEMPTABLEUPDATE2 = ''
DECLARE Measure_Cursor CURSOR FOR
select Measure.[Name], Measure.id, MeasureTableName,AggregationType,ISNULL(Suffix,'') SymbolType from KeyData inner join Measure on Measure.id=KeyData.MeasureId where KeyData.id= @KeyDataId
OPEN Measure_Cursor
FETCH NEXT FROM Measure_Cursor
INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
WHILE @@FETCH_STATUS = 0
	Begin
		DECLARE @tmpColumn1 NVARCHAR(MAX)
		SET @tmpColumn1 = ''
		IF(@FilterCondition = '')
			SET @FilterConditionMeasure = ' WHERE d2.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
		ELSE
			SET @FilterConditionMeasure = ' AND d2.Measure = ' + CAST(@MEASUREID AS NVARCHAR)

		DECLARE @AggregartedMeasure NVARCHAR(200)
		IF(LOWER(@AggregationType) = 'avg')
		BEGIN
			IF(@SymbolType != '%')
			BEGIN
				SET @AggregartedMeasure = ' ROUND(sum(D2.Value*d2.rows)/sum(d2.rows),2) AS CalculatedValue,sum(d2.rows) RecordCountSum '
			END
			ELSE
			BEGIN
				SET @AggregartedMeasure = ' ROUND((sum(D2.Value*d2.rows)/sum(d2.rows))*100,2) AS CalculatedValue,sum(d2.rows) RecordCountSum '
			END
		END
		ELSE 
		BEGIN
			SET @AggregartedMeasure = ' SUM(d2.Value) AS CalculatedValue,sum(d2.rows) RecordCountSum '
		END
			
		IF (@IsDateDimensionOnly = 1 )
		BEGIN
			SET @tmpColumn1 =  'SELECT ' + @Columns1 + REPLACE(@AggregartedMeasure,'rows','RecordCount') + ' FROM MeasureValue D2 INNER JOIN DimensionValue D3 ON D2.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','')  + ' WHERE D2.Measure =  ' + CONVERT(NVARCHAR,@MEASUREID) + ' ' +  @GroupBy
		END
		ELSE 
		BEGIN
			SET @tmpColumn1 =  'SELECT ' + @Columns1 + @AggregartedMeasure + ' FROM '+ @DimensionTableName +' d1 INNER JOIN ' + @DimensionTableName  +'Value d2 ON d1.id = ' + 'd2.' +  @DimensionTableName  + @InnerJoin1 +  @DateFilter + @FilterCondition + @FilterConditionMeasure + @GroupBy
		END
		SET @CreateTable = @CreateTable + '[' + @MEASURENAME + '] FLOAT ,RecordCountSum FLOAT, '
		SET @CreateColTable = @CreateColTable + '[' + @MEASURENAME + '] FLOAT ,RecordCountSum FLOAT, '
		
		IF(@SelectColumns = '')
		BEGIN
			SET @SelectColumns =  @SelectColumns + '0,0'
		END
		ELSE
		BEGIN
			SET @SelectColumns =  @SelectColumns + ',0,0'
		END
		
		--Manoj Start changes to table variable
		SET @TEMPTABLEUPDATE1 = @TEMPTABLEUPDATE + N'INSERT INTO @COLTABLE'+ CAST(@KeyDataId AS NVARCHAR) + ' ' +  @tmpColumn1 
		SET @TEMPTABLEUPDATE1 = @TEMPTABLEUPDATE1  + @TEMPTABLEUPDATE + N' UPDATE @KeyDataTable' + cast(@KeyDataId AS NVARCHAR) + ' SET ['+ @MEASURENAME +'] = A.['+ @MEASURENAME + '], RecordCountSum=A.RecordCountSum FROM ( SELECT * FROM @COLTABLE'+ cast(@KeyDataId AS NVARCHAR) +') A ' + @Where
		SET @TEMPTABLEUPDATE2 =  @TEMPTABLEUPDATE2 + @TEMPTABLEUPDATE1
		--Manoj End changes to table variable

	FETCH NEXT FROM Measure_Cursor
	INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME, @AggregationType, @SymbolType
	END
CLOSE Measure_Cursor
DEALLOCATE Measure_Cursor
SET @TEMPTABLEUPDATE  = @TEMPTABLEUPDATE2

SET @SelectTable = @SelectTable + @SelectColumns + '  FROM KeyData ' + @InnerJoin + ' WHERE KeyData.id = ' + CAST(@KeyDataId AS NVARCHAR) +' AND ' +@ExcludeWhere

SET @CreateTable = SUBSTRING(@CreateTable, 0 , LEN(@CreateTable))
SET @CreateTable = @CreateTable + ');'

SET @CreateColTable = SUBSTRING(@CreateColTable, 0 , LEN(@CreateColTable))
SET @CreateColTable = @CreateColTable + ');'

DECLARE @CLOSINGSQL NVARCHAR(1000);
DECLARE @TOTALSQL NVARCHAR(MAX);
 
SET @CLOSINGSQL= N'DECLARE @SUM FLOAT,@WEIGHTEDSUM FLOAT,@TOTALRECORDS FLOAT; ' +
				' SELECT @SUM = SUM(['+@MEASURENAME+']),@TOTALRECORDS = SUM(RecordCountSum),@WEIGHTEDSUM = SUM(['+@MEASURENAME+'] * RecordCountSum) ' +
				' FROM @KeyDataTable' + CAST(@KeyDataId AS NVARCHAR) 

IF(@AggregationType='avg' OR (SELECT COUNT(*) FROM KeyDataDimension WHERE KeyDataId = @KeyDataId AND DimensionValue IS NOT NULL) > 0)
	SET @CLOSINGSQL = @CLOSINGSQL + ';IF(@TOTALRECORDS = 0) SET @TOTALRECORDS = 1; set @FINALVALUE = @WEIGHTEDSUM / @TOTALRECORDS;'
ELSE
	SET @CLOSINGSQL = @CLOSINGSQL + '; SEt @FINALVALUE = @SUM; '

SET @TOTALSQL = @CreateTable + ' ' + @SelectTable + ' ' + ' ' +  @CreateColTable + ' ' + @TEMPTABLEUPDATE + ' ' + @CLOSINGSQL
--PRINT (@TOTALSQL)

EXECUTE SP_EXECUTESQL @TOTALSQL,N'@FINALVALUE FLOAT OUTPUT',@FINALVALUE = @KEYDATAVALUE OUTPUT
--RETURN @KEYDATAVALUE
END

