IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'InteractionBasedAttrCalc') AND xtype IN (N'P'))
    DROP PROCEDURE [InteractionBasedAttrCalc]
GO

CREATE PROCEDURE [dbo].[InteractionBasedAttrCalc]
@AttrQuery NVARCHAR(MAX),
@AttrWhereQuery NVARCHAR(MAX),
@BaseTblName NVARCHAR(MAX)
AS
BEGIN
	BEGIN TRY
		DECLARE @Query NVARCHAR(MAX) ='',
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

		SET @Query = ' DECLARE @TempTouchData TABLE(id int NOT NULL PRIMARY KEY IDENTITY,OpportunityFieldname NVARCHAR(100),TouchDateFieldName DATE,Revenue FLOAT,IndexNo INT,  CampaignId NVARCHAR(100), DimensionId INT,								DimensionValue NVARCHAR(MAX), TouchWeight int, RevunueAfterWeight Float, idDenseRank int) '+
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
				IF EXISTS(SELECT * FROM sys.columns WHERE Name = @ColumnName AND Object_ID = Object_ID(@BaseTblName))
				BEGIN
					SET @Query = REPLACE(@Query, 'INSERT INTO @TempTouchData (',' INSERT INTO @TempTouchData (DV.DimensionId,DV.DimensionValue,')
					SET @Query = @Query + REPLACE(@AttrQuery,'SELECT',' SELECT DV.DimensionId, DV.DisplayValue AS DimensionValue, ')

					SET @Query = @Query + ' INNER JOIN  ' + @BaseTblName + ' WB' + ' ON objTouches.id = WB.id INNER JOIN DimensionValue DV ON WB.' + @ColumnName + ' = DV.id '	
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

		SET @Query = @Query + ISNULL(@WhereClause,'')
		SET @Query = @Query + ' ;SELECT * FROM @TempTouchData '

		Insert INTO #TempData EXEC(@Query)

	
		DECLARE @intFlag INT
		DECLARE @TotalRow INT

		SET @intFlag = 1
		SET @TotalRow = (select count(*) from #TempData)
		-- Get Interaction based revenue value
		WHILE (@intFlag <= @TotalRow)
		BEGIN
			DECLARE @TouchDateFieldName DATETIME
			DECLARE @DimensionId INT
			DECLARE @DimensionValue NVARCHAR(MAX)
			SET @TouchDateFieldName = (SELECT TouchDateFieldName FROM #TempData WHERE id = @intFlag)
			SET @DimensionId = (SELECT DimensionId FROM #TempData WHERE id = @intFlag)
			SET @DimensionValue = (SELECT DimensionValue FROM #TempData WHERE id = @intFlag)

			UPDATE #TempData SET TouchWeight = dbo.[GetWeight](@TouchDateFieldName,@DimensionId,@DimensionValue) where id = @intFlag

			SET @intFlag = @intFlag + 1
		end

		DECLARE @intFlag2 INT
		DECLARE @TotalRow2 INT

		SET @intFlag2 = 1
		SET @TotalRow2 = (SELECT COUNT(DISTINCT idDenseRank) FROM #TempData)
		-- Calculate Revenue based on weight
		WHILE (@intFlag2 <= @TotalRow2)
		BEGIN
			DECLARE @OpportunityFieldname nvarchar(max)
			DECLARE @TotalRevenue FLOAT
			DECLARE @TotalTouchWeight FLOAT
			DECLARE @AvgTouchWeight FLOAT
			SET @OpportunityFieldname = (SELECT TOP 1 OpportunityFieldname FROM #TempData WHERE idDenseRank = @intFlag2)
			SET @TotalRevenue = (SELECT TOP 1 Revenue FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname)
			SET @TotalTouchWeight = (SELECT SUM(TouchWeight) FROM #TempData WHERE OpportunityFieldname = @OpportunityFieldname)
			IF(@TotalRevenue <> 0)
			BEGIN
				set @AvgTouchWeight = (select (@TotalRevenue / @TotalTouchWeight))
			END
			ELSE
			BEGIN
				set @AvgTouchWeight = 0
			END

			SET @intFlag2 = @intFlag2 + 1

			UPDATE #TempData set RevunueAfterWeight = (@AvgTouchWeight * TouchWeight) WHERE OpportunityFieldname = @OpportunityFieldname
		END

		SELECT OpportunityFieldname, TouchDateFieldName, CampaignId,	DimensionValue	,RevunueAfterWeight AS Revunue FROM #TempData
	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE()
	END CATCH
END
