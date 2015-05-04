-- Run This script in MRP
GO
-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 05/01/2015
-- Description :replace bulldog with Hive9
-- ======================================================================================
GO
UPDATE dbo.[Notification] SET EmailContent=REPLACE(EmailContent,'bulldog','Hive9') WHERE EmailContent LIKE '%bulldog%'
GO
