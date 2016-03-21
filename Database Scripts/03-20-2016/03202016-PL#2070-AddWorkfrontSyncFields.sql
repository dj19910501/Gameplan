/* ------------- Start - Related to PL ticket #2070 ------------- 
Created By: Brad Gray
Created Date: 03/20/2016
Description: Add Fields for Workfront sync mapping
*/

DECLARE @inst as int = (SELECT IntegrationTypeId FROM [dbo].[IntegrationType] where Title = 'WorkFront')

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'StartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program_Tactic', 'StartDate', 'Tactic Start Date', 0,0,0)
 
  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'EndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program_Tactic', 'EndDate', 'Tactic End Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ParentCampaign')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'ParentCampaign', 'Parent Campaign', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ParentProgram')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ParentProgram', 'Parent Program', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramOwner')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramOwner', 'Program Owner', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignOwner')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'CampaignOwner', 'Campaign Owner', 0,0,0)
     
  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignStartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'CampaignStartDate', 'Campaign Start Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignEndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'Campaign End Date', 'Campaign End Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramStartDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramStartDate', 'Program Start Date', 0,0,0)

  if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'ProgramEndDate')
  insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign_Program', 'ProgramEndDate', 'Program End Date', 0,0,0)