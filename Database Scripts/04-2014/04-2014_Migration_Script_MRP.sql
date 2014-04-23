--02_PL_375_Change Password Email Content.sql
IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'ChangePassword')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear [NameToBeReplaced],<br/><br/>This message is to inform you that your password was recently changed on your Bulldog Gameplan account. If you did not perform this action, please contact you administrator as soon as possible to resolve this issue.<br/><br/>Thank You,<br />Bulldog Gameplan Team',
		   [Subject] = 'Bulldog Gameplan Password Changed'
	WHERE NotificationInternalUseOnly = 'ChangePassword'
END

--04_PL_160_UI for Creating Improvement Tactics.sql
if   exists(select * from sys.columns where Name = N'ModifiedDate' and Object_ID = Object_ID(N'ImprovementTacticType'))
begin
            ALTER TABLE [ImprovementTacticType] drop column [ModifiedDate] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedBy' and Object_ID = Object_ID(N'ImprovementTacticType'))
begin
            ALTER TABLE [ImprovementTacticType] drop column [ModifiedBy] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedDate' and Object_ID = Object_ID(N'ImprovementTacticType_Metric'))
begin
            ALTER TABLE [ImprovementTacticType_Metric] drop column [ModifiedDate] 
end
GO

if  exists(select * from sys.columns where Name = N'ModifiedBy' and Object_ID = Object_ID(N'ImprovementTacticType_Metric'))
begin
         ALTER TABLE [ImprovementTacticType_Metric] drop column [ModifiedBy] 
end
GO

 --05_PL_183_Model tactic creation.sql
 if not exists(select * from sys.columns where Name = N'AllowedTargetStage' and Object_ID = Object_ID(N'Model_Funnel_Stage'))
begin
             ALTER TABLE [Model_Funnel_Stage] ADD [AllowedTargetStage] BIT NOT NULL  DEFAULT(0)
end
Go
/*set AllowedTargetStage true which Model stage are used in Tactic_Type table */
Update Model_Funnel_Stage set AllowedTargetStage=1 
 where ModelFunnelId in
 (
	select ModelFunnelId from Model_Funnel where ModelId In 
	(
		select ModelId from TacticType where StageId Is Not NuLL
	)
 ) and StageType='CR'
Go 
/****** Update Description column of  Stage table by client wise and stage code.  ******/
Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'

Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'

Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'


--06_PL_183_Model tactic creation.sql
 /*set AllowedTargetStage true which Model stage are used in Tactic_Type table */
Update Model_Funnel_Stage set AllowedTargetStage=1 
 where ModelFunnelId in
 (
	select ModelFunnelId from Model_Funnel where ModelId In 
	(
		select ModelId from TacticType where StageId Is Not NuLL
	)
 ) and StageType='CR'

 --01_PL_159_UI_for_Best_in_class_data_entry.sql
update BestInClass set Value=Value*100 from BestInClass
inner join metric on BestInClass .MetricId = metric.MetricId
where MetricType='CR' and Value > 0 and Value <= 1


--2_PL_326_Forgot_Password_functionality.sql
-- Execute Below Script in MRPDev
Go
IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ResetPasswordLink')
BEGIN

INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ResetPasswordLink', N'ResetPasswordLink', N'When user requested to reset password', N'CM', N'This e-mail has been sent in response to your request for help resetting your Gameplan password.<br /> To initiate the process for resetting the password for your Gameplan account, follow the link below: <br /><br /> [PasswordResetLinkToBeReplaced] <br/><br />Note that this e-mail will expire on [ExpireDateToBeReplaced]. If it expires before you are able to complete the password reset process, you may request a new password reset.<br/><br />Thank you,<br/>Gameplan Administrator', 0, CAST(0x0000A2FC00C8772A AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Password reset link')
END

--1_376_Remove_storing_of_pre-calculated_MQL_to_DB.sql
GO
IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Plan_Campaign_Program_Tactic' 
           AND  COLUMN_NAME = 'MQLs')
BEGIN
		   ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN MQLs
END
GO
IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Plan_Campaign_Program' 
           AND  COLUMN_NAME = 'MQLs')
BEGIN
		   ALTER TABLE Plan_Campaign_Program DROP COLUMN MQLs
END
GO
IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Plan_Campaign' 
           AND  COLUMN_NAME = 'MQLs')
BEGIN
		   ALTER TABLE Plan_Campaign DROP COLUMN MQLs
END
GO
GO

