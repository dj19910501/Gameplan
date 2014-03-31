GO
WITH Tactics AS (
SELECT DISTINCT
              T.PlanProgramId PlanProgramId1,
              INQSum = (SELECT SUM(T1.INQs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(T1.MQLs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              CostSum = (SELECT SUM(T1.Cost) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0)
FROM Plan_Campaign_Program_Tactic T
WHERE T.IsDeleted = 0
),
AllData AS(
SELECT 
              T.PlanProgramId1, 
              P.PlanProgramId, 
              INQSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              P.INQs, 
              P.MQLs, 
              P.Cost 
FROM Tactics T 
RIGHT JOIN Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId1 AND P.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign_Program p  RIGHT JOIN AllData T ON P.PlanProgramId = T.PlanProgramId AND P.IsDeleted = 0;



WITH Programs AS (
SELECT DISTINCT
              P.PlanCampaignId PlanCampaignId1,
              INQSum = (SELECT SUM(P1.INQs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(P1.MQLs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              CostSum = (SELECT SUM(P1.Cost) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0)
FROM Plan_Campaign_Program P
WHERE P.IsDeleted = 0
)
,
AllData AS(
SELECT 
              T.PlanCampaignId1, 
              C.PlanCampaignId, 
              INQSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              C.INQs, 
              C.MQLs, 
              C.Cost 
FROM Programs T 
RIGHT JOIN Plan_Campaign C ON C.PlanCampaignId = T.PlanCampaignId1 AND C.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign p  RIGHT JOIN AllData T ON P.PlanCampaignId = T.PlanCampaignId AND P.IsDeleted = 0;
