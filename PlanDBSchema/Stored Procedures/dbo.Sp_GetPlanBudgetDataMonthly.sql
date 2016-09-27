IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanBudgetDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataMonthly] 
END
GO
CREATE PROCEDURE [dbo].[Sp_GetPlanBudgetDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData_Test1 READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Plan' as ActivityType, Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select 
Convert(varchar(max),[PlanId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
    SELECT p.planid, value , period,Budget, p.Title
  FROM  [Plan] p
  left JOIN plan_budget pb on p.PlanId=pb.PlanId  
  WHERE p.PlanId in( SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')


) d
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
) as rPlan group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union 
select ActivityId,[Task Name],'Campaign' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanCampaignId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
  select pc.PlanCampaignId,value, period,CampaignBudget as Budget, pc.Title from Plan_Campaign pc
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId in(SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted = 0
  
)
 e
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
) as rPlanCampaign group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

union
select ActivityId,[Task Name],'Program' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanProgramId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select t.PlanProgramId,t.Title,t.Budget,Value,Period from
 (
  select pc.PlanProgramId, pc.Title,ProgramBudget as Budget from Plan_Campaign_Program pc where IsDeleted=0 and PlanCampaignId in
   ( select PlanCampaignId from Plan_Campaign where PlanId in(SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0) 
  ) as t
  left join Plan_Campaign_Program_Budget pcb on t.PlanProgramId= pcb.PlanProgramBudgetId 
  
) r
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
) as rPlanCampaignProgram group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
union
select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Value,Period,b.Title,b.TacticBudget as Budget from Plan_Campaign_Program_Tactic_Budget as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in( SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData

select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k


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


	
	--if (@EntityId = 17634)
	--begin
	--update [plan] set [version] = '555' where planid = 17634 
	--end
	
	IF ( @Type='Plan')
		BEGIN
	
		IF Exists (select top 1 PlanId from [Plan] where PlanId =  @EntityId)

			BEGIN
			
			UPDATE P SET P.Budget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Budget END
			from [Plan] P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId


			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END
        ELSE
		BEGIN
		INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'test') 
		END


		END
		
IF ( @Type='Campaign')
		BEGIN
		IF Exists (select top 1 PlanCampaignId from [Plan_Campaign] where PlanCampaignId =  @EntityId)
			BEGIN

			UPDATE P SET P.CampaignBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.CampaignBudget END
			from [Plan_Campaign] P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END
		ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'test') 
		END


		END

IF ( @Type='Program')
		BEGIN
			IF Exists (select top 1 PlanProgramId from [Plan_Campaign_Program] where PlanProgramId =  @EntityId)
			BEGIN

			UPDATE P SET P.ProgramBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.ProgramBudget END
			from [Plan_Campaign_Program] P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END

	
		END
		 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'test') 
		END

IF ( @Type='Tactic')
		BEGIN
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId)
			BEGIN

			UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y2')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2'
				END
		    ELSE
				BEGIN
				IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y3')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y5')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5'
				END
		    ELSE
				BEGIN
				IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y6')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y8')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8'
				END
		    ELSE
				BEGIN
				IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y9')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9'
				END
		    ELSE
				BEGIN
				IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

				IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y11')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11'
				END
		    ELSE
				BEGIN
				IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y12')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12'
				END
		    ELSE
				BEGIN
				IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'test') 
		END

		END

 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp

END




