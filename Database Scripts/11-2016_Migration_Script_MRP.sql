/****** Object:  StoredProcedure [INT].[PullLineItemActuals]    Script Date: 11/28/2016 12:26:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[INT].[PullFinancialTransactions]'))
	DROP PROCEDURE [INT].[PullFinancialTransactions]
GO

CREATE PROCEDURE [INT].[PullFinancialTransactions](@DataSource NVARCHAR(255), @ClientID INT, @UserID INT, @IntegrationInstanceID INT)
AS
BEGIN 
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @Start DATETIME = GETDATE()
		
	--DELETE, UPDATE AND INSERT plan tactic actuals for stage title ProjectedStageValue which match with measure sfdc tactics	
	SET @CustomQuery='

		DECLARE @Updated INT;
		DECLARE @Inserted INT;
		DECLARE @Start DATETIME = GETDATE();

		DECLARE @LastDate DATETIME

		SELECT @LastDate = MAX(DateCreated) FROM dbo.Transactions;
		IF (@LastDate IS NULL) SET @LastDate = DATEADD(YEAR,-2,GETDATE()) --goes 2 years back 

		UPDATE dbo.Transactions 
		SET 
			 ClientTransactionID = V.ClientTransactionID 
			,TransactionDescription = V.TransactionDescription
			,Amount = V.Amount 
			,Account = V.Account
			,AccountDescription = V.AccountDescription 
			,SubAccount = V.SubAccount 
			,Department = V.Department 
			,TransactionDate = V.TransactionDate 
			,AccountingDate = V.AccountingDate 
			,Vendor = V.Vendor 
			,PurchaseOrder = V.PurchaseOrder 
			,LineItemId = V.LineItemId 
			,CustomField1 = V.CustomField1 
			,CustomField2 = V.CustomField2 
			,CustomField3 = V.CustomField3 
			,CustomField4 = V.CustomField4 
			,CustomField5 = V.CustomField5 
			,CustomField6 = V.CustomField6
			,DateCreated = V.DateCreated 
	
		FROM '+@DataSource+' V
		WHERE V.DateCreated > @LastDate 
			AND dbo.Transactions.ClientTransactionID = V.ClientTransactionID
			AND dbo.Transactions.ClientId = '+STR(@ClientId)+'

		SET @Updated = @@ROWCOUNT; 

		INSERT INTO dbo.Transactions
				   ( ClientID 
					,ClientTransactionID
					,TransactionDescription
					,Amount
					,Account 
					,AccountDescription 
					,SubAccount 
					,Department 
					,TransactionDate 
					,AccountingDate 
					,Vendor 
					,PurchaseOrder 
					,CustomField1 
					,CustomField2 
					,CustomField3 
					,CustomField4 
					,CustomField5 
					,CustomField6 
					,LineItemId 
					,DateCreated
				)

		SELECT  ' + STR(@ClientID) + ' 
				,V.ClientTransactionID 
				,V.TransactionDescription 
				,V.Amount 
				,V.Account 
				,V.AccountDescription 
				,V.SubAccount 
				,V.Department 
				,V.TransactionDate 
				,V.AccountingDate 
				,V.Vendor 
				,V.PurchaseOrder 
				,V.CustomField1 
				,V.CustomField2 
				,V.CustomField3 
				,V.CustomField4 
				,V.CustomField5 
				,V.CustomField6 
				,V.LineItemId 
				,V.DateCreated
		FROM ' + @DataSource + ' V 
			LEFT JOIN dbo.Transactions T ON T.ClientTransactionID = T.ClientTransactionID
		WHERE  V.DateCreated > @LastDate 
			AND T.ClientTransactionID IS NULL


		SET @Inserted = @@ROWCOUNT;

		INSERT INTO [dbo].[IntegrationInstanceLog] ( 
			   [IntegrationInstanceID]
			  ,[SyncStart]
			  ,[SyncEnd]
			  ,[Status]
			  ,[ErrorDescription]
			  ,[CreatedDate]
			  ,[CreatedBy]
			  ,[IsAutoSync]) 
		SELECT ' + STR(@IntegrationInstanceID) + '
			, @Start
			, GETDATE()
			, ''SUCCESS'' 
			, ''Pulled Financial Transactions From'' +''' + @DataSource + '''+STR(@Updated) + '' Updated.'' + STR(@Inserted) + '' Inserted.'' 
			, GETDATE()
			, ' + STR(@UserID) + '
			, 1  

	'
		BEGIN TRY 
			--PRINT @CustomQuery;
			EXEC (@CustomQuery)
		END TRY 

		BEGIN CATCH 
			INSERT INTO [dbo].[IntegrationInstanceLog] ( 
				   [IntegrationInstanceID]
				  ,[SyncStart]
				  ,[SyncEnd]
				  ,[Status]
				  ,[ErrorDescription]
				  ,[CreatedDate]
				  ,[CreatedBy]
				  ,[IsAutoSync]) 
			SELECT @IntegrationInstanceID, @Start, GETDATE(), 'ERROR' ,ERROR_MESSAGE(), GETDATE(), @UserID, 1  
		END CATCH 

END 
GO

--NOTE: this is a correction of existing function used in integration -zz
/****** Object:  UserDefinedFunction [INT].[Period]    Script Date: 11/23/2016 2:21:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [INT].[Period](@tacticStartDate DATE, @actualDate DATE)
RETURNS VARCHAR(10)
AS 
BEGIN

    RETURN 
	CASE WHEN DATEPART(YEAR, @tacticStartDate)<DATEPART(YEAR, @actualDate)
		 THEN 'Y' + CAST(((DATEPART(YEAR, @actualDate) - DATEPART(YEAR, @tacticStartDate))*12 + DATEPART(MONTH, @actualDate)) AS VARCHAR(10))
		 ELSE 'Y' +  CAST(DATEPART(MONTH, @actualDate) AS VARCHAR(10))
	END
END
GO
/****** Object:  View [dbo].[CampaignDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[CampaignDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[CampaignDetail];
GO

CREATE VIEW [dbo].[CampaignDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, C.Title, C.Description, C.StartDate, C.EndDate, C.CreatedDate, C.ModifiedDate, C.IsDeleted, C.Status, C.IsDeployedToIntegration, C.IntegrationInstanceCampaignId, 
                  C.LastSyncDate, C.CampaignBudget, C.Abbreviation, C.IntegrationWorkFrontProgramID, C.CreatedBy, C.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId

GO
/****** Object:  View [dbo].[LineItemDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[LineItemDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[LineItemDetail];
GO

CREATE VIEW [dbo].[LineItemDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, L.PlanLineItemId, L.PlanTacticId, L.LineItemTypeId, L.Title, L.Description, L.StartDate, L.EndDate, L.Cost, L.IsDeleted, L.CreatedDate, L.ModifiedDate, 
                  L.LinkedLineItemId, L.ModifiedBy, L.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic_LineItem AS L ON L.PlanTacticId = T.PlanTacticId

GO
/****** Object:  View [dbo].[PlanDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[PlanDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[PlanDetail];
GO

CREATE VIEW [dbo].[PlanDetail]
AS
SELECT M.ClientId, P.PlanId, P.ModelId, P.Title, P.Version, P.Description, P.Budget, P.Status, P.IsActive, P.IsDeleted, P.CreatedDate, P.ModifiedDate, P.Year, P.GoalType, P.GoalValue, P.AllocatedBy, P.EloquaFolderPath, 
                  P.DependencyDate, P.CreatedBy, P.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId

GO
/****** Object:  View [dbo].[ProgramDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[ProgramDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[ProgramDetail];
GO

CREATE VIEW [dbo].[ProgramDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, Pr.PlanProgramId, Pr.PlanCampaignId AS Expr1, Pr.Title, Pr.Description, Pr.StartDate, Pr.EndDate, Pr.CreatedDate, Pr.ModifiedDate, Pr.IsDeleted, Pr.Status, 
                  Pr.IsDeployedToIntegration, Pr.IntegrationInstanceProgramId, Pr.LastSyncDate, Pr.ProgramBudget, Pr.Abbreviation, Pr.CreatedBy, Pr.ModifiedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId

GO
/****** Object:  View [dbo].[TacticDetail]    Script Date: 11/23/2016 12:06:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[TacticDetail]', 'V') IS NOT NULL
    DROP VIEW [dbo].[TacticDetail];
GO

CREATE VIEW [dbo].[TacticDetail]
AS
SELECT M.ClientId, P.PlanId, C.PlanCampaignId, T.PlanTacticId, T.PlanProgramId, T.TacticTypeId, T.Title, T.Description, T.StartDate, T.EndDate, T.Cost, T.TacticBudget, T.Status, T.CreatedDate, T.ModifiedDate, T.IsDeleted, 
                  T.IsDeployedToIntegration, T.IntegrationInstanceTacticId, T.LastSyncDate, T.ProjectedStageValue, T.StageId, T.TacticCustomName, T.IntegrationWorkFrontProjectID, T.IntegrationInstanceEloquaId, T.LinkedTacticId, 
                  T.LinkedPlanId, T.IsSyncSalesForce, T.IsSyncEloqua, T.IsSyncWorkFront, T.IsSyncMarketo, T.IntegrationInstanceMarketoID, T.ModifiedBy, T.CreatedBy
FROM     dbo.[Plan] AS P INNER JOIN
                  dbo.Model AS M ON M.ModelId = P.ModelId INNER JOIN
                  dbo.Plan_Campaign AS C ON C.PlanId = P.PlanId INNER JOIN
                  dbo.Plan_Campaign_Program AS Pr ON Pr.PlanCampaignId = C.PlanCampaignId INNER JOIN
                  dbo.Plan_Campaign_Program_Tactic AS T ON T.PlanProgramId = Pr.PlanProgramId


GO

/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExportToCSV]
GO
/****** Object:  StoredProcedure [dbo].[ExportToCSV]    Script Date: 10/27/2016 5:00:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ExportToCSV] AS' 
END
GO
ALTER PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@clientId INT=0
,@HoneyCombids nvarchar(max)=null
,@CurrencyExchangeRate FLOAT=1
AS
BEGIN

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
,'--' as TacticCategory
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost','--' AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost','--' AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',([Tactic].Cost*@CurrencyExchangeRate) As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,TacticType.AssetType as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title,AssetType FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',([lineitem].Cost*@CurrencyExchangeRate) As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
,'--' as TacticCategory
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
TacticCategory NVARCHAR(MAX),
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy INT,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		TacticCategory,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	--PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	--PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	PRINT(@MergeData)
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
			IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
			BEGIN
				IF(@Val='Col_PlannedCost')
				BEGIN
					SET @UpdateStatement+='  @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ ' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ '] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),CAST(['
													+ (SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
													+ '] AS decimal(38,2))) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
													+ '+'';''+ CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
													+ '] AS decimal(38,2))) END'+@Delimeter
				END
				ELSE 
				BEGIN
					/*Add Condition - If @Val is tactic or startdate or enddate or targetstagegoal or tacticcategory then no need to concat values.*/
					IF (@Val!='Col_Tactic' AND @Val!='Col_StartDate' AND @Val!='Col_EndDate' AND @Val!='Col_TargetStageGoal' AND @Val != 'Col_TacticCategory')
					BEGIN
						SET @UpdateStatement += '  @Col_' + ( SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@') ) 
														  + ' = ['+ (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														  + '] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['
														  + ( SELECT REPLACE(REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														  + ']) ELSE @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														  + '+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														  + ']) END'+@Delimeter
					END
					ELSE
					BEGIN
						SET @UpdateStatement+='  @Col_' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														+ ' = [' + (SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))
														+ '] = CONVERT(NVARCHAR(MAX),[' + (SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))
														+ '])'+@Delimeter
					END
				END
			END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	--PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic','Lineitem')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End
END
GO

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransactionLineItemMapping'))
DROP TABLE [dbo].[TransactionLineItemMapping]

IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Transactions'))
DROP TABLE [dbo].[Transactions]
GO

/****** Object:  Table [dbo].[Transactions]    Script Date: 11/15/2016 6:04:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Transactions](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientID] [int] NOT NULL,
	[ClientTransactionID] [varchar](150) NOT NULL,
	[TransactionDescription] [varchar](250) NULL,
	[Amount] [numeric](18, 0) NOT NULL,
	[Account] [varchar](150) NULL,
	[AccountDescription] [varchar](150) NULL,
	[SubAccount] [varchar](150) NULL,
	[Department] [varchar](150) NULL,
	[TransactionDate] [datetime] NULL,
	[AccountingDate] [datetime] NOT NULL,
	[Vendor] [varchar](150) NULL,
	[PurchaseOrder] [varchar](150) NULL,
	[CustomField1] [varchar](150) NULL,
	[CustomField2] [varchar](150) NULL,
	[CustomField3] [varchar](150) NULL,
	[CustomField4] [varchar](150) NULL,
	[CustomField5] [varchar](150) NULL,
	[CustomField6] [varchar](150) NULL,
	[LineItemId] [int] NULL,
	[DateCreated] [datetime] NOT NULL,
	[AmountAttributed] [float] NULL,
	[LastProcessed] [datetime] NULL,
 CONSTRAINT uc_ClientID_ClientTransactionId UNIQUE (ClientID, ClientTransactionId),
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TransactionLineItemMapping]    Script Date: 11/15/2016 6:01:21 PM ******/
CREATE TABLE [dbo].[TransactionLineItemMapping](
	[TransactionLineItemMappingId] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[LineItemId] [int] NOT NULL,
	[Amount] [float] NULL,
	[DateModified] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[DateProcessed] [datetime] NULL,
 CONSTRAINT uc_PersonID UNIQUE (TransactionId,LineItemId),
 CONSTRAINT [PK_TransactionLineItemMapping] PRIMARY KEY CLUSTERED 
(
	[TransactionLineItemMappingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([LineItemId])
REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Plan_Campaign_Program_Tactic_LineItem]
GO

ALTER TABLE [dbo].[TransactionLineItemMapping]  WITH CHECK ADD  CONSTRAINT [FK_TransactionLineItemMapping_Transactions] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[Transactions] ([TransactionId])
GO

ALTER TABLE [dbo].[TransactionLineItemMapping] CHECK CONSTRAINT [FK_TransactionLineItemMapping_Transactions]
GO

/****** Object:  StoredProcedure [dbo].[GetLinkedLineItemsForTransaction]    Script Date: 11/20/2016 4:09:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[GetLinkedLineItemsForTransaction]'))
DROP PROCEDURE [dbo].[GetLinkedLineItemsForTransaction];
GO 

CREATE PROCEDURE [dbo].[GetLinkedLineItemsForTransaction](@TransactionId INT)
AS 
BEGIN 

	--dataset 1: tactic data in context of a transaction
	SELECT T.PlanTacticId AS TacticId
			, T.Title
			, T.Cost AS PlannedCost
			, SUM(ISNULL(M.Amount, 0.0)) AS TotalLinkedCost --only the portion of the transaction that is linked to this tactic
			, SUM(ISNULL(LA.Value, 0.0)) AS TotalActual

	FROM Plan_Campaign_Program_tactic T
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanTacticId = T.PlanTacticId
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
	WHERE M.TransactionId = @TransactionId
	GROUP BY T.PlanTacticId, T.Title, T.Cost

	--dataset 2: line items linked to the @transaction
	SELECT    L.PlanTacticId AS TacticId
			, L.PlanLineItemId -- this the prmary key, the rest of non aggregate columns are auxiliary info.
			, L.Title
			, L.Cost AS Cost
            , T.Title AS TacticTitle
			, P.Title AS ProgramTitle
			, C.Title AS CampaignTitle
			, PL.Title AS PlanTitle
            , M.TransactionLineItemMappingId
            , M.TransactionId
            , SUM(M.Amount) AS TotalLinkedCost -- SUM is a no-op as a transaction can only be linked once per line item
			, SUM(ISNULL(LA.Value, 0.0)) AS Actual

	FROM dbo.Plan_Campaign_Program_Tactic_LineItem L
        JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
        JOIN dbo.Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId
        JOIN dbo.Plan_Campaign C ON C.PlanCampaignId = P.PlanCampaignId
        JOIN dbo.[Plan] PL ON PL.PlanId = C.PlanId
		LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual LA ON LA.PlanLineItemId = L.PlanLineItemId
		JOIN dbo.TransactionLineItemMapping M ON M.LineItemId = L.PlanLineItemId
	WHERE M.TransactionId = @TransactionId
	GROUP BY L.PlanTacticId, L.PlanLineItemId, L.Title, L.Cost, T.Title, P.Title, C.Title, PL.Title, M.TransactionLineItemMappingId, M.TransactionId

END 
GO

/****** Object:  Trigger [dbo].[UpdateCustomFieldEntityRestrictedTextByUser]    Script Date: 11/22/2016 11:56:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.triggers WHERE OBJECT_ID = OBJECT_ID('[dbo].TransactionCostToLineItemAttribution'))
	DROP TRIGGER  [dbo].[TransactionCostToLineItemAttribution]
GO

CREATE TRIGGER [dbo].[TransactionCostToLineItemAttribution] 
   ON  [dbo].[TransactionLineItemMapping] 
   AFTER INSERT,UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	--Handle delete (reall delete on this table)
	DELETE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual
	FROM DELETED D 
		JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = D.LineItemId
		JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
		JOIN dbo.Transactions TX ON TX.TransactionId = D.TransactionId
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = D.LineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = INT.Period(T.StartDate, TX.AccountingDate)  

    -- We need set the createdBy and CreatedDate for both inserts and updates. 
	-- NOTE: there is no modified on line item actuals   
    DECLARE @CreatedBy INT
	DECLARE @CreatedDate DATETIME
	SELECT TOP 1  @CreatedBy = INSERTED.ModifiedBy
			, @CreatedDate = INSERTED.ModifiedBy 
	FROM INSERTED
	
	---Handle updates 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET   Value = A.Amount
		, CreatedBy = @CreatedBy
		, CreatedDate =@CreatedDate
	FROM ( 
			SELECT SUM(M.Amount) AS Amount
					, L.PlanLineItemId
					, A.Period
			FROM	dbo.TransactionLineItemMapping M 
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
					JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
					JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
					JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId
	
			WHERE A.Period = INT.Period(T.StartDate, TX.AccountingDate) 
				  AND M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
			GROUP BY L.PlanLineItemId, A.Period
	) A
	WHERE A.PlanLineItemId = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId
		AND A.Period = dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period

	-- Handle inserts 
	INSERT INTO  dbo.Plan_Campaign_Program_Tactic_LineItem_Actual(Value, PlanLineItemId, Period, CreatedDate, CreatedBy)
	SELECT SUM(M.Amount) AS Amount
			, L.PlanLineItemId
			, INT.Period(T.StartDate, TX.AccountingDate)
			, @CreatedDate
			, @CreatedBy
	FROM	dbo.TransactionLineItemMapping M 
			JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = M.LineItemId
			JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId
			JOIN dbo.Transactions TX ON TX.TransactionId = M.TransactionId
			LEFT JOIN dbo.Plan_Campaign_Program_Tactic_LineItem_Actual A ON A.PlanLineItemId = L.PlanLineItemId 
				AND A.Period = INT.Period(T.StartDate, TX.AccountingDate) 	
	WHERE A.PlanLineItemId IS NULL 
		AND M.LineItemId IN (SELECT M.LineItemId FROM INSERTED)
	GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)

	--Consider the update for 1 to 1 mapped transactions 
	--NOTE: this step is additive to the above 2 steps 
	UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual 
	SET Value = Value + A.Amount
	FROM (
			SELECT SUM(TX.Amount) AS Amount
					, L.PlanLineItemId
					, INT.Period(T.StartDate, TX.AccountingDate) AS Period
			FROM dbo.Transactions TX 
				JOIN dbo.Plan_Campaign_Program_Tactic_LineItem L ON L.PlanLineItemId = TX.LineItemId
				JOIN dbo.Plan_Campaign_Program_Tactic T ON T.PlanTacticId = L.PlanTacticId 
			WHERE TX.LineItemId IN (SELECT Inserted.LineItemId FROM INSERTED) 
			GROUP BY L.PlanLineItemId, INT.Period(T.StartDate, TX.AccountingDate)
		) A
	WHERE dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.PlanLineItemId = A.PlanLineItemId 
		AND dbo.Plan_Campaign_Program_Tactic_LineItem_Actual.Period = A.Period

END
Go
-- =============================================
-- Author: Rahul Shah 
-- Create date: 11/21/2016
-- Description:	to Add TotalBudget & TotalForcast in Budget_Detail table to calculate unallocated cost.
-- =============================================
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalBudget')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalBudget] float null 
   
END
GO
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForcast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	DROP Column [TotalForcast] 
   
END
GO
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Budget_Detail' AND COLUMN_NAME = 'TotalForecast')
BEGIN

    ALTER TABLE [dbo].[Budget_Detail] 
	ADD [TotalForecast] float null
   
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Team]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[Plan_Team]
END
GO
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataMonthly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanCostDataMonthly] 
END
GO
CREATE PROCEDURE [dbo].[ImportPlanCostDataMonthly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetMonthData READONLY,
@UserId INT
--@ClientId INT
AS
BEGIN
DECLARE @OutputTable TABLE (ActivityId INT,Type NVARCHAR(50),Name NVARCHAR(255))
SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0)) 
  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

--end tactic
UNION
--start line item
select ActivityId,[Task Name],'LineItem' as ActivityType,0 as Budget,Y1 AS JAN,Y2 AS FEB,Y3 AS MAR,Y4 AS APR,Y5 AS MAY,Y6 AS JUN, Y7 AS JUL, Y8 AS AUG, Y9 AS SEP, Y10 AS OCT, Y11 AS NOV, Y12 AS DEC from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
(
 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_tactic_Lineitem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData

select * into #temp2 from (select * from @ImportData )   k
--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, JAN,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC from #Temp)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.JAN,T1.FEB,T1.MAR,T1.APR,T1.MAY,T1.JUN,T1.JUL,T1.AUG,T1.SEP,T1.OCT,T1.NOV,T1.DEC, T2.ActivityType 
from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE])

 TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData



IF ( LOWER(@Type)='tactic')
	
	BEGIN
    
	IF Exists (select top 1 PlanTacticId from [Plan_Campaign_Program_Tactic] where PlanTacticId =  @EntityId )
			BEGIN

			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

			--update balance lineitem
			--update balance lineitem Cost of tactic -(sum of line item)
			
			IF((SELECT Top 1  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null and isdeleted=0) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y1' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
			      from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
				END
				  ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId), GETDATE(),@UserId)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
					
		
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y2' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y2'
				END
				ELSE
					BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		 
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y3' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y3' 
				END
			ELSE
					BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
				END
					ELSE
					BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

		

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y5' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y5' 
				END

				ELSE
					BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y6' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y6' 
				END
			ELSE
					BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
				END
		ELSE
					BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END



			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y8' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y8' 
				END
				ELSE
					BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END
		  --  ELSE
	
			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y9' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y9' 
				END
		ELSE
					BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END


			
			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
				END
	

	ELSE
					BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y11' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y11' 
				END
	
					ELSE
					BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

			IF EXISTS (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId AND Period = 'Y12' )
				BEGIN
					UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
			from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y12' 
				END

					ELSE
					BEGIN
					IF ((SELECT [DEC] from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   INSERT  INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)
					
				END

	
		
			END

	 ELSE
		BEGIN
			INSERT INTO @OutputTable (ActivityId,[Type],Name) Values (@EntityId,@Type,'') 
		END

	END
	

IF ( LOWER(@Type)='lineitem')
		BEGIN
			IF Exists (select top 1 PlanLineItemId from [Plan_Campaign_Program_Tactic_LineItem] where PlanLineItemId =  @EntityId)
			BEGIN		
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN

			--update lineitem cost
				UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId

				--update balance row
			IF((SELECT Top 1 ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null and isdeleted=0) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END


			--Y1
			IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JAN != '' THEN T.JAN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
				END
				ELSE
				BEGIN
					IF ((SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y1', (SELECT JAN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

	
             --Y2
			 	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y2')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.FEB != '' THEN T.FEB ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y2'
				END
				ELSE
				BEGIN
					IF ((SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y2', (SELECT FEB from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
			---Y3
				IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y3')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAR != '' THEN T.MAR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y3'
				END
				ELSE
				BEGIN
					IF ((SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y3', (SELECT MAR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 

----Y4

	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.APR != '' THEN T.APR ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
				END
				ELSE
				BEGIN
					IF ((SELECT APR from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y4', (SELECT APR from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
--Y5
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y5')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.MAY != '' THEN T.MAY ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y5'
				END
				ELSE
				BEGIN
					IF ((SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y5', (SELECT MAY from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		

---Y6
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y6')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUN != '' THEN T.JUN ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y6'
				END
				ELSE
				BEGIN
					IF ((SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y6', (SELECT JUN from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END

---y7
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.JUL != '' THEN T.JUL ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
				END
				ELSE
				BEGIN
					IF ((SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y7', (SELECT JUL from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
--Y8
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y8')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.AUG != '' THEN T.AUG ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y8'
				END
				ELSE
				BEGIN
					IF ((SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y8', (SELECT AUG from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y9
	IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y9')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.SEP != '' THEN T.SEP ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y9'
				END
				ELSE
				BEGIN
					IF ((SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y9', (SELECT SEP from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
				--Y10
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.OCT != '' THEN T.OCT ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
				END
				ELSE
				BEGIN
					IF ((SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y10', (SELECT OCT from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
	
				--Y11
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y11')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.NOV != '' THEN T.NOV ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y11'
				END
				ELSE
				BEGIN
					IF ((SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y11', (SELECT NOV from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		
				--Y12
					IF EXISTS (SELECT * from Plan_Campaign_Program_tactic_Lineitem_Cost WHERE PlanLineItemId = @EntityId AND Period = 'Y12')
				BEGIN
						UPDATE P SET P.Value = CASE WHEN T.DEC != '' THEN T.DEC ELSE P.Value END
						from Plan_Campaign_Program_tactic_Lineitem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y12'
				END
				ELSE
				BEGIN
					IF ((SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)				 
				   INSERT  INTO Plan_Campaign_Program_tactic_Lineitem_Cost  VALUES (@EntityId,'Y12', (SELECT DEC from #TempDiffer WHERE ActivityId = @EntityId),GETDATE(),@UserId)					
				END
		 END
			END
		END
 set @cnt = @cnt + 1


  DROP TABLE #TempDiffer

End
--select ActivityId from @ImportData  EXCEPT select ActivityId from #Temp
--select * from @OutputTable
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp

END
GO
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'ImportPlanCostDataQuarterly') AND type IN ( N'P', N'PC' ) ) 
BEGIN

	DROP PROCEDURE [dbo].[ImportPlanCostDataQuarterly] 
END
GO
CREATE PROCEDURE [dbo].[ImportPlanCostDataQuarterly]  --17314
--@PlanId int,
@ImportData ImportExcelBudgetQuarterData READONLY,
@UserId INT 
--@ClientId INT
AS
BEGIN

SELECT *
INTO #Temp
FROM (

select ActivityId,[Task Name],'Tactic' as ActivityType,Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanTacticId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],CASE WHEN Budget IS NULL THEN '0' ELSE convert(varchar(max),Budget) END AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.IsDeleted,b.PlanProgramId, b.PlanTacticId, value as value,Period,b.Title,0 as Budget from Plan_Campaign_Program_Tactic_Cost as a 
right join Plan_Campaign_Program_Tactic as b on a.PlanTacticId=b.PlanTacticId 
) as t
where IsDeleted=0 and PlanProgramId in (select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and
 PlanCampaignId in ( select PlanCampaignId from Plan_Campaign where PlanId 
in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan')
 and IsDeleted=0))   
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name],Budget, Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12
UNION
select ActivityId,[Task Name],'lineitem' as ActivityType,0 as Budget,Sum(Y1+Y2+Y3) AS Q1,Sum(Y4+Y5+Y6) AS Q2,Sum(Y7+Y8+Y9) AS Q3,Sum(Y10+Y11+Y12) AS Q4 from (
select Convert(varchar(max),[PlanLineItemId]) as ActivityId,REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Title, '&amp;', '&'), '&quot;', '"'), '&lt;', '<'), '&gt;', '>'), '&amp;amp;', '&') as [Task Name],
0 AS Budget
,case when Y1 is null then 0 else Y1 end as Y1,case when Y2 is null then 0 else Y2 end as Y2,case when Y3 is null then 0 else Y3 end as Y3,case when Y4 is null then 0 else Y4 end as Y4,case when Y5 is null then 0 else Y5 end as Y5,case when Y6 is null then 0 else Y6 end as Y6,case when Y7 is null then 0 else Y7 end as Y7,case when Y8 is null then 0 else Y8 end as Y8,case when Y9 is null then 0 else Y9 end as Y9,case when Y10 is null then 0 else Y10 end as Y10,case when Y11 is null then 0 else Y11 end as Y11,case when Y12 is null then 0 else Y12 end as Y12
from
( 
 select * from(
select b.PlanLineItemId,b.PlanTacticId,Value,Period,b.Title  from Plan_Campaign_Program_Tactic_LineItem_Cost as a 
right join Plan_Campaign_Program_Tactic_LineItem as b on a.PlanLineItemId=b.PlanLineItemId
) as t
where  PlanTacticId in (select PlanTacticId from Plan_Campaign_Program_Tactic where IsDeleted =0 and PlanProgramId in ( 
select PlanProgramId from Plan_Campaign_Program where IsDeleted =0 and PlanCampaignId in(select PlanCampaignId from Plan_Campaign where PlanId in(  SELECT ActivityId FROM @ImportData WHERE LOWER([TYPE])='plan') and IsDeleted=0)))  
) t
pivot
(
  SUM(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramTacticDetails
) as rPlanCampaignProgramTactic group by ActivityId,[Task Name], Y1,Y2,Y3,Y4,Y5,Y6,Y7,Y8,Y9,Y10,Y11,Y12

) as ExistingData



--select * into #temp2 from (select * from @ImportData EXCEPT select ActivityId,ActivityType,[Task Name],Budget, Q1,Q2,Q3,Q4 from #Temp)   k
select * into #temp2 from (select * from @ImportData)   k


--select * from @ImportData EXCEPT select * from #Temp
select * into #TempFinal from
(select T1.ActivityId,T1.[Task Name],T1.Budget,T1.Q1,T1.Q2,T1.Q3,T1.Q4,T2.ActivityType from #temp2 AS T1 inner join #Temp AS T2 ON  T1.ActivityId = T2.ActivityId WHERE T2.ActivityType=t1.[TYPE]) TempInner

Declare @Type varchar(10)
Declare @EntityId int
Declare @Title int
Declare @cnt int =0
declare @total int = (Select Count(*) From #TempFinal)
While (@cnt<@total)
Begin

 set @Type = ( SELECT  ActivityType FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 set @EntityId = (SELECT  ActivityId FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY)

 SELECT * into #TempDiffer from (SELECT  * FROM #TempFinal
                              ORDER BY ActivityId
                              OFFSET @cnt ROWS
                              FETCH NEXT 1 ROWS ONLY) tempData
Declare @Sum float;	DECLARE @newValue FLOAT;
	



IF (LOWER(@Type)='tactic')
		BEGIN
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic where PlanTacticId =  @EntityId )
			BEGIN				

			--update tactic cost
			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic] P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId

				--update balance lineitem
			
			IF((SELECT Top 1  ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanTacticId=@EntityId and LineItemTypeId IS null and isdeleted=0) = 0)
				BEGIN
					  UPDATE Plan_Campaign_Program_Tactic_LineItem SET  
					  COST=((Select cost from Plan_Campaign_Program_tactic where PlanTacticId=@EntityId)-(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=@EntityId)) 
					  Where LineItemTypeId is null and PlanTacticId=@EntityId
				END


				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataActualTactic') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataActualTactic
			END 
			SELECT * INTO #tempDataActualTactic FROM (SELECT * from Plan_Campaign_Program_Tactic_Cost WHERE PlanTacticId = @EntityId ) a 
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y1' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y4' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y4' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y7' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y7' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataActualTactic where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='Tactic'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataActualTactic WHERE PlanTacticId = @EntityId AND Period = 'Y10' )
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_Cost P INNER JOIN #TempDiffer T on P.PlanTacticId = T.ActivityId WHERE P.PlanTacticId = @EntityId AND Period = 'Y10' 
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_Cost (PlanTacticId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC Tactic_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END			
		
		 END
       --complete end
		END

IF (LOWER(@Type)='lineitem')
		BEGIN
	
		IF Exists (select top 1 PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where PlanLineItemId =  @EntityId)
			BEGIN
			
			--If line item type will be other line item then it will not be updated.
			IF((SELECT ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem Where PlanLineItemId=@EntityId) != 0)
			BEGIN


			UPDATE P SET P.Cost = CASE WHEN T.Budget != '' THEN T.Budget ELSE P.Cost END
			from [Plan_Campaign_Program_Tactic_LineItem] P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId



			--update balance row
			IF((SELECT top 1 ISNULL(LineItemTypeId,0) from Plan_Campaign_Program_tactic_Lineitem 
			Where PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId) and LineItemTypeId is null and isdeleted=0) = 0)
				BEGIN
				  UPDATE Plan_Campaign_Program_Tactic_LineItem SET 
				  COST=((Select cost from Plan_Campaign_Program_tactic WHERE PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId))
				  -(Select ISNULL(sum(cost),0) from Plan_Campaign_Program_tactic_Lineitem Where LineItemTypeId is not null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem 
				  where  PlanLineItemId=@EntityId))) 
				  Where LineItemTypeId is null and PlanTacticId=(Select PlanTacticId from Plan_Campaign_Program_Tactic_LineItem where  PlanLineItemId=@EntityId)
			END





				IF EXISTS (SELECT * FROM tempdb.sys.objects WHERE object_id = OBJECT_ID(N'tempdb..#tempDataLineItemActual') AND type in (N'U'))
			BEGIN
				DROP TABLE #tempDataLineItemActual
			END 
			
			SELECT * INTO #tempDataLineItemActual FROM (SELECT * from Plan_Campaign_Program_Tactic_LineItem_Cost WHERE PlanLineItemId = @EntityId) a 

			

			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y1','Y2','Y3')		
			SELECT @newValue=Q1 from #TempDiffer WHERE ActivityId = @EntityId and LOWEr([ActivityType])='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y1')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q1 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y1'
						END
					ELSE
						BEGIN
					IF ((SELECT Q1 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y1', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END
				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,1,@newValue			
				END
			END
			
			SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y4','Y5','Y6')		
			SELECT @newValue=Q2 from #TempDiffer WHERE ActivityId = @EntityId and [ActivityType]='lineitem'
			
				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y4')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q2 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y4'
						END
					ELSE
						BEGIN
					IF ((SELECT Q2 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				   -- INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
				   INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId, 'Y4', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,2,@newValue			
				END
			END
			

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y7','Y8','Y9')		
			SELECT @newValue=Q3 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y7')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q3 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y7'
						END
					ELSE
						BEGIN
					IF ((SELECT Q3 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				  --  INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y7', @newValue-@Sum, GETDATE(),@UserId)
				  INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y7', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,3,@newValue			
				END
			END

				SELECT @Sum=ISNULL(ISNULL(SUM(value),0),0) from #tempDataLineItemActual where Period in('Y10','Y11','Y12')		
			SELECT @newValue=Q4 from #TempDiffer WHERE ActivityId = @EntityId and LOWER([ActivityType])='lineitem'

				IF(@newValue!=@Sum)
			BEGIN
				if(@Sum<@newValue)
					BEGIN
						IF EXISTS (SELECT * from #tempDataLineItemActual WHERE PlanLineItemId = @EntityId AND Period = 'Y10')
						BEGIN
					UPDATE P SET P.Value = CASE WHEN T.Q4 != '' THEN p.Value+(@newValue-@Sum) ELSE P.Value END 
			     from Plan_Campaign_Program_Tactic_LineItem_Cost P INNER JOIN #TempDiffer T on P.PlanLineItemId = T.ActivityId WHERE P.PlanLineItemId = @EntityId AND Period = 'Y10'
						END
					ELSE
						BEGIN
					IF ((SELECT Q4 from #TempDiffer WHERE ActivityId = @EntityId) IS NOT NULL)
				    --INSERT INTO Plan_Campaign_Program_Tactic_Cost VALUES (@EntityId, 'Y10', @newValue-@Sum, GETDATE(),@UserId)
					INSERT INTO Plan_Campaign_Program_Tactic_LineItem_Cost (PlanLineItemId,Period,Value,CreatedDate,CreatedBy) VALUES (@EntityId,'Y10', @newValue-@Sum, GETDATE(),@UserId)
						END
		         END

				   ELSE
					BEGIN
					EXEC LineItem_CostQuarterCalculation @EntityId,4,@newValue			
				END
			END		
			
			
				
		END
	END

		END




 set @cnt = @cnt + 1
  DROP TABLE #TempDiffer

End
select ActivityId from @ImportData where TYPE not in('plan')  EXCEPT select ActivityId from #Temp
END

--Insertation End #2623 import multiple plan
Go

-- Start - Added by Arpita Soni for Ticket #2790 on 11/28/2016

/****** Object:  Table [MV].[PreCalculatedMarketingBudget]    Script Date: 11/18/2016 03:00:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Add new table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MV].[PreCalculatedMarketingBudget]') AND type in (N'U'))
BEGIN
	CREATE TABLE [MV].[PreCalculatedMarketingBudget](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[BudgetDetailId] [int] NULL,
		[Year] [int] NULL,
		[Y1_Budget] [float] NULL,
		[Y2_Budget] [float] NULL,
		[Y3_Budget] [float] NULL,
		[Y4_Budget] [float] NULL,
		[Y5_Budget] [float] NULL,
		[Y6_Budget] [float] NULL,
		[Y7_Budget] [float] NULL,
		[Y8_Budget] [float] NULL,
		[Y9_Budget] [float] NULL,
		[Y10_Budget] [float] NULL,
		[Y11_Budget] [float] NULL,
		[Y12_Budget] [float] NULL,
		[Y1_Forecast] [float] NULL,
		[Y2_Forecast] [float] NULL,
		[Y3_Forecast] [float] NULL,
		[Y4_Forecast] [float] NULL,
		[Y5_Forecast] [float] NULL,
		[Y6_Forecast] [float] NULL,
		[Y7_Forecast] [float] NULL,
		[Y8_Forecast] [float] NULL,
		[Y9_Forecast] [float] NULL,
		[Y10_Forecast] [float] NULL,
		[Y11_Forecast] [float] NULL,
		[Y12_Forecast] [float] NULL,
		[Y1_Planned] [float] NULL,
		[Y2_Planned] [float] NULL,
		[Y3_Planned] [float] NULL,
		[Y4_Planned] [float] NULL,
		[Y5_Planned] [float] NULL,
		[Y6_Planned] [float] NULL,
		[Y7_Planned] [float] NULL,
		[Y8_Planned] [float] NULL,
		[Y9_Planned] [float] NULL,
		[Y10_Planned] [float] NULL,
		[Y11_Planned] [float] NULL,
		[Y12_Planned] [float] NULL,
		[Y1_Actual] [float] NULL,
		[Y2_Actual] [float] NULL,
		[Y3_Actual] [float] NULL,
		[Y4_Actual] [float] NULL,
		[Y5_Actual] [float] NULL,
		[Y6_Actual] [float] NULL,
		[Y7_Actual] [float] NULL,
		[Y8_Actual] [float] NULL,
		[Y9_Actual] [float] NULL,
		[Y10_Actual] [float] NULL,
		[Y11_Actual] [float] NULL,
		[Y12_Actual] [float] NULL
	) ON [PRIMARY]
END
GO

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


-- DROP AND CREATE STORED PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[PreCalBudgetForecastForFinanceGrid]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
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
-- [MV].[PreCalBudgetForecastForFinanceGrid] 12538,2,0,2016,'Y1',5555
CREATE PROCEDURE [MV].[PreCalBudgetForecastForFinanceGrid]
	@BudgetDetailId INT,		-- Id of the Budget_Detail table
	@Year INT,					-- Year 
	@Period VARCHAR(5),			-- Period in case of editing Monthly/Quarterly allocation
	@BudgetValue FLOAT,			-- New value for Budget/Forecast/Custom column
	@ForecastValue FLOAT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @InsertQuery		NVARCHAR(MAX), 
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'
	
	-- Insert Budget/Forecast values in PreCalculatedMarketingBudget table if already exists 
	SET @InsertQuery = ' MERGE INTO [MV].[PreCalculatedMarketingBudget] AS T1
						 USING
						 (
							SELECT ' + CAST(@BudgetDetailId AS VARCHAR(30))+ ' AS BudgetDetailId, 
							' + CAST(@Year AS VARCHAR(30))+ ' AS [Year], 
							'+CAST(@BudgetValue AS VARCHAR(30))+' AS '+@BudgetColumnName+',
							'+CAST(@ForecastValue AS VARCHAR(30))+' AS '+@ForecastColumnName+'
						 ) AS T2
						 ON (T2.BudgetDetailId = T1.BudgetDetailId AND T2.Year = T1.Year)
						 WHEN MATCHED THEN
						 UPDATE SET ' + @BudgetColumnName + ' = T2.'+CAST(@BudgetValue AS VARCHAR(30))+',
									' + @ForecastColumnName + ' = T2.'+CAST(@ForecastValue AS VARCHAR(30))+'
						 WHEN NOT MATCHED THEN  
						 INSERT (BudgetDetailId, [Year], ' + @BudgetColumnName + ',' + @ForecastColumnName + ')
						 VALUES (BudgetDetailId, [Year], ' + @BudgetColumnName + ',' + @ForecastColumnName + ');'
	
	EXEC (@InsertQuery)
END
GO


-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetLineItemIdsByBudgetDetailId]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetLineItemIdsByBudgetDetailId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Viral
-- Create date: 22/11/2016
-- Description:	Returns Commaseparated LineItems group by BudgetDetailId
-- =============================================
CREATE FUNCTION [dbo].[GetLineItemIdsByBudgetDetailId]
(
	-- Add the parameters for the function here
	@BudgetId int 
)
RETURNS 
@tblLineItem TABLE 
(
	-- Add the column definitions for the TABLE variable here
	BudgetDetailId int,
	LineItemCount int,
	PlanLineItemIds varchar(max)
)
AS
BEGIN
	-- SELECT * FROM GetLineItemIdsByBudgetDetailId(2807)
	
		-- For child records, Get comma separated LineItemIds by BudgetDetailId 
		;WITH MyData AS(
		select L.BudgetDetailId,L.PlanLineItemId 
		from LineItem_Budget L
		inner join Budget_Detail B ON B.Id = L.BudgetDetailId
		AND B.BudgetId = @BudgetId
		)
		
		INSERT INTO @tblLineItem(BudgetDetailId,LineItemCount,PlanLineItemIds)
		SELECT BudgetDetailId,Count(*)as LineItemCount, PlanLineItemId = STUFF((
		    SELECT ', ' + CAST(PlanLineItemId AS VARCHAR) FROM MyData
		    WHERE BudgetDetailId = x.BudgetDetailId
		    FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)'), 1, 2, '')
		FROM MyData x
		GROUP BY BudgetDetailId;


	RETURN 
END
GO


-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'GetFinanceBasicData') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetFinanceBasicData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFinanceBasicData]
(
	@BudgetId		INT,
	@ClientId		INT,
	@lstUserIds		NVARCHAR(MAX),
	@UserId			INT,
	@CurrencyRate	FLOAT
)
RETURNS 
@ResultFinanceData TABLE 
(
	Permission		VARCHAR(20),
	BudgetDetailId	INT,
	ParentId		INT,
	Name			VARCHAR(MAX),
	[Owner]			INT,
	TotalBudget		FLOAT,
	TotalForecast	FLOAT,
	TotalPlanned	FLOAT,
	LineItems		INT,
	[User]			INT
)
AS

BEGIN
	
	-- select * from [dbo].GetFinanceBasicData(2807,24,'470,308,104',470)
	
	DECLARE @tblUserIds TABLE (UserId INT)

	INSERT INTO @tblUserIds
	SELECT val FROM [dbo].comma_split(@lstUserIds,',')

	BEGIN
		DECLARE @tblResult TABLE
		(
			Permission		VARCHAR(20),
			BudgetDetailId	INT,
			ParentId		INT,
			Name			VARCHAR(MAX),
			[Owner]			INT,
			TotalBudget		FLOAT,
			TotalForecast	FLOAT
		)

		-- Get Budget details based on permissions
		INSERT INTO @tblResult(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast)
		SELECT
				CASE WHEN BP.PermisssionCode = 0 THEN 'Edit'
					  WHEN BP.PermisssionCode = 1 THEN 'View'
					  ELSE 'None' 
				END AS Permission	
				,BD.Id AS BudgetDetailId
				,BD.ParentId
				,BD.Name
				,U.UserId as '[Owner]'
				,BD.TotalBudget
				,BD.TotalForecast
		FROM [dbo].Budget_Detail BD 
		INNER JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		LEFT JOIN [dbo].[Budget_Permission] BP ON BD.Id = BP.BudgetDetailId AND BP.UserId = @UserId
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId
	END

	DECLARE @tblLineItemIds TABLE
	(
		BudgetDetailId	INT,
		LineItems		VARCHAR(MAX)
	)

	-- Get Parent & Children comma separated LineItemIds by BudgetDetailIds
	;With TblLineItem as(
		
		-- Get Children records
		SELECT R.BudgetDetailId,R.ParentId,L.PlanLineItemIds FROM @tblResult R
		JOIN [dbo].[GetLineItemIdsByBudgetDetailId](@BudgetId) L ON R.BudgetDetailId = L.BudgetDetailId
		WHERE R.BudgetDetailId NOT IN (SELECT ISNULL(ParentId,0) FROM @tblResult)

		UNION ALL 
		
		-- Get parent records
		SELECT P.BudgetDetailId,P.ParentId,c.PlanLineItemIds
		FROM @tblResult p
		JOIN TblLineItem c ON c.ParentId = p.BudgetDetailId
	)

	-- Insert Parent,child distinct lineItem count by comma separated lineItemsIds.
	INSERT INTO @tblLineItemIds(BudgetDetailId,LineItems)
	SELECT	BudgetDetailId
			,SUM(LEN(PlanLineItemIds) - LEN(REPLACE(PlanLineItemIds, ',', '')) +1) AS LineItems	-- Get distinct comma separated lineItemIds count.
	FROM
	(
		SELECT BudgetDetailId
				-- Get Distinct comma separated lineItemIds for each parent-child level record
				,REPLACE(
						CAST(
							CAST('<d>'+ REPLACE(PlanLineItemIds, ', ','</d><d>')+'</d>'  AS XML)
							.query('distinct-values(/d)') AS VARCHAR(MAX)
							), ' ', ', ')AS [PlanLineItemIds]
		
		FROM (
				SELECT BudgetDetailId,PlanLineItemIds = STUFF((
				    SELECT ', ' + CAST(PlanLineItemIds AS VARCHAR(MAX)) FROM TblLineItem
				    WHERE BudgetDetailId = x.BudgetDetailId
				    FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)'), 1, 2, '')
				FROM TblLineItem x
				GROUP BY BudgetDetailId
			) AS planlineIds
	) AS linecnts
	GROUP BY BudgetDetailId

	INSERT INTO @ResultFinanceData(Permission,BudgetDetailId,ParentId,Name,[Owner],TotalBudget,TotalForecast,TotalPlanned,LineItems,[User])
	SELECT DISTINCT R.Permission,R.BudgetDetailId,R.ParentId,R.Name,R.[Owner],R.TotalBudget,R.TotalForecast,LineItem.TotalPlanned,L.LineItems,usrcnt.[User]
	FROM @tblResult R
	LEFT JOIN @tblLineItemIds L on R.BudgetDetailId = L.BudgetDetailId
	LEFT JOIN 
	(
		-- Get User Count
		SELECT  BD.Id as BudgetDetailId,COUNT(BP.UserId) as [User]
		FROM [dbo].[Budget_Detail] BD
		JOIN @tblUserIds U ON BD.CreatedBy = U.UserId 
		JOIN [dbo].[Budget_Permission] BP ON BD.Id = BP.BudgetDetailId 
		WHERE BD.IsDeleted='0' AND BD.BudgetId = @BudgetId
		GROUP BY BD.Id
	) AS usrcnt ON R.BudgetDetailId = usrcnt.BudgetDetailId
	LEFT JOIN 
	(	
		-- Get Planned Cost values
		SELECT BD.Id AS BudgetDetailId, SUM((PCPTL.Cost * Weightage/100)) AS TotalPlanned FROM 
		[dbo].[Budget_Detail] BD 
		INNER JOIN LineItem_Budget LB ON BD.Id = LB.BudgetDetailId
		INNER JOIN Plan_Campaign_Program_Tactic_LineItem PCPTL ON LB.PlanLineItemId = PCPTL.PlanLineItemId
		GROUP BY BD.Id
	) LineItem ON L.BudgetDetailId = LineItem.BudgetDetailId

	RETURN 
END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalBudgetForecastMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalBudgetForecastMarketingBudget]
   ON  [dbo].[Budget_DetailAmount]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Period VARCHAR(30),
			@BudgetValue FLOAT,
			@ForecastValue FLOAT,
			@BudgetDetailId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX),
			@BudgetColumnName	NVARCHAR(50),
			@ForecastColumnName	NVARCHAR(50)

	SET @BudgetColumnName = @Period+'_Budget'
	SET @ForecastColumnName = @Period+'_Forecast'	

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @BudgetValue = Budget,
			   @ForecastValue = Forecast,
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED I
		INNER JOIN Budget_Detail BD ON I.BudgetDetailId = BD.Id

		-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
		EXEC [MV].[PreCalBudgetForecastForFinanceGrid] @BudgetDetailId, @Year, @Period, @BudgetValue, @ForecastValue
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period,
			   @BudgetDetailId = BudgetDetailId,
			   @Year = YEAR(CreatedDate) FROM DELETED D
			   INNER JOIN Budget_Detail BD ON D.BudgetDetailId = BD.Id

		-- Delete/Update record into pre-calculated table while Cost entry is deleted
		SET @DeleteQuery = 'UPDATE P SET ' +@Period + '_'+@BudgetColumnName +' = NULL, 
										 ' +@Period + '_'+@ForecastColumnName +' = NULL  
							FROM [MV].[PreCalculatedMarketingBudget] P
							WHERE P.BudgetDetailId = ' +CAST(@BudgetDetailId AS VARCHAR(30)) +' AND P.Year = ' + CAST(@Year AS VARCHAR(30)) 
		EXEC (@DeleteQuery)
	END

END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalPlannedMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalPlannedMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalPlannedMarketingBudget]
   ON  [dbo].[Plan_Campaign_Program_Tactic_LineItem_Cost]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @UpdatedColumn VARCHAR(30) = 'Planned',
			@Period VARCHAR(30),
			@NewValue FLOAT,
			@OldValue FlOAT = 0,
			@PlanLineItemId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED 

		-- Get old value in case of update
		SELECT @OldValue = Value FROM DELETED

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Year, @Period, @NewValue,@OldValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId,@Year = YEAR(CreatedDate) FROM DELETED 

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET 
								' +@Period + '_'+@UpdatedColumn +' = (P.' +@Period + '_'+ @UpdatedColumn + ' - '+CAST(@OldValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) + '+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100))
								FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) + ' AND P.Year = ' + CAST(@Year AS VARCHAR(30))
			EXEC (@DeleteQuery)
		END
	END

END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgPreCalActualMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgPreCalActualMarketingBudget]
   ON  [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @UpdatedColumn VARCHAR(30) = 'Actual',
			@Period VARCHAR(30),
			@NewValue FLOAT,
			@OldValue FLOAT,
			@PlanLineItemId INT,
			@Year INT,
			@DeleteQuery NVARCHAR(MAX)

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Get values which are inserted/updated
		SELECT @Period = Period,
			   @NewValue = Value,
			   @PlanLineItemId = PlanLineItemId,
			   @Year = YEAR(CreatedDate)
		FROM INSERTED 

		-- Get old value in case of update
		SELECT @OldValue = Value FROM DELETED

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Call SP which update/insert new values to pre-calculated table(i.e.[MV].[PreCalculatedMarketingBudget]) for Marketing Budget
			EXEC [MV].[PreCalPlannedActualForFinanceGrid] @UpdatedColumn, @Year, @Period, @NewValue,@OldValue, @PlanLineItemId
		END
	END
	ELSE 
	BEGIN
		-- Get values which are deleted
		SELECT @Period = Period, @PlanLineItemId = PlanLineItemId,@Year = YEAR(CreatedDate) FROM DELETED 

		IF ((SELECT COUNT(id) FROM LineItem_Budget WHERE PlanLineItemId = @PlanLineItemId AND CAST(REPLACE(@Period,'Y','') AS INT) < 13) > 0)
		BEGIN
			-- Delete/Update record into pre-calculated table while Cost entry is deleted
			SET @DeleteQuery = 'UPDATE P SET 
								' +@Period + '_'+@UpdatedColumn +' = (P.' +@Period + '_'+ @UpdatedColumn + ' - '+CAST(@OldValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100) + '+CAST(@NewValue AS VARCHAR(30))+' * (CAST(Weightage AS FLOAT)/100))
								FROM [MV].[PreCalculatedMarketingBudget] P
								INNER JOIN [dbo].[LineItem_Budget] LB ON P.BudgetDetailId = LB.BudgetDetailId 
								WHERE LB.PlanLineItemId = ' +CAST(@PlanLineItemId AS VARCHAR(30)) + ' AND P.Year = ' + CAST(@Year AS VARCHAR(30))
			EXEC (@DeleteQuery)
		END
	END

END
GO


IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TrgInsertDeletePreCalMarketingBudget]'))
BEGIN
	DROP TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita
-- Create date: 24/11/2016
-- Description:	Trigger which Insert records into pre-calculate table for Marketing Budget
-- =============================================
CREATE TRIGGER [dbo].[TrgInsertDeletePreCalMarketingBudget]
   ON  [dbo].[Budget_Detail]
   AFTER INSERT, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF ((SELECT COUNT(*) FROM INSERTED) > 0)
	BEGIN
		-- Insert new record into pre calculate when new budget is generated
		INSERT INTO [MV].[PreCalculatedMarketingBudget] (BudgetDetailId, [Year])
		SELECT Id, YEAR(CreatedDate) FROM INSERTED
	END
	ELSE
	BEGIN
		-- Delete record from pre calculate table
		DELETE P FROM [MV].[PreCalculatedMarketingBudget] P
		INNER JOIN DELETED D ON P.BudgetDetailId = D.Id
	END

END
GO


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

	--Exec GetFinanceCustomfieldColumnsData 2807,24
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		-- Insert statements for procedure here
		Declare @query varchar(max)=''
		
		Declare @columns varchar(max)
		Declare @drpdCustomType varchar(50)='DropDownList'
		
		SELECT @columns= COALESCE(@columns+', ' ,'')+C.Name+'_'+Cast(C.CustomFieldId as varchar(30))
		FROM Budget_ColumnSet A
		INNER JOIN Budget_Columns B ON A.Id= B.Column_SetId
		INNER JOIN CustomField C ON B.CustomFieldId = C.CustomFieldId and C.EntityType='Budget'
		LEFT JOIN CustomField_Entity CF ON C.CustomFieldId = CF.CustomFieldId and EntityID IN (select Id FROM Budget_Detail where BudgetId = @budgetID and IsDeleted=0)
		LEFT JOIN CustomFieldOption CFO ON CF.Value = CAST(CFO.CustomFieldOptionId AS INT)
		WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND A.ClientId = @clientID AND MapTableName = 'CustomField_Entity'
		
		SET @query = '
		SELECT *
		FROM (
		     SELECT C.Name+''_''+Cast(C.CustomFieldId as varchar(30)) as Name,CF.EntityId as BudgetDetailId, 
			 		CASE 
					WHEN CT.Name='''+@drpdCustomType+''' THEN CFO.Value ELSE CF.Value
				END as Value
			FROM Budget_ColumnSet A
			INNER JOIN Budget_Columns B ON A.Id= B.Column_SetId
			INNER JOIN CustomField C ON B.CustomFieldId = C.CustomFieldId and C.EntityType=''Budget''
			INNER JOIN CustomFieldType CT ON C.CustomFieldTypeId = CT.CustomFieldTypeId
			LEFT JOIN CustomField_Entity CF ON C.CustomFieldId = CF.CustomFieldId and EntityID IN (select Id FROM Budget_Detail where BudgetId = '+Cast(@budgetID as varchar(20)) +' and IsDeleted=0) 
			--INNER JOIN Budget_Detail BD ON CF.EntityId = BD.Id and BD.IsDeleted=0 and BD.BudgetId ='+Cast(@budgetID as varchar(20)) +'
			LEFT JOIN CustomFieldOption CFO ON CF.Value = CAST(CFO.CustomFieldOptionId AS nvarchar(30)) and CT.Name='''+@drpdCustomType+'''
			WHERE A.IsDeleted = 0 AND B.IsDeleted = 0 AND A.ClientId = '+Cast(@clientID as varchar(20))+' AND MapTableName = ''CustomField_Entity'' and IsNUll(CF.EntityId,'''') <> ''''
		) as s
		PIVOT
		(
		    MIN(Value)
		    FOR [Name] IN ('+@columns+')
		)AS pvt'
	EXEC(@query)
END
GO


-- DROP AND CREATE STORED PROCEDURE [MV].[GetFinanceGridData]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[MV].[GetFinanceGridData]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [MV].[GetFinanceGridData]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Arpita Soni
-- Create date: 11/18/2016
-- Description:	SP to fetch finance grid data
-- =============================================
CREATE PROCEDURE [MV].[GetFinanceGridData]
	@BudgetId		INT,
	@ClientId		INT,
	@timeframe		VARCHAR(50),
	@lstUserIds		NVARCHAR(MAX),
	@UserId			INT,
	@CurrencyRate	FLOAT
AS
BEGIN
	
	-- EXEC MV.[GetFinanceGridData] 2807,24,'months','470,308,104',470,0.5
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Declare local variables
	BEGIN

		-- Start: declare timeframe related variables
		Declare @ThisYear varchar(100) ='Yearly' -- This Year
		Declare @ThisQuarters varchar(100) ='quarters' -- This Year (Quarterly)
		Declare @ThisMonthly varchar(100) ='months' -- This Year (Monthly)
		Declare @Quarter1 varchar(100) ='Quarter1' -- Quarter1
		Declare @Quarter2 varchar(100) ='Quarter2' -- Quarter2
		Declare @Quarter3 varchar(100) ='Quarter3' -- Quarter3
		Declare @Quarter4 varchar(100) ='Quarter4' -- Quarter4
		-- End: declare timeframe related variables

	END
	
	IF(@timeframe = @ThisYear)	-- This Year
	BEGIN
		SELECT 
			F.Permission	
			,F.BudgetDetailId
			,F.ParentId
			,F.Name
			,F.TotalBudget * @CurrencyRate as Budget
			,F.TotalForecast * @CurrencyRate as Forecast
			,F.TotalPlanned * @CurrencyRate as Planned
			,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate  as Actual
			,F.[User]
			,F.LineItems
			,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	IF(@timeframe = @ThisQuarters)	-- This Year (Quarterly)
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals for Quarter 1
				,(ISNULL(Y1_Budget,0)+ISNULL(Y2_Budget,0)+ISNULL(Y3_Budget,0)) * @CurrencyRate as Y1_Budget
				,(ISNULL(Y1_Forecast,0)+ISNULL(Y2_Forecast,0)+ISNULL(Y3_Forecast,0)) * @CurrencyRate as Y1_Forecast
				,(ISNULL(Y1_Planned,0)+ISNULL(Y2_Planned,0)+ISNULL(Y3_Planned,0)) * @CurrencyRate as Y1_Planned
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0)) * @CurrencyRate as Y1_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 2
				,(ISNULL(Y4_Budget,0)+ISNULL(Y5_Budget,0)+ISNULL(Y6_Budget,0)) * @CurrencyRate as Y4_Budget
				,(ISNULL(Y4_Forecast,0)+ISNULL(Y5_Forecast,0)+ISNULL(Y6_Forecast,0)) * @CurrencyRate as Y4_Forecast
				,(ISNULL(Y4_Planned,0)+ISNULL(Y5_Planned,0)+ISNULL(Y6_Planned,0)) * @CurrencyRate as Y4_Planned
				,(ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0)) * @CurrencyRate as Y4_Actual
				
				-- Budget, Forecast, Planned, Actuals for Quarter 3
				,(ISNULL(Y7_Budget,0)+ISNULL(Y8_Budget,0)+ISNULL(Y9_Budget,0)) * @CurrencyRate as Y7_Budget
				,(ISNULL(Y7_Forecast,0)+ISNULL(Y8_Forecast,0)+ISNULL(Y9_Forecast,0)) * @CurrencyRate as Y7_Forecast
				,(ISNULL(Y7_Planned,0)+ISNULL(Y8_Planned,0)+ISNULL(Y9_Planned,0)) * @CurrencyRate as Y7_Planned
				,(ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0)) * @CurrencyRate as Y7_Actual

				-- Budget, Forecast, Planned, Actuals for Quarter 4
				,(ISNULL(Y10_Budget,0)+ISNULL(Y11_Budget,0)+ISNULL(Y12_Budget,0)) * @CurrencyRate as Y10_Budget
				,(ISNULL(Y10_Forecast,0)+ISNULL(Y11_Forecast,0)+ISNULL(Y12_Forecast,0)) * @CurrencyRate as Y10_Forecast
				,(ISNULL(Y10_Planned,0)+ISNULL(Y11_Planned,0)+ISNULL(Y12_Planned,0)) * @CurrencyRate as Y10_Planned
				,(ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as Y10_Actual

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'

				,F.[User]
				,F.LineItems
				,F.[Owner]

		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @ThisMonthly)	-- This Year (Monthly)
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals month wise columns 
				,(Y1_Budget * @CurrencyRate  ) AS Y1_Budget,  (Y1_Forecast * @CurrencyRate ) AS Y1_Forecast
				,(Y1_Planned * @CurrencyRate ) AS Y1_Planned, (Y1_Actual * @CurrencyRate   ) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate  ) AS Y2_Budget,  (Y2_Forecast * @CurrencyRate ) AS Y2_Forecast 
				,(Y2_Planned * @CurrencyRate ) AS Y2_Planned, (Y2_Actual * @CurrencyRate   ) AS Y2_Actual
				,(Y3_Budget * @CurrencyRate  ) AS Y3_Budget,  (Y3_Forecast * @CurrencyRate ) AS Y3_Forecast
				,(Y3_Planned * @CurrencyRate ) AS Y3_Planned, (Y3_Actual * @CurrencyRate   ) AS Y3_Actual
				,(Y4_Budget * @CurrencyRate  ) AS Y4_Budget,  (Y4_Forecast * @CurrencyRate ) AS Y4_Forecast 
				,(Y4_Planned * @CurrencyRate ) AS Y4_Planned, (Y4_Actual * @CurrencyRate   ) AS Y4_Actual 
				,(Y5_Budget * @CurrencyRate  ) AS Y5_Budget,  (Y5_Forecast * @CurrencyRate ) AS Y5_Forecast 
				,(Y5_Planned * @CurrencyRate ) AS Y5_Planned, (Y5_Actual * @CurrencyRate   ) AS Y5_Actual 
				,(Y6_Budget * @CurrencyRate  ) AS Y6_Budget,  (Y6_Forecast * @CurrencyRate ) AS Y6_Forecast 
				,(Y6_Planned * @CurrencyRate ) AS Y6_Planned, (Y6_Actual * @CurrencyRate   ) AS Y6_Actual 
				,(Y7_Budget * @CurrencyRate  ) AS Y7_Budget,  (Y7_Forecast * @CurrencyRate ) AS Y7_Forecast 
				,(Y7_Planned * @CurrencyRate ) AS Y7_Planned, (Y7_Actual * @CurrencyRate   ) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate  ) AS Y8_Budget,  (Y8_Forecast * @CurrencyRate ) AS Y8_Forecast 
				,(Y8_Planned * @CurrencyRate ) AS Y8_Planned, (Y8_Actual * @CurrencyRate   ) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate  ) AS Y9_Budget,  (Y9_Forecast * @CurrencyRate ) AS Y9_Forecast 
				,(Y9_Planned * @CurrencyRate ) AS Y9_Planned, (Y9_Actual * @CurrencyRate   ) AS Y9_Actual 
				,(Y10_Budget * @CurrencyRate ) AS Y10_Budget, (Y10_Forecast * @CurrencyRate) AS  Y10_Forecast 
				,(Y10_Planned * @CurrencyRate) AS Y10_Planned,(Y10_Actual * @CurrencyRate  ) AS Y10_Actual 
				,(Y11_Budget * @CurrencyRate ) AS Y11_Budget, (Y11_Forecast * @CurrencyRate) AS  Y11_Forecast 
				,(Y11_Planned * @CurrencyRate) AS Y11_Planned,(Y11_Actual * @CurrencyRate  ) AS Y11_Actual 
				,(Y12_Budget * @CurrencyRate ) AS Y12_Budget, (Y12_Forecast * @CurrencyRate) AS  Y12_Forecast 
				,(Y12_Planned * @CurrencyRate) AS Y12_Planned,(Y12_Actual * @CurrencyRate  ) AS Y12_Actual 

				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'

				,F.[User]
				,F.LineItems
				,F.[Owner]

		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter1)	-- Quarter1
	BEGIN
		SELECT 
				 F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 1
				,(Y1_Budget * @CurrencyRate) AS Y1_Budget,(Y1_Forecast * @CurrencyRate) AS Y1_Forecast,(Y1_Planned * @CurrencyRate) AS Y1_Planned,(Y1_Actual * @CurrencyRate) AS Y1_Actual 
				,(Y2_Budget * @CurrencyRate) AS Y2_Budget,(Y2_Forecast * @CurrencyRate) AS Y2_Forecast,(Y2_Planned * @CurrencyRate) AS Y2_Planned,(Y2_Actual * @CurrencyRate) AS Y2_Actual 
				,(Y3_Budget * @CurrencyRate) AS Y3_Budget,(Y3_Forecast * @CurrencyRate) AS Y3_Forecast,(Y3_Planned * @CurrencyRate) AS Y3_Planned,(Y3_Actual * @CurrencyRate) AS Y3_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter2)	-- Quarter2
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name

				-- Budget, Forecast, Planned, Actuals columns for Quarter 2
				,(Y4_Budget * @CurrencyRate) AS Y4_Budget,(Y4_Forecast * @CurrencyRate) AS Y4_Forecast,(Y4_Planned * @CurrencyRate) AS Y4_Planned,(Y4_Actual * @CurrencyRate) AS Y4_Actual
				,(Y5_Budget * @CurrencyRate) AS Y5_Budget,(Y5_Forecast * @CurrencyRate) AS Y5_Forecast,(Y5_Planned * @CurrencyRate) AS Y5_Planned,(Y5_Actual * @CurrencyRate) AS Y5_Actual
				,(Y6_Budget * @CurrencyRate) AS Y6_Budget,(Y6_Forecast * @CurrencyRate) AS Y6_Forecast,(Y6_Planned * @CurrencyRate) AS Y6_Planned,(Y6_Actual * @CurrencyRate) AS Y6_Actual
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'

				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter3)	-- Quarter3
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 3
				,(Y7_Budget * @CurrencyRate) AS Y7_Budget ,(Y7_Forecast * @CurrencyRate) AS Y7_Forecast ,(Y7_Planned * @CurrencyRate) AS Y7_Planned ,(Y7_Actual * @CurrencyRate) AS Y7_Actual 
				,(Y8_Budget * @CurrencyRate) AS Y8_Budget ,(Y8_Forecast * @CurrencyRate) AS Y8_Forecast ,(Y8_Planned * @CurrencyRate) AS Y8_Planned ,(Y8_Actual * @CurrencyRate) AS Y8_Actual 
				,(Y9_Budget * @CurrencyRate) AS Y9_Budget ,(Y9_Forecast * @CurrencyRate) AS Y9_Forecast ,(Y9_Planned * @CurrencyRate) AS Y9_Planned ,(Y9_Actual * @CurrencyRate) AS Y9_Actual 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END
	ELSE IF(@timeframe = @Quarter4)	-- Quarter 4
	BEGIN
		SELECT 
				F.Permission	
				,F.BudgetDetailId
				,F.ParentId
				,F.Name
				-- Budget, Forecast, Planned, Actuals columns for Quarter 4
				,(Y10_Budget * @CurrencyRate) AS Y10_Budget ,(Y10_Forecast * @CurrencyRate) AS Y10_Budget ,(Y10_Planned * @CurrencyRate) AS Y10_Budget ,(Y10_Actual * @CurrencyRate) AS Y10_Budget 
				,(Y11_Budget * @CurrencyRate) AS Y11_Budget ,(Y11_Forecast * @CurrencyRate) AS Y11_Budget ,(Y11_Planned * @CurrencyRate) AS Y11_Budget ,(Y11_Actual * @CurrencyRate) AS Y11_Budget 
				,(Y12_Budget * @CurrencyRate) AS Y12_Budget ,(Y12_Forecast * @CurrencyRate) AS Y12_Budget ,(Y12_Planned * @CurrencyRate) AS Y12_Budget ,(Y12_Actual * @CurrencyRate) AS Y12_Budget 
				
				,F.TotalBudget * @CurrencyRate as 'Total_Budget'
				,F.TotalForecast * @CurrencyRate as 'Total_Forecast'
				,F.TotalPlanned * @CurrencyRate as 'Total_Planned'
				-- Total Actual
				,(ISNULL(Y1_Actual,0)+ISNULL(Y2_Actual,0)+ISNULL(Y3_Actual,0) +
				  ISNULL(Y4_Actual,0)+ISNULL(Y5_Actual,0)+ISNULL(Y6_Actual,0) +
				  ISNULL(Y7_Actual,0)+ISNULL(Y8_Actual,0)+ISNULL(Y9_Actual,0) +
				  ISNULL(Y10_Actual,0)+ISNULL(Y11_Actual,0)+ISNULL(Y12_Actual,0)) * @CurrencyRate as 'Total_Actual'
				
				,F.[User]
				,F.LineItems
				,F.[Owner]
		FROM [dbo].GetFinanceBasicData(@BudgetId,@ClientId,@lstUserIds,@UserId,@CurrencyRate) F
		INNER JOIN [MV].[PreCalculatedMarketingBudget] P on F.BudgetDetailId = P.BudgetDetailId
	END

	-- Get custom columns data
	 EXEC [dbo].[GetFinanceCustomfieldColumnsData] @BudgetId, @ClientId


END
GO

-- End - Added by Arpita Soni for Ticket #2790 on 11/28/2016

Go



-- Start - Added by Viral for Ticket #2763 on 11/29/2016

/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridFilters]
GO
/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridFilters] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetGridFilters] 
	@userId int
	,@ClientId int
	,@IsDefaultCustomRestrictionsViewable bit 
AS --Todo: New user login then need to some more 
BEGIN
	
	--EXEC GetGridFilters 549,4,0

	SET NOCOUNT ON;

		Declare @PlanId NVARCHAR(MAX) = ''
		Declare @OwnerIds NVARCHAR(MAX) = ''
		Declare @TacticTypeIds varchar(max)=''
		Declare @StatusIds varchar(max)=''
		Declare @customFields varchar(max)=''

		Declare @viewname  varchar(max)

		Declare @tblUserSavedViews Table(
		Id INT
		,ViewName NVARCHAR(max)
		,FilterName NVARCHAR(1000)
		,FilterValues NVARCHAR(max)
		,LastModifiedDate DATETIME
		,IsDefaultPreset BIT
		,Userid INT
		)

		Declare @keyPlan varchar(30)='Plan'
		Declare @keyOwner varchar(30)='Owner'
		Declare @keyStatus varchar(30)='Status'
		Declare @keyTacticType varchar(30)='TacticType'
		Declare @keyAll varchar(10)='All'
		Declare @keyCustomField varchar(100)='CustomField'
		

		select TOP 1 @viewname =  ViewName from Plan_UserSavedViews where Userid=@userId AND IsDefaultPreset = 1
		SET @viewname = ISNULL(@viewname,'')

		-- Insert user filters to local variables.
		INSERT INTO @tblUserSavedViews(Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid)
		select Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid from Plan_UserSavedViews where Userid=@userId AND ISNULL(ViewName,'') = IsNull(@viewname,'')

		IF EXISTS(select Id from @tblUserSavedViews)
		BEGIN
			
		

		-- Get PlanIds that user has selected under filter.
		SELECT TOP 1 @PlanId = FilterValues from @tblUserSavedViews where FilterName=@keyPlan

		-- Get OwnerIds that user has selected under filter.
		SELECT TOP 1 @OwnerIds = FilterValues from @tblUserSavedViews where FilterName=@keyOwner

		-- Get TacticTypeIds that user has selected under filter.
		SELECT TOP 1 @TacticTypeIds = FilterValues from @tblUserSavedViews where FilterName=@keyTacticType

		-- Get Status that user has selected under filter.
		SELECT TOP 1 @StatusIds = FilterValues from @tblUserSavedViews where FilterName=@keyStatus

		-- Get Status that user has selected under filter.
		SET @customFields = ''


		BEGIN

			Declare @customFieldId varchar(100)
			Declare @FilterValues varchar(max)
			Declare @cntFiltr INT
			Declare @cntPermsn INT
			Declare @CustomFilters varchar(max) 


			  Declare @CustomFieldIDs varchar(max)=''

			 SELECT  @CustomFieldIDs = COALESCE(@CustomFieldIDs + ',', '') + CONVERT(varchar(100), CF.[CustomFieldId])  + '_null'  FROM [Hive9ProdGP].[dbo].[CustomField] CF
			 inner join CustomFieldType CT on CT.CustomFieldTypeId = CF.CustomFieldTypeId and CT.name = 'DropDownList'
			  where  CF.EntityType = 'Tactic'  and CF.isdeleted = 0 and CF.IsDisplayForFilter = 1 AND CF.ClientId = @ClientId 
             AND ( CF.[CustomFieldId] NOT IN
			  (select  CAST((CASE WHEN REPLACE(FilterName,'CF_','') NOT LIKE '%[^0-9]%' THEN REPLACE(FilterName,'CF_','') END) AS INT)  from [Hive9ProdGP].DBO.Plan_UserSavedViews where Userid=@userId and FilterName like'CF_%')
			  )

			   if( LEFT(@CustomFieldIDs,1) = ',')
			     SET  @CustomFieldIDs = substring(@CustomFieldIDs,2,LEN(@CustomFieldIDs))

			select @cntPermsn = count(*) from CustomRestriction as CR Where UserId = @userId

			


			
			DECLARE db_cursor CURSOR FOR  
			select REPLACE(FilterName,'CF_',''),FilterValues from @tblUserSavedViews where Userid=@userId and FilterName like'CF_%'
			
			OPEN db_cursor   
			FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues   
			
			WHILE @@FETCH_STATUS = 0   
			BEGIN   
				
				IF(IsNull(@cntPermsn,0) > 0)
				BEGIN
				   select @cntFiltr = count(*) from CustomRestriction as CR
					JOIN CustomField as C on CR.CustomFieldId = C.CustomFieldId and C.ClientId=@ClientId and IsDeleted='0' and C.IsRequired='1' and ( (CR.Permission = 1)  OR (CR.Permission = 2) )
					where UserId = @userId and C.CustomFieldId=Cast(@customFieldId as INT) and cr.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
			
				

				END
				ELSE IF(@IsDefaultCustomRestrictionsViewable = '1' )
				BEGIN
						SELECT @cntFiltr = count(*) from CustomField as C
						Join CustomFieldType as CT on C.CustomFieldTypeId = C.CustomFieldTypeId and CT.Name ='DropDownList'
						JOIN CustomFieldOption as CO on C.CustomFieldId = CO.CustomFieldId and CO.IsDeleted='0' and CO.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
						where ClientId=@ClientId and C.IsDeleted='0' and  EntityType='Tactic' and IsDisplayForFilter='1' and C.CustomFieldId= Cast(@customFieldId as INT)
				END

				IF (IsNull(@cntFiltr,0) > 0)
				BEGIN
					SET @customFields = @customFields + ',' + @customFieldId + '_' + REPLACE(@FilterValues,',',','+@customFieldId + '_' ) 
				END
				
			       FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues      
			END   
			
			CLOSE db_cursor   
			DEALLOCATE db_cursor
			if(LEN(@customFields) > 2)
			SET @customFields = SUBSTRING(@customFields,2, LEN(@customFields)-1) 

		END

		END
		ELSE
		BEGIN
			
			SELECT Top 1 @PlanId = p.PlanId from Model as M
			JOIN [Plan] as P on M.ModelId = P.ModelId and M.ClientId=@ClientId and P.IsDeleted='0' and M.IsDeleted='0' and P.[Year]<=Datepart(yyyy,GETDATE())
			Order by P.Year desc, P.Title
			
			SET @OwnerIds=@keyAll
			SET @TacticTypeIds=@keyAll
			SET @StatusIds='Created,Submitted,Approved,In-Progress,Complete,Declined'
			SET @customFields = ''
		END
		
		IF(@CustomFieldIDs != '')
		BEGIN
		 IF(@customFields = '')
				    BEGIN
				       SET @customFields =  @CustomFieldIDs
				   END
				   ELSE
				   BEGIN
				      SET @customFields = @customFields + ',' + @CustomFieldIDs
				   END
		END
	             	
				
			

		select @PlanId PlanIds,@OwnerIds OwnerIds,@StatusIds StatusIds,@TacticTypeIds TacticTypeIds,@customFields CustomFieldIds

    
END



GO

-- End - Added by Viral for Ticket #2763 on 11/29/2016



-- ===========================Please put your script above this script=============================
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'November.2016'
set @version = 'November.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO

