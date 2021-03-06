
-- DROP AND CREATE STORED PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetFinanceCustomfieldColumnsData]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
END
GO
/****** Object:  StoredProcedure [dbo].[GetFinanceCustomfieldColumnsData]    Script Date: 11/28/2016 06:31:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Viral
-- Create date: 11/21/2016
-- Description:	Get Finance Customfield Columns
-- =============================================
CREATE PROCEDURE [dbo].[GetFinanceCustomfieldColumnsData]
	-- Add the parameters for the stored procedure here
	@BudgetId int, 
	@ClientId int 
AS
BEGIN
	-- Exec GetFinanceCustomfieldColumnsData 2766,24
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	DECLARE @query VARCHAR(MAX) = '',
			@columns VARCHAR(MAX),
			@drpdCustomType VARCHAR(50)='DropDownList',
			@txtCustomType VARCHAR(50)='TextBox',
			@CFwithoutOptions NVARCHAR(MAX)
	
	CREATE TABLE #TblCustomFields (
		CustomFieldId INT,
		CustomFieldName NVARCHAR(255),
		CustomFieldTypeName NVARCHAR(50)
	)

	INSERT INTO #TblCustomFields 
	SELECT C.CustomFieldId,C.Name,CT.Name
	FROM Budget_ColumnSet(NOLOCK) A
	INNER JOIN Budget_Columns(NOLOCK) B ON A.Id= B.Column_SetId
	INNER JOIN CustomField(NOLOCK) C ON B.CustomFieldId = C.CustomFieldId and C.EntityType='Budget'
	INNER JOIN CustomFieldType(NOLOCK) CT ON C.CustomFieldTypeId = CT.CustomFieldTypeId
	WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND C.IsDeleted = 0 AND A.ClientId = @clientID AND MapTableName = 'CustomField_Entity'

	-- Remove custom fields having no options into CustomFieldOption table
	SELECT @CFwithoutOptions = COALESCE(@CFwithoutOptions +', ' ,'') + CAST(CF.CustomFieldId AS NVARCHAR(30))
	FROM #TblCustomFields CF
	LEFT JOIN CustomFieldOption(NOLOCK) CFO ON CF.CustomFieldId = CFO.CustomFieldId
	WHERE CF.CustomFieldTypeName = @drpdCustomType AND CFO.CustomFieldOptionId IS NULL

	-- Get list of custom columns 
	SELECT @columns= COALESCE(@columns+', ' ,'') + CustomFieldName+ '_'+ CAST(CustomFieldId AS NVARCHAR(30))
	FROM #TblCustomFields
	WHERE CustomFieldId NOT IN (SELECT VAL FROM dbo.comma_split(ISNULL(@CFwithoutOptions,''),','))

	IF(ISNULL(@columns,'') != '')
	BEGIN
		SET @query = '
		SELECT * 
		FROM (
			  -- Get custom columns of type textbox
		      SELECT C.CustomFieldName +''_''+ CAST(C.CustomFieldId AS NVARCHAR(30)) as Name,
					 CF.EntityId as BudgetDetailId, 
					 CF.Value
			  FROM #TblCustomFields C
			  LEFT JOIN CustomField_Entity(NOLOCK) CF ON C.CustomFieldId = CF.CustomFieldId AND 
			  		  EntityID IN (SELECT Id FROM Budget_Detail WHERE BudgetId = '+CAST(@budgetID AS VARCHAR(20)) +' AND IsDeleted=0) 
					  
			  WHERE C.CustomFieldTypeName ='''+@txtCustomType+''' 
			  AND IsNUll(CF.EntityId,'''') <> ''''
			  
			  UNION 
			  -- Get custom columns of type drop down list
			  SELECT C.CustomFieldName +''_''+ CAST(C.CustomFieldId AS NVARCHAR(30)) as Name,
					 CF.EntityId as BudgetDetailId,
			   		 CAST(CFO.CustomFieldOptionId AS NVARCHAR(30)) 
			  FROM #TblCustomFields C
			  LEFT JOIN CustomField_Entity(NOLOCK) CF ON C.CustomFieldId = CF.CustomFieldId AND 
			  		    EntityID IN (SELECT Id FROM Budget_Detail WHERE BudgetId = '+CAST(@budgetID AS VARCHAR(20)) +' AND IsDeleted=0) 
						
			  LEFT JOIN CustomFieldOption(NOLOCK) CFO ON CF.Value = CAST(CFO.CustomFieldOptionId AS nvarchar(30)) AND CFO.IsDeleted = 0 
			  WHERE C.CustomFieldTypeName ='''+@drpdCustomType+''' AND C.CustomFieldId NOT IN ('+ISNULL(@CFwithoutOptions,0)+')
			  AND IsNUll(CF.EntityId,'''') <> ''''
			  
		) as s
		PIVOT
		(
		    MIN(Value)
		    FOR [Name] IN ('+@columns+')
		)AS pvt'
		EXEC (@query)

		-- Get custom field list with options to bind drop downs
		SELECT CF.CustomFieldId, CFO.CustomFieldOptionId, CFO.Value 
		FROM #TblCustomFields CF
		INNER JOIN CustomFieldOption(NOLOCK) CFO ON CF.CustomFieldId = CFO.CustomFieldId AND CFO.IsDeleted = 0
	END
END
GO