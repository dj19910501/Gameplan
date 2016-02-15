
DECLARE @ClientId UNIQUEIDENTIFIER;

SET @ClientId = 'C251AB18-0683-4D1D-9F1E-06709D59FD53';  -- Set ClientId for Zebra.

IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Campaign_Program_Tactic_BKP_15Feb2015'))
BEGIN
   SELECT * INTO Plan_Campaign_Program_Tactic_BKP_15Feb2015 FROM Plan_Campaign_Program_Tactic;
END

;WITH RawTable AS (
	SELECT 
		T.PlanTacticId, T.Title Tactic, T.IsDeployedToIntegration, T.IsSyncSalesForce, T.IsSyncEloqua,M.IntegrationInstanceId,M.IntegrationInstanceEloquaId
	FROM Plan_Campaign_Program_Tactic T
		INNER JOIN Plan_Campaign_Program PP ON PP.PlanProgramId = T.PlanProgramId AND PP.IsDeleted = 0
		INNER JOIN Plan_Campaign C ON C.PlanCampaignId = PP.PlanCampaignId AND C.IsDeleted = 0
		INNER JOIN [Plan] P ON P.PlanId = C.PlanId AND P.IsDeleted = 0 AND P.Status = 'Published'
		INNER JOIN Model M ON M.ModelId = P.ModelId AND M.IsDeleted = 0 AND (M.IntegrationInstanceId IS NOT NULL OR M.IntegrationInstanceEloquaId IS NOT NULL) AND M.ClientId = @ClientId
	WHERE T.IsDeleted = 0
	AND T.IsDeployedToIntegration = 1
	AND T.Status IN ('In-Progress','Complete','Approved')
)
UPDATE Plan_Campaign_Program_Tactic 
	SET 
		IsSyncSalesForce	= ISNULL(RawTable.IntegrationInstanceId,0),
		IsSyncEloqua		= ISNULL(RawTable.IntegrationInstanceEloquaId,0)
FROM Plan_Campaign_Program_Tactic T
INNER JOIN RawTable ON RawTable.PlanTacticId = T.PlanTacticId
