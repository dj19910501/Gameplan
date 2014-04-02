IF EXISTS (SELECT * FROM [Notification] WHERE NotificationInternalUseOnly = 'ChangePassword')
BEGIN
	UPDATE [Notification] SET EmailContent ='Dear [NameToBeReplaced],<br/><br/>This message is to inform you that your password was recently changed on your Bulldog Gameplan account. If you did not perform this action, please contact you administrator as soon as possible to resolve this issue.<br/><br/>Thank You,<br />Bulldog Gameplan Admin',
		   [Subject] = 'Bulldog Gameplan Password Changed'
	WHERE NotificationInternalUseOnly = 'ChangePassword'
END

