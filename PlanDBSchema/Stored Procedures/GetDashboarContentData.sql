
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
	@UserId varchar(max),
	@DashboardID int = 0
AS
BEGIN
	IF (@UserId IS NOT NULL AND @UserId != '')
	BEGIN
		IF (ISNULL(@DashboardID, 0) > 0)
		BEGIN
			IF EXISTS (SELECT D.id
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID)
			BEGIN
				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, [Rows], [Columns], ParentDashboardId, IsDeleted,IsComparisonDisplay=ISNULL(D.IsComparisonDisplay,0), 
					ISNULL(HelpTextId,0) AS HelpTextId 
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0 AND d.id = @DashboardID
                    ORDER BY D.DisplayOrder


				SELECT dc.id, dc.DisplayName, dc.DashboardId, dc.DisplayOrder, dc.ReportTableId, dc.ReportGraphId, dc.Height, dc.Width, dc.Position, dc.IsCumulativeData, dc.IsCommunicativeData, dc.DashboardPageID,					dc.IsDeleted, dc.DisplayIfZero, dc.KeyDataId, dc.HelpTextId
					FROM DashboardContents AS dc 
					INNER JOIN Dashboard AS D ON D.id = dc.DashboardId AND D.IsDeleted = 0 AND D.ParentDashboardId IS NULL
					INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE dc.IsDeleted = 0 AND dc.DashboardId = @DashboardID
					ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'Client Not Authorize to Access Dashboard'
			END
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT D.id
				FROM Dashboard D
                INNER JOIN User_Permission UP ON d.id = UP.DashboardId  AND UP.UserId = @UserId
				WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0)
			BEGIN

				SELECT DISTINCT D.id,D.Name, D.DisplayName, D.DisplayOrder, D.CustomCSS, D.[Rows], D.[Columns], D.ParentDashboardId, D.IsDeleted, D.IsComparisonDisplay, D.HelpTextId
					FROM Dashboard D
                    INNER JOIN User_Permission UP ON d.id = UP.DashboardId AND UP.UserId = @UserId
					WHERE D.ParentDashboardId IS NULL AND D.IsDeleted = 0
                    ORDER BY D.DisplayOrder
			END
			ELSE
			BEGIN
				SELECT 'No Dashboard Configured For Client'
			END
		END
	END
	ELSE
	BEGIN
		SELECT 'Please Provide Proper UserId'
	END
END

