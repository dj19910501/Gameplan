GO

--Modified BY : Komal rawal
--Date :15-7-16
--Desc : To get Tactic Type listing after we delete tactic type from model
/****** Object:  StoredProcedure [dbo].[GetTacticTypeList]    Script Date: 07/15/2016 14:33:40 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticTypeList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTacticTypeList]
GO
/****** Object:  StoredProcedure [dbo].[GetTacticTypeList]    Script Date: 07/15/2016 14:33:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticTypeList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetTacticTypeList] AS' 
END
GO
-- Created BY Nisahnt Sheth
-- Desc :: get list of tactic type 
-- Created Date : 04-Mar-2016
ALTER PROCEDURE [dbo].[GetTacticTypeList]
@TacticIds nvarchar(max)
AS
BEGIN
SET NOCOUNT ON;

Select val INTO #PlanTacticIds From dbo.comma_split(@TacticIds,',')

SELECT [TacticType].Title,[TacticType].TacticTypeId,Count([TacticType].TacticTypeId) As Number FROM TacticType WITH (NOLOCK) 
CROSS APPLY (SELECT PlanTacticId,TacticTypeId FROM Plan_Campaign_Program_Tactic As Tactic WITH (NOLOCK)
CROSS APPLY (SELECT * FROM #PlanTacticIds PT WHERE PT.val = Tactic.PlanTacticId) PT
WHERE TacticType.TacticTypeId=Tactic.TacticTypeId  AND IsDeleted=0) Tactic
GROUP BY [TacticType].TacticTypeId,[TacticType].Title
ORDER BY [TacticType].Title
END
GO

GO
