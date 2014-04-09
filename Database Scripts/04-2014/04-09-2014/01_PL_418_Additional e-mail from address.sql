IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'ContactSupport')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear Admin,<br/><br/>Please note that following issue has been submitted.<br><br><table><tr><td>Submitted by</td><td>:</td><td>[EmailToBeReplaced]</td></tr><tr><td>Issue</td><td>:</td><td>[IssueToBeReplaced]</td></tr></table><br><br>Thank You<br>'
	WHERE NotificationInternalUseOnly = 'ContactSupport'
END


