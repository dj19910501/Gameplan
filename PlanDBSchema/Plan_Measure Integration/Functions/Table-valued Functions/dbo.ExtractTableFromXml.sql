CREATE FUNCTION [dbo].[ExtractTableFromXml] (@XmlString XML)
RETURNS @FilterString TABLE (
			ID NVARCHAR(MAX),
			DimensionValueID NVARCHAR(MAX)
		)
AS
BEGIN
	
	--DECLARE @FilterValues NVARCHAR(MAX)=(@XmlString)
	--DECLARE @XmlString XML
		--SET @XmlString = @FilterValues;
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

		INSERT INTO @FilterString SElect * from @TempValueTable
	RETURN
END