--Run below script on MRPQA

Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_IntegrationInstance_IntegrationType]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE [IntegrationInstance] ADD CONSTRAINT FK_IntegrationInstance_IntegrationType FOREIGN KEY (IntegrationTypeId) REFERENCES IntegrationType(IntegrationTypeId)
END


Go
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'IntegrationInstanceId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    Alter table [Model] ADD IntegrationInstanceId int NULL
END

Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Model_IntegrationInstance]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE [Model] ADD CONSTRAINT FK_Model_IntegrationInstance FOREIGN KEY (IntegrationInstanceId) REFERENCES IntegrationInstance(IntegrationInstanceId)
END