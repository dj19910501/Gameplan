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

	-- Get user restricted values
	DECLARE @UserRestrictedValues TABLE (CustomFieldId INT,Text NVARCHAR(MAX),Value NVARCHAR(MAX))
	;WITH CommaSaperated AS (
	SELECT CR.CustomFieldId, CFO.Value,CFO.CustomFieldOptionId FROM CustomRestriction CR
	INNER JOIN CustomFieldOption CFO ON CFO.CustomFieldId = CR.CustomFieldId AND CFO.CustomFieldOptionId = CR.CustomFieldOptionId
	WHERE CR.Userid=@UserId AND CR.Permission IN (2)
	)
	INSERT INTO @UserRestrictedValues
	SELECT CustomFieldId,
	Text = STUFF(
		(SELECT ',' + Value FROM CommaSaperated C WHERE C.CustomFieldId = P.CustomFieldId FOR XML PATH ('') ), 1, 1, ''
	) 
	,Value = STUFF(
		(SELECT ',' + CAST(CustomFieldOptionId AS NVARCHAR(MAX)) FROM CommaSaperated C WHERE C.CustomFieldId = P.CustomFieldId FOR XML PATH ('') ), 1, 1, ''
	) 
	FROM CommaSaperated P GROUP BY CustomFieldId

	-- Get List of Custom fields which are textbox type
	SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,ET.EntityTypeId AS EntityType 
			,C.AbbreviationForMulti
			,@CustomFieldTypeText As 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' FROM CustomFieldType CT
				WHERE CT.Name=@CustomFieldTypeText 
				AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET 
			WHERE ClientId=@ClientId
					AND IsDeleted=0
			UNION ALL
		-- Get Custom fields which are dropdown type and get only that custom fields which have it's option of that custom field
		SELECT C.CustomFieldId
			,C.Name AS 'CustomFieldName' 
			,C.CustomFieldTypeId 
			,C.IsRequired
			,ET.EntityTypeId AS EntityType 
			,C.AbbreviationForMulti
			,@CustomFieldTypeDropDown AS 'CustomFieldType'
			,'custom_'+CAST (C.CustomFieldId AS VARCHAR(100))+':'+C.EntityType AS CustomUniqueId
			FROM CustomField  C
			CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
						WHERE selCol.Item = C.CustomFieldId) selCol
			CROSS APPLY (SELECT CT.Name AS 'CustomFieldType' 
							FROM CustomFieldType CT
							WHERE CT.Name=@CustomFieldTypeDropDown 
								AND CT.CustomFieldTypeId = C.CustomFieldTypeId)CT
			CROSS APPLY (SELECT CP.CustomFieldId 
							FROM CustomFieldOption CP
							WHERE 
								C.CustomFieldId = CP.CustomFieldId
							GROUP BY CP.CustomFieldId
							HAVING COUNT(CP.CustomFieldOptionId)>0) CP
			CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET
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
		,MAX(A.RestrictedText) AS 'RestrictedText'
		,MAX(A.RestrictedValue) AS 'RestrictedValue'
		FROM (
				SELECT CE.CustomFieldId
					,Hireachy.EntityId
					,CE.Value
					,Hireachy.EntityTypeId AS EntityType
					,C.CustomFieldType	
					,Hireachy.EntityType +'_'+CAST(CE.EntityId AS VARCHAR) AS 'UniqueId'
					,CE.UnrestrictedText AS 'Text'
					,RV.Text AS 'RestrictedText'
					,RV.Value AS 'RestrictedValue'
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
								FROM [MV].CustomFieldData CE
								WHERE C.CustomFieldId = CE.CustomFieldId
									AND Hireachy.EntityId = CE.EntityId ) CE
				OUTER APPLY (SELECT * FROM @UserRestrictedValues RV WHERE C.CustomFieldId = RV.CustomFieldId) RV
							UNION ALL 
							SELECT C.CustomFieldId,NULL,NULL,ET.EntityTypeId,CT.CustomFieldType
							,NULL AS 'UniqueId',NULL
							,NULL AS 'RestrictedText'
							,NULL AS 'RestrictedValue' FROM CustomField C 
							CROSS APPLY (SELECT * FROM EntityType WHERE Name = C.EntityType) ET  
							CROSS APPLY (SELECT Item FROM @CustomFieldIds selCol 
											WHERE selCol.Item = C.CustomFieldId) selCol
									CROSS APPLY(SELECT Name AS 'CustomFieldType' FROM CustomFieldType CT 
											WHERE C.CustomFieldTypeId = CT.CustomFieldTypeId)CT 
									WHERE C.ClientId = @ClientId 
									AND C.IsDeleted = 0 
									AND C.EntityType IN('Campaign','Program','Tactic','Lineitem') 
				) A
	GROUP BY A.CustomFieldId, A.EntityId
							
			

END
GO


