--Update Program
Update	Plan_Campaign_Program SET IsDeleted = 1
WHERE PlanCampaignId IN
(SELECT PlanCampaignId FROM Plan_Campaign WHERE IsDeleted = 1)

-- Update Tactic
Update	Plan_Campaign_Program_Tactic SET IsDeleted = 1
WHERE PlanProgramId IN 
(SELECT PlanProgramId FROM Plan_Campaign_Program WHERE IsDeleted = 1)

-- Update Line Item
Update	Plan_Campaign_Program_Tactic_LineItem SET IsDeleted = 1
WHERE PlanTacticId IN 
(SELECT PlanTacticId FROM Plan_Campaign_Program_Tactic WHERE IsDeleted = 1)
