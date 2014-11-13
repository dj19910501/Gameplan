---Execute this script on BDSAuth database.
IF EXISTS(Select 1 from [dbo].[Application] Where [Code] = 'MRP')
BEGIN
	UPDATE [dbo].[Application] set ReleaseVersion= '2014.November' Where [Code] = 'MRP'
END
GO