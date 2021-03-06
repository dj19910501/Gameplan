IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionNullTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionNullTest
GO


CREATE PROCEDURE [DimensionNullTest]
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @DimensionID int
DECLARE @DimensionFormula nvarchar(max)


DECLARE DimensionCursor Cursor for
select id, formula from dimension where ValueFormula is null
Open DimensionCursor
FETCH NEXT FROM DimensionCursor
into @DimensionID, @DimensionFormula
while @@FETCH_STATUS = 0
	BEGIN
	DECLARE @CheckSQL nvarchar(max)

	
	SET @CheckSQL = 'if exists (Select 1 from (' + @DimensionFormula + ') A where value is null or display is null or orderby is null ) BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with null values from formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;' 

	print @CheckSQL
	exec sp_executesql @CheckSQL


	FETCH NEXT FROM DimensionCursor
	into @DimensionID, @DimensionFormula
	END


Close DimensionCursor
Deallocate DimensionCursor
RETURN
END


GO
