

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportMarketingBudgetMonthly]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ImportMarketingBudgetMonthly]
GO

/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetMonthly]    Script Date: 12/01/2016 12:41:52 PM ******/
SET ANSI_NULLS ON
GO


CREATE  PROCEDURE [dbo].[ImportMarketingBudgetMonthly]
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@clientId INT
,@UserId INT
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
	----disable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
	ALTER TABLE Budget_DetailAmount DISABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
	ALTER TABLE Budget_Detail DISABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
	CREATE TABLE  #tmpXmlData  (ROWNUM BIGINT) --create # table because there are dynamic columns added as per file imported for marketing budget

	DECLARE @Textboxcol nvarchar(max)=''
	DECLARE @UpdateColumn NVARCHAR(255)
	DECLARE @CustomEntityDeleteDropdownCount BIGINT
	DECLARE @CustomEntityDeleteTextBoxCount BIGINT
	DECLARE @IsCutomFieldDrp BIT
	DECLARE @GetBudgetAmoutData NVARCHAR(MAX)=''
	DECLARE @Count Int = 1;
	DECLARE @RowCount INT;
	DECLARE @ColName nvarchar(100)
	DECLARE @IsMonth nvarchar(100)

	SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol
	DECLARE @XmldataQuery NVARCHAR(MAX)=''

	DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

	SET @XmldataQuery += 'SELECT ROW_NUMBER() OVER(ORDER BY(SELECT 100)),'
	DECLARE @ConcatColumns NVARCHAR(MAX)=''

	WHILE(@Count<=@RowCount)
	BEGIN
		SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

		SET @ConcatColumns  += 'pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],'

		SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	SELECT @ConcatColumns=CASE WHEN LEN(@ConcatColumns)>1
						THEN LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)
							ELSE @ConcatColumns 
						END

	SET @XmldataQuery+= @ConcatColumns+' FROM	@XmlData.nodes(''/data/row'') AS People(pref);'

	EXEC(@tmpXmlDataAlter)
	Declare @tmpChildBudgets table(Id int,ParentId int, BudgetId int)

	;WITH tblChild AS
	(
		SELECT Id,ParentId,BudgetId	FROM Budget_Detail WHERE Id = @BudgetDetailId 
		UNION ALL
		SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
		CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
	)
	insert into @tmpChildBudgets
	select *FROM tblChild
	OPTION(MAXRECURSION 0)

	INSERT INTO #tmpXmlData EXECUTE sp_executesql @XmldataQuery, N'@XmlData XML OUT', @XmlData = @XmlData  OUT
		-- Remove Other Child items which are not related to parent
		DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
		left join @tmpChildBudgets tmpChildBudgets
		on CAST(tmpXmlData.[Id#1] AS INT)= tmpChildBudgets.Id
		WHERE tmpChildBudgets.Id IS NULL

		-- Remove View/None Permission budgets
		DELETE tmpXmlData FROM #tmpXmlData tmpXmlData
		left join Budget_Permission BudgetPermission
		on CAST(tmpXmlData.[Id#1] AS INT)=BudgetPermission.BudgetDetailId AND UserId=@UserId
		AND (BudgetPermission.IsOwner=1 OR BudgetPermission.PermisssionCode=0)
		WHERE BudgetPermission.Id IS NULL

		-- Add only Child/Forecast item and store into # table because there are dynamic columns added as per imported file.
		Select *  into #childtempData from #tmpXmlData
		inner join (select * from dbo.fnGetBudgetForeCast_List (@BudgetDetailId,@clientId)) child
		on CAST(#tmpXmlData.[Id#1] AS INT) = child.Id

	-- Update Process
	DECLARE @MonthNumber varchar(2) -- variable for month number while import budget data
	DECLARE @ConvertCount nvarchar(5) --variable to set column count with casting
	SET @Count=2;
																																																																																																																																																																												WHILE(@Count<=@RowCount)
	BEGIN

	SELECT @UpdateColumn=[ColumnName],@Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE [Month] END FROM @ImportBudgetCol WHERE ColumnIndex=@Count
	-- Insert/Update values for budget and forecast
	SET @ConvertCount=CAST(@Count AS varchar(5))

	 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
	 BEGIN
		IF(@Ismonth!='' AND @Ismonth!='Total')
		BEGIN
			SELECT  @MonthNumber = CAST(DATEPART(MM,''+@IsMonth+' 01 1990') AS varchar(2))
			DECLARE @temp nvarchar(max)=''
			SET @GetBudgetAmoutData+=' 
			-- Update the Budget Detail amount table for Forecast and Budget values
			UPDATE BudgetDetailAmount SET ['+(@UpdateColumn)+']=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END 
			FROM 
			Budget_DetailAmount BudgetDetailAmount
			CROSS APPLY (SELECT * FROM #childtempData tmpXmlData WHERE BudgetDetailAmount.BudgetDetailId=CAST([Id#1] AS INT) 
			AND BudgetDetailAmount.Period=''Y'+@MonthNumber+''' AND ISNULL(['+@UpdateColumn+'#'+CAST(@Count AS VARCHAR(50))+'],'''')<>'''' 
			AND CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END <> ISNULL(['+(@UpdateColumn)+'],0)
			) tmpXmlData
			
			-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
			INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,['+@UpdateColumn+'])
			SELECT  tmpXmlData.[Id#1] 
			,''Y'+@MonthNumber+'''
			,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
			THEN 
			Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
			--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) > 0
			--THEN 
			CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
			--ELSE 0 END
			ELSE 0 END
			FROM #childtempData  tmpXmlData
			OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
			WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
			AND A.Period=''Y'+@MonthNumber+''')
			A WHERE A.Id IS NULL 
			
			 '
		END
		Else If(@Ismonth!='' AND @Ismonth='Total') -- Update total budget and Forecast for child items
		Begin
			Declare @UpdateTotal NVARCHAR(max)
			IF(@UpdateColumn='Budget')
			begin
				set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalBudget=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
				THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT) 
				ELSE 0 END 
				from Budget_Detail BudgetDetail
				inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
				exec sp_executesql @UpdateTotal
			end
			Else
			begin
				set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalForeCast=CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
				THEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'','','''') AS FLOAT)
				ELSE 0 END 
				from Budget_Detail BudgetDetail
				inner join (SELECT * FROM #childtempData) tmpXmlData on BudgetDetail.Id=CAST(tmpXmlData.[Id#1] AS INT)'
				exec sp_executesql @UpdateTotal
			End

		End
	END

	-- Custom Columns
	 IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
	 BEGIN
		IF(@Ismonth='' AND @Ismonth!='Total')
		BEGIN

			SELECT @IsCutomFieldDrp = CASE WHEN CustomFieldType.Name='TextBox' THEN 0 ELSE 1 END FROM CustomField 
			CROSS APPLY(SELECT CustomFieldType.Name,CustomFieldType.CustomFieldTypeId FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) CustomFieldType
			WHERE CustomField.Name=''+@UpdateColumn+'' AND CustomField.ClientId=@ClientId AND CustomField.IsDeleted=0 AND CustomField.EntityType='Budget'
			
			-- Insert/Update/Delete values for custom field as dropdown
			IF(@IsCutomFieldDrp=1)
			BEGIN
				SET @GetBudgetAmoutData+=' 
				-- Get List of record which need to delete from CustomField Entity Table

				INSERT INTO @tmpCustomDeleteDropDown 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteDropdownCount=COUNT(*) FROM @tmpCustomDeleteDropDown tmpCustomDelete

				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteDropdownCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteDropDown)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteDropDown)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,CustOpt.CustomFieldOptionId,'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE CustomFieldEntity.CustomFieldEntityId IS NULL
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=CustOpt.CustomFieldOptionId FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				CROSS APPLY (SELECT * FROM CustomFieldOption CustOpt WHERE CustomField.CustomFieldId=CustOpt.CustomFieldId AND CustOpt.IsDeleted=0
				AND CustOpt.Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) CustOpt 
				WHERE CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				'
			END

			-- Insert/Update/Delete values for custom field as Textbox
			IF(@IsCutomFieldDrp<>1)
			BEGIN
				SET @GetBudgetAmoutData+='  
				-- Get List of record which need to delete from CustomField Entity Table
				INSERT INTO @tmpCustomDeleteTextBox 
				SELECT DISTINCT CAST(CustomFieldEntity.EntityId AS BIGINT),CAST(CustomField.CustomFieldId AS BIGINT) FROM CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId AND ISNULL(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''')='''') tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField
				WHERE  CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId

				SELECT @CustomEntityDeleteTextBoxCount=COUNT(*) FROM @tmpCustomDeleteTextBox tmpCustomDelete
				
				-- Delete from CustomField Entity Table
				DELETE TOP(@CustomEntityDeleteTextBoxCount) FROM CustomField_Entity
				WHERE CustomField_Entity.EntityId IN(SELECT EntityId FROM @tmpCustomDeleteTextBox)
				AND CustomField_Entity.CustomFieldId IN(SELECT CustomFieldId FROM @tmpCustomDeleteTextBox)

				-- Insert new values of CustomField_Entity tables 
				INSERT INTO CustomField_Entity (EntityId,CustomFieldId,Value,CreatedBy,CreatedDate) 
				SELECT tmpXmlData.[Id#1],CustomField.CustomFieldId,tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'],'''+CAST(@UserId AS VARCHAR(50))+''',GETDATE() FROM #tmpXmlData tmpXmlData 
				CROSS APPLY(SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0 AND CustomField.EntityType=''Budget'')CustomField
				OUTER APPLY (
				SELECT EntityId,CustomFieldEntityId FROM CustomField_Entity CustomFieldEntity WHERE CustomFieldEntity.EntityId=CAST(tmpXmlData.[Id#1] AS INT)
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId
				)CustomFieldEntity WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL AND CustomFieldEntity.CustomFieldEntityId IS NULL 
				
				-- Update values of CustomField_Entity tables 
				UPDATE CustomFieldEntity SET Value=tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] FROM
				CustomField_Entity CustomFieldEntity
				CROSS APPLY (SELECT * FROM #tmpXmlData WHERE CAST([Id#1] AS INT)=CustomFieldEntity.EntityId ) tmpXmlData
				CROSS APPLY (SELECT CustomField.* FROM CustomField WHERE
				CustomField.Name='''+@UpdateColumn+''' AND CustomField.ClientId='''+CAST(@ClientId AS VARCHAR(50))+''' AND CustomField.IsDeleted=0
				AND CustomField.EntityType=''Budget'') CustomField 
				WHERE tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+'] IS NOT NULL
				AND CustomField.CustomFieldId=CustomFieldEntity.CustomFieldId AND CAST(tmpXmlData.[Id#1] AS INT)=CustomFieldEntity.EntityId
				'

			END
			
		END
		
	END
	SET @Ismonth=''
	SET @Count=@Count+1;
	SET @MonthNumber=0;
END
	set @GetBudgetAmoutData='
		Declare @tmpCustomDeleteDropDown TABLE(EntityId BIGINT,CustomFieldId BIGINT)
		Declare @tmpCustomDeleteTextBox TABLE(EntityId BIGINT,CustomFieldId BIGINT)'
		+ @GetBudgetAmoutData

	EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT

	---call sp of pre calculation for marketing budget
	Declare @BudgetId int=(select BudgetId from budget_detail where id=@BudgetDetailId)
	exec PreCalFinanceGridForExistingData @BudgetId
	---end
	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END TRY
BEGIN CATCH
	----Enable trigger for marketing budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount ENABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail ENABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END
	----end
END CATCH
END

GO


