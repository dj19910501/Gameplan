-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/18/2014
-- Description :- Drop GeographyId field from User table
-- NOTE :- Run this script on 'BDSAuth' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='User')
BEGIN
	
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'User' AND COLUMN_NAME = 'GeographyId')
	BEGIN
		ALTER TABLE [User] DROP COLUMN GeographyId
	END
END