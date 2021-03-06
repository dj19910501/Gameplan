IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ComputeErrorBar') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION ComputeErrorBar
GO
CREATE Function [ComputeErrorBar] (@PopulationRate float, @NumberTests float, @ConfidenceLevel float, @NumberOfTails int)
returns float
as
BEGIN
	--Function will return the upper range of the confidence interval for the selected proportion
	if @ConfidenceLevel <= 0 or @ConfidenceLevel >=1
	BEGIN
		return null
	END
	if @NumberOfTails<>1 and @NumberOfTails<>2
	BEGIN
		return null
	END
	DECLARE @SD float = sqrt( @NumberTests*@PopulationRate*(1-@PopulationRate))
	DECLARE @Z float = dbo.normsinv((1-@ConfidenceLevel)/@NumberOfTails)

	return @NumberTests*@PopulationRate-(@Z*@SD)

END

GO
