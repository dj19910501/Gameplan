-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp will create temp table for MEasure SFDC actuals data 
-- =============================================
ALTER PROCEDURE [INT].[CreateMeasureSFDCActualTable]
(
@ClientId nvarchar(36),
@AuthDatabaseName Nvarchar(1000),
@SFDCActualTempTable  NVARCHAR(1000) OUTPUT
)
AS
BEGIN
	DECLARE @NewSFDCActualTableName NVARCHAR(255)
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @DataBaseName NVARCHAR(100)
	DECLARE @tempDbName TABLE
	(
		DbName NVARCHAR(100)
	)
	INSERT INTO @tempDbName
	EXEC [INT].[GETMeasureClientDbName] @ClientId=@ClientId,@AuthDatabaseName=@AuthDatabaseName
	
	SELECT @DataBaseName= DbName from @tempDbName

	IF (@DataBaseName <> '' AND @DataBaseName IS NOT NULL)
	BEGIN
		SET @NewSFDCActualTableName='MeasureSFDC_'
									+CONVERT(NVARCHAR(4), DATEPART(YEAR,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(MONTH,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(DAY,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(HOUR,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(MINUTE,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(SECOND,GETDATE()))+'_'
									+CONVERT(NVARCHAR(10), DATEPART(MILLISECOND,GETDATE()))

		SET @CustomQuery='CREATE TABLE '+@NewSFDCActualTableName+'
								(
								IntegrationType NVARCHAR(20),
								StageTitle NVARCHAR(20),
								Period NVARCHAR(10),
								ActualValue FLOAT,
								Unit VARCHAR(10),
								PusheeID NVARCHAR(255),
								PulleeID NVARCHAR(255),
								ModifiedDate DateTime
								)
				INSERT INTO ['+@NewSFDCActualTableName+' ]
				SELECT * FROM '+@DataBaseName+'.[INT].[GetTacticActuals](GETDATE(),GETDATE()-1);'
		
		--EXEC (@CustomQuery)
		SELECT @SFDCActualTempTable=@NewSFDCActualTableName
	END
END


