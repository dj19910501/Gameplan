
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='GameplanDataTypePull')
BEGIN

	IF NOT((SELECT COUNT(*) FROM GameplanDataTypePull where [Type]='CW' AND [ActualFieldName] = 'Campaign ID') > 0)
	BEGIN

		UPDATE GameplanDataTypePull SET [ActualFieldName] = 'CampaignID' 
		where [Type]='CW' AND [ActualFieldName] = 'Campaign ID'

	END

	IF NOT((SELECT COUNT(*) FROM GameplanDataTypePull where [Type]='CW' AND [ActualFieldName] = 'Revenue Amount') > 0)
	BEGIN

		UPDATE GameplanDataTypePull SET [ActualFieldName] = 'Amount' 
		where [Type]='CW' AND [ActualFieldName] = 'Revenue Amount'

	END
END