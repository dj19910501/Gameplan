-- DROP AND CREATE STORED PROCEDURE [dbo].[GetHeaderValuesForFinance]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetHeaderValuesForFinance]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetHeaderValuesForFinance]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/30/2016
-- Description:	Get HUD values for finance grid
-- =============================================
-- [dbo].[GetHeaderValuesForFinance] 1,'2,4,6,12,13,17,22,24,27,30,32,37,38,42,43,49,53,58,59,641,62,66,71,77,79,80,82,643,85,86,87,88,89,92,96,97,101,106,107,617,646,110,113,114,624,120,125,129,131,133,619,134,136,137,139,647,142,144,146,150,155,157,158,160,163,165,166,645,169,174,175,176,179,186,623,188,190,191,192,194,195,208,210,211,213,214,219,220,221,223,224,621,243,245,250,626,254,256,258,259,260,625,261,262,269,270,272,273,275,276,277,281,288,289,291,292,293,295,298,299,301,303,304,313,314,315,318,320,322,324,327,330,339,341,342,344,349,355,359,361,362,365,366,370,374,378,382,387,389,393,394,398,399,403,406,407,408,412,415,419,426,427,428,431,434,435,440,446,447,449,450,451,454,457,459,462,465,466,467,468,470,471,472,475,481,485,486,487,488,492,493,495,497,503,504,505,512,514,516,517,518,519,520,521,526,527,533,536,537,542,544,545,546,549,551,555,556,561,566,573,574,578,584,585,586,588,590,649,593,595,602,608',1
CREATE PROCEDURE [dbo].[GetHeaderValuesForFinance]
	-- Add the parameters for the stored procedure here
	@BudgetId		INT,
	@lstUserIds		NVARCHAR(MAX)='',
	@CurrencyRate	FLOAT = 1.0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Budget		FLOAT,
			@Forecast	FLOAT,
			@Planned	FLOAT,
			@Actual		FLOAT

	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	-- Get Budget, Forecast for Header
	SELECT @Budget = SUM(TotalBudget) * @CurrencyRate, @Forecast = SUM(TotalForecast) * @CurrencyRate
	FROM Budget_Detail BD
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	WHERE BudgetId = @BudgetId AND IsDeleted = 0

	-- Get Planned Cost for Header
	SELECT @Planned = SUM(PCPTL.Cost * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0

	-- Get Actual for Header
	SELECT @Actual = SUM(PCPTLA.Value * CAST(Weightage AS FLOAT)/100) * @CurrencyRate
	FROM [dbo].[Budget_Detail] BD 
	INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
	INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId 
	INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PCPTLA ON PCPTL.PlanLineItemId = PCPTLA.PlanLineItemId
	WHERE BD.IsDeleted = 0 AND BD.BudgetId = @BudgetId AND PCPTL.LineItemTypeId IS NOT NULL AND PCPTL.IsDeleted = 0
	AND REPLACE(Period,'Y','') < 13

	SELECT ISNULL(@Budget,0) AS Budget, ISNULL(@Forecast,0) AS Forecast, ISNULL(@Planned,0) AS Planned, ISNULL(@Actual,0) AS Actual
END
GO
