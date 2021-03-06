IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ComputeMultipleRegression') AND xtype IN (N'P'))
    DROP PROCEDURE ComputeMultipleRegression
GO


CREATE PROCEDURE [ComputeMultipleRegression]  @ValuesQuery varchar(max), @NumX int
AS
BEGIN
SET NOCOUNT ON
--set @ValuesQuery='select top 40 LOS, ARCodingDays, SurgicalDRG, AdmissionSourceCode from _MedicalData'
--set @NumX=3

--print 'Start calc: ' + cast(GETDATE() as varchar)


set @NumX=@NumX+1

IF OBJECT_ID('tempdb..#RegressionData') IS NOT NULL
BEGIN 
     DROP TABLE #RegressionData
END 
IF OBJECT_ID('tempdb..#InverseMatrix') IS NOT NULL
BEGIN 
     DROP TABLE #InverseMatrix
END 


CREATE TABLE #RegressionData (id int identity(1,1), y int)
ALTER TABLE #RegressionData ADD  CONSTRAINT [PK_RegressionTable] PRIMARY KEY CLUSTERED
([ID] ASC)
CREATE INDEX IDX_RegressionDatay ON #RegressionData(y)

DECLARE @i int=2 -- We already have a constant as the first term
DECLARE @j int=1

while @i<=@NumX
BEGIN

	DECLARE @addString varchar(500);
	SET @addString='alter table #RegressionData add x' + cast(@i as varchar) + ' float CREATE INDEX IDX_RegressionDatax' + cast(@i as varchar) + ' ON #RegressionData(x' + cast(@i as varchar) + ')'
	exec(@addString)

	SET @i=@i+1
END

--print 'Begin regression insert: ' + cast(GETDATE() as varchar)

insert into #RegressionData
exec(@ValuesQuery)

alter table #RegressionData add x1 float
update #RegressionData set x1=1

DECLARE @SquareMatrix MatrixTable

