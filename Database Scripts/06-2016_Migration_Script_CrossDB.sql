
DECLARE @CustomQuery NVARCHAR(MAX) = NULL
DECLARE @AuthDBName NVARCHAR(255) = NULL -- Name Of Database from where User's List have to fetch
DECLARE @DBName NVARCHAR(255) = NULL -- Name of Database in which User_Permission need to set
DECLARE @ClientId NVARCHAR(MAX) = NULL -- List of ClientId in Comma Separated  -- e.g. '''C251AB18-0683-4D1D-9F1E-06709D59FD53'',''B14648DA-6A91-43E1-8FE2-0997C9180F55''' In this format

SET @CustomQuery='	
	DELETE FROM '+@DBName+'.dbo.User_Permission WHERE UserId IN (SELECT UA.UserId
	FROM '+@AuthDBName+'.dbo.User_Application UA
	INNER JOIN '+@AuthDBName+'.dbo.[Application] App ON (App.ApplicationId = UA.ApplicationId and App.Code = ''MRP'' AND App.IsDeleted = 0)
	INNER JOIN '+@AuthDBName+'.dbo.[User] Usr ON (Usr.UserId = UA.UserId AND Usr.IsDeleted = 0)
	WHERE UA.IsDeleted = 0
	AND Usr.ClientId IN ('+@ClientId+'))

	DECLARE @intFlag INT
	DECLARE @TotalRow INT
	DECLARE @TempTable AS TABLE (id INT, DashboardId INT)
	INSERT INTO @TempTable 
	SELECT ROW_NUMBER() OVER (ORDER BY id) AS RowNo, id FROM '+@DBName+'.dbo.Dashboard WHERE IsDeleted = 0

	SET @intFlag = 1
	SET @TotalRow = (SELECT COUNT(*) FROM @TempTable)

	WHILE (@intFlag <= @TotalRow)
	BEGIN
		DECLARE @DashboardId INT	

		SET @DashboardId = (SELECT DashboardId FROM @TempTable WHERE id = @intFlag)

		INSERT INTO '+@DBName+'.dbo.User_Permission
		SELECT @DashboardId, NULL, UA.UserId, GETDATE(), ''14D7D588-CF4D-46BE-B4ED-A74063B67D66'', ''View'', NULL
		FROM '+@AuthDBName+'.dbo.User_Application UA
		INNER JOIN '+@AuthDBName+'.dbo.[Application] App ON (App.ApplicationId = UA.ApplicationId AND App.Code = ''MRP'' AND App.IsDeleted = 0)
		INNER JOIN '+@AuthDBName+'.dbo.[User] Usr ON (Usr.UserId = UA.UserId AND Usr.IsDeleted = 0)
		WHERE UA.IsDeleted = 0
		AND Usr.ClientId IN ('+@ClientId+')
	
		SET @intFlag = @intFlag + 1

END
'
exec(@CustomQuery)
