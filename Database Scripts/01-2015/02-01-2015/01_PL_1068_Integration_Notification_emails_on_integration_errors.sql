-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 02/01/2015
-- Description : Add email entry for Sync Notification Email in Notification table.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Notification')
BEGIN

	IF NOT EXISTS (SELECT 1 FROM [Notification] WHERE NotificationInternalUseOnly = 'SyncIntegrationError')
	BEGIN

		INSERT INTO [Notification](NotificationInternalUseOnly, Title, [Description], NotificationType, EmailContent, IsDeleted, CreatedDate, CreatedBy, [Subject])
		SELECT TOP 1 
			'SyncIntegrationError',
			'Sync integration error email',
			'Sync integration error email',
			'CM',
			'Dear [NameToBeReplaced],
			<br><br>Below is the Sync integration summary for your latest sync:
			<br><br>[ErrorBody]
			<br><br>Thank You,<br>Bulldog Gameplan Admin',
			IsDeleted,
			CreatedDate,
			CreatedBy,
			'Gameplan : Sync Integration Summary'
		FROM Notification 
		WHERE IsDeleted = 0

	END

END