DECLARE @rCount int = (select count(*) from #RegressionData)

--Let's multiply the matrix by its transpose
--for i, 1 =1 to numx

DECLARE @SquareMatrixQuery varchar(max)=''

--print 'Start square matrix query generation: ' + cast(GETDATE() as varchar)


set @i=1
while @i <= @NumX
BEGIN
	set @j=@i
	while @j <= @NumX
	BEGIN
		DECLARE @CurCellQuery varchar(max)=''
		DECLARE @CurCellQuery2 varchar(max)=''


		DECLARE @k int=1

		set @CurCellQuery= ' select ' + cast(@i as varchar) + ',' + cast(@j as varchar) + ', sum(x' + cast(@i as varchar) + ' * x' + cast(@j as varchar) + ') from #RegressionData '
		set @CurCellQuery2= ' UNION ALL select ' + cast(@j as varchar) + ',' + cast(@i as varchar) + ', sum(x' + cast(@i as varchar) + ' * x' + cast(@j as varchar) + ') from #RegressionData '
		

		if @i<>@j
		BEGIN
			SET @SquareMatrixQuery= @SquareMatrixQuery + @CurCellQuery + @CurCellQuery2
		END
		ELSE
		BEGIN
			SET @SquareMatrixQuery= @SquareMatrixQuery + @CurCellQuery
		END

		if @i<@NumX or @j<@NumX
		BEGIN
			SET @SquareMatrixQuery = @SquareMatrixQuery + ' UNION ALL '
		END


		set @j=@j+1
	END

	set @i=@i+1
END


--print 'Start square matrix insert: ' + cast(GETDATE() as varchar)
insert into @SquareMatrix
exec(@SquareMatrixQuery)

--Now we need to find the inverse of the square matrix

--First thing we need to do is find the determinant
DECLARE @Determinant float=0

DECLARE @dTable MatrixTable;

insert into @dTable
select * from @SquareMatrix

--print 'Start find determinant: ' + cast(GETDATE() as varchar)

SET @Determinant=(select dbo.FindDeterminant(@dTable))


CREATE TABLE #inverseMatrix (x int, y int, value float)

--print 'Start find inverse: ' + cast(GETDATE() as varchar)
insert into #inverseMatrix
select * from dbo.FindMatrixInverse(@dTable)


DECLARE @Betas MatrixTable

set @i=1
set @j=1

--print 'Start beta query generation: ' + cast(GETDATE() as varchar)
while @i<=@NumX
BEGIN
	while @j<=@rCount
	BEGIN
		DECLARE @BetaCellQuery varchar(max)='select ' + cast(@i as varchar) + ', ' + cast(@j as varchar) + ', '
		DECLARE @BetaJoin varchar(max)=' '
		set @k=1
		while @k<=@NumX
		BEGIN
			set @BetaCellQuery = @BetaCellQuery + ' i' + cast(@k as varchar) + '.value*r.x' + cast(@k as varchar)

			if @k<@NumX
			BEGIN
				set @BetaCellQuery = @BetaCellQuery + ' + '
			END

			set @BetaJoin= @BetaJoin + ' inner join #inverseMatrix i' + cast(@k as varchar) + ' on i' + cast(@k as varchar) + '.x=' + cast(@k as varchar) + ' and i' + cast(@k as varchar) + '.y=' + cast(@i as varchar) + ' '

			set @k=@k+1
		END

		set @BetaCellQuery = @BetaCellQuery + ' from #RegressionData r ' + @BetaJoin + ' where r.id=' + cast(@j as varchar)

		--print @BetaCellQuery

		insert into @Betas
		exec(@BetaCellQuery)

		set @j=@j+1
	END

	set @j=1
	set @i=@i+1
END


--We have found the betas.  Now we need to find the r^2

--Finding r^2 involves finding the correlation matrix and the y correlation vector.  Let's find the correlation matrix first

DECLARE @CorrelMatrix MatrixTable
--print 'Start R^2 calc: ' + cast(GETDATE() as varchar)

SET @i=2
while @i<=@NumX --Ignore the constant x for this
BEGIN
	SET @j=2
	while @j<=@NumX
	BEGIN
		if @i<>@j
		BEGIN

			DECLARE @toRun varchar(max)='select ' +  cast((@i-1) as varchar) + ', ' + cast((@j-1) as varchar) + ', x' + cast(@i as varchar) + ', x' + cast(@j as varchar) + ' from #RegressionData'
			insert into @CorrelMatrix
			exec CorrelationCalculation @QueryToRun=@toRun

		END
		ELSE
		BEGIN
			insert into @CorrelMatrix
			select (@i-1),( @j-1), 1
		END

	
		SET @j=@j+1
	END

	SET @i=@i+1
END

--Now we need to setup the c vector
--print 'Start c vector calc: ' + cast(GETDATE() as varchar)

DECLARE @CMatrix MatrixTable
SET @i=2
while @i<=@NumX
BEGIN
	SET @toRun ='select ' +  cast((@i-1) as varchar) + ', 1, x' + cast(@i as varchar) + ', y from #RegressionData'

	insert into @CMatrix
	exec CorrelationCalculation @QueryToRun=@toRun

	SET @i=@i+1
END

--Now we have the c vector, we need to multiply it by the inverse of the correlation matrix
DECLARE @invCorrelMatrix MatrixTable

--print 'Start inverse correl calc: ' + cast(GETDATE() as varchar)
insert into @invCorrelMatrix
select * from dbo.FindMatrixInverse(@CorrelMatrix)

DECLARE @cProductMatrix MatrixTable

--print 'Start c product calc: ' + cast(GETDATE() as varchar)
SET @i=1
while @i<@NumX
BEGIN
	insert into @cProductMatrix
	select @i, 1, sum(cm.Value*c.Value) from @CMatrix c 
	inner join @invCorrelMatrix cm on c.x=cm.y
	where cm.x=@i

	SET @i=@i+1
END

--Now we need to finish the R^2 calculation

DECLARE @R2 float = (select sum(v.Value*c.Value) from @CMatrix c inner join @cProductMatrix v on v.x=c.x)

DECLARE @BetaVector MatrixTable

--print 'Start beta vector calc: ' + cast(GETDATE() as varchar)

insert into @BetaVector
select 1, b.x, sum(b.Value*r.y) as Beta from @Betas b
inner join #RegressionData r on b.y=r.id
group by b.x





--We've Found R^2, now we need to find the standard error

ALTER TABLE #RegressionData add Residual float


DECLARE @residuals MatrixTable

DECLARE @ResidualQuery varchar(max)='update #RegressionData set Residual= y - '

--print 'Start residuals calc: ' + cast(GETDATE() as varchar)
insert into @residuals
select 1, id, y from #RegressionData

set @i=1
while @i<=@NumX
BEGIN
	DECLARE @curBeta float = (select Value from @BetaVector where y=@i)

	SET @ResidualQuery = @ResidualQuery + ' x'+ cast(@i as varchar) + '*' + cast(@curBeta as varchar) + ' '

	if @i<@NumX
	BEGIN
		SET @ResidualQuery= @ResidualQuery + ' - '
	END

	SET @i=@i+1
END

exec(@ResidualQuery)


--We've found the residuals, now we need to find the standard error

--print 'Start standard error calc: ' + cast(GETDATE() as varchar)
DECLARE @StdErr float=0
DECLARE @divisor int=(select count(*) from #RegressionData)-@Numx

SET @StdErr = (select sqrt(sum(Residual*Residual)/@divisor) from #RegressionData)


--Now we need to find the standard errors for each of the x terms

DECLARE @Errors MatrixTable
--print 'Start beta errors calc: ' + cast(GETDATE() as varchar)

insert into @Errors
select 1, i.y, sqrt(@StdErr*@StdErr*i.Value) from #inverseMatrix i inner join @BetaVector b on b.y=i.y where i.x=i.y

--select * from @BetaVector
--select * from @Errors

DECLARE @RetTable table (id int identity(1,1), Beta float, RSquared float, StdError float, BetaError float, BetaUpper Float, BetaLower float)

--print 'Start final calc: ' + cast(GETDATE() as varchar)
insert into @RetTable(Beta)
select sum(b.Value*r.y) as Beta from @Betas b
inner join #RegressionData r on b.y=r.id
group by b.x

update @RetTable set RSquared = @R2 where id=1
update @RetTable set StdError = @StdErr where id=1

update @RetTable set BetaError = Value from @Errors where id=y

update @RetTable set BetaUpper = Beta + 1.96*BetaError
update @RetTable set BetaLower = Beta - 1.96*BetaError

select * from @RetTable


drop table #RegressionData
drop table #inverseMatrix
--print 'End calc: ' + cast(GETDATE() as varchar)

END


GO
