--
-- This function returns onky allowed custom field options 
--
IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[MV].[fnCustomFieldEntityRestrictedTextByUser]'))
DROP FUNCTION [MV].[fnCustomFieldEntityRestrictedTextByUser];
GO 

CREATE FUNCTION [MV].[fnCustomFieldEntityRestrictedTextByUser] (@CustomFieldId INT, @UserId INT)
RETURNS VARCHAR(MAX)
AS
BEGIN
	RETURN SUBSTRING((  SELECT ','+MAX(CFO.Value)
						FROM CustomFieldOption CFO
								INNER JOIN CustomRestriction CR ON CR.CustomFieldId = CFO.CustomFieldId
									AND CR.CustomFieldOptionId = CFO.CustomFieldOptionId
						WHERE CR.Permission=2 
								AND @CustomFieldId = CR.CustomFieldId
						GROUP BY CFO.CustomFieldOptionId
						FOR XML PATH('')),2,90000)
END 
GO