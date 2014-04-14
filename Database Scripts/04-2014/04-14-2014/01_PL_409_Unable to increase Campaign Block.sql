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

