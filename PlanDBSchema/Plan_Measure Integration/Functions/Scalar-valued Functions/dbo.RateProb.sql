IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RateProb') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION RateProb
GO


CREATE Function [RateProb] (@PopulationRate float, @NumberTests float, @TestRate float)
returns float
as
BEGIN
	--Function will return the probability that the number of tests would be at least the number passed in.  Returns null in case of degenerate data
	if @PopulationRate <= 0 or @PopulationRate >=1
	BEGIN
		return null
	END
	DECLARE @SD float = sqrt( @NumberTests*@PopulationRate*(1-@PopulationRate))
	DECLARE @Z float = (@NumberTests*@TestRate-@NumberTests*@PopulationRate)/@SD

	return dbo.normsdist(@Z)

END

GO
