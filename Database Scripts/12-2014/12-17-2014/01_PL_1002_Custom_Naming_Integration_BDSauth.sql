-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 17/12/2014
-- Description : Custom Naming: Integration
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Client_Activity')
BEGIN
DROP TABLE Client_Activity
END