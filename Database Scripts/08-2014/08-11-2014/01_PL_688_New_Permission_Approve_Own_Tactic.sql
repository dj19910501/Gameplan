--Run this script on BDSAuth database

--==============================Start Script-1 (Update dbo.Application_Activity table) ===============================================
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Application_Activity')
BEGIN

INSERT INTO  dbo.Application_Activity
        (ApplicationActivityId, ApplicationId, ParentId, ActivityTitle, Code,
         CreatedDate)
VALUES
        (21,'1c10d4b9-7931-4a7c-99e9-a158ce158951',7,'Approve Own Tactic','TacticApproveOwn', GETDATE())


END
--==============================End Script-1 (Update dbo.Application_Activity table) ===============================================