IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateSalesforceIdForMarketoTactic]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateSalesforceIdForMarketoTactic]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nihshant Sheth
-- Create date: 16-May-2016
-- Description:	Update SalesforceId for marketo's tactic
-- =============================================
CREATE PROCEDURE UpdateSalesforceIdForMarketoTactic
@SalesForce SalesforceType ReadOnly
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Tactic SET Tactic.IntegrationInstanceTacticId=Salesforce.Id FROM Plan_Campaign_Program_Tactic Tactic
	CROSS APPLY (SELECT Id,Name FROM @SalesForce Salesforce WHERE Salesforce.Name=Tactic.TacticCustomName) Salesforce
	WHERE Tactic.IsDeleted=0 AND Tactic.IntegrationInstanceMarketoID IS NOT NULL
	AND Tactic.IsSyncSalesForce=1 AND Tactic.IntegrationInstanceTacticId IS NULL
	AND Tactic.IsDeployedToIntegration=1
	AND Tactic.Status IN('Approved','In-Progress','Complete')
END
GO
