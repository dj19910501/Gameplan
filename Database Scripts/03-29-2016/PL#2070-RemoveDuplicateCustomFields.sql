/* ------------- Start - Related to PL ticket #2070 ------------- 
Created By: Brad Gray
Created Date: 03/29/2016
Description: Remove erroneous 'Campaign End Date' Custom fields and insert 'CampaignEndDate' custom field
*/


  DECLARE @inst as int = (SELECT IntegrationTypeId FROM [dbo].[IntegrationType] where Title = 'WorkFront')
  if exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'Campaign End Date')
  begin 

DELETE m
FROM [dbo].[IntegrationInstanceDataTypeMapping] m
INNER JOIN [dbo].[GameplanDataType] t
  on t.ActualFieldName = 'Campaign End Date' and t.IntegrationTypeId = @inst and t.GameplanDataTypeId = m.GameplanDataTypeId
 delete from  [dbo].[GameplanDataType]  where IntegrationTypeId = @inst AND ActualFieldName = 'Campaign End Date'
  end

 if not exists(select * from  [dbo].[GameplanDataType] where IntegrationTypeId = @inst AND ActualFieldName = 'CampaignEndDate')
 insert into [dbo].[GameplanDataType] values(@inst,'Plan_Campaign', 'CampaignEndDate', 'Campaign End Date', 0,0,0)
