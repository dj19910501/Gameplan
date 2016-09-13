/****** Object:  UserDefinedFunction [dbo].[fnGetEntitieHirarchyByPlanId]    Script Date: 09/13/2016 12:55:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'--This function will return all the enties with hirarchy
--Multiple plan ids can be passed saperated by comma
--If we pass null then it will retuen all plans hirarchy data
CREATE FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId] ( @PlanIds NVARCHAR(MAX))
RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		UNIQUEIDENTIFIER
		)
AS
BEGIN

	;WITH FilteredPlan AS(
		SELECT ''Plan'' EntityType,''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId,P.PlanId EntityId, P.Title EntityTitle,NULL ParentEntityId,NULL ParentUniqueId, P.Status, NULL StartDate, NULL EndDate,P.CreatedBy FROM [Plan] P 
			--INNER JOIN Model M ON M.ModelId = P.ModelId AND M.ClientId = @ClientId
		WHERE P.IsDeleted = 0 
			AND (
					@PlanIds IS NULL 
					OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
				)
	),
	Campaigns AS (
		SELECT ''Campaign'' EntityType,''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId,C.PlanCampaignId EntityId, C.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, C.Status, C.StartDate StartDate, C.EndDate EndDate,C.CreatedBy FROM Plan_Campaign C
			INNER JOIN FilteredPlan P ON P.EntityId = C.PlanId 
		WHERE C.IsDeleted = 0 
	),
	Programs AS (
		SELECT ''Program'' EntityType,''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy FROM Plan_Campaign_Program P
			INNER JOIN Campaigns C ON C.EntityId = P.PlanCampaignId
		WHERE P.IsDeleted = 0 
	),
	Tactics AS (
		SELECT ''Tactic'' EntityType,''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Programs P ON P.EntityId = T.PlanProgramId
		WHERE T.IsDeleted = 0 
	),
	LineItems AS (
		SELECT ''LineItem'' EntityType,''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId, NULL Status, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem L
			INNER JOIN Tactics T ON T.EntityId = L.PlanTacticId
		WHERE L.IsDeleted = 0 
	),
	AllEntities AS (    
		SELECT * FROM FilteredPlan UNION ALL
		SELECT * FROM Campaigns UNION ALL
		SELECT * FROM Programs UNION ALL
		SELECT * FROM Tactics UNION ALL
		SELECT * FROM LineItems
	)
	INSERT INTO @Entities (UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,Status,StartDate,EndDate,CreatedBy)
	SELECT E.UniqueId, E.EntityId,E.EntityTitle, E.ParentEntityId,E.ParentUniqueId,E.EntityType, C.ColorCode,E.Status,E.StartDate,E.EndDate,E.CreatedBy FROM AllEntities E
	LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType

	RETURN
END


' 
END

GO
