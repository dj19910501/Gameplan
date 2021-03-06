IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ELMAH_GetErrorXml') AND xtype IN (N'P'))
    DROP PROCEDURE ELMAH_GetErrorXml
GO

CREATE PROCEDURE [ELMAH_GetErrorXml]
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS

    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application


GO
