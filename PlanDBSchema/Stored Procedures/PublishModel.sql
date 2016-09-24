
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
,@UserId INT= 0
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
IF (@TacticTypeIds != 'All')
Begin
SELECT @FilterValues = COALESCE(@FilterValues + ',', '') + CAST(TacticTypeId AS NVARCHAR) FROM TacticType 
WHERE PreviousTacticTypeId IN(SELECT val FROM dbo.comma_split(@TacticTypeIds,','))
AND ModelId=@NewModelId
End

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