IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[AttributionCalculation_TimeDecay]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[AttributionCalculation_TimeDecay]  
END
GO

CREATE PROCEDURE [dbo].[AttributionCalculation_TimeDecay]

 @TouchTableName Nvarchar(255),
 @OpportunityFieldname  Nvarchar(255),
 @TouchDateFieldName Nvarchar(255),
 @OpportunityTableName Nvarchar(255),
 @OpportunityRevenue Nvarchar(255) , --
 @OpportunityCloseDate Nvarchar(255),
 @TouchWhereClause NVARCHAR(MAX),
 @AttributionType int,  /*  @AttributionType 1 - First Touch,  2 - Last Touch, 3 - Evenly Distributed,  4 - Time Decay,  5 - Position,  6 - Interaction, 7 - W Shaped  */
 @MaximumDays INT,
 @HalfLife INT,--Need to write 0 here for linear method 
 @Query NVARCHAR(MAX)

 AS
BEGIN


BEGIN TRY

--CREATE temp table and insert base query data in to it for further calculation
DECLARE @tempTable NVARCHAR(2000)='Declare @temptable Table('+@opportunityfieldname+' NVARCHAR(500),'+@TouchDateFieldName+' Date,Amount float,[Index] INT,CampaignID NVARCHAR(500),
'+@OpportunityCloseDate+' Date, [daysDifference] Float,[Weight] DECIMAL(18,2),Revenue Float ) INSERT INTO @temptable '


DECLARE @tempQuery NVARCHAR(MAX)=''

--CURSOR for Linear and half life method
SET @tempQuery='
DECLARE @OpportunityId NVARCHAR(500)
DECLARE @Amount FLOAT
DECLARE @Index INT

--------------------------------------------------------
DECLARE @WeightageCursor CURSOR
SET @WeightageCursor = CURSOR FAST_FORWARD
FOR
SELECT  '+@OpportunityFieldname+','+@OpportunityRevenue+',[INDEX] from @temptable

OPEN @WeightageCursor
FETCH NEXT FROM @WeightageCursor 
INTO @OpportunityId,@Amount,@Index
WHILE @@FETCH_STATUS = 0
BEGIN
Declare @Sum Float
IF(@Amount!=0)
BEGIN
IF('+CAST(@HalfLife AS NVARCHAR(50))+' =0)
BEGIN
SET @Sum=(SELECT SUM(('+(CAST(@MaximumDays AS NVARCHAR(50)))+'-daysDifference)) From @temptable  where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@OpportunityId)
IF(@Sum>0)
BEGIN
UPDATE @temptable SET Weight=(@Amount/@Sum) WHERE '+@OpportunityFieldname+'=@OpportunityId
UPDATE @temptable SET Revenue=Weight*('+CAST(@MaximumDays AS NVARCHAR)+'-daysDifference) WHERE '+@OpportunityFieldname+'=@OpportunityId
END
END
ELSE
BEGIN

UPDATE @temptable SET weight=('+CAST(@MaximumDays AS NVARCHAR)+'/(POWER(2.00,daysdifference/'+CAST(@HalfLife AS NVARCHAR(50))+'))) where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@OpportunityId AND [INDEX]=@Index
END

END

 
FETCH NEXT FROM @WeightageCursor
INTO @OpportunityId,@Amount,@Index
END
CLOSE @WeightageCursor
DEALLOCATE @WeightageCursor 

IF('+CAST(@HalfLife AS NVARCHAR(50))+'!=0)
BEGIN
DECLARE @InnerOpportunityId NVARCHAR(500)
DECLARE @InnerAmount FLOAT
DECLARE @InnerIndex INt

DECLARE @WeightageInnerCursor CURSOR
SET @WeightageInnerCursor = CURSOR FAST_FORWARD
FOR
SELECT  OpportunityId,Amount,[INDEX] from @temptable


OPEN @WeightageInnerCursor
FETCH NEXT FROM @WeightageInnerCursor
INTO @InnerOpportunityId,@InnerAmount,@InnerIndex
WHILE @@FETCH_STATUS = 0
BEGIN
IF(@InnerAmount!=0)
BEGIN
SET @Sum=(SELECT SUM(weight) From @temptable  where [daysDifference]<='+CAST(@MaximumDays AS NVARCHAR(50))+' AND '+@OpportunityFieldname+'=@InnerOpportunityId)
IF(@Sum>0)
BEGIN
UPDATE @temptable SET Revenue=(Weight*Amount/@SUM) WHERE '+@OpportunityFieldname+'=@InnerOpportunityId AND [INDEX]=@InnerIndex
END
END

 
FETCH NEXT FROM @WeightageInnerCursor
INTO @InnerOpportunityId,@InnerAmount,@InnerIndex
END
CLOSE @WeightageInnerCursor
DEALLOCATE @WeightageInnerCursor 
END
'

DECLARE @SelectQuery NVARCHAR(2000)='SELECT '+@OpportunityFieldname+','+@TouchDateFieldName+','+'Revenue,CampaignID FROM @temptable WHERE DaysDifference<= '+CAST(@MaximumDays AS NVARCHAR)
EXEC(@tempTable+ @Query+@tempQuery+@selectquery)

END TRY
BEGIN CATCH
		PRINT ERROR_MESSAGE();
	END CATCH


END
GO