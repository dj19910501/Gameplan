IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionValueFormulaNullTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionValueFormulaNullTest
GO

CREATE PROCEDURE [DimensionValueFormulaNullTest]
	-- Add the parameters for the stored procedure here
@TestNumber int,
@TestName nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @TableName nvarchar(max)
	DECLARE @ValueFormula nvarchar(max)
	DECLARE @DimensionID int

	DECLARE QueryCursor cursor for
	select tablename, ValueFormula, id from dimension
	where ValueFormula is not null

	OPEN QueryCursor
	FETCH NEXT from QueryCursor 
	into @TableName, @ValueFormula, @DimensionID
	while @@FETCH_STATUS=0
		BEGIN

		DECLARE @QueryToRun nvarchar(max)

		SET @QueryToRun = 'if exists (select id from ' + @TableName +  ' A left outer join (' + @ValueFormula + ') B on A.id=b.id where b.id is null) BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Dimension with null values from Value formula'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;'


		FETCH NEXT from QueryCursor 
		into @TableName, @ValueFormula, @DimensionID
		END

	CLOSE QueryCursor
	DEALLOCATE QueryCursor

END


GO
