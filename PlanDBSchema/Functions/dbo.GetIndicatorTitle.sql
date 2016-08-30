
-- ======================================================================================================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	Get title of indicator based on rule
-- ======================================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetIndicatorTitle]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetIndicatorTitle]
GO

CREATE FUNCTION [dbo].[GetIndicatorTitle]
(
	@IndicatorCode NVARCHAR(50),
	@ClientId UNIQUEIDENTIFIER,
	@EntityType NVARCHAR(50)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @IndicatorTitle NVARCHAR(MAX)

	IF(@IndicatorCode = 'PLANNEDCOST' OR @EntityType = 'Line Item' OR @EntityType = 'LineItem')
	BEGIN
		SET @IndicatorTitle = 'Planned Cost'
	END
	ELSE IF (@IndicatorCode = 'REVENUE')
	BEGIN
		SET @IndicatorTitle = 'Revenue'
	END
	ELSE
	BEGIN
		SELECT @IndicatorTitle = Title FROM Stage WHERE ClientId = @ClientId AND Code = @IndicatorCode
	END

	RETURN @IndicatorTitle
END

GO