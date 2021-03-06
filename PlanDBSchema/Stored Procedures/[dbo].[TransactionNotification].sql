SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('[dbo].[TransactionNotification]'))
	DROP PROCEDURE [dbo].[TransactionNotification]
GO

CREATE PROCEDURE [dbo].[TransactionNotification] (@ClientID INT, @UserId INT, @LastDate DATETIME, @NewTransactionCount INT, @UpdatedTrasactionCount INT)
AS
BEGIN

	DECLARE @Message NVARCHAR (1000)
	SET @Message = ''
	IF (@NewTransactionCount > 0)
		SET @Message = @Message + STR(@NewTransactionCount) + ' new transactions imported into plan. '
	IF (@UpdatedTrasactionCount > 0) 
		SET @Message = @Message + STR(@UpdatedTrasactionCount) + ' transactions updated'

	IF (LEN(@Message) > 0)
	INSERT INTO dbo.User_Notification_Messages
			( ComponentName ,
			  ComponentId ,
			  EntityId ,
			  Description ,
			  ActionName ,
			  IsRead ,
			  ReadDate ,
			  CreatedDate ,
			  UserId ,
			  RecipientId ,
			  ClientID
			)
	SELECT  N'Plan' , -- ComponentName - nvarchar(50)
			  0 , -- ComponentId - int
			  0 , -- EntityId - int
			  @Message, -- Description - nvarchar(250)
			  N'FinancialIntegration' , -- ActionName - nvarchar(50)
			  0 , -- IsRead - bit
			  GETDATE() , -- ReadDate - datetime
			  GETDATE() , -- CreatedDate - datetime
			  @UserId , -- UserId - int
			  UN.UserId , -- RecipientId - int
			  @ClientID
	FROM dbo.User_Notification UN JOIN dbo.Notification N ON N.NotificationId = UN.NotificationId
	WHERE UN.ClientId = @ClientID AND N.IsDeleted = 0 AND N.NotificationInternalUseOnly = 'NewTransactionsArrived'

END