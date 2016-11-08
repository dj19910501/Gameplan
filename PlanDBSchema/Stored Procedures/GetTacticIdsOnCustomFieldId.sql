IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticIdsOnCustomField]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTacticIdsOnCustomField]
GO

CREATE PROCEDURE GetTacticIdsOnCustomField
@ClientId INT = 0
,@UserId INT = 0
,@IsDefaultCustomRestrictionsViewable BIT = 1
AS
BEGIN
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1
	DECLARE @PlanId NVARCHAR(MAX) = ''
	DECLARE @OwnerIds NVARCHAR(MAX) = ''
	DECLARE @TacticTypeIds varchar(max)=''
	DECLARE @StatusIds varchar(max)=''
	DECLARE @ViewBy varchar(max)='Tactic'

	CREATE TABLE  #FilterData (
	PlanId NVARCHAR(MAX) NULL,
	OwnerIds NVARCHAR(MAX) NULL,
	StatusIds NVARCHAR(MAX) NULL,
	TacticTypeIds NVARCHAR(MAX) NULL,
	customFields NVARCHAR(MAX) NULL
	)
	-- Get Filters data
	INSERT INTO  #FilterData EXEC GetGridFilters @UserId,@ClientId,@IsDefaultCustomRestrictionsViewable
	
	-- Set Filter data 
	SELECT @PlanId=PlanId,@OwnerIds=OwnerIds,@StatusIds=StatusIds,@TacticTypeIds=TacticTypeIds FROM #FilterData

	DECLARE @DropDownTypeCustomFieldid INT = 0 
	
	SELECT @DropDownTypeCustomFieldid  = CustomFieldTypeId FROM CustomFieldType WHERE Name ='DropDownList'

	SELECT CE.* FROM [dbo].fnViewByEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@ViewBy,@TimeFrame,@Isgrid) Hireachy 
	 INNER JOIN CustomField_Entity CE WITH(NOLOCK) ON Hireachy.EntityId = CE.EntityId
	 INNER JOIN CustomField C WITH(NOLOCK) ON C.CustomFieldId = CE.CustomFieldId
	 WHERE
	 Hireachy.EntityType='Tactic'
	 AND C.ClientId=@ClientId
	 AND C.CustomFieldTypeId = @DropDownTypeCustomFieldid
	 AND C.IsDeleted=0
END