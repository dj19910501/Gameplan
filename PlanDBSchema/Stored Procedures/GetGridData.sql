IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridData] AS' 
END
GO
-- =============================================
-- Author:		Nishant Sheth
-- Create date: 09-Sep-2016
-- Description:	Get home grid data with custom field 19910781.11
-- =============================================
ALTER PROCEDURE GetGridData
	-- Add the parameters for the stored procedure here
	@PlanId NVARCHAR(MAX) = ''
	,@ClientId uniqueidentifier =''
	,@OwnerIds nvarchar(max)=''
	,@TacticTypeIds varchar(max)=''
	,@StatusIds varchar(max)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	DECLARE @StageMqlMaxLevel INT = 1
	DECLARE @StageRevenueMaxLevel INT = 1

	SELECT @StageMqlMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='MQL'

	SELECT @StageRevenueMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='CW'

	SELECT Hireachy.*,
				TacticType.AssetType,
				TacticType.Title AS TacticType,
				Tactic.TacticTypeId,
				LineItem.LineItemTypeId,
				LineItem.LineItemType,
				CASE WHEN EntityType = 'Tactic'
						THEN Tactic.Cost 
							WHEN EntityType = 'LineItem' 
								THEN LineItem.Cost
							WHEN EntityType='Program'
								THEN ProgramPlannedCost.PlannedCost
							WHEN EntityType='Campaign'
								THEN CampaignPlannedCost.PlannedCost
							WHEN EntityType='Plan'
								THEN PlanPlannedCost.PlannedCost
					END AS PlannedCost
				,Tactic.ProjectedStageValue 
				,Stage.Title AS 'ProjectedStage'
				,MQL.Value as MQL
				,Revenue.Value as Revenue
				,Tactic.TacticCustomName AS 'MachineName'
				,Tactic.LinkedPlanId
				,Tactic.LinkedTacticId
				,P.PlanName AS 'LinkedPlanName'
				,ROI.AnchorTacticID
				,(SELECT SUBSTRING((	
						SELECT ',' + CAST(PlanTacticId AS VARCHAR) FROM ROI_PackageDetail R
						WHERE ROI.AnchorTacticID = R.AnchorTacticID
						FOR XML PATH('')), 2,900000
					))AS PackageTacticIds
				FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
	OUTER APPLY (SELECT M.AverageDealSize FROM Model M WITH (NOLOCK)
					WHERE M.IsDeleted = 0
							AND Hireachy.ModelId = M.ModelId
							) M
				--FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds) Hireachy 
	OUTER APPLY (SELECT Tactic.PlanTacticId,
						Tactic.TacticTypeId,
						Tactic.Cost,
						Tactic.StageId,
						Tactic.ProjectedStageValue,
						Tactic.PlanProgramId,
						Tactic.LinkedPlanId,
						Tactic.LinkedTacticId,
						Tactic.TacticCustomName
						FROM Plan_Campaign_Program_Tactic Tactic WITH (NOLOCK)
							WHERE Hireachy.EntityType='Tactic'
						AND Hireachy.EntityId = Tactic.PlanTacticId) Tactic
	OUTER APPLY (SELECT ROI.PlanTacticId
						,ROI.AnchorTacticID FROM ROI_PackageDetail ROI
						WHERE Tactic.PlanTacticId = ROI.PlanTacticId) ROI
	OUTER APPLY (SELECT Title AS 'PlanName' FROM [Plan] P WITH (NOLOCK)
					WHERE Tactic.LinkedPlanId = P.PlanId) P
	OUTER APPLY(SELECT TacticType.TacticTypeId,
						TacticType.AssetType,
						TacticType.Title  
						FROM TacticType WITH (NOLOCK)
						WHERE Tactic.TacticTypeId = TacticType.TacticTypeId) TacticType
	OUTER APPLY (SELECT LineItem.LineItemTypeId,
						LineItem.PlanLineItemId,
						LineItem.Cost,
						LT.Title AS 'LineItemType'
						FROM Plan_Campaign_Program_Tactic_LineItem LineItem WITH (NOLOCK)
						CROSS APPLY(SELECT LT.LineItemTypeId,LT.Title FROM LineItemType LT
									WHERE LineItem.LineItemTypeId = LT.LineItemTypeId
									AND LT.IsDeleted = 0)LT
						WHERE Hireachy.EntityType = 'LineItem'
						AND Hireachy.EntityId = LineItem.PlanLineItemId) LineItem
	OUTER APPLY (SELECT Stage.Title,Stage.StageId,Stage.[Level] FROM Stage WITH (NOLOCK) WHERE Tactic.StageId = Stage.StageId AND Stage.IsDeleted=0) Stage
	OUTER APPLY (SELECT Value FROM dbo.fnGetMqlByEntityTypeAndEntityId(Hireachy.EntityType,@ClientId,Stage.[Level],@StageMqlMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue) MQL
					WHERE Hireachy.EntityType='Tactic') AS MQL
	OUTER APPLY (SELECT Value FROM dbo.fnGetRevueneByEntityTypeAndEntityId(Hireachy.EntityType,@ClientId,Stage.[Level],@StageRevenueMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue,M.AverageDealSize) Revenue
					WHERE Hireachy.EntityType='Tactic') AS Revenue
	OUTER APPLY (SELECT PlanPlannedCost.PlanId
						,PlanPlannedCost.PlannedCost FROM Plan_PlannedCost PlanPlannedCost WHERE 
						Hireachy.EntityType='Plan'
							AND Hireachy.EntityId=PlanPlannedCost.PlanId)PlanPlannedCost
	OUTER APPLY (SELECT CampaignPlannedCost.PlanCampaignId
							,CampaignPlannedCost.PlannedCost FROM Campaign_PlannedCost CampaignPlannedCost WHERE 
							Hireachy.EntityType='Campaign'
								AND Hireachy.EntityId=CampaignPlannedCost.PlanCampaignId)CampaignPlannedCost
	OUTER APPLY (SELECT ProgramPlannedCost.PlanProgramId
							,ProgramPlannedCost.PlannedCost FROM Program_PlannedCost ProgramPlannedCost WHERE 
							Hireachy.EntityType='Program'
								AND Hireachy.EntityId=ProgramPlannedCost.PlanProgramId)ProgramPlannedCost
	
END
