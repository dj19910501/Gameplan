
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetCustomFieldList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[sp_GetCustomFieldList] AS' 
END
GO

Alter PROCEDURE [dbo].[sp_GetCustomFieldList]
@ClientId INT 
AS
BEGIN
SET NOCOUNT ON;

Declare @CustomfieldType int
set @CustomfieldType=(select TOP 1 CustomFieldTypeId from CustomFieldType where Name='DropDownList')

SELECT distinct CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,case when  CustomField.EntityType='LineItem' then 'Line Item' else CustomField.EntityType end as EntityType,ISnull(CustomFieldDependency.ParentCustomFieldId,0) as ParentId ,CustomFieldType.Name as CustomFieldType,
case when  CustomField.EntityType='Campaign' then 1
when  CustomField.EntityType='Program' then 2
when  CustomField.EntityType='Tactic' then 3
when  CustomField.EntityType='LineItem' then 4
end as entityorder
FROM CustomField (NOLOCK) 
inner join CustomFieldType on CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId
LEFT join CustomFieldDependency on
CustomField.CustomFieldId= CustomFieldDependency.ChildCustomFieldId
Left join CustomFieldOption on CustomField.CustomFieldId = CustomFieldOption.CustomFieldId 
 WHERE 
CustomField.IsDeleted=0 and (CustomField.CustomFieldTypeId <> @CustomfieldType or (CustomFieldOptionId IS NOT NULL AND CustomField.CustomFieldTypeId=@CustomfieldType ))
and ClientId= CASE WHEN @ClientId IS NULL THEN ClientId ELSE @ClientId END and CustomField.EntityType in('Campaign','Program','Tactic','LineItem')
group by CustomField.CustomFieldId,CustomField.Name,CustomField.IsRequired,CustomField.EntityType,CustomFieldDependency.ParentCustomFieldId,CustomFieldType.Name
order by entityorder,CustomField.Name

END

--select * from CustomFieldType where CustomFieldId=29
-- EXEC sp_GetCustomFieldList '464eb808-ad1f-4481-9365-6aada15023bd'

GO


