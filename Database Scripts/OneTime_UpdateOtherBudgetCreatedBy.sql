-- Start: Change below Auth & Plan DB name 

Declare @authDBName varchar(1000)='Hive9ProdAuth'	
Declare @planDBName varchar(1000)='Hive9ProdGP'

-- End: Change above Auth & Plan DB name 

Declare @query varchar(max)=''

-- Correct CreatedBy for existing client's Other budget.
SET @query = '
Declare @tblBudgetUserId Table(
	BudgetId int,
	ClientID int,
	UserId int
)

--Update B SET CreatedBy=cl.UserId
INSERT INTO @tblBudgetUserId(BudgetId,ClientID,UserId)
SELECT B.ID,cl.ClientId,cl.UserId
FROM ['+@planDBName+'].[dbo].[Budget] B
JOIN 
	(

		SELECT c.ID ClientId,MAX(u.ID) UserId
		FROM ['+@authDBName+'].[dbo].[Client] c
		JOIN ['+@authDBName+'].[dbo].[User] u on c.ClientId = u.ClientId 
		and u.IsDeleted=''0'' 
		and ManagerId is NUll	
		WHERE c.IsDeleted=''0''
		Group By c.ID
	) cl on B.ClientId = cl.ClientId 
WHere B.IsOther=''1''

-- Update BUdget table CreatedBy
Update B SET CreatedBy=cl.UserId
FROM ['+@planDBName+'].[dbo].[Budget] B
JOIN @tblBudgetUserId cl on B.Id = cl.BudgetId

-- Update BUdget_Detail table CreatedBy
Update BD SET CreatedBy=cl.UserId
FROM ['+@planDBName+'].[dbo].[Budget_Detail] BD
JOIN @tblBudgetUserId cl on BD.BudgetId = cl.BudgetId

-- Update BUdget_Permission table CreatedBy
Update BD SET CreatedBy=cl.UserId
FROM ['+@planDBName+'].[dbo].[Budget_Detail] BD
JOIN @tblBudgetUserId cl on BD.BudgetId = cl.BudgetId
JOIN ['+@planDBName+'].[dbo].[Budget_Permission] BDP on BD.Id = BDP.BudgetDetailId

'
EXEC(@query)