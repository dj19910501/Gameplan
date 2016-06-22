IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'GetHelpText') AND TYPE IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[GetHelpText]
END
GO

CREATE PROCEDURE [dbo].[GetHelpText]
	-- Add the parameters for the stored procedure here
	@ReportDashboardID int,
    @GraphType varchar(50)
AS
BEGIN
	if(@GraphType = 'Widget' OR @GraphType = 'KeydataWidget')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join DashboardContents dc on (ht.Id = dc.HelpTextId and dc.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'Keydata')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join keydata kd on (ht.Id = kd.HelpTextId and kd.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'Dashboard')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join dashboard db on (ht.Id = db.HelpTextId and db.id = @ReportDashboardID)
	End
	Else if(@GraphType = 'HomeDashboard')
	Begin
		select ht.[Description] AS 'Description' from helptext ht 
		inner join Homepage db on (ht.Id = db.HelpTextId and db.id = @ReportDashboardID)
	End
END

