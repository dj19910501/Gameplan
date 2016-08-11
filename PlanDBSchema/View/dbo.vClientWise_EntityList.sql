/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/11/2016 10:56:10 AM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
DROP VIEW [dbo].[vClientWise_EntityList]
GO
/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/11/2016 10:56:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, ''Plan'' Entity, 1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, ''Campaign'' Entity, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, ''Program'' Entity, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, ''Tactic'' Entity, 4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in (''In-Progress'',''Approved'',''Complete'')
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, ''LineItem'' Entity, 5 EntityOrder 
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

' 
GO
