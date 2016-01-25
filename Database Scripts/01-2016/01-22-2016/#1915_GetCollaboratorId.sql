-- Add BY Nishant Sheth
-- Desc :: To GetCollaboratorsId
IF OBJECT_ID(N'[dbo].[GetCollaboratorId]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[GetCollaboratorId]
GO
-- Created By Nishant Sheth
-- EXEC GetCollaboratorId 226
CREATE PROCEDURE GetCollaboratorId
@PlanId int = 0
AS
BEGIN
SELECT tacComment.* From [dbo].[Plan_Campaign_Program_Tactic_Comment] TacComment
CROSS APPLY(Select PlanTacticId,PlanProgramId From [dbo].[Plan_Campaign_Program_Tactic] Tactic WHERE TacComment.PlanTacticId=Tactic.PlanTacticId) Tactic
CROSS APPLY(Select PlanProgramId,PlanCampaignId From  [dbo].[Plan_Campaign_Program] Program WHERE Tactic.PlanProgramId=Program.[PlanProgramId])Program
CROSS APPLY(Select PlanCampaignId,Planid FROM  [dbo].[Plan_Campaign] Camp WHERE Program.[PlanCampaignId]=Camp.[PlanCampaignId])Camp
Where Camp.Planid=@PlanId
END