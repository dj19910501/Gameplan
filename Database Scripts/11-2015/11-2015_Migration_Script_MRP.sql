-- Created By : Dashrath Prajapati
-- Created Date : 11/24/2015
-- Description :Insert BudgetDetail Id into BudgetPermission table related to PL #1679
-- ======================================================================================
GO
IF NOT EXISTS(SELECT id FROM [dbo].[Budget_Detail] WHERE Id NOT IN  (SELECT BudgetDetailId 
     FROM [dbo].[Budget_Permission]))
begin
	INSERT INTO [dbo].[Budget_Permission] ([UserId],[BudgetDetailId],[CreatedDate],[CreatedBy],[PermisssionCode])
Select [CreatedBy],[Id],GETDATE(),[CreatedBy],0 From [dbo].[Budget_Detail] Where Id NOT IN(
Select BudgetDetailId From [dbo].[Budget_Permission]
)
end
GO
