/* To be executed on BDS Auth database */

IF EXISTS (SELECT * FROM [Application] WHERE Code = 'MRP')
BEGIN
	UPDATE [Application] SET Name ='Bulldog Gameplan', [Description] = 'Bulldog Gameplan'
	WHERE Code = 'MRP'
END

/* To be executed on BDS Auth database */

