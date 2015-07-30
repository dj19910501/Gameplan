-- Created By : Brad Gray
-- Created Date : 07/24/2015
-- Description :Database change to allow CustomFields to be ReadOnly
-- ======================================================================================

-- Add new column in CustomField called "IsGet"

IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsGet' AND Object_ID = Object_ID(N'CustomField'))
  ALTER TABLE [dbo].[CustomField]   ADD IsGet bit NOT NULL default 0
  Go