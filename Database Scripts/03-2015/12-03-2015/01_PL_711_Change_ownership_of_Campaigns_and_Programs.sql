---- ProgramOwnerChanged 
IF NOT EXISTS (SELECT TOP 1 NotificationInternalUseOnly FROM dbo.Notification WHERE NotificationInternalUseOnly=N'ProgramOwnerChanged')
BEGIN

INSERT dbo.Notification
        ( NotificationInternalUseOnly ,
          Title ,
          Description ,
          NotificationType ,
          EmailContent ,
          IsDeleted ,
          CreatedDate ,
          CreatedBy ,
          ModifiedDate ,
          ModifiedBy ,
          Subject
        )
VALUES  ( 'ProgramOwnerChanged' , -- NotificationInternalUseOnly - varchar(255)
          N'Program Owner Changed' , -- Title - nvarchar(255)
          N'When owner of program changed' , -- Description - nvarchar(4000)
          'CM' , -- NotificationType - char(2)
          N'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following program.<br><br><table><tr><td>Program</td><td>:</td><td>[programname]</td></tr><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin' , -- EmailContent - nvarchar(4000)
          0 , -- IsDeleted - bit
          GETDATE() , -- CreatedDate - datetime
          '092F54DF-4C71-4F2F-9D21-0AE16155E5C1' , -- CreatedBy - uniqueidentifier,
		  NULL,
		  NULL,
          'Gameplan : Program owner has been changed'  -- Subject - varchar(255)
        )

END

------------ CampaignOwnerChanged
IF NOT EXISTS (SELECT TOP 1 NotificationInternalUseOnly FROM dbo.Notification WHERE NotificationInternalUseOnly=N'CampaignOwnerChanged')
BEGIN

INSERT dbo.Notification
        ( NotificationInternalUseOnly ,
          Title ,
          Description ,
          NotificationType ,
          EmailContent ,
          IsDeleted ,
          CreatedDate ,
          CreatedBy ,
          ModifiedDate ,
          ModifiedBy ,
          Subject
        )
VALUES  ( 'CampaignOwnerChanged' , -- NotificationInternalUseOnly - varchar(255)
          N'Campaign Owner Changed' , -- Title - nvarchar(255)
          N'When owner of Campaign changed' , -- Description - nvarchar(4000)
          'CM' , -- NotificationType - char(2)
          N'Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following campaign.<br><br><table><tr><td>Campaign</td><td>:</td><td>[campaignname]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Bulldog Gameplan Admin' , -- EmailContent - nvarchar(4000)
          0 , -- IsDeleted - bit
          GETDATE() , -- CreatedDate - datetime
          '092F54DF-4C71-4F2F-9D21-0AE16155E5C1' , -- CreatedBy - uniqueidentifier,
		  NULL,
		  NULL,
          'Gameplan : Campaign owner has been changed'  -- Subject - varchar(255)
        )

END