/* --------- Start Script of PL ticket #1856 --------- */
-- Created by : Brad Gray
-- Created On : 1/07/2016
-- Description : Update storage of WorkFront Template Information - create a new column titled WOrkFrontTemplateId to store the ID column from the 
--               IntegrationWorkFrontTemplates table (primary key) instead of storing the Template ID as nvarchar. 

if not exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFrontTemplateId' AND Object_ID = Object_ID(N'TacticType'))
begin
ALTER TABLE [dbo].[TacticType] ADD WorkFrontTemplateId int
end
GO

IF (OBJECT_ID('FK_TacticType_TacticType_WorkFrontTemplateId', 'F') IS NULL)
ALTER TABLE [dbo].[TacticType]  WITH CHECK ADD  CONSTRAINT [FK_TacticType_TacticType_WorkFrontTemplateId] FOREIGN KEY([WorkFrontTemplateId])
REFERENCES [dbo].[IntegrationWorkFrontTemplates] ([ID])
GO


declare @WorkFrontTemplate varchar(22) = '[WorkFront Template]';
declare @WorkFrontTemplateId nvarchar(25) = '[WorkFrontTemplateId]';
DECLARE @sqlText nvarchar(1000); 
if ( exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFront Template' AND Object_ID = Object_ID(N'TacticType'))
and exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFrontTemplateId'  AND Object_ID = Object_ID(N'TacticType')) )
begin
SET @sqlText = 'update a
set a.'+@WorkFrontTemplateId +' = b.ID
from TacticType a
inner join IntegrationworkFrontTemplates b on (a.' + @WorkFrontTemplate +' = b.TemplateId) and b.IntegrationInstanceId = 19'
EXEC (@sqlText)
end

if  exists (SELECT * FROM sys.columns  WHERE Name = N'WorkFront Template' AND Object_ID = Object_ID(N'TacticType'))
begin
alter table [dbo].[TacticType] drop column [WorkFront Template]
end
go


