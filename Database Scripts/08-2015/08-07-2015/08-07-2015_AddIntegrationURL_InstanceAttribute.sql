-- Created By : Brad Gray
-- Created Date : 08/07/2015
-- Description :Add new integration attribute for WorkFront called "Integration URL"
-- ======================================================================================

-- Add new integration attribute for WorkFront called "Integration URL"

  if not exists(select Attribute from [dbo].[IntegrationTypeAttribute] where Attribute = 'Integration URL')
  begin
  declare @WorkFrontIntegrationTypeId int
  set  @WorkFrontIntegrationTypeId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'WorkFront')
  insert into [dbo].[IntegrationTypeAttribute] 
  values(@WorkFrontIntegrationTypeId, 'Integration URL', 'textbox', 0)

  declare @MarketoIntegrationTypeId int
  set  @MarketoIntegrationTypeId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Marketo')
  insert into [dbo].[IntegrationTypeAttribute] 
  values(@MarketoIntegrationTypeId, 'Integration URL', 'textbox', 0)

  declare @EloquaIntegrationTypeId int
  set  @EloquaIntegrationTypeId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Eloqua')
  insert into [dbo].[IntegrationTypeAttribute] 
  values(@EloquaIntegrationTypeId, 'Integration URL', 'textbox', 0)

  declare @SalesforceIntegrationTypeId int
  set  @SalesforceIntegrationTypeId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce')
  insert into [dbo].[IntegrationTypeAttribute] 
  values(@SalesforceIntegrationTypeId, 'Integration URL', 'textbox', 0)

  declare @SalesforceSandboxIntegrationTypeId int
  set  @SalesforceSandboxIntegrationTypeId = (select IntegrationTypeId from [dbo].[IntegrationType] where title = 'Salesforce-Sandbox')
  insert into [dbo].[IntegrationTypeAttribute] 
  values(@SalesforceSandboxIntegrationTypeId, 'Integration URL', 'textbox', 0)

  end
  go