/****** Object:  StoredProcedure [dbo].[PlanDuplicate]    Script Date: 4/3/2014 5:32:18 PM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PlanDuplicate')
BEGIN
	DROP PROCEDURE [dbo].[PlanDuplicate]
END
GO

/****** Object:  StoredProcedure [dbo].[PlanDuplicate]    Script Date: 4/3/2014 5:32:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--Stored Procedure
CREATE PROCEDURE [dbo].[PlanDuplicate]    
    (    
      @PlanId INT,    
	  @PlanStatus VARCHAR(255),    
      @TacticStatus VARCHAR(255),    
      @CreatedDate DateTime,    
	  @CreatedBy UNIQUEIDENTIFIER,   
	  @Suffix VARCHAR(50),
	  @CopyClone VARCHAR(50),
	  @Id INT,
      @ReturnValue INT = 0 OUT              
    )    
AS     
BEGIN

  SET NOCOUNT ON    
	     
	DECLARE @Title varchar(255), @clientId UNIQUEIDENTIFIER;
	DECLARE @newId int
	DECLARE @IsCount int
	----Declaring mapping table to hold old and new value
	DECLARE @TempPlanCampaign TABLE (OldPlanCampaignId int, NewPlanCampaignId int);
	DECLARE @TempPlanProgram TABLE (OldPlanProgramId int, NewPlanProgramId int);
	BEGIN TRANSACTION PlanDuplicate  
	if(@CopyClone = 'Plan') -- 4
	BEGIN
		----Getting plan title and client id to check whether copy plan title already exist for client of current plan.
		SELECT @Title = [PLAN].Title + @Suffix, @clientId = ClientId FROM [PLAN] 
		INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
		INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
		WHERE [PLAN].PlanId=@PlanId
		
			IF EXISTS(SELECT 1 FROM [PLAN] 
			INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
			INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
			WHERE ClientId=@clientId AND  [PLAN].Title = @Title)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
			----Inserting Plan.
				INSERT INTO [dbo].[Plan] 
		           ([ModelId]
				   ,[Title]
				   ,[Version]
				   ,[Description]
				   ,[MQLs]
				   ,[Budget]
				   ,[Status]
				   ,[IsActive]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
				   ,[ModifiedBy]
				   ,[Year])
				SELECT   [ModelId]
						,@Title
						,[Version]
						,[Description]
						,[MQLs]
						,[Budget]
						,@PlanStatus
						,[IsActive]
						,[IsDeleted]
						,@CreatedDate
						,@CreatedBy
						,null
						,null
						,[Year]
					 from [PLAN] WHERE PlanId = @PlanId

			  	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            

			----Getting id of new Plan.
			SET @newId = SCOPE_IDENTITY();  
			SET @IsCount = 4;
			END
	
	END
	ELSE if(@CopyClone = 'Campaign') -- 3
	BEGIN
		----Getting campaign title to check whether copy campaign title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign WHERE PlanCampaignId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign 
			WHERE Title = @Title AND PlanId = @PlanId)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				----Inserting Campaign.
				INSERT INTO [dbo].[Plan_Campaign]
					([PlanId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[IsDeleted]
				   ,@CreatedDate
					,@CreatedBy
				   ,NULL
					,NULL
					FROM Plan_Campaign
					WHERE PlanCampaignId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY();
					SET @IsCount = 3;
					INSERT INTO @TempPlanCampaign Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Program') -- 2
	BEGIN
		----Getting Program title to check whether copy Program title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program WHERE PlanProgramId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program]
				   ([PlanCampaignId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanCampaignId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[IsDeleted]
				   ,@CreatedDate
					,@CreatedBy
				   ,NULL
					,NULL
					FROM Plan_Campaign_Program
					WHERE PlanProgramId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 2;
					INSERT INTO @TempPlanProgram Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Tactic') -- 1
	BEGIN
		----Getting Tactic title to check whether copy Tactic title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program_Tactic.PlanProgramId = Plan_Campaign_Program.PlanProgramId 
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program_Tactic.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program_Tactic.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program_Tactic]
					   ([PlanProgramId]
					   ,[TacticTypeId]
					   ,[Title]
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,[BusinessUnitId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,[INQsActual]
					   ,[MQLsActual]
					   ,[CWs]
					   ,[CWsActual]
					   ,[Revenues]
					   ,[RevenuesActual]
					   ,[Cost]
					   ,[CostActual]
					   ,[ROI]
					   ,[ROIActual]
					   ,[Status]
					   ,[IsDeleted]
					   ,[CreatedDate]
					   ,[CreatedBy]
					   ,[ModifiedDate]
					   ,[ModifiedBy])
					SELECT
						PlanProgramId
						,TacticTypeId
					   ,@Title
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,BusinessUnitId
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,NULL
					   ,NULL
					   ,[CWs]
					   ,NULL
					   ,[Revenues]
					   ,NULL
					   ,[Cost]
					   ,NULL
					   ,[ROI]
					   ,NULL
					   ,@TacticStatus
					   ,[IsDeleted]
					  ,@CreatedDate
						,@CreatedBy
					   ,NULL
						,NULL
						FROM [Plan_Campaign_Program_Tactic]
						WHERE PlanTacticId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 1;
			END
	END
	-- Check Duplicate for Plan, Campaign, Program or Tactic and insert child table entry
	IF(@IsCount >= 4)
	BEGIN
	----Inserting Campaign.
		----Inserting campaign for new plan and populating mapping table with old and new id for campaign
		;WITH [sourceCampaign] AS (
						SELECT
								PlanCampaignId
							   ,@newId AS [PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
						FROM Plan_Campaign WHERE PlanId=@PlanId AND IsDeleted = 0)
		MERGE Plan_Campaign AS TargetCampaign
		USING sourceCampaign
		ON 0 = 1
		WHEN NOT MATCHED THEN
						INSERT  ([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,[CreatedDate]
							   ,[CreatedBy]
							   ,[ModifiedDate]
							   ,[ModifiedBy])
					  VALUES	([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,@CreatedDate
							   ,@CreatedBy
							   ,NULL
							   ,NULL)
		OUTPUT
							  sourceCampaign.PlanCampaignId,
							  inserted.PlanCampaignId
		INTO @TempPlanCampaign (OldPlanCampaignId, NewPlanCampaignId);
	
			  IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 3)
	BEGIN
	----Inserting Program.
		----Inserting program for new plan's campaign and populating mapping table with old and new id for program
		;WITH SourcePlanProgram AS (
								SELECT  Plan_Campaign_Program.PlanProgramId
									   ,mappingPlanCampaign.NewPlanCampaignId AS PlanCampaignId
									   ,[Title]
									   ,[Description]
									   ,[VerticalId]
									   ,[AudienceId]
									   ,[GeographyId]
									   ,[StartDate]
									   ,[EndDate]
									   ,[INQs]
									   ,[Cost]
								FROM Plan_Campaign_Program
								INNER JOIN @TempPlanCampaign mappingPlanCampaign ON Plan_Campaign_Program.PlanCampaignId = mappingPlanCampaign.OldPlanCampaignId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program AS TargetPlanCampaign
		USING SourcePlanProgram
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanCampaignId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[Cost]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy])
						  VALUES   ([PlanCampaignId]
									,[Title]
									,[Description]
									,[VerticalId]
									,[AudienceId]
									,[GeographyId]
									,[StartDate]
									,[EndDate]
									,[INQs]
									,[Cost]
									,@CreatedDate
									,@CreatedBy
									,NULL
									,NULL)
		OUTPUT
							  SourcePlanProgram.PlanProgramId,
							  inserted.PlanProgramId
		INTO @TempPlanProgram (OldPlanProgramId, NewPlanProgramId);

			  IF @@ERROR <> 0    
 
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 2)
	BEGIN
	
	----Inserting Tactic
		----No need to declaring mapping table to hold old and new value. Just inserting tactic for new plan's program
		;WITH SourcePlanTactic AS (
								SELECT  Plan_Campaign_Program_Tactic.PlanTacticId
				  						,mappingPlanProgram.NewPlanProgramId AS PlanProgramId
										,[TacticTypeId]
										,[Title]
										,[Description]
										,[VerticalId]
										,[AudienceId]
										,[GeographyId]
										,[BusinessUnitId]
										,[StartDate]
										,[EndDate]
										,[INQs]
										,[CWs]
										,[Revenues]
										,[RevenuesActual]
										,[Cost]
										,[ROI]
								FROM Plan_Campaign_Program_Tactic
								INNER JOIN @TempPlanProgram mappingPlanProgram ON Plan_Campaign_Program_Tactic.PlanProgramId = mappingPlanProgram.OldPlanProgramId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program_Tactic AS TargetPlanTactic
		USING SourcePlanTactic
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanProgramId]
								   ,[TacticTypeId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[BusinessUnitId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[INQsActual]
								   ,[MQLsActual]
								   ,[CWs]
								   ,[CWsActual]
								   ,[Revenues]
								   ,[RevenuesActual]
								   ,[Cost]
								   ,[CostActual]
								   ,[ROI]
								   ,[ROIActual]
								   ,[Status]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy])
						  VALUES ([PlanProgramId]
								  ,[TacticTypeId]
								  ,[Title]
								  ,[Description]
								  ,[VerticalId]
								  ,[AudienceId]
								  ,[GeographyId]
								  ,[BusinessUnitId]
								  ,[StartDate]
								  ,[EndDate]
								  ,[INQs]
								  ,NULL
								  ,NULL
								  ,[CWs]
								  ,NULL
								  ,[Revenues]
								  ,NULL
								  ,[Cost]
								  ,NULL
								  ,[ROI]
								  ,NULL
								  ,@TacticStatus
								  ,@CreatedDate
								  ,@CreatedBy
								  ,NULL
								  ,NULL);

			IF @@ERROR <> 0     
            BEGIN                
                SET @ReturnValue = 0 -- 0 ERROR                
                ROLLBACK TRANSACTION PlanDuplicate                    
                RETURN                
            END            
	END

	COMMIT TRANSACTION PlanDuplicate                  
    SET @ReturnValue = @newId;        
    RETURN @ReturnValue      
END

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Update_MQL')
BEGIN
	DROP PROCEDURE [dbo].[Update_MQL]
END
GO

--01_PL_418_Additional e-mail from address.sql
IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'ContactSupport')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear Admin,<br/><br/>Please note that following issue has been submitted.<br><br><table><tr><td>Submitted by</td><td>:</td><td>[EmailToBeReplaced]</td></tr><tr><td>Issue</td><td>:</td><td>[IssueToBeReplaced]</td></tr></table><br><br>Thank You<br>'
	WHERE NotificationInternalUseOnly = 'ContactSupport'
END


--01_PL_222_Remove Drop downs for geography and verticals for campaigns and programs.sql
/* script for remove null constrint in Plan_Campaign and Plan_Campaign_Program table for VerticalId, AudienceId and GeographyId column   */
ALTER TABLE Plan_Campaign ALTER COLUMN  VerticalId int  NULL 
ALTER TABLE Plan_Campaign ALTER COLUMN  AudienceId int  NULL 
ALTER TABLE Plan_Campaign ALTER COLUMN  GeographyId uniqueidentifier  NULL 

