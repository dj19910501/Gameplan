IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetTTestSignificance') AND xtype IN (N'P'))
    DROP PROCEDURE GetTTestSignificance
GO


CREATE PROCEDURE [GetTTestSignificance]  
@ReportGraphID int, 
@DIMENSIONTABLENAME nvarchar(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15),
@SubDashboardOtherDimensionTable int = 0,	
@SubDashboardMainDimensionTable int = 0,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'

AS
BEGIN
DECLARE @Dimensions NVARCHAR(MAX),@IsDateDimensionExist BIT,@IsDateOnYAxis BIT,@TotalDimensionCount INT, @TOTALSQL NVARCHAR(MAX), 
		@DimensionId1 INT,@DimensionId2 INT, @Columns NVARCHAR(1000), @BaseQuery NVARCHAR(1000),@DimensionName1 NVARCHAR(500), @DimensionName2 NVARCHAR(500),
		@CLOSINGSQL NVARCHAR(MAX),@DistinctOnly VARCHAR(20),@CreateDimensionTable NVARCHAR(1000), @GType NVARCHAR(100),
		@IsOnlyDateDimension BIT=0;

SET @Columns='' ;SET @BaseQuery='';SET @DistinctOnly=' DISTINCT '
SELECT @GType =LOWER (GraphType) FROM ReportGraph WHERE id = @ReportGraphID

DECLARE @TTestTable TABLE(Dimension1 NVARCHAR(400) INDEX Dimension1 CLUSTERED,ColumnOrder1 NVARCHAR(500), Dimension2 NVARCHAR(500),ColumnOrder2 NVARCHAR(500), TTestValue FLOAT); 
DECLARE @TTestTableTemp TABLE(Dimension1 NVARCHAR(400) INDEX Dimension1 CLUSTERED,ColumnOrder1 NVARCHAR(500), Dimension2 NVARCHAR(500),ColumnOrder2 NVARCHAR(500), TTestValue FLOAT); 

 --Get Dimension Table 
SELECT 
		@Dimensions				= selectdimension, 
		@Columns				= CreateTableColumn,
		@IsDateDimensionExist	= IsDateDimensionExist, 
		@IsDateOnYAxis			= IsDateOnYAxis,
		@DimensionName1			= DimensionName1, 
		@DimensionName2			= DimensionName2, 
		@DimensionId1			= DimensionId1,
		@DimensionId2			= DimensionId2,
		@TotalDimensionCount	= Totaldimensioncount,
		@IsOnlyDateDimension	= CAST(IsOnlyDateDImensionExist AS BIT) 
FROM dbo.GetDimensions(@ReportGraphID,@ViewByValue,@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@GType,@SubDashboardOtherDimensionTable,@SubDashboardMainDimensionTable,'')

SET @BaseQuery=(dbo.DimensionBaseQuery(@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@ReportGraphID,@FilterValues,@IsOnlyDateDimension,1,@UserId,@RoleId))
SET @BaseQuery=REPLACE(@BaseQuery,'#COLUMNS#',@Dimensions)

