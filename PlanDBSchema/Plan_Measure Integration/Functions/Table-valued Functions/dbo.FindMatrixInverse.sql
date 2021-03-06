IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'FindMatrixInverse') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION FindMatrixInverse
GO
CREATE FUNCTION [FindMatrixInverse]
( @ToFind MatrixTable READONLY
)
RETURNS 
@adjugateMatrix table (x int, y int, value float)
AS
BEGIN

	
	insert into @adjugateMatrix
	select * from @ToFind

	DECLARE @Determinant float = (select dbo.FindDeterminant(@toFind))

	DECLARE @i int
	DECLARE @j int
	DECLARE @NumX int =(select max(x) from @adjugateMatrix)

	update @adjugateMatrix set Value=0

	set @i=1
	while @i<=@NumX
	BEGIN
		set @j=1

		while @j<=@NumX
		BEGIN
			DECLARE @ValToAdd float=1

			if (@i+@j)%2=1
			BEGIN
				SET @ValToAdd=-1
			END

			DECLARE @cofactor MatrixTable
			delete from @cofactor

			insert into @cofactor
			select case when x < @i then x else x-1 end, case when y < @j then y else y-1 end, value
			from @ToFind
			where x<>@i and y<>@j

			set @ValToAdd=@ValToAdd*(select dbo.FindDeterminant(@cofactor))



			--print 'update #AdjugateMatrix set value=value - ' + cast(@finalVal as varchar) + ' where x=' + cast(@i as varchar) + ' and y=' + cast(@j as varchar)
			update @adjugateMatrix set value=@ValToAdd where x=@i and y=@j


			set @j=@j+1
		END

		set @i=@i+1
	END

	update @adjugateMatrix set value=value/@Determinant	
	
	RETURN 
END


GO
