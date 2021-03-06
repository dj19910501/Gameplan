
/****** Object:  StoredProcedure [dbo].[SaveLogNoticationdata]    Script Date: 08/12/2016 17:52:34 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveLogNoticationdata]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveLogNoticationdata]
GO
/****** Object:  StoredProcedure [dbo].[SaveLogNoticationdata]    Script Date: 08/12/2016 17:52:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveLogNoticationdata]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveLogNoticationdata] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveLogNoticationdata]
@action nvarchar(50) = null,
@actionSuffix nvarchar(max) = null,
@componentId int = null,
@componentTitle nvarchar(256) = null,
@description nvarchar(50) = null,
@objectId int = null ,
@parentObjectId int =  null ,
@TableName nvarchar(50) = null,
@Userid uniqueidentifier,
@ClientId uniqueidentifier,
@UserName nvarchar(250) = null,
@RecipientIDs nvarchar(max) = null,
@EntityOwnerID nvarchar(max) = null
AS
BEGIN
Declare @InsertedCount int
Declare @ActivityMessageIds int
Declare @NotificationMessage nvarchar(250)
Declare @TacticIsApproved nvarchar(50) = 'TacticIsApproved'
Declare @ReportShared nvarchar(50) = 'ReportIsShared'
Declare @TacticEdited nvarchar(50) = 'TacticIsEdited'
Declare @CommentAddedToTactic nvarchar(50) = 'CommentAddedToTactic'
Declare @CampaignIsEdited nvarchar(50) = 'CampaignIsEdited'
Declare @ProgramIsEdited nvarchar(50) = 'ProgramIsEdited'
Declare @TacticIsSubmitted nvarchar(50) = 'TacticIsSubmitted'
Declare @CommentAddedToCampaign nvarchar(50) = 'CommentAddedToCampaign'
Declare @CommentAddedToProgram nvarchar(50) = 'CommentAddedToProgram'
Declare @CampaignIsApproved nvarchar(50) = 'CampaignIsApproved'
Declare @ProgramIsApproved nvarchar(50) = 'ProgramIsApproved'
Declare @OwnerChange nvarchar(50) = 'EntityOwnershipAssigned'
Declare @NotificationName nvarchar(50)

IF OBJECT_ID('tempdb..#tempNotificationdata') IS NOT NULL
Drop Table #tempNotificationdata
 
insert into ChangeLog(TableName,ObjectId,ParentObjectId,ComponentId,ComponentTitle,ComponentType,ActionName,ActionSuffix,[TimeStamp],UserId,IsDeleted,ClientId) 
values (@TableName,@objectId,@parentObjectId,@componentId,@componentTitle,@description,@action,@actionSuffix,GETDATE(),@Userid,0,@ClientId)

SELECT @InsertedCount=@@ROWCOUNT
DECLARE @ret int = CASE WHEN @InsertedCount = 0 THEN 0 ELSE 1 END
select @ret

if(@TableName <> 'Model')
BEGIN
	select * into #tempnotificationdata from 
	(select u.userid,u.notificationid,n.notificationinternaluseonly from user_notification as u join notification as n on
	 u.notificationid= n.notificationid where  n.notificationtype = 'AM'  and n.isdeleted = 0 and
	 userid in (SELECT Item From dbo.SplitString(@RecipientIDs,','))) as result

    IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticEdited and @action='updated' and (@description ='tactic' or @description ='tactic results'))) 
	Begin
		SET @NotificationMessage = 'Tactic '+ @componentTitle +' has been changed by ' + @UserName
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsEdited and @action='updated' and  @description ='campaign' )) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
	    SET @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
		
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsEdited  and @action='updated' and @description ='program')) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
	
	End

	if(@action='updated' and @Userid <> @EntityOwnerID  and @NotificationMessage <> '' and (@description ='tactic' or @description ='tactic results' or @description ='campaign' or @description ='program'))
    Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @ReportShared and  @TableName ='Report' and @action='shared' )) 
	Begin
		select @NotificationMessage = @UserName +' has shared report with you '
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @ReportShared and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @CommentAddedToTactic and @description ='tactic' and @action='commentadded' )) 
	Begin
	
	  SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	  SET  @NotificationName = @CommentAddedToTactic
	
	End

	 IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CommentAddedToCampaign and @description ='campaign'and @action='commentadded' )) 
	Begin
	
	   SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
       SET	@NotificationName = @CommentAddedToCampaign
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @CommentAddedToProgram and @description ='program'and @action='commentadded' )) 
	Begin
	
		SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	    SET  @NotificationName = @CommentAddedToProgram
	End

	if(@action='commentadded' and @NotificationMessage <> '' and (@description ='tactic' or @description ='campaign' or @description ='program'))
	begin
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @NotificationName and UserId <> @Userid
    End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsApproved and @action='approved' and @description ='tactic' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @TacticIsApproved
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsApproved  and @action='approved' and  @description ='campaign' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @CampaignIsApproved
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsApproved  and @action='approved' and @description ='program')) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @ProgramIsApproved
	End

    if(@action='approved' and @NotificationMessage <> '' and (@description ='tactic' or @description ='campaign' or @description ='program'))
	begin
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @NotificationName and UserId <> @Userid
    End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsSubmitted and @description ='tactic' and @action='submitted' )) 
	Begin
		select @NotificationMessage = @UserName + ' has submitted tactic ' + @componentTitle +' for approval '

	    insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @TacticIsSubmitted and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @OwnerChange  and @action='ownerchanged' )) 
	Begin
		select @NotificationMessage = @UserName + ' has made you the owner of ' + @description + ' ' + @componentTitle
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
	End

END

End



GO
