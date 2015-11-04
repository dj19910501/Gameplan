GO 
DECLARE @BDSAUTHDBName nvarchar(50) = 'BDSAuthQA'
DECLARE @MRPDBName nvarchar(50) = 'MRPQA'

DECLARE @SQLString nvarchar(max)=''

SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.Budget ([ClientId], [Name], [CreatedDate], [CreatedBy],[IsOther],[IsDeleted])
SELECT C.ClientId,''Other'',GETDATE(),C.CreatedBy,1,0 FROM ' + @BDSAUTHDBName + '.dbo.Client C
WHERE C.ClientId NOT IN (SELECT ClientId FROM ' + @MRPDBName + '.dbo.Budget WHERE IsOther = 1) AND C.CreatedBy IS NOT NULL'

Exec(@SQLString)
SET @SQLString = ''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.Budget_Detail ([BudgetId],[Name],[CreatedDate],[CreatedBy])
SELECT MB.Id,''Other'',GETDATE(),MB.CreatedBy FROM ' + @MRPDBName + '.dbo.Budget MB WHERE IsOther = 1 AND id NOT IN 
(SELECT BudgetId FROM ' + @MRPDBName + '.dbo.Budget_Detail) AND MB.CreatedBy IS NOT NULL'

Exec(@SQLString)

-- Query for add Default columns in exist client
Declare @SQLCreateTemp nvarchar(max)=''
Declare @SQLDROPTemp nvarchar(max)=''
Declare @SQLColumnSet nvarchar(max)=''
Declare @SQLColumns nvarchar(max)=''

-- Variables for ColumnName
Declare @BudgetCol nvarchar(max)='Budget'
Declare @ForecastCol nvarchar(max)='Forecast'
Declare @PlannedCol nvarchar(max)='Planned'
Declare @ActualCol nvarchar(max)='Actual'

-- Variables for ColumnValidation
Declare @BudgetVal nvarchar(max)='ValidNumeric'
Declare @ForecastVal nvarchar(max)='ValidNumeric'
Declare @PlannedVal nvarchar(max)='ValidNumeric'
Declare @ActualVal nvarchar(max)='ValidNumeric'

-- Variables for MapTableName
Declare @BudgetMapTable nvarchar(max)='Budget_DetailAmount'
Declare @ForecastMapTable nvarchar(max)='Budget_DetailAmount'
Declare @PlannedMapTable nvarchar(max)='Plan_Campaign_Program_Tactic_LineItem_Cost'
Declare @ActualMapTable nvarchar(max)='Plan_Campaign_Program_Tactic_LineItem_Actual'

--Variables for Default ColumnSet Name
Declare @ColumnSetName nvarchar(max)='Finance'

IF OBJECT_ID ('tempdb..#client') IS NOT NULL
BEGIN
	DROP TABLE #client
END

CREATE TABLE #client
(
    [ClientId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier]  NULL,
)

SET @SQLCreateTemp = @SQLCreateTemp +N'INSERT INTO #client ([ClientId],[CreatedBy]) SELECT C.ClientId,C.CreatedBy
FROM ' + @BDSAUTHDBName + '.dbo.Client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM ' + @MRPDBName + '.dbo.CustomField Cust
Where Cust.IsDeleted=0 AND Cust.EntityType=''Budget'') AND  C.CreatedBy IS NOT NULL
'
EXEC(@SQLCreateTemp)

-- Insert Default Column Set 
SET @SQLColumnSet = @SQLColumnSet+N'INSERT INTO '+ @MRPDBName +'.dbo.Budget_ColumnSet([Name],[ClientId],[CreatedBy],[CreatedDate],[IsDeleted])
SELECT '''+@ColumnSetName+''' as Name,C.ClientId,C.CreatedBy,GETDATE(),0 FROM #client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM '+ @MRPDBName +'.dbo.Budget_ColumnSet Where IsDeleted=0) AND  C.CreatedBy IS NOT NULL'
EXEC(@SQLColumnSet)
-- End Default Coumn Set

Declare @i int=1

WHILE(@i<=4)
BEGIN


