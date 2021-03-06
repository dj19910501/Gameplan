-- =============================================
-- Created By Nishant Sheth
-- Created Date : 20-May-2016
-- Description : Remove Space and make word Captialize and remove special character
-- =============================================
IF object_id(N'RemoveSpaceAndUppercaseFirst', N'FN') IS NOT NULL
    DROP FUNCTION RemoveSpaceAndUppercaseFirst
GO
GO
/****** Object:  UserDefinedFunction [dbo].[RemoveSpaceAndUppercaseFirst]    Script Date: 05/20/2016 12:43:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[RemoveSpaceAndUppercaseFirst] ( @InputString varchar(MAX) ) 
RETURNS VARCHAR(MAX)
AS
BEGIN

DECLARE @Index          INT
DECLARE @Char           CHAR(1)
DECLARE @PrevChar       CHAR(1)
DECLARE @OutputString   VARCHAR(255)

SET @InputString=REPLACE(@InputString,'&amp;','&')
SET @InputString=REPLACE(@InputString,'&lt;','<')
SET @InputString=REPLACE(@InputString,'&gt;','>')

SET @OutputString = LOWER(@InputString)
SET @Index = 1

WHILE @Index <= LEN(@InputString)
BEGIN
    SET @Char     = SUBSTRING(@InputString, @Index, 1)
    SET @PrevChar = CASE WHEN @Index = 1 THEN ' '
                         ELSE SUBSTRING(@InputString, @Index - 1, 1)
                    END

    IF @PrevChar IN (' ', ';', ':', '!', '?', ',', '.', '_', '-', '/', '''', '(')
    BEGIN
        IF @PrevChar != '''' OR UPPER(@Char) != 'S'
            SET @OutputString = STUFF(@OutputString, @Index, 1, UPPER(@Char))
    END

    SET @Index = @Index + 1

END

SET @OutputString=REPLACE(@OutputString,' ','')

DECLARE @KeepValues AS VARCHAR(MAX)
SET @KeepValues = '%[^a-z0-9A-Z_]%'
WHILE PATINDEX(@KeepValues, @OutputString) > 0
SET @OutputString = STUFF(@OutputString, PATINDEX(@KeepValues, @OutputString), 1, '')


RETURN @OutputString

END
