IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionInvalidFormulaTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionInvalidFormulaTest
GO


CREATE PROCEDURE [DimensionInvalidFormulaTest]
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

	DECLARE @Valid int
	CREATE TABLE  #TempTableStore (Display nvarchar(max), Value nvarchar(max), OrderBy nvarchar(max));
	SET @Valid=1

	SEt @DimensionFormula = ' Insert into #TempTableStore ' + @DimensionFormula

	BEGIN TRY
	 exec sp_executesql @DimensionFormula
	END TRY
	BEGIN CATCH
	 SET @Valid=0
	END CATCH

	DROP TABLE #TempTableStore


	
	if @Valid=0
		BEGIN
		SET @CheckSQL = 'insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with invalid formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''');' 

		exec sp_executesql @CheckSQL
		END


	FETCH NEXT FROM DimensionCursor
	into @DimensionID, @DimensionFormula
	END


Close DimensionCursor
Deallocate DimensionCursor
RETURN
END



GO
