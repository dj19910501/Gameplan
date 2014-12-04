IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been approved.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been declined.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been submitted for approval.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignCommentAdded')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that comment has been added to the following campaign<br/><br/><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignCommentAdded'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been approved.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been declined.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='CampaignSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been submitted for approval.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='CampaignSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramCommentAdded')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that comment has been added to the following program<br/><br/><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Comment</td><td>:</td><td>[CommentToBeReplaced]</td></tr><tr><td>Comment added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramCommentAdded'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been approved.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been declined.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ProgramSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following program has been submitted for approval.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ProgramSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticApproved')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been approved.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticApproved'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticDeclined')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been declined.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Declined by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticDeclined'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ImprovementTacticSubmitted')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>Please note that following improvement tactic has been submitted for approval.<br><br><table><tr><td>Improvement Tactic Name</td><td>:</td><td>[ImprovementTacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='ImprovementTacticSubmitted'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='TacticOwnerChanged')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following tactic.<br><br><table><tr><td>Tactic</td><td>:</td><td>[tacticname]</td></tr><tr><td>Program</td><td>:</td><td>[programname]</td></tr><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin'
	Where NotificationInternalUseOnly='TacticOwnerChanged'
End
IF Exists(Select 1 from [Notification] Where NotificationInternalUseOnly='ChangePassword')
Begin
	Update [Notification] Set EmailContent='Dear [NameToBeReplaced],<br/><br/>This message is to inform you that your password was recently changed on your Bulldog Gameplan account. If you did not perform this action, please contact you administrator as soon as possible to resolve this issue.<br/><br/>URL:&nbsp;&nbsp;&nbsp;[URL] <br/><br/>Thank You,<br />Bulldog Gameplan Team'
	Where NotificationInternalUseOnly='ChangePassword'
End