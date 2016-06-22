IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'AttributionCalculation') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[AttributionCalculation]
END
GO

CREATE PROCEDURE [dbo].[AttributionCalculation]

 @TouchTableName NVARCHAR(255)= '_WaterFall',
 @OpportunityFieldname  NVARCHAR(255)= 'OpportunityId',
 @TouchDateFieldName NVARCHAR(255)= 'ActivityDate',
 @OpportunityTableName NVARCHAR(255)= '_Opportunities',
 @OpportunityRevenue NVARCHAR(255) ='Amount', --
 @OpportunityCloseDate NVARCHAR(255)= 'CloseDate',
 @TouchWhereClause NVARCHAR(MAX)= 'SELECT ID FROM _Waterfall WHERE ID IN (510074,333900,347622,510091,719583,140549,140567,333880,347568,716203,717655,717825) ',
 @AttributionType INT = 5,  /*  @AttributionType 1 - First Touch,  2 - Last Touch, 3 - Evenly Distributed,  4 - Time Decay,  5 - Position,  6 - Interaction, 7 - W Shaped  */
 @MaximumDays INT=400,
 @HalfLife INT=0--Need to write 0 here for linear method 

AS
BEGIN
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TouchTableName) AND 
   EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @OpportunityTableName) AND 
   EXISTS (SELECT * FROM sys.columns WHERE Name = @TouchDateFieldName AND Object_ID = Object_ID(@TouchTableName)) AND 
   EXISTS (SELECT * FROM sys.columns WHERE Name = @OpportunityFieldname AND Object_ID = Object_ID(@OpportunityTableName))
