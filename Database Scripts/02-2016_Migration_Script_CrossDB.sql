Go
-- Created By : Rahul Shah
-- Created Date : 09/02/2016
-- Description : insert permission of SFDC "Pull MQL" for Client 
-- ======================================================================================

--change authntication and MRP DB Name.
DECLARE @BDSAUTHDBName nvarchar(50) = 'BDSAuthDev'
DECLARE @MRPDBName nvarchar(50) = 'MRPDev'

--Please do Not make any change below variable value
DECLARE @Code nvarchar(max) = 'Salesforce'
DECLARE @SQLCreateTemp nvarchar(max)=''
DECLARE @SQLCreateTempIntegration nvarchar(max)=''

DECLARE @ClientRow int = 0
IF OBJECT_ID('tempdb..#TempIntegrationType') IS NOT NULL
    DROP TABLE #TempIntegrationType

IF OBJECT_ID ('tempdb..#client') IS NOT NULL
BEGIN
	DROP TABLE #client
END

CREATE TABLE #client
(
    [ClientId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier]  NULL,
	[RowId] [int]  NULL,
)

SET @SQLCreateTemp = @SQLCreateTemp +N'INSERT INTO #client ([ClientId],[CreatedBy],[RowId]) SELECT C.ClientId,C.CreatedBy,ROW_NUMBER() OVER(ORDER BY C.ClientId) AS CROWID
FROM ' + @BDSAUTHDBName + '.dbo.Client C where C.isDeleted = 0' 
EXEC(@SQLCreateTemp)
SELECT @ClientRow = Count(*) FROM #client 

SET @SQLCreateTempIntegration = @SQLCreateTempIntegration + N'SELECT IntegrationTypeId,ROW_NUMBER() OVER(ORDER BY IntegrationTypeId) AS ROWID into #TempIntegrationType FROM ' + @MRPDBName + '.dbo.IntegrationType WHERE Code= '''+@Code+''''
+ ' Declare @IntegrationRow int;'
+ ' Declare @RowCount int=1;'
+ ' DECLARE @ClientRows int  =  '+cast(@ClientRow as nvarchar(50))+' ; '
+ ' DECLARE @PermissionCode nvarchar(max) = ''MQL'''
+ ' SELECT @IntegrationRow = Count(*) FROM #TempIntegrationType'
+ ' While(@RowCount <= @IntegrationRow)'
+ ' BEGIN '									
+ ' Declare @IntegrationTypeIdValue int; '
+ ' SELECT  @IntegrationTypeIdValue=IntegrationTypeId FROM #TempIntegrationType Where ROWID=@RowCount'
+ ' DECLARE @RowCountClient int=1; ' 
+ ' While(@RowCountClient <= @ClientRows) '
+ ' BEGIN '
+ ' DECLARE @ClientIdValue nvarchar(100); '
+ ' SELECT  @ClientIdValue = ClientId FROM #client Where RowId=@RowCountClient '
+ ' IF NOT EXISTS (SELECT * FROM ' + @MRPDBName + '.dbo.Client_Integration_Permission WHERE IntegrationTypeId = @IntegrationTypeIdValue AND ClientId =  @ClientIdValue AND PermissionCode = @PermissionCode) '
+ ' BEGIN '
+ ' INSERT INTO ' + @MRPDBName + '.dbo.Client_Integration_Permission (ClientId,IntegrationTypeId,PermissionCode,CreatedBy,CreatedDate)VALUES(@ClientIdValue,@IntegrationTypeIdValue,@PermissionCode,@ClientIdValue,GETDate()) '
+ ' END'
+ ' SET @RowCountClient=@RowCountClient+1 '
+ ' END '
+ ' SET @RowCount=@RowCount+1 '
+ ' END ';
EXEC(@SQLCreateTempIntegration)
Go