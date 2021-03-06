IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestQueryForAll') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetTTestQueryForAll
GO
CREATE FUNCTION [GetTTestQueryForAll]
(
	@Dimension1 NVARCHAR(500),
	@Dimension2 NVARCHAR(500),
	@ViewByValue VARCHAR(10),
	@STARTDATE DATE='1-1-1900', 
	@ENDDATE DATE='1-1-2100', 
	@ReportGraphID INT, 
	@DIMENSIONTABLENAME NVARCHAR(100), 
	@DATEFIELD NVARCHAR(100)=NULL, 
	@FilterValues NVARCHAR(MAX)=NULL,
	@AxisCount INT
)
RETURNS NVARCHAR(4000)
AS
BEGIN
	
	DECLARE @Dimensionid int, @IsColumn NVARCHAR(20),@DimensionName NVARCHAR(100), @Count int, @IsDateDimension BIT, @InnerJoin1 NVARCHAR(MAX);
	DECLARE @TTestQuery1 NVARCHAR(MAX), @TTestQuery2 NVARCHAR(MAX);
	
	SET @InnerJoin1 = ''; SET @Count= 1; SET @TTestQuery2 = '';

	DECLARE @HeatMapDimColumn NVARCHAR(1000), @HeatMapGrpuoColumn NVARCHAR(1000), @HeatMapWhere1 NVARCHAR(1000),@HeatMapWhere2 NVARCHAR(1000),@OnlyDateDimQuery NVARCHAR(MAX)
	SET @HeatMapDimColumn = ''; SET @HeatMapGrpuoColumn = ''; SET @HeatMapWhere1 = ''; SET @HeatMapWhere2 = '';
	SET @OnlyDateDimQuery = 'SELECT #COLUMNS# FROM MeasureValue d1 INNER JOIN DimensionValue d2 ON d1.DimensionValue=d2.id AND ';

	SET @TTestQuery1 = 'SELECT #COLUMNS# FROM ' + @DIMENSIONTABLENAME + 'Value d1';
	SET @TTestQuery1 =  @TTestQuery1 + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' d2 ON d2.id = d1.' + @DIMENSIONTABLENAME 

	DECLARE @DimensionNames TABLE(DimensionID INT,CountIndex INT);
	INSERT INTO @DimensionNames
	SELECT dimension, cnt FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D')

	DECLARE @IsOnlyDateDimension INT,@Select2ColumnIndex INT,@Select1ColumnIndex INT,@FinalTTestQuery NVARCHAR(MAX),@DimensionValue1 NVARCHAR(100),@DimensionValue2 NVARCHAR(100);
	SET @IsOnlyDateDimension=0;
	SET @DimensionValue1 = ''; 
	SET @DimensionValue2 = ''; 
	
	--identify about dimensions
	IF(@AxisCount = 2)
	BEGIN
		SET @IsOnlyDateDimension = 0;
	END
	ELSE 
	BEGIN
		DECLARE @datecount INT
		SELECT @datecount = COUNT(*) FROM ReportAxis INNER JOIN Dimension ON Dimension.id = ReportAxis.Dimensionid and Dimension.IsDateDimension = 1 where ReportGraphId = @ReportGraphID 
		IF(ISNULL(@datecount,0) = 1)
		BEGIN
			SET @IsOnlyDateDimension = 1;
		END
		ELSE
		BEGIN	
			SET @IsOnlyDateDimension = 0;
		END
	END

	DECLARE Column_Cursor CURSOR FOR
	SELECT DN.DimensionID AS Dimensionid, '[' + Replace(D.Name,' ','') + ']' DimensionName, D.IsDateDimension FROM @DimensionNames DN
	INNER JOIN Dimension D ON DN.DimensionID = D.id  
	OPEN Column_Cursor 
	FETCH NEXT FROM Column_Cursor 
	INTO @Dimensionid, @DimensionName,@IsDateDimension 
		WHILE @@FETCH_STATUS = 0
			BEGIN
				
				DECLARE @tempIndex INT, @sbString NVARCHAR(100), @CurrentDimension NVARCHAR(100);
				SET @tempIndex = 0;
				SET @sbString = SUBSTRING(@DimensionTableName,0,CHARINDEX('d' + CAST(@Dimensionid AS NVARCHAR),@DimensionTableName))
				SET @tempIndex = (LEN(@sbString) - LEN(REPLACE(@sbString, 'd', ''))) + 1
				IF(@Dimensionid = SUBSTRING(@Dimension1,2,CHARINDEX(':',@Dimension1)-2))
						SET @CurrentDimension = @Dimension1
				ELSE IF(@Dimensionid = SUBSTRING(@Dimension2,2,CHARINDEX(':',@Dimension2)-2))
						SET @CurrentDimension = @Dimension2
				ELSE 
						SET @CurrentDimension = NULL
				
				IF(@IsOnlyDateDimension=1)
				BEGIN
					
					DECLARE @Dates1 NVARCHAR(100),@Dates2 NVARCHAR(100);
					SET @Dates1 = [dbo].[CalculateStartAndEndDate](REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':',''),@ViewByValue,@STARTDATE,@ENDDATE);
					SET @Dates2 = [dbo].[CalculateStartAndEndDate](REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':',''),@ViewByValue,@STARTDATE,@ENDDATE);
					
					SET @STARTDATE = SUBSTRING(@Dates1,1,CHARINDEX(',',@Dates1)-1)
					SET @ENDDATE = SUBSTRING(@Dates1,CHARINDEX(',',@Dates1)+1,LEN(@Dates1)-CHARINDEX(',',@Dates1))
					SET @InnerJoin1 = ISNULL(@OnlyDateDimQuery,'')  + ' d2.Dimensionid = '+ CAST(@Dimensionid AS NVARCHAR) + ' AND CAST(d2.DisplayValue AS DATE) BETWEEN ''' +CAST(@STARTDATE AS NVARCHAR) +''' AND '''+CAST(@ENDDATE AS NVARCHAR) +''''
					
					SET @STARTDATE = SUBSTRING(@Dates2,1,CHARINDEX(',',@Dates2)-1)
					SET @ENDDATE = SUBSTRING(@Dates2,CHARINDEX(',',@Dates2)+1,LEN(@Dates2)-CHARINDEX(',',@Dates2))
					SET @TTestQuery2 = ISNULL(@OnlyDateDimQuery,'')  + ' d2.Dimensionid = '+ CAST(@Dimensionid AS NVARCHAR) + ' AND CAST(d2.DisplayValue AS DATE) BETWEEN ''' +CAST(@STARTDATE AS NVARCHAR) +''' AND '''+CAST(@ENDDATE AS NVARCHAR) +''''

					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 
											+' d1.value '
				END
				ELSE IF(@IsDateDimension=1)
				BEGIN
					SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d'  + CAST(@tempIndex AS NVARCHAR) + ' AND d' + CAST(@Count + 2 AS NVARCHAR) + '.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR)
					SET @InnerJoin1 = @InnerJoin1 + ' and CAST(d'+  CAST(@Count + 2  AS NVARCHAR) + '.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' +  cast(@ENDDATE as nvarchar) + ''' '  
					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + 
											+' d1.value '
				END
				ELSE 
				BEGIN
					IF(CHARINDEX('D'+CAST(@Dimensionid AS NVARCHAR)+':',@Dimension1)=1)
					BEGIN
						SET @Select1ColumnIndex = @Count;
						SET @DimensionValue1 = REPLACE(@Dimension1,'D' +CAST(@Dimensionid AS NVARCHAR) +':','');
						SET @InnerJoin1 = @InnerJoin1 + ' INNER JOIN DimensionValue d' + CAST(@Count + 2 AS NVARCHAR) + ' ON d' + 
											  CAST(@Count + 2 AS NVARCHAR) + '.id = d2.d#FIRSTINDEX# AND d'+CAST(@Count + 2 AS NVARCHAR) 
						SET @InnerJoin1 = @InnerJoin1 + '.DisplayValue = ''#NAME1#'''
					END
					IF(CHARINDEX('D'+CAST(@Dimensionid AS NVARCHAR)+':',@Dimension2)=1)
					BEGIN
						SET @DimensionValue2 = REPLACE(@Dimension2,'D' +CAST(@Dimensionid AS NVARCHAR) +':','');
						SET @Select2ColumnIndex = @Count;
					END
					IF(@tempIndex = (SELECT MAX(CountIndex) FROM @DimensionNames))
						SET @HeatMapDimColumn = @HeatMapDimColumn  + CASE WHEN @HeatMapDimColumn <> '' THEN ', ' ELSE '' END + ' d1.value '
				END
				SET @Count = @Count + 1
		FETCH NEXT FROM Column_Cursor
		INTO @Dimensionid, @DimensionName,@IsDateDimension
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
		BEGIN
		
			DECLARE @AggregartedMeasure NVARCHAR(200)

			IF(@MeCount = 1)
			BEGIN
					SET @AggregartedMeasure = 'a.value'
					SET @HeatMapDimColumn = @HeatMapDimColumn + ' ,' + @AggregartedMeasure --+ ','
					SET @HeatMapWhere1 = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
			END
			ELSE
			BEGIN
					SET @HeatMapWhere2 = ' WHERE d1.Measure = ' + CAST(@MEASUREID AS NVARCHAR)
			END
		
			SET @Count = @Count + 1
			SET @MeCount = @MeCount + 1
		FETCH NEXT FROM Measure_Cursor
		INTO @MEASURENAME, @MEASUREID, @MEASURETABLENAME,@AggregationType,@SymbolType
		END
	CLOSE Measure_Cursor
	DEALLOCATE Measure_Cursor

	IF(@IsOnlyDateDimension = 1)
	BEGIN
		SET @InnerJoin1 = REPLACE(@InnerJoin1,'#COLUMNS#',@HeatMapDimColumn)
		SET @TTestQuery1 = ISNULL(@InnerJoin1,'')  --+ ' Group By ' + @HeatMapGrpuoColumn
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d2.id, d1.value ')
		SET @FinalTTestQuery = ISNULL(@TTestQuery1,'') +' FULL OUTER JOIN ( '+ISNULL(@TTestQuery2,'') +' '+ISNULL(@HeatMapWhere1,'')+' ) a ON a.id=d2.id' + ISNULL(@HeatMapWhere1,'')
	END
	ELSE
	BEGIN
		SET @TTestQuery2 = ' FULL OUTER JOIN ( ';
		
		IF(@MeCount=2)
		BEGIN
			SET @TTestQuery2 = ISNULL(@TTestQuery2,'') + ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'') + ISNULL(@HeatMapWhere1,'') 
			SET @TTestQuery2 =  ISNULL(@TTestQuery2,'') + ' ) a ON a.d1=d2.d1 ' 
			SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d2.d1, d1.value ')
		END
		ELSE
		BEGIN
			--For correlation heat maps
			SET @TTestQuery2 = ISNULL(@TTestQuery2,'') + ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'') + ISNULL(@HeatMapWhere2,'') 
			SET @TTestQuery2 = REPLACE(@TTestQuery2,'#COLUMNS#',' d1.value, d1.'+@DIMENSIONTABLENAME)
			SET @TTestQuery2 =  ISNULL(@TTestQuery2,'') + ' ) a ON a.'+@DIMENSIONTABLENAME+'=d1.'+@DIMENSIONTABLENAME+ ' ' 
		END
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#NAME1#','#NAME2#')
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#FIRSTINDEX#','#SECONDINDEX#')
		SET @TTestQuery1 = ISNULL(@TTestQuery1,'') + ISNULL(@InnerJoin1,'');
		
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#COLUMNS#',@HeatMapDimColumn) 
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#NAME1#',@DimensionValue1)
		SET @TTestQuery1 = REPLACE(@TTestQuery1,'#FIRSTINDEX#',@Select1ColumnIndex)
		
		
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#NAME2#',@DimensionValue2)
		SET @TTestQuery2 = REPLACE(@TTestQuery2,'#SECONDINDEX#',@Select2ColumnIndex)

		SET @FinalTTestQuery = ISNULL(@TTestQuery1,'') +  ISNULL(@TTestQuery2,'') + ISNULL(@HeatMapWhere1,'') 
		
	END

	--Added by kausha somaiya for filter condition and For Exclude column
	DECLARE @FilterCondition NVARCHAR(MAX);
	SET @FilterCondition = ''
	IF(@FilterValues IS NOT NULL)
	BEGIN
		SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
		set @FinalTTestQuery=@FinalTTestQuery+' AND '+ REPLACE(@FilterCondition,'#',' AND ')
	END
	RETURN @FinalTTestQuery;
END

GO
