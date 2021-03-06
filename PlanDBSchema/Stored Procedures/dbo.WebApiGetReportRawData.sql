IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WebApiGetReportRawData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[WebApiGetReportRawData] AS' 
END
GO
CREATE PROCEDURE [dbo].[WebApiGetReportRawData] 
(
	@Id INT, @TopOnly BIT = 1, @ViewBy NVARCHAR(2) = 'Q',@StartDate DATETIME= '1/1/1900', @EndDate DATETIME = '1/1/2100', @FilterValues NVARCHAR(MAX) = NULL
)
AS 
BEGIN
	SET NOCOUNT ON;
	--Identify if it is a custom query or not
	DECLARE @CustomQuery NVARCHAR(MAX),@DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX),@CustomFilter NVARCHAR(MAX), @GT NVARCHAR(100)
	SELECT TOP 1 
			@GT						= ISNULL(G.GraphType,''),
			@CustomQuery			= ISNULL(G.CustomQuery,''),
			@DrillDownCustomQuery	= ISNULL(G.DrillDownCustomQuery,''),
			@DrillDownXFilter		= ISNULL(G.DrillDownXFilter,''),
			@CustomFilter			= ISNULL(G.CustomFilter,'')
	FROM ReportGraph G 
		LEFT JOIN ReportAxis A			ON G.id = A.ReportGraphId 
		WHERE G.Id = @Id

	IF(@GT != 'bar' AND @GT != 'column' AND @GT != 'pie' AND @GT != 'donut' AND @GT != 'line' AND @GT != 'stackbar' AND @GT != 'stackcol' AND @GT != 'area' AND @GT != 'bubble' AND @GT != 'scatter' AND @GT != 'columnrange' AND @GT != 'negativearea' AND @GT != 'negativebar' AND @GT != 'negativecol' AND @GT != 'solidgauge' AND @GT != 'gauge')
	BEGIN
			SET @GT = 'Currently we are not supporting graph type "'+ @GT +'"  in Measure Report Web API'
			RAISERROR(@GT,16,1) 
	END
	ELSE
	BEGIN
		--Identify if there is only date dimension is configured and will return with the chart attribute
		DECLARE @IsDateDimensionOnly BIT = 0
		IF(@CustomQuery != '') --Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
		BEGIN
			SET @IsDateDimensionOnly = 
				CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) = 1 THEN
						(SELECT COUNT(*) FROM ReportAxis  A
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
						WHERE ReportGraphId  = @Id)
				ELSE 0 
			END
		END

		--Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
		DECLARE @DateOnX BIT = 0
		IF(@CustomQuery = '') 
		BEGIN
			SET @DateOnX = 
				CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) > 1 THEN
						(SELECT COUNT(*) FROM ReportAxis  A
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
						WHERE ReportGraphId  = @Id AND AxisNAme = 'X')
				ELSE 0 
			END
		END
	
		DECLARE @ColumnNames NVARCHAR(MAX) 
		SELECT @ColumnNames = COALESCE(@ColumnNames + ', ', '') +  AttributeKey FROM (SELECT DISTINCT  '[' + AttributeKey + ']' AttributeKey FROM ChartOptionAttribute)  A
		IF(@ColumnNames IS NULL OR @ColumnNames ='')
			SET @ColumnNames = 'X'

		DECLARE @Query NVARCHAR(MAX);
		SET @Query = '
		;WITH ReportAttribute AS (
		SELECT 
				Id,
				GraphType				=	ISNULL(GraphType,''''),
				IsLableDisplay			=	ISNULL(IsLableDisplay,0),
				IsLegendVisible			= 	ISNULL(IsLegendVisible,0),
				LegendPosition			=	ISNULL(LegendPosition,''right,middle,y''),
				IsDataLabelVisible		= 	ISNULL(IsDataLabelVisible,0),
				DataLablePosition		=	ISNULL(DataLablePosition,''''),
				DefaultRows				=	ISNULL(DefaultRows,10),
				ChartAttribute			=	ISNULL(ChartAttribute,''''),
				ConfidenceLevel			=	ConfidenceLevel,
				CustomQuery				=	ISNULL(CustomQuery,''''),
				IsSortByValue			=	ISNULL(IsSortByValue,0),
				SortOrder				=	ISNULL(SortOrder,''asc''),
				DrillDownCustomQuery	=	ISNULL(DrillDownCustomQuery,''''),
				DrillDownXFilter		=	ISNULL(DrillDownXFilter,''''),
				CustomFilter			=	ISNULL(CustomFilter,''''),
				TotalDecimalPlaces		=	(SELECT TOP 1 CASE WHEN ISNULL(TotalDecimalPlaces,-1) = -1 THEN ISNULL(G.TotalDecimalPlaces,-1) ELSE TotalDecimalPlaces END FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				MagnitudeValue			=	(SELECT TOP 1 CASE WHEN ISNULL(MagnitudeValue,'''') = '''' THEN ISNULL(G.MagnitudeValue,'''') ELSE MagnitudeValue END  FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				DimensionCount			=   CASE WHEN ISNULL(CustomQuery,'''') = ''''
												THEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + ')
											ELSE 
												CASE WHEN CHARINDEX(''#DIMENSIONGROUP#'',CustomQuery) <= 0
													THEN 1
													ELSE 2
												END
											END,
				SymbolType = (SELECT TOP 1 ISNULL(SymbolType,'''') FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				IsDateDimensionOnly		=   '+ CAST(@IsDateDimensionOnly AS NVARCHAR) +',
				DateOnX					=   '+ CAST(@DateOnX AS NVARCHAR) +'
				FROM ReportGraph G
				WHERE G.Id = ' + CAST(@Id AS NVARCHAR) + '
		),
		ExtendedAttribute AS
		(
	
			SELECT * FROM (
						SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
						LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
						WHERE C1.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '
				) AS R
				PIVOT 
				(
					MIN(AttributeValue)
					FOR AttributeKey IN ( '+  @ColumnNames + ')
				) AS A
				
		)
		SELECT *,ChartColor = dbo.GetColor(E.ColorSequenceNo) FROM ReportAttribute R
		LEFT JOIN ExtendedAttribute E ON R.id = E.ReportGraphId1
		'
	
		--This dynamic query will returns all the attributes of chart
	
		EXEC(@Query)

	
		DECLARE @DateDimensionId INT;
		DECLARE @DimensionName VARCHAR(MAX);
		DECLARE @FilterXML NVARCHAR(MAX) = NULL
		DECLARE @FilterXMLString NVARCHAR(MAX) = NULL
		DECLARE @DimList NVARCHAR(MAX) = NULL
		DECLARE @ColDimLst NVARCHAR(MAX)
			IF(@CustomQuery != '') --In case of custom query is configured for the report
			BEGIN
			
				SELECT TOP 1 @DateDimensionId = DateD.id FROM ReportAxis A 
					INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDeleted = 0
					INNER JOIN Dimension DateD ON D.TableName = DateD.TableName and DateD.IsDateDimension = 1 AND DateD.IsDeleted = 0
				WHERE A.ReportGraphId = @Id

				IF(@GT = 'columnrange')
				BEGIN
					IF (@FilterValues IS NOT NULL AND @FilterValues != '')
					BEGIN
						SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
						SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterValues, ',')
						SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
					END
					ELSE
					BEGIN
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id order by DimensionId asc
					END

					IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
					BEGIN
						SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
					END	
				END
				ELSE
				BEGIN
					IF (@FilterValues IS NOT NULL AND @FilterValues != '')
					BEGIN
						SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
						SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
					END
					ELSE
					BEGIN
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
					END

					IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
					BEGIN
						SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
					END
				END

				IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
				BEGIN
					SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
				END
			
				IF((CHARINDEX('#DIMENSIONGROUP#',@CustomQuery) > 0 OR CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) > 0) AND ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
				BEGIN
					EXEC [CustomGraphQuery]  
						@ReportGraphID			= @Id, 
						@STARTDATE				= @StartDate, 
						@ENDDATE				= @EndDate,
						@FilterValues			= @FilterXML,
						@ViewByValue			= @ViewBy,
						@DimensionTableName		= '',
						@DateDimensionId		= @DateDimensionId,--this value must be pass , other wise CustomGraphQuery will throw an error
						@IsDrillDownData		= 0,
						@DrillRowValue			= NULL,
						@SortBy					= NULL,
						@SortDirection			= NULL,
						@PageSize				= NULL,
						@PageIndex				= NULL,
						@IsExportAll			= 0,
						@Rate                   = @Rate
				END
				ELSE IF(CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) <= 0)
				BEGIN
					EXEC [CustomGraphQuery]  
						@ReportGraphID			= @Id, 
						@STARTDATE				= @StartDate, 
						@ENDDATE				= @EndDate,
						@FilterValues			= @FilterXML,
						@ViewByValue			= @ViewBy,
						@DimensionTableName		= '',
						@DateDimensionId		= '', --this value is not passed here
						@IsDrillDownData		= 0,
						@DrillRowValue			= NULL,
						@SortBy					= NULL,
						@SortDirection			= NULL,
						@PageSize				= NULL,
						@PageIndex				= NULL,
						@IsExportAll			= 0,
						@Rate                   = @Rate
				END
				ELSE 
				BEGIN
						RAISERROR('Date Dimension is not configured for Report ',16,1) 
				END
			END
			ELSE --In case of custom query is not configured for the report, but Dimension and Measure are configured
			BEGIN
					SELECT TOP 1 @DateDimensionId = DateD.id FROM ReportAxis A 
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDeleted = 0
						INNER JOIN Dimension DateD ON D.TableName = DateD.TableName and DateD.IsDateDimension = 1 AND DateD.IsDeleted = 0
					WHERE A.ReportGraphId = @Id

					IF(ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
					BEGIN
						IF(@GT = 'columnrange')
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id order by DimensionId asc
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END	
						END
						ELSE
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END						
						END

						IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
						BEGIN
							SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
						END
						
						EXEC [ReportGraphResultsNew]
							@ReportGraphID						= @Id, 
							@DIMENSIONTABLENAME					= @DimensionName, 
							@STARTDATE							= @StartDate, 
							@ENDDATE							= @EndDate, 
							@DATEFIELD							= @DateDimensionId, 
							@FilterValues						= @FilterXML,
							@ViewByValue						= @ViewBy,
							@SubDashboardOtherDimensionTable	= 0,
							@SubDashboardMainDimensionTable		= 0,
							@DisplayStatSignificance			= NULL,
							@UserId								= NULL,
							@RoleId								= NULL,
							@Rate                               = @Rate
					END
					ELSE
					BEGIN
							RAISERROR('Date Dimension is not configured for Report ',16,1) 
					END
			END

			--If chart type is Gauge than returns Measure Output values

			IF (@GT='gauge')
			BEGIN
					SELECT 
						LowerLimit,
						UpperLimit,
						Value 
					FROM MeasureOutputValue MOV
					INNER JOIN ReportGraphColumn RGC 
					ON MOV.MeasureId=RGC.MeasureId
					WHERE RGC.ReportGraphId=@Id

			END

			SELECT ISNULL(m.IsCurrency, 0) AS IsCurrency 
				FROM Measure m 
				INNER JOIN ReportGraphColumn rgc ON (rgc.MeasureId = m.id AND rgc.ReportGraphId = @Id)
		END
END

GO