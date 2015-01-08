 ---Execute this script on BDSAuth database.
----Updated Release month.
BEGIN
	IF EXISTS(Select 1 from [dbo].[Application] Where [Code] = 'MRP')
	BEGIN
		UPDATE [dbo].[Application] set ReleaseVersion= '2015.January' Where [Code] = 'MRP'
	END
END
GO