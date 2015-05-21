Go
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldOption')
 begin
 IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] =  N'IsDeleted'  AND [object_id] = OBJECT_ID(N'CustomFieldOption'))
 begin
 ALTER TABLE CustomFieldOption
ADD IsDeleted bit  NOT NULL 
CONSTRAINT Delete_Option DEFAULT 0
end
end