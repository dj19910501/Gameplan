IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'fnSplitString') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION fnSplitString
GO
GO
CREATE FUNCTION [fnSplitString] 
( 
    @string NVARCHAR(MAX), 
    @delimiter CHAR(1) 
) 
RETURNS @output TABLE(dimension NVARCHAR(MAX),cnt int 
) 
BEGIN 
    DECLARE @start INT, @end INT ,@index INT 
    SELECT @start = 1, @end = CHARINDEX(@delimiter, @string) ,@index=0
    WHILE @start < LEN(@string) + 1 BEGIN 
        IF @end = 0  
            SET @end = LEN(@string) + 1
       IF(SUBSTRING(@string, @start, @end - @start)!='')
	   BEGIN
	 
        INSERT INTO @output (dimension,cnt)  
        VALUES(SUBSTRING(@string, @start, @end - @start),@index) 
		END
		  set @index  =@index + 1
        SET @start = @end + 1 
        SET @end = CHARINDEX(@delimiter, @string, @start)
        
    END 
    RETURN 
END

GO
