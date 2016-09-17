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
	,@ClientId uniqueidentifier =''
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
					AND EntityType NOT IN('Budget','MediaCode')

	SELECT Hireachy.EntityId,Hireachy.EntityType,C.CustomFieldId,C.CustomFieldEntityId,C.Value
		 FROM dbo.fnGetEntitieHirarchyByPlanId(@PlanId) Hireachy 
			CROSS APPLY (SELECT C.CustomFieldId
								,C.EntityType
								,CE.CustomFieldEntityId
								,Ce.Value
							 FROM CustomField C
							 CROSS APPLY(SELECT CE.CustomFieldEntityId
												,CE.CustomFieldId
												,CE.Value FROM CustomField_Entity CE
											WHERE C.CustomFieldId = CE.CustomFieldId
													AND Hireachy.EntityId = CE.EntityId
										)CE
								WHERE C.ClientId=@ClientId
									AND C.IsDeleted=0
									AND C.EntityType NOT IN('Budget','MediaCode')
									AND C.EntityType = Hireachy.EntityType) C

END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridData] AS' 
END
