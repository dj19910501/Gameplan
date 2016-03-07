-- Start Client On Boarding
Go
-- Created By : Rahul Shah
-- Created Date : 09/02/2016
-- Description : insert permission of SFDC "Pull MQL" for Client 
-- ======================================================================================

--change authntication and MRP DB Name.
DECLARE @BDSAUTHDBName nvarchar(50) = 'BDSAuthQA'
DECLARE @MRPDBName nvarchar(50) = 'MRPQA'

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
-- End Client On Boarding

-- Start Migration script
DECLARE @ClientId UNIQUEIDENTIFIER;

SET @ClientId = 'C251AB18-0683-4D1D-9F1E-06709D59FD53';  -- Set ClientId for Zebra.

IF (NOT EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Plan_Campaign_Program_Tactic_BKP_15Feb2015'))
BEGIN
   SELECT * INTO Plan_Campaign_Program_Tactic_BKP_15Feb2015 FROM Plan_Campaign_Program_Tactic;
END

;WITH RawTable AS (
	SELECT 
		T.PlanTacticId, T.Title Tactic, T.IsDeployedToIntegration, T.IsSyncSalesForce, T.IsSyncEloqua,M.IntegrationInstanceId,M.IntegrationInstanceEloquaId
	FROM Plan_Campaign_Program_Tactic T
		INNER JOIN Plan_Campaign_Program PP ON PP.PlanProgramId = T.PlanProgramId AND PP.IsDeleted = 0
		INNER JOIN Plan_Campaign C ON C.PlanCampaignId = PP.PlanCampaignId AND C.IsDeleted = 0
		INNER JOIN [Plan] P ON P.PlanId = C.PlanId AND P.IsDeleted = 0 --AND P.Status = 'Published'
		INNER JOIN Model M ON M.ModelId = P.ModelId AND M.IsDeleted = 0 AND (M.IntegrationInstanceId IS NOT NULL OR M.IntegrationInstanceEloquaId IS NOT NULL) AND M.ClientId = @ClientId
	WHERE T.IsDeleted = 0
	AND T.IsDeployedToIntegration = 1
)
UPDATE Plan_Campaign_Program_Tactic 
	SET 
		IsSyncSalesForce	= ISNULL(RawTable.IntegrationInstanceId,0),
		IsSyncEloqua		= ISNULL(RawTable.IntegrationInstanceEloquaId,0)
FROM Plan_Campaign_Program_Tactic T
INNER JOIN RawTable ON RawTable.PlanTacticId = T.PlanTacticId

GO

/* --------- Start Script of PL ticket #1979 --------- */
-- Added by Viral Kadiya on 02/16/2016
-- Increase 'ActionSuffix' column size from '50' to 'MAX'

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ActionSuffix' AND OBJECT_ID = OBJECT_ID(N'Changelog'))
BEGIN
ALTER TABLE [Changelog]
ALTER COLUMN [ActionSuffix] NVARCHAR(MAX)
END  
GO
/* --------- End Script of PL ticket #1979 --------- */

/* --------- Start Script of PL ticket #2006 --------- */
-- Added by Viral Kadiya on 02/19/2016
-- Increase 'ErrorDescription' column size from '8000' to 'MAX'

IF EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ErrorDescription' AND OBJECT_ID = OBJECT_ID(N'IntegrationInstancePlanEntityLog'))
BEGIN
ALTER TABLE [IntegrationInstancePlanEntityLog]
ALTER COLUMN [ErrorDescription] NVARCHAR(MAX)
END  
GO
/* --------- End Script of PL ticket #2006 --------- */

/* --------- Start Script of PL ticket #2022 --------- */
-- Added by Rahul Shah on 02/29/2016
-- insert 'Pull MQL' Data For SFDC in GameplanDataTypePull  
IF OBJECT_ID('tempdb..#TempIntegrationTypeType') IS NOT NULL
    DROP TABLE #TempIntegrationTypeType
--Please do not make any change below variables and its values.    
DECLARE @IntegrationRow int=0
DECLARE @RowCount as int=1
DECLARE @IntegrationTypeIdValue int
DECLARE @Code nvarchar (50) = 'Salesforce'
DECLARE @Type nvarchar (50) = 'MQL'
DECLARE @FieldNameStatus nvarchar (50) = 'Status'
DECLARE @FieldNameTimeStamp nvarchar (50) = 'Timestamp'
DECLARE @FieldNameCampaignID nvarchar (50) = 'CampaignID'
DECLARE @DisplayFieldNameCampaignID nvarchar (50) = 'Campaign ID'
SELECT IntegrationTypeId,ROW_NUMBER() OVER(ORDER BY IntegrationTypeId) AS ROWID into #TempIntegrationTypeType FROM IntegrationType WHERE Code=@Code
SELECT @IntegrationRow = Count(*) FROM #TempIntegrationTypeType 
While(@RowCount <= @IntegrationRow )
BEGIN
	SELECT  @IntegrationTypeIdValue=IntegrationTypeId FROM #TempIntegrationTypeType Where ROWID=@RowCount
	IF NOT EXISTS (SELECT * FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeIdValue AND Type = @Type AND ActualFieldName = @FieldNameStatus AND DisplayFieldName = @FieldNameStatus AND isDeleted = 0)
	BEGIN

    INSERT INTO [GameplanDataTypePull](IntegrationTypeId,ActualFieldName,DisplayFieldName,Type,IsDeleted) 
         VALUES(@IntegrationTypeIdValue,@FieldNameStatus,@FieldNameStatus,@Type,0)
	END
	
	IF NOT EXISTS (SELECT * FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeIdValue AND Type = @Type AND ActualFieldName = @FieldNameTimeStamp AND DisplayFieldName = @FieldNameTimeStamp AND isDeleted = 0)
	BEGIN

    INSERT INTO [GameplanDataTypePull](IntegrationTypeId,ActualFieldName,DisplayFieldName,Type,IsDeleted) 
         VALUES(@IntegrationTypeIdValue,@FieldNameTimeStamp,@FieldNameTimeStamp,@Type,0)
	END
	
	IF NOT EXISTS (SELECT * FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeIdValue AND Type = @Type AND ActualFieldName = @FieldNameCampaignID AND DisplayFieldName = @DisplayFieldNameCampaignID AND isDeleted = 0)
	BEGIN

    INSERT INTO [GameplanDataTypePull](IntegrationTypeId,ActualFieldName,DisplayFieldName,Type,IsDeleted) 
         VALUES(@IntegrationTypeIdValue,@FieldNameCampaignID,@DisplayFieldNameCampaignID,@Type,0)
	END
     
SET @RowCount=@RowCount+1;
END
GO
/* --------- End Script of PL ticket #2022 --------- */

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](50) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](50) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(10)
declare @release nvarchar(10)
set @release = 'Feb.2016'
set @version = 'Feb.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
-- End Migration script