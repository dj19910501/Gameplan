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
	,@UserId int = 0
	,@SelectedCustomField varchar(max)=''
AS
BEGIN

SET NOCOUNT ON;

	DECLARE @CustomFieldTypeText VARCHAR(20)= 'TextBox'
	DECLARE @CustomFieldTypeDropDown VARCHAR(20)= 'DropDownList'

	-- Variables for fnGetFilterEntityHierarchy we pass defualt values because no need to pass timeframe option on grid
	DECLARE @TimeFrame VARCHAR(20)='' 
	DECLARE @Isgrid BIT=1

	DECLARE @CustomFieldIds TABLE (
		Item BIGINT Primary Key
	)
	INSERT INTO @CustomFieldIds
		SELECT CAST(Item AS BIGINT) as Item FROM dbo.SplitString(@SelectedCustomField,',') 

	-- Get List of Custom fields which are textbox type
	SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,C.EntityType
			,C.AbbreviationForMulti
			,@CustomFieldTypeText As 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' FROM CustomFieldType CT
				WHERE CT.Name=@CustomFieldTypeText 
				AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			WHERE ClientId=@ClientId
					AND IsDeleted=0
			UNION ALL
		-- Get Custom fields which are dropdown type and get only that custom fields which have it's option of that custom field
		SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,C.EntityType
			,C.AbbreviationForMulti
			,@CustomFieldTypeDropDown AS 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (	SELECT CT.Name AS 'CustomFieldType' 
							FROM CustomFieldType CT
							WHERE CT.Name=@CustomFieldTypeDropDown 
								AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (	SELECT CP.CustomFieldId 
							FROM CustomFieldOption CP
							WHERE 
								C.CustomFieldId = CP.CustomFieldId
							GROUP BY CP.CustomFieldId
							HAVING COUNT(CP.CustomFieldOptionId)>0) CP
			WHERE ClientId=@ClientId
					AND IsDeleted=0
					
	-- Get list of Entity custom fields values
	SELECT 
		A.EntityId
		,MAX(A.EntityType) EntityType
		,A.CustomFieldId
		,MAX(A.Value) AS Value
		,MAX(A.UniqueId) AS UniqueId
		,MAX(A.Text) AS 'Text'
		FROM (
				SELECT CE.CustomFieldId
					,Hireachy.EntityId
					,CE.Value
					,Hireachy.EntityType
					,C.CustomFieldType	
					,Hireachy.EntityType +'_'+CAST(CE.EntityId AS VARCHAR) AS 'UniqueId'
					,(  CASE WHEN C.IsRequired=1
							 THEN 
								CE.RestrictedText
							 ELSE 
								CE.UnRestrictedText
						END ) AS 'Text'
				FROM dbo.fnGetFilterEntityHierarchy(@PlanId,@OwnerIds,@TacticTypeIds,@StatusIds,@TimeFrame,@Isgrid) Hireachy 
				CROSS APPLY (SELECT C.CustomFieldId
									,C.EntityType
									,CT.CustomFieldType
									,C.IsRequired FROM CustomField C
							CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
											WHERE selCol.Item = C.CustomFieldId) selCol
							 CROSS APPLY(	SELECT Name AS 'CustomFieldType' 
											FROM CustomFieldType CT
											WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId) CT
							 WHERE Hireachy.EntityType = C.EntityType AND C.ClientId = @ClientId
									AND C.IsDeleted=0) C
				CROSS APPLY(SELECT   CE.EntityId
										,CE.CustomFieldId
										,CE.Value
										,CE.UnrestrictedText
										,RE.[Text] AS RestrictedText 
								FROM [MV].CustomFieldData CE
								LEFT JOIN [MV].[CustomFieldEntityRestrictedTextByUser] RE 
										ON RE.CustomFieldId = CE.CustomFieldId AND RE.UserId = @UserId
								WHERE C.CustomFieldId = CE.CustomFieldId
									AND Hireachy.EntityId = CE.EntityId ) CE
				) A
	GROUP BY A.CustomFieldId, A.EntityId

END
GO