ALTER TABLE Plan_Campaign_Program ALTER COLUMN  VerticalId int  NULL 
ALTER TABLE Plan_Campaign_Program ALTER COLUMN  AudienceId int  NULL 
ALTER TABLE Plan_Campaign_Program ALTER COLUMN  GeographyId uniqueidentifier  NULL 

--01_PL_409_Unable to increase Campaign Block.sql
-- Run below script on MRPQA

-- Script to update old data
Go
update Plan_Campaign set [Status] =
	case when 
			(select count(*) from Plan_Campaign_Program_Tactic where IsDeleted = 0 and PlanProgramId = PlanProgramId and [Status] != 'Submitted') = 0	and
			(
				select count(*) from Plan_Campaign_Program_Tactic as a, Plan_Campaign_Program as b
				where a.IsDeleted = 0 and a.PlanProgramId = b.PlanProgramId and b.PlanCampaignId = PlanCampaignId and a.[Status] != 'Submitted'
			) = 0
		then 'Submitted'
		when 
			(select count(*) from Plan_Campaign_Program_Tactic where IsDeleted = 0 and PlanProgramId = PlanProgramId and [Status] != 'In-Progress' and [Status] != 'Approved' and [Status] != 'Complete') = 0	and
			(
				select count(*) from Plan_Campaign_Program_Tactic as a, Plan_Campaign_Program as b
				where a.IsDeleted = 0 and a.PlanProgramId = b.PlanProgramId and b.PlanCampaignId = PlanCampaignId and a.[Status] != 'In-Progress' and a.[Status] != 'Approved' and a.[Status] != 'Complete'
			) = 0
		then 'Approved'
		when 
			(select count(*) from Plan_Campaign_Program_Tactic where IsDeleted = 0 and PlanProgramId = PlanProgramId and [Status] != 'Declined') = 0	and
			(
				select count(*) from Plan_Campaign_Program_Tactic as a, Plan_Campaign_Program as b
				where a.IsDeleted = 0 and a.PlanProgramId = b.PlanProgramId and b.PlanCampaignId = PlanCampaignId and a.[Status] != 'Declined'
			) = 0
		then 'Declined'
	else 
		'Created'
	end
where Status is null

-- Script to update column property
Go
ALTER TABLE Plan_Campaign ALTER COLUMN Status nvarchar(50) not null


-- Script to update old data
Go
update Plan_Campaign_Program  set [Status] = 
	case when 
			(select count(*) from Plan_Campaign_Program_Tactic as a where IsDeleted = 0 and a.PlanProgramId = PlanProgramId and a.[Status] != 'Submitted') = 0	
		then 'Submitted'
		when 
			(select count(*) from Plan_Campaign_Program_Tactic as a where IsDeleted = 0 and a.PlanProgramId = PlanProgramId and a.[Status] != 'In-Progress' and a.[status] != 'Approved' and a.[Status] != 'Complete') = 0	
		then 'Approved'
		when 
			(select count(*) from Plan_Campaign_Program_Tactic as a where IsDeleted = 0 and a.PlanProgramId = PlanProgramId and a.[Status] != 'Declined') = 0	
		then 'Declined'
	else 
		'Created'
	end
where Status is null

-- Script to update column property
Go
ALTER TABLE Plan_Campaign_Program ALTER COLUMN Status nvarchar(50) not null


-- Alter stored procedure
GO

--Stored Procedure
ALTER PROCEDURE [dbo].[PlanDuplicate]    
    (    
      @PlanId INT,    
	  @PlanStatus VARCHAR(255),    
      @TacticStatus VARCHAR(255),    
      @CreatedDate DateTime,    
	  @CreatedBy UNIQUEIDENTIFIER,   
	  @Suffix VARCHAR(50),
	  @CopyClone VARCHAR(50),
	  @Id INT,
      @ReturnValue INT = 0 OUT              
    )    
AS     
BEGIN

  SET NOCOUNT ON    
	     
	DECLARE @Title varchar(255), @clientId UNIQUEIDENTIFIER;
	DECLARE @newId int
	DECLARE @IsCount int
	----Declaring mapping table to hold old and new value
	DECLARE @TempPlanCampaign TABLE (OldPlanCampaignId int, NewPlanCampaignId int);
	DECLARE @TempPlanProgram TABLE (OldPlanProgramId int, NewPlanProgramId int);
	BEGIN TRANSACTION PlanDuplicate  
	if(@CopyClone = 'Plan') -- 4
	BEGIN
		----Getting plan title and client id to check whether copy plan title already exist for client of current plan.
		SELECT @Title = [PLAN].Title + @Suffix, @clientId = ClientId FROM [PLAN] 
		INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
		INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
		WHERE [PLAN].PlanId=@PlanId
		
			IF EXISTS(SELECT 1 FROM [PLAN] 
			INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
			INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
			WHERE ClientId=@clientId AND  [PLAN].Title = @Title)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
			----Inserting Plan.
				INSERT INTO [dbo].[Plan] 
		           ([ModelId]
				   ,[Title]
				   ,[Version]
				   ,[Description]
				   ,[MQLs]
				   ,[Budget]
				   ,[Status]
				   ,[IsActive]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
				   ,[ModifiedBy]
				   ,[Year])
				SELECT   [ModelId]
						,@Title
						,[Version]
						,[Description]
						,[MQLs]
						,[Budget]
						,@PlanStatus
						,[IsActive]
						,[IsDeleted]
						,@CreatedDate
						,@CreatedBy
						,null
						,null
						,[Year]
					 from [PLAN] WHERE PlanId = @PlanId

			  	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            

			----Getting id of new Plan.
			SET @newId = SCOPE_IDENTITY();  
			SET @IsCount = 4;
			END
	
	END
	ELSE if(@CopyClone = 'Campaign') -- 3
	BEGIN
		----Getting campaign title to check whether copy campaign title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign WHERE PlanCampaignId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign 
			WHERE Title = @Title AND PlanId = @PlanId)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				----Inserting Campaign.
				INSERT INTO [dbo].[Plan_Campaign]
					([PlanId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign
					WHERE PlanCampaignId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY();
					SET @IsCount = 3;
					INSERT INTO @TempPlanCampaign Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Program') -- 2
	BEGIN
		----Getting Program title to check whether copy Program title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program WHERE PlanProgramId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program]
				   ([PlanCampaignId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanCampaignId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign_Program
					WHERE PlanProgramId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 2;
					INSERT INTO @TempPlanProgram Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Tactic') -- 1
	BEGIN
		----Getting Tactic title to check whether copy Tactic title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program_Tactic.PlanProgramId = Plan_Campaign_Program.PlanProgramId 
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program_Tactic.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program_Tactic.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program_Tactic]
					   ([PlanProgramId]
					   ,[TacticTypeId]
					   ,[Title]
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,[BusinessUnitId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,[INQsActual]
					   ,[MQLsActual]
					   ,[CWs]
					   ,[CWsActual]
					   ,[Revenues]
					   ,[RevenuesActual]
					   ,[Cost]
					   ,[CostActual]
					   ,[ROI]
					   ,[ROIActual]
					   ,[Status]
					   ,[IsDeleted]
					   ,[CreatedDate]
					   ,[CreatedBy]
					   ,[ModifiedDate]
					   ,[ModifiedBy])
					SELECT
						PlanProgramId
						,TacticTypeId
					   ,@Title
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,BusinessUnitId
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,NULL
					   ,NULL
					   ,[CWs]
					   ,NULL
					   ,[Revenues]
					   ,NULL
					   ,[Cost]
					   ,NULL
					   ,[ROI]
					   ,NULL
					   ,@TacticStatus
					   ,[IsDeleted]
					   ,@CreatedDate
						,@CreatedBy
					   ,NULL
						,NULL
						FROM [Plan_Campaign_Program_Tactic]
						WHERE PlanTacticId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 1;
			END
	END
	-- Check Duplicate for Plan, Campaign, Program or Tactic and insert child table entry
	IF(@IsCount >= 4)
	BEGIN
	----Inserting Campaign.
		----Inserting campaign for new plan and populating mapping table with old and new id for campaign
		;WITH [sourceCampaign] AS (
						SELECT
								PlanCampaignId
							   ,@newId AS [PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
						FROM Plan_Campaign WHERE PlanId=@PlanId AND IsDeleted = 0)
		MERGE Plan_Campaign AS TargetCampaign
		USING sourceCampaign
		ON 0 = 1
		WHEN NOT MATCHED THEN
						INSERT  ([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,[CreatedDate]
							   ,[CreatedBy]
							   ,[ModifiedDate]
							   ,[ModifiedBy])
					  VALUES	([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,@CreatedDate
							   ,@CreatedBy
							   ,NULL
							   ,NULL)
		OUTPUT
							  sourceCampaign.PlanCampaignId,
							  inserted.PlanCampaignId
		INTO @TempPlanCampaign (OldPlanCampaignId, NewPlanCampaignId);
	
			  IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 3)
	BEGIN
	----Inserting Program.
		----Inserting program for new plan's campaign and populating mapping table with old and new id for program
		;WITH SourcePlanProgram AS (
								SELECT  Plan_Campaign_Program.PlanProgramId
									   ,mappingPlanCampaign.NewPlanCampaignId AS PlanCampaignId
									   ,[Title]
									   ,[Description]
									   ,[VerticalId]
									   ,[AudienceId]
									   ,[GeographyId]
									   ,[StartDate]
									   ,[EndDate]
									   ,[INQs]
									   ,[Cost]
								FROM Plan_Campaign_Program
								INNER JOIN @TempPlanCampaign mappingPlanCampaign ON Plan_Campaign_Program.PlanCampaignId = mappingPlanCampaign.OldPlanCampaignId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program AS TargetPlanCampaign
		USING SourcePlanProgram
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanCampaignId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[Cost]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy]
								   ,[Status])
						  VALUES   ([PlanCampaignId]
									,[Title]
									,[Description]
									,[VerticalId]
									,[AudienceId]
									,[GeographyId]
									,[StartDate]
									,[EndDate]
									,[INQs]
									,[Cost]
									,@CreatedDate
									,@CreatedBy
									,NULL
									,NULL
									,@tacticStatus)
		OUTPUT
							  SourcePlanProgram.PlanProgramId,
							  inserted.PlanProgramId
		INTO @TempPlanProgram (OldPlanProgramId, NewPlanProgramId);

			  IF @@ERROR <> 0    
 
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 2)
	BEGIN
	
	----Inserting Tactic
		----No need to declaring mapping table to hold old and new value. Just inserting tactic for new plan's program
		;WITH SourcePlanTactic AS (
								SELECT  Plan_Campaign_Program_Tactic.PlanTacticId
				  						,mappingPlanProgram.NewPlanProgramId AS PlanProgramId
										,[TacticTypeId]
										,[Title]
										,[Description]
										,[VerticalId]
										,[AudienceId]
										,[GeographyId]
										,[BusinessUnitId]
										,[StartDate]
										,[EndDate]
										,[INQs]
										,[CWs]
										,[Revenues]
										,[RevenuesActual]
										,[Cost]
										,[ROI]
								FROM Plan_Campaign_Program_Tactic
								INNER JOIN @TempPlanProgram mappingPlanProgram ON Plan_Campaign_Program_Tactic.PlanProgramId = mappingPlanProgram.OldPlanProgramId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program_Tactic AS TargetPlanTactic
		USING SourcePlanTactic
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanProgramId]
								   ,[TacticTypeId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[BusinessUnitId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[INQsActual]
								   ,[MQLsActual]
								   ,[CWs]
								   ,[CWsActual]
								   ,[Revenues]
								   ,[RevenuesActual]
								   ,[Cost]
								   ,[CostActual]
								   ,[ROI]
								   ,[ROIActual]
								   ,[Status]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy])
						  VALUES ([PlanProgramId]
								  ,[TacticTypeId]
								  ,[Title]
								  ,[Description]
								  ,[VerticalId]
								  ,[AudienceId]
								  ,[GeographyId]
								  ,[BusinessUnitId]
								  ,[StartDate]
								  ,[EndDate]
								  ,[INQs]
								  ,NULL
								  ,NULL
								  ,[CWs]
								  ,NULL
								  ,[Revenues]
								  ,NULL
								  ,[Cost]
								  ,NULL
								  ,[ROI]
								  ,NULL
								  ,@TacticStatus
								  ,@CreatedDate
								  ,@CreatedBy
								  ,NULL
								  ,NULL);

			IF @@ERROR <> 0     
            BEGIN                
                SET @ReturnValue = 0 -- 0 ERROR                
                ROLLBACK TRANSACTION PlanDuplicate                    
                RETURN                
            END            
	END

	COMMIT TRANSACTION PlanDuplicate                  
    SET @ReturnValue = @newId;        
    RETURN @ReturnValue      
END

--01_PL_204_Copying a Plan lose All collaborators.sql
-- Run below script on MRPQA

GO

--Stored Procedure
ALTER PROCEDURE [dbo].[PlanDuplicate]    
    (    
      @PlanId INT,    
	  @PlanStatus VARCHAR(255),    
      @TacticStatus VARCHAR(255),    
      @CreatedDate DateTime,    
	  @CreatedBy UNIQUEIDENTIFIER,   
	  @Suffix VARCHAR(50),
	  @CopyClone VARCHAR(50),
	  @Id INT,
      @ReturnValue INT = 0 OUT              
    )    
AS     
BEGIN

  SET NOCOUNT ON    
	     
	DECLARE @Title varchar(255), @clientId UNIQUEIDENTIFIER;
	DECLARE @newId int
	DECLARE @IsCount int
	----Declaring mapping table to hold old and new value
	DECLARE @TempPlanCampaign TABLE (OldPlanCampaignId int, NewPlanCampaignId int);
	DECLARE @TempPlanProgram TABLE (OldPlanProgramId int, NewPlanProgramId int);
	BEGIN TRANSACTION PlanDuplicate  
	if(@CopyClone = 'Plan') -- 4
	BEGIN
		----Getting plan title and client id to check whether copy plan title already exist for client of current plan.
		SELECT @Title = [PLAN].Title + @Suffix, @clientId = ClientId FROM [PLAN] 
		INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
		INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
		WHERE [PLAN].PlanId=@PlanId
		
			IF EXISTS(SELECT 1 FROM [PLAN] 
			INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
			INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
			WHERE ClientId=@clientId AND  [PLAN].Title = @Title)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
			----Inserting Plan.
				INSERT INTO [dbo].[Plan] 
		           ([ModelId]
				   ,[Title]
				   ,[Version]
				   ,[Description]
				   ,[MQLs]
				   ,[Budget]
				   ,[Status]
				   ,[IsActive]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
				   ,[ModifiedBy]
				   ,[Year])
				SELECT   [ModelId]
						,@Title
						,[Version]
						,[Description]
						,[MQLs]
						,[Budget]
						,@PlanStatus
						,[IsActive]
						,[IsDeleted]
						,@CreatedDate
						,@CreatedBy
						,null
						,null
						,[Year]
					 from [PLAN] WHERE PlanId = @PlanId

			  	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            

			----Getting id of new Plan.
			SET @newId = SCOPE_IDENTITY();  
			SET @IsCount = 4;
			END
	
	END
	ELSE if(@CopyClone = 'Campaign') -- 3
	BEGIN
		----Getting campaign title to check whether copy campaign title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign WHERE PlanCampaignId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign 
			WHERE Title = @Title AND PlanId = @PlanId)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				----Inserting Campaign.
				INSERT INTO [dbo].[Plan_Campaign]
					([PlanId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign
					WHERE PlanCampaignId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY();
					SET @IsCount = 3;
					INSERT INTO @TempPlanCampaign Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Program') -- 2
	BEGIN
		----Getting Program title to check whether copy Program title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program WHERE PlanProgramId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program]
				   ([PlanCampaignId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanCampaignId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign_Program
					WHERE PlanProgramId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 2;
					INSERT INTO @TempPlanProgram Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Tactic') -- 1
	BEGIN
		----Getting Tactic title to check whether copy Tactic title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program_Tactic.PlanProgramId = Plan_Campaign_Program.PlanProgramId 
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program_Tactic.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program_Tactic.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program_Tactic]
					   ([PlanProgramId]
					   ,[TacticTypeId]
					   ,[Title]
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,[BusinessUnitId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,[INQsActual]
					   ,[MQLsActual]
					   ,[CWs]
					   ,[CWsActual]
					   ,[Revenues]
					   ,[RevenuesActual]
					   ,[Cost]
					   ,[CostActual]
					   ,[ROI]
					   ,[ROIActual]
					   ,[Status]
					   ,[IsDeleted]
					   ,[CreatedDate]
					   ,[CreatedBy]
					   ,[ModifiedDate]
					   ,[ModifiedBy])
					SELECT
						PlanProgramId
						,TacticTypeId
					   ,@Title
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,BusinessUnitId
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,NULL
					   ,NULL
					   ,[CWs]
					   ,NULL
					   ,[Revenues]
					   ,NULL
					   ,[Cost]
					   ,NULL
					   ,[ROI]
					   ,NULL
					   ,@TacticStatus
					   ,[IsDeleted]
					   ,@CreatedDate
						,@CreatedBy
					   ,NULL
						,NULL
						FROM [Plan_Campaign_Program_Tactic]
						WHERE PlanTacticId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 1;
			END
	END
	-- Check Duplicate for Plan, Campaign, Program or Tactic and insert child table entry
	IF(@IsCount >= 4)
	BEGIN
	----Inserting Campaign.
		----Inserting campaign for new plan and populating mapping table with old and new id for campaign
		;WITH [sourceCampaign] AS (
						SELECT
								PlanCampaignId
							   ,@newId AS [PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
						FROM Plan_Campaign WHERE PlanId=@PlanId AND IsDeleted = 0)
		MERGE Plan_Campaign AS TargetCampaign
		USING sourceCampaign
		ON 0 = 1
		WHEN NOT MATCHED THEN
						INSERT  ([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,[CreatedDate]
							   ,[CreatedBy]
							   ,[ModifiedDate]
							   ,[ModifiedBy]
							   ,[Status])
					  VALUES	([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,@CreatedDate
							   ,@CreatedBy
							   ,NULL
							   ,NULL
							   ,@tacticStatus)
		OUTPUT
							  sourceCampaign.PlanCampaignId,
							  inserted.PlanCampaignId
		INTO @TempPlanCampaign (OldPlanCampaignId, NewPlanCampaignId);
	
			  IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 3)
	BEGIN
	----Inserting Program.
		----Inserting program for new plan's campaign and populating mapping table with old and new id for program
		;WITH SourcePlanProgram AS (
								SELECT  Plan_Campaign_Program.PlanProgramId
									   ,mappingPlanCampaign.NewPlanCampaignId AS PlanCampaignId
									   ,[Title]
									   ,[Description]
									   ,[VerticalId]
									   ,[AudienceId]
									   ,[GeographyId]
									   ,[StartDate]
									   ,[EndDate]
									   ,[INQs]
									   ,[Cost]
								FROM Plan_Campaign_Program
								INNER JOIN @TempPlanCampaign mappingPlanCampaign ON Plan_Campaign_Program.PlanCampaignId = mappingPlanCampaign.OldPlanCampaignId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program AS TargetPlanCampaign
		USING SourcePlanProgram
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanCampaignId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[Cost]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy]
								   ,[Status])
						  VALUES   ([PlanCampaignId]
									,[Title]
									,[Description]
									,[VerticalId]
									,[AudienceId]
									,[GeographyId]
									,[StartDate]
									,[EndDate]
									,[INQs]
									,[Cost]
									,@CreatedDate
									,@CreatedBy
									,NULL
									,NULL
									,@tacticStatus)
		OUTPUT
							  SourcePlanProgram.PlanProgramId,
							  inserted.PlanProgramId
		INTO @TempPlanProgram (OldPlanProgramId, NewPlanProgramId);

			  IF @@ERROR <> 0    
 
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 2)
	BEGIN
	
	----Inserting Tactic
		----No need to declaring mapping table to hold old and new value. Just inserting tactic for new plan's program
		;WITH SourcePlanTactic AS (
								SELECT  Plan_Campaign_Program_Tactic.PlanTacticId
				  						,mappingPlanProgram.NewPlanProgramId AS PlanProgramId
										,[TacticTypeId]
										,[Title]
										,[Description]
										,[VerticalId]
										,[AudienceId]
										,[GeographyId]
										,[BusinessUnitId]
										,[StartDate]
										,[EndDate]
										,[INQs]
										,[CWs]
										,[Revenues]
										,[RevenuesActual]
										,[Cost]
										,[ROI]
								FROM Plan_Campaign_Program_Tactic
								INNER JOIN @TempPlanProgram mappingPlanProgram ON Plan_Campaign_Program_Tactic.PlanProgramId = mappingPlanProgram.OldPlanProgramId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program_Tactic AS TargetPlanTactic
		USING SourcePlanTactic
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanProgramId]
								   ,[TacticTypeId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[BusinessUnitId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[INQsActual]
								   ,[MQLsActual]
								   ,[CWs]
								   ,[CWsActual]
								   ,[Revenues]
								   ,[RevenuesActual]
								   ,[Cost]
								   ,[CostActual]
								   ,[ROI]
								   ,[ROIActual]
								   ,[Status]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy])
						  VALUES ([PlanProgramId]
								  ,[TacticTypeId]
								  ,[Title]
								  ,[Description]
								  ,[VerticalId]
								  ,[AudienceId]
								  ,[GeographyId]
								  ,[BusinessUnitId]
								  ,[StartDate]
								  ,[EndDate]
								  ,[INQs]
								  ,NULL
								  ,NULL
								  ,[CWs]
								  ,NULL
								  ,[Revenues]
								  ,NULL
								  ,[Cost]
								  ,NULL
								  ,[ROI]
								  ,NULL
								  ,@TacticStatus
								  ,@CreatedDate
								  ,@CreatedBy
								  ,NULL
								  ,NULL);

			IF @@ERROR <> 0     
            BEGIN                
                SET @ReturnValue = 0 -- 0 ERROR                
                ROLLBACK TRANSACTION PlanDuplicate                    
                RETURN                
            END            
	END

	COMMIT TRANSACTION PlanDuplicate                  
    SET @ReturnValue = @newId;        
    RETURN @ReturnValue      
END

--01_PL_204_Copying a Plan lose All collaborators.sql
-- Run below script on MRPQA

GO

--Stored Procedure
ALTER PROCEDURE [dbo].[PlanDuplicate]    
    (    
      @PlanId INT,    
	  @PlanStatus VARCHAR(255),    
      @TacticStatus VARCHAR(255),    
      @CreatedDate DateTime,    
	  @CreatedBy UNIQUEIDENTIFIER,   
	  @Suffix VARCHAR(50),
	  @CopyClone VARCHAR(50),
	  @Id INT,
      @ReturnValue INT = 0 OUT              
    )    
AS     
BEGIN

  SET NOCOUNT ON    
	     
	DECLARE @Title varchar(255), @clientId UNIQUEIDENTIFIER;
	DECLARE @newId int
	DECLARE @IsCount int
	----Declaring mapping table to hold old and new value
	DECLARE @TempPlanCampaign TABLE (OldPlanCampaignId int, NewPlanCampaignId int);
	DECLARE @TempPlanProgram TABLE (OldPlanProgramId int, NewPlanProgramId int);
	BEGIN TRANSACTION PlanDuplicate  
	if(@CopyClone = 'Plan') -- 4
	BEGIN
		----Getting plan title and client id to check whether copy plan title already exist for client of current plan.
		SELECT @Title = [PLAN].Title + @Suffix, @clientId = ClientId FROM [PLAN] 
		INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
		INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
		WHERE [PLAN].PlanId=@PlanId
		
			IF EXISTS(SELECT 1 FROM [PLAN] 
			INNER JOIN MODEL ON [PLAN].ModelId = [Model].ModelId
			INNER JOIN BusinessUnit ON [Model].BusinessUnitId = [BusinessUnit].BusinessUnitId
			WHERE ClientId=@clientId AND  [PLAN].Title = @Title)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
			----Inserting Plan.
				INSERT INTO [dbo].[Plan] 
		           ([ModelId]
				   ,[Title]
				   ,[Version]
				   ,[Description]
				   ,[MQLs]
				   ,[Budget]
				   ,[Status]
				   ,[IsActive]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
				   ,[ModifiedBy]
				   ,[Year])
				SELECT   [ModelId]
						,@Title
						,[Version]
						,[Description]
						,[MQLs]
						,[Budget]
						,@PlanStatus
						,[IsActive]
						,[IsDeleted]
						,@CreatedDate
						,@CreatedBy
						,null
						,null
						,[Year]
					 from [PLAN] WHERE PlanId = @PlanId

			  	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            

			----Getting id of new Plan.
			SET @newId = SCOPE_IDENTITY();  
			SET @IsCount = 4;
			END
	
	END
	ELSE if(@CopyClone = 'Campaign') -- 3
	BEGIN
		----Getting campaign title to check whether copy campaign title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign WHERE PlanCampaignId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign 
			WHERE Title = @Title AND PlanId = @PlanId)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				----Inserting Campaign.
				INSERT INTO [dbo].[Plan_Campaign]
					([PlanId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign
					WHERE PlanCampaignId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY();
					SET @IsCount = 3;
					INSERT INTO @TempPlanCampaign Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Program') -- 2
	BEGIN
		----Getting Program title to check whether copy Program title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program WHERE PlanProgramId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program]
				   ([PlanCampaignId]
				   ,[Title]
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,[Status]
				   ,[IsDeleted]
				   ,[CreatedDate]
				   ,[CreatedBy]
				   ,[ModifiedDate]
					,[ModifiedBy])
					SELECT
					[PlanCampaignId]
				   ,@Title
				   ,[Description]
				   ,[VerticalId]
				   ,[AudienceId]
				   ,[GeographyId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[INQs]
				   ,[Cost]
				   ,@TacticStatus
				   ,[IsDeleted]
				   ,@CreatedDate
				   ,@CreatedBy
				   ,NULL
				   ,NULL
					FROM Plan_Campaign_Program
					WHERE PlanProgramId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 2;
					INSERT INTO @TempPlanProgram Values (@Id, @newId)
			END
	END
	ELSE if(@CopyClone = 'Tactic') -- 1
	BEGIN
		----Getting Tactic title to check whether copy Tactic title already exist for current plan.
			SELECT @Title = Title + @Suffix FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @Id
			IF EXISTS(SELECT 1 FROM Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program_Tactic.PlanProgramId = Plan_Campaign_Program.PlanProgramId 
			INNER JOIN Plan_Campaign ON Plan_Campaign_Program.PlanCampaignId = Plan_Campaign.PlanCampaignId 
			WHERE Plan_Campaign_Program_Tactic.Title = @Title AND PlanId = @PlanId AND Plan_Campaign_Program_Tactic.IsDeleted = 0)
			BEGIN
				SET @ReturnValue = 0
				Return
			END
			ELSE
			BEGIN
				INSERT INTO [dbo].[Plan_Campaign_Program_Tactic]
					   ([PlanProgramId]
					   ,[TacticTypeId]
					   ,[Title]
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,[BusinessUnitId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,[INQsActual]
					   ,[MQLsActual]
					   ,[CWs]
					   ,[CWsActual]
					   ,[Revenues]
					   ,[RevenuesActual]
					   ,[Cost]
					   ,[CostActual]
					   ,[ROI]
					   ,[ROIActual]
					   ,[Status]
					   ,[IsDeleted]
					   ,[CreatedDate]
					   ,[CreatedBy]
					   ,[ModifiedDate]
					   ,[ModifiedBy])
					SELECT
						PlanProgramId
						,TacticTypeId
					   ,@Title
					   ,[Description]
					   ,[VerticalId]
					   ,[AudienceId]
					   ,[GeographyId]
					   ,BusinessUnitId
					   ,[StartDate]
					   ,[EndDate]
					   ,[INQs]
					   ,NULL
					   ,NULL
					   ,[CWs]
					   ,NULL
					   ,[Revenues]
					   ,NULL
					   ,[Cost]
					   ,NULL
					   ,[ROI]
					   ,NULL
					   ,@TacticStatus
					   ,[IsDeleted]
					   ,@CreatedDate
					   ,@CreatedBy
					   ,NULL
						,NULL
						FROM [Plan_Campaign_Program_Tactic]
						WHERE PlanTacticId = @Id

					IF @@ERROR <> 0     
					BEGIN                
						SET @ReturnValue = 0 -- 0 ERROR                
						ROLLBACK TRANSACTION PlanDuplicate                    
						RETURN                
					END  

					----Getting id of new Plan.
					SET @newId = SCOPE_IDENTITY(); 
					SET @IsCount = 1;
			END
	END
	-- Check Duplicate for Plan, Campaign, Program or Tactic and insert child table entry
	IF(@IsCount >= 4)
	BEGIN
	----Inserting Campaign.
		----Inserting campaign for new plan and populating mapping table with old and new id for campaign
		;WITH [sourceCampaign] AS (
						SELECT
								PlanCampaignId
							   ,@newId AS [PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,[createdBy]
						FROM Plan_Campaign WHERE PlanId=@PlanId AND IsDeleted = 0)
		MERGE Plan_Campaign AS TargetCampaign
		USING sourceCampaign
		ON 0 = 1
		WHEN NOT MATCHED THEN
						INSERT  ([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,[CreatedDate]
							   ,[CreatedBy]
							   ,[ModifiedDate]
							   ,[ModifiedBy]
							   ,[Status])
					  VALUES	([PlanId]
							   ,[Title]
							   ,[Description]
							   ,[VerticalId]
							   ,[AudienceId]
							   ,[GeographyId]
							   ,[StartDate]
							   ,[EndDate]
							   ,[INQs]
							   ,[Cost]
							   ,@CreatedDate
							   ,[createdBy]
							   ,NULL
							   ,NULL
							   ,@tacticStatus)
		OUTPUT
							  sourceCampaign.PlanCampaignId,
							  inserted.PlanCampaignId
		INTO @TempPlanCampaign (OldPlanCampaignId, NewPlanCampaignId);
	
			  IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 3)
	BEGIN
	----Inserting Program.
		----Inserting program for new plan's campaign and populating mapping table with old and new id for program
		;WITH SourcePlanProgram AS (
								SELECT  Plan_Campaign_Program.PlanProgramId
									   ,mappingPlanCampaign.NewPlanCampaignId AS PlanCampaignId
									   ,[Title]
									   ,[Description]
									   ,[VerticalId]
									   ,[AudienceId]
									   ,[GeographyId]
									   ,[StartDate]
									   ,[EndDate]
									   ,[INQs]
									   ,[Cost]
									   ,[createdBy]
								FROM Plan_Campaign_Program
								INNER JOIN @TempPlanCampaign mappingPlanCampaign ON Plan_Campaign_Program.PlanCampaignId = mappingPlanCampaign.OldPlanCampaignId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program AS TargetPlanCampaign
		USING SourcePlanProgram
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanCampaignId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[Cost]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy]
								   ,[Status])
						  VALUES   ([PlanCampaignId]
									,[Title]
									,[Description]
									,[VerticalId]
									,[AudienceId]
									,[GeographyId]
									,[StartDate]
									,[EndDate]
									,[INQs]
									,[Cost]
									,@CreatedDate
									,[createdBy]
									,NULL
									,NULL
									,@tacticStatus)
		OUTPUT
							  SourcePlanProgram.PlanProgramId,
							  inserted.PlanProgramId
		INTO @TempPlanProgram (OldPlanProgramId, NewPlanProgramId);

			  IF @@ERROR <> 0    
 
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION PlanDuplicate                    
					RETURN                
				END            
	END
	IF(@IsCount >= 2)
	BEGIN
	
	----Inserting Tactic
		----No need to declaring mapping table to hold old and new value. Just inserting tactic for new plan's program
		;WITH SourcePlanTactic AS (
								SELECT  Plan_Campaign_Program_Tactic.PlanTacticId
				  						,mappingPlanProgram.NewPlanProgramId AS PlanProgramId
										,[TacticTypeId]
										,[Title]
										,[Description]
										,[VerticalId]
										,[AudienceId]
										,[GeographyId]
										,[BusinessUnitId]
										,[StartDate]
										,[EndDate]
										,[INQs]
										,[CWs]
										,[Revenues]
										,[RevenuesActual]
										,[Cost]
										,[ROI]
										,[createdBy]
								FROM Plan_Campaign_Program_Tactic
								INNER JOIN @TempPlanProgram mappingPlanProgram ON Plan_Campaign_Program_Tactic.PlanProgramId = mappingPlanProgram.OldPlanProgramId AND IsDeleted = 0)
		MERGE Plan_Campaign_Program_Tactic AS TargetPlanTactic
		USING SourcePlanTactic
		ON 0 = 1
		WHEN NOT MATCHED THEN
							INSERT ([PlanProgramId]
								   ,[TacticTypeId]
								   ,[Title]
								   ,[Description]
								   ,[VerticalId]
								   ,[AudienceId]
								   ,[GeographyId]
								   ,[BusinessUnitId]
								   ,[StartDate]
								   ,[EndDate]
								   ,[INQs]
								   ,[INQsActual]
								   ,[MQLsActual]
								   ,[CWs]
								   ,[CWsActual]
								   ,[Revenues]
								   ,[RevenuesActual]
								   ,[Cost]
								   ,[CostActual]
								   ,[ROI]
								   ,[ROIActual]
								   ,[Status]
								   ,[CreatedDate]
								   ,[CreatedBy]
								   ,[ModifiedDate]
								   ,[ModifiedBy])
						  VALUES ([PlanProgramId]
								  ,[TacticTypeId]
								  ,[Title]
								  ,[Description]
								  ,[VerticalId]
								  ,[AudienceId]
								  ,[GeographyId]
								  ,[BusinessUnitId]
								  ,[StartDate]
								  ,[EndDate]
								  ,[INQs]
								  ,NULL
								  ,NULL
								  ,[CWs]
								  ,NULL
								  ,[Revenues]
								  ,NULL
								  ,[Cost]
								  ,NULL
								  ,[ROI]
								  ,NULL
								  ,@TacticStatus
								  ,@CreatedDate
								  ,[createdBy]
								  ,NULL
								  ,NULL);

			IF @@ERROR <> 0     
            BEGIN                
                SET @ReturnValue = 0 -- 0 ERROR                
                ROLLBACK TRANSACTION PlanDuplicate                    
                RETURN                
            END            
	END

	COMMIT TRANSACTION PlanDuplicate                  
    SET @ReturnValue = @newId;        
    RETURN @ReturnValue      
END






