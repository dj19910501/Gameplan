-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/18/2015
-- Description:	create index for CustomField_Entity table
-- =============================================
If NOT EXISTS (SELECT 1 
FROM sys.indexes 
WHERE name='CustomFieldID_Index' AND object_id = OBJECT_ID('CustomField_Entity'))

Begin
CREATE NONCLUSTERED INDEX [CustomFieldID_Index] ON [dbo].[CustomField_Entity]
(
	[CustomFieldId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

END


If NOT EXISTS (SELECT 1 
FROM sys.indexes 
WHERE name='EntityID_Index' AND object_id = OBJECT_ID('CustomField_Entity'))

Begin
CREATE NONCLUSTERED INDEX [EntityID_Index] ON [dbo].[CustomField_Entity]
(
	[EntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

END