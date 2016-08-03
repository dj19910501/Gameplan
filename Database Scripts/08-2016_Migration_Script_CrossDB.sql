
DECLARE @CustomQuery NVARCHAR(MAX) = NULL
DECLARE @AuthDBName NVARCHAR(255) = NULL -- Name Of Database from where ClientId have to fetch
DECLARE @DBName NVARCHAR(255) = NULL -- Name of Database in which Clientwise Dashboard permission need to set

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''+@DBName+'.[dbo].[Report_Intergration_Conf]') AND type in (N'U'))
BEGIN
	SET @CustomQuery='	
		DELETE FROM '+@DBName+'.dbo.Report_Intergration_Conf

		INSERT INTO '+@DBName+'.dbo.Report_Intergration_Conf
		SELECT DISTINCT ''Dashboard'',''id'',DashboardId, usr.ClientId FROM '+@DBName+'.dbo.User_Permission up
		INNER JOIN '+@AuthDBName+'.dbo.[User] usr ON (usr.UserId = up.UserId AND usr.IsDeleted = 0)
		WHERE DashboardId IS NOT NULL
	
	'
	exec(@CustomQuery)
END