-- Created By : Brad Gray
-- Created Date : 07/16/2015
-- Description :Database changes to enable WorkFront Integration
-- ======================================================================================


--Add WorkFront entry to IntegrationType
IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationType] WHERE [Title] = 'WorkFront' AND [APIURL] = '.attask-ondemand.com/attask/api')
INSERT INTO [dbo].[IntegrationType] (Title, IsDeleted, APIURL, Code) values ('WorkFront', 0, '.attask-ondemand.com/attask/api', 'WorkFront')
GO

DECLARE @IntegrationTypeID int;

select  @IntegrationTypeID = IntegrationTypeId from [dbo].[IntegrationType]	where Title = 'WorkFront' and [APIURL] = '.attask-ondemand.com/attask/api'

--Insert Datatypes for mapping Gameplan Fields to WorkFront
IF NOT EXISTS(SELECT * FROM [dbo].[GameplanDataType] WHERE [IntegrationTypeId] = @IntegrationTypeID)
begin
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'Title', 'Name', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'Description', 'Description', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'StartDate', 'Start Date', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'EndDate', 'End Date', 0,0,0)
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Global', 'CreatedBy', 'Owner', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Plan_Campaign_Program_Tactic', 'Cost', 'Cost (Budgeted)', 0,0,0) 
	insert into [dbo].[GameplanDataType]
		values(@IntegrationTypeID, 'Plan_Campaign_Program_Tactic', 'CostActual', 'Cost (Actual)', 0,0,0) 
end
GO
			 
DECLARE @IntegrationTypeID int;

select  @IntegrationTypeID = IntegrationTypeId from [dbo].[IntegrationType]	where Title = 'WorkFront' and [APIURL] = '.attask-ondemand.com/attask/api'
-- Add WorkFront integration attribute to IntegrationTypeAttribute
--WorkFront api requires the company name in front of the uri
IF NOT EXISTS(SELECT * FROM [dbo].[IntegrationTypeAttribute] WHERE [IntegrationTypeID] = @IntegrationTypeID AND [Attribute] = 'Company Name')
	insert into [dbo].[IntegrationTypeAttribute] values(@IntegrationTypeID, 'Company Name', 'textbox', 0)
GO

-- Add new column in Plan_Campaign_Program_Tactic called "IntegrationWorkFrontProjectID"
--This is designed to contain WorkFront Project IDs to link the tactic to the project
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'IntegrationWorkFrontProjectID') 
ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] 	ADD IntegrationWorkFrontProjectID nvarchar(50)
GO
