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
