-- Created By : Kalpesh Sharma
-- 08/25/2014
-- Identify that Model has None LineItemType or not , if model has no None LineItemType at that time we have to insert into LineItemType table  

INSERT INTO LineItemType
SELECT ModelId
	,'None' --Model Title
	,'None' -- Description
	,0  -- Is Deleted
	,(Select GETDATE()) -- Created Date 
	,'14D7D588-CF4D-46BE-B4ED-A74063B67D66' --Created By
	,NULL -- Modified Date
	,NULL -- Modified By
FROM Model
WHERE ModelId NOT IN (
		SELECT DISTINCT LIT.ModelId
		FROM LineItemType LIT
		WHERE LIT.ModelId IN (
				SELECT ModelId
				FROM [dbo].[LineItemType]
				WHERE Title = 'None'
				)
		)
 Go