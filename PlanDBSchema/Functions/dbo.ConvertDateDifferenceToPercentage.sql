-- ======================================================
-- Author:		Arpita Soni
-- Create date: 08/09/2016
-- Description:	Convert date differenct into percentage
-- ======================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ConvertDateDifferenceToPercentage]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ConvertDateDifferenceToPercentage]
GO
CREATE FUNCTION [dbo].[ConvertDateDifferenceToPercentage]
(
	@StartDate DATETIME,
	@EndDate DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @Percent INT = (DATEDIFF(DAY, @StartDate,GETDATE()) * 100) / DATEDIFF(DAY, @StartDate,@EndDate)
	
	IF(@Percent > 100)
		SET @Percent = 100

	RETURN @Percent
END
GO