BEGIN
	BEGIN TRY
		DECLARE @BaseQuery NVARCHAR(MAX)='',
				@WhereQuery NVARCHAR(MAX)='',
				@Attribution NVARCHAR(50)='',
				@TempOpportunityRevenue NVARCHAR(50)='',
				@Revenue NVARCHAR(50)='Revenue',
				@IndexNo NVARCHAR(50)='IndexNo',
				@index NVARCHAR(2000) = '',
				@ClosingQuery NVARCHAR(1000)=') DatTable ',
				@IndexWhere Nvarchar(500)='',
				@DenseRank NVARCHAR(2000) = '',
				@DenseRankNo NVARCHAR(50)='idDenseRank'

		--Use For Evenly Weighted Attribution
		DECLARE @TmpTbl TABLE (OpportunityId NVARCHAR(MAX), ActivityDate NVARCHAR(MAX), Revenue FLOAT, IndexNo INT, CampaignId NVARCHAR(MAX))

		IF(@AttributionType = 1) 
		BEGIN
			SET @Attribution = 'ASC'
		END  
		ELSE IF(@AttributionType = 2)
		BEGIN
			SET @Attribution = 'DESC'   
		END
		ELSE IF(@AttributionType = 3)
		BEGIN 
			SET @Attribution = ''
		END
		
		SET @TempOpportunityRevenue = @OpportunityRevenue

		IF(@AttributionType = 5 OR @AttributionType = 7)
		BEGIN
			SET @DenseRank = 'DENSE_RANK() OVER (PARTITION BY objOpportunities.' + @OpportunityFieldname + ' ORDER BY objOpportunities.' + @OpportunityFieldname + ', ' + @TouchDateFieldName + ' ASC ) AS '+ @DenseRankNo + ', '	
		END
		ELSE IF(@AttributionType = 6)
		BEGIN
			SET @DenseRank = 'DENSE_RANK() OVER (ORDER BY objOpportunities.' + @OpportunityFieldname + ') AS ' + @DenseRankNo + ', '
		END

		SET @index = ',ROW_NUMBER() OVER (PARTITION BY objOpportunities.' + @OpportunityFieldname + ' ORDER BY ['+@TouchDateFieldName+'] ' + @Attribution + ') AS ' + @IndexNo

		SET @TempOpportunityRevenue = @TempOpportunityRevenue + ' AS ' + @Revenue

		IF(@AttributionType = 1 OR @AttributionType = 2)
		BEGIN
			--Table Join
			SET @BaseQuery='SELECT '+@OpportunityFieldname+','

			IF(@AttributionType=3)
			BEGIN
				SET @BaseQuery= @BaseQuery + '(' + @Revenue + ' / MAX(' + @IndexNo + ')' + ') AS ' + @Revenue
			END
			ELSE
			BEGIN
				SET @BaseQuery= @BaseQuery + @TouchDateFieldName+','+@Revenue
			END

			SET @BaseQuery= @BaseQuery + ',CampaignId FROM ('
		END
	
		SET @BaseQuery= @BaseQuery + 'SELECT '

		IF(@AttributionType = 5 OR @AttributionType = 6 OR @AttributionType = 7)
		BEGIN
			SET @BaseQuery= @BaseQuery + @DenseRank
		END
		DECLARE @TimeDecayFields NVARCHAR(1000)
		SET @TimeDecayFields='';

		IF(@AttributionType=4)
		SET @TimeDecayFields=','+@OpportunityCloseDate+',DATEDIFF(day,'+@TouchDateFieldName+','+@OpportunityCloseDate+'),0,0'

			--Updattion start 16/06/2016  Kausha #838 Revenue Attribution - Graphs
		--SET @BaseQuery= @BaseQuery + 'objOpportunities.'+@OpportunityFieldname+','+@TouchDateFieldName+','+@TempOpportunityRevenue+''+@index+',objOpportunities.CampaignId '+@TimeDecayFields+' FROM '+@TouchTableName +' objTouches'
		SET @BaseQuery= @BaseQuery + 'objOpportunities.'+@OpportunityFieldname+','+@TouchDateFieldName+','+@TempOpportunityRevenue+''+@index+',objTouches.CampaignId '+@TimeDecayFields+' FROM '+@TouchTableName +' objTouches'
		--Updattion End 16/06/2016  Kausha #838 Revenue Attribution - Graphs

		IF(LEN(@OpportunityTableName) > 0)
		BEGIN
			SET @BaseQuery=@BaseQuery+ ' Inner join '+@OpportunityTableName+' objOpportunities ON objOpportunities.'+@OpportunityFieldname+' =objTouches.'+@OpportunityFieldname+' '
		END
		--Touch Where Clause
		IF(LEN(@TouchWhereClause) > 0)
		BEGIN
			SET @WhereQuery=' WHERE  objTouches.Id IN('+@TouchWhereClause+') AND '
		END
		ELSE
		BEGIN
			SET @WhereQuery= @WhereQuery+' WHERE '
		END
		--Opportunity Close Date Where Clause 
		SET @WhereQuery=@WhereQuery+'objOpportunities.'+@OpportunityCloseDate+'<=GETDATE()'
		
		SET @IndexWhere = ' WHERE ' + @IndexNo + ' = 1'

		SET @WhereQuery=@WhereQuery + ' AND '+@TouchDateFieldName+' < '+@OpportunityCloseDate
		IF (@AttributionType = 1 OR @AttributionType = 2)
		BEGIN
			EXEC(@BaseQuery+@WhereQuery+@CLosingQuery+@IndexWhere)
		END
		ELSE IF(@AttributionType=3)
		BEGIN
			INSERT INTO @TmpTbl
			EXEC (@BaseQuery+@WhereQuery)
			Update A set A.IndexNo = B.IndNo FROM @TmpTbl A INNER JOIN (
			SELECT OpportunityId, MAX(IndexNo) AS IndNo FROM @TmpTbl A GROUP BY OpportunityId) B ON A.OpportunityId = B.OpportunityId

			SELECT OpportunityId, ActivityDate, (Revenue / IndexNo) AS Revenue , CampaignId FROM @TmpTbl
		END
		ELSE IF(@AttributionType = 4) 
		BEGIN
			 IF( LEN(@HalfLife) IS NULL)
			 SET @HalfLife=0
			  --Half life and maximum days can not less then zero
		 		 IF(@HalfLife<0)
				 BEGIN
				 PRINT 'Half life can not be nagative.'
				 RETURN
				 END
			 IF(LEN(@MaximumDays) IS NULL OR @MaximumDays<=0)
				 BEGIN
				 PRINT 'Maximum Days must be grater then 0.  '
				 RETURN
				 END
				 ELSE
				 BEGIN
					DECLARE @FinalQuery NVARCHAR(MAX)=@BaseQuery+@WhereQuery
					EXEC AttributionCalculation_TimeDecay @TouchTableName,@OpportunityFieldname,@TouchDateFieldName,@OpportunityTableName,@OpportunityRevenue,@OpportunityCloseDate,@TouchWhereClause,@AttributionType,@MaximumDays,@HalfLife,@FinalQuery
				 END
		END  
		ELSE IF(@AttributionType = 5 OR @AttributionType = 7)
		BEGIN
			IF EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE XValue IS NOT NULL AND YValue IS NOT NULL)
			BEGIN
				EXEC Attribution_PositionBased @BaseQuery, @WhereQuery, @TouchTableName, @AttributionType
			END
			ELSE
			BEGIN
				PRINT 'Please enter valid configuration in AttrPositionConfig Table.'
			END
		END
		ELSE IF(@AttributionType = 6)
		BEGIN
			IF EXISTS(SELECT TOP 1 * FROM AttrPositionConfig WHERE [Weight] IS NOT NULL)
			BEGIN
				EXEC InteractionBasedAttrCalc @BaseQuery, @WhereQuery, @TouchTableName
			END
			ELSE
			BEGIN
				PRINT 'Please enter valid configuration in AttrPositionConfig Table.'
			END
		END
	
	END TRY
	BEGIN CATCH
		PRINT ERROR_MESSAGE();
	END CATCH
END
ELSE
BEGIN
	PRINT 'Table name / column names passed as parameter are not exit in the database, please check and pass the proper parameters.'
END	

END


GO

