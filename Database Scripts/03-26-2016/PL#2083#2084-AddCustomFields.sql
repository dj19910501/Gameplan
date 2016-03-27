/* ------------- Start - Related to PL ticket #2083 & #2084 ------------- 
Created By: Brad Gray
Created Date: 03/26/2016
Description: Add Custom Fields as Read-Only
*/

/*  Change the ClientId and UserId as appropriate */
Declare @ClientId  uniqueidentifier = '55B19C77-3356-4B22-8FC2-8E026138BEC2';
Declare @UserId  uniqueidentifier = '962456D6-8868-4B66-88CC-883619C4E340';
Declare @Date  datetime = GETDATE();

if not exists (Select * from [dbo].[CustomField] where ClientId = @ClientId and Name = 'Creative Project Number')
begin
 insert into [dbo].[CustomField]
 (Name, CustomFieldTypeId, IsRequired, EntityType, ClientId, IsDeleted, CreatedDate, CreatedBy, ModifiedDate,ModifiedBy, IsDisplayForFilter,
 IsDefault, IsGet)
  values('Creative Project Number', 1, 0, 'Tactic', @ClientId, 0, 
	@Date, @UserId, null, null, 0, 0, 1)
end


if not exists (Select * from [dbo].[CustomField] where ClientId = @ClientId and Name = 'Creative Cost')
begin
 insert into [dbo].[CustomField]
 (Name, CustomFieldTypeId, IsRequired, EntityType, ClientId, IsDeleted, CreatedDate, CreatedBy, ModifiedDate,ModifiedBy, IsDisplayForFilter,
 IsDefault, IsGet)
  values('Creative Cost', 1, 0, 'Tactic', @ClientId, 0, 
	@Date, @UserId, null, null, 0, 0, 1)
end