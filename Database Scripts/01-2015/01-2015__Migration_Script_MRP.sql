--Start 01/01/2015

-- ======================================================================================
-- Created By : Sohel Pathan
-- Created Date : 22/12/2014
-- Description : Make default entries for Gameplan DataType Pull for MQL 
-- ======================================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'GameplanDataTypePull')
BEGIN
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'IntegrationType')
	BEGIN
		IF EXISTS (SELECT 1 FROM IntegrationType WHERE Code = 'Eloqua' AND IsDeleted = 0)
		BEGIN
			DECLARE @IntegrationTypeId INT = (SELECT IntegrationTypeId FROM IntegrationType WHERE Code = 'Eloqua' AND IsDeleted = 0)
			IF (@IntegrationTypeId > 0)
			BEGIN
				
				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'MQLDate' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'MQLDate', 'MQL Date', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'CampaignId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'CampaignId', 'Last Eloqua Campaign Id', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'ViewId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'ViewId', 'View Id', 'MQL', 0)
				END

				IF NOT EXISTS(SELECT 1 FROM GameplanDataTypePull WHERE IntegrationTypeId = @IntegrationTypeId AND ActualFieldName = 'ListId' AND [Type] = 'MQL' AND IsDeleted = 0)
				BEGIN
					INSERT INTO GameplanDataTypePull (IntegrationTypeId, ActualFieldName, DisplayFieldName, [Type], IsDeleted)
					VALUES(@IntegrationTypeId, 'ListId', 'List Id', 'MQL', 0)
				END

			END
		END
	END
END


--End 01/01/2015

GO

--Start 01/02/2015

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

--End 01/02/2015