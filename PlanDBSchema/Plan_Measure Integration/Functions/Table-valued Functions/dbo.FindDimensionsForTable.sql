IF OBJECT_ID(N'dbo.FindDimensionsForTable', N'TF') IS NOT NULL
    DROP FUNCTION dbo.FindDimensionsForTable ;
GO

CREATE FUNCTION dbo.FindDimensionsForTable (@tablename nvarchar(max), @Dimensions int)
RETURNS @retTable TABLE
(
	id int identity(1,1),
	DimensionValue int
)
AS
BEGIN
DECLARE @DIMENSIONTOCHECK int
DECLARE @dIndex int=1
DECLARE @dNextIndex int=CHARINDEX('d',@TABLENAME,@dIndex+1)

DECLARE @c int =1

while @c<=@Dimensions 
BEGIN

	SET @DIMENSIONTOCHECK=cast(SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) as int);

	insert into @retTable
	select @DIMENSIONTOCHECK

	set @dIndex = @dNextIndex;
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);
	--Added for #711
	if @dNextIndex=0
	BEGIN
		SET @dNextIndex=len(@TABLENAME)+1
	END
	set @c=@c+1
END

return 
END

GO