IF OBJECT_ID(N'dbo.FindDateDimension', N'FN') IS NOT NULL
    DROP FUNCTION dbo.FindDateDimension ;
GO

CREATE FUNCTION dbo.FindDateDimension (@tablename nvarchar(max), @Dimensions int)
RETURNS INT
AS
BEGIN
DECLARE @DIMENSIONTOCHECK int
DECLARE @dIndex int=1
DECLARE @dNextIndex int=CHARINDEX('d',@TABLENAME,@dIndex+1)

DECLARE @c int =1
DECLARE @found bit =0

while @c<=@Dimensions and @found=0
BEGIN

	SET @DIMENSIONTOCHECK=cast(SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) as int);
	SET @FOUND=(select isnull(computeAllValues,0) from Dimension where id=@DIMENSIONTOCHECK)	

	if @found=0
	BEGIN
		set @dIndex = @dNextIndex;
		set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);
		if @dNextIndex=0
		BEGIN
			SET @dNextIndex=len(@TABLENAME)
		END
		set @c=@c+1
	END

END

if @found=0
BEGIN
	return -1
END

return @c
END

--select * from dimension
