/****** Object:  StoredProcedure [dbo].[GetGridData]    Script Date: 10/17/2016 8:04:18 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridData]
GO
/****** Object:  StoredProcedure [dbo].[GetGridData]    Script Date: 10/17/2016 8:04:18 PM ******/
SET ANSI_NULLS ON
GO

-- =============================================
-- Author:		Nishant Sheth
-- Create date: 09-Sep-2016
-- Description:	Get home grid data with custom field 19910781.11
-- =============================================
CREATE PROCEDURE [dbo].[GetGridData]
	-- Add the parameters for the stored procedure here
		@PlanId NVARCHAR(MAX) = ''
		,@ClientId INT = 0
		,@OwnerIds NVARCHAR(MAX) = ''
		,@TacticTypeIds varchar(max)=''
		,@StatusIds varchar(max)=''
		,@ViewBy varchar(max)=''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	DECLARE @StageMqlMaxLevel INT = 1
	DECLARE @StageRevenueMaxLevel INT = 1
	-- Variables for fnGetFilterEntityHierarchy we pass defualt values because no need to pass timeframe option on grid
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1

	SELECT @StageMqlMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='MQL'

	SELECT @StageRevenueMaxLevel = [Level] FROM Stage
			 WHERE Stage.ClientId=@ClientId
					AND Stage.IsDeleted=0
						AND Stage.Code='CW'

	SELECT Hireachy.UniqueId,
				Hireachy.EntityId,		
				Hireachy.EntityTitle,		
				IsNull(Hireachy.ParentEntityId,0) as ParentEntityId,
				Hireachy.ParentUniqueId	,
				IsNull(Hireachy.EntityTypeId,0) As EntityType	,	
				Hireachy.ColorCode,		
				Hireachy.[Status],		
				Hireachy.StartDate,		
				Hireachy.EndDate,			
				Hireachy.CreatedBy AS 'Owner',
				Hireachy.AltId,			
				Hireachy.TaskId,			
				Hireachy.ParentTaskId,	
				Hireachy.PlanId	,		
				IsNull(Hireachy.ModelId,0) as ModelId,			
				TacticType.AssetType,
				TacticType.Title AS TacticType,
				IsNull(Tactic.TacticTypeId,0) as TacticTypeId,
				IsNull(LineItem.LineItemTypeId,0) as LineItemTypeId,
				LineItem.LineItemType,
				IsNUll(CASE WHEN EntityType = 'Tactic'
						THEN Tactic.Cost 
							WHEN EntityType = 'LineItem' 
								THEN LineItem.Cost
				END,0) AS PlannedCost
				,IsNull(Tactic.ProjectedStageValue,0) as ProjectedStageValue
				,Stage.Title AS 'ProjectedStage'
				,NULL AS 'TargetStageGoal'
				,CASE WHEN Hireachy.EntityType='Tactic'
					THEN
					(SELECT IsNUll(Value,0) FROM dbo.fnGetMqlByEntityTypeAndEntityId('Tactic',@ClientId,Stage.[Level],@StageMqlMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue))
					ELSE 0
					END
					AS MQL
				,CASE WHEN Hireachy.EntityType='Tactic'
					THEN (SELECT IsNUll(Value,0) FROM dbo.fnGetRevueneByEntityTypeAndEntityId('Tactic',@ClientId,Stage.[Level],@StageRevenueMaxLevel,Hireachy.ModelId,Tactic.ProjectedStageValue,M.AverageDealSize))
					ELSE 0
					END AS Revenue
				,Tactic.TacticCustomName AS 'MachineName'
				,IsNull(Tactic.LinkedPlanId,0) as LinkedPlanId
				,IsNull(Tactic.LinkedTacticId,0) as LinkedTacticId
				,P.PlanName AS 'LinkedPlanName'
				,IsNULL(ROI.AnchorTacticID,0) as AnchorTacticID
				--PackageTacticIds - comma saperated values selected as part of ROI package
				,Hireachy.ROIPackageIds AS PackageTacticIds 
				,PlanDetail.PlanYear
				--Hireachy.EloquaId AS 'Eloquaid',
				--Hireachy.MarketoId AS 'Marketoid',
				--Hireachy.WorkfrontId AS 'WorkFrontid',
				--Hireachy.SalesforceId AS 'Salesforceid'
				FROM [dbo].fnViewByEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@ViewBy,@TimeFrame,@Isgrid) Hireachy
				LEFT JOIN Model M WITH (NOLOCK) ON Hireachy.ModelId = M.ModelId
				LEFT JOIN Plan_Campaign_Program_Tactic Tactic WITH (NOLOCK) ON Hireachy.EntityType='Tactic'
					AND Hireachy.EntityId = Tactic.PlanTacticId
	
	OUTER APPLY (SELECT ROI.PlanTacticId
						,ROI.AnchorTacticID FROM ROI_PackageDetail ROI
						WHERE Tactic.PlanTacticId = ROI.PlanTacticId) ROI
	OUTER APPLY (SELECT Title AS 'PlanName'
						FROM [Plan] P WITH (NOLOCK)
					WHERE Tactic.LinkedPlanId = P.PlanId) P
	OUTER APPLY (SELECT [PlanDetail].[Year] AS 'PlanYear'
						FROM [Plan] PlanDetail WITH (NOLOCK)
					WHERE 
					Hireachy.EntityType = 'Plan' AND
					Hireachy.EntityId = PlanDetail.PlanId) PlanDetail
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
						OUTER APPLY(SELECT LT.LineItemTypeId,LT.Title FROM LineItemType LT
									WHERE 
									LineItem.LineItemTypeId = LT.LineItemTypeId
									AND
									 LT.IsDeleted = 0)LT
						WHERE Hireachy.EntityType = 'LineItem'
						AND Hireachy.EntityId = LineItem.PlanLineItemId) LineItem
	OUTER APPLY (SELECT Stage.Title,Stage.StageId,Stage.[Level] FROM Stage WITH (NOLOCK) WHERE Tactic.StageId = Stage.StageId AND Stage.IsDeleted=0) Stage
END

GO