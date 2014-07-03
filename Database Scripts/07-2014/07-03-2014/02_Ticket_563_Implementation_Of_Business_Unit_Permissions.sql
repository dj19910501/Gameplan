-- ============================================================================
-- Created By :- Sohel Pathan
-- Created Date :- 07/03/2014
-- Description :- Insert default entry of custom restriction for BusinessUnit.

-- NOTE :- Please changes MRP and BDSAuth DB name, respective to deployment. And Run this script on BDSAuth DB.
-- ============================================================================

DECLARE @MRPDB VARCHAR(50) = 'MRPDev'				-- Set MRP DB name here
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuthDev'			-- Set BDSAuth DB name here
DECLARE @ApplicationCode VARCHAR(100) = 'MRP'		-- Application code (Here MRP = Gameplan)
DECLARE @QUERY VARCHAR(MAX)

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomRestriction')
BEGIN

	SET @QUERY = ''
	SET @QUERY +=  'INSERT INTO ' + @BDSAuth + '.[dbo].[CustomRestriction](UserId, CustomField, CustomFieldId, Permission, CreatedDate, CreatedBy, ApplicationId)
					SELECT U.UserId, ''BusinessUnit'', CAST(B.BusinessUnitId as VARCHAR(50)), ''2'', GETDATE(), UA.UserId, A.ApplicationId
					FROM ' + @BDSAuth + '.[dbo].[Application] A 
					INNER JOIN ' + @BDSAuth + '.[dbo].[User_Application] UA ON UA.ApplicationId = A.ApplicationId AND ISNULL(UA.IsDeleted,0 ) = 0
					INNER JOIN ' + @BDSAuth + '.[dbo].[User] U ON U.UserId = UA.UserId AND ISNULL(U.IsDeleted, 0) = 0
					INNER JOIN ' + @BDSAuth + '.[dbo].[Client] C ON C.ClientId = U.ClientId AND ISNULL(C.IsDeleted,0) = 0
					INNER JOIN ' + @MRPDB + '.[dbo].[BusinessUnit] B ON B.ClientId = C.ClientId AND ISNULL(B.IsDeleted, 0) = 0
					WHERE ISNULL(A.IsDeleted, 0) = 0 AND A.Code = ''' + CAST(@ApplicationCode as VARCHAR) + '''
					ORDER BY U.UserId'

	EXEC(@QUERY)
END