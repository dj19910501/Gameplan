-- =======================================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 08/07/2014
-- Description :- Update DisplayFieldName by removing 'Gameplan.[Tablename]' from it
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataType')
BEGIN

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataType' AND COLUMN_NAME = 'DisplayFieldName')
	BEGIN
	
		IF (SELECT COUNT(1) FROM GameplanDataType WHERE PATINDEX('%.%', DisplayFieldName) > 0) > 0
		BEGIN
			
			PRINT('Done')

			UPDATE GameplanDataType SET DisplayFieldName = REPLACE(DisplayFieldName, 'Ganeplan.', 'Gameplan.')
			WHERE DisplayFieldName like '%Ganeplan.%'

			UPDATE GameplanDataType 
			SET DisplayFieldName =  REPLACE(REPLACE(DisplayFieldName, 'Gameplan.', ''), LEFT(REPLACE(DisplayFieldName, 'Gameplan.', ''),CHARINDEX('.',REPLACE(DisplayFieldName, 'Gameplan.', ''))), '')
		END

	END

END