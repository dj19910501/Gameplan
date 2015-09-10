/* ------------- Start: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Campaign_Program_Tactic  ------------- */

UPDATE src set IntegrationInstanceTacticId=NULL,IntegrationInstanceEloquaId = elq.IntegrationInstanceTacticId FROM Plan_Campaign_Program_Tactic as src
INNER JOIN Plan_Campaign_Program_Tactic as elq on src.PlanTacticId = elq.PlanTacticId AND ISNUMERIC(elq.IntegrationInstanceTacticId)=1 AND (elq.IntegrationInstanceTacticId  IS NOT NULL)

/* ------------- End: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Campaign_Program_Tactic  ------------- */

/* ------------- Start: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Improvement_Campaign_Program_Tactic  ------------- */

UPDATE src set IntegrationInstanceTacticId=NULL,IntegrationInstanceEloquaId = elq.IntegrationInstanceTacticId FROM Plan_Improvement_Campaign_Program_Tactic as src
INNER JOIN Plan_Improvement_Campaign_Program_Tactic as elq on src.ImprovementPlanTacticId = elq.ImprovementPlanTacticId AND ISNUMERIC(elq.IntegrationInstanceTacticId)=1 AND (elq.IntegrationInstanceTacticId  IS NOT NULL)

/* ------------- End: Move EloquaId from IntegrationInstanceTacticId to IntegrationInstanceEloquaID into column Plan_Improvement_Campaign_Program_Tactic  ------------- */
