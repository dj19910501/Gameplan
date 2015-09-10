 
 -- Created By : Brad Gray
-- Created Date : 09/04/2015
-- Description : Database changes for PL#1368
-- ======================================================================================


-- Remove GameplanDataType 'Owner' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Owner')

	delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Owner')
 end
 go

 -- Remove GameplanDataType 'Start Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
    delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Start Date')

	delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'Start Date')
 end
 go

 -- Remove GameplanDataType 'End Date' from WorkFront integrations
 
  if exists (select * from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) )
  begin
  delete FROM IntegrationInstanceDataTypeMapping where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'End Date')

    delete from [dbo].[GameplanDataType] where GameplanDataTypeId in 
	 (select GameplanDataTypeId from [dbo].[GameplanDataType] where IntegrationTypeId in (select IntegrationTypeId from [dbo].[IntegrationType] where 
	[dbo].[IntegrationType].Code ='WorkFront' ) and DisplayFieldName = 'End Date')
 end
 go

 -- Viral Kadiya
 -- Three Way Integration
 -- 1.) Insert_IntegrationInstanceEloquaID_Column_Script_09_08_2015
 Go
 IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Model]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Model]
	ADD IntegrationInstanceEloquaId int NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END

IF NOT EXISTS (  SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Improvement_Campaign_Program_Tactic]') AND name = 'IntegrationInstanceEloquaId')
BEGIN
	ALTER TABLE [dbo].[Plan_Improvement_Campaign_Program_Tactic]
	ADD IntegrationInstanceEloquaId nvarchar(50) NULL
END

GO

-- 2.) Udate_Data_Script_Model_Table_EloquaInstance_Move_09-08-2015
Declare @eloquaInstanceType varchar(10) ='Eloqua'
UPDATE Model set IntegrationInstanceId=null,IntegrationInstanceEloquaId = inst.IntegrationInstanceId FROM Model as mdl
INNER JOIN IntegrationInstance as inst on mdl.IntegrationInstanceId = inst.IntegrationInstanceId and inst.IntegrationTypeId=(SELECT IntegrationTypeId FROM IntegrationType WHERE Code=@eloquaInstanceType)

-- 3.) Update_Data_Script_Plan_Campaign_Program_Tactic_Table_Move_EloquaID_09-08-2015
UPDATE src set IntegrationInstanceTacticId=NULL,IntegrationInstanceEloquaId = elq.IntegrationInstanceTacticId FROM Plan_Campaign_Program_Tactic as src
INNER JOIN Plan_Campaign_Program_Tactic as elq on src.PlanTacticId = elq.PlanTacticId AND ISNUMERIC(elq.IntegrationInstanceTacticId)=1 AND (elq.IntegrationInstanceTacticId  IS NOT NULL)