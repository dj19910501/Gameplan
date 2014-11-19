-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 14/11/2014
-- Description : Add email entry for TacticOwnerChanged in Notification table.
-- ======================================================================================

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Notification')
BEGIN

	IF EXISTS (SELECT 1 FROM [Notification] WHERE NotificationInternalUseOnly = 'TacticOwnerChanged')
	BEGIN

		UPDATE [NOTIFICATION] SET EmailContent = REPLACE(EmailContent, '<br><br>Thank You,<br>Bulldog Gameplan Admin', '<br>Thank You,<br>Bulldog Gameplan Admin')
		WHERE NotificationInternalUseOnly = 'TacticOwnerChanged'

	END

END