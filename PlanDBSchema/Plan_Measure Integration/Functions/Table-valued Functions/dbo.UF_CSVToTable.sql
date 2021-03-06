IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'UF_CSVToTable') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION UF_CSVToTable
GO

CREATE FUNCTION [UF_CSVToTable]
(
 @psCSString VARCHAR(MAX)
)
RETURNS @otTemp TABLE(sID VARCHAR(MAX))
AS
BEGIN
 DECLARE @sTemp VARCHAR(MAX)

 WHILE LEN(@psCSString) > 0
 BEGIN
  SET @sTemp = LEFT(@psCSString, ISNULL(NULLIF(CHARINDEX(',', @psCSString) - 1, -1),
                    LEN(@psCSString)))
  SET @psCSString = SUBSTRING(@psCSString,ISNULL(NULLIF(CHARINDEX(',', @psCSString), 0),
                               LEN(@psCSString)) + 1, LEN(@psCSString))
  INSERT INTO @otTemp VALUES (@sTemp)
 END

RETURN
END

GO
