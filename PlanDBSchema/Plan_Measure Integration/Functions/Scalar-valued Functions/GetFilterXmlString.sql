IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetFilterXmlString') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetFilterXmlString
GO
--Create procedure and function

-- =============================================
-- Author:		Nandish Shah
-- Create date: 15/06/2016
-- Description:	Get Filter XML String
-- =============================================
CREATE FUNCTION [dbo].[GetFilterXmlString]
(	
	-- Add the parameters for the function here
	@FilterVAlues NVARCHAR(MAX),
	@DimensionName NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @FilterXML NVARCHAR(MAX)
	DECLARE @TempDimensionTable AS TABLE (Id INT, DimensionIds INT)
	INSERT INTO @TempDimensionTable
	select ROW_NUMBER() OVER (ORDER BY dimension) AS RowNo,dimension from [dbo].[fnSplitString](@DimensionName, 'D')

	DECLARE @TempDimensionValueTable AS TABLE (Id INT, DimensionValues NVARCHAR(MAX))
	INSERT INTO @TempDimensionValueTable
	select ROW_NUMBER() OVER (ORDER BY dimension) AS RowNo,dimension from [dbo].[fnSplitString](@FilterVAlues, ',')

	DECLARE @intFlag INT
	DECLARE @TotalRow INT

	SET @intFlag = 1
	SET @TotalRow = (select COUNT(dimension) from [dbo].[fnSplitString](@FilterVAlues, ','))

	WHILE (@intFlag <= @TotalRow)
	BEGIN
		DECLARE @DimId INT
		DECLARE @DimVal INT
		DECLARE @FilterIndex INT

		SET @DimId = (SELECT LEFT(DimensionValues, CHARINDEX(':',DimensionValues)-1) FROM @TempDimensionValueTable WHERE Id = @intFlag)
		SET @DimVal = (SELECT RIGHT(DimensionValues,LEN(DimensionValues)-CHARINDEX(':',DimensionValues)) FROM @TempDimensionValueTable WHERE Id = @intFlag)
		SET @FilterIndex = (SELECT Id FROM @TempDimensionTable WHERE DimensionIds = @DimId)

		IF EXISTS (SELECT * FROM @TempDimensionTable WHERE DimensionIds = @DimId)
		BEGIN
			SET @FilterXML = CONCAT(@FilterXML, '<filter ID="' + CAST(@FilterIndex AS NVARCHAR(MAX)) + '">' + CAST(@DimVal AS NVARCHAR(MAX)) + '</filter>')
		END

		SET @intFlag = @intFlag + 1
	END
	RETURN @FilterXML
END
