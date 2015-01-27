----- Remove Fk and allowed NULL values

BEGIN TRANSACTION DeleteVerAudGeoBuIds

--------------Tactic Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_BusinessUnit]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Tactic_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Vertical]

------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN BusinessUnitId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program_Tactic ALTER COLUMN AudienceId INTEGER NULL
END

------------------ Program Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Program_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program]'))
ALTER TABLE [dbo].[Plan_Campaign_Program] DROP CONSTRAINT [FK_Plan_Campaign_Program_Vertical]

------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign_Program'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign_Program ALTER COLUMN AudienceId INTEGER NULL
END


----------------Campaign Table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Audience]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Audience]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Geography]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Geography]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Campaign_Vertical]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Campaign]'))
ALTER TABLE [dbo].[Plan_Campaign] DROP CONSTRAINT [FK_Plan_Campaign_Vertical]

---------------
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN VerticalId INTEGER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN	
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN GeographyId UNIQUEIDENTIFIER NULL
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Campaign'))
BEGIN
	ALTER TABLE dbo.Plan_Campaign ALTER COLUMN AudienceId INTEGER NULL
END


-------- Improvement Tactic table
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Plan_Improvement_Campaign_Program_Tactic]'))
ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic] DROP CONSTRAINT [FK_Plan_Improvement_Campaign_Program_Tactic_BusinessUnit]

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
	ALTER TABLE dbo.Plan_Improvement_Campaign_Program_Tactic ALTER COLUMN BusinessUnitId INTEGER NULL
END

COMMIT TRANSACTION DeleteVerAudGeoBuIds