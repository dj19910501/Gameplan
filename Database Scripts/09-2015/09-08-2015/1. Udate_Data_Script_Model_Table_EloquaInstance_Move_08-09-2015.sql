Declare @eloquaInstanceType varchar(10) ='Eloqua'
UPDATE Model set IntegrationInstanceId=null,IntegrationInstanceEloquaId = inst.IntegrationInstanceId FROM Model as mdl
INNER JOIN IntegrationInstance as inst on mdl.IntegrationInstanceId = inst.IntegrationInstanceId and inst.IntegrationTypeId=(SELECT IntegrationTypeId FROM IntegrationType WHERE Code=@eloquaInstanceType)