/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 10/17/2016 10:19:23 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetFilterEntityHierarchy]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 10/17/2016 10:19:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[fnGetFilterEntityHierarchy]
(
	@planIds varchar(max)='''',
	@ownerIds nvarchar(max)='''',
	@tactictypeIds varchar(max)='''',
	@statusIds varchar(max)='''',
	@TimeFrame varchar(20)='''',
	@isGrid bit=0
)

RETURNS @Entities TABLE (
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
			EloquaId		nvarchar(100),
			MarketoId		nvarchar(100),
			WorkfrontID		nvarchar(100),
			SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN


Declare @entTactic varchar(8)=''Tactic''
Declare @entLineItem varchar(10)=''LineItem''

Declare @HierarchyEntities TABLE (
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
			EloquaId		nvarchar(100),
			MarketoId		nvarchar(100),
			WorkfrontID		nvarchar(100),
			SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)

INSERT INTO @HierarchyEntities 

SELECT 
UniqueId		
,EntityId		
,EntityTitle		
,ParentEntityId	
,ParentUniqueId	
,EntityType		
,ColorCode		
,[Status]		
,StartDate		
,EndDate			
,CreatedBy		
,AltId			
,TaskId			
,ParentTaskId	
,PlanId			
,ModelId
,EloquaId
,MarketoId
,WorkfrontID
,SalesforceId
,ROIPackageIds

FROM fnGetEntityHierarchyByPlanId(@planIds,@TimeFrame,@isGrid)

	-- Fill the table variable with the rows for your result set
	
	;WITH FilteredEnt AS(
Select * from @HierarchyEntities
)
,tac as (
	Select distinct ent.* 
	FROM FilteredEnt as ent
	Join [Plan_Campaign_Program_Tactic] as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType=@entTactic AND tac.[Status] IN (select val from comma_split(@statusIds,'','')) and  tac.[CreatedBy] IN (select case when val = '''' then null else Convert(int,val) end from comma_split(@ownerIds,'',''))
	Join [TacticType] as typ on tac.TacticTypeId = typ.TacticTypeId and typ.IsDeleted=''0'' and typ.[TacticTypeId] IN (select val from comma_split(@tactictypeIds,'',''))
	where ent.EntityType = @entTactic
)
,line as (
	SELECT ent.* 
	FROM FilteredEnt as ent
	JOIN tac on ent.ParentEntityId = tac.EntityId and ent.EntityType=@entLineItem

)

INSERT INTO @Entities
select * from FilteredEnt where EntityType not in (''Tactic'',''LineItem'')
union all
SELECT * FROM tac 
union all
select * from line

RETURN

END

' 
END

GO
