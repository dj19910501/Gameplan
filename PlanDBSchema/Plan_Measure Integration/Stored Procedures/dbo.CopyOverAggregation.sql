IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CopyOverAggregation') AND xtype IN (N'P'))
    DROP PROCEDURE CopyOverAggregation
GO



CREATE PROCEDURE [CopyOverAggregation] @DEBUG bit=0
AS
BEGIN
	SET NOCOUNT ON;
BEGIN TRY

DECLARE @CURQUERYFORERROR nvarchar(max)

DECLARE @VIEWNAME nvarchar(max)

DECLARE ViewCursor Cursor for 
select distinct tablename + '_VIEW' from Dimension where IsDeleted=0
OPEN ViewCursor
Fetch next from ViewCursor 
into @VIEWNAME
while @@FETCH_STATUS = 0
	BEGIN
	
	DECLARE @DROPVIEWSQL nvarchar(max)

	IF OBJECT_ID ('dbo.' + @VIEWNAME, 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)




		END

	IF OBJECT_ID ('dbo.' + @VIEWNAME + '_NEW', 'V') IS NOT NULL
		BEGIN
		SET @DROPVIEWSQL='Drop View ' + @VIEWNAME + '_New'
		print @DROPVIEWSQL
		SET @CURQUERYFORERROR=@DROPVIEWSQL
		exec(@DROPVIEWSQL)


		print 'After Rename'

		END

	Fetch next from ViewCursor 
	into @VIEWNAME
	END

CLOSE ViewCursor
DEALLOCATE ViewCursor

DECLARE @TABLENAME nvarchar(max)
DECLARE ValueCursor Cursor for 
select TableName + 'Value' from DynamicDimension_New
OPEN ValueCursor
Fetch next from ValueCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPVALUESQL nvarchar(max)
		SET @DROPVALUESQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPVALUESQL
		exec(@DROPVALUESQL)
		END

	Fetch next from ValueCursor 
	into @TABLENAME
	END

CLOSE ValueCursor
DEALLOCATE ValueCursor

DECLARE BaseValueCursor Cursor for 
select distinct DimensionValueTableName from DynamicDimension_New
OPEN BaseValueCursor
Fetch next from BaseValueCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPBaseValueSQL nvarchar(max)
		SET @DROPBaseValueSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPBaseValueSQL
		exec(@DROPBaseValueSQL)
		END

	Fetch next from BaseValueCursor 
	into @TABLENAME
	END

CLOSE BaseValueCursor
DEALLOCATE BaseValueCursor


DECLARE DynamicCursor Cursor for 
select TableName from DynamicDimension_New
OPEN DynamicCursor
Fetch next from DynamicCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPDynamicSQL nvarchar(max)
		SET @DROPDynamicSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPDynamicSQL
		exec(@DROPDynamicSQL)
		END

	Fetch next from DynamicCursor 
	into @TABLENAME
	END

CLOSE DynamicCursor
DEALLOCATE DynamicCursor


DECLARE BaseCursor Cursor for 
select distinct Dimensiontablename + '_Base' from DynamicDimension_New
OPEN BaseCursor
Fetch next from BaseCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + ']') AND TYPE IN (N'U'))
		BEGIN
		DECLARE @DROPBaseSQL nvarchar(max)
		SET @DROPBaseSQL='DROP TABLE [dbo].[' + @TABLENAME + ']'
		SET @CURQUERYFORERROR=@DROPBaseSQL
		exec(@DROPBaseSQL)
		END

	Fetch next from BaseCursor 
	into @TABLENAME
	END

CLOSE BaseCursor
DEALLOCATE BaseCursor

--Clear out foreignkeys on MeasureValue and DimensionValue (for #528)
CREATE TABLE #Keys
(
PKTABLE_QUALIFIER nvarchar(50),
PKTABLE_OWNER nvarchar(50),
PKTABLE_NAME nvarchar(50),
PKCOLUMN_NAME nvarchar(50),
FKTABLE_QUALIFIER nvarchar(50),
FKTABLE_OWNER nvarchar(50),
FKTABLE_NAME nvarchar(50),
FKCOLUMN_NAME nvarchar(50),
KEY_SEQ int,
UPDATE_RULE int,
DELETE_RULE int,
FK_NAME nvarchar(255),
PK_NAME nvarchar(255),
DEFERRABILITY int
)
insert #Keys exec sp_fkeys @pktable_name='DimensionValue'

insert #Keys exec sp_fkeys @pktable_name='MeasureValue'

DECLARE @FKTABLE nvarchar(255);
DECLARE @FKNAME nvarchar(255);

DECLARE Constraint_Cursor CURSOR FOR
select FKTABLE_NAME, FK_NAME from #Keys
OPEN Constraint_Cursor
FETCH NEXT FROM Constraint_Cursor
INTO @FKTABLE, @FKNAME
while @@FETCH_STATUS = 0
        Begin

        DECLARE @DROPCONSTRAINT nvarchar(1000);
        SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;

        SET @CURQUERYFORERROR=@DROPCONSTRAINT
        if @DEBUG=1
        BEGIN
            print @DROPCONSTRAINT
        END
        insert into logging select getDate(), left('Start: ' + @DROPCONSTRAINT,1000)
        execute sp_executesql @DROPCONSTRAINT
        insert into logging select getDate(), left('End: ' + @DROPCONSTRAINT,1000)
        --print @DROPCONSTRAINT
        --print '------------------------'

        FETCH NEXT FROM Constraint_Cursor
        INTO @FKTABLE, @FKNAME
        End
Close Constraint_Cursor
Deallocate Constraint_Cursor





IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue]') AND TYPE IN (N'U'))
	BEGIN
	drop table MeasureValue
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimensionValue]') AND TYPE IN (N'U'))
	BEGIN
	drop table DimensionValue
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension]') AND TYPE IN (N'U'))
	BEGIN
	drop table DynamicDimension
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.DynamicDimension_New', 'DynamicDimension'
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimensionValue_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.DimensionValue_New', 'DimensionValue'
	END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue_New]') AND TYPE IN (N'U'))
	BEGIN
	exec sp_rename 'dbo.MeasureValue_New', 'MeasureValue'
	END


DECLARE DynamicRenameCursor Cursor for 
select TableName from DynamicDimension
union
select DimensionValueTableName from DynamicDimension
union
select distinct DimensionTableName + '_Base' from DynamicDimension
OPEN DynamicRenameCursor
Fetch next from DynamicRenameCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @RenameDynamicSQL nvarchar(max)
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_New]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_New'', ''' + @TABLENAME + ''''
		SET @CURQUERYFORERROR=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_NewValue]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_NewValue'', ''' + @TABLENAME + 'Value'''
		SET @CURQUERYFORERROR=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	Fetch next from DynamicRenameCursor 
	into @TABLENAME
	END

CLOSE DynamicRenameCursor
DEALLOCATE DynamicRenameCursor


DECLARE @QueryToRun varchar(max)
DECLARE ViewCreateCursor Cursor for
select QueryToRun from AggregationQueries
OPEN ViewCreateCursor
FETCH next from ViewCreateCursor
into @QueryToRun
while @@FETCH_STATUS = 0
	BEGIN

	SET @CURQUERYFORERROR=@QueryToRun
	exec(@QueryToRun)

	FETCH next from ViewCreateCursor
	into @QueryToRun
	END
CLOSE ViewCreateCursor
DEALLOCATE ViewCreateCursor

END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CopyOverAggregation',@CURQUERYFORERROR);
	THROW;
END CATCH
END





GO
