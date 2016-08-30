/****** Object:  StoredProcedure [dbo].[GetClientEntityList]    Script Date: 08/23/2016 3:22:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClientEntityList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetClientEntityList]
GO
/****** Object:  StoredProcedure [dbo].[GetClientEntityList]    Script Date: 08/23/2016 3:22:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClientEntityList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetClientEntityList] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 23-8-2016
-- Description:	get the list of al entities for client for create alert Rule
-- =============================================
ALTER PROCEDURE [dbo].[GetClientEntityList]
	-- Add the parameters for the stored procedure here
	@ClientId nvarchar(255)
AS
BEGIN

	SET NOCOUNT ON;

	select * from [vClientWise_EntityList] where clientid=@ClientId
	order by CreatedDate

END

GO