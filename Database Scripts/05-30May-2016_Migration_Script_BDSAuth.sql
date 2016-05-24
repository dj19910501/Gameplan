/* Added by Arpita Soni for Ticket #2202 on 05/24/2016 */

IF NOT EXISTS(SELECT TOP 1 MenuApplicationId from Menu_Application where Code = 'PLANBUDGET')
BEGIN
	INSERT INTO Menu_Application VALUES(
	(SELECT TOP 1 MenuApplicationId from Menu_Application where Code = 'FINANCE'),
	(SELECT TOP 1 ApplicationId FROM APPLICATION WHERE Code='MRP'),
	'PLANBUDGET','Plan Budgeting',NULL,1,4,'Plan','Budgeting',
	GETDATE(),
	(SELECT TOP 1 UserId from [User_Application] WHERE ApplicationId=(SELECT TOP 1 ApplicationId FROM APPLICATION WHERE Code='MRP')),
	NULL,NULL,0)
END


IF NOT EXISTS(SELECT TOP 1 MenuApplicationId from Menu_Application where Code = 'ADVANCEDBUDGET')
BEGIN
	INSERT INTO Menu_Application VALUES(
	(SELECT TOP 1 MenuApplicationId from Menu_Application where Code = 'FINANCE'),
	(SELECT TOP 1 ApplicationId FROM APPLICATION WHERE Code='MRP'),
	'ADVANCEDBUDGET','Advanced Budgeting',NULL,1,4,'Finance','Index',
	GETDATE(),
	(SELECT TOP 1 UserId from [User_Application] WHERE ApplicationId=(SELECT TOP 1 ApplicationId FROM APPLICATION WHERE Code='MRP')),
	NULL,NULL,0)
END