-- Create dimension table
SET @CreateDimensionTable='DECLARE @DimensionTable TABLE( '+@Columns+' )'
SET @CreateDimensionTable=@CreateDimensionTable+'INSERT INTO @DimensionTable '+@BaseQuery+'';

		IF(@TotalDimensionCount > 1)
		BEGIN
			IF(@IsDateDimensionExist = 0)
			BEGIN
				SET @CLOSINGSQL = N'SELECT T1.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName2+ ',T1.OrderValue2 FROM @DimensionTable T1 ORDER BY T1.OrderValue1,T1.OrderValue2' 
			END
			ELSE
			BEGIN
				IF(@IsDateOnYAxis = 1)
				BEGIN
					SET @DimensionId2 = @DimensionId1
					SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName1+ ',T1.OrderValue1 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
				END
				ELSE
				BEGIN
					SET @DimensionId1 = @DimensionId2
					SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName2+ ',T2.OrderValue2,T1.' +@DimensionName2+ ',T1.OrderValue2 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
				END
			END
			
		END
		ELSE 
		BEGIN
			SET @DimensionId2 = @DimensionId1
			SET @CLOSINGSQL = N'SELECT ' + @DistinctOnly + ' T2.' +@DimensionName1+ ',T1.OrderValue1,T1.' +@DimensionName1+ ',T1.OrderValue1 FROM @DimensionTable T1 ' +
									'CROSS JOIN @DimensionTable T2' 
		END
		SET @TOTALSQL = @CreateDimensionTable + ' ' + @CLOSINGSQL

		INSERT INTO @TTestTableTemp(Dimension1, ColumnOrder1, Dimension2, ColumnOrder2)
		EXEC(@TOTALSQL)
		
		--Get distinct dimension value pair with order by 
		INSERT INTO @TTestTable(Dimension1, ColumnOrder1, Dimension2, ColumnOrder2)
		SELECT	DISTINCT
			Dimension1, 
			ColumnOrder1 = MIN(ColumnOrder1) OVER (PARTITION BY Dimension1), 
			Dimension2, 
			ColumnOrder2 = MIN(ColumnOrder2) OVER (PARTITION BY Dimension2)
		FROM @TTestTableTemp ORDER BY ColumnOrder1,ColumnOrder2
		
		DECLARE @Dimension1 NVARCHAR(500), @ColumnOrder1 NVARCHAR(500), @Dimension2 NVARCHAR(500), @ColumnOrder2 NVARCHAR(500),
				@TTestQuery NVARCHAR(MAX), @IsTTestValueExist float;
		
		DECLARE TTestCursor CURSOR FOR
		SELECT Dimension1,ColumnOrder1,Dimension2,ColumnOrder2 FROM @TTestTable ORDER BY ColumnOrder1
		OPEN TTestCursor
		FETCH NEXT FROM TTestCursor
		INTO @Dimension1, @ColumnOrder1, @Dimension2,@ColumnOrder2 
			WHILE @@FETCH_STATUS = 0
				BEGIN
				
				SET @IsTTestValueExist= (SELECT TOP 1 TTestValue FROM @TTestTable WHERE Dimension1=@Dimension1 AND Dimension2=@Dimension2)
				
				-- Check if TTestValue already exist
				IF(@IsTTestValueExist IS NULL)
				BEGIN
					SET @Dimension1 = 'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':' + @Dimension1;
					SET @Dimension2 = 'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':' + @Dimension2;
				
					DECLARE @PValue FLOAT;
					IF(@Dimension1 != @Dimension2)
					BEGIN
					BEGIN TRY
						IF(@GType='coheatmap')
							SET @TTestQuery = [dbo].[GetTTestQueryForCoRelation] (@Dimension1,@Dimension2,@ReportGraphID,@DIMENSIONTABLENAME,@STARTDATE ,@ENDDATE ,@DATEFIELD ,@FilterValues ,@ViewByValue ,@SubDashboardOtherDimensionTable ,@SubDashboardMainDimensionTable )
						ELSE
							SET @TTestQuery = [dbo].[GetTTestQueryForAll](@Dimension1,@Dimension2,@ViewByValue,@STARTDATE,@ENDDATE,@ReportGraphID,@DIMENSIONTABLENAME,@DATEFIELD,@FilterValues,@TotalDimensionCount) 
						
						-- call TTestCalculation stored procedure to get @PValue
						EXECUTE TTestCalculation @TTestQuery ,@PValue OUTPUT
					END TRY
					BEGIN CATCH
						SET @PValue = NULL
					END CATCH
					END
					ELSE
					BEGIN
						SET @PValue = NULL
					END
					
					--If t-test value is lesser than 0.001 set it to zero
					IF(@PValue < 0.001)
						SET @PValue = 0;

					UPDATE @TTestTable SET TTestValue = @PValue 
					WHERE (Dimension1 = REPLACE(@Dimension1,'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':','') 
						   AND Dimension2 = REPLACE(@Dimension2,'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':','')) 
						OR 
						   (Dimension2 = REPLACE(@Dimension1,'D' + CAST(ISNULL(@DimensionId1,@DimensionId2) AS NVARCHAR) + ':','') 
						   AND Dimension1 = REPLACE(@Dimension2,'D' + CAST(ISNULL(@DimensionId2,@DimensionId1) AS NVARCHAR) + ':',''))	
				END
				FETCH NEXT FROM TTestCursor
				INTO @Dimension1, @ColumnOrder1, @Dimension2,@ColumnOrder2 
			END
		CLOSE TTestCursor
		DEALLOCATE TTestCursor
		SELECT Dimension1,Dimension2,TTestValue AS MeasureValue FROM @TTestTable ORDER BY ColumnOrder1;
END

GO
