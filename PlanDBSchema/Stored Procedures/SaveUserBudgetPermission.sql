-- Added by Komal Rawal
-- Added on :: 08-June-2016
-- Desc :: On creation of new item in Marketing budget give permission as per parent items.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveuserBudgetPermission]
GO
/****** Object:  StoredProcedure [dbo].[SaveuserBudgetPermission]    Script Date: 06/09/2016 15:30:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveuserBudgetPermission]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveuserBudgetPermission] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy INT 
AS
BEGIN

IF OBJECT_ID('tempdb..#tempbudgetdata') IS NOT NULL
Drop Table #tempbudgetdata

IF OBJECT_ID('tempdb..#AllUniqueBudgetdata') IS NOT NULL
Drop Table #AllUniqueBudgetdata

;WITH CTE AS
(
 
    SELECT ParentId 
    FROM Budget_Detail
    WHERE id= @BudgetDetailId
    UNION ALL
    --This is called multiple times until the condition is met
    SELECT g.ParentId 
    FROM CTE c, Budget_Detail g
    WHERE g.id= c.parentid

)


Select * into #tempbudgetdata from (select ParentId ,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as RN from CTE) as result
option (maxrecursion 0)

select * from #tempbudgetdata where ParentId is not null

IF OBJECT_ID('tempdb..#AllUniqueBudgetdata') IS NOT NULL
Drop Table #AllUniqueBudgetdata

--Get user data of all the parents
SELECT * INTO #AllUniqueBudgetdata FROM (
select Distinct BudgetDetailId, UserId,@BudgetDetailId as bid,GETDATE() as dt,@CreatedBy as Cby,Case WHEN UserId = @CreatedBy
THEN 
@PermissionCode
 ELSE
PermisssionCode END as percode,
Case WHEN UserId = @CreatedBy
THEN 
 1
 ELSE
 0 END as usrid
from Budget_Permission where BudgetDetailId in (select ParentId from #tempbudgetdata)) as data


-- Insert unique data of parents for new item,take users from upper level parent if user not present in immediate parent
insert into Budget_Permission
(
BudgetDetailId,
CreatedDate,
PermisssionCode,
IsOwner,
UserId,
CreatedBy
)
select Uniquedata.bid,GETDATE(),Uniquedata.percode,Uniquedata.usrid,Uniquedata.UserId,Uniquedata.Cby from #AllUniqueBudgetdata as Uniquedata
JOIN #tempbudgetdata as TempBudgetTable on Uniquedata.BudgetDetailId = TempBudgetTable.ParentId and UserId NOT IN 
(
select UserId 
FROM #AllUniqueBudgetdata as Alldata
JOIN #tempbudgetdata as parent on Alldata.BudgetDetailId = parent.ParentId and RN < TempBudgetTable.RN 
)
UNION
select  @BudgetDetailId,GETDATE(),@PermissionCode,1,@CreatedBy,@CreatedBy from Budget_Permission 


END
GO
