IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[Attribution_PositionBased]') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Attribution_PositionBased]
END
GO

CREATE PROCEDURE [dbo].[Attribution_PositionBased]
@AttrQuery NVARCHAR(MAX),
@AttrWhereQuery NVARCHAR(MAX),
@BaseTblName NVARCHAR(MAX),
@AttributionType INT
AS
BEGIN
BEGIN TRY
	DECLARE @Query NVARCHAR(MAX) ='',
			@TouchDateFieldName DATETIME, 
			@Revenue FLOAT, 
			@Index INT, 
			@DimensionId INT, 
			@DimensionValue NVARCHAR(MAX), 
			@RowNumber INT,
			@MinValue INT, 
			@MaxValue INT,
			@TouchId INT,
			@OppFirstTouchDate DATETIME,
			@OppLastTouchDate DATETIME,
			@OpportunityFieldname NVARCHAR(MAX),
			@DenseRank INT,
			@WhereClause NVARCHAR(MAX)

	-- Create temp table 
	CREATE TABLE #TempData (id INT,
							OpportunityFieldname NVARCHAR(100),
							TouchDateFieldName DATE,
							Revenue FLOAT,
							IndexNo INT,
							CampaignId NVARCHAR(100),
							DimensionId INT,
							DimensionValue NVARCHAR(MAX),
							TouchWeight INT,
							RevunueAfterWeight FLOAT,
							idDenseRank INT)

	SET @Query = ' DECLARE @TempTouchData TABLE(id INT NOT NULL PRIMARY KEY IDENTITY, OpportunityFieldname NVARCHAR(100), TouchDateFieldName DATE, '+
				 ' Revenue FLOAT, IndexNo INT, CampaignId NVARCHAR(100), DimensionId INT, DimensionValue NVARCHAR(MAX), TouchWeight INT, '+
				 ' RevunueAfterWeight FLOAT, idDenseRank INT) '+
				 ' INSERT INTO @TempTouchData (idDenseRank,OpportunityFieldname,TouchDateFieldName,Revenue,IndexNo,CampaignId)'

	-- Set base table name
	SET @BaseTblName = @BaseTblName + '_Base'

	-- If base table exist then apply inner join of base table and dimension value table
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @BaseTblName)
	BEGIN
		DECLARE @ConfigDimensionId INT, @ConfigDimensionValue NVARCHAR(MAX)
		IF ((SELECT COUNT(DISTINCT DimensionId) FROM AttrPositionConfig WHERE DimensionId IS NOT NULL) = 1)
		BEGIN
			SET @ConfigDimensionId = (SELECT TOP 1 DimensionId FROM AttrPositionConfig WHERE DimensionId IS NOT NULL)
			DECLARE @ColumnName NVARCHAR(MAX) = 'DimensionValue' + CAST(@ConfigDimensionId AS NVARCHAR)
			IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = @ColumnName AND OBJECT_ID = OBJECT_ID(@BaseTblName))
			BEGIN
				SET @Query = REPLACE(@Query, 'INSERT INTO @TempTouchData (',' INSERT INTO @TempTouchData (DV.DimensionId,DV.DimensionValue,')
				SET @Query = @Query + REPLACE(@AttrQuery,'SELECT',' SELECT DV.DimensionId, DV.DisplayValue AS DimensionValue, ')
			
				SET @Query = @Query + ' INNER JOIN  ' + @BaseTblName + ' WB' +' ON objTouches.id = WB.id INNER JOIN DimensionValue DV ON WB.' + @ColumnName + ' = DV.id '	
				SET @WhereClause = @AttrWhereQuery + ' AND DV.DimensionId = ' + CAST(@ConfigDimensionId AS NVARCHAR)
			END
			ELSE
			BEGIN
				SET @WhereClause = @AttrWhereQuery
				SET @Query = @Query + @AttrQuery
			END
		END
		ELSE
		BEGIN
			SET @WhereClause = @AttrWhereQuery
			SET @Query = @Query + @AttrQuery
		END
	END
	ELSE
	BEGIN
		SET @WhereClause = @AttrWhereQuery
		SET @Query = @Query + @AttrQuery
	END

	SET @Query = @Query + ISNULL(@WhereClause,'') + ' ;SELECT * FROM @TempTouchData '
	
	INSERT INTO #TempData EXEC(@Query)
	
	-- Get position based revenue value
	DECLARE @TouchRowCursor CURSOR
	SET @TouchRowCursor = CURSOR FAST_FORWARD FOR SELECT Id,TouchDateFieldName,Revenue,[IndexNo],DimensionId,DimensionValue,OpportunityFieldname FROM #TempData
	OPEN @TouchRowCursor
	FETCH NEXT FROM @TouchRowCursor
	INTO @TouchId, @TouchDateFieldName, @Revenue, @Index, @DimensionId, @DimensionValue,@OpportunityFieldname
	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- Get minimum and maximum index values and dates of first and last touch for an opportunity
		SELECT @MaxValue = MAX([IndexNo]),@MinValue = MIN([IndexNo]), @OppFirstTouchDate = MIN(TouchDateFieldName), @OppLastTouchDate = MAX(TouchDateFieldName) 
		FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname
		GROUP BY OpportunityFieldname 

		-- Update touch value as per configuration 
		UPDATE #TempData 
		SET Revenue = dbo.GetPositionBasedRevenue(@TouchDateFieldName,@Revenue,@Index,@DimensionId,@DimensionValue,@MinValue,@Maxvalue,@AttributionType,@OppFirstTouchDate,@OppLastTouchDate) 
		WHERE Id = @TouchId
		
		FETCH NEXT FROM @TouchRowCursor
		INTO @TouchId, @TouchDateFieldName,@Revenue,@Index,@DimensionId,@DimensionValue,@OpportunityFieldname
	END
	CLOSE @TouchRowCursor
	DEALLOCATE @TouchRowCursor

	-- Update average revenue in case when multiple records in first/last touches
	DECLARE @RevenueCursor CURSOR
	SET @RevenueCursor = CURSOR FAST_FORWARD FOR SELECT DISTINCT idDenseRank, OpportunityFieldname FROM #TempData
	OPEN @RevenueCursor 
	FETCH NEXT FROM @RevenueCursor 
	INTO @DenseRank, @OpportunityFieldname 
	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #TempData 
		SET Revenue = ROUND((SELECT AVG(Revenue) FROM #TempData WHERE idDenseRank = @DenseRank AND OpportunityFieldname = @OpportunityFieldname 
						GROUP BY OpportunityFieldname, idDenseRank),2)
		WHERE idDenseRank = @DenseRank AND OpportunityFieldname = @OpportunityFieldname
	
		FETCH NEXT FROM @RevenueCursor 
		INTO @DenseRank, @OpportunityFieldname
	END
	CLOSE @RevenueCursor 
	DEALLOCATE @RevenueCursor 
	
	SELECT OpportunityFieldname,TouchDateFieldName,Revenue,CampaignId,DimensionId,DimensionValue FROM #TempData 

END TRY
BEGIN CATCH
	PRINT ERROR_MESSAGE()
END CATCH
END
GO