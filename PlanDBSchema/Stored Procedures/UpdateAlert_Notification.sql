
/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/17/2016 14:56:16 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAlert_Notification]
GO
/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/17/2016 14:56:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateAlert_Notification] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 16-8-2016
-- Description:	method to update isready field for Alert and Notification
-- =============================================
ALTER PROCEDURE [dbo].[UpdateAlert_Notification]
	@UserId UniqueIdentifier,
	@Type nvarchar(100)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	If(@Type='alert')
	Begin
		Update Alerts set IsRead=1 where UserId=@UserId
	End
	Else If(@Type='notification')
	Begin
		Declare @ReadDate datetime = GETDATE()
		Update user_notification_messages set IsRead=1 , ReadDate=@ReadDate where RecipientId=@UserId
	End
END

GO
