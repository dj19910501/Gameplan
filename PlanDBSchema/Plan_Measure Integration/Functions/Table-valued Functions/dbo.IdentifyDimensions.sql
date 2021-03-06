IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'IdentifyDimensions') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION IdentifyDimensions
GO
-- =============================================
-- Author:		Manoj
-- Create date: 20Jun2014
-- Description:	Identify the dimensions and order them
-- =============================================
CREATE FUNCTION [IdentifyDimensions] 
(	
	@ObjectId INT,
	@IsGraph INT,
	@String NVARCHAR(1000)
)
RETURNS @tmpTable TABLE (Number INT,DimensionId INT)
AS
BEGIN
	DECLARE @temp_Table TABLE (DimensionId INT)
	DECLARE @Delimiter NVARCHAR(2);
	DECLARE @ConfiguredDimensions NVARCHAR(1000);
	DECLARE @Id INT
	SET @Delimiter = 'd';
	SET @ConfiguredDimensions = '';
	IF(@IsGraph = 1)	--Graph
	BEGIN	
			INSERT @temp_Table
			SELECT D.id FROM ReportGraph RG 
			INNER JOIN ReportGraphColumn GC ON GC.ReportGraphId = RG.id
			INNER JOIN Measure M ON M.id = GC.MeasureId
			INNER JOIN ReportAxis RA ON RA.ReportGraphId = RG.id
			INNER JOIN Dimension D ON D.id = RA.Dimensionid
			WHERE RG.id = @ObjectId
	END
	ELSE IF(@IsGraph = 2) -- Key data
	BEGIN
			INSERT @temp_Table
			SELECT DimensionId FROM KeyDataDimension WHERE KeyDataId = @ObjectId
	END		
	ELSE IF (@IsGraph = 3) -- Table
	BEGIN
			INSERT @temp_Table
			select DimensionId from ReportTableDimension WHERE ReportTableId = @ObjectId 
	END		
	ELSE
	BEGIN
			INSERT @temp_Table
			select DimensionId from ReportTableDimension WHERE ReportTableId = @ObjectId 
	END		
	DECLARE Column_Cursor CURSOR FOR
	SELECT DimensionId id FROM @temp_Table
	ORDER BY DimensionId
	OPEN Column_Cursor
	FETCH NEXT FROM Column_Cursor INTO @Id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @ConfiguredDimensions = @ConfiguredDimensions + 'd' + CAST(@Id AS NVARCHAR)
		FETCH NEXT FROM Column_Cursor INTO @Id
		END
	Close Column_Cursor
	Deallocate Column_Cursor
	SET @String = @String + @ConfiguredDimensions;
	;WITH Split(stpos,endpos) 
		AS(
			SELECT 0 AS stpos, CHARINDEX(@Delimiter,@String) AS endpos
			UNION ALL
			SELECT endpos+1, CHARINDEX(@Delimiter,@String,endpos+1)
				FROM Split
				WHERE endpos > 0
		),AllData AS(
			
			SELECT DISTINCT 
				'DimensionId' = SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos)
			FROM Split
			WHERE ISNULL(SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos),'') <> ''
		)
		INSERT @tmpTable 
		SELECT 'Number' = ROW_NUMBER() OVER (ORDER BY (SELECT 1)),* FROM AllData ORDER BY CAST (AllData.DimensionId AS INT) 
RETURN 
END



GO
