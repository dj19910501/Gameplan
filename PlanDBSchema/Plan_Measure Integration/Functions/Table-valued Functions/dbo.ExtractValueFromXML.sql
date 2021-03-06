IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ExtractValueFromXML') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION ExtractValueFromXML
GO



-- =============================================
-- Author:		Manoj
-- Description:	Extract value from XML and make string for condition
-- =============================================
CREATE FUNCTION [ExtractValueFromXML] (@XmlString XML, @TableAlias NVARCHAR(10), @IsGraph INT)
RETURNS @FilterString TABLE (
			KeyValue NVARCHAR(MAX)
		)
AS
BEGIN
	
	--Temporary table will store the Id and values passed 
	DECLARE @temp_Table TABLE (
			ID NVARCHAR(10),
			Value NVARCHAR(1000)
		)

	INSERT INTO @temp_Table
	SELECT data.col.value('(@ID)[1]', 'int')
		,data.col.value('(.)', 'INT')
	FROM @XmlString.nodes('(/filters/filter)') AS data(col);

	--Based on the Id, combines value comma saperated
	WITH ExtractValue AS(
		SELECT ID, Value = STUFF((
				SELECT ', ' + Value
				FROM @temp_Table b
				WHERE b.ID = a.ID
				FOR XML PATH('')
				), 1, 2, '')
		FROM @temp_Table a
		GROUP BY ID
	),
	_AllFilters AS(
		SELECT 1 Id,  
		Condition = CASE 
					WHEN @IsGraph = 0 THEN  'AND '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					WHEN @IsGraph = 2 THEN  ' '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					ELSE ' left outer join DimensionValue d' + CONVERT(NVARCHAR(20),(ID + 4)) + ' on ' +  @TableAlias + '.Id = d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id and d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id in (' + Value  +')'
		END 
		FROM ExtractValue
	)
	INSERT @FilterString (KeyValue)
	SELECT DISTINCT 
    STUFF	(
				(SELECT CASE WHEN @IsGraph = 2 THEN '#' ELSE ',' END + B.Condition FROM _AllFilters B WHERE B.Id = A.Id FOR XML PATH(''))
	,1,1,''	) Condition
	FROM
    _AllFilters A;
	RETURN
END


GO
