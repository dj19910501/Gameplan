IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionNonNumericDateOrderByTest') AND xtype IN (N'P'))
    DROP PROCEDURE DimensionNonNumericDateOrderByTest
GO


CREATE PROCEDURE [DimensionNonNumericDateOrderByTest]
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
	DECLARE @Formula nvarchar(max)
	DECLARE @DimensionID int

	DECLARE QueryCursor cursor for
	select tablename, Formula, id from dimension
	where IsDateDimension=0

	OPEN QueryCursor
	FETCH NEXT from QueryCursor 
	into @TableName, @Formula, @DimensionID
	while @@FETCH_STATUS=0
		BEGIN

		DECLARE @QueryToRun nvarchar(max)

		SET @QueryToRun = 'if (select count(*) from  (' + @Formula + ') A where isnumeric(A.OrderBy=0) ) > 0 BEGIN insert into #Results (TestNumber, TestName, ErrorMessage, ErrorInformation) values (' + cast(@TestNumber as nvarchar) + ', ''' + @TestName + ''', ''Date Dimension with non-numeric order by'', ''On Dimension ' + cast(@DimensionID as nvarchar) + ''') END;'


		FETCH NEXT from QueryCursor 
		into @TableName, @Formula, @DimensionID
		END

	CLOSE QueryCursor
	DEALLOCATE QueryCursor

END




GO
