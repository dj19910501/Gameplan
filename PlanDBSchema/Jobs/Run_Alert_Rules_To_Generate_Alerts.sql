/*===================================================================================
Added By : Arpita Soni on 08/23/2016
Ticket : #2536
Description: Create a SQL Job to run alert rules in backend
===================================================================================*/
BEGIN TRANSACTION

DECLARE @PlanDatabaseName NVARCHAR(255)='' --The name of the plan database which store proc. will be executed through this job
DECLARE @JobName NVARCHAR(1000)='Run_Alert_Rules_To_Generate_Alerts' -- Client wise name of the job.

IF NOT EXISTS(SELECT job_id FROM msdb.dbo.sysjobs WHERE (name = N'Run_Alert_Rules_To_Generate_Alerts'))
BEGIN
	Declare @JobScheduleTime INT = 010000 -- Time in HHmmss format E.g. 1:23 PM is equal to 132300

	IF (@PlanDatabaseName <>'' OR @JobName<>'')
	BEGIN

		/****** Object:  Job [Run_Alert_Rules_Daily]    Script Date: 08/22/2016 08:56:58 PM ******/
		DECLARE @ReturnCode INT
		SELECT @ReturnCode = 0

		/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 08/22/2016 08:56:58 PM ******/
		IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
		BEGIN
		EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
		END

		DECLARE @jobId BINARY(16)
		DECLARE @LoginName NVARCHAR(1000)=SUSER_SNAME()  

		EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=@JobName, 
				@enabled=1, 
				@notify_level_eventlog=0, 
				@notify_level_email=0, 
				@notify_level_netsend=0, 
				@notify_level_page=0, 
				@delete_level=0, 
				@description=N'This job will run alert rules and generate alerts based on that.', 
				@category_name=N'[Uncategorized (Local)]', 
				@owner_login_name=@LoginName, @job_id = @jobId OUTPUT
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	
		/****** Object:  Step [Run stored procedure RunAlertRules]    Script Date: 08/22/2016 08:56:58 PM ******/
		DECLARE @cmd nvarchar(1000)=N'EXEC '+@PlanDatabaseName+'.[dbo].[RunAlertRules] '

		EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Run stored procedure RunAlertRules', 
				@step_id=1, 
				@cmdexec_success_code=0, 
				@on_success_action=1, 
				@on_success_step_id=0, 
				@on_fail_action=2, 
				@on_fail_step_id=0, 
				@retry_attempts=0, 
				@retry_interval=0, 
				@os_run_priority=0, @subsystem=N'TSQL', 
				@command=@cmd, 
				@database_name=N'master', 
				@flags=0
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	
		EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	
		EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Run alert rules', 
				@enabled=1, 
				@freq_type=4, 
				@freq_interval=1, 
				@freq_subday_type=1, 
				@freq_subday_interval=0, 
				@freq_relative_interval=0, 
				@freq_recurrence_factor=0, 
				@active_start_date=20160822, 
				@active_end_date=99991231, 
				@active_start_time=@JobScheduleTime, 
				@active_end_time=235959, 
				@schedule_uid=N'7d910e5b-bf10-4288-8bfd-fc01e590eeb6'
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	
		EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

		END
		END
		COMMIT TRANSACTION
		GOTO EndSave
		QuitWithRollback:
			IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
		EndSave:

GO

/*===================================================================================
Completed By : Arpita Soni
===================================================================================*/
