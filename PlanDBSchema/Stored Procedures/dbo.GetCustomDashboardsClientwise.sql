
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomDashboardsClientwise]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[GetCustomDashboardsClientwise]
END
GO

/****** Object:  StoredProcedure [dbo].[GetCustomDashboardsClientwise]    Script Date: 2/22/2016 4:20:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomDashboardsClientwise] 
(
	@UserId UNIQUEIDENTIFIER, @ClientId UNIQUEIDENTIFIER
)
AS 
BEGIN
	SELECT dash.DisplayName,dash.id as DashboardId,ISNULL(up.PermissionType, '') as PermissionType
	FROM Dashboard dash
	INNER JOIN Report_Intergration_Conf ric ON (dash.id = ric.IdentifierValue AND ric.TableName = 'Dashboard' AND ric.IdentifierColumn = 'id' AND ric.ClientId = @ClientId)
	LEFT JOIN User_Permission up ON (dash.id = up.DashboardId and up.UserId = @UserId)
END
