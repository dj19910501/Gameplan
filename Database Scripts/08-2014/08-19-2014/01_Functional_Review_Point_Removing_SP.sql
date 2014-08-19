-- =======================================================================================
-- Created By :- Mitesh Vaishnav
-- Created Date :- 08/19/2014
-- Description :- delete store procedure from database
-- NOTE :- Run this script on 'MRP' DB.
-- =================================================
IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='Plan_Task_Delete')
BEGIN
DROP PROCEDURE dbo.Plan_Task_Delete
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='PlanDuplicate')
BEGIN
DROP PROCEDURE dbo.PlanDuplicate
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='SaveModelInboundOutboundEvent')
BEGIN
DROP PROCEDURE dbo.SaveModelInboundOutboundEvent
END

IF EXISTS(SELECT 1 FROM sys.objects WHERE type='p' AND name='Plan_Campaign_Program_Tactic_ActualDelete')
BEGIN
DROP PROCEDURE dbo.Plan_Campaign_Program_Tactic_ActualDelete
END