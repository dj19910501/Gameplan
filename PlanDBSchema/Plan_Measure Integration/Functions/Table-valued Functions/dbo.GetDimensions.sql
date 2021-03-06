IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDimensions') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetDimensions
GO

CREATE FUNCTION [dbo].[GetDimensions] 
(   @ReportGraphID INT, 
    @ViewByValue NVARCHAR(10),
    @DIMENSIONTABLENAME NVARCHAR(100)='d46D47', 
	@STARTDATE DATE='01/01/2014', 
    @ENDDATE DATE='12/31/2014',
	@GType nvarchar(20),
	@SubDashboardOtherDimensionTable int,
	@SubDashboardMainDimensionTable int,
	@DisplayStatSignificance NVARCHAR(15) = NULL -- Added by Arpita Soni on 08/19/2015 for PL ticket #427
) 
RETURNS @DimensionTable TABLE(SelectDimension NVARCHAR(2000),
CreateTableColumn nvarchar(2000),--2
MeasureTableColumn nvarchar(2000),--3
MeasureSelectColumn nvarchar(2000),--4
UpdateCondition nvarchar(2000),--5
GroupBy nvarchar(2000),--6
Totaldimensioncount nvarchar(2000),--7
DimensionName1 nvarchar(50),--8
DimensionName2 nvarchar(50),--9
DimensionId1 nvarchar(10),--10
DimensionId2 nvarchar(10),--11
IsDateDImensionExist nvarchar(10),--12
IsDateOnYAxis nvarchar(10),--13
IsOnlyDateDImensionExist nvarchar(10),--14


cnt int) 
BEGIN
	--Start Get the column names as per configured in Report Axis
	DECLARE @DimensionName NVARCHAR(100),@Column NVARCHAR(MAX),@CreateTableColumn NVARCHAR(MAX),@MeasureTableColumn NVARCHAR(MAX),@MeasureSelectColumn NVARCHAR(MAX),@UpdateCondition NVARCHAR(MAX),@GroupBy NVARCHAR(500),@IsDateDimension BIT,@DimensionIndex NVARCHAR(10),@DimensionId INT,@Count INT,@IsDateDImensionExist bit=0,@IsDateOnYAxis bit=0,@DimensionName1 Nvarchar(50),@DimensionName2 Nvarchar(50),@DimensionId1 INT,@DimensionId2 INT,@DateDImensionId INT,@IsOnlyDateDImensionExist bit=0
	SET @Column='';SET @CreateTableColumn=''; set @MeasureTableColumn='';set @MeasureSelectColumn=''; SET @UpdateCondition=''; SET @GroupBy='';SET @Count = 1
	 
	DECLARE @DimensionCursor CURSOR
	SET @DimensionCursor = CURSOR FAST_FORWARD FOR SELECT D.id, '[' + Replace(D.Name,' ','') + ']',D.IsDateDimension ,'D' + CAST(D1.cnt + 2 AS NVARCHAR) 
												    FROM ReportAxis rx 
													INNER JOIN Dimension d ON rx.Dimensionid=d.id
													LEFT JOIN dbo.fnSplitString(@DIMENSIONTABLENAME,'D') D1 ON D1.dimension = D.id --to get the index of dimension in combination table
													WHERE rx.ReportGraphId = @ReportGraphID
													ORDER BY rx.AxisName 
	OPEN @DimensionCursor
	FETCH NEXT FROM @DimensionCursor
	INTO @DimensionId,@DimensionName,@IsDateDimension,@DimensionIndex
	WHILE @@FETCH_STATUS = 0 
	BEGIN
		-- =============================================================================================
			-- Added by Sohel Pathan on 20/02/2015 for PL ticket #129
			IF ((@SubDashboardOtherDimensionTable > 0) AND (@SubDashboardMainDimensionTable = @Dimensionid))
			BEGIN
				SET @Dimensionid = @SubDashboardOtherDimensionTable
			END
		-- =============================================================================================
		IF(LEN(@Column) > 0)
		BEGIN
			SET @Column = @Column + ',' --to add the comma after each column, here when there is second dimension
			SET @CreateTableColumn = @CreateTableColumn + ',' --to add the comma after each column, here when there is second dimension
			SET @MeasureTableColumn = @MeasureTableColumn + ',' --to add the comma after each column, here when there is second dimension
			SET @MeasureSelectColumn=@MeasureSelectColumn+','
			SET @GroupBy=@GroupBy+','
			SET @UpdateCondition=@UpdateCondition+' AND '

			END
			DEclare @Index NVARCHAR(100)=' INDEX '+@DimensionName+' CLUSTERED'
			if(@Count>1)
			SET @Index=''
		SET @CreateTableColumn = @CreateTableColumn + @DimensionName + ' NVARCHAR(1000)'+@Index+',OrderValue' + CAST(@Count AS NVARCHAR) + ' NVARCHAR(1000) '
		SET @MeasureTableColumn = @MeasureTableColumn + @DimensionName + ' NVARCHAR(1000) '+@Index
		SET @UpdateCondition=@UpdateCondition+'D.'+@DimensionName+'=M.'+@DimensionName

			
		IF(@IsDateDimension=0)
			BEGIN
				SET @Column = @Column + @DimensionIndex+'.DisplayValue as '+ @DimensionName + ','+ @DimensionIndex +'.OrderValue AS OrderValue' + CAST(@Count AS NVARCHAR) + ' '
				SET @MeasureSelectColumn=@MeasureSelectColumn+@DimensionIndex+'.DisplayValue as '+@DimensionName+' '
				SET @GroupBy=@GroupBy+@DimensionIndex+'.DisplayValue'
				
			END
		ELSE
			BEGIN
			SET @IsDateDImensionExist=1
			SET @DateDImensionId=@DimensionId
			if(@Count=2)
			SET @IsDateOnYAxis=1;
				--SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			--	SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			if(LOWER(@GType)='errorbar')
			BEGIN
			SET @Column = @Column + 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',' +CAST(@STARTDATE AS NVARCHAR)+','+CAST(@ENDDATE AS NVARCHAR)+') AS '+@DimensionName+',min('+@DimensionIndex+'.ordervalue)  over (partition by dbo.GetDatePart('+@DimensionIndex+'.DisplayValue,'''+@ViewByValue+''','+cast(@STARTDATE as nvarchar)+','+cast(@ENDDATE as nvarchar)+')) as OrderValue'+CAST(@Count as nvarchar)+' '
			/*  Insertion start:04/01/2016 Kausha #782   */
			SET @MeasureSelectColumn=@MeasureSelectColumn+ 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS '+@DimensionName+' '
			SET @GroupBy=@GroupBy+'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''')' 
			/*  Insertion end:04/01/2016 Kausha #782   */
			END
			ELSE
			BEGIN

				IF(@ViewByValue = 'Q') 
				BEGIN
					SET @Column = @Column +'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS ' +@DimensionName
					SET @GroupBy=@GroupBy++'''Q'' + CAST(DATEPART(Q,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) '
				END
				ELSE IF(@ViewByValue = 'Y') 
				BEGIN
					SET @Column = @Column+ 'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR)  '
				END
				ELSE IF(@ViewByValue = 'M') 
				BEGIN
					SET @Column = @Column+ 'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'SUBSTRING(DateName(MONTH,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)),0,4) + ''-'' + CAST(DATEPART(YY,CAST(' + @DimensionIndex +'.DisplayValue AS DATE)) AS NVARCHAR) '
				END
				/* Start - Added by Arpita Soni for ticket #191 on 03/19/2015 */
				ELSE IF(@ViewByValue='W')
				BEGIN
					IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
					BEGIN
					SET @Column = @Column+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' '' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) '
					END
					ELSE 
					BEGIN
						SET @Column = @Column+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
						SET @MeasureSelectColumn=@MeasureSelectColumn+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))) AS '+@DimensionName
						SET @GroupBy=@GroupBy+'LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + '' ''+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)))) + ''-'' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(' + @DimensionIndex +'.DisplayValue  AS NVARCHAR)) - 6, CAST(' + @DimensionIndex +'.DisplayValue AS NVARCHAR)) AS NVARCHAR))) '
					END
				END
				/* End - Added by Arpita Soni for ticket #191 on 03/19/2015 */
				/* Start - Added by Arpita Soni for ticket #244 on 05/04/2015 */
				ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
				BEGIN
					SET @Column = @Column+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)),' + @DimensionIndex +'.OrderValue AS OrderValue'+CAST(@Count as nvarchar)
					SET @MeasureSelectColumn=@MeasureSelectColumn+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)) AS '+@DimensionName
					SET @GroupBy=@GroupBy+'[dbo].[CalculateFiscalQuarterYear]('''+@ViewByValue+''',CAST(' + @DimensionIndex +'.DisplayValue AS DATETIME)) '
				END
				END

				if(@GType='columnrange')
				SET @Column = @Column+','+'cast(' + CAST(@DimensionIndex AS NVARCHAR) + '.displayvalue as datetime)'

				--SET @MeasureSelectColumn=@MeasureSelectColumn+ 'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS '+@DimensionName+' '
--				SET @GroupBy=@GroupBy+'dbo.GetDatePart(' + @DimensionIndex +'.DisplayValue,''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''')' 
			END
	      --Total dimension count
		 if(@Count=1)
		 BEGIN
			
			SET @DimensionName1=@DimensionName
			SET @DimensionId1=@DimensionId
			
		END
		else

		 BEGIN
		 SET @DimensionName2=@DimensionName
		 SET @DimensionId2=@DimensionId
		
		 END
		SET @Count = @Count+1
		
		
	FETCH NEXT FROM @DimensionCursor
	INTO @DimensionId,@DimensionName,@IsDateDimension,@DimensionIndex
	END
	CLOSE @DimensionCursor
	DEALLOCATE @DimensionCursor
	if(@GType='ColumnRange')
	SET @CreateTableColumn = @CreateTableColumn + ',DislayValue NVARCHAR(1000)'

		IF(LOWER(@DisplayStatSignificance)='rate')
		BEGIN
			DECLARE @ConfidenceLevel FLOAT = (SELECT ConfidenceLevel FROM ReportGraph WHERE id=@ReportGraphId)
			SET @CreateTableColumn = @CreateTableColumn + ',PopulationRate NVARCHAR(1000),[LowerLimit-'+CAST((@ConfidenceLevel*100) AS NVARCHAR)+'%] NVARCHAR(1000),[UpperLimit-'+CAST((@ConfidenceLevel*100) AS NVARCHAR)+'%]  NVARCHAR(1000)'	
			SET @Column = @Column+','+'0,0,0 '
		END
		ELSE IF(@GType='errorbar')
		BEGIN
		SET @CreateTableColumn = @CreateTableColumn + ',PopulationRate NVARCHAR(1000),LowerLimit NVARCHAR(1000),UpperLimit NVARCHAR(1000)'	
		SET @Column = @Column+','+'0,0,0 '
		END

		IF(@IsDateDImensionExist=1 and (@Count-1=1 or @Count=1) )
			SET @IsonlyDateDimensionExist=1

	Insert into @DimensionTable(SelectDimension,CreateTableColumn,MeasureTableColumn,MeasureSelectColumn,UpdateCondition,GroupBy,Totaldimensioncount,DimensionName1,
	DimensionName2,DimensionId1,DimensionId2,IsDateDImensionExist,IsDateOnYAxis,IsonlyDateDimensionExist,cnt)values(@column ,@CreateTableColumn,@MeasureTableColumn,@MeasureSelectColumn,@UpdateCondition,@GroupBy,CAST(@Count-1 as nvarchar),@DimensionName1,
	@DimensionName2,@DimensionId1,@DimensionId2,@IsDateDImensionExist,@IsDateOnYAxis,@IsonlyDateDimensionExist,1)

	RETURN 
END
