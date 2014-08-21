Declare @EloquaIntegrationTypeId int
select TOP 1 @EloquaIntegrationTypeId  = IntegrationTypeId from IntegrationType where Title='Eloqua'
delete from GameplanDataType where integrationtypeid=@EloquaIntegrationTypeId and isget=1 and ActualFieldName='Revenue'