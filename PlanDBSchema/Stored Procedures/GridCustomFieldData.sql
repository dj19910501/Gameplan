IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GridCustomFieldData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GridCustomFieldData] AS' 
END
GO
-- =============================================
-- Author:		Nishant Sheth
-- Create date: 16-Sep-2016
-- Description:	Get home grid customfields and it's values
-- =============================================
ALTER PROCEDURE [dbo].[GridCustomFieldData]
	@PlanId	 NVARCHAR(MAX)=''
	,@ClientId int = 0
	,@OwnerIds NVARCHAR(MAX) = ''
	,@TacticTypeIds varchar(max)=''
	,@StatusIds varchar(max)=''
AS
BEGIN

SET NOCOUNT ON;

	SELECT CustomFieldId
			,Name AS 'CustomFieldName' 
			,CustomFieldTypeId 
			,IsRequired
			,EntityType
			,AbbreviationForMulti
			FROM CustomField 
				WHERE ClientId=@ClientId
					AND IsDeleted=0
					AND EntityType IN('Campaign','Program','Tactic','Lineitem')

	SELECT 
	A.EntityId
	,MAX(A.EntityType) EntityType
	,A.CustomFieldId
	,MAX(A.Value) AS Value
	FROM (
	SELECT CE.CustomFieldId,Hireachy.EntityId
	,(SELECT SUBSTRING((	
						SELECT ',' + CAST(R.Value AS VARCHAR) FROM CustomField_Entity R
						WHERE R.EntityId = CE.EntityId
						AND R.CustomFieldId = CE.CustomFieldId
						FOR XML PATH('')), 2,900000
						)) AS Value,
						Hireachy.EntityType
					--FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
					FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds) Hireachy 
					CROSS APPLY (SELECT C.CustomFieldId FROM CustomField C
						WHERE Hireachy.EntityType = C.EntityType AND C.ClientId = @ClientId
						AND C.IsDeleted=0) C
					CROSS APPLY(SELECT CE.EntityId,CE.CustomFieldId FROM CustomField_Entity CE
						WHERE C.CustomFieldId = CE.CustomFieldId
						AND Hireachy.EntityId = CE.EntityId)CE
					UNION ALL
					SELECT C.CustomFieldId,NULL,NULL,NULL FROM CustomField C
					WHERE C.ClientId = @ClientId
					AND C.IsDeleted = 0
					AND C.EntityType IN('Campaign','Program','Tactic','Lineitem')
		) A
		GROUP BY A.CustomFieldId,A.EntityId

END
