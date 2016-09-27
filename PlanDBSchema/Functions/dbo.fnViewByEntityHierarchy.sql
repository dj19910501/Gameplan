/****** Object:  UserDefinedFunction [dbo].[fnViewByEntityHierarchy]    Script Date: 09/27/2016 11:54:13 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnViewByEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnViewByEntityHierarchy]
GO
/****** Object:  UserDefinedFunction [dbo].[fnViewByEntityHierarchy]    Script Date: 09/27/2016 11:54:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnViewByEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Viral
-- Create date: 09/23/2016
-- Description:	Create Function to return data based on view by value.
-- =============================================
CREATE FUNCTION [dbo].[fnViewByEntityHierarchy]
(
	@planIds varchar(max),
	@ownerIds nvarchar(max),
	@tactictypeIds varchar(max),
	@statusIds varchar(max),
	@ViewBy varchar(500)
)
RETURNS 
@ResultEntities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT
		)
AS
BEGIN

	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''Tactic'')	
	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''Status'')
	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''Stage'')
	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''TacticCustom71'')
	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''ProgramCustom18'')
	--Select * from fnViewByEntityHierarchy(''20220'',''104'',''31104,31121'',''Created,Complete,Approved,Declined,Submitted,In-Progress'',''CampaignCustom3'')
	
	-- Declare Local variables
	BEGIN
			Declare @stage varchar(10)=''Stage''
			Declare @ROIPackage varchar(20)=''ROIPackage''
			Declare @Status varchar(20)=''Status''
			Declare @custom varchar(50)=''Custom''
			
			Declare @entType varchar(50)
			Declare @entTactic varchar(50)=''Tactic''
			Declare @entProgram varchar(50)=''Program''
			Declare @entCampaign varchar(50)=''Campaign''
			Declare @entPlan varchar(50)=''Plan''
			
			Declare @custmEntityTypeId int 
			Declare @custCampaign varchar(20)=''CampaignCustom''
			Declare @custProgram varchar(20)=''ProgramCustom''
			Declare @custTactic varchar(20)=''TacticCustom''
			Declare @isCustom bit=''0''
			
			Declare @ResultViewByHierarchyEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
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
						ViewByTitle		NVARCHAR(500)
					)
			
			
			Declare @vwEntities TABLE (
						UniqueId		NVARCHAR(30), 
						EntityId		BIGINT,
						EntityTitle		NVARCHAR(1000),
						ParentEntityId	BIGINT, 
						ParentUniqueId	NVARCHAR(30),
						EntityType		NVARCHAR(15), 
						ColorCode		NVARCHAR(7),
						[Status]		NVARCHAR(15), 
						StartDate		DATETIME, 
						EndDate			DATETIME, 
						CreatedBy		INT,
						AltId			NVARCHAR(500),
						TaskId			NVARCHAR(500),
						ParentTaskId	NVARCHAR(500),
						PlanId			BIGINT,
						ModelId			BIGINT
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
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId
				FROM		fnGetFilterEntityHierarchy (@planIds,@ownerIds,@tactictypeIds,@statusIds)
	
				RETURN
			END
			ELSE
			BEGIN
				-- GET Data with applying required filter and insert into local table to re use for further process.
				INSERT Into @vwEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)		
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId
				FROM		fnGetFilterEntityHierarchy (@planIds,@ownerIds,@tactictypeIds,@statusIds)
			END
	
	
			-- Insert distinct ViewBy values of Tactics.
			If(@ViewBy = @Status)
			BEGIN
				SET @entType = @entTactic	-- Prepared View by structure based on Tactic
	
				Insert Into @distViewByValues(ViewByTitle,ViewById) 
				Select Distinct [Status],[Status] from @vwEntities where EntityType=@entType
	
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct EntityId,[Status] from @vwEntities where EntityType=@entType
			END
			
			ELSE If(@ViewBy = @stage)
			BEGIN
				SET @entType = @entTactic	-- Prepared View by structure based on Tactic
			
			-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct S.Title,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T on H.EntityId = T.PlanTacticId and EntityType=@entType
				JOIN Stage as S on T.StageId = S.StageId and S.IsDeleted=''0''
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(S.StageId as varchar) from @vwEntities as H
				Join Plan_Campaign_Program_Tactic as T on H.EntityId = T.PlanTacticId and EntityType=@entType
				JOIN Stage as S on T.StageId = S.StageId and S.IsDeleted=''0''
	
			END
			
			ELSE If(@ViewBy = @ROIPackage)
			Begin
				SET @entType = @entTactic	-- Prepared View by structure based on Tactic
	
				-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
				Insert Into @distViewByValues(ViewByTitle,ViewById)
				Select Distinct H.EntityTitle,Cast(H.EntityId  as varchar(max)) FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI ON H.EntityId = ROI.AnchorTacticID
				WHERE H.EntityType=@entType
	
				-- Insert Entity and ViewBy value mapping records to local table for further process.
				Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
				Select Distinct H.EntityId,Cast(ROI.AnchorTacticID as varchar)  FROM @vwEntities as H
				JOIN ROI_PackageDetail as ROI ON H.EntityId = ROI.AnchorTacticID
				WHERE H.EntityType=@entType
			END
	
			ELSE 
			BEGIN
				
				-- Identify that view by custom value is Campaign type or not
				IF EXISTS(SELECT 1 WHERE @ViewBy like ''%''+@custCampaign+''%'')
				BEGIN
					SET @entType = @entCampaign
					SET @isCustom = ''1''
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custCampaign,'''')
				END
	
				-- Identify that view by custom value is Program type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like ''%''+@custProgram+''%'')
				BEGIN
					SET @entType = @entProgram
					SET @isCustom = ''1''
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custProgram,'''')
				END
				
				-- Identify that view by custom value is Tactic type or not
				ELSE IF EXISTS(SELECT 1 WHERE @ViewBy like ''%''+@custTactic+''%'')
				BEGIN
					SET @entType = @entTactic
					SET @isCustom = ''1''
					SET @custmEntityTypeId = REPLACE(@ViewBy,@custTactic,'''')
				END
	
				IF(@isCustom =''1'')	-- If View by selection is Customfield then 
				BEGIN
	
					-- Insert Distict ViewBy values to local table to show 1st as parent record into Calendar or Grid.
					Insert Into @distViewByValues(ViewByTitle,ViewById) 
					Select Distinct CFO.Value,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE on H.EntityId = CE.EntityId and H.EntityType=@entType and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted=''0''
					where H.EntityType=@entType
	
					-- Insert Entity and ViewBy value mapping records to local table for further process.
					Insert Into @tblEntityViewByMapping(EntityId,ViewByValue)
					Select Distinct H.EntityId,Cast(CFO.CustomFieldOptionId as varchar) from @vwEntities as H
					JOIN CustomField_Entity as CE on H.EntityId = CE.EntityId and H.EntityType=@entType and CE.CustomFieldId=@custmEntityTypeId
					JOIN CustomFieldOption as CFO on CE.CustomFieldId = CFO.CustomFieldId and CFO.CustomFieldOptionId=CE.Value and CFO.IsDeleted=''0''
					where H.EntityType=@entType
				END
			END
	
			
			-- Insert Distinct view by values to Result set.
			INSERT INTO @ResultViewByHierarchyEntities(
						EntityTitle,[EntityType],ViewByTitle,TaskId)
			SELECT		ViewByTitle,@ViewBy,ViewByTitle,''Z''+ViewById 
			FROM		@distViewByValues
	
			-- Insert Entity(based on value @entType set) for all ViewBy
			INSERT INTO @ResultViewByHierarchyEntities
			SELECT		H.UniqueId ,H.EntityId ,H.EntityTitle ,H.ParentEntityId ,H.ParentUniqueId ,H.EntityType ,H.ColorCode,H.[Status],H.StartDate,H.EndDate,H.CreatedBy,H.AltId			
						,''Z''+R.ViewByValue+''_''+H.TaskId		
						,''Z''+R.ViewByValue+''_''+H.ParentTaskId
						,H.PlanId ,H.ModelId ,R.ViewByValue
			FROM		@distViewByValues as DV
			JOIN		@tblEntityViewByMapping as R on DV.ViewById = R.ViewByValue
			JOIN		@vwEntities as H on R.EntityId = H.EntityId and H.EntityType=@entType
			
	
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
				JOIN @distViewByValues as V on R.ViewByTitle  = V.ViewById and R.EntityType=@entType
				Group By R.ViewByTitle,R.ParentUniqueId
	
				
				;WITH prnt AS 
				(
						
						(
							SELECT	H.UniqueId ,H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId			
									,''Z''+C.ViewByTitle+''_''+H.TaskId as TaskId
									,''Z''+C.ViewByTitle+''_''+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, C.ViewByTitle
							FROM @vwEntities H
							JOIN @prntEntityTable as C ON H.UniqueId = C.ParentUniqueId
						
						)
	
							UNION ALL 
	
						(
							-- Get recursive parents data based
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy ,H.AltId		
									,''Z''+P.ViewByTitle+''_''+H.TaskId as TaskId
									,''Z''+P.ViewByTitle+''_''+H.ParentTaskId as ParentTaskId
									,H.PlanId, H.ModelId, P.ViewByTitle
							FROM @vwEntities H
							JOIN prnt as P ON H.UniqueId = P.ParentUniqueId
							
						)
				
				)
				
	
				INSERT INTO @ResultViewByHierarchyEntities(
							UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle)
				SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle 
				FROM		prnt
			
			END
	
			-- Get Child hierarchy data
			BEGIN
				IF(@isCustom =''1'')	-- Identify that view by is Custom field or not
				BEGIN
	
					;WITH child AS 
					(
						(
							-- Get Parent records from @ResultViewByHierarchyEntities to create child hierarchy data.
							SELECT	UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle
							FROM	@ResultViewByHierarchyEntities 
							WHERE	EntityType=@entType
						)
							UNION ALL 
	
						(
							-- Get recursive child data based on above parents query
							SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
									Cast(''Z''+C.ViewByTitle+''_''+H.TaskId as nvarchar(500))  as TaskId,
									C.TaskId as ParentTaskId,
									H.PlanId, H.ModelId, C.ViewByTitle
							FROM	@vwEntities as H
							JOIN	child C on C.UniqueId = H.ParentUniqueId
						)
					)
					
					--select * from child
	
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle)
					SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle 
					FROM		child 
					WHERE EntityType <> @entType
				END
				ELSE
				BEGIN
					INSERT INTO @ResultViewByHierarchyEntities (
								UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId,ViewByTitle)
				
					SELECT	H.UniqueId, H.EntityId, H.EntityTitle, H.ParentEntityId, H.ParentUniqueId, H.EntityType, H.ColorCode, H.[Status], H.StartDate, H.EndDate, H.CreatedBy, H.AltId,
							''Z''+R.ViewByTitle+''_''+H.TaskId,
							R.TaskId, H.PlanId, H.ModelId, R.ViewByTitle
					FROM	@ResultViewByHierarchyEntities as R
					JOIN	@vwEntities H on R.UniqueId  = H.ParentUniqueId and R.EntityType=@entType
				END
			END
	
			-- Update Plan ParentTaskId value
			Update @ResultViewByHierarchyEntities set ParentTaskId = ''Z''+ViewByTitle 
			WHERE	EntityType=@entPlan
	
			-- Insert data to result set.
			Insert INTO @ResultEntities (
						UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)
			SELECT		UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,ColorCode,[Status],StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId
			FROM		@ResultViewByHierarchyEntities
	
	
		RETURN 
END
' 
END

GO
