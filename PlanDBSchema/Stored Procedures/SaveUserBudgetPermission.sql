-- Added by Komal Rawal
-- Added on :: 08-June-2016
-- Desc :: On creation of new item in Marketing budget give permission as per parent items.
DROP PROCEDURE [dbo].[SaveuserBudgetPermission]
GO
CREATE PROCEDURE [dbo].[SaveuserBudgetPermission]
@BudgetDetailId int  = 0,
@PermissionCode int = 0,
@CreatedBy uniqueidentifier 
AS
BEGIN
WITH CTE AS
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

Select * into #tempbudgetdata from CTE
option (maxrecursion 0)

select * from #tempbudgetdata where ParentId is not null

insert into Budget_Permission select Distinct UserId,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode from Budget_Permission where BudgetDetailId in (select ParentId from #tempbudgetdata)
UNION
select  @CreatedBy,@BudgetDetailId,GETDATE(),@CreatedBy,@PermissionCode from Budget_Permission 

Drop Table #tempbudgetdata
END

