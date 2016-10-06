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

	DECLARE @CustomFieldTypeText VARCHAR(20)= 'TextBox'
	DECLARE @CustomFieldTypeDropDown VARCHAR(20)= 'DropDownList'

	-- Variables for fnGetFilterEntityHierarchy we pass defualt values because no need to pass timeframe option on grid
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1

	-- Get List of Custom fields which are textbox type
	SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,C.EntityType
			,C.AbbreviationForMulti
			,@CustomFieldTypeText As 'CustomFieldType'
			FROM CustomField  C
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' FROM CustomFieldType CT
				WHERE CT.Name=@CustomFieldTypeText 
				AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			WHERE ClientId=@ClientId
					AND IsDeleted=0
					AND EntityType IN('Campaign','Program','Tactic','Lineitem')
		UNION ALL
		-- Get Custom fields which are dropdown type and get only that custom fields which have it's option of that custom field
		SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,C.EntityType
			,C.AbbreviationForMulti
			,@CustomFieldTypeDropDown AS 'CustomFieldType'
			FROM CustomField  C
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' FROM CustomFieldType CT
				WHERE CT.Name=@CustomFieldTypeDropDown 
				AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (SELECT CP.CustomFieldId FROM CustomFieldOption CP
							WHERE 
							C.CustomFieldId = CP.CustomFieldId
							GROUP BY CP.CustomFieldId
							HAVING COUNT(CP.CustomFieldOptionId)>0) CP
			WHERE ClientId=@ClientId
					AND IsDeleted=0
					AND EntityType IN('Campaign','Program','Tactic','Lineitem')

	-- Get list of Entity custom fields values
	SELECT 
	A.EntityId
	,MAX(A.EntityType) EntityType
	,A.CustomFieldId
	,MAX(A.Value) AS Value
	,MAX(A.UniqueId) AS UniqueId
	,MAX(A.Text) AS 'Text'
	FROM (
	SELECT CE.CustomFieldId,Hireachy.EntityId
	,(SELECT SUBSTRING((	
						SELECT ',' + CAST(R.Value AS VARCHAR) FROM CustomField_Entity R
						WHERE R.EntityId = CE.EntityId
						AND R.CustomFieldId = CE.CustomFieldId
						FOR XML PATH('')), 2,900000
						)) AS Value,
						Hireachy.EntityType
						,C.CustomFieldType	
						,Hireachy.EntityType +'_'+CAST(CE.EntityId AS VARCHAR) AS 'UniqueId'
						,(SELECT SUBSTRING((	
							SELECT ',' + 
							CASE WHEN C.CustomFieldType = @CustomFieldTypeText
								THEN R.Value
							ELSE 
								CAST(CCP.Value AS VARCHAR(50)) 
							END
							FROM CustomField_Entity R
							LEFT JOIN CustomFieldOption CCP ON R.Value = CAST(CCP.CustomFieldOptionId AS varchar(50))
							WHERE R.EntityId = CE.EntityId
							AND R.CustomFieldId = CE.CustomFieldId
							AND CE.CustomFieldId = C.CustomFieldId
							--AND R.Value = CAST(CP.CustomFieldOptionId AS varchar(50))
							FOR XML PATH('')), 2,900000
						)) AS 'Text'
					--FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
					FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@TimeFrame,@Isgrid) Hireachy 
					CROSS APPLY (SELECT C.CustomFieldId
										,C.EntityType
										,CT.CustomFieldType FROM CustomField C
							CROSS APPLY(SELECT Name AS 'CustomFieldType' FROM CustomFieldType CT
								WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId)CT
							WHERE Hireachy.EntityType = C.EntityType AND C.ClientId = @ClientId
						AND C.IsDeleted=0) C
					CROSS APPLY(SELECT CE.EntityId,CE.CustomFieldId FROM CustomField_Entity CE
						WHERE C.CustomFieldId = CE.CustomFieldId
						AND Hireachy.EntityId = CE.EntityId)CE
					UNION ALL
					SELECT C.CustomFieldId,NULL,NULL,NULL,CT.CustomFieldType,NULL,NULL FROM CustomField C
					CROSS APPLY(SELECT Name AS 'CustomFieldType' FROM CustomFieldType CT
						WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId)CT
						WHERE C.ClientId = @ClientId
					AND C.IsDeleted = 0
					AND C.EntityType IN('Campaign','Program','Tactic','Lineitem')
		) A
		GROUP BY A.CustomFieldId,A.EntityId

END


