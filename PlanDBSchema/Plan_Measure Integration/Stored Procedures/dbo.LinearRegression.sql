IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'LinearRegression') AND xtype IN (N'P'))
    DROP PROCEDURE LinearRegression
GO

CREATE procedure [LinearRegression] (@QueryToRun varchar(max))
as
BEGIN
	--Query passed in as value must return two fields (both floats) and no nulls for every row.  Procedure will return null if regression model is degenerate.
	DECLARE @values table(val1 float, val2 float)
	insert into @values
	exec(@QueryToRun)

	BEGIN try
		DECLARE @muxy float = (select avg(val1*val2) from @values)
		DECLARE @mux float = (select avg(val1) from @values)
		DECLARE @muy float = (select avg(val2) from @values)
		DECLARE @mux2 float = (select avg(val1*val1) from @values)
		DECLARE @Beta float = (@muxy - @mux*@muy)/(@mux2-@mux*@mux)
		DECLARE @Alpha float = @muy - @Beta*@mux
		DECLARE @SigmaError float = (select sqrt((STDEV(val2 - (@Beta*val1+ @Alpha))*STDEV(val2 - (@Beta*val1+ @Alpha)))/sum((val1-@mux)*(val1-@mux))) from @values)
		DECLARE @R2 float = (select (@Alpha * sum(val2) + @Beta * sum(val1*val2) - power(sum(val2),2) / count(*)) / (sum(val2*val2) - power(sum(val2),2) / count(*)) from @values)




		select @Alpha as Alpha, @Beta as Beta, @SigmaError as StandardError, @Beta-1.96025*@SigmaError as Lower95, @Beta+1.96025*@SigmaError as Upper95, @R2 as RSquared
	END TRY
	BEGIN CATCH
		select null
	END CATCH
END


GO
