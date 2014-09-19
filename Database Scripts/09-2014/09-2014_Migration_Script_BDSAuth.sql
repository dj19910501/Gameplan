----01_PL_712_Edit_Own_And_Subordinate_Plan.sql
Begin
	-- Run below script on BDSAuth

	Declare @Application_ID UNIQUEIDENTIFIER
	Set @Application_ID = (Select ApplicationId from [Application] where Code ='MRP')

	IF ((SELECT COUNT(*) FROM [Application_Activity] where [Code]='PlanEditOwnAndSubordinates' and ApplicationId = @Application_ID) > 0)
	BEGIN

		UPDATE [Application_Activity] SET ActivityTitle = 'Edit Subordinate Plan',[Code] = 'PlanEditSubordinates'
		where [Code] = 'PlanEditOwnAndSubordinates' and ApplicationId = @Application_ID

	END
END
GO