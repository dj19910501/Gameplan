/****** Object:  UserDefinedFunction [dbo].[fnViewByEntityHierarchy]    Script Date: 10/18/2016 1:56:42 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnViewByEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnViewByEntityHierarchy]
GO
CREATE FUNCTION [dbo].[fnViewByEntityHierarchy]
(
	@planIds varchar(max),
	@ownerIds nvarchar(max),
	@tactictypeIds varchar(max),
	@statusIds varchar(max),
	@ViewBy varchar(500),
	@TimeFrame varchar(20)='',
	@isGrid bit=0
)
RETURNS 
@ResultEntities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			EntityTypeId	INT, 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT,
			--EloquaId		nvarchar(100),
			--MarketoId		nvarchar(100),
			--WorkfrontID		nvarchar(100),
			--SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN

	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Tactic')	
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Status','2016',0)
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','Stage')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','TacticCustom71')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','ProgramCustom18')
	--Select * from fnViewByEntityHierarchy('20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','CampaignCustom3')
	
	-- Declare Local variables
	BEGIN
			Declare @stage varchar(10)='Stage'
			Declare @ROIPackage varchar(20)='ROI Package'
			Declare @Status varchar(20)='Status'
			Declare @custom varchar(50)='Custom'
			
			Declare @entTactic varchar(50)='Tactic'
			Declare @entProgram varchar(50)='Program'
			Declare @entCampaign varchar(50)='Campaign'
			Declare @entPlan varchar(50)='Plan'
			
			Declare @entTypeId INT
			Declare @entTacticId INT = 4
			Declare @entProgramId INT = 3
			Declare @entCampaignId INT = 2
			Declare @entPlanId INT = 1
			
			Declare @custmEntityTypeId int 
			Declare @custCampaign varchar(20)='CampaignCustom'
			Declare @custProgram varchar(20)='ProgramCustom'
			Declare @custTactic varchar(20)='TacticCustom'
			Declare @isCustom bit='0'
			
			Declare @ResultViewByHierarchyEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
						EntityTypeId	INT, 
						ColorCode		NVARCHAR(7),
						[Status]		NVARCHAR(15), 
						StartDate		DATETIME, 
						EndDate			DATETIME, 
						CreatedBy		INT,
						AltId			NVARCHAR(500),
						TaskId			NVARCHAR(500),
						ParentTaskId	NVARCHAR(500),
						PlanId			BIGINT,
						ModelId			BIGINT,
						--EloquaId		nvarchar(100),
						--MarketoId		nvarchar(100),
						--WorkfrontID		nvarchar(100),
						--SalesforceId	nvarchar(100),
						ViewByTitle		NVARCHAR(500),
						ROIPackageIds	Varchar(max)
					)
			
			
			Declare @vwEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
						EntityTypeId	INT, 
						ColorCode		NVARCHAR(7),
						[Status]		NVARCHAR(15), 
						StartDate		DATETIME, 
						EndDate			DATETIME, 
						CreatedBy		INT,
						AltId			NVARCHAR(500),
						TaskId			NVARCHAR(500),
						ParentTaskId	NVARCHAR(500),
						PlanId			BIGINT,
						ModelId			BIGINT,
						--EloquaId		nvarchar(100),
						--MarketoId		nvarchar(100),
						--WorkfrontID		nvarchar(100),
						--SalesforceId	nvarchar(100),
						ROIPackageIds	Varchar(max)
					)
			
					Declare @distViewByValues Table(
					 ViewByTitle NVarchar(max),
					 ViewById Varchar(max)
					)
							
					Declare @tblEntityViewByMapping Table(
						EntityId bigint,
						ViewByValue Nvarchar(1000) 
					)
			
	END
			-- If Viewby is Tactic then return Filter result set table
			IF(@ViewBy = @entTactic)
			BEGIN
				
				INSERT Into @ResultEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
				FROM		fnGetFilterEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@TimeFrame,@isGrid)
	
				RETURN
			END
			ELSE
			BEGIN
				-- GET Data with applying required filter and insert into local table to re use for further process.
				INSERT Into @vwEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
				FROM		fnGetFilterEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@TimeFrame,@isGrid)
			END
	
	
			-- Insert distinct ViewBy values of Tactics.
			If(@ViewBy = @Status)
			BEGIN
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
	
				Insert Into @distViewByValues(ViewByTitle,ViewById) 
				Select Distinct [Status],[Status] from @vwEntities where EntityTypeId=@entTypeId
	
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct EntityId,[Status] from @vwEntities where EntityTypeId=@entTypeId
			END
			
			ELSE If(@ViewBy = @stage)
			BEGIN
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
			
			-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct S.Title,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T WITH(NOLOCK) on H.EntityId = T.PlanTacticId and EntityTypeId=@entTypeId
				JOIN Stage as S WITH(NOLOCK) on T.StageId = S.StageId and S.IsDeleted='0'
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T WITH(NOLOCK) on H.EntityId = T.PlanTacticId and EntityTypeId=@entTypeId
				JOIN Stage as S  WITH(NOLOCK) on T.StageId = S.StageId and S.IsDeleted='0'
	
			END
			
			ELSE If(@ViewBy = @ROIPackage)
			Begin
				SET @entTypeId = @entTacticId	-- Prepared View by structure based on Tactic
	
				-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct H.EntityTitle,Cast(H.EntityId  as varchar(max)) FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI WITH(NOLOCK) ON H.EntityId = ROI.AnchorTacticID
				WHERE H.EntityTypeId=@entTypeId
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(ROI.AnchorTacticID as varchar)  FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI WITH(NOLOCK) ON H.EntityId = ROI.PlanTacticId
				WHERE H.EntityTypeId=@entTypeId
			END
	
			ELSE 
			BEGIN
				
				-- Identify that view by custom value is Campaign type or not
				IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custCampaign+'%')
				BEGIN
					SET @entTypeId = @entCampaignId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custCampaign,'')
				END
	
				-- Identify that view by custom value is Program type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custProgram+'%')
				BEGIN
					SET @entTypeId = @entProgramId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custProgram,'')
				END
				
				-- Identify that view by custom value is Tactic type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like '%'+@custTactic+'%')
				BEGIN
					SET @entTypeId = @entTacticId
					SET @isCustom = '1'
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custTactic,'')
				END
	
				IF(@isCustom ='1')	-- If View by selection is Customfield then 
				BEGIN
	
					-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
					Insert Into @distViewByValues(ViewByTitle,ViewById) 
					Select Distinct CFO.Value,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE WITH(NOLOCK) on H.EntityId = CE.EntityId and H.EntityTypeId=@entTypeId and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO WITH(NOLOCK) on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted='0'
					where H.EntityTypeId=@entTypeId
	
					-- Insert Entity and ViewBy value mapping records to local table for further process.
					Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
					Select Distinct H.EntityId,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE WITH(NOLOCK) on H.EntityId = CE.EntityId and H.EntityTypeId=@entTypeId and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO WITH(NOLOCK) on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted='0'
					where H.EntityTypeId=@entTypeId
				END
			END
	
			
			-- Insert Distinct view by values to Result set.
			INSERT INTO @ResultViewByHierarchyEntities(
						EntityTitle,[EntityType],ViewByTitle,TaskId)
			SELECT		ViewByTitle,@ViewBy,ViewByTitle,'Z'+ViewById 
			FROM		@distViewByValues
	
			-- Insert Entity(based on value @entType set) for all ViewBy
			INSERT INTO @ResultViewByHierarchyEntities
			SELECT		H.UniqueId ,H.EntityId ,H.EntityTitle ,H.ParentEntityId ,H.ParentUniqueId ,H.EntityType, H.EntityTypeId,H.ColorCode,H.[Status],H.StartDate,H.EndDate,H.CreatedBy,H.AltId			
						,'Z'+R.ViewByValue+'_'+H.TaskId		
						,'Z'+R.ViewByValue+'_'+H.ParentTaskId
						,H.PlanId ,H.ModelId ,
						R.ViewByValue,H.ROIPackageIds
			FROM		@distViewByValues as DV
			JOIN		@tblEntityViewByMapping as R on DV.ViewById = R.ViewByValue
			JOIN		@vwEntities as H on R.EntityId = H.EntityId and H.EntityTypeId=@entTypeId
			
	
			-- Get Parent Hierarchy
			BEGIN
	
				-- Get Distinct Entity(based on value @entType set) ParentEntityId by ViewBy value
				Declare @prntEntityTable Table (
					ParentUniqueId NVARCHAR(500),
					ViewByTitle Nvarchar(max)
				)
	
				-- Create ParentEntity distinct Unique ids into local table to create parent hierarchy
				Insert Into @prntEntityTable(ParentUniqueId,ViewByTitle)
				Select Distinct R.ParentUniqueId,R.ViewByTitle
				FROM @ResultViewByHierarchyEntities as R
				JOIN @distViewByValues as V on R.ViewByTitle  = V.ViewById and R.EntityTypeId=@entTypeId
				Group By R.ViewByTitle,R.ParentUniqueId
	
				
				;WITH prnt AS 
				(
						
						(
							SELECT	H.UniqueId ,H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId			
									,'Z'+C.ViewByTitle+'_'+H.TaskId as TaskId
									,'Z'+C.ViewByTitle+'_'+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, C.ViewByTitle,
									H.ROIPackageIds
							FROM @vwEntities H
							JOIN @prntEntityTable as C ON H.UniqueId = C.ParentUniqueId
						
						)
	
							UNION ALL 
	
						(
							-- Get recursive parents data based
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy ,H.AltId		
									,'Z'+P.ViewByTitle+'_'+H.TaskId as TaskId
									,'Z'+P.ViewByTitle+'_'+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, P.ViewByTitle,
									H.ROIPackageIds
							FROM @vwEntities H
							JOIN prnt as P ON H.UniqueId = P.ParentUniqueId
							
						)
				
				)
				
	
				INSERT INTO @ResultViewByHierarchyEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds 
				FROM		prnt
			
			END
	
			-- Get Child hierarchy data
			BEGIN
				IF(@isCustom ='1')	-- Identify that view by is Custom field or not
				BEGIN
	
					;WITH child AS 
					(
						(
							-- Get Parent records from @ResultViewByHierarchyEntities to create child hierarchy data.
							SELECT	UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds
							FROM	@ResultViewByHierarchyEntities 
							WHERE	EntityTypeId=@entTypeId
						)
							UNION ALL 
	
						(
							-- Get recursive child data based on above parents query
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
									Cast('Z'+C.ViewByTitle+'_'+H.TaskId as nvarchar(500))  as TaskId,
									C.TaskId as ParentTaskId,
									H.PlanId, H.ModelId, C.ViewByTitle,
									H.ROIPackageIds
							FROM	@vwEntities as H
							JOIN	child C on C.UniqueId = H.ParentUniqueId
						)
					)
					
					--select * from child
	
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
					SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds 
					FROM		child 
					WHERE EntityTypeId <> @entTypeId
				END
				ELSE
				BEGIN
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle,ROIPackageIds)
				
					SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType,H.EntityTypeId, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
							'Z'+R.ViewByTitle+'_'+H.TaskId,
							R.TaskId, H.PlanId, H.ModelId, R.ViewByTitle,
							H.ROIPackageIds
					FROM	@ResultViewByHierarchyEntities as R
					JOIN	@vwEntities H on R.UniqueId  = H.ParentUniqueId and R.EntityTypeId=@entTypeId
				END
			END
	
			-- Update Unique & ParentUniqueId
			Update @ResultViewByHierarchyEntities SET UniqueId='Z'+ViewByTitle+'_'+UniqueId,ParentUniqueId='Z'+ViewByTitle+'_'+ParentUniqueId
			where EntityType <> @ViewBy


			-- Update Plan ParentUniqueId & ParentTaskID value
			Update @ResultViewByHierarchyEntities set ParentTaskId = 'Z'+ViewByTitle,ParentUniqueId = 'Z'+ViewByTitle 
			WHERE	EntityType=@entPlan 

			-- Update UniqueId value
			Update @ResultViewByHierarchyEntities set UniqueId = TaskId 
			WHERE  EntityType = @ViewBy
	
			-- Insert data to result set.
			Insert INTO @ResultEntities (
						UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)
			SELECT		Distinct UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,EntityTypeId,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds
			FROM		@ResultViewByHierarchyEntities
	
	
		RETURN 
END

GO
