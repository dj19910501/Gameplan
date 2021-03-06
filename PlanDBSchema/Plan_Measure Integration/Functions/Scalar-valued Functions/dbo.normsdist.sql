IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'normsdist') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION normsdist
GO

CREATE Function [normsdist] (@x float)
returns float
as
BEGIN
	--This function approximates what is returned by the normsdist function in Excel.  It will not be exactly the same, but close enough for our purposes.
	if @x < -10
		BEGIN
		return 0
		END

	if @x > 10
		BEGIN
		return 1
		END


	--Using the 1945 Polya approximation formula as it is good enough for our purposes
	if @X < 0
		BEGIN
		RETURN 1- 0.5*(1+sqrt((1-exp(-1*sqrt(pi()/8)*power(@x,2)))))
		END

	return 0.5*(1+sqrt((1-exp(-1*sqrt(pi()/8)*power(@x,2)))))
END


GO
