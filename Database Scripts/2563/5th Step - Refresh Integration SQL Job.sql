--
-- Replace <$targetDb>  with your plan database name
--

USE [msdb]
GO

IF EXISTS (SELECT 1 FROM [msdb].[dbo].[sysjobs] J WHERE J.Name = 'Hive9 - AlertLogic Responses/MQL/CW/Revenue Update')
EXEC msdb.dbo.sp_delete_job @job_name=N'Hive9 - AlertLogic Responses/MQL/CW/Revenue Update', @delete_unused_schedule=1
GO

/****** Object:  Job [Hive9 - AlertLogic Responses/MQL/CW/Revenue Update]    Script Date: 9/23/2016 3:32:27 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 9/23/2016 3:32:27 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Hive9 - AlertLogic Responses/MQL/CW/Revenue Update', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'PlanIntegrationUser', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Pull Responses/MQL/CW/Revenue From Measure_AlertLogic_dev on ramp-db3]    Script Date: 9/23/2016 3:32:27 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Pull Responses/MQL/CW/Revenue From Measure_AlertLogic_dev on ramp-db3', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'use <$targetDb>;

--SELECT top 1 * from Plan_campaign_program_tactic; 

DECLARE @ClientID INT =  25;
DECLARE @UserID INT = 373;
DECLARE @IntegrationInstanceID INT = 37;

EXEC [INT].MapIDs ''[ramp-db3].[Measure_AlertLogic_Dev].[dbo].vTacticIDMapping'', @ClientID, @UserID, @IntegrationInstanceID

EXEC [INT].PullActuals ''[ramp-db3].[Measure_AlertLogic_Dev].[dbo].vTacticResponses'', ''ProjectedStageValue'', @ClientID,  @UserID, @IntegrationInstanceID

EXEC [INT].PullActuals ''[ramp-db3].[Measure_AlertLogic_Dev].[dbo].vTacticCW'', ''CW'', @ClientID,  @UserID, @IntegrationInstanceID

EXEC [INT].PullActuals ''[ramp-db3].[Measure_AlertLogic_Dev].[dbo].vTacticMQL'', ''MQL'', @ClientID,  @UserID, @IntegrationInstanceID

EXEC [INT].PullActuals ''[ramp-db3].[Measure_AlertLogic_Dev].[dbo].vTacticRevenue'', ''Revenue'', @ClientID,  @UserID, @IntegrationInstanceID
', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Mid-night pull', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20160824, 
		@active_end_date=99991231, 
		@active_start_time=40000, 
		@active_end_time=235959, 
		@schedule_uid=N'e63c0dfe-be39-49f8-a1c7-f7d63328a5ab'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO
