DECLARE @MRPDB VARCHAR(50) = 'MRP'
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuth'

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomRestriction')
BEGIN

EXECUTE('INSERT INTO ' + @BDSAuth  +'.dbo.CustomRestriction (UserId,CustomField,CustomFieldId,Permission,CreatedDate,CreatedBy)
SELECT UserId,''Verticals'',CAST(VerticalId AS varchar(50)),2,GETDATE(),UserId FROM ' + @BDSAuth + '.dbo.[User]
CROSS JOIN ' + @MRPDB + '.dbo.[Vertical] 
WHERE '+ @BDSAuth + '.dbo.[User].IsDeleted = 0 AND ' + @MRPDB + '.dbo.[Vertical].IsDeleted = 0
AND '+ @BDSAuth + '.dbo.[User].ClientId = ' + @MRPDB + '.dbo.[Vertical].ClientId
AND UserId NOT IN (SELECT UserId FROM ' + @BDSAuth + '.dbo.CustomRestriction WHERE CustomField = ''Verticals'')')

EXECUTE('INSERT INTO ' + @BDSAuth  +'.dbo.CustomRestriction (UserId,CustomField,CustomFieldId,Permission,CreatedDate,CreatedBy)
SELECT UserId,''Geography'',CAST(' + @MRPDB + '.dbo.[Geography].GeographyId AS varchar(50)),2,GETDATE(),UserId FROM ' + @BDSAuth + '.dbo.[User]
CROSS JOIN ' + @MRPDB + '.dbo.[Geography] 
WHERE '+ @BDSAuth + '.dbo.[User].IsDeleted = 0 AND ' + @MRPDB + '.dbo.[Geography].IsDeleted = 0
AND '+ @BDSAuth + '.dbo.[User].ClientId = ' + @MRPDB + '.dbo.[Geography].ClientId
AND UserId NOT IN (SELECT UserId FROM ' + @BDSAuth + '.dbo.CustomRestriction WHERE CustomField = ''Geography'')')


END