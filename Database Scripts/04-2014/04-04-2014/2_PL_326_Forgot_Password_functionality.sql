-- Execute Below Script in MRPDev

Go
IF NOT EXISTS(SELECT TOP 1 * FROM Notification WHERE [NotificationInternalUseOnly] = 'ResetPasswordLink')
BEGIN

INSERT [dbo].[Notification] ([NotificationInternalUseOnly], [Title], [Description], [NotificationType], [EmailContent], [IsDeleted], [CreatedDate], [CreatedBy], [ModifiedDate], [ModifiedBy], [Subject]) VALUES (N'ResetPasswordLink', N'ResetPasswordLink', N'When user requested to reset password', N'CM', N'This e-mail has been sent in response to your request for help resetting your Gameplan password.<br /> To initiate the process for resetting the password for your Gameplan account, follow the link below: <br /><br /> [PasswordResetLinkToBeReplaced] <br/><br />Note that this e-mail will expire on [ExpireDateToBeReplaced]. If it expires before you are able to complete the password reset process, you may request a new password reset.<br/><br />Thank you,<br/>Gameplan Administrator', 0, CAST(0x0000A2FC00C8772A AS DateTime), N'092f54df-4c71-4f2f-9d21-0ae16155e5c1', NULL, NULL, N'Gameplan : Password reset link')
END