IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetColor') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetColor
GO
-- =============================================
-- Author:		Manoj Limbachiya
-- Create date: 04Dec2014
-- Description:	Return the color code for graph element from color sequence table
-- =============================================
CREATE FUNCTION [GetColor](@SeqColorId INT)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @aa NVARCHAR(4000)
	SET @aa = ''
	
	SELECT @aa = 
		COALESCE (CASE WHEN @aa = ''
					   THEN ColorCode
					   ELSE @aa + ',' + ColorCode
				   END
				  ,'')
	  FROM ColorSequence where SequenceNumber = 
	  CASE WHen 
	  @SeqColorId IS NULL then 1 else @SeqColorId end  ORDER BY DisplayOrder 
	  if(LEN(@aa)<=0)
		SET @aa = '#73c4ee,#5693b3,#2384b8,#1a638a,#b04499,#cc91bf,#333333,#4d4d4d,#666666,#808080,#8cc63f,#39b54a,#009245,#006837,#f0be29,#f6d87f,#ff931e,#f0be29,#c1272d,#ed1c24'
	RETURN @aa;

END


GO
