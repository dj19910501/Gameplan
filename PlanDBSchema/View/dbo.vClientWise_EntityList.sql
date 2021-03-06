IF EXISTS (SELECT 1 FROM sys.views WHERE OBJECT_ID=OBJECT_ID('vClientWise_EntityList'))
BEGIN
	DROP view vClientWise_EntityList
END
GO

CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, 'Plan' Entity,P.CreatedDate,P.Title as PlanTitle, P.PlanId As PlanId  ,1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, 'Campaign' Entity,P.CreatedDate,C.PlanTitle as PlanTitle, c.PlanId As PlanId, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, 'Program' Entity,P.CreatedDate, C.PlanTitle as PlanTitle, c.PlanId As PlanId, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllLinkedTactic as
(
SELECT P.LinkedTacticId 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and P.LinkedTacticId is not null
	   and (DATEPART(year,P.EndDate)-DATEPART(year,P.StartDate))>0
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, 'Tactic' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
			  LEFT OUTER JOIN AllLinkedTactic L on P.PlanTacticId=L.LinkedTacticId
       WHERE P.IsDeleted = 0 and P.Status in ('In-Progress','Approved','Complete') and L.LinkedTacticId is null
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, 'Line Item' Entity,P.CreatedDate,C.PlanTitle as PlanTitle,  c.PlanId As PlanId,  5 EntityOrder 
       FROM Plan_Campaign_Program_Tactic_LineItem P
              INNER JOIN AllTactic C ON P.PlanTacticId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.LineItemTypeId is not null
)
SELECT * FROM AllPlans
UNION ALL 
SELECT * FROM AllCampaigns
UNION ALL 
SELECT * FROM AllProgram
UNION ALL 
SELECT * FROM AllTactic
UNION ALL 
SELECT * FROM AllLineitem

GO


