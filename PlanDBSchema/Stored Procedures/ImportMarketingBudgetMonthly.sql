

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportMarketingBudgetMonthly]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ImportMarketingBudgetMonthly]
GO

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
SET ANSI_NULLS ON
GO


CREATE  PROCEDURE [dbo].[ImportPlanActualDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Actualvalue as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Actual as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId and LOWER(a.StageTitle)='cost'
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

--end tactic
UNION
--start line item
select ActivityId,[Task Name],'LineItem' as ActivityType,0 as Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Actual as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData

--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData



IF ( LOWER(@Type)='tactic')
	
	BEGIN
    
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId and [Status] IN('In-Progress','Complete','Approved') )
			BEGIN

			IF NOT EXISTS(Select * from Plan_Campaign_Program_Tactic_LineItem where PlanTacticId =  @EntityId  AND IsDeleted=0 and LineItemTypeId IS NOT NULL)
			BEGIN			
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JAN 
			      from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' and StageTitle='Cost'
				END
				  ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Actual VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
					
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y2' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.FEB 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2' and StageTitle='Cost'
				END
				ELSE
					BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		 
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y3' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.MAR 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3' and StageTitle='Cost'
				END
			ELSE
					BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.APR 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' and StageTitle='Cost'
				END
					ELSE
					BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y5' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.MAY 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5' and StageTitle='Cost'
				END

				ELSE
					BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y6' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JUN 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6' and StageTitle='Cost'
				END
			ELSE
					BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.JUL 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' and StageTitle='Cost'
				END
		ELSE
					BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y8' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.AUG 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8' and StageTitle='Cost'
				END
				ELSE
					BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  --  ELSE
	
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y9' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.SEP 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9' and StageTitle='Cost'
				END
		ELSE
					BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END


			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.OCT 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' and StageTitle='Cost'
				END
	

	ELSE
					BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y11' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.NOV 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11' and StageTitle='Cost'
				END
	
					ELSE
					BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Actual WHERE PlanTacticId = @EntityId AND Period = 'Y12' and StageTitle='Cost')
				BEGIN
					UPDATE P SET P.Actualvalue = T.DEC 
			from Plan_Campaign_Program_Tactic_Actual P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12' and StageTitle='Cost'
				END

					ELSE
					BEGIN
					IF ((SELECT [DEC] from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Actual (PlanTacticId,StageTitle,Period,Actualvalue,CreatedDate,CreatedBy) VALUES (@EntityId,'Cost','Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		END
		
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

	END
	

IF ( LOWER(@Type)='lineitem')
		BEGIN
			IF Exists (select top 1 PlanLineItemId from [Plan_Campaign_Program_Tactic_LineItem] where PlanLineItemId =  @EntityId)
			BEGIN		

			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			--Y1
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
				BEGIN
						UPDATE P SET P.Value = T.JAN 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
				END
				ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

	
             --Y2
			 	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y2')
				BEGIN
						UPDATE P SET P.Value = T.FEB 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y2'
				END
				ELSE
				BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
			---Y3
				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y3')
				BEGIN
						UPDATE P SET P.Value = T.MAR 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y3'
				END
				ELSE
				BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

----Y4

	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
				BEGIN
						UPDATE P SET P.Value = T.APR 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
				END
				ELSE
				BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
--Y5
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y5')
				BEGIN
						UPDATE P SET P.Value = T.MAY 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y5'
				END
				ELSE
				BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		

---Y6
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y6')
				BEGIN
						UPDATE P SET P.Value = T.JUN 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y6'
				END
				ELSE
				BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

---y7
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
				BEGIN
						UPDATE P SET P.Value = T.JUL 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
				END
				ELSE
				BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
--Y8
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y8')
				BEGIN
						UPDATE P SET P.Value = T.AUG 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y8'
				END
				ELSE
				BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y9
	IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y9')
				BEGIN
						UPDATE P SET P.Value = T.SEP 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y9'
				END
				ELSE
				BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
				--Y10
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
				BEGIN
						UPDATE P SET P.Value = T.OCT 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
				END
				ELSE
				BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
				--Y11
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y11')
				BEGIN
						UPDATE P SET P.Value = T.NOV 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y11'
				END
				ELSE
				BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y12
					IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Actual WHERE PlanLineItemId = @EntityId AND Period = 'Y12')
				BEGIN
						UPDATE P SET P.Value = T.DEC 
						from Plan_Campaign_Program_Tactic_LineItem_Actual P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y12'
				END
				ELSE
				BEGIN
					IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_Tactic_LineItem_Actual  VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

			END

			END
		END
 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp

END

GO