if(@i=1)
BEGIN
-- Insert Column Name to Custom Field table
SET @SQLString=''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.CustomField([Name],[CustomFieldTypeId],[IsRequired],[EntityType],
[ClientId],[CreatedDate],[CreatedBy],[IsDisplayForFilter],[AbbreviationForMulti],[IsDefault],[IsGet])
SELECT '''+@BudgetCol+''' as Name,(SELECT CustomFieldTypeId From '+@MRPDBName+'.dbo.CustomFieldType where Name=''TextBox''),0,''Budget'',C.ClientId,GETDATE(),C.CreatedBy,1,''MULTI'',0,0 FROM #client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM ' + @MRPDBName + '.dbo.CustomField Cust
Where Cust.IsDeleted=0 AND EntityType=''Budget'' AND Name='''+@BudgetCol+''') AND  C.CreatedBy IS NOT NULL'
EXEC(@SQLString)

-- End 

-- Insert Columns on Budget Columns Table 
SET @SQLColumns=''
SET @SQLColumns=@SQLColumns+N'INSERT INTO '+ @MRPDBName +'.dbo.Budget_Columns([Column_SetId],[CustomFieldId],[CreatedDate],[CreatedBy]
,[IsTimeFrame],[IsDeleted],[MapTableName],[ValueOnEditable],[ValidationType])
Select Distinct ColSet.Id,Custom.CustomFieldId,GETDATE(),C.CreatedBy,1,0,'''+@BudgetMapTable+''' as MapTableName,1,'''+@BudgetVal+''' as ValidationType
 From '+@MRPDBName+'.dbo.Budget_ColumnSet ColSet 
Inner Join '+@MRPDBName+'.dbo.CustomField Custom
On ColSet.ClientId=Custom.ClientId
RIGHT JOIN #client C
ON Custom.ClientId=C.ClientId
Where Custom.EntityType=''Budget'' AND Custom.Name='''+@BudgetCol+''' 
AND Custom.IsDeleted=0
AND ColSet.IsDeleted=0
AND ColSet.ClientId IN( C.ClientId )
AND Custom.CustomFieldId NOT IN(Select CustomFieldId From '+@MRPDBName+'.dbo.Budget_Columns)'
EXEC(@SQLColumns)

END

-- END
if(@i=2)
BEGIN
-- Insert Column Name to Custom Field table
SET @SQLString=''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.CustomField([Name],[CustomFieldTypeId],[IsRequired],[EntityType],
[ClientId],[CreatedDate],[CreatedBy],[IsDisplayForFilter],[AbbreviationForMulti],[IsDefault],[IsGet])
SELECT '''+@ForecastCol+''' as Name,(SELECT CustomFieldTypeId From '+@MRPDBName+'.dbo.CustomFieldType where Name=''TextBox''),0,''Budget'',C.ClientId,GETDATE(),C.CreatedBy,1,''MULTI'',0,0 FROM #client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM ' + @MRPDBName + '.dbo.CustomField Cust
Where Cust.IsDeleted=0 AND EntityType=''Budget'' AND Name='''+@ForecastCol+''') AND  C.CreatedBy IS NOT NULL'
EXEC(@SQLString)
-- END

-- Insert Columns on Budget Columns Table 
SET @SQLColumns=''
SET @SQLColumns=@SQLColumns+N'INSERT INTO '+ @MRPDBName +'.dbo.Budget_Columns([Column_SetId],[CustomFieldId],[CreatedDate],[CreatedBy]
,[IsTimeFrame],[IsDeleted],[MapTableName],[ValueOnEditable],[ValidationType])
Select Distinct ColSet.Id,Custom.CustomFieldId,GETDATE(),C.CreatedBy,1,0,'''+@ForecastMapTable+''' as MapTableName,2,'''+@ForecastVal+''' as ValidationType
 From '+@MRPDBName+'.dbo.Budget_ColumnSet ColSet 
Inner Join '+@MRPDBName+'.dbo.CustomField Custom
On ColSet.ClientId=Custom.ClientId
RIGHT JOIN #client C
ON Custom.ClientId=C.ClientId
Where Custom.EntityType=''Budget'' AND Custom.Name='''+@ForecastCol+'''
AND Custom.IsDeleted=0
AND ColSet.IsDeleted=0
AND ColSet.ClientId IN( C.ClientId )
AND Custom.CustomFieldId NOT IN(Select CustomFieldId From '+@MRPDBName+'.dbo.Budget_Columns)'
EXEC(@SQLColumns)
--END
END

if(@i=3)
BEGIN

