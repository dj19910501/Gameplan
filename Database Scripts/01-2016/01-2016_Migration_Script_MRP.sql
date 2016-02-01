/* --------- Start Script of PL ticket #1921 --------- */
-- Created by : Viral Kadiya
-- Created On : 1/22/2016
-- Description : Add column IsSyncSalesForce & IsSyncEloqua column to Tactic table - To save Integration settings from Review tab

IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncSalesForce' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncSalesForce bit
END
GO
IF not exists (SELECT * FROM sys.columns  WHERE Name = N'IsSyncEloqua' AND Object_ID = Object_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] ADD IsSyncEloqua bit
END
GO

/* --------- End Script of PL ticket #1921 --------- */

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
-- Add By Rahul Shah
-- Desc :: set delete flag when otherLineItem's cost is '0'
Go
IF (EXISTS(SELECT * FROM [Plan_Campaign_Program_Tactic_Lineitem] WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0)) 
BEGIN 
	Update [Plan_Campaign_Program_Tactic_Lineitem] Set isDeleted = 1 WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0
End

