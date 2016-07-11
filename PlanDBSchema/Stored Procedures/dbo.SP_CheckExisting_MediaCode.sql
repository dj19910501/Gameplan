
GO

/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/08/2016 6:34:02 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CheckExisting_MediaCode]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
END
GO

/****** Object:  StoredProcedure [dbo].[SP_CheckExisting_MediaCode]    Script Date: 07/08/2016 6:34:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 08-07-2016
-- Description:	method to check whether the media code already exist or not
-- =============================================
CREATE PROCEDURE [dbo].[SP_CheckExisting_MediaCode]
	-- Add the parameters for the stored procedure here
	@ClientId uniqueidentifier ,
	@MediaCode nvarchar(max),
	@IsExists int Output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	set @IsExists=(Select count(*) from [vClientWise_Tactic] where ClientId=@ClientId
	and MediaCode is not null and mediacode=@MediaCode and IsDeleted=0)

END

GO


