-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 11/08/2014
-- Description :- Update DisplayFieldName to separate the words by splitting the string with space. Examplae 'StartDate' will be 'Start Date'
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE SPECIFIC_NAME = 'InitCap' AND ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = 'dbo')
BEGIN
	DROP FUNCTION [dbo].[InitCap] 
END
GO

CREATE FUNCTION [dbo].[InitCap] ( @InputString varchar(4000) ) 
RETURNS VARCHAR(4000)
AS
BEGIN

	DECLARE @Index          INT
	DECLARE @Char           CHAR(1)
	DECLARE @PrevChar       CHAR(1)
	DECLARE @OutputString   VARCHAR(255)

	SET @OutputString = @InputString
	SET @Index = 1

	WHILE @Index <= LEN(@InputString)
	BEGIN
		SET @Char     = SUBSTRING(@InputString, @Index, 1)
		SET @PrevChar = CASE WHEN @Index = 1 THEN ' '
							 ELSE SUBSTRING(@InputString, @Index - 1, 1)
						END

		IF LOWER(@PrevChar) = @PrevChar COLLATE Latin1_General_CS_AS AND UPPER(@Char) = @Char COLLATE Latin1_General_CS_AS
		BEGIN
			IF @PrevChar != ' ' AND @Char != ' ' AND @PrevChar != '(' AND @PrevChar != ')' AND @Char != '(' AND @Char != ')'
				SET @OutputString = STUFF(@OutputString, @Index, 1, UPPER( ' ' + @Char))
		END

		SET @Index = @Index + 1
	END

	RETURN @OutputString

END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'DisplayFieldName')
	BEGIN

		UPDATE GameplanDataType SET DisplayFieldName = dbo.[InitCap](DisplayFieldName)   
		
		UPDATE GameplanDataType Set DisplayFieldName = 'Owner' WHERE DisplayFieldName COLLATE Latin1_General_CS_AS = 'owner'

	END

END

GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE SPECIFIC_NAME = 'InitCap' AND ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = 'dbo')
BEGIN
	DROP FUNCTION [dbo].[InitCap] 
END
GO

SELECT DisplayFieldName from GameplanDataType