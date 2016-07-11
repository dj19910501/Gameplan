
GO

/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/08/2016 6:35:42 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_Tactic]') AND type in (N'P', N'PC',N'V'))
BEGIN
DROP VIEW [dbo].[vClientWise_Tactic]
End
GO

/****** Object:  View [dbo].[vClientWise_Tactic]    Script Date: 07/08/2016 6:35:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vClientWise_Tactic] AS
SELECT  a.PlanTacticId, a.Title, me.*, e.ClientId from Plan_Campaign_Program_Tactic a
left join Tactic_MediaCodes me on me.TacticId=a.PlanTacticId
inner join Plan_Campaign_Program b on a.PlanProgramId=b.PlanProgramId
inner join Plan_Campaign c on b.PlanCampaignId=c.PlanCampaignId
inner join [Plan] d on c.PlanId=d.PlanId
inner join Model e on d.ModelId=e.ModelId
where a.IsDeleted=0  and b.IsDeleted=0 and c.IsDeleted=0 and e.IsDeleted=0
GO


