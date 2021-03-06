-- Add By Nishant Sheth
-- Desc :: Create sample data logic for Cost/MQL and COST/CW
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'InsertDataForTacticTypeCosts') AND TYPE IN ( N'P', N'PC' ))
    DROP PROCEDURE [dbo].[InsertDataForTacticTypeCosts]
GO

GO
/****** Object:  StoredProcedure [dbo].[InsertDataForTacticTypeCosts]    Script Date: 06/21/2016 18:47:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertDataForTacticTypeCosts]
AS
BEGIN

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED  

IF OBJECT_ID('tempdb..#TempDepencey') IS NOT NULL
   DROP TABLE #TempDepencey

IF OBJECT_ID('tempdb..#TempDepenceytables') IS NOT NULL
   DROP TABLE #TempDepenceytables

CREATE TABLE #TempDepencey (oType INT,	oObjName NVARCHAR(MAX),	oOwner NVARCHAR(MAX),	oSequence  INT)
INSERT INTO #TempDepencey EXEC('EXEC sp_MSdependencies N''[TacticTypeCost]'', null, 1315327')

SELECT  ROW_NUMBER() OVER (ORDER BY (oObjName)) AS ROWNUM, * into #TempDepenceytables FROM #TempDepencey WHERE oType=8 AND oObjName IS NOT NULL

DECLARE @DependcyCount int;
DECLARE @DependcyInitCount int=1;
Declare @DependcyTableName nvarchar(max);
Declare @DependcyTableQuery nvarchar(max)='';

SELECT @DependcyCount = Count(*) FROm #TempDepenceytables

While @DependcyInitCount<=@DependcyCount
BEGIN	

	SELECT @DependcyTableName = oObjName FROM #TempDepenceytables WHERE ROWNUM=@DependcyInitCount;

		SET @DependcyTableQuery += ' DROP TABLE '+@DependcyTableName+' ';

	SET @DependcyInitCount = @DependcyInitCount+1;
END

EXEC(@DependcyTableQuery);

IF OBJECT_ID(N'dbo.[TacticTypeCost]', N'U') IS NOT NULL
BEGIN
	DROP TABLE TacticTypeCost;
END


   
IF OBJECT_ID('tempdb..#tmpCostData') IS NOT NULL
   DROP TABLE #tmpCostData

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE  #tblCustomData

IF OBJECT_ID('tempdb..#tblCustomFieldColumn') IS NOT NULL
   DROP TABLE  #tblCustomFieldColumn

CREATE TABLE  #tblCustomData(
TacticTypeId INT,
   TacticType NVARCHAR(MAX),
   MQL float NULL,
   Cost float NULL,
   CW float NULL,
   [Cost/MQL] float NULL,
   [Cost/CW] float NULL,
   PlanId Int,
   PlanName NVARCHAR(MAX),
   StartDate date,
   ClientId uniqueidentifier,
   CreatedBy uniqueidentifier
   )

Declare @CustomFieldType NVARCHAR(100)='DropDownList'
Declare @EntityType NVARCHAR(100)='Tactic'
DECLARE @ColumnName NVARCHAR(MAX);
DECLARE @DynamicPivot nvarchar(MAX);

SELECT TacticTypeId
,MAX(Title) AS TacticType
,DateCol AS StartDate
,CreatedBy
,ClientId
,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,SUM(MQL) AS MQL
,SUM(COST) AS Cost
,SUM(CW) AS CW
, CASE WHEN SUM(COST) > 0 AND SUM(MQL) > 0 THEN SUM(COST)/SUM(MQL) END AS [Cost/MQL]
, CASE WHEN SUM(COST) > 0 AND SUM(CW) > 0 THEN SUM(COST)/SUM(CW) END AS [Cost/CW]
into #tmpCostData
FROM(
SELECT TacticType.TacticTypeId
,TacticType.Title
,MQL.DateCol
,MQL.CreatedBy
,MQL.ClientId
,MQL.PlanId
,MQL.PlanName
,MQL.CustomFieldName
,MQL.CustomFieldId
,MQL.CustomFieldOptionId
,MQL.MQL
,NULL AS COST
,NULL AS CW
 FROM 
TacticType
INNER JOIN ( SELECT MQLData.TacticTypeId,SUM(MQL) AS MQL,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar) AS CustomFieldName
,CustomField.CustomFieldId
,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(Actual.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN Actual.Actualvalue ELSE NULL END) AS MQL 
,CAST((DATEADD(month,CAST(REPLACE(Actual.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_Actual Actual ON Tactic.PlanTacticId = Actual.PlanTacticId  AND Actual.StageTitle='MQL'
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('','')='' THEN Model.ClientId  ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS MQLData WHERE MQLData.MQL IS NOT NULL GROUP BY TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId) AS MQL ON TacticType.TacticTypeId=MQL.TacticTypeId

WHERE TacticType.IsDeleted=0

UNION ALL

SELECT TacticType.TacticTypeId
,TacticType.Title
,CW.DateCol
,CW.CreatedBy
,CW.ClientId
,CW.PlanId
,CW.PlanName
,CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,NULL AS MQL
,NULL AS COST
,CW.CW
 FROM 
TacticType
INNER JOIN ( SELECT CWData.TacticTypeId,SUM(CW) AS CW,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar)AS CustomFieldName
,CustomField.CustomFieldId,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(Actual.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN Actual.Actualvalue ELSE NULL END) AS CW 
,CAST((DATEADD(month,CAST(REPLACE(Actual.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_Actual Actual ON Tactic.PlanTacticId = Actual.PlanTacticId  AND Actual.StageTitle='CW'
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('464eb808-ad1f-4481-9365-6aada15023bd','')='' THEN Model.ClientId ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS CWData WHERE CWData.CW IS NOT NULL GROUP BY TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId) AS CW ON TacticType.TacticTypeId=CW.TacticTypeId

WHERE TacticType.IsDeleted=0

UNION ALL

SELECT TacticType.TacticTypeId
,TacticType.Title
,Cost.DateCol
,Cost.CreatedBy
,Cost.ClientId
,Cost.PlanId
,Cost.PlanName
,CustomFieldName
,CustomFieldId
,CustomFieldOptionId
,NULl AS MQL
,Cost.Cost
,NULL AS CW
 FROM 
TacticType
INNER JOIN (SELECT CostData.TacticTypeId,SUM(Cost) AS Cost,DateCol,CreatedBy,ClientId,PlanId
,MAX(PlanName) AS PlanName
,MAX(CustomFieldName) AS CustomFieldName
,CustomFieldId
,CustomFieldOptionId FROM (SELECT Tactic.TacticTypeId,Tactic.CreatedBy,Model.ClientId,[Plan].PlanId
,[Plan].Title AS PlanName
,CustomField.Name+'_'+CAST(CustomField.CustomFieldId AS nvarchar) AS CustomFieldName
,CustomField.CustomFieldId
,CustomFieldOptionId
,(CASE  WHEN CAST(REPLACE(LineItemCost.Period,'Y','') AS INT) <= CAST(DATEPART(m, getdate()) AS INT)
AND CAST([Plan].[Year] AS INT)<=CAST(DATEPART(YYYY, getdate()) AS INT) THEN LineItemCost.Value ELSE NULL END) AS Cost
,CAST((DATEADD(month,CAST(REPLACE(LineItemCost.Period,'Y','') AS INT)-1,DATEADD(year,CAST([Plan].Year AS int)-1900,0))) AS DATE) AS DateCol
FROM Plan_Campaign_Program_Tactic Tactic 
INNER JOIN TacticType ON Tactic.TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0
INNER JOIN Model ON Model.ModelId=TacticType.ModelId AND Model.IsDeleted=0
INNER JOIN [Plan] ON Model.ModelId=[Plan].ModelId AND  [Plan].IsDeleted=0
--AND [Plan].PlanId = 19053
INNER JOIN Plan_Campaign_Program_Tactic_LineItem LineItem ON Tactic.PlanTacticId = LineItem.PlanTacticId  AND LineItem.IsDeleted=0
INNER JOIN Plan_Campaign_Program_Tactic_LineItem_Actual LineItemCost ON LineItem.PlanLineItemId = LineItemCost.PlanLineItemId
INNER JOIN CustomField WITH (NOLOCK) ON 
CustomField.CustomFieldTypeId = (SELECT CustomFieldTypeId FROM CustomFieldType WITH (NOLOCK) WHERE Name=@CustomFieldType)
--AND CustomField.ClientId=CASE WHEN ISNULL('464eb808-ad1f-4481-9365-6aada15023bd','')='' THEN Model.ClientId ELSE '464eb808-ad1f-4481-9365-6aada15023bd' END
AND CustomField.ClientId=Model.ClientId  
AND CustomField.IsDeleted=0 
AND CustomField.IsDisplayForFilter=1 
AND CustomField.EntityType=@EntityType
LEFT JOIN CustomField_Entity WITH (NOLOCK) ON Tactic.PlanTacticId=CustomField_Entity.EntityId
AND CustomField.CustomFieldId=CustomField_Entity.CustomFieldId
LEFT JOIN CustomFieldOption ON 
CustomFieldOption.CustomFieldOptionId = CASE WHEN 
ISNUMERIC(CustomField_Entity.Value) = 1 THEN  CAST(CustomField_Entity.Value AS INT) END
AND CustomFieldOption.IsDeleted=0
WHERE Tactic.IsDeleted=0 AND Tactic.[Status] IN ('Approved','In-Progress','Complete')
) AS CostData WHERE CostData.Cost IS NOT NULL GROUP BY DateCol,TacticTypeId,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId
) AS Cost ON TacticType.TacticTypeId=Cost.TacticTypeId
WHERE TacticType.IsDeleted=0
) AS CostByMQL
GROUP BY  TacticTypeId,DateCol,ClientId,CreatedBy,PlanId,CustomFieldId,CustomFieldOptionId

SELECT @ColumnName= COALESCE(@ColumnName + ',', '') + +'['+ CAST(CustomFieldName AS NVARCHAR) +']'
FROM 
(SELECT DISTINCT CustomFieldName FROM #tmpCostData) 
as DistiCol


SELECT ROW_NUMBER() OVER (ORDER BY (CustomFieldName)) AS ROWNUM,CustomFieldName  INTO #tblCustomFieldColumn  FROM (
SELECT DISTINCT CustomFieldName
FROM #tmpCostData
WHERE CustomFieldName IS NOT NULL OR CustomFieldName='') AS DummyColumn

Declare @AlterTable NVARCHAR(MAX)='';

DECLARE @Count INT = 1
DECLARE @EntityRowCount INT;

SELECT @EntityRowCount = COUNT(*) FROM #tblCustomFieldColumn

DECLARE @CustomColumnName NVARCHAR(MAX)='';

While @Count<=@EntityRowCount
BEGIN	
	SELECT @CustomColumnName = CustomFieldName FROM #tblCustomFieldColumn WHERE ROWNUM=@Count
	
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD ['+@CustomColumnName+'] NVARCHAR(MAX) '

	SET @Count = @Count+1;
END

--SELECT @AlterTable
EXEC(@AlterTable)

SELECT @DynamicPivot = '
INSERT INTO #tblCustomData
SELECT 
TacticTypeId,TacticType,MQL,Cost,CW,[Cost/MQL],[Cost/CW],PlanId,PlanName,StartDate,ClientId,CreatedBy,'+@ColumnName+'
FROM #tmpCostData
PIVOT(
MAX(CustomFieldOptionId)
FOR CustomFieldName IN('+@ColumnName+')
)As PVT
'

EXEC SP_EXECUTESQL @DynamicPivot

DECLARE @DynamicData NVARCHAR(MAX);

SET @DynamicData = '
SELECT ROW_NUMBER() Over (Order by TacticTypeId) As Id,* into TacticTypeCost FROM 
(
SELECT TacticTypeId,MAX(TacticType)TacticType,MAX(MQL) MQL,MAX(Cost) Cost,MAX(CW) CW
,MAX([Cost/MQL])[Cost/MQL],MAX([Cost/CW])[Cost/CW],PlanId,MAX(PlanName)PlanName,StartDate
,MAX(ClientId)ClientId,CreatedBy';

SET @Count=1;
While @Count<=@EntityRowCount
BEGIN	
	SELECT @CustomColumnName = CustomFieldName FROM #tblCustomFieldColumn WHERE ROWNUM=@Count
	
SET @DynamicData+=', STUFF((SELECT DISTINCT '','' + MAX(A.['+@CustomColumnName+']) FROM #tblCustomData A
Where A.[TacticTypeId]=B.[TacticTypeId] Group BY PlanId FOR XML PATH('''')),1,1,'''') As ['+@CustomColumnName+'] '

	SET @Count = @Count+1;
END

SET @DynamicData+=' FROM #tblCustomData B 
GROUP BY TacticTypeId,StartDate,PlanId,CreatedBy) AS CostData'
EXEC SP_EXECUTESQL @DynamicData

END