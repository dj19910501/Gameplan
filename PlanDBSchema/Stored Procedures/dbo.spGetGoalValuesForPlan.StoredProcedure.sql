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
	@PlanIds VARCHAR(max),
	@ClientId UNIQUEIDENTIFIER
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--EXEC spGetGoalValuesForPlan '20220','464eb808-ad1f-4481-9365-6aada15023bd'

	SET NOCOUNT ON;

    DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@INQValue FLOAT = 0,
			@MQLValue FLOAT = 0,
			@CWValue FLOAT = 0,
			@RevenueValue FLOAT = 0,
			@INQTotal FLOAT = 0,
			@MQLTotal FLOAT = 0,
			@CWTotal FLOAT = 0,
			@RevenueTotal FLOAT = 0,
			@PlanId INT,
			@ModelId INT,
			@PlanStageCode VARCHAR(20),
			@inqTitle varchar(255)='',
			@mqlTitle varchar(255)='',
			@cwTitle varchar(255)='',
			@revenue varchar(20)='Revenue'
	
	Declare @plans TABLE(
		PlanId INT,
		ModelId INT,
		PlanStageCode varchar(10)
	)


	INSERT INTO @plans 
	SELECT P.PlanId,P.ModelId,P.GoalType
	FROM [dbo].[comma_split](@PlanIds,',') as V
	INNER JOIN [Plan] as P on Cast(V.val as INT) = P.PlanId

	BEGIN
		DECLARE planGoal CURSOR FOR  
		SELECT PlanId,ModelId,PlanStageCode 
		FROM @plans 

		OPEN planGoal   
		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode  
		
		WHILE @@FETCH_STATUS = 0   
		BEGIN   

		SELECT @INQValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@INQCode,@ClientId)
		SELECT @MQLValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@MQLCode,@ClientId)
		SELECT @CWValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@CWCode,@ClientId)
		SELECT @RevenueValue = [dbo].[PlanIndicatorProjectedValue] (@PlanId,@ModelId,@PlanStageCode,@RevenueCode,@ClientId)
		
		SET @INQTotal = @INQTotal + @INQValue
		SET @MQLTotal = @MQLTotal + @MQLValue
		SET @CWTotal = @CWTotal + @CWValue
		SET @RevenueTotal = @RevenueTotal + @RevenueValue

		FETCH NEXT FROM planGoal INTO @PlanId, @ModelId, @PlanStageCode 
		END   
		
		CLOSE planGoal   
		DEALLOCATE planGoal
	END
		
	Select @inqTitle = Title from Stage where ClientId=@ClientId and Code=@INQCode and IsDeleted='0'
	Select @mqlTitle = Title from Stage where ClientId=@ClientId and Code=@MQLCode and IsDeleted='0'
	Select @cwTitle = Title from Stage where ClientId=@ClientId and Code=@CWCode and IsDeleted='0'


	SELECT @inqTitle as Title, @INQTotal as Value, @INQCode as StageCode
	UNION ALL
	SELECT @mqlTitle as Title, @MQLTotal as Value, @MQLCode as StageCode
	UNION ALL
	SELECT @cwTitle as Title, @CWTotal as Value, @CWCode as StageCode
	UNION ALL
	SELECT @revenue as Title, @RevenueTotal as Value, @revenue as StageCode
	
END

GO
