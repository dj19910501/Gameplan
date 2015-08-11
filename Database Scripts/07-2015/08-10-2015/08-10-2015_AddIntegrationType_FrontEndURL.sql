-- Created By : Brad Gray
-- Created Date : 08/10/2015
-- Description :Add FrontEndURL column to IntegrationType table
-- ======================================================================================


IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'FrontEndUrl') 
ALTER TABLE [dbo].[IntegrationType] ADD FrontEndUrl nvarchar(50)
GO


IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Eloqua' AND [FrontEndUrl] = 'https://login.eloqua.com')
  begin
  declare @EloquaInstanceId int
  set  @EloquaInstanceId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Eloqua')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://login.eloqua.com' where IntegrationTypeId = @EloquaInstanceId
 end
 go

 IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Salesforce-Sandbox' AND [FrontEndUrl] = 'https://test.salesforce.com')
  begin
  declare @SalesforceSandboxIntegrationId int
  set  @SalesforceSandboxIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce-Sandbox')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://test.salesforce.com' where IntegrationTypeId = @SalesforceSandboxIntegrationId
 end
 go

  IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'Salesforce' AND [FrontEndUrl] = 'https://login.salesforce.com')
  begin
  declare @SalesforceIntegrationId int
  set  @SalesforceIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = 'https://login.salesforce.com' where IntegrationTypeId = @SalesforceIntegrationId
 end
 go

  IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'WorkFront' AND [FrontEndUrl] = '.attask-ondemand.com')
  begin
  declare @WFIntegrationId int
  set  @WFIntegrationId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'WorkFront')
 update [dbo].[IntegrationType]
 set [FrontEndUrl] = '.attask-ondemand.com' where IntegrationTypeId = @WFIntegrationId
 end
 go