-- Insert Column Name to Custom Field table
SET @SQLString=''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.CustomField([Name],[CustomFieldTypeId],[IsRequired],[EntityType],
[ClientId],[CreatedDate],[CreatedBy],[IsDisplayForFilter],[AbbreviationForMulti],[IsDefault],[IsGet])
SELECT '''+@PlannedCol+''' as Name,(SELECT CustomFieldTypeId From '+@MRPDBName+'.dbo.CustomFieldType where Name=''TextBox''),0,''Budget'',C.ClientId,GETDATE(),C.CreatedBy,1,''MULTI'',0,0 FROM #client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM ' + @MRPDBName + '.dbo.CustomField Cust
Where Cust.IsDeleted=0 AND EntityType=''Budget'' AND Name='''+@PlannedCol+''') AND  C.CreatedBy IS NOT NULL'
EXEC(@SQLString)
-- END

-- Insert Columns on Budget Columns Table 
SET @SQLColumns=''
SET @SQLColumns=@SQLColumns+N'INSERT INTO '+ @MRPDBName +'.dbo.Budget_Columns([Column_SetId],[CustomFieldId],[CreatedDate],[CreatedBy]
,[IsTimeFrame],[IsDeleted],[MapTableName],[ValueOnEditable],[ValidationType])
Select Distinct ColSet.Id,Custom.CustomFieldId,GETDATE(),C.CreatedBy,1,0,'''+@PlannedMapTable+''' as MapTableName,4,'''+@PlannedVal+''' as ValidationType 
 From '+@MRPDBName+'.dbo.Budget_ColumnSet ColSet 
Inner Join '+@MRPDBName+'.dbo.CustomField Custom
On ColSet.ClientId=Custom.ClientId
RIGHT JOIN #client C
ON Custom.ClientId=C.ClientId
Where Custom.EntityType=''Budget'' AND Custom.Name='''+@PlannedCol+''' 
AND Custom.IsDeleted=0
AND ColSet.IsDeleted=0
AND ColSet.ClientId IN( C.ClientId )
AND Custom.CustomFieldId NOT IN(Select CustomFieldId From '+@MRPDBName+'.dbo.Budget_Columns)'
EXEC(@SQLColumns)
END
-- END

if(@i=4)

-- Insert Column Name to Custom Field table
BEGIN
SET @SQLString=''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.CustomField([Name],[CustomFieldTypeId],[IsRequired],[EntityType],
[ClientId],[CreatedDate],[CreatedBy],[IsDisplayForFilter],[AbbreviationForMulti],[IsDefault],[IsGet])
SELECT '''+@ActualCol+''' as Name,(SELECT CustomFieldTypeId From '+@MRPDBName+'.dbo.CustomFieldType where Name=''TextBox''),0,''Budget'',C.ClientId,GETDATE(),C.CreatedBy,1,''MULTI'',0,0 FROM #client C
WHERE C.ClientId NOT IN(SELECT ClientId FROM ' + @MRPDBName + '.dbo.CustomField Cust
Where Cust.IsDeleted=0 AND EntityType=''Budget''  AND Name='''+@ActualCol+''') AND  C.CreatedBy IS NOT NULL'
EXEC(@SQLString)
-- END

-- Insert Columns on Budget Columns Table 
SET @SQLColumns=''
SET @SQLColumns=@SQLColumns+N'INSERT INTO '+ @MRPDBName +'.dbo.Budget_Columns([Column_SetId],[CustomFieldId],[CreatedDate],[CreatedBy]
,[IsTimeFrame],[IsDeleted],[MapTableName],[ValueOnEditable],[ValidationType])
Select Distinct ColSet.Id,Custom.CustomFieldId,GETDATE(),C.CreatedBy,1,0,'''+@ActualMapTable+''' as MapTableName,4,'''+@ActualVal+''' as ValidationType
 From '+@MRPDBName+'.dbo.Budget_ColumnSet ColSet 
Inner Join '+@MRPDBName+'.dbo.CustomField Custom
On ColSet.ClientId=Custom.ClientId
RIGHT JOIN #client C
ON Custom.ClientId=C.ClientId
Where Custom.EntityType=''Budget'' AND Custom.Name='''+@ActualCol+'''
AND Custom.IsDeleted=0
AND ColSet.IsDeleted=0
AND ColSet.ClientId IN( C.ClientId )
AND Custom.CustomFieldId NOT IN(Select CustomFieldId From '+@MRPDBName+'.dbo.Budget_Columns)'
EXEC(@SQLColumns)
END
-- END

SET @i=@i+1
END;
-- End Columns on Budget Columns Table 

GO