
/****** Object:  StoredProcedure [dbo].[ELMAH_GetErrorXml]    Script Date: 2/22/2016 4:32:29 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorXml]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[ELMAH_GetErrorXml]
END
GO

/****** Object:  StoredProcedure [dbo].[ELMAH_GetErrorXml]    Script Date: 2/22/2016 4:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml]
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

