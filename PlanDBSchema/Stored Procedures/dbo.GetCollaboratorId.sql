/****** Object:  StoredProcedure [dbo].[GetCollaboratorId]    Script Date: 2/22/2016 4:35:57 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCollaboratorId]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[GetCollaboratorId]
END
GO
/****** Object:  StoredProcedure [dbo].[GetCollaboratorId]    Script Date: 2/22/2016 4:36:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Created By Nishant Sheth
-- EXEC GetCollaboratorId 226
CREATE PROCEDURE [dbo].[GetCollaboratorId]
@PlanId int = 0
AS
BEGIN
SELECT tacComment.* From [dbo].[Plan_Campaign_Program_Tactic_Comment] TacComment
CROSS APPLY(Select PlanTacticId,PlanProgramId From [dbo].[Plan_Campaign_Program_Tactic] Tactic WHERE TacComment.PlanTacticId=Tactic.PlanTacticId) Tactic
CROSS APPLY(Select PlanProgramId,PlanCampaignId From  [dbo].[Plan_Campaign_Program] Program WHERE Tactic.PlanProgramId=Program.[PlanProgramId])Program
CROSS APPLY(Select PlanCampaignId,Planid FROM  [dbo].[Plan_Campaign] Camp WHERE Program.[PlanCampaignId]=Camp.[PlanCampaignId])Camp
Where Camp.Planid=@PlanId
END