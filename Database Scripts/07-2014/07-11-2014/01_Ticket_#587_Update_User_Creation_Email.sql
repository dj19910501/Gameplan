
--Execute this script on MRP Database--

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Notification' AND COLUMN_NAME = 'EmailContent')
BEGIN

UPDATE Notification SET EmailContent='Dear [NameToBeReplaced],<br><br>An account has been created for you in Bulldog Gameplan. Here are your credentials:<br><br><table><tr><td>Username</td><td>:</td><td>[LoginToBeReplaced]</td></tr><tr><td>Password</td><td>:</td><td>[PasswordToBeReplaced]</td></tr></table><br>Click on the link and start planning! <a href=[ApplicationLink]>Log into Bulldog Gameplan</a><br><br>Thank You,<br>Bulldog Gameplan Admin' WHERE NotificationInternalUseOnly='UserCreated'

END
