 
 -- Created By : Brad Gray
-- Created Date : 09/04/2015
-- Description : Database changes for PL#1368
-- ======================================================================================


-- Remove GameplanDataType 'Owner' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Owner')
 end
 go

 -- Remove GameplanDataType 'Start Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Start Date')
 end
 go

 -- Remove GameplanDataType 'End Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'End Date')
 end
 go