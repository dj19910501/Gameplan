-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 11/09/2014
-- Description : Set IsGet field to '0' for ActualCost DataType in GameplanDataType table
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GameplanDataType' AND TABLE_SCHEMA = 'dbo')
BEGIN
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsGet' AND TABLE_NAME = 'GameplanDataType' AND TABLE_SCHEMA = 'dbo')
	BEGIN
		IF (SELECT COUNT(1) FROM GameplanDataType WHERE ActualFieldName = 'CostActual' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsGet, 0) = 1) > 0
		BEGIN
			UPDATE GameplanDataType 
			SET IsGet = 0 
			WHERE ActualFieldName = 'CostActual' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsGet, 0) = 1
		END
	END
END
