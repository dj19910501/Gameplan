-- DROP AND CREATE STORED PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[PreCalPlannedActualForFinanceGrid]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to insert/update precalculated data for marketing budget grid
-- =============================================
-- [MV].[PreCalculatePlannedActualForFinance] 'Planned','Y1',5555, 109415
CREATE PROCEDURE [MV].[PreCalPlannedActualForFinanceGrid]
	@UpdatedColumn		VARCHAR(30),  -- Enum which is used to identify Planned/Actuals
	@Year				INT,		  -- Year 
	@Period				VARCHAR(5),   -- Period in case of editing Monthly/Quarterly allocation
	@NewValue			FLOAT,        -- New value for Budget/Forecast/Custom column
	@OldValue			FLOAT,
	@PlanLineItemId		INT           -- Line Item Id in which Budget is associated
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @InsertQuery		NVARCHAR(MAX), 
			@UpdateColumnName	NVARCHAR(50)
	
	SET @UpdateColumnName = @Period + '_' + @UpdatedColumn
	
	-- Insert Planned/Actual values in PreCalculatedMarketingBudget table if already exists 
	SET @InsertQuery = ' MERGE INTO [MV].[PreCalculatedMarketingBudget] AS T1
						 USING
						 (	
							SELECT BudgetDetailId, 
							' + CAST(@Year AS VARCHAR(30))+ ' AS [Year], 
							'+CAST(ISNULL(@OldValue,0) AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) AS OldValue, 
						 	'+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) AS '+@UpdateColumnName+' 
							FROM LineItem_Budget WHERE PlanLineItemid = '+CAST(@PlanLineItemId AS VARCHAR(30))+'
						 ) AS T2
						 ON (T2.BudgetDetailId = T1.BudgetDetailId AND T2.Year = T1.Year)
						 WHEN MATCHED THEN
						 UPDATE SET ' + @UpdateColumnName + ' = (ISNULL(T1.' + @UpdateColumnName + ',0) - T2.OldValue + ' +'T2.' + @UpdateColumnName +')
						 WHEN NOT MATCHED THEN  
						 INSERT (BudgetDetailId, [Year], ' + @UpdateColumnName + ')
						 VALUES (BudgetDetailId, [Year], ' + @UpdateColumnName + ');'
	
	EXEC(@InsertQuery)
END
GO


