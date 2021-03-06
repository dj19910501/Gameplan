/****** Object:  View [dbo].[CampaignDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[CampaignDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[CampaignDetail];
GO

CREATE VIEW [dbo].[CampaignDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, C.Title, C.Description, C.StartDate, C.EndDate, C.CreatedDate, C.ModifiedDate, C.IsDeleted, C.Status, C.IsDeployedToIntegration, C.IntegrationInstanceCampaignId, 
                  C.LastSyncDate, C.CampaignBudget, C.Abbreviation, C.IntegrationWorkFrontProgramID, C.CreatedBy, C.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId

GO
/****** Object:  View [dbo].[LineItemDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[LineItemDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[LineItemDetail];
GO

CREATE VIEW [dbo].[LineItemDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, L.PlanLineItemId, L.PlanTacticId, L.LineItemTypeId, L.Title, L.Description, L.StartDate, L.EndDate, L.Cost, L.IsDeleted, L.CreatedDate, L.ModifiedDate, 
                  L.LinkedLineItemId, L.ModifiedBy, L.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic_LineItem AS L ON L.PlanTacticId = T.PlanTacticId

GO
/****** Object:  View [dbo].[PlanDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[PlanDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[PlanDetail];
GO

CREATE VIEW [dbo].[PlanDetail]
AS
SELECT M.ClientId, P.PlanId, P.ModelId, P.Title, P.Version, P.Description, P.Budget, P.Status, P.IsActive, P.IsDeleted, P.CreatedDate, P.ModifiedDate, P.Year, P.GoalType, P.GoalValue, P.AllocatedBy, P.EloquaFolderPath, 
                  P.DependencyDate, P.CreatedBy, P.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId

GO
/****** Object:  View [dbo].[ProgramDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[ProgramDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[ProgramDetail];
GO

CREATE VIEW [dbo].[ProgramDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, Pr.PlanCampaignId AS Expr1, Pr.Title, Pr.Description, Pr.StartDate, Pr.EndDate, Pr.CreatedDate, Pr.ModifiedDate, Pr.IsDeleted, Pr.Status, 
                  Pr.IsDeployedToIntegration, Pr.IntegrationInstanceProgramId, Pr.LastSyncDate, Pr.ProgramBudget, Pr.Abbreviation, Pr.CreatedBy, Pr.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId

GO
/****** Object:  View [dbo].[TacticDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[TacticDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[TacticDetail];
GO

CREATE VIEW [dbo].[TacticDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, T.PlanTacticId, T.PlanProgramId, T.TacticTypeId, T.Title, T.Description, T.StartDate, T.EndDate, T.Cost, T.TacticBudget, T.Status, T.CreatedDate, T.ModifiedDate, T.IsDeleted, 
                  T.IsDeployedToIntegration, T.IntegrationInstanceTacticId, T.LastSyncDate, T.ProjectedStageValue, T.StageId, T.TacticCustomName, T.IntegrationWorkFrontProjectID, T.IntegrationInstanceEloquaId, T.LinkedTacticId, 
                  T.LinkedPlanId, T.IsSyncSalesForce, T.IsSyncEloqua, T.IsSyncWorkFront, T.IsSyncMarketo, T.IntegrationInstanceMarketoID, T.ModifiedBy, T.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId

GO