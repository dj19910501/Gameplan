/****** Object:  StoredProcedure [dbo].[spGetGoalValuesForPlan]    Script Date: 09/16/2016 12:26:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetGoalValuesForPlan]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetGoalValuesForPlan] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[spGetGoalValuesForPlan]
	@PlanId INT,
	@ModelId INT,
	@PlanStageCode NVARCHAR(255),
	@ClientId UNIQUEIDENTIFIER
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--EXEC spGetGoalValuesForPlan 20220,10263,'INQ','464eb808-ad1f-4481-9365-6aada15023bd'

	SET NOCOUNT ON;

    DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@INQValue FLOAT = 0,
			@MQLValue FLOAT = 0,
			@CWValue FLOAT = 0,
			@Revenue FLOAT = 0
	
	SELECT @INQValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@INQCode,@ClientId)
	SELECT @MQLValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@MQLCode,@ClientId)
	SELECT @CWValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@CWCode,@ClientId)
	SELECT @Revenue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@RevenueCode,@ClientId)
	
	SELECT @INQValue as 'INQ',@MQLValue as 'MQL',@CWValue as 'CW',@Revenue as 'Revenue'
	
END

GO
