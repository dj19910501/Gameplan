-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 14/11/2014
-- Description : Add email entry for TacticOwnerChanged in Notification table.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Notification')
BEGIN

	IF NOT EXISTS (SELECT 1 FROM [Notification] WHERE NotificationInternalUseOnly = 'TacticOwnerChanged')
	BEGIN

		INSERT INTO [Notification](NotificationInternalUseOnly, Title, [Description], NotificationType, EmailContent, IsDeleted, CreatedDate, CreatedBy, [Subject])
		SELECT TOP 1 
			'TacticOwnerChanged',
			'Tactic Owner Changed',
			'When owner of tactic changed',
			'CM',
			'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following tactic.<br><br>
			<table><tr>
			<td>Tactic</td><td>:</td><td>[tacticname]</td></tr><tr>
			<td>Program</td><td>:</td><td>[programname]</td></tr><tr>
			<td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr>
			<td>Plan</td><td>:</td><td>[planname]</td></tr>
			</table>
			<br><br>Thank You,<br>Bulldog Gameplan Admin',
			IsDeleted,
			CreatedDate,
			CreatedBy,
			'Gameplan : Tactic owner has been changed'
		FROM Notification 
		WHERE IsDeleted = 0

	END

END

