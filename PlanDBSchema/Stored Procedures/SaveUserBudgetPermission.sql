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

GO
