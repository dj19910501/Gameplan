-- Run This script in BDSAuth
GO
-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 05/01/2015
-- Description :replace bulldog with Hive9
-- ======================================================================================
GO
UPDATE dbo.[Application] SET Name=REPLACE(Name,'bulldog','Hive9'),Description=REPLACE(Description,'bulldog','Hive9')
GO
