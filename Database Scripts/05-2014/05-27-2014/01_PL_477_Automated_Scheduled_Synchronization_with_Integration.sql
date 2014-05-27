-- Run below script on MRPQA

IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'NextSyncDate' AND [object_id] = OBJECT_ID(N'SyncFrequency'))
BEGIN
    Alter table [SyncFrequency] 
	ADD NextSyncDate datetime NULL
END


Go
IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'IntegrationWindowsService')
BEGIN
insert into Notification 
	(
		NotificationInternalUseOnly,
		Title,
		Description,
		NotificationType,
		EmailContent,
		IsDeleted,
		CreatedDate,
		CreatedBy,
		Subject
	)
values 
	(
		'IntegrationWindowsService',
		'IntegrationWindowsService',
		'When integration windows service stopped working',
		'CM',
		'Scheduled integration windows service has stopped working at [time].<br /><br /> For more detail please look into log file.<br /><br />Thank you,<br/>Bulldog Gameplan Admin',
		0,
		GETDATE(),
		'092f54df-4c71-4f2f-9d21-0ae16155e5c1',
		'Gameplan : Integration windows service'
	)
end