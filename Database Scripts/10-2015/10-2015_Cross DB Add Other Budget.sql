GO 
DECLARE @BDSAUTHDBName nvarchar(50) = 'BDSAuthDev'
DECLARE @MRPDBName nvarchar(50) = 'MRPDev'

DECLARE @SQLString nvarchar(max)=''

SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.Budget ([ClientId], [Name], [CreatedDate], [CreatedBy],[IsOther])
SELECT C.ClientId,''Other'',GETDATE(),C.CreatedBy,1 FROM ' + @BDSAUTHDBName + '.dbo.Client C
WHERE C.ClientId NOT IN (SELECT ClientId FROM ' + @MRPDBName + '.dbo.Budget WHERE IsOther = 1)'

Exec(@SQLString)
SET @SQLString = ''
SET @SQLString = @SQLString + N'INSERT INTO ' + @MRPDBName + '.dbo.Budget_Detail ([BudgetId],[Name],[CreatedDate],[CreatedBy])
SELECT MB.Id,''Other'',GETDATE(),MB.CreatedBy FROM ' + @MRPDBName + '.dbo.Budget MB WHERE IsOther = 1 AND id NOT IN 
(SELECT BudgetId FROM ' + @MRPDBName + '.dbo.Budget_Detail)'

Exec(@SQLString)

GO