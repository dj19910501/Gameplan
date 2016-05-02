-- Created by Komal Rawal 
-- Created on :: 02-May-2016
-- Desc :: Delete LastViewedData

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteLastViewedData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteLastViewedData]
GO
CREATE PROCEDURE DeleteLastViewedData
@UserId nvarchar(max) = null,
@PreviousIds nvarchar(max) = null
AS
BEGIN
SET NOCOUNT ON;
DECLARE @CheckIsAlreadyDeleted  INT = 0
SELECT @CheckIsAlreadyDeleted  = COUNT(*) FROM [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL AND Id IN(SELECT CONVERT(INT,val) From [dbo].[comma_split](@PreviousIds,','))
IF(@CheckIsAlreadyDeleted>0)
BEGIN

DELETE FROM  [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL AND Id IN(SELECT CONVERT(INT,val) From [dbo].[comma_split](@PreviousIds,','))

END
ELSE
BEGIN
	DELETE FROM  [Plan_UserSavedViews] WHERE Userid=@UserId AND ViewName IS NULL 	
END
END
GO
