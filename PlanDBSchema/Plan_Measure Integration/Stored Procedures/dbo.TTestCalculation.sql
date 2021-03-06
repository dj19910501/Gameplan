IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'TTestCalculation') AND xtype IN (N'P'))
    DROP PROCEDURE TTestCalculation
GO

CREATE PROCEDURE [TTestCalculation] 
(
@QueryToRun VARCHAR(MAX),
@Pvalue FLOAT OUTPUT
)
AS
BEGIN
	--Query passed in as value must return two fields (both floats).  Procedure will return null if data is degenerate.
	DECLARE @DataValues TABLE(d1 FLOAT, d2 FLOAT) 
	BEGIN TRY
		
		INSERT INTO @DataValues
		EXEC(@QueryToRun);
		
		DECLARE @Var1 FLOAT = (SELECT POWER( STDEV(d1),2) FROM @DataValues);
		DECLARE @Var2 FLOAT = (SELECT POWER( STDEV(d2),2) FROM @DataValues);
		DECLARE @Mu1 FLOAT = (SELECT AVG(d1) FROM @DataValues);
		DECLARE @Mu2 FLOAT = (SELECT AVG(d2) FROM @DataValues);
		DECLARE @Count1 FLOAT = (SELECT COUNT(*) FROM @DataValues WHERE d1 IS NOT NULL);
		DECLARE @Count2 FLOAT = (SELECT COUNT(*) FROM @DataValues WHERE d2 IS NOT NULL);

		DECLARE @TStat FLOAT = (@Mu1-@Mu2)/SQRT(@Var1/@Count1+@Var2/@Count2);
		DECLARE @DegsFree FLOAT = (POWER(@Var1/@Count1+@Var2/@Count2,2)/(POWER(@Var1,2)/(@Count1*@Count1*(@Count1-1))+POWER(@Var2,2)/(@Count2*@Count2*(@Count2-1))));
		DECLARE @VValue INT;

		IF @DegsFree < 100.5
		BEGIN
			SET @VValue=ROUND(@DegsFree,0);
		END

		IF @DegsFree>=100.5
		BEGIN
			SET @PValue=EXP(-1*POWER(@TStat,2)/2)/SQRT(2*PI());
		END
		ELSE
		BEGIN
			SET @PValue=(SELECT Probability FROM TValues WHERE DEGREES=@VValue AND ABS(Tstat-ABS(@Tstat)) IN (SELECT MIN(ABS(Tstat-ABS(@Tstat))) FROM TValues WHERE DEGREES=@VValue))
		END
		RETURN @PValue

	END TRY
	BEGIN CATCH
		RETURN NULL
	END CATCH
END;

GO
