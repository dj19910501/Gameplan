IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'Sp_GetPlanBudgetDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataQuarterly] 
END
Go
CREATE PROCEDURE [dbo].[Sp_GetPlanBudgetDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData_Test READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Plan' as ActivityType, Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
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
select ActivityId,[Task Name],'Campaign' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanCampaignId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
  select pc.PlanCampaignId,value, period,CampaignBudget as Budget, pc.Title from Plan_Campaign pc
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId in(
SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted = 0
  
) e
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
) as rPlanCampaign group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

union
select ActivityId,[Task Name],'Program' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanProgramId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select t.PlanProgramId,t.Title,t.Budget,Value,Period from
 (
  select pc.PlanProgramId, pc.Title,ProgramBudget as Budget from Plan_Campaign_Program pc where IsDeleted=0 and PlanCampaignId in
   ( select PlanCampaignId from Plan_Campaign where PlanId in( 
 SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0) 
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
select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, Value,Period,b.Title,b.TacticBudget as Budget from Plan_Campaign_Program_Tactic_Budget as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId
in( 
SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan'

)
and IsDeleted=0)) 
  
) t
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData



select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

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
Declare @Sum float;	DECLARE @newValue FLOAT;
	IF ( @Type='Plan')
		BEGIN
		IF Exists (select top 1 PlanId from [Plan] where PlanId =  @EntityId)
			BEGIN


			UPDATE P SET P.Budget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Budget END
			from [Plan] P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId

			--get data for that specific plan

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinal') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinal
			END 
			SELECT * INTO #tempDataFinal FROM (SELECT * from Plan_Budget where PlanId=@EntityId) a 


			
			SELECT @Sum=SUM(value) from #tempDataFinal where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId
			--start kausha 
			IF(@newValue!='')
			BEGIN

			if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y1')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y1',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
			END
			ELSE

			BEGIN
			EXEC Plan_BudgetQuarterCalculation @EntityId,1,@newValue			
			END
			

			SELECT @Sum=SUM(value) from #tempDataFinal where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y4')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y4',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END		

				SELECT @Sum=SUM(value) from #tempDataFinal where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y7')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y7',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

		
			SELECT @Sum=SUM(value) from #tempDataFinal where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinal WHERE PlanId = @EntityId AND Period = 'Y10')
						BEGIN
							UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
							from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
							INSERT INTO Plan_Budget VALUES (@EntityId, 'Y10',@newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
				 BEGIN
					EXEC Plan_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END

			END



		END
		
IF ( @Type='Campaign')
		BEGIN
		IF Exists (select top 1 PlanCampaignId from Plan_Campaign where PlanCampaignId =  @EntityId)
			BEGIN

			UPDATE P SET P.CampaignBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.CampaignBudget END
			from [Plan_Campaign] P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId

			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalCampaign') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalCampaign
			END 
			SELECT * INTO #tempDataFinalCampaign FROM (SELECT * from Plan_Campaign_Budget where PlanCampaignId=@EntityId) a 



			SELECT @Sum=SUM(value) from #tempDataFinalCampaign where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y1')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 ELSE
					BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=SUM(value) from #tempDataFinalCampaign where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y4')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END

		
			SELECT @Sum=SUM(value) from #tempDataFinalCampaign where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y7')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

			SELECT @Sum=SUM(value) from #tempDataFinalCampaign where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Campaign'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalCampaign WHERE PlanCampaignId = @EntityId AND Period = 'Y10')
						BEGIN
							UPDATE P SET P.Value =  CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			       from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
			             INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				 BEGIN
					EXEC Plan_CampaignBudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END
		

			END



		END

IF ( @Type='Program')
		BEGIN
		IF Exists (select top 1 PlanProgramId from Plan_Campaign_Program where PlanProgramId =  @EntityId)
			BEGIN

			UPDATE P SET P.ProgramBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.ProgramBudget END 
			from [Plan_Campaign_Program] P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId



			IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalProgram') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalProgram
			END 
			SELECT * INTO #tempDataFinalProgram FROM (SELECT * from Plan_Campaign_Program_Budget where PlanProgramId=@EntityId) a 



			SELECT @Sum=SUM(value) from #tempDataFinalProgram where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y1')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
						IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END
				  ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

				SELECT @Sum=SUM(value) from #tempDataFinalProgram where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y4')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
						IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END
		
				SELECT @Sum=SUM(value) from #tempDataFinalProgram where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y7')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
						IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END
		
				SELECT @Sum=SUM(value) from #tempDataFinalProgram where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Program'
				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalProgram WHERE PlanProgramId = @EntityId AND Period = 'Y10')
						BEGIN
						UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			      from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
						IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END
		
			END



		END

IF ( @Type='Tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId)
			BEGIN

			UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataFinalTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataFinalTactic
			END 
			SELECT * INTO #tempDataFinalTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId) a 



			SELECT @Sum=SUM(value) from #tempDataFinalTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=SUM(value) from #tempDataFinalTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=SUM(value) from #tempDataFinalTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=SUM(value) from #tempDataFinalTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!='')
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataFinalTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Plan_Campaign_Program_Tactic_Budget_BudgetQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		
			END

		END

 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End

select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
END