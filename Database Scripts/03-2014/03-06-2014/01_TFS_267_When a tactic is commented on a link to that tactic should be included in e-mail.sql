IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'TacticCommentAdded')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear User,<br/><br/>Please note that comment has been added to the following tactic<br/><br/><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Gameplan Admin'
	WHERE NotificationInternalUseOnly = 'TacticCommentAdded'
END

