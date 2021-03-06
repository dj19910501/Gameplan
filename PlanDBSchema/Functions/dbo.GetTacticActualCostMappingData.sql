/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:00:39 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticActualCostMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticActualCostMappingData]    Script Date: 06/10/2016 11:00:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticActualCostMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetTacticActualCostMappingData]
(
	@entIds varchar(max)=''''
)
RETURNS @tac_actualcost_mappingtbl Table(
PlanTacticId int,
ActualCost varchar(50)
)

AS
BEGIN
	Declare @costStage varchar(20)=''Cost''

	-- Get Tactic & Tactic Actual Cost Mapping data 
	-- If Tactic has lineitems then Sum up of LineItem Actual''s value else Tactic Actual''s value.

	INSERT INTO @tac_actualcost_mappingtbl
	SELECT tac.PlanTacticId,
	   	   CASE 
			WHEN COUNT(distinct line.PlanLineItemId) >0 THEN  Cast(IsNULL(SUM(lActl.Value),0) as varchar(50)) ELSE  Cast(IsNULL(SUM(tActl.Actualvalue),0) as varchar(50))
		   END as ActualCost
	FROM Plan_Campaign_Program_Tactic as tac
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem as line on tac.PlanTacticId = line.PlanTacticId and line.IsDeleted=''0''
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual as lActl on line.PlanLineItemId = lActl.PlanLineItemId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual as tActl on tac.PlanTacticId = tActl.PlanTacticId and  tActl.StageTitle=@costStage
	WHERE tac.PlanTacticId IN (select val from comma_split(@entIds,'',''))
	GROUP BY tac.PlanTacticId
	RETURN 
END
' 
END

GO
