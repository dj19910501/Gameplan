-- =============================================
-- Author:		Mitesh Vaishnav 
-- Create date: 08/22/2016
-- Description:	Add integration instance log with section details
-- =============================================
CREATE PROCEDURE [INT].[AddIntegrationInstanceSectionLog]
	(
	@IntegrationInstanceSectionLogId INT=0,
	@IntegrationInstanceId INT,
	@UserID NVARCHAR(36),
	@IntegrationInstanceLogId INT,
	@sectionName NVARCHAR(1000),
	@Status NVARCHAR(20) NULL=NULL,
	@ErrorDescription NVARCHAR(MAX) NULL=NULL,
	@OutPutLogId INT OUTPUT
	)
AS
BEGIN
	IF (@IntegrationInstanceSectionLogId=0)
	BEGIN
	INSERT INTO [dbo].[IntegrationInstanceSection]
           ([IntegrationInstanceLogId]
           ,[IntegrationInstanceId]
           ,[SectionName]
           ,[SyncStart]
           ,[SyncEnd]
           ,[Status]
           ,[Description]
           ,[CreatedDate]
           ,[CreateBy])
     VALUES
           (@IntegrationInstanceLogId
           ,@IntegrationInstanceId
           ,@sectionName
           ,GETDATE()
           ,NULL
           ,@Status
           ,@ErrorDescription
           ,GETDATE()
           ,@UserID)

		   SELECT @IntegrationInstanceSectionLogId=SCOPE_IDENTITY()
		   END
	ELSE
	BEGIN
	UPDATE [dbo].[IntegrationInstanceSection]
			SET [SyncEnd]=GETDATE(),
				[Status]=@Status,
				[Description]=@ErrorDescription
			WHERE IntegrationInstanceSectionId=@IntegrationInstanceSectionLogId
			SELECT @IntegrationInstanceSectionLogId=0
	END

	SET @OutPutLogId=@IntegrationInstanceSectionLogId
END
