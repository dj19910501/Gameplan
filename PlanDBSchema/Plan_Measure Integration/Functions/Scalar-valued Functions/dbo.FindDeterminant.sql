IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'FindDeterminant') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION FindDeterminant
GO

CREATE Function [FindDeterminant] (
	-- Add the parameters for the stored procedure here
	@ToFind MatrixTable READONLY)
RETURNS FLOAT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @mSize int = (select max(x) from @ToFind)

	DECLARE @retval float=0
	if @mSize=2
	BEGIN

		return (select r1.Value*r2.Value-r3.Value*r4.Value from @ToFind r1
		inner join @ToFind r2 on r2.x=2 and r2.y=2
		inner join @ToFind r3 on r3.x=2 and r3.y=1
		inner join @ToFind r4 on r4.x=1 and r4.y=2
		where r1.x=1 and r1.y=1);
	END

	DECLARE @x int=1
	DECLARE @y int

	while @x<=@mSize
	BEGIN
		SET @y=1
		DECLARE @subD MatrixTable
		delete from @subD

		insert into @subD
		select case when x < @x then x else x-1 end, case when y < @y then y else y-1 end, value
		from @ToFind
		where x<>@x and y<>@y




		if (@x+@y)%2=0
		BEGIN
			set @retval = @retval + (select dbo.FindDeterminant (@subD))*(select Value from @ToFind where x=@x and y=@y)
		END
		ELSE
		BEGIN
			set @retval = @retval - (select dbo.FindDeterminant (@subD))*(select Value from @ToFind where x=@x and y=@y)
		END

		SET @x=@x+1
	END

	return @retval;

END


GO
