
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDashboarContentData]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[GetDashboarContentData]
END
GO

/****** Object:  StoredProcedure [dbo].[DeleteBudget]    Script Date: 2/22/2016 4:20:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDashboarContentData]
	-- Add the parameters for the stored procedure here
	@ClientId varchar(max),
	@DashboardID int = 0    
AS
BEGIN
	IF (@ClientId IS NOT NULL AND @ClientId != '')
	BEGIN
		IF (ISNULL(@DashboardID, 0) > 0)
		BEGIN
			IF EXISTS (SELECT db.id
				FROM Dashboard db
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE IsDeleted = 0 AND db.id = @DashboardID)
			BEGIN
				SELECT db.id, Name, DisplayName, DisplayOrder, CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted, IsComparisonDisplay, HelpTextId
				FROM Dashboard db
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE IsDeleted = 0 AND db.id = @DashboardID

				SELECT dc.id, dc.DisplayName, dc.DashboardId, dc.DisplayOrder, dc.ReportTableId, dc.ReportGraphId, dc.Height, dc.Width, dc.Position, dc.IsCumulativeData, dc.IsCommunicativeData, dc.DashboardPageID,					dc.IsDeleted, dc.DisplayIfZero, dc.KeyDataId, dc.HelpTextId
				FROM DashboardContents AS dc 
				INNER JOIN Dashboard AS db ON db.id = dc.DashboardId AND db.IsDeleted = 0
				INNER JOIN Report_Intergration_Conf AS RIC ON (RIC.IdentifierValue = db.id AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId)
				WHERE dc.IsDeleted = 0 AND dc.DashboardId = @DashboardID
			END
			ELSE
			BEGIN
				SELECT 'Client Not Authorize to Access Dashboard'
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT db.id
						FROM Dashboard AS db
						INNER JOIN Report_Intergration_Conf RIC ON db.id = RIC.IdentifierValue AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId
						WHERE db.IsDeleted = 0)
			BEGIN

				SELECT db.id, Name, DisplayName, DisplayOrder, CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted, IsComparisonDisplay, HelpTextId
					FROM Dashboard AS db
					INNER JOIN Report_Intergration_Conf RIC ON db.id = RIC.IdentifierValue AND TableName = 'Dashboard' AND IdentifierColumn = 'id' AND ClientId = @ClientId
					WHERE db.IsDeleted = 0
			END
			ELSE
			BEGIN
				SELECT 'No Dashboard Configured For Client'
			END
		END
	END
	ELSE
	BEGIN
		SELECT 'Please Provide Proper ClientId'
	END
END

