
/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetQuarter]    Script Date: 12/01/2016 12:44:37 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportMarketingBudgetQuarter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ImportMarketingBudgetQuarter]
GO
/****** Object:  StoredProcedure [dbo].[ImportMarketingBudgetQuarter]    Script Date: 12/01/2016 12:44:37 PM ******/
SET ANSI_NULLS ON
GO

CREATE  PROCEDURE [dbo].[ImportMarketingBudgetQuarter]
@XMLData AS XML
,@ImportBudgetCol MarketingBudgetColumns READONLY
,@clientId INT
,@UserId int
,@BudgetDetailId BIGINT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
	----disable trigger For Marketing Budget
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_DetailAmount DISABLE TRIGGER TrgPreCalBudgetForecastMarketingBudget
	END
	IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
	BEGIN
		ALTER TABLE Budget_Detail DISABLE TRIGGER TrgInsertDeletePreCalMarketingBudget
	END

	----end
	CREATE TABLE #tmpXmlData (ROWNUM BIGINT) --create # table because there are dynamic columns added as per file imported for marketing budget
	CREATE TABLE  #childtempData (ROWNUM BIGINT)

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

	-- Declare variable for time frame
	DECLARE @QFirst NVARCHAR(MAX)=''
	DECLARE @QSecond NVARCHAR(MAX)=''
	DECLARE @QThird NVARCHAR(MAX)=''
	-- Declare Variable for forecast/budget column is exist or not

	DECLARE @BudgetOrForecastIndex INT

	SELECT @RowCount = COUNT(*) FROM @ImportBudgetCol

	DECLARE @XmldataQuery NVARCHAR(MAX)=''

	DECLARE @tmpXmlDataAlter NVARCHAR(MAX)=''

	SET @XmldataQuery += 'SELECT ROW_NUMBER() OVER(ORDER BY(SELECT 100)),'
	DECLARE @ConcatColumns NVARCHAR(MAX)=''

	WHILE(@Count<=@RowCount)
	BEGIN
	SELECT @ColName = ColumnName FROM @ImportBudgetCol WHERE ColumnIndex=@Count

	SET @ConcatColumns  += 'pref.value(''(value)['+CAST(@Count AS VARCHAR(50))+']'', ''nvarchar(max)'') as ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'],' --set name of dynamic column

	SET @tmpXmlDataAlter+= ' ALTER TABLE #tmpXmlData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX)
							ALTER TABLE #childtempData ADD ['+@ColName+'#'+CAST(@Count AS VARCHAR(50))+'] NVARCHAR(MAX) ' -- add dynamic columns into table from XML data
	SET @Count=@Count+1;
	END
	SELECT @ConcatColumns=CASE WHEN LEN(@ConcatColumns)>1
						THEN LEFT(@ConcatColumns, LEN(@ConcatColumns) - 1)
							ELSE @ConcatColumns 
						END

	SET @XmldataQuery+= @ConcatColumns+' FROM @XmlData.nodes(''/data/row'') AS People(pref);'

	EXEC(@tmpXmlDataAlter)
	
	Declare @tmpChildBudgets table(Id int,ParentId int, BudgetId int)

	;WITH tblChild AS
	(
		SELECT Id,ParentId,BudgetId
		FROM Budget_Detail WHERE Id = @BudgetDetailId 
		UNION ALL
		SELECT Budget_Detail.Id,Budget_Detail.ParentId,Budget_Detail.BudgetId FROM Budget_Detail  
		CROSS APPLY (SELECT * FROM tblChild WHERE Budget_Detail.ParentId = tblChild.Id) tblChild
	)

	insert into @tmpChildBudgets
	select * from tblChild OPTION(MAXRECURSION 0)

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

	-- Add only Child/Forecast item
	Insert  into #childtempData 
	select #tmpXmlData.* from #tmpXmlData
	inner join (select * from dbo.fnGetBudgetForeCast_List (@BudgetDetailId,@clientId)) child
	on CAST(#tmpXmlData.[Id#1] AS INT) = child.Id

	-- Update Process
	DECLARE @MonthNumber varchar(2) -- variable for month number while import budget data
	DECLARE @ConvertCount nvarchar(5) --variable to set column count with casting

	SET @Count=3;
	WHILE(@Count<=@RowCount)
	BEGIN

		SELECT  @Ismonth = CASE  WHEN  ISNULL(Month,'')='' THEN '' ELSE LTRIM(RTRIM([Month])) END
			,@UpdateColumn = CASE  WHEN  ISNULL(ColumnName,'')='' THEN '' ELSE [ColumnName] END
			,@BudgetOrForecastIndex = ColumnIndex FROM @ImportBudgetCol WHERE ColumnIndex=@Count
		
		SET @ConvertCount=CAST(@Count AS varchar(5))

		IF (@Ismonth<>'')
		BEGIN
			-- Set Time frame based on Quarters
			IF(@IsMonth='Q1')
			BEGIN	
				SET @QFirst ='Y1'
				SET @QSecond ='Y2'
				SET @QThird ='Y3'
			END

			IF(@IsMonth='Q2')
			BEGIN	
				SET @QFirst ='Y4'
				SET @QSecond ='Y5'
				SET @QThird ='Y6'
			END

			IF(@IsMonth='Q3')
			BEGIN	
				SET @QFirst ='Y7'
				SET @QSecond ='Y8'
				SET @QThird ='Y9'
			END

			IF(@IsMonth='Q4')
			BEGIN	
				SET @QFirst ='Y10'
				SET @QSecond ='Y11'
				SET @QThird ='Y12'
			END

		 -- Insert/Update values for budget and forecast
		 IF((@UpdateColumn='Budget' OR @UpdateColumn='Forecast'))
		 BEGIN
			IF(@Ismonth!='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
			BEGIN
			
				SET @GetBudgetAmoutData=' 
				-- Update the Budget Detail amount table for Forecast and Budget values
				UPDATE BudgetDetailAmount
				SET BudgetDetailAmount.['+@UpdateColumn+']=TableData.['+@UpdateColumn+']
				FROM Budget_DetailAmount BudgetDetailAmount
				CROSS APPLY(
				SELECT * FROM (
				SELECT [Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',['+@QThird+'] = CASE WHEN SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+']) > MIN(['+@IsMonth+@UpdateColumn+'])
				THEN 
					CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
					THEN 0
				ELSE 
					SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
				END
				ELSE SUM(['+@QThird+@UpdateColumn+'])
				END  
				,['+@QSecond+'] = CASE WHEN SUM(['+@QThird+@UpdateColumn+']) < (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
				THEN 
					CASE WHEN SUM(['+@QSecond+@UpdateColumn+']) < (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+'])))
				THEN 0
				WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
					THEN 0
				ELSE SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))) 
				END
				ELSE SUM(['+@QSecond+@UpdateColumn+'])
				END
				,['+@QFirst+'] = CASE WHEN 0 > (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
							THEN SUM(['+@QFirst+@UpdateColumn+']) + (SUM(['+@QSecond+@UpdateColumn+']) + (SUM(['+@QThird+@UpdateColumn+'])-(SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))))
					  ELSE 
							CASE WHEN SUM(['+@QFirst+@UpdateColumn+']) > SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
							THEN SUM(['+@QFirst+@UpdateColumn+'])
							ELSE
							SUM(['+@QFirst+@UpdateColumn+']) - (SUM(['+@QFirst+@UpdateColumn+']+['+@QSecond+@UpdateColumn+']+['+@QThird+@UpdateColumn+'])-MIN(['+@IsMonth+@UpdateColumn+']))
							END
						END
				' END +'

				FROM(
				SELECT [Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',SUM(ISNULL(['+@QFirst+@UpdateColumn+'],0)) AS '+@QFirst+@UpdateColumn+'
				,SUM(ISNULL(['+@QSecond+@UpdateColumn+'],0)) AS '+@QSecond+@UpdateColumn+'
				,SUM(ISNULL(['+@QThird+@UpdateColumn+'],0)) AS '+@QThird+@UpdateColumn+'
				,MIN(ISNULL(['+@IsMonth+@UpdateColumn+'],0)) AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM
				(
				-- First Month Of Quarter
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN '
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QFirst+@UpdateColumn+'
				,NULL AS '+@QSecond+@UpdateColumn+'
				,NULL AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QFirst+''') BudgetDetailAmount 
				-- Second Month Of Quarter
				UNION ALL
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QSecond+@UpdateColumn+'
				,NULL AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QSecond+''') BudgetDetailAmount 
				-- Third Month Of Quarter
				UNION ALL
				SELECT tmpXmlData.[Id#1]
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ',NULL AS '+@QFirst+@UpdateColumn+'
				,NULL AS '+@QSecond+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(['+@UpdateColumn+']) = 1 THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850
				--CASE WHEN ['+@UpdateColumn+'] > 0 
				--THEN 
				['+@UpdateColumn+']
				--ELSE 0 END 
				ELSE 0 END AS '+@QThird+@UpdateColumn+'
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850 
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END AS '+@IsMonth+@UpdateColumn+'' END +'
				FROM #childtempData tmpXmlData
				CROSS APPLY(SELECT BudgetDetailId,Period
				'+CASE WHEN ISNULL(@UpdateColumn,'')!='' THEN ','+@UpdateColumn END +'
				FROM Budget_DetailAmount BudgetDetailAmount WHERE 
				tmpXmlData.[Id#1]=BudgetDetailAmount.BudgetDetailId AND Period = '''+@QThird+''') BudgetDetailAmount 
				) AS A
				GROUP BY [Id#1]
				) AS QuarterData
				GROUP BY Id#1
				) As P
				UNPIVOT(
				['+@UpdateColumn+'] FOR Period
				IN(['+@QFirst+'],['+@QSecond+'],['+@QThird+'])
				) as TableData
				WHERE TableData.[Id#1]=BudgetDetailAmount.BudgetDetailId
				AND TableData.Period=BudgetDetailAmount.Period			
				) TableData

				-- Insert into the Budget Detail amount table for Forecast and Budget values if that period values are not exist
				INSERT INTO Budget_DetailAmount (BudgetDetailId,Period,'+@UpdateColumn+')
				SELECT tmpXmlData.[Id#1],'''+@QFirst+'''
				,CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+']) = 1 
				THEN 
				--Commented by Preet Shah on 08/12/2016. For allowed negative values PL #2850 
				--CASE WHEN CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) > 0
				--THEN 
				CAST(REPLACE(tmpXmlData.['+@UpdateColumn+'#'+CAST(@BudgetOrForecastIndex AS varchar(5))+'],'','','''') AS FLOAT) 
				--ELSE 0 END
				ELSE 0 END
				FROM #childtempData tmpXmlData
				OUTER APPLY (SELECT A.BudgetDetailId,A.Period,A.Id FROM Budget_DetailAmount A
				WHERE A.BudgetDetailId = CAST(tmpXmlData.[Id#1] AS INT) 
				AND A.Period IN('''+@QFirst+''','''+@QSecond+''','''+@QThird+''')
				) A WHERE A.Id IS NULL 
				'
				EXECUTE sp_executesql @GetBudgetAmoutData
				SET @GetBudgetAmoutData=''
			END
			Else If(@Ismonth!='' AND @Ismonth='Total' AND @Ismonth!='Unallocated') -- update total budget and Forecast for child items
			Begin
				Declare @UpdateTotal NVARCHAR(max)
				IF(@UpdateColumn='Budget')
				begin
					set @UpdateTotal='UPDATE BudgetDetail set BudgetDetail.TotalBudget= CASE WHEN ISNUMERIC(tmpXmlData.['+@UpdateColumn+'#'+@ConvertCount+']) = 1 
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
		--END
		END
		-- Custom Columns
		IF((@UpdateColumn!='Budget' OR @UpdateColumn!='Forecast'))
		 BEGIN
			IF(@Ismonth='' AND @Ismonth!='Total' AND @Ismonth!='Unallocated')
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
	DECLARE  @tmpCustomDeleteDropDown TABLE(EntityId BIGINT,CustomFieldId BIGINT)
	DECLARE  @tmpCustomDeleteTextBox TABLE(EntityId BIGINT,CustomFieldId BIGINT)'
	+@GetBudgetAmoutData
	EXECUTE sp_executesql @GetBudgetAmoutData, N'@CustomEntityDeleteDropdownCount BIGINT,@CustomEntityDeleteTextBoxCount BIGINT OUT', @CustomEntityDeleteDropdownCount = @CustomEntityDeleteDropdownCount,@CustomEntityDeleteTextBoxCount = @CustomEntityDeleteTextBoxCount OUT

	---call sp of pre calculation for marketing budget
	Declare @BudgetId int=(select BudgetId from budget_detail where id=@BudgetDetailId)
	exec PreCalFinanceGridForExistingData @BudgetId
	-----end

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


