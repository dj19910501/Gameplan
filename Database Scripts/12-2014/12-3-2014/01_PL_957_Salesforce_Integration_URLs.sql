--Run Script On MRP

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationType')
BEGIN
	IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code')
	BEGIN
		ALTER TABLE [IntegrationType] ADD Code VARCHAR(255) NULL
	END
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='IntegrationType')
BEGIN
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code' AND IS_NULLABLE = 'YES')
	BEGIN
		Update IntegrationType SET Code = Title
	END
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType' AND COLUMN_NAME = 'Code' AND IS_NULLABLE = 'YES')
	BEGIN
		ALTER TABLE [IntegrationType] ALTER COLUMN Code VARCHAR(255) NOT NULL
	END
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntegrationType' AND TABLE_SCHEMA = 'dbo')
	BEGIN
		IF (SELECT COUNT(*) FROM IntegrationType WHERE (Title = 'Salesforce' OR Title = 'Salesforce-Sandbox')) = 1
		BEGIN
			Update IntegrationType SET Title = 'Salesforce-Sandbox' WHERE Title = 'Salesforce'
			INSERT INTO IntegrationType (Title,IsDeleted,APIURL,Code) VALUES ('Salesforce',0,'https://login.salesforce.com/services/oauth2/token','Salesforce')
		END
	END


IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IntegrationType' AND TABLE_SCHEMA = 'dbo')
BEGIN
	IF (SELECT COUNT(*) FROM IntegrationType WHERE (Title = 'Salesforce' OR Title = 'Salesforce-Sandbox')) = 2
	BEGIN
		DECLARE @SalesforceSandBoxId int = (SELECT IntegrationTypeId FROM IntegrationType WHERE Title = 'Salesforce-Sandbox')
		DECLARE @SalesforceId int = (SELECT IntegrationTypeId FROM IntegrationType WHERE Title = 'Salesforce')
		IF NOT EXISTS (SELECT 1 FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @SalesforceId)
		BEGIN
			INSERT INTO IntegrationTypeAttribute (IntegrationTypeId, Attribute, AttributeType, IsDeleted)
			SELECT @SalesforceId,Attribute,AttributeType,IsDeleted FROM IntegrationTypeAttribute WHERE IntegrationTypeId = @SalesforceSandBoxId
		END
	END
END