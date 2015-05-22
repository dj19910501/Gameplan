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


Go
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldOption')
 begin
 IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] =  N'IsDeleted'  AND [object_id] = OBJECT_ID(N'CustomFieldOption'))
 begin
 ALTER TABLE CustomFieldOption
ADD IsDeleted bit  NOT NULL 
CONSTRAINT Delete_Option DEFAULT 0
end
end
GO

GO
WITH tblTemp as
(
SELECT ROW_NUMBER() Over(PARTITION BY ModelId,StageId,StageType ORDER BY StageId)
   As RowNumber,* FROM Model_Stage
)
DELETE From Model_Stage WHERE ModelStageId in 
(
SELECT ModelStageId FROM tblTemp
WHERE RowNumber > 1
)

GO