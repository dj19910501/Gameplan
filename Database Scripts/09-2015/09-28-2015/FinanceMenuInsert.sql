IF (NOT EXISTS(SELECT * FROM [Menu_Application] WHERE code = 'FINANCE')) 
BEGIN 
    INSERT INTO [Menu_Application](ApplicationId,Code,Name,IsDisplayInMenu,SortOrder,ControllerName,ActionName,CreatedDate,CreatedBy,IsDeleted) 
    VALUES('1C10D4B9-7931-4A7C-99E9-A158CE158951','FINANCE','Finance',1,4,'Finance','Index',GETDATE(),'F37A855C-9BF4-4A1F-AB7F-B21AF43EB2AF',0) 

	UPDATE [Menu_Application] 
    SET SortOrder = 5
	WHERE Code = 'BOOST' 
	AND ApplicationId ='1C10D4B9-7931-4A7C-99E9-A158CE158951'
	AND IsDeleted = 0

	UPDATE [Menu_Application] 
    SET SortOrder = 6
	WHERE Code = 'Report' 
	AND ApplicationId ='1C10D4B9-7931-4A7C-99E9-A158CE158951'
	AND IsDeleted = 0
END 

 



