-- =============================================
-- Author:Mitesh Vaishnav
-- Create date:12/08/2016
-- Description:	pull actuals from Measure database
-- =============================================
ALTER PROCEDURE [INT].[PullMeasureSFDCActual]
	(
	@ClientId nvarchar(36),
	@AuthDatabaseName Nvarchar(1000)
	)
AS
BEGIN
DECLARE @MeasureSFDCActualTableName nvarchar(255)
DECLARE @CustomQuery NVARCHAR(MAX)
DECLARE @tempDbName TABLE
	(
		DbName NVARCHAR(100)
	)
	INSERT INTO @tempDbName
	EXEC [INT].[CreateMeasureSFDCActualTable] @ClientId=@ClientId,@AuthDatabaseName=@AuthDatabaseName

	SELECT @MeasureSFDCActualTableName=DbName from @tempDbName

		--START:- Pulling CW
		EXEC [INT].[PullCw] @MeasureSFDCActualTableName
		--END:- Pulling CW

		--START:- Pulling Responses
		BEGIN
			EXEC [INT].[PullResponses] @MeasureSFDCActualTableName
		END
		--END:- Pulling Responses

		--START:- Pulling MQL
		BEGIN
			     EXEC [INT].[PullMQL] @MeasureSFDCActualTableName,@ClientId 
		END
		--END:- Pulling MQL

		--Remove SFDC table which will created from Measure database function
		EXEC [INT].[RemoveMeasureSFDCActualTable] @MeasureSFDCActualTableName=@MeasureSFDCActualTableName
END


