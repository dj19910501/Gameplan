-- Run This Script in MRP
Go
---- Execute this script on MRP database
Declare @varSalesForce varchar(50) ='Salesforce'
Declare @RawCnt int =0
Declare @rawId int =1
Declare @IntegrationTypeId int =0
Declare	@ActualFieldName varchar(50)='ResponseDate'
Declare	@DisplayFieldName varchar(50)='Response Date'
Declare @Type varchar(10)='CW'
Declare @IsDeleted bit = 0
Create TABLE #IntegrationType
    (
		ID int IDENTITY(1, 1) primary key,
		IntegrationTypeId int
    )
Insert Into #IntegrationType Select IntegrationTypeId from IntegrationType where Code =@varSalesForce
Select @RawCnt = Count(ID) from #IntegrationType
While(@rawId <= @RawCnt)
BEGIN
	Select @IntegrationTypeId = IntegrationTypeId from #IntegrationType where ID=@rawId
	IF NOT Exists(Select 1 from GameplanDataTypePull where IntegrationTypeId=@IntegrationTypeId and ActualFieldName=@ActualFieldName and DisplayFieldName = @DisplayFieldName and [Type] = @Type)
	Begin
		Insert Into GameplanDataTypePull(IntegrationTypeId,ActualFieldName,DisplayFieldName,[Type],IsDeleted) Values(@IntegrationTypeId,@ActualFieldName,@DisplayFieldName,@Type,@IsDeleted)
	End
	Set @rawId = @rawId + 1
END

DROP TABLE #IntegrationType

GO

GO

-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/02/2015
-- Description : Remove business unit,Audience,Vertical,Geography tables and its references.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Audience')
BEGIN
DROP TABLE dbo.Audience
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='BusinessUnit')
BEGIN
DROP TABLE dbo.BusinessUnit
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField_Entity_StageWeight')
BEGIN
DROP TABLE dbo.CustomField_Entity_StageWeight
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Geography')
BEGIN
DROP TABLE dbo.Geography
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Vertical')
BEGIN
DROP TABLE dbo.Vertical
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomLabel')
BEGIN
DROP TABLE dbo.CustomLabel
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
ALTER TABLE dbo.Plan_Campaign DROP COLUMN AudienceId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program DROP COLUMN AudienceId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program_Tactic DROP COLUMN AudienceId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program_Tactic DROP COLUMN BusinessUnitId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE dbo.Plan_Improvement_Campaign_Program_Tactic DROP COLUMN BusinessUnitId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
ALTER TABLE dbo.Plan_Campaign DROP COLUMN GeographyId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program DROP COLUMN GeographyId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program_Tactic DROP COLUMN GeographyId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
ALTER TABLE dbo.Plan_Campaign DROP COLUMN VerticalId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program DROP COLUMN VerticalId
END

IF EXISTS (SELECT * FROM sys.columns WHERE [name] = N'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
ALTER TABLE dbo.Plan_Campaign_Program_Tactic DROP COLUMN VerticalId
END

GO

Go 

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationInstance')
BEGIN
        IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'IsFirstPullCW' AND [object_id] = OBJECT_ID(N'IntegrationInstance'))
	    BEGIN
		    ALTER TABLE [IntegrationInstance] ADD IsFirstPullCW BIT NOT NULL DEFAULT 0
	    END
END

GO

---- Execute this script on MRP database
Declare @varSalesForce varchar(50) ='Salesforce'
Declare @RawCnt int =0
Declare @rawId int =1
Declare @IntegrationTypeId int =0
Declare	@ActualFieldName varchar(50)='LastModifiedDate'
Declare	@DisplayFieldName varchar(50)='Last Modified Date'
Declare @Type varchar(10)='CW'
Declare @IsDeleted bit = 0
Create TABLE #IntegrationType
    (
		ID int IDENTITY(1, 1) primary key,
		IntegrationTypeId int
    )
Insert Into #IntegrationType Select IntegrationTypeId from IntegrationType where Code =@varSalesForce
Select @RawCnt = Count(ID) from #IntegrationType
While(@rawId <= @RawCnt)
BEGIN
	Select @IntegrationTypeId = IntegrationTypeId from #IntegrationType where ID=@rawId
	IF NOT Exists(Select 1 from GameplanDataTypePull where IntegrationTypeId=@IntegrationTypeId and ActualFieldName=@ActualFieldName and DisplayFieldName = @DisplayFieldName and [Type] = @Type)
	Begin
		Insert Into GameplanDataTypePull(IntegrationTypeId,ActualFieldName,DisplayFieldName,[Type],IsDeleted) Values(@IntegrationTypeId,@ActualFieldName,@DisplayFieldName,@Type,@IsDeleted)
	End
	Set @rawId = @rawId + 1
END

DROP TABLE #IntegrationType

GO