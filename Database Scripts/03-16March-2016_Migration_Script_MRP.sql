-- Added By : Rahul Shah
-- Added Date : 03/10/2016
-- Description :insert Email Format for Plan Owner Name change.
-- ======================================================================================
GO
DECLARE @NotificationInternalUseOnly nvarchar (max) = 'PlanOwnerChanged'
DECLARE @Title nvarchar (50) = 'Plan Owner Changed'
DECLARE @Description nvarchar (50) = 'When owner of Plan changed'
DECLARE @NotificationType nvarchar (10) = 'CM'
DECLARE @EmailContent nvarchar (max) = 'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following plan.<br><br><table><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Hive9 Plan Admin'
DECLARE @isDeleted bit = 0
DECLARE @CreatedBy nvarchar (50) 
DECLARE @ModifiedBy nvarchar (50)
DECLARE @Subject nvarchar (50) = 'Plan : Plan owner has been changed'

select @CreatedBy = CreatedBy from [Notification] where NotificationId = (select max(NotificationId) from [Notification])
select @ModifiedBy = ModifiedBy from [Notification] where NotificationId = (select max(NotificationId) from [Notification])

IF NOT EXISTS (SELECT * FROM [Notification] WHERE  NotificationInternalUseOnly= @NotificationInternalUseOnly AND isDeleted = @isDeleted)
BEGIN
    INSERT INTO [Notification](NotificationInternalUseOnly,Title,Description,NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,Subject) 
         VALUES(@NotificationInternalUseOnly,@Title,@Description,@NotificationType,@EmailContent,@isDeleted,getdate(),@CreatedBy,getdate(),@ModifiedBy,@Subject)
END
GO

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](50) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](50) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(10)
declare @release nvarchar(10)
set @release = 'Mar16.2016'
set @version = 'Mar16.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO