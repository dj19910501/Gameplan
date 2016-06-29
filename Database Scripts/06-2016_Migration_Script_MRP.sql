
--Created by Rushil Buptani Date : 22-June-2016
--Start Import budget script 
--BudgetDataMonthly
/****** Object:  UserDefinedTableType [dbo].[ImportExcelBudgetMonthData]    Script Date: 06/22/2016 20:43:03 ******/
IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name ='ImportExcelBudgetMonthData')
	BEGIN
		DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataMonthly]
		DROP TYPE [dbo].[ImportExcelBudgetMonthData]    
	END
GO

/****** Object:  UserDefinedTableType [dbo].[ImportExcelBudgetMonthData]    Script Date: 06/22/2016 20:43:03 ******/
CREATE TYPE [dbo].[ImportExcelBudgetMonthData] AS TABLE(
	[ActivityId] [int] NULL,
	[Task Name] [varchar](3000) NULL,
	[Budget] [float] NULL,
	[JAN] [float] NULL,
	[FEB] [float] NULL,
	[MAR] [float] NULL,
	[APR] [float] NULL,
	[MAY] [float] NULL,
	[JUN] [float] NULL,
	[JUL] [float] NULL,
	[AUG] [float] NULL,
	[SEP] [float] NULL,
	[OCT] [float] NULL,
	[NOV] [float] NULL,
	[DEC] [float] NULL
)
GO

/****** Object:  StoredProcedure [dbo].[Sp_GetPlanBudgetDataMonthly]    Script Date: 06/22/2016 20:35:34 ******/


IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'Sp_GetPlanBudgetDataMonthly') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataMonthly]
GO

/****** Object:  StoredProcedure [dbo].[Sp_GetPlanBudgetDataMonthly]    Script Date: 06/22/2016 20:35:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rushil Bhuptani
-- Create date: 06/08/2016
-- Description:	Procedure to get formatted data of Plan Budget.
-- Exec spGetPlanBudgetData  17314
-- =============================================

CREATE PROCEDURE [dbo].[Sp_GetPlanBudgetDataMonthly]  --17314
@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId uniqueidentifier
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Plan' as ActivityType, Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select 
Convert(varchar(max),[PlanId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
  --SELECT pb.planid, value , period,Budget, p.Title
  --FROM plan_budget pb
  --left JOIN [Plan] p on pb.PlanId=p.PlanId
  
  -- WHERE pb.PlanId = @PlanId

     SELECT p.planid, value , period,Budget, p.Title
  FROM  [Plan] p
  left JOIN plan_budget pb on p.PlanId=pb.PlanId
  
   WHERE p.PlanId = @PlanId
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
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId = @PlanId and IsDeleted = 0
  
) e
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
   ( select PlanCampaignId from Plan_Campaign where PlanId=@PlanId and IsDeleted=0) 
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
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId=@PlanId and IsDeleted=0)) 
  
) t
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData

select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId) TempInner

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



		END

 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp

END



GO

--DataQuarterly
/****** Object:  UserDefinedTableType [dbo].[ImportExcelBudgetQuarterData]    Script Date: 06/22/2016 20:49:12 ******/
IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name ='ImportExcelBudgetQuarterData')
	BEGIN
		DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataQuarterly]
		DROP TYPE [dbo].[ImportExcelBudgetQuarterData]    	
	END
GO

/****** Object:  UserDefinedTableType [dbo].[ImportExcelBudgetQuarterData]    Script Date: 06/22/2016 20:49:12 ******/
CREATE TYPE [dbo].[ImportExcelBudgetQuarterData] AS TABLE(
	[ActivityId] [int] NULL,
	[Task Name] [varchar](3000) NULL,
	[Budget] [float] NULL,
	[Q1] [float] NULL,
	[Q2] [float] NULL,
	[Q3] [float] NULL,
	[Q4] [float] NULL
)
GO


IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'Sp_GetPlanBudgetDataQuarterly') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].[Sp_GetPlanBudgetDataQuarterly]
GO

/****** Object:  StoredProcedure [dbo].[Sp_GetPlanBudgetDataQuarterly]    Script Date: 06/22/2016 20:41:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rushil Bhuptani
-- Create date: 06/08/2016
-- Modified date: 06/28/2016
-- Description:	Procedure to get formatted data of Plan Budget.
-- Exec spGetPlanBudgetData  17314
-- =============================================

CREATE PROCEDURE [dbo].[Sp_GetPlanBudgetDataQuarterly]  --17314
@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId uniqueidentifier 
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
  
   WHERE p.PlanId = @PlanId
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
  left join Plan_Campaign_Budget pcb on pc.planCampaignid = pcb.PlanCampaignId where pc.PlanId = @PlanId and IsDeleted = 0
  
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
   ( select PlanCampaignId from Plan_Campaign where PlanId=@PlanId and IsDeleted=0) 
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
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId=@PlanId and IsDeleted=0)) 
  
) t
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12


) as ExistingData



select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId) TempInner

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

	IF ( @Type='Plan')
		BEGIN
		IF Exists (select top 1 PlanId from [Plan] where PlanId =  @EntityId)
			BEGIN

			UPDATE P SET P.Budget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Budget END
			from [Plan] P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId


			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN T.Q1 ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y1', (SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN T.Q2 ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
				IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y4', (SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN T.Q3 ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
				IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y7', (SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			
			IF EXISTS (SELECT * from Plan_Budget WHERE PlanId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN T.Q4 ELSE P.Value END
			from Plan_Budget P INNER JOIN #TempDiffer T on P.PlanId = T.ActivityId WHERE P.PlanId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
				IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Budget VALUES (@EntityId, 'Y10', (SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			END



		END
		
IF ( @Type='Campaign')
		BEGIN
		IF Exists (select top 1 PlanCampaignId from Plan_Campaign where PlanCampaignId =  @EntityId)
			BEGIN

			UPDATE P SET P.CampaignBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.CampaignBudget END
			from [Plan_Campaign] P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value =  CASE WHEN T.Q1 != '' THEN T.Q1 ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
				IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y1', (SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN T.Q2 ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y4', (SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN T.Q3 ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y7', (SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			
			IF EXISTS (SELECT * from Plan_Campaign_Budget WHERE PlanCampaignId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN T.Q4 ELSE P.Value END
			from Plan_Campaign_Budget P INNER JOIN #TempDiffer T on P.PlanCampaignId = T.ActivityId WHERE P.PlanCampaignId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Budget VALUES (@EntityId, 'Y10', (SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			END



		END

IF ( @Type='Program')
		BEGIN
		IF Exists (select top 1 PlanProgramId from Plan_Campaign_Program where PlanProgramId =  @EntityId)
			BEGIN

			UPDATE P SET P.ProgramBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.ProgramBudget END 
			from [Plan_Campaign_Program] P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN T.Q1 ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y1', (SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN T.Q2 ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y4', (SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN T.Q3 ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y7', (SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Budget WHERE PlanProgramId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN T.Q4 ELSE P.Value END
			from Plan_Campaign_Program_Budget P INNER JOIN #TempDiffer T on P.PlanProgramId = T.ActivityId WHERE P.PlanProgramId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Budget VALUES (@EntityId, 'Y10', (SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			END



		END

IF ( @Type='Tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId)
			BEGIN

			UPDATE P SET P.TacticBudget = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.TacticBudget END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y1')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN T.Q1 ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y1', (SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y4')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN T.Q2 ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y4', (SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y7')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN T.Q3 ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y7', (SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Budget WHERE PlanTacticId = @EntityId AND Period = 'Y10')
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN T.Q4 ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Budget P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10'
				END
		    ELSE
				BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Budget VALUES (@EntityId, 'Y10', (SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
					
				END

			END



		END

 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End

select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
END



GO


--End Import budget script



-- Updated by Akashdeep Kadia 
-- Updated on :: 15-June-2016
-- Desc :: Special charachters replace with Hyphen in Export to CSV. 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
Create PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@ClientId nvarchar(max)=''
,@HoneyCombids nvarchar(max)=null
AS
BEGIN

SET NOCOUNT ON;
--Update CustomField set Name =REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') where ClientId=@ClientId-- This is to Special charachter En Dash replace with Hyphen in CustomField Name
IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost',null AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',[Tactic].Cost As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',[lineitem].Cost As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy Uniqueidentifier,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
		END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType <> 'Budget'
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType <> 'Budget' 
AND EntityType <> 'Lineitem'
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End

END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublishModel]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[PublishModel]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Created by Nishant Sheth
-- Created on :: 06-Jun-2016
-- Desc :: Update Plan model id and tactic's tactic type id with new publish version of model 
CREATE PROCEDURE [dbo].[PublishModel]
@NewModelId int = 0 
,@UserId uniqueidentifier=''
AS
SET NOCOUNT ON;

BEGIN
IF OBJECT_ID(N'tempdb..#tblModelids') IS NOT NULL
BEGIN
  DROP TABLE #tblModelids
END

-- Get all parents model of new model
;WITH tblParent  AS
(
    SELECT ModelId,ParentModelId
        FROM [Model] WHERE ModelId = @NewModelId
    UNION ALL
    SELECT [Model].ModelId,[Model].ParentModelId FROM [Model]  JOIN tblParent  ON [Model].ModelId = tblParent.ParentModelId
)
SELECT ModelId into #tblModelids
    FROM tblParent 
	OPTION(MAXRECURSION 0)

-- Update Tactic Type for Default saved views
DECLARE  @TacticTypeIds NVARCHAR(MAX)=''
SELECT @TacticTypeIds = FilterValues From Plan_UserSavedViews WHERE Userid=@UserId AND FilterName='TacticType'

DECLARE   @FilterValues NVARCHAR(MAX)
SELECT    @FilterValues = COALESCE(@FilterValues + ',', '') + CAST(TacticTypeId AS NVARCHAR) FROM TacticType 
WHERE PreviousTacticTypeId IN(SELECT val FROM dbo.comma_split(@TacticTypeIds,','))
AND ModelId=@NewModelId

IF @FilterValues <>'' 
BEGIN
	UPDATE Plan_UserSavedViews SET FilterValues=@FilterValues WHERE Userid=@UserId AND FilterName='TacticType'
END

-- Update Plan's ModelId with new modelid
UPDATE [Plan] SET ModelId=@NewModelId WHERE ModelId IN(SELECT ModelId FROM #tblModelids)

-- Update Tactic's Tactic Type with new model's tactic type
UPDATE Tactic SET Tactic.TacticTypeId=TacticType.TacticTypeId FROM 
Plan_Campaign_Program_Tactic Tactic 
CROSS APPLY(SELECT TacticType.TacticTypeId FROM TacticType WHERE TacticType.PreviousTacticTypeId=Tactic.TacticTypeId)TacticType
CROSS APPLY(SELECT Program.PlanProgramId,Program.PlanCampaignId FROM Plan_Campaign_Program Program WHERE Program.PlanProgramId=Tactic.PlanProgramId) Program
CROSS APPLY(SELECT Camp.PlanCampaignId,Camp.PlanId FROM Plan_Campaign Camp WHERE Camp.PlanCampaignId=Program.PlanCampaignId 
AND Camp.PlanId IN(SELECT PlanId FROM [Plan] WHERE ModelId IN(SELECT ModelId FROM #tblModelids)))Camp
WHERE Tactic.IsDeleted=0
AND Tactic.TacticTypeId IS NOT NULL

END
GO

-- Modified By Nishant Sheth
-- Date :: 16-Jun-2016
-- Desc :: To resolve the linked tactic id pass as null with store marketo url
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment]
	@strCreatedTacIds nvarchar(max),
	@strUpdatedTacIds nvarchar(max),
	@strUpdateComment nvarchar(max),
	@strCreateComment nvarchar(max),
	@isAutoSync bit='0',
	@userId uniqueidentifier,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'

	Declare @strAllPlanTacIds nvarchar(max)=''
	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@strCreateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@strUpdateComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
	END
    
END


GO


-- ========================================================================================================

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'June.2016'
set @version = 'June.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO




-- Added by Komal Rawal
-- Added on :: 08-June-2016
-- Desc :: On creation of new item in Marketing budget give permission as per parent items.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveuserBudgetPermission]
GO
/****** Object:  StoredProcedure [dbo].[SaveuserBudgetPermission]    Script Date: 06/09/2016 15:30:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveuserBudgetPermission] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy uniqueidentifier 
AS
BEGIN
WITH CTE AS
(
 
    SELECT ParentId 
    FROM Budget_Detail
    WHERE id= @BudgetDetailId
    UNION ALL
    --This is called multiple times until the condition is met
    SELECT g.ParentId 
    FROM CTE c, Budget_Detail g
    WHERE g.id= c.parentid

)

Select * into ##tempbudgetdata from (select ParentId ,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as RN from CTE) as result
option (maxrecursion 0)

select * from ##tempbudgetdata where ParentId is not null

IF OBJECT_ID('tempdb..##AllUniqueBudgetdata') IS NOT NULL
Drop Table ##AllUniqueBudgetdata

--Get user data of all the parents
SELECT * INTO ##AllUniqueBudgetdata FROM (
select Distinct BudgetDetailId, UserId,@BudgetDetailId as bid,GETDATE() as dt,@CreatedBy as Cby,Case WHEN UserId = @CreatedBy
THEN 
@PermissionCode
 ELSE
PermisssionCode END as percode,
Case WHEN UserId = @CreatedBy
THEN 
 1
 ELSE
 0 END as usrid
from Budget_Permission where BudgetDetailId in (select ParentId from ##tempbudgetdata)) as data


-- Insert unique data of parents for new item,take users from upper level parent if user not present in immediate parent
insert into Budget_Permission select Uniquedata.UserId,Uniquedata.bid,GETDATE(),Uniquedata.Cby,Uniquedata.percode,Uniquedata.usrid from ##AllUniqueBudgetdata as Uniquedata
JOIN ##tempbudgetdata as TempBudgetTable on Uniquedata.BudgetDetailId = TempBudgetTable.ParentId and UserId NOT IN 
(
select UserId 
FROM ##AllUniqueBudgetdata as Alldata
JOIN ##tempbudgetdata as parent on Alldata.BudgetDetailId = parent.ParentId and RN < TempBudgetTable.RN 
)
UNION
select  @CreatedBy,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode,1 from Budget_Permission 

IF OBJECT_ID('tempdb..##tempbudgetdata') IS NOT NULL
Drop Table ##tempbudgetdata

IF OBJECT_ID('tempdb..##AllUniqueBudgetdata') IS NOT NULL
Drop Table ##AllUniqueBudgetdata

END

GO


IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Report_Intergration_Conf'))
BEGIN
CREATE TABLE [dbo].[Report_Intergration_Conf](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[IdentifierColumn] [nvarchar](255) NOT NULL,
	[IdentifierValue] [nvarchar](1000) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MeasureApiConfiguration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_TableName_IdentifierColumn_IdentifierValue] UNIQUE NONCLUSTERED 
(
	[TableName] ASC,
	[IdentifierColumn] ASC,
	[IdentifierValue] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboarContentData]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[GetDashboarContentData]
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboardContentData]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[GetDashboardContentData]
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetDashboardContentData]
	-- Add the parameters for the stored procedure here
	@UserId varchar(max),
	@DashboardID int = 0
AS
BEGIN
	IF (@UserId IS NOT NULL AND @UserId != '')
	BEGIN
		IF (ISNULL(@DashboardID, 0) > 0)
		BEGIN
			IF EXISTS (SELECT D.id
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID)
			BEGIN
				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted,IsComparisonDisplay=ISNULL(D.IsComparisonDisplay,0), 
					ISNULL(HelpTextId,0) AS HelpTextId 
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID
                    ORDER BY D.DisplayOrder


				SELECT dc.id, dc.DisplayName, dc.DashboardId, dc.DisplayOrder, dc.ReportTableId, dc.ReportGraphId, dc.Height, dc.Width, dc.Position, dc.IsCumulativeData, dc.IsCommunicativeData, dc.DashboardPageID,					dc.IsDeleted, dc.DisplayIfZero, dc.KeyDataId, dc.HelpTextId
					FROM DashboardContents AS dc 
					INNER JOIN Dashboard AS D ON D.id = dc.DashboardId AND D.IsDeleted = 0 AND D.ParentDashboardId IS NULL
					INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE dc.IsDeleted = 0 AND dc.DashboardId = @DashboardID
					ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'User Not Authorize to Access Dashboard'
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT D.id
				FROM Dashboard D
                INNER JOIN User_Permission UP ON d.id = UP.DashboardId  AND UP.UserId = @UserId
				WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0)
			BEGIN

				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, D.[Rows], D.[Columns], D.ParentDashboardId, D.IsDeleted, D.IsComparisonDisplay, D.HelpTextId
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0
                    ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'No Dashboard Configured For User'
			END
		END
	END
	ELSE
	BEGIN
		SELECT 'Please Provide Proper UserId'
	END
END

GO

--- START: PL ticket #2251 related SPs & Functions --------------------
-- Created By: Viral 
-- Created On: 06/10/2016
-- Description: PL ticket #2251: Prepared Data before push to SFDC through Integration Web API.

/****** Object:  StoredProcedure [dbo].[spGetSalesforceMarketo3WayData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceMarketo3WayData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceMarketo3WayData]
GO
/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetSalesforceData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticActualCostMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''105406'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementCampaign'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16404'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementProgram'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''16402'',2,1203,255,0,0)
--SELECT * from  [GetSFDCSourceTargetMappingData](''ImprovementTactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''7864'',2,1203,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		--Declare @trgtSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016
		--Declare @trgtSourceParentId varchar(50)=''''
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		--Declare @trgtMode varchar(255)=''''
		Declare @actObjType varchar(255)=''ObjectType''
		--Declare @trgtObjType varchar(255)=''''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		Declare @entImprvTactic varchar(255)=''ImprovementTactic''
		Declare @entImprvProgram varchar(255)=''ImprovementProgram''
		Declare @entImprvCampaign varchar(255)=''ImprovementCampaign''
		Declare @actIsSyncedSFDC varchar(100)=''IsSyncedSFDC''				-- Added on 06/23/2016
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables

		 -- START:- Improvement Variable declaration
		 --Cost Field
		Declare @imprvCost varchar(20)=''ImprvCost''
		Declare @actImprvCost varchar(20)=''Cost''

		 -- Static Status
		 Declare @imprvPlannedStatus varchar(50)=''Planned''
		 Declare @tblImprvTactic varchar(200)=''Plan_Improvement_Campaign_Program_Tactic''
		 Declare @tblImprvProgram varchar(200)=''Plan_Improvement_Campaign_Program''
		 Declare @tblImprvCampaign varchar(200)=''Plan_Improvement_Campaign''

		 -- Imprv. Tactic table Actual Fields
		 Declare @actEffectiveDate varchar(50)=''EffectiveDate''
		 -- END: Improvement Variable declaration
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

	-- IF EntityType is ''Improvement Campaign, Program or Tactic'' then add respective entity related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				CASE 
					WHEN ((gpDataType.TableName=@tblImprvTactic) AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''1'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@entityType) and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
IF((@entityType=@entImprvTactic) OR (@entityType=@entImprvProgram) OR (@entityType=@entImprvCampaign))
BEGIN
	-- Insert table name ''Global'' and IsImprovement flag ''1'' in case of Improvement entities
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement		-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblGlobal as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''1'' as IsImprovement			-- Added on 06/23/2016
END
ELSE
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement		-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Campaigns
		SELECT 
			IC.ImprovementPlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign IC 
		WHERE @entityType=@entImprvCampaign and ImprovementPlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Programs
		SELECT 
			IP.ImprovementPlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program IP 
		WHERE @entityType=@entImprvProgram and ImprovementPlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Improvement Tactics
		SELECT 
			IT.ImprovementPlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Improvement_Campaign_Program_Tactic IT 
		WHERE @entityType=@entImprvTactic and ImprovementPlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN(Tac.IsSyncMarketo=''1'') THEN IsNUll(Tac.TacticCustomName,'''') ELSE		-- if 3way Marketo-SFDC related data then pass TacticCustomName as ''Title''
																		CASE
																			WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																			ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																		END
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(prg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN Cast(ISNULL(Tac.IsSyncMarketo,''0'') as varchar(50))		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(cmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Improvement Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(Imprvcmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Imprvcmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Imprvcmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Imprvcmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvCampaign
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = T.SourceId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvCampaign)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvPrg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvPrg.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvPrg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvPrg.ImprovementPlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(Imprvcmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvPrg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvProgram
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvProgram)
	)
	UNION
	(
		-- GET Improvement Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entImprvTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(ImprvTac.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(ImprvTac.[Description],'''')
							WHEN Mapp.ActualFieldName=''Status'' THEN @imprvPlannedStatus
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actImprvCost THEN ISNull(Cast(ImprvTac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actEffectiveDate THEN ISNull(CONVERT(VARCHAR(19),ImprvTac.EffectiveDate),'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(ImprvTac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(ImprvTac.ImprovementPlanProgramId as nvarchar(255)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(ImprvPrg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(ImprvTac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entImprvTactic
		LEFT JOIN Plan_Improvement_Campaign_Program_Tactic as ImprvTac ON ImprvTac.ImprovementPlanTacticId = T.SourceId
		LEFT JOIN Plan_Improvement_Campaign_Program as ImprvPrg ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
		LEFT JOIN Plan_Improvement_Campaign as Imprvcmpgn ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId
		LEFT JOIN [Plan] as pln ON pln.PlanId = Imprvcmpgn.ImprovePlanId and pln.IsDeleted=0
		Where (Mapp.IsImprovement=''1'' and Mapp.TableName=@tblGlobal) OR (Mapp.TableName= @tblImprvTactic)
	)
) as result;



Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''


RETURN
END
' 
END

GO

/****** Object:  UserDefinedFunction [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSFDCSourceTargetMappingData_Marketo3Way]
(
	@entityType varchar(255)='''',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255,	-- default value 255
	@isCustomNameAllow bit =''0'',
	@isClientAllowCustomName bit =''0''
)

--SELECT * from  [GetSFDCSourceTargetMappingData_Marketo3Way](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''105314'',2,1211,255,0,0)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS

BEGIN

------- START:- Declare local variables 
	BEGIN
		Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),CustomFieldId INT,IsImprovement bit)
		Declare @tacActCostTable Table(PlanTacticId int,ActualCost varchar(50))
		Declare @ColumnName nvarchar(max)
		-- START: Declare Fixed columns SFDC variables
		Declare @actSFDCID varchar(50)=''SalesforceId''
		Declare @actSourceParentId varchar(50)=''SourceParentId''
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016
		Declare @actIsSyncedSFDC varchar(100)=''IsSyncedSFDC''				-- Added on 06/23/2016
		Declare @actTitle varchar(255)=''Title''
		Declare @actMode varchar(255)=''Mode''
		Declare @actObjType varchar(255)=''ObjectType''
		Declare @actStartDate varchar(255)=''StartDate''
		Declare @actEndDate varchar(255)=''EndDate''
		Declare @actsfdcParentId varchar(50)=''ParentId''
		 -- END: Declare Fixed columns SFDC variables
		Declare @modeCREATE varchar(20)=''Create''
		Declare @modeUPDATE varchar(20)=''Update''
		Declare @actCreatedBy varchar(255)=''CreatedBy''
		Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''
		Declare @tblGlobal varchar(100)=''Global''
		 -- START:- Declare entityType variables
		Declare @entTactic varchar(20 )=''Tactic''
		Declare @entProgram varchar(20 )=''Program''
		Declare @entCampaign varchar(20 )=''Campaign''
		-- END:- Declare entityType variables

		-- START: Plan Entity Status Variables
		Declare @declined varchar(50)=''Declined''
		Declare @InProgress varchar(50)=''In-Progress''
		Declare @completed varchar(50)=''Complete''
		Declare @sfdcAborted varchar(50)=''Aborted''
		Declare @sfdcInProgress varchar(50)=''In Progress''
		Declare @sfdcCompleted varchar(50)=''Completed''
		Declare @sfdcPlanned varchar(50)=''Planned''
		-- END: Plan Entity Status Variables
		
	END

 
------- END:- Declare local variables 

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	(
			-- Select GLOBAL standard fields from IntegrationInstanceDataTypeMapping table.

				Select  IntegrationInstanceID,
						IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
						TableName,
						gpDataType.ActualFieldName,
						IsNull(mapp.CustomFieldId,0) as CustomFieldId,
						IsNull(gpDataType.IsImprovement,''0'') as IsImprovement
				FROM GamePlanDataType as gpDataType
				JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
				Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@tblGlobal and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
			--END
			
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId,
					''0'' as IsImprovement
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	)
	insert into @Table 
	select * from ResultTable

	-- IF EntityType is ''Tactic'' then add Tacic related mapping fields from IntegrationInstanceDataTypeMapping table.
	IF(@entityType=@entTactic)
	BEGIN
		insert into @Table 
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				gpDataType.ActualFieldName,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId,
				''0'' as IsImprovement
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=''Plan_Campaign_Program_Tactic'' and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	END

END
-------- END: Get Standard & CustomField Mappings data --------

-------- START: Insert fixed SFDC fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSFDCID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actSourceParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actObjType as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actsfdcParentId as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actcParentID as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actIsSyncedSFDC as ActualFieldName,0 as CustomFieldId,''0'' as IsImprovement			-- Added on 06/23/2016
END
-------- END: Insert fixed SFDC fields to Mapping list. -------- 

-------- START: Get Tacticwise ActualCost. -------- 

Declare @actCost varchar(20)=''CostActual''
Declare @actCostGPTypeId int=0
Select @actCostGPTypeId = GameplanDataTypeId from GameplanDataType where IntegrationTypeId=@integrationTypeId and IsDeleted=''0'' and TableName=@tblTactic and ActualFieldName=@actCost

-- Calculate Tactiwise ActualCost in case of If user has made ActualCost mapping and EntityType is Tactic 
IF EXISTS(Select * from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@id and GameplanDataTypeId=@actCostGPTypeId)AND(@entityType=@entTactic)
BEGIN
	INSERT INTO @tacActCostTable
	SELECT * FROM [dbo].[GetTacticActualCostMappingData](@EntityIds)
END
-------- END: Get Tacticwise ActualCost. -------- 

;WITH entTbl as(
	(
		-- Get Tactics
		SELECT 
			T.PlanTacticId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program_Tactic T 
		WHERE @entityType=@entTactic and PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Programs
		SELECT 
			P.PlanProgramId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign_Program P 
		WHERE @entityType=@entProgram and PlanProgramId IN (select val from comma_split(@EntityIds,'',''))
	)
	UNION 
	(
		-- Get Campaigns
		SELECT 
			C.PlanCampaignId as SourceId,
			''Static_Mapp'' as Link
		FROM Plan_Campaign C 
		WHERE @entityType=@entCampaign and PlanCampaignId IN (select val from comma_split(@EntityIds,'',''))
	)
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Static_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
SELECT * FROM 
(
	(
		-- GET Tactic Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entTactic
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (CASE
																		WHEN(Tac.IsSyncMarketo=''1'') THEN IsNUll(Tac.TacticCustomName,'''') ELSE		-- if 3way Marketo-SFDC related data then pass TacticCustomName as ''Title''
																		CASE
																			WHEN (@isCustomNameAllow=''1'' AND @isClientAllowCustomName=''1'') THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
																			ELSE (ISNull(SUBSTRING(Tac.Title,1,@SFDClength),''''))
																		END
																   END)
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),Tac.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),Tac.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(Tac.IntegrationInstanceTacticId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(Tac.PlanProgramId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(prg.IntegrationInstanceProgramId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(Tac.IntegrationInstanceTacticId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''CostActual'' THEN ISNull(Cast(0 as varchar(255)),'''')
							WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN Cast(ISNULL(Tac.IsSyncMarketo,''0'') as varchar(50))		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(Tac.IntegrationInstanceTacticId,'''')<>'''') THEN prg.IntegrationInstanceProgramId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entTactic
		LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.SourceId = Tac.PlanTacticId
		LEFT JOIN @tacActCostTable as acost ON T.SourceId = acost.PlanTacticId
		LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Program Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entProgram
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(prg.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(prg.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),prg.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),prg.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(prg.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(prg.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(prg.IntegrationInstanceProgramId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ISNull(Cast(prg.PlanCampaignId as varchar(50)),'''')
							WHEN Mapp.ActualFieldName=@actcParentID THEN ISNull(Cast(cmpgn.IntegrationInstanceCampaignId as varchar(50)),'''')		-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(prg.IntegrationInstanceProgramId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN  ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
							WHEN (Mapp.ActualFieldName=@actsfdcParentId) AND (ISNULL(prg.IntegrationInstanceProgramId,'''')<>'''') THEN cmpgn.IntegrationInstanceCampaignId		-- In case of Marketo-SFDC 3-Way integration, Add Program SFDCID to create hierearchy in SFDC
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entProgram
		LEFT JOIN Plan_Campaign_Program as prg ON T.SourceId = prg.PlanProgramId and prg.IsDeleted=0
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
	UNION
	(
		-- GET Campaign Data based on Mapping defined in IntegrationInstanceDataTypeMapping table
		SELECT Mapp.ActualFieldName,
				Mapp.CustomFieldId,
				CASE 
					WHEN @entityType=@entCampaign
					THEN
						CASE 
							WHEN Mapp.ActualFieldName=@actTitle THEN (ISNull(SUBSTRING(cmpgn.Title,1,@SFDClength),''''))
							WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(cmpgn.[Description],'''')
							WHEN Mapp.ActualFieldName=@actStartDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.StartDate,126),'''')
							WHEN Mapp.ActualFieldName=@actEndDate THEN ISNull(CONVERT(VARCHAR(100),cmpgn.EndDate,126),'''')  
							WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(cmpgn.[Status],'''')
							WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(cmpgn.CreatedBy as varchar(100)),'''')
							WHEN Mapp.ActualFieldName=''ActivityType'' THEN @entityType
							WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
							WHEN Mapp.ActualFieldName=@actSFDCID THEN ISNull(cmpgn.IntegrationInstanceCampaignId,'''')
							WHEN Mapp.ActualFieldName=@actSourceParentId THEN ''''
							WHEN Mapp.ActualFieldName=@actcParentID THEN ''''												-- Added on 06/23/2016
							WHEN Mapp.ActualFieldName=@actObjType THEN @entityType
							WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
																		WHEN ISNULL(cmpgn.IntegrationInstanceCampaignId,'''')='''' THEN @modeCREATE 
																		ELSE @modeUPDATE 
																	 END)
							WHEN Mapp.ActualFieldName=@actIsSyncedSFDC THEN ''0''		-- For Marketo -SFDC 3 way identification.	added on 06/23/2016
							WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,'''')
						END
					ELSE
						Null 
				END AS TacValue,
				T.SourceId as SourceId
		from IntegMapp as Mapp
		INNER JOIN entTbl as T ON Mapp.Link = T.Link and @entityType=@entCampaign
		LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = T.SourceId and cmpgn.IsDeleted=0
		LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
		LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.SourceId = custm.EntityId
	)
) as result;

Update @src_trgt_mappdata Set TacValue=
								CASE 
									WHEN TacValue=@declined THEN @sfdcAborted
									WHEN TacValue=@InProgress THEN @sfdcInProgress
									WHEN TacValue=@completed THEN @sfdcCompleted
									ELSE @sfdcPlanned
								END 
WHERE ActualFieldName=''Status''

RETURN
END
' 
END

GO

/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- SElect [GetSFDCTacticResultColumns] (1203,''464EB808-AD1F-4481-9365-6AADA15023BD'',2)
CREATE FUNCTION [dbo].[GetSFDCTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN
Declare @imprvCost varchar(20)=''ImprvCost''
Declare @actImprvCost varchar(20)=''Cost''
declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

	;With ResultTable as(

(
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				-- Rename actualfield ''Cost'' to ''ImprvCost''  in case of Table Name ''Plan_Improvement_Campaign_Program_Tactic'' to ignore conflict of same name ''Cost'' actual field of both Tactic & Improvement Tactic table
				CASE 
					WHEN  ((gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'') AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost 
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Plan_Campaign_Program'' OR gpDataType.TableName=''Plan_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		SELECT  mapp.IntegrationInstanceId,
				0 as GameplanDataTypeId,
				Null as TableName,
				custm.Name as ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
				
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and (custm.EntityType=''Tactic'' or custm.EntityType=''Campaign'' or custm.EntityType=''Program'')
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select Distinct ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END

' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:02:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTacticActualCostMappingData]
(
	@entIds varchar(max)=''''
)
RETURNS @tac_actualcost_mappingtbl Table(
PlanTacticId int,
ActualCost varchar(50)
)

AS
BEGIN
	Declare @costStage varchar(20)=''Cost''

	-- Get Tactic & Tactic Actual Cost Mapping data 
	-- If Tactic has lineitems then Sum up of LineItem Actual''s value else Tactic Actual''s value.

	INSERT INTO @tac_actualcost_mappingtbl
	SELECT tac.PlanTacticId,
	   	   CASE 
			WHEN COUNT(distinct line.PlanLineItemId) >0 THEN  Cast(IsNULL(SUM(lActl.Value),0) as varchar(50)) ELSE  Cast(IsNULL(SUM(tActl.Actualvalue),0) as varchar(50))
		   END as ActualCost
	FROM Plan_Campaign_Program_Tactic as tac
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem as line on tac.PlanTacticId = line.PlanTacticId and line.IsDeleted=''0''
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual as lActl on line.PlanLineItemId = lActl.PlanLineItemId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual as tActl on tac.PlanTacticId = tActl.PlanTacticId and  tActl.StageTitle=@costStage
	WHERE tac.PlanTacticId IN (select val from comma_split(@entIds,'',''))
	GROUP BY tac.PlanTacticId
	RETURN 
END
' 
END

GO


/****** Object:  UserDefinedFunction [dbo].[fnGetSalesforceMarketo3WayData]    Script Date: 06/24/2016 1:53:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetSalesforceMarketo3WayData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetSalesforceMarketo3WayData]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetSalesforceMarketo3WayData]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetSalesforceMarketo3WayData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create Function [dbo].[fnGetSalesforceMarketo3WayData]
(
	@entityType varchar(255)='''',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0
)
RETURNS @dataQuery table
(
	query nvarchar(max)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)=''Tactic''
		Declare @entityTypeProg varchar(255)=''Program''
		Declare @entityTypeCampgn varchar(255)=''Campaign''
		Declare @entityTypeIntegrationInstance varchar(255)=''IntegrationInstance''
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)=''In-Progress''
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit=''0''
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit =''0''
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''''
		Declare @dynResultQuery nvarchar(max)=''''
		Declare @actcParentID varchar(100)=''cParentId''				-- Added on 06/23/2016

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)=''PushTacticData''
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)=''Name''
		Declare @colDescription varchar(50)=''Description''
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)=''Start :''
		Declare @logEnd varchar(20)=''End :''
		Declare @logSP varchar(100)=''Stored Procedure Execution- ''
		Declare @logError varchar(20)=''Error :''
		Declare @logInfo varchar(20)=''Info :''
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)=''Tactic''
		Declare @prg varchar(20)=''Program''
		Declare @cmpgn varchar(20)=''Campaign''
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''''
		Declare @entPrgIds nvarchar(max)=''''
		Declare @entCmpgnIds nvarchar(max)=''''
		Declare @entImrvmntTacIds nvarchar(max)=''''
		Declare @entImrvmntPrgIds nvarchar(max)=''''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''''
		-- End: Entity Ids

	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Start: Identify Entity Type

	BEGIN

		--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Identify EnityType Integration Instance'')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted=''0'' and IsActive=''1'')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get Tactic Data by Instance Id'')
					SET @instanceId= @id

					-- START: Identify 3 Way integration between Marketo & SFDC - Create hierarchy in SFDC 
					BEGIN
						Declare @isSyncSFDCWithMarketo bit=''1''
						--Declare @ModelIds varchar(max)=''''
						--SELECT @ModelIds = ISNULL(@ModelIds  + '','','''') + (mdlId)
						--FROM (Select DISTINCT Cast (ModelId as varchar(max)) mdlId from Model where ((IsNull(IsDeleted,''0'')=''0'') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id OR IntegrationInstanceId=@id))) AS planIds
					END

					-- START: Get Tactic Data by Instance Id
					--BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration=''1'' and (tact.IsSyncMarketo=''1'') and (IsNUll(tact.IntegrationInstanceMarketoID,'''')<>'''') and (tact.[Status]=''Approved'' or tact.[Status]=''In-Progress'' or tact.[Status]=''Complete'')
								where  mdl.ModelId IN (
														Select DISTINCT ModelId from Model where ((IsNull(IsDeleted,''0'')=''0'') AND (IntegrationInstanceMarketoID<>@id OR (IsNull(IntegrationInstanceMarketoID,0)=0)) AND (IntegrationInstanceIdINQ=@id OR IntegrationInstanceIdMQL=@id OR IntegrationInstanceIdCW=@id))
													   ) and mdl.[Status]=''Published'' and mdl.IsActive=''1''
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + '':'' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + '':'' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and (prgrm.[Status]=''Approved'' or prgrm.[Status]=''In-Progress'' or prgrm.[Status]=''Complete'')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]=''Published'' and mdl.IsActive=''1''
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and (campgn.[Status]=''Approved'' or campgn.[Status]=''In-Progress'' or campgn.[Status]=''Complete'')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]=''Published'' and mdl.IsActive=''1''
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
					--END CATCH

					-- END: Get Tactic Data by Instance Id
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get Tactic Data by Instance Id'')
				END
				
			END
			--ELSE
			--BEGIN
			--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Instance Not Exist'')
			--END
			
		END	
		
		--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Identify EnityType Integration Instance'')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'''') =''''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'''')='''')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'''') =''''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'''')='''')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''''
					Declare @updIds varchar(max)=''''

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get comma separated column names'')
					
					--BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
					--END CATCH
										
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get comma separated column names'')	
					
					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Get Tactic Ids'')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + '','','''') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + '','','''') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,''0'')=''1'' AND IsNull(@isClientAllowCustomName,''0'')=''1'')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + '','','''') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Get Tactic Ids'')

						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Update Tactic CustomName'')

						--BEGIN TRY
							-- START: Update Tactic Name --
							--UPDATE Plan_Campaign_Program_Tactic 
							--SET TacticCustomName = T1.CustomName 
							--FROM GetTacCustomNameMappingList(''Tactic'',@clientId,@updIds) as T1 
							--INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'''')=''''
							-- END: Update Tactic Name --
						--END TRY
						--BEGIN CATCH
						--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()))	
						--END CATCH

						--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Update Tactic CustomName'')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''101371'',2,1203,255,0,0)

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+''Create final result Pivot Query'')

					--BEGIN TRY
						SET @dynResultQuery =''SELECT distinct SourceId,SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,''+@ColumnName+'' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @tact +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entTacIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @prg +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entPrgIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData_Marketo3Way](''''''+ @cmpgn +'''''',''''''+ CAST(@clientId AS NVARCHAR(36)) +'''''',''''''+ @entCmpgnIds +'''''',''''''+ CAST(@integrationTypeId AS NVARCHAR) +'''''',''''''+ CAST(@instanceId AS NVARCHAR) +'''''',''''''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +'''''',''''''+ CAST(@isCustomNameAllow AS NVARCHAR) +'''''',''''''+ CAST(@isClientAllowCustomName AS NVARCHAR) +'''''')
																		)
																		
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,''+@ColumnName+'')
								 ) AS PVTTable
								 ''
					--END TRY
					--BEGIN CATCH
					--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+''Exception throw while executing query:''+ (SELECT ''Error Code.- ''+ Cast(ERROR_NUMBER() as varchar(255))+'',Error Line No-''+Cast(ERROR_LINE()as varchar(255))+'',Error Msg-''+ERROR_MESSAGE()+'',SQL Query-''+ (Select @dynResultQuery)))	
					--END CATCH

					--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+''Create final result Pivot Query'')
					--PRINT @dynResultQuery			
				END
				--ELSE
				--BEGIN
				--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''No single field mapped for Salesforce instance'')
				--END
			END
			--ELSE
			--BEGIN
			--	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Data does not exist'')
			--END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	--Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+''Get final result data to push Salesforce'')
	--EXEC(@dynResultQuery)
	Insert Into @dataQuery
	select @dynResultQuery

	Return
END

' 
END

GO



/****** Object:  StoredProcedure [dbo].[spGetSalesforceData]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetSalesforceData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetSalesforceData] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetSalesforceData]
	@entityType varchar(255)='',
	@id int=0,
	@clientId nvarchar(max),
	@SFDCTitleLengthLimit int,
	@integrationInstanceLogId int=0,
	@isClientAllowCustomName bit=0,
	@isSyncSFDCWithMarketo bit=0
AS
BEGIN

	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1211,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'1'
	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1212,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'1'
	--Exec [spGetSalesforceData_06_23_2016] 'IntegrationInstance',1203,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,'0'
	--Exec [spGetSalesforceData_06_23_2016] 'Tactic',105422,'464EB808-AD1F-4481-9365-6AADA15023BD',80,0,0,0
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- START: Declare local variables
	BEGIN
		-- Start: Entity Type variables
		Declare @entityTypeTac varchar(255)='Tactic'
		Declare @entityTypeProg varchar(255)='Program'
		Declare @entityTypeCampgn varchar(255)='Campaign'
		Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
		Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
		Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
		Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
		-- END: Entity Type variables

		-- Start: Sync Status variables
		Declare @syncStatusInProgress varchar(255)='In-Progress'
		-- End: Sync Status variables
		
		--Declare @isAutoSync bit='0'
		--Declare @nullGUID uniqueidentifier
		Declare @integrationTypeId int=0
		Declare @isCustomNameAllow bit ='0'
		Declare @instanceId int=0
		Declare @entIds varchar(max)=''
		Declare @dynResultQuery nvarchar(max)=''

		--Start: Instance Section Name Variables
		Declare @sectionPushTacticData varchar(1000)='PushTacticData'
		--END: Instance Section Name Variables

		-- Start: PUSH Col Names
		Declare @colName varchar(50)='Name'
		Declare @colDescription varchar(50)='Description'
		
		-- End: PUSH Col Names

		-- Start: Log variables
		Declare @logStart varchar(20)='Start :'
		Declare @logEnd varchar(20)='End :'
		Declare @logSP varchar(100)='Stored Procedure Execution- '
		Declare @logError varchar(20)='Error :'
		Declare @logInfo varchar(20)='Info :'
		-- Start: End variables

		-- Start: Object Type variables
		Declare @tact varchar(20)='Tactic'
		Declare @prg varchar(20)='Program'
		Declare @cmpgn varchar(20)='Campaign'
		-- END: Object Type variables

		-- Start: Entity Ids
		Declare @entTacIds nvarchar(max)=''
		Declare @entPrgIds nvarchar(max)=''
		Declare @entCmpgnIds nvarchar(max)=''
		Declare @entImrvmntTacIds nvarchar(max)=''
		Declare @entImrvmntPrgIds nvarchar(max)=''
		Declare @entImrvmntCmpgnIds nvarchar(max)=''
		-- End: Entity Ids
		Declare @query nvarchar(max)=''
		
	END
	-- END: Declare local variables

	-- Store Campaign, Program & Tactic related data
	Declare @tblTaclist table (
								PlanTacticId int,
								PlanProgramId int,
								PlanCampaignId int,
								LinkedTacticId int,
								LinkedPlanId int,
								PlanYear int,
								ObjectType varchar(20),
								RN int
								)

	-- Store Improvement Entities related data
	Declare @tblImprvEntity table (
									ImprovementPlanTacticId int,
									ImprovementPlanProgramId int,
									ImprovementPlanCampaignId int,
									ObjectType varchar(50)
								  )

	-- Start: Identify Entity Type

	BEGIN

		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Identify EnityType Campaign,Program,Tactic,Improvement Tactic or Integration Instance')
		
		IF(UPPER(@entityType) = UPPER(@entityTypeIntegrationInstance))
		BEGIN

			-- Identified Instance Exist or Not
			IF EXISTS(SELECT IntegrationInstanceId from IntegrationInstance where IntegrationInstanceId=@id and IsDeleted='0' and IsActive='1')
			BEGIN
				-- Identified Instance already In-Progress or Not
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Instance Id')
					SET @instanceId= @id

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Model] as mdl
									join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
									Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where  mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- Get Improvement TacticIds
							BEGIN
								Insert into @tblImprvEntity
								Select Imprvtact.ImprovementPlanTacticId,
									   Imprvtact.ImprovementPlanProgramId,
									   Imprvcampgn.ImprovementPlanCampaignId,
									   @entityTypeImprvTac as ObjectType
								from [Model] as mdl
								join [Plan] as pln on mdl.ModelId = pln.ModelId and pln.IsDeleted=0
								Join Plan_Improvement_Campaign as Imprvcampgn ON Imprvcampgn.ImprovePlanId = pln.PlanId 
								join Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
								join Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
								where mdl.IntegrationInstanceId=@id and mdl.IsDeleted=0 and mdl.[Status]='Published' and mdl.IsActive='1'
							END

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Instance Id')
				END
				
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Instance Not Exist')
			END
			
		END	
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeTac))
		BEGIN
			
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Tactic Id')
			BEGIN TRY

				-- Pick latest year tactic in case of linked Tactic and push to SFDC.
				IF EXISTS(SELECT LinkedTacticId from Plan_Campaign_Program_Tactic where PlanTacticId=@id)
				BEGIN
					
					DECLARE @tac_lnkdIds varchar(20)=''
					SELECT @tac_lnkdIds=cast(PlanTacticId as varchar)+','+Cast(ISNULL(LinkedTacticId,0) as varchar) 
					FROM Plan_Campaign_Program_Tactic where PlanTacticId=@id
					;WITH tbl as(
								SELECT tact.PlanTacticId,tact.LinkedTacticId,tact.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tact
								WHERE PlanTacticId IN (select val from comma_split(@tac_lnkdIds,',')) and tact.IsDeleted=0
								UNION ALL
								SELECT tac.PlanTacticId,tac.LinkedTacticId,tac.LinkedPlanId
								FROM  Plan_Campaign_Program_Tactic as tac 
								INNER JOIN tbl as lnk on tac.LinkedTacticId=lnk.PlanTacticId
								WHERE tac.PlanTacticId=@id
								)
					-- Set latest year tactic to @id variable
					SELECT TOP 1 @id=LinkedTacticId 
					FROM tbl
					INNER JOIN [Plan] as pln on tbl.LinkedPlanId = pln.PlanId and pln.IsDeleted=0
					ORDER BY [Year] DESC
				END
			
				INSERT INTO @tblTaclist 
				SELECT tact.PlanTacticId,
						tact.PlanProgramId,
						prg.PlanCampaignId,
						tact.LinkedTacticId ,
						tact.LinkedPlanId,
						Null as PlanYear,
						@tact as ObjectType,
						1 as RN
				FROM Plan_Campaign_Program_Tactic as tact 
				INNER JOIN Plan_Campaign_Program as prg on tact.PlanProgramId = prg.PlanProgramId and prg.IsDeleted='0'
				WHERE tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete') and tact.PlanTacticId=@id
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
				INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0
				INNER JOIN @tblTaclist as tac ON prg.PlanProgramId = tac.PlanProgramId
			END TRY
			BEGIN CATCH
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
			END CATCH

			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Tactic Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeProg))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Program Id')

					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln
								Join Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
								join Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
								join Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.PlanProgramId = @id
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

							-- START: Get list of Campaigns not pushed into SFDC.
							BEGIN
								Insert into @tblTaclist 
								select Null as PlanTacticId,
									   Null as PlanProgramId,
									   cmpgn.PlanCampaignId,
									   Null as LinkedTacticId ,
									   Null as LinkedPlanId,
									   tac.PlanYear,
									   @cmpgn as ObjectType,
									   RN= 1
								from @tblTaclist as tac
								INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
								INNER JOIN Plan_Campaign_Program as P on tac.PlanProgramId=P.PlanProgramId and (IsNull(P.IntegrationInstanceProgramId,'')='')
								where tac.ObjectType=@prg
							END
							-- END: Get list of Campaigns not pushed into SFDC.
							

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0
					INNER JOIN [Plan_Campaign_Program] as prg ON cmpgn.PlanCampaignId = prg.PlanCampaignId and prg.IsDeleted=0 and prg.PlanProgramId=@id
					
					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Program Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeCampgn))
		BEGIN
			Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Data by Campaign Id')
					
					-- START: Get Tactic Data by Instance Id
					BEGIN TRY
							;WITH tblTact AS (
								Select tact.PlanTacticId,
									   tact.PlanProgramId,
									   campgn.PlanCampaignId,
										tact.LinkedTacticId ,
										tact.LinkedPlanId,
										pln.[Year] as PlanYear
								from [Plan] as pln 
								INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
								INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 
								INNER JOIN Plan_Campaign_Program_Tactic as tact on prgrm.PlanProgramId = tact.PlanProgramId and tact.IsDeleted=0 and tact.IsDeployedToIntegration='1' and tact.IsSyncSalesForce='1' and (tact.[Status]='Approved' or tact.[Status]='In-Progress' or tact.[Status]='Complete')
								where  pln.IsDeleted=0
							),
							 tactList AS (
								(
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
												tact1.LinkedTacticId ,
												tact1.LinkedPlanId,
												tact1.PlanYear,
												@tact as ObjectType,
												RN= 1
									FROM tblTact as tact1 
									WHERE IsNull(Tact1.LinkedTacticId,0) <=0
								 )
								 UNION
								 (
									SELECT tact1.PlanTacticId,
											tact1.PlanProgramId,
											tact1.PlanCampaignId,
											tact1.LinkedTacticId ,
											tact1.LinkedPlanId,
											tact1.PlanYear,
											@tact as ObjectType,
											-- Get latest year tactic
											RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + ':' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + ':' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				 END 
																	ORDER BY PlanYear DESC) 
									FROM tblTact as tact1 
									WHERE (tact1.LinkedTacticId > 0)
								 )
								 UNION
								 (
									-- Get Program data
									Select Null as PlanTacticId,
									   prgrm.PlanProgramId,
									   prgrm.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@prg as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id
									INNER JOIN Plan_Campaign_Program as prgrm on campgn.PlanCampaignId = prgrm.PlanCampaignId and prgrm.IsDeleted=0 and prgrm.IsDeployedToIntegration='1' and (prgrm.[Status]='Approved' or prgrm.[Status]='In-Progress' or prgrm.[Status]='Complete')
									where pln.IsDeleted=0
								 )
								 UNION
								 (
									-- Get Campaign list.
									Select Null as PlanTacticId,
										Null as PlanProgramId,
										campgn.PlanCampaignId,
										Null as LinkedTacticId ,
										Null as LinkedPlanId,
										pln.[Year] as PlanYear,
										@cmpgn as ObjectType,
										RN= 1
									from [Plan] as pln 
									INNER JOIN Plan_Campaign as campgn ON campgn.PlanId = pln.PlanId and campgn.IsDeleted=0 and campgn.PlanCampaignId=@id and campgn.IsDeployedToIntegration='1' and (campgn.[Status]='Approved' or campgn.[Status]='In-Progress' or campgn.[Status]='Complete')
									where pln.IsDeleted=0
								 )
							)
							Insert into @tblTaclist select * from tactList WHERE RN = 1;

					--select * from @tblTaclist
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH

					-- Get Integration Instance Id based on Program Id.
					SELECT @instanceId=mdl.IntegrationInstanceId
					FROM [Model] as mdl
					INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
					INNER JOIN [Plan_Campaign] as cmpgn ON pln.PlanId = cmpgn.PlanId and cmpgn.IsDeleted=0 and cmpgn.PlanCampaignId=@id

					-- END: Get Tactic Data by Instance Id
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Data by Campaign Id')
		END
		ELSE IF(UPPER(@entityType) = UPPER(@entityTypeImprvTac))
		BEGIN
			-- Get Improvement TacticIds
			BEGIN
				Insert into @tblImprvEntity
				Select Imprvtact.ImprovementPlanTacticId,
					   Imprvtact.ImprovementPlanProgramId,
					   Imprvcampgn.ImprovementPlanCampaignId,
					   @entityTypeImprvTac as ObjectType
				from Plan_Improvement_Campaign as Imprvcampgn
				INNER JOIN Plan_Improvement_Campaign_Program as Imprvprgrm on Imprvcampgn.ImprovementPlanCampaignId = Imprvprgrm.ImprovementPlanCampaignId 
				INNER JOIN Plan_Improvement_Campaign_Program_Tactic as Imprvtact on Imprvprgrm.ImprovementPlanProgramId = Imprvtact.ImprovementPlanProgramId and Imprvtact.ImprovementPlanTacticId=@id and Imprvtact.IsDeleted=0 and Imprvtact.IsDeployedToIntegration='1'and (Imprvtact.[Status]='Approved' or Imprvtact.[Status]='In-Progress' or Imprvtact.[Status]='Complete')
				
				-- Get Integration Instance Id based on Tactic Id.
				SELECT @instanceId=mdl.IntegrationInstanceId
				FROM [Model] as mdl
				INNER JOIN [Plan] as pln ON mdl.ModelId = pln.ModelId and pln.IsDeleted=0
				INNER JOIN [Plan_Improvement_Campaign] as Imprvcmpgn ON pln.PlanId = Imprvcmpgn.ImprovePlanId
				INNER JOIN [Plan_Improvement_Campaign_Program] as ImprvPrg ON Imprvcmpgn.ImprovementPlanCampaignId = ImprvPrg.ImprovementPlanCampaignId 
				INNER JOIN @tblImprvEntity as ImprvTac ON ImprvPrg.ImprovementPlanProgramId = ImprvTac.ImprovementPlanProgramId
			END
		END
		Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Identify EnityType Tactic or Integration Instance')

		-- START: Get IntegrationTypeId
		IF(@instanceId>0)
		BEGIN
			SELECT @integrationTypeId=IntegrationTypeId,@isCustomNameAllow=CustomNamingPermission from IntegrationInstance where IntegrationInstanceId=@instanceId
		END
		-- END: Get IntegrationTypeId

		-- START: Get list of Programs not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
						   prg.PlanProgramId,
						   prg.PlanCampaignId,
							Null as LinkedTacticId ,
							Null as LinkedPlanId,
							tac.PlanYear,
							@prg as ObjectType,
							RN= 1
			from @tblTaclist as tac
			INNER Join Plan_Campaign_Program as prg on tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0 and IsNull(prg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblTaclist 
			select Null as PlanTacticId,
				   Null as PlanProgramId,
				   cmpgn.PlanCampaignId,
				   Null as LinkedTacticId ,
				   Null as LinkedPlanId,
				   tac.PlanYear,
				   @cmpgn as ObjectType,
				   RN= 1
			from @tblTaclist as tac
			INNER join Plan_Campaign as cmpgn on tac.PlanCampaignId = cmpgn.PlanCampaignId and cmpgn.IsDeleted=0 and IsNull(cmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Campaign_Program_Tactic as t on tac.PlanTacticId=t.PlanTacticId and (IsNull(t.IntegrationInstanceTacticId,'')='')
			where tac.ObjectType=@tact
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: Add list of Improvement Programs not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Imprvprg.ImprovementPlanProgramId,
						   Imprvprg.ImprovementPlanCampaignId,
							@entityTypeImprvProg as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign_Program as Imprvprg on Imprvtac.ImprovementPlanProgramId = Imprvprg.ImprovementPlanProgramId and IsNull(Imprvprg.IntegrationInstanceProgramId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Programs not pushed into SFDC.


		-- START: Get list of Improvement Campaigns not pushed into SFDC.
		BEGIN
			Insert into @tblImprvEntity 
			select Null as ImprovementPlanTacticId,
						   Null as ImprovementPlanProgramId,
						   ImprvCmpgn.ImprovementPlanCampaignId,
							@entityTypeImprvCamp as ObjectType
			from @tblImprvEntity as Imprvtac
			INNER Join Plan_Improvement_Campaign as ImprvCmpgn on Imprvtac.ImprovementPlanCampaignId = ImprvCmpgn.ImprovementPlanCampaignId and IsNull(ImprvCmpgn.IntegrationInstanceCampaignId,'') =''
			INNER JOIN Plan_Improvement_Campaign_Program_Tactic as IT on Imprvtac.ImprovementPlanTacticId=IT.ImprovementPlanTacticId and (IsNull(IT.IntegrationInstanceTacticId,'')='')
			where Imprvtac.ObjectType=@entityTypeImprvTac 
		END
		-- END: Get list of Campaigns not pushed into SFDC.


		-- START: GET result data based on Mapping fields
		BEGIN
			IF (EXISTS(Select 1 from @tblTaclist)) OR (EXISTS(Select 1 from @tblImprvEntity))
			-- Identify that Data Exist or Not
			BEGIN
				IF EXISTS(Select IntegrationInstanceDataTypeMappingId from IntegrationInstanceDataTypeMapping where IntegrationInstanceId=@instanceId)
				BEGIN
					DECLARE @DynamicPivotQuery AS NVARCHAR(MAX) =''
					DECLARE @ColumnName AS NVARCHAR(MAX) =''
					Declare @updIds varchar(max)=''

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get comma separated column names')
					
					BEGIN TRY
						-- Get comma separated  mapping fields name as columns of Campaign,Program,Tactic & Improvement Campaign,Program & Tactic 
						select  @ColumnName = dbo.GetSFDCTacticResultColumns(@instanceId,@clientId,@integrationTypeId)
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
					END CATCH
										
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get comma separated column names')	
					
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Get Tactic Ids')
					
					-- START: Get TacticIds
					SELECT @entTacIds= ISNULL(@entTacIds + ',','') + (PlanTacticId1)
					FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
					-- END: Get TacticIds

					-- START: Get Campaign Ids
					SELECT @entCmpgnIds= ISNULL(@entCmpgnIds + ',','') + (PlanCampgnId1)
					FROM (Select DISTINCT Cast (PlanCampaignId as varchar(max)) PlanCampgnId1 FROM @tblTaclist where ObjectType=@cmpgn) AS PlanCampaignIds
					-- END: Get Campaign Ids

					-- START: Get Program Ids
					SELECT @entPrgIds= ISNULL(@entPrgIds + ',','') + (PlanPrgrmId1)
					FROM (Select DISTINCT Cast (PlanProgramId as varchar(max)) PlanPrgrmId1 FROM @tblTaclist where ObjectType=@prg) AS PlanProgramIds
					-- END: Get Program Ids

					-- Get Improvement Ids
					BEGIN
						-- START: Get ImprvmntTacticIds
						SELECT @entImrvmntTacIds = ISNULL(@entImrvmntTacIds  + ',','') + (ImprvTac)
						FROM (Select DISTINCT Cast (ImprovementPlanTacticId as varchar(max)) ImprvTac FROM @tblImprvEntity where ObjectType=@entityTypeImprvTac) AS PlanTacticIds
						-- END: Get ImprvmntTacticIds

						-- START: Get ImprvmntCampaign Ids
						SELECT @entImrvmntCmpgnIds = ISNULL(@entImrvmntCmpgnIds  + ',','') + (ImprvCampgn)
						FROM (Select DISTINCT Cast (ImprovementPlanCampaignId as varchar(max)) ImprvCampgn FROM @tblImprvEntity where ObjectType=@entityTypeImprvCamp) AS PlanCampaignIds
						-- END: Get ImprvmntCampaign Ids

						-- START: Get ImprvmntProgram Ids
						SELECT @entImrvmntPrgIds= ISNULL(@entImrvmntPrgIds + ',','') + (ImprvPrgrm)
						FROM (Select DISTINCT Cast (ImprovementPlanProgramId as varchar(max)) ImprvPrgrm FROM @tblImprvEntity where ObjectType=@entityTypeImprvProg) AS PlanProgramIds
						-- END: Get ImprvmntProgram Ids
					END
					
					-- START: IF Client & Instance has CustomName permission then generate customname for all required tactics
					IF(IsNull(@isCustomNameAllow,'0')='1' AND IsNull(@isClientAllowCustomName,'0')='1')
					BEGIN
						----- START: Get Updte CustomName TacIds -----
						SELECT @updIds= ISNULL(@updIds + ',','') + (PlanTacticId1)
						FROM (Select DISTINCT Cast (PlanTacticId as varchar(max)) PlanTacticId1 FROM @tblTaclist where ObjectType=@tact) AS PlanTacticIds
						----- END: Get Updte CustomName TacIds -----
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Get Tactic Ids')

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Update Tactic CustomName')

						BEGIN TRY
							-- START: Update Tactic Name --
							UPDATE Plan_Campaign_Program_Tactic 
							SET TacticCustomName = T1.CustomName 
							FROM GetTacCustomNameMappingList('Tactic',@clientId,@updIds) as T1 
							INNER JOIN Plan_Campaign_Program_Tactic as T2 ON T1.PlanTacticId = T2.PlanTacticId and IsNull(T2.TacticCustomName,'')=''
							-- END: Update Tactic Name --
						END TRY
						BEGIN CATCH
							Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()))	
						END CATCH

						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Update Tactic CustomName')
					END

					--SELECT * from  [GetSFDCSourceTargetMappingData]('Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',N'101371',2,1203,255,0,0)

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logStart+@logSP+'Create final result Pivot Query')

					BEGIN TRY
						SET @dynResultQuery ='SELECT distinct SourceId,SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,'+@ColumnName+' 
																FROM (
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @tact +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @prg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @cmpgn +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvCamp +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntCmpgnIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvProg +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntPrgIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																		UNION
																		(
																			SELECT ActualFieldName,TacValue,SourceId 
																			FROM [GetSFDCSourceTargetMappingData]('''+ @entityTypeImprvTac +''','''+ CAST(@clientId AS NVARCHAR(36)) +''','''+ @entImrvmntTacIds +''','''+ CAST(@integrationTypeId AS NVARCHAR) +''','''+ CAST(@instanceId AS NVARCHAR) +''','''+ CAST(@SFDCTitleLengthLimit AS NVARCHAR) +''','''+ CAST(@isCustomNameAllow AS NVARCHAR) +''','''+ CAST(@isClientAllowCustomName AS NVARCHAR) +''')
																		)
																	) as R
						    PIVOT(
								  MIN(TacValue)
						          FOR ActualFieldName IN (SourceParentId,cParentId,IsSyncedSFDC,SalesforceId,ObjectType,Mode,'+@ColumnName+')
								 ) AS PVTTable
								 '
								 -- IF Marketo-SFDC 3way data exist then get marketo related tactics by below function.
								 IF(@isSyncSFDCWithMarketo='1')
								 BEGIN

									select @query = query from [fnGetSalesforceMarketo3WayData](@entityType,@id,@clientId,@SFDCTitleLengthLimit,@integrationInstanceLogId,@isClientAllowCustomName) as T1
									--select @query
									IF(ISNULL(@dynResultQuery,'')<>'')
									BEGIN
										IF(ISNULL(@query,'')<>'')
										BEGIN
											SET @dynResultQuery = @dynResultQuery + ' UNION ' + @query
										END
									END
									ELSE IF(ISNULL(@query,'')<>'')
									BEGIN
										SET @dynResultQuery = @query
									END

								 END
					END TRY
					BEGIN CATCH
						Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logError+@logSP+'Exception throw while executing query:'+ (SELECT 'Error Code.- '+ Cast(ERROR_NUMBER() as varchar(255))+',Error Line No-'+Cast(ERROR_LINE()as varchar(255))+',Error Msg-'+ERROR_MESSAGE()+',SQL Query-'+ (Select @dynResultQuery)))	
					END CATCH

					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logEnd+@logSP+'Create final result Pivot Query')
					--PRINT @dynResultQuery  
					--Execute the Dynamic Pivot Query
					--EXEC sp_executesql @dynResultQuery
					
				END
				ELSE
				BEGIN
					Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'No single field mapped for Salesforce instance')
				END
			END
			ELSE IF(@isSyncSFDCWithMarketo='1')
			BEGIN
				
				select @query = query from [fnGetSalesforceMarketo3WayData](@entityType,@id,@clientId,@SFDCTitleLengthLimit,@integrationInstanceLogId,@isClientAllowCustomName) as T1
				--select @query
				IF(ISNULL(@query,'')<>'')
				BEGIN
					SET @dynResultQuery = @query
				END
			END
			ELSE
			BEGIN
				Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Data does not exist')
			END
		END
		-- END: GET result data based on Mapping fields

	END
	-- END
	Insert Into IntegrationInstanceLogDetails Values(@id,@integrationInstanceLogId,GETDATE(),@logInfo+@logSP+'Get final result data to push Salesforce')
	EXEC(@dynResultQuery)
	--select * from @tblSyncError
	--SELECT @logStartInstanceLogId as 'InstanceLogStartId'
END

GO

--- END: PL ticket #2251 related SPs & Functions --------------------

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc :: Add is owner column to the table.
IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'IsOwner' AND OBJECT_ID = OBJECT_ID(N'[Budget_Permission]'))
BEGIN
ALTER TABLE [dbo].[Budget_Permission]
ADD [IsOwner] [bit] NOT NULL CONSTRAINT [DF_Budget_Permission_IsOwner]  DEFAULT 0
END 
Go

-- Added by Komal Rawal
-- Added on :: 13-June-2016
-- Desc ::update isowner flag for existing budget.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Budget_Permission]') AND type in (N'U') )
BEGIN
	Declare @cntOwner int=0
	select @cntOwner = Count(*) FROM  [dbo].[Budget_Permission] where IsOwner = 1
	IF(@cntOwner=0)
	BEGIN
		UPDATE [dbo].[Budget_Permission]
		SET IsOwner = 1 WHERE UserId = CreatedBy 
	END
END 
Go
-- Added by Viral Kadiya
-- Added on :: 17-June-2016
-- Desc :: Insert Campaign,Program,Tactic & Improvement Tactic Comment.

/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
GO
/****** Object:  StoredProcedure [dbo].[UpdateTacticInstanceTacticId_Comment_API]    Script Date: 06/21/2016 1:27:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateTacticInstanceTacticId_Comment_API]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API] AS' 
END
GO

ALTER PROCEDURE [dbo].[UpdateTacticInstanceTacticId_Comment_API]
	@strCreatedTacIds nvarchar(max)='',
	@strUpdatedTacIds nvarchar(max)='',
	@strCrtCampaignIds nvarchar(max)='',
	@strUpdCampaignIds nvarchar(max)='',
	@strCrtProgramIds nvarchar(max)='',
	@strUpdProgramIds nvarchar(max)='',
	@strCrtImprvmntTacIds nvarchar(max)='',
	@strUpdImprvmntTacIds nvarchar(max)='',
	@isAutoSync bit='0',
	@userId uniqueidentifier,
	@integrationType varchar(100)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @instanceTypeMarketo varchar(50)='Marketo'
	Declare @instanceTypeSalesforce varchar(50)='Salesforce'
	Declare @AttrType varchar(50)='MarketoUrl'
	Declare @entType varchar(20)='Tactic'
	Declare @strAllPlanTacIds nvarchar(max)=''

	-- Start:- Declare comment related variables for each required entity.
	Declare @TacticSyncedComment varchar(500) = 'Tactic synced with ' + @integrationType
	Declare @TacticUpdatedComment varchar(500) = 'Tactic updated with ' + @integrationType
	Declare @ProgramSyncedComment varchar(500) = 'Program synced with ' + @integrationType
	Declare @ProgramUpdatedComment varchar(500) = 'Program updated with ' + @integrationType
	Declare @CampaignSyncedComment varchar(500) = 'Campaign synced with ' + @integrationType
	Declare @CampaignUpdatedComment varchar(500) = 'Campaign updated with ' + @integrationType
	Declare @ImprovementTacticSyncedComment varchar(500) = 'Improvement Tactic synced with ' + @integrationType
	Declare @ImprovementTacticUpdatedComment varchar(500) = 'Improvement Tactic updated with ' + @integrationType
	-- End:- Declare comment related variables for each required entity.

	IF(@strCreatedTacIds<>'')
	BEGIN
		SET @strAllPlanTacIds = @strCreatedTacIds
	END
	IF(@strUpdatedTacIds<>'')
	BEGIN
		IF(@strAllPlanTacIds<>'')
		BEGIN
			SET @strAllPlanTacIds = @strAllPlanTacIds+','+@strUpdatedTacIds
		END
		ELSE
		BEGIN
			SET @strAllPlanTacIds = @strUpdatedTacIds
		END
	END

	IF(@integrationType = @instanceTypeSalesforce)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceTacticId=tac1.IntegrationInstanceTacticId,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))
	END
	ELSE IF(@integrationType = @instanceTypeMarketo)
	BEGIN
		-- update IntegrationInstanceTacticId for linked tactic 
		Update  tac2 set tac2.IntegrationInstanceMarketoID=tac1.IntegrationInstanceMarketoID,tac2.TacticCustomName=tac1.TacticCustomName,tac2.LastSyncDate=tac1.LastSyncDate,tac2.ModifiedDate = tac1.ModifiedDate,tac2.ModifiedBy = tac1.ModifiedBy from Plan_Campaign_Program_Tactic tac1
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Update Marketo URL for Linked tactic
		Update lnkEnt set lnkEnt.AttrValue=orgEnt.AttrValue from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		INNER JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Marketo URL for Linked tactic
		INSERT INTO EntityIntegration_Attribute(EntityId,EntityType,IntegrationinstanceId,AttrType,AttrValue,CreatedDate) 
		SELECT tac1.LinkedTacticId,@entType,orgEnt.IntegrationinstanceId,orgEnt.AttrType,orgEnt.AttrValue,GETDATE()
		from Plan_Campaign_Program_Tactic as tac1
		INNER JOIN EntityIntegration_Attribute as orgEnt on tac1.PlanTacticId = orgEnt.EntityId and orgEnt.EntityType=@entType and orgEnt.AttrType=@AttrType
		LEFT JOIN EntityIntegration_Attribute as lnkEnt on tac1.LinkedTacticId=lnkEnt.EntityId and lnkEnt.EntityType=@entType and lnkEnt.AttrType=@AttrType
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ',')) and lnkEnt.EntityId IS NULL AND tac1.LinkedTacticId > 0

	END

	IF(@isAutoSync =0)
	BEGIN
		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
		
		Create Table #tmp_Plan_Campaign_Program_Tactic_Comment(CommentId int,Tacticid int)
		
		-- Insert comment for PlanTactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		OUTPUT inserted.PlanTacticCommentId,inserted.PlanTacticId into #tmp_Plan_Campaign_Program_Tactic_Comment
		SElect PlanTacticId,@TacticSyncedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCreatedTacIds, ','))
		UNION
		SElect PlanTacticId,@TacticUpdatedComment,GETDATE(),@userId,null,null from Plan_Campaign_Program_Tactic where PlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdatedTacIds, ','))
		
		-- Insert comment for linked Tactic
		Insert Into Plan_Campaign_Program_Tactic_Comment
		Select tac2.PlanTacticId,cmnt.Comment,cmnt.CreatedDate,cmnt.CreatedBy,cmnt.PlanProgramId,cmnt.PlanCampaignId from #tmp_Plan_Campaign_Program_Tactic_Comment as tmpComment
		join Plan_Campaign_Program_Tactic tac1 on tac1.PlanTacticId = tmpComment.TacticId
		join Plan_Campaign_Program_Tactic tac2 on tac1.LinkedTacticId=tac2.PlanTacticId 
		join Plan_Campaign_Program_Tactic_Comment as cmnt on tmpComment.CommentId = cmnt.PlanTacticCommentId and tmpComment.TacticId = cmnt.PlanTacticId
		where tac1.PlanTacticId IN (Select cast(val as int) from dbo.[comma_split](@strAllPlanTacIds, ','))

		-- Insert Comment for Plan Campaign
		IF( (@strCrtCampaignIds <>'') OR (@strUpdCampaignIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@CampaignSyncedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strCrtCampaignIds, ','))
			UNION
			SELECT null,@CampaignUpdatedComment,GETDATE(),@userId,null,PlanCampaignId from Plan_Campaign where PlanCampaignId In (Select cast(val as int) from dbo.[comma_split](@strUpdCampaignIds, ','))
		END

		-- Insert Comment for Plan Program
		IF( (@strCrtProgramIds <>'') OR (@strUpdProgramIds <>'') )
		BEGIN
			INSERT Into Plan_Campaign_Program_Tactic_Comment
			SELECT null,@ProgramSyncedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strCrtProgramIds, ','))
			UNION
			SELECT null,@ProgramUpdatedComment,GETDATE(),@userId,PlanProgramId,null from Plan_Campaign_Program where PlanProgramId In (Select cast(val as int) from dbo.[comma_split](@strUpdProgramIds, ','))
		END

		-- Insert Comment for Improvement Tactic
		IF( (@strCrtImprvmntTacIds <>'') OR (@strUpdImprvmntTacIds <>'') )
		BEGIN
			INSERT Into Plan_Improvement_Campaign_Program_Tactic_Comment
			SELECT ImprovementPlanTacticId,@ImprovementTacticSyncedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strCrtImprvmntTacIds, ','))
			UNION
			SELECT ImprovementPlanTacticId,@ImprovementTacticUpdatedComment,GETDATE(),@userId from Plan_Improvement_Campaign_Program_Tactic where ImprovementPlanTacticId In (Select cast(val as int) from dbo.[comma_split](@strUpdImprvmntTacIds, ','))
		END

		IF OBJECT_ID('tempdb..#tmp_Plan_Campaign_Program_Tactic_Comment') IS NOT NULL 
		BEGIN
			DROP TABLE #tmp_Plan_Campaign_Program_Tactic_Comment
		END
	END
    
END

GO

--===========================================================================================================================================
/* Start - Added by Arpita Soni for Ticket #2279 on 06/21/2016 */

-- Add column PlanId into IntegrationWorkFrontPortfolios table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'PlanId' AND [object_id] = OBJECT_ID(N'IntegrationWorkFrontPortfolios'))
BEGIN
	ALTER TABLE dbo.IntegrationWorkFrontPortfolios ADD PlanId INT NULL
END
GO

-- Alter column PlanProgramId into IntegrationWorkFrontPortfolios table
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'PlanProgramId' AND [object_id] = OBJECT_ID(N'IntegrationWorkFrontPortfolios'))
BEGIN
	ALTER TABLE dbo.IntegrationWorkFrontPortfolios ALTER COLUMN PlanProgramId INT NULL
END
GO

-- Add column IntegrationWorkFrontProgramID into Plan_Campaign table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'IntegrationWorkFrontProgramID' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign ADD IntegrationWorkFrontProgramID NVARCHAR(50) NULL
END
GO

-- Create table IntegrationWorkFrontPortfolio_Mapping
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationWorkFrontProgram_Mapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PortfolioTableId] [int] NOT NULL,
	[ProgramId] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontProgram_Mapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

-- Create FK on PortfolioTableId column in table IntegrationWorkFrontPortfolio_Mapping  
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Program_PortfolioTableId]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]'))
ALTER TABLE [dbo].[IntegrationWorkFrontProgram_Mapping]  WITH CHECK ADD  CONSTRAINT FK_Program_PortfolioTableId FOREIGN KEY([PortfolioTableId])
REFERENCES [dbo].[IntegrationWorkFrontPortfolios] ([Id])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Program_PortfolioTableId]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationWorkFrontProgram_Mapping]'))
ALTER TABLE [dbo].[IntegrationWorkFrontProgram_Mapping] CHECK CONSTRAINT FK_Program_PortfolioTableId
GO

/* End - Added by Arpita Soni for Ticket #2279 on 06/21/2016 */
--===========================================================================================================================================

/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/24/2016 1:53:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSFDCFieldMappings]
GO
/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetSFDCFieldMappings] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetSFDCFieldMappings] 
	@clientId uniqueidentifier,
	@integrationTypeId int,
	@id int=0,
	@isSFDCMarketoIntegration bit='0'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Exec GetSFDCFieldMappings '464EB808-AD1F-4481-9365-6AADA15023BD',2,1203

BEGIN
	Declare @Table TABLE (sourceFieldName NVARCHAR(250),destinationFieldName NVARCHAR(250),fieldType varchar(255))
	Declare @ColumnName nvarchar(max)
	Declare @trgtCampaignFolder varchar(255)='Id'
	Declare @actMode varchar(255)='Mode'
	Declare @trgtMode varchar(255)=''
	Declare @modeCREATE varchar(20)='Create'
	Declare @modeUPDATE varchar(20)='Update'
	Declare @actCost varchar(30)='Cost'
	Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'
	Declare @tblProgram varchar(255)='Plan_Campaign_Program'
	Declare @tblCampaign varchar(255)='Plan_Campaign'
	Declare @tblImprvTactic varchar(255)='Plan_Improvement_Campaign_Program_Tactic'
	Declare @varGlobal varchar(100)='Global'
	Declare @varGlobalImprovemnt varchar(100)='GlobalImprovement'
	Declare @actActivityType varchar(255)='ActivityType'
	Declare @actCreatedBy varchar(255)='CreatedBy'
	Declare @actPlanName varchar(255)='PlanName'
	Declare @actStatus varchar(255)='Status'
	Declare @entityTypeTac varchar(255)='Tactic'
	Declare @entityTypeProg varchar(255)='Program'
	Declare @entityTypeCampgn varchar(255)='Campaign'
	Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
	Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
	Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
	Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
	Declare @actsfdcParentId varchar(50)='ParentId'
END

;With ResultTable as(

(
		-- Add Campaign,Program, Tactic, Improvement Campaign,Program & Tactic Or Global fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				CASE 
					WHEN gpDataType.TableName=@tblTactic THEN @entityTypeTac
					WHEN gpDataType.TableName=@tblProgram THEN @entityTypeProg
					WHEN gpDataType.TableName=@tblCampaign THEN @entityTypeCampgn
					WHEN gpDataType.TableName=@tblImprvTactic THEN @entityTypeImprvTac
					ELSE @varGlobal
				END as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@tblTactic OR gpDataType.TableName=@tblProgram OR gpDataType.TableName=@tblCampaign OR gpDataType.TableName=@tblImprvTactic OR gpDataType.TableName=@varGlobal) and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- Add Improvement Global Fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				 @varGlobalImprovemnt as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@varGlobal and IsImprovement='1' and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- CustomField Query
		SELECT  custm.Name as sourceFieldName,
				TargetDataType as destinationFieldName,
				custm.EntityType as fieldType
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and ((custm.EntityType=@entityTypeTac) OR (custm.EntityType=@entityTypeProg) OR (custm.EntityType=@entityTypeCampgn))
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
--IF(@isSFDCMarketoIntegration='1')
--BEGIN
--	-- Insert ParentId field for Campaign,Program & Tactic
--	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobal as fieldType

--	-- Insert ParentId field for Improvement Campaign,Program & Tactic
--	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobalImprovemnt as fieldType
--END
select * from @Table
END


GO

/* Start- Added by Arpita Soni for Ticket #2304 - Workfront Integration */
DECLARE @ID INT = 1
DECLARE @TmpTable TABLE(
	Id INT IDENTITY,
	IntegrationTypeId INT
)
INSERT INTO @TmpTable
SELECT IntegrationTypeId FROM IntegrationType WHERE Code='workfront'

WHILE (@ID<=(SELECT MAX(ID) FROM @TmpTable))
BEGIN
	IF NOT EXISTS(SELECT * FROM GameplanDataType WHERE ActualFieldName='ProgramName' AND IntegrationTypeId = (SELECT IntegrationTypeId FROM @TmpTable WHERE Id = @ID))
	BEGIN
		INSERT INTO GameplanDataType 
		SELECT (SELECT IntegrationTypeId FROM @TmpTable WHERE Id = @ID),'Plan_Campaign_Program','ProgramName','Program Name',0,0,0
	END
	SET @ID = @ID+1
END 
GO
/* End - Added by Arpita Soni for Ticket #2304 - Workfront Integration */

GO
IF NOT EXISTS (SELECT * FROM AggregationStatus)
BEGIN
	INSERT INTO AggregationStatus VALUES ('NOTRUNNING')
END
GO
