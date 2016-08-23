-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Add log for integration instance
-- =============================================
CREATE PROCEDURE [INT].[AddIntegrationInstanceLog]
	(
	@IntegrationInstanceId INT,
	@UserID NVARCHAR(36),
	@IntegrationInstanceLogId INT,
	@Status NVARCHAR(20) NULL=NULL,
	@ErrorDescription NVARCHAR(MAX) NULL=NULL,
	@OutPutLogid int output
	)
AS
BEGIN
	IF (@IntegrationInstanceLogId=0)
		BEGIN
	INSERT INTO [dbo].[IntegrationInstanceLog]
           ([IntegrationInstanceId]
           ,[SyncStart]
           ,[SyncEnd]
           ,[Status]
           ,[ErrorDescription]
           ,[CreatedDate]
           ,[CreatedBy]
           ,[IsAutoSync])
     VALUES
           (@IntegrationInstanceId
           ,GETDATE()
           ,NULL
           ,NULL
           ,NULL
           ,GETDATE()
           ,@UserID
           ,NULL)

		   select @IntegrationInstanceLogId=SCOPE_IDENTITY()
		END
	ELSE
		BEGIN
		UPDATE [dbo].[IntegrationInstanceLog]
		SET [SyncEnd]=GETDATE()
		   ,[Status]=@Status
		   ,[ErrorDescription]=@ErrorDescription
		WHERE IntegrationInstanceLogId=@IntegrationInstanceLogId
		END
		set @OutPutLogid=@IntegrationInstanceLogId

END
