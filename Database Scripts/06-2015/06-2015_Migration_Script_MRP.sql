-- Created By : Mitesh Vaishnav
-- Created Date : 05/25/2015
-- Description :Add a mapping field for SFDC - Activity Type
-- ======================================================================================

--Add column Client into ELMAH_Error table
IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'Client' AND [object_id] = OBJECT_ID(N'ELMAH_Error'))
ALTER TABLE [dbo].[ELMAH_Error] ADD Client NVARCHAR(50) NULL 
GO

-- Drop and create ELMAH_LogError stored procedure
IF EXISTS ( SELECT  * FROM sys.objects WHERE   object_id = OBJECT_ID(N'ELMAH_LogError') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE ELMAH_LogError
END
GO

CREATE PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME,
	@Client NVARCHAR(500)
)
AS

    SET NOCOUNT ON

    INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc],
			[Client]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc,
			@Client
        )
GO

-- Drop and create ELMAH_GetErrorsXml stored procedure
IF EXISTS ( SELECT  * FROM sys.objects WHERE   object_id = OBJECT_ID(N'ELMAH_GetErrorsXml') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE ELMAH_GetErrorsXml
END
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS 

    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT 
        @TotalCount = COUNT(1) 
    FROM 
        [ELMAH_Error]
    WHERE 
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM 
            [ELMAH_Error]
        WHERE   
            [Application] = @Application
        ORDER BY 
            [TimeUtc] DESC, 
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT 
        errorId     = [ErrorId], 
        application = [Application],
        host        = [Host], 
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode], 
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z',
		client		= [Client]
    FROM 
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND 
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC, 
        [Sequence] DESC
    FOR
        XML AUTO
GO


GO
/****** Object:  Table [dbo].[IntegrationInstance_UnprocessData]    Script Date: 06/30/2015 16:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IntegrationInstance_UnprocessData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[EloquaCampaignID] [nvarchar](50) NULL,
	[ExternalCampaignID] [nvarchar](50) NULL,
	[ResponseDateTime] [datetime] NOT NULL,
	[ResponseCount] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_IntegrationInstance_UnprocessData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IntegrationInstance_UnprocessData_IntegrationInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]'))
ALTER TABLE [dbo].[IntegrationInstance_UnprocessData]  WITH CHECK ADD  CONSTRAINT [FK_IntegrationInstance_UnprocessData_IntegrationInstance] FOREIGN KEY([IntegrationInstanceId])
REFERENCES [dbo].[IntegrationInstance] ([IntegrationInstanceId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IntegrationInstance_UnprocessData_IntegrationInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[IntegrationInstance_UnprocessData]'))
ALTER TABLE [dbo].[IntegrationInstance_UnprocessData] CHECK CONSTRAINT [FK_IntegrationInstance_UnprocessData_IntegrationInstance]
GO
