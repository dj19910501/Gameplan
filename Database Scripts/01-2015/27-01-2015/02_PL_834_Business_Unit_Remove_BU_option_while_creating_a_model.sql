---- set Client Id in Model table and remove BU Id column


IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ClientId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	ALTER TABLE dbo.Model ADD ClientId UNIQUEIDENTIFIER
END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	UPDATE dbo.Model SET ClientId=bu.ClientId FROM dbo.BusinessUnit AS bu WHERE dbo.Model.BusinessUnitId=bu.BusinessUnitId 
END

ALTER TABLE dbo.Model ALTER COLUMN ClientId UNIQUEIDENTIFIER NOT NULL

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Model_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[Model]'))
ALTER TABLE [dbo].[Model] DROP CONSTRAINT [FK_Model_BusinessUnit]

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    -- Column Exists
	ALTER TABLE dbo.Model DROP COLUMN BusinessUnitId
END



