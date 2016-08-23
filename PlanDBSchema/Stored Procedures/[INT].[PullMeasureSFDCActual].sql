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

--set integration instance id which have type as "Measure Actual"
DECLARE @IntegrationInstanceId INT=0
DECLARE @IntegrationInstanceUserId NVARCHAR(36)=''
DECLARE @SectionName NVARCHAR(1000)=''
DECLARE @IntegrationInstanceLogId INT=0 -- set output perameter in this veriable of add log function

	SELECT TOP 1 @IntegrationInstanceId=I.IntegrationInstanceId
				,@IntegrationInstanceUserId=I.CreatedBy
	FROM IntegrationInstance I INNER JOIN IntegrationType It ON It.IntegrationTypeId=I.IntegrationTypeId AND It.Code='MA'
	WHERE ClientId=@ClientId
	--Add initial instance log
	EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId,@UserID=@IntegrationInstanceUserId,@IntegrationInstanceLogId=@IntegrationInstanceLogId,@OutPutLogid=@IntegrationInstanceLogId OUTPUT


BEGIN TRY

DECLARE @MeasureSFDCActualTableName nvarchar(255)
DECLARE @CustomQuery NVARCHAR(MAX)
DECLARE @IntegrationInstanceSectionLogId INT=0
DECLARE @tempDbName TABLE
	(
		DbName NVARCHAR(100)
	)

	--Fetch name of the temp table at where SFDC data inserted in plan database
	SET @SectionName='Create Measure Actuals Temp Table'
	EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

			--INSERT INTO @tempDbName
			EXEC [INT].[CreateMeasureSFDCActualTable] @ClientId=@ClientId,@AuthDatabaseName=@AuthDatabaseName,@SFDCActualTempTable=@MeasureSFDCActualTableName OUTPUT

			--SELECT @MeasureSFDCActualTableName=DbName from @tempDbName

		IF (@MeasureSFDCActualTableName IS NOT NULL)
		BEGIN
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Success'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT


		--START:- Pulling CW
		SET @SectionName='Pulling CW'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

		EXEC [INT].[PullCw] @MeasureSFDCActualTableName

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Success'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
												
		--END:- Pulling CW

		--START:- Pulling Responses
		BEGIN
		SET @SectionName='Pulling Responses'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

		EXEC [INT].[PullResponses] @MeasureSFDCActualTableName

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId
										,@IntegrationInstanceLogId=@IntegrationInstanceLogId
										,@sectionName=@SectionName
										,@Status='Success'
										,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END
		--END:- Pulling Responses

		--START:- Pulling MQL
		BEGIN
		SET @SectionName='Pulling MQL'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
			     EXEC [INT].[PullMQL] @MeasureSFDCActualTableName,@ClientId 

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId
										,@IntegrationInstanceLogId=@IntegrationInstanceLogId
										,@sectionName=@SectionName
										,@Status='Success'
										,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END
		--END:- Pulling MQL

		--Remove SFDC table which will created from Measure database function
		EXEC [INT].[RemoveMeasureSFDCActualTable] @MeasureSFDCActualTableName=@MeasureSFDCActualTableName
		END
	ELSE
		BEGIN
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Error'
												,@ErrorDescription='Measure Sfdc Actual table not created.'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END

		EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId,
										@IntegrationInstanceLogId=@IntegrationInstanceLogId,
										@Status='Success',
										@OutPutLogid=@IntegrationInstanceLogId OUTPUT
END TRY
BEGIN CATCH
DECLARE @ErrorMsg NVARCHAR(MAX)
SELECT  @ErrorMsg=ERROR_MESSAGE()
EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Error'
												,@ErrorDescription=@ErrorMsg
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId,
										@IntegrationInstanceLogId=@IntegrationInstanceLogId,
										@Status='Error',
										@ErrorDescription=@ErrorMsg,
										@OutPutLogid=@IntegrationInstanceLogId OUTPUT
END CATCH
END




