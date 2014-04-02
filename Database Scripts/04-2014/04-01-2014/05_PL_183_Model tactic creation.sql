if not exists(select * from sys.columns where Name = N'AllowedTargetStage' and Object_ID = Object_ID(N'Model_Funnel_Stage'))
begin
             ALTER TABLE [Model_Funnel_Stage] ADD [AllowedTargetStage] BIT NOT NULL  DEFAULT(0)
end
Go


