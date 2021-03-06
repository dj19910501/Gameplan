IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[RunAggregation]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[RunAggregation]  
END
GO



CREATE PROCEDURE [dbo].[RunAggregation] @DEBUG bit = 0, @Partial bit =0, @ReturnStatus int = 0 output
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE StepCursor CURSOR FOR
	select stepName, LogStart, LogEnd, QueryText, StatusCode, PartialQueryText from AggregationSteps where isDeleted=0 order by StepOrder
	BEGIN TRY

	DECLARE @StepName varchar(max);
	DECLARE @LogStart bit;
	DECLARE @LogEnd bit;
	DECLARE @QueryText varchar(max);
	DECLARE @PartialQueryText varchar(max);
	DECLARE @StatusCode varchar(max);


	OPEN StepCursor
	FETCH NEXT FROM StepCursor INTO @StepName, @LogStart, @LogEnd, @QueryText, @StatusCode, @PartialQueryText
	WHILE @@FETCH_STATUS =0
	BEGIN

		if @StatusCode is null 
		BEGIN
			SET @StatusCode='RUNNING'
		END

		update AggregationStatus set StatusCode=@StatusCode

		if @LogStart=1 
		BEGIN
			insert into Logging select getDate(), @StepName + ' Start'
		END

		if @Partial=1 and @PartialQueryText is not null and len(@PartialQueryText) > 0
		BEGIN
			exec(@PartialQueryText)
		END
		ELSE
		BEGIN
			exec(@QueryText)
		END

		if @LogEnd=1
		BEGIN
			insert into Logging select getDate(), @StepName + ' End'
		END

		FETCH NEXT FROM StepCursor INTO @StepName, @LogStart, @LogEnd, @QueryText, @StatusCode, @PartialQueryText
	END

	CLOSE StepCursor
	DEALLOCATE StepCursor



	insert into Logging select GETDATE(), 'End Aggregation'
	SET @ReturnStatus = 1
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'SUCCESS','',GETDATE(),'bcg',null,null)
	END TRY
	BEGIN CATCH
		CLOSE StepCursor
		DEALLOCATE StepCursor
		SET @ReturnStatus = 0
		INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','RunAggregation',null)
	END CATCH;
	update AggregationStatus set StatusCode='NOTRUNNING'
	return @ReturnStatus
END

GO