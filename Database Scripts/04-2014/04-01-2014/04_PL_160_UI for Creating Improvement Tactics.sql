if   exists(select * from sys.columns where Name = N'ModifiedDate' and Object_ID = Object_ID(N'ImprovementTacticType'))
begin
            ALTER TABLE [ImprovementTacticType] drop column [ModifiedDate] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedBy' and Object_ID = Object_ID(N'ImprovementTacticType'))
begin
            ALTER TABLE [ImprovementTacticType] drop column [ModifiedBy] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedDate' and Object_ID = Object_ID(N'ImprovementTacticType_Metric'))
begin
            ALTER TABLE [ImprovementTacticType_Metric] drop column [ModifiedDate] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedBy' and Object_ID = Object_ID(N'ImprovementTacticType_Metric'))
begin
         ALTER TABLE [ImprovementTacticType_Metric] drop column [ModifiedBy] 
end
GO
