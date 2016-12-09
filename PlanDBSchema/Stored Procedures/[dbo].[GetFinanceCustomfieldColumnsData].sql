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
	DECLARE @query VARCHAR(MAX)=''
		
	DECLARE @columns VARCHAR(MAX)
	DECLARE @drpdCustomType VARCHAR(50)='DropDownList'
		
	SELECT @columns= COALESCE(@columns+', ' ,'') + C.Name+ '_'+CAST(C.CustomFieldId AS NVARCHAR(30))
	FROM Budget_ColumnSet(NOLOCK) A
	INNER JOIN Budget_Columns(NOLOCK) B ON A.Id= B.Column_SetId
	INNER JOIN CustomField(NOLOCK) C ON B.CustomFieldId = C.CustomFieldId and C.EntityType='Budget'
	WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND C.IsDeleted = 0 AND A.ClientId = @clientID AND MapTableName = 'CustomField_Entity'
		
	IF(ISNULL(@columns,'')!='')
	BEGIN
		SET @query = '
		SELECT * 
		FROM (
		     SELECT C.Name as Name,CF.EntityId as BudgetDetailId, 
			 		CASE 
					WHEN CT.Name='''+@drpdCustomType+''' THEN CFO.Value ELSE CF.Value
				END as Value
			FROM Budget_ColumnSet(NOLOCK) A
			INNER JOIN Budget_Columns(NOLOCK) B ON A.Id= B.Column_SetId
			INNER JOIN CustomField(NOLOCK) C ON B.CustomFieldId = C.CustomFieldId and C.EntityType=''Budget''
			INNER JOIN CustomFieldType(NOLOCK) CT ON C.CustomFieldTypeId = CT.CustomFieldTypeId
			LEFT JOIN CustomField_Entity(NOLOCK) CF ON C.CustomFieldId = CF.CustomFieldId and EntityID IN (select Id FROM Budget_Detail where BudgetId = '+CAST(@budgetID AS VARCHAR(20)) +' AND IsDeleted=0) 
			--INNER JOIN Budget_Detail BD ON CF.EntityId = BD.Id and BD.IsDeleted=0 and BD.BudgetId ='+CAST(@budgetID AS VARCHAR(20)) +'
			LEFT JOIN CustomFieldOption(NOLOCK) CFO ON CF.Value = CAST(CFO.CustomFieldOptionId AS nvarchar(30)) and CT.Name='''+@drpdCustomType+''' AND CFO.IsDeleted = 0 
			WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND C.IsDeleted = 0 AND 
			A.ClientId = '+CAST(@clientID AS VARCHAR(20))+' AND MapTableName = ''CustomField_Entity'' and IsNUll(CF.EntityId,'''') <> ''''
		) as s
		PIVOT
		(
		    MIN(Value)
		    FOR [Name] IN ('+@columns+')
		)AS pvt'
		EXEC (@query)

		-- Get custom field list with options to bind drop downs
		SELECT CF.CustomFieldId, CFO.CustomFieldOptionId, CFO.Value 
		FROM Budget_Columns(NOLOCK) Col 
		INNER JOIN Budget_ColumnSet(NOLOCK) CS ON Col.Column_SetId = CS.Id
		INNER JOIN CustomField(NOLOCK) CF ON Col.CustomFieldId = CF.CustomFieldId AND CF.EntityType='Budget'
		INNER JOIN CustomFieldType(NOLOCK) CFT ON CF.CustomFieldTypeId = CFT.CustomFieldTypeId AND CFT.Name = @drpdCustomType
		INNER JOIN CustomFieldOption(NOLOCK) CFO ON CF.CustomFieldId = CFO.CustomFieldId AND CFO.IsDeleted = 0
		WHERE Col.IsDeleted = 0 AND CS.IsDeleted = 0 AND CF.IsDeleted = 0 
			  	AND CS.ClientId = @clientID AND MapTableName = 'CustomField_Entity'
	END
END
GO