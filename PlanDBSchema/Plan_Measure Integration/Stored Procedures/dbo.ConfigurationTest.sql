IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ConfigurationTest') AND xtype IN (N'P'))
    DROP PROCEDURE ConfigurationTest
GO


CREATE PROCEDURE [ConfigurationTest] 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	DECLARE @TestNum int;
	DECLARE @TestName nvarchar(max);
	DECLARE @TestQuery nvarchar(max);
	DECLARE @TestStoredProcedure nvarchar(max)
	CREATE TABLE #Results (TestNumber int, TestName nvarchar(max), ErrorMessage nvarchar(max), ErrorInformation nvarchar(max))

	DECLARE TestCursor cursor for
	select id, TestCaseName, TestCaseQuery, TestStoredProcedure from ConfigurationTestCases where isDeleted=0
	OPEN TestCursor
	FETCH NEXT FROM TestCursor
	INTO @TestNum, @TestName, @TestQuery, @TestStoredProcedure
	while @@FETCH_STATUS = 0
		BEGIN

		if @TestQuery is not null
			BEGIN
			DECLARE @QueryToRun nvarchar(max)

			SET @QueryToRun = 'Insert into #Results select ' + cast(@TestNum as nvarchar) + ', ''' + @TestName + ''', ErrorMessage, ErrorInformation from ( ' + @TestQuery + ') A';

			print @QueryToRun
			exec sp_executesql @QueryToRun
			END

		if @TestStoredProcedure is not null
			BEGIN
			SET @QueryToRun = 'exec ' + @TestStoredProcedure + ' @TestNumber=' + cast(@TestNum as nvarchar)  + ', @TestName=''' + @TestName + ''''

			print @QueryToRun
			exec sp_executesql @QueryToRun
			END


		FETCH NEXT FROM TestCursor
		INTO @TestNum, @TestName, @TestQuery, @TestStoredProcedure
		END

	Close TestCursor
	Deallocate TestCursor

	select * from #Results
	RETURN

	Drop Table #Results
END


GO
