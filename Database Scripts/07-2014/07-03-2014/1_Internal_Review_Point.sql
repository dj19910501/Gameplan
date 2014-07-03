
-- Created by : Kalpesh Sharma 
-- Ticket : Internal Reviews Point  
-- This script will be check that ApplicationID field is exists or not in CustomRestriction Table . if it will be not in that table at that time we have to insert that 
   --field with default data 

if not EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'CustomRestriction' AND COLUMN_NAME = 'ApplicationId')
  ALTER TABLE CustomRestriction   ADD ApplicationId uniqueidentifier CONSTRAINT ApplicationId_fk REFERENCES dbo.Application(ApplicationId)  
GO
if EXISTS(select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'CustomRestriction' AND COLUMN_NAME = 'ApplicationId') 
 update CustomRestriction set ApplicationId = (Select ApplicationId from Application where Name = 'Bulldog Gameplan')


