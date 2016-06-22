IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[CopyOverAggregationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[CopyOverAggregationPartial]  
END
GO

CREATE PROCEDURE [dbo].[CopyOverAggregationPartial]  @DEBUG bit = 0
AS
BEGIN

--Steps
--1)  Update values in DimensionValue/Add New ones
--2)  Update values in MeasureValue/Add New ones
--3)  Drop extended columns that aren't needed anymore
--4)  Drop extended value tables that aren't needed anymore
--5)  Drop extended tables that aren't needed anymore
--6)  Drop basevalue tables that aren't needed anymore
--7)  Remove values from extended value tables that aren't needed anymore
--8)  Remove values from extended tables that aren't needed anymore
--9)  Remove values from MeasureValue that aren't needed anymore
--10)  Create any new extended tables
--11)  Create any new extended value tables
--12)  Update/add values to extended tables
--13)  Update/add values to extended value tables
--14)  Update/add rows to _Base tables
--15)  Update/add rows to _Base_Valuex tables
--16)  Update/add/remove rows to DynamicDimension
--17)  Drop views
--18)  Create views
--19)  Mark everything as clean


DECLARE @CurQueryForError nvarchar(max)
BEGIN TRY


--1)  Update values in DimensionValue/Add New ones
update DimensionValue
set value=v, DisplayValue=dv, OrderValue=ov from
(select id as did, value as v, DisplayValue as dv, OrderValue as ov from DimensionValue_New) A
where id=did

SET IDENTITY_INSERT dbo.DimensionValue ON

insert into DimensionValue (id, DimensionID, Value, DisplayValue, OrderValue)
select id, DimensionID, Value, DisplayValue, OrderValue from DimensionValue_New
where id not in (select id from DimensionValue)

SET IDENTITY_INSERT dbo.DimensionValue OFF

--2)  Update values in MeasureValue/Add New ones
update MeasureValue
set Measure=m, DimensionValue=dv, Value=v, RecordCount=rc from
(select id as did, Measure as m, dimensionValue as dv, value as v, RecordCount as rc from MeasureValue_New) A
where Measure=m and DimensionValue=dv

SET IDENTITY_INSERT dbo.MeasureValue ON

insert into MeasureValue (id, Measure, DimensionValue, Value, RecordCount)
select id, Measure, DimensionValue, Value, RecordCount from MeasureValue_New m1
where not exists (select m2.id from MeasureValue m2 where m2.Measure=m1.Measure and m2.DimensionValue=m1.DimensionValue)

SET IDENTITY_INSERT dbo.MeasureValue OFF

--3)  Drop extended columns that aren't needed anymore
--4)  Drop extended value tables that aren't needed anymore
--5)  Drop extended tables that aren't needed anymore
DECLARE @ExtendedToDrop table (TableName nvarchar(max), DimensionValueTableName nvarchar(max))

insert into @ExtendedToDrop
select TableName, DimensionValueTableName from DynamicDimension where DimensionTableName in (select DimensionTableName from DynamicDimension_New)
and TableName not in (select TableName from DynamicDimension_New)

while (select count(*) from @ExtendedToDrop) > 0
BEGIN
	DECLARE @TableNameToDrop nvarchar(max)= (select top 1 TableName from @ExtendedToDrop)
	DECLARE @DimensionValueTableName nvarchar(max)= (select DimensionValueTableName from @ExtendedToDrop where TableName=@TableNameToDrop)

	--Four steps here
	--1) Drop any constraint on the column
	--2) Drop the Column off of the value table
	--3) Drop the extended value table
	--4) Drop the extended table


	DECLARE @fkeys TABLE
	(
	id int identity(1,1),
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


	insert into @fkeys
	exec sp_fkeys @FKtable_Name=@DimensionValueTableName

	DECLARE @ConsToDelete nvarchar(max)=(
	select top 1 FK_Name from @fkeys where FKColumn_Name=@TableNameToDrop)


	if @ConsToDelete is not null
	BEGIN
		DECLARE @DROPCONSTRAINT nvarchar(max) = N'ALTER TABLE ' + @DimensionValueTableName + ' DROP CONSTRAINT ' + @ConsToDelete;

		SET @CurQueryForError=@DROPCONSTRAINT
		exec(@DROPCONSTRAINT)
		if @DEBUG=1
		BEGIN
			print @CurQueryForError
		END

	END

	--Check the column exists
	 IF COL_LENGTH(@DimensionValueTableName,@TableNameToDrop) IS NOT NULL
	 BEGIN

		SET @CurQueryForError='Alter table ' + @DimensionValueTableName + ' DROP COLUMN ' + @TableNameToDrop
		exec('Alter table ' + @DimensionValueTableName + ' DROP COLUMN ' + @TableNameToDrop)

		--Check table exists
		IF OBJECT_ID(@TableNameToDrop + 'Value', N'U') IS NOT NULL
		BEGIN
			SET @CurQueryForError='DROP TABLE ' + @TableNameToDrop + 'Value'
			exec('DROP TABLE ' + @TableNameToDrop + 'Value')
		END


		--Check table exists
		IF OBJECT_ID(@TableNameToDrop, N'U') IS NOT NULL
		BEGIN
			SET @CurQueryForError='Drop table ' + @TableNameToDrop
			exec('Drop table ' + @TableNameToDrop)
		END
	END



	delete from @ExtendedToDrop where TableName=@TableNameToDrop
END


--6)  Drop basevalue tables that aren't needed anymore
delete from @ExtendedToDrop

insert into @ExtendedToDrop
select DimensionValueTableName, '' from DynamicDimension where DimensionValueTableName not in
(select DimensionValueTableName from DynamicDimension_New) and DimensionTableName in
(select DimensionTableName from DynamicDimension_New)

while (select count(*) from @ExtendedToDrop) > 0
BEGIN
	SET @TableNameToDrop = (select top 1 TableName from @ExtendedToDrop)

	SET @CurQueryForError='Drop table ' + @TableNameToDrop
	IF OBJECT_ID(@TableNameToDrop, N'U') IS NOT NULL
	BEGIN
		exec('Drop table ' + @TableNameToDrop)
	END

	delete from @ExtendedToDrop where TableName=@TableNameToDrop
END



--7)  Remove values from extended value tables that aren't needed anymore
--8)  Remove values from extended tables that aren't needed anymore
--We need to find what values do not exist in the base tables anymore
DECLARE @ToDelete table(id int identity(1,1), TableName nvarchar(max), DimensionTableName nvarchar(max), DimensionValueTableName nvarchar(max))

insert into @ToDelete
select Tablename, DimensionTableName, DimensionValueTableName from DynamicDimension_New where containsdatedimension=1

while (select count(*) from @ToDelete)>0
BEGIN
	DECLARE @TableName nvarchar(max)
	--DECLARE @DimensionValueTableName nvarchar(max)
	DECLARE @MinID int = (select min(id) from @ToDelete)

	SET @TableName = (select TableName from @ToDelete where id=@MinID)
	SET @DimensionValueTableName = (select DimensionValueTableName from @ToDelete where id=@MinID)

	DECLARE @DeleteSubQuery nvarchar(max)=''
	
	SET @DeleteSubQuery='select id from ' + @TableName + ' where id not in (select ' + @TableName + ' from ' + @DimensionValueTableName + ' union select ' + @TableName + ' from ' + @DimensionValueTableName + '_New)'

	--If this is a new column
	If COL_LENGTH(@DimensionValueTableName,@TableName) IS NULL
	BEGIN
		SET @DeleteSubQuery='select id from ' + @TableName + ' where id not in (select ' + @TableName + ' from ' + @DimensionValueTableName + '_New)'
	END

	--Delete from the value table and then the regular extended table
	SET @CurQueryForError='delete from ' + @TableName + 'Value where ' + @TableName + ' in ( ' + @DeleteSubQuery + ')'
	exec('delete from ' + @TableName + 'Value where ' + @TableName + ' in ( ' + @DeleteSubQuery + ')')
	SET @CurQueryForError='delete from ' + @TableName + ' where id in ( ' + @DeleteSubQuery + ')'
	exec('delete from ' + @TableName + ' where id in ( ' + @DeleteSubQuery + ')')

	delete from @ToDelete where id=@MinID
END


--9)  Remove values from MeasureValue that aren't needed anymore
delete from MeasureValue where id in (
select m1.ID from MeasureValue m1
inner join DimensionValue d1 on m1.DimensionValue=d1.id
inner join Dimension d2 on d1.DimensionID=d2.id
where d2.IsDeleted=1)

--Also remove unneeded dimensionvalue entries
--Need to get rid of constraints on dimensionValueFirst

delete from @fkeys


insert into @fkeys
exec sp_fkeys @pktable_name='DimensionValue'


DECLARE @curRow int=1

while @curRow <= (select max(id) from @fkeys)
BEGIN
	DECLARE @FK_Name nvarchar(max)=(select FK_Name from @fkeys where id=@curRow)
	DECLARE @FKTABLE_Name nvarchar(max)=(select FKTABLE_NAME from @fkeys where id=@curRow)

	SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE_Name + ' DROP CONSTRAINT ' + @FK_NAME;

	SET @CurQueryForError=@DROPCONSTRAINT
	exec(@DROPCONSTRAINT)
	if @DEBUG=1
	BEGIN
		print @CurQueryForError
	END

	SET @curRow=@curRow+1
END

--Now remove the rows

SET @CurQueryForError='delete from DimensionValue where DimensionID in (select id from dimension where IsDeleted=1)'
exec('delete from DimensionValue where DimensionID in (select id from dimension where IsDeleted=1)')

--Add the constraints back in
--set @curRow=1
--while @curRow <= (select max(id) from @fkeys)
--BEGIN
	--SET @FK_Name=(select FK_Name from @fkeys where id=@curRow)
	--SET @FKTABLE_Name=(select FKTABLE_NAME from @fkeys where id=@curRow)
	--DECLARE @FKColumn_Name nvarchar(max)=(select FKColumn_Name from @fkeys where id=@curRow)

	--SET @DROPCONSTRAINT = 'Alter table ' + @FKTABLE_NAME + ' add constraint ' + @FK_Name + ' foreign key (' + @FKColumn_Name + ') REFERENCES dimensionValue(id)'

	--SET @CurQueryForError=@DROPCONSTRAINT
	--exec(@DROPCONSTRAINT)

	--SET @curRow=@curRow+1
--END


--10)  Create any new extended tables
--11)  Create any new extended value tables
DECLARE DynamicRenameCursor Cursor for 
select tablename from DynamicDimension_New
where tablename not in (select tablename from DynamicDimension)
OPEN DynamicRenameCursor
Fetch next from DynamicRenameCursor 
into @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @RenameDynamicSQL nvarchar(max)
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_New]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_New'', ''' + @TABLENAME + ''''
		SET @CurQueryForError=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[' + @TABLENAME + '_NewValue]') AND TYPE IN (N'U'))
		BEGIN
		SET @RenameDynamicSQL='exec sp_rename ''dbo.' + @TABLENAME + '_NewValue'', ''' + @TABLENAME + 'Value'''
		SET @CurQueryForError=@RenameDynamicSQL
		exec(@RenameDynamicSQL)
		END

	Fetch next from DynamicRenameCursor 
	into @TABLENAME
	END

CLOSE DynamicRenameCursor
DEALLOCATE DynamicRenameCursor

--12)  Update/add values to extended tables
--13)  Update/add values to extended value tables


--First add the values to the extended tables
DECLARE @TablesToUpdate table (id int identity(1,1), tablename nvarchar(max), dimensions int)


insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	DECLARE @Dimensions int = (select dimensions from @TablesToUpdate where id=@MinID)


	DECLARE @ToInsert nvarchar(max) = 'Insert into ' + @TableName + ' select '

	DECLARE @d int=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd' + cast(@d as nvarchar) 

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ', '
		END

		SET @d=@d+1
	END

	SET @ToInsert= @ToInsert + ' from ' + @TableName + '_New where id in ( select d1.id from ' + @TableName + '_New d1 left outer join ' + @TableName + ' d2 on '

	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd1.d' + cast(@d as nvarchar) + '=d2.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END

	SET @ToInsert = @ToInsert + ' where d2.id is null)'

	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--Add the values to the value tables
delete from @TablesToUpdate
insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	SET @Dimensions = (select dimensions from @TablesToUpdate where id=@MinID)


	SET @ToInsert  = 'Insert into ' + @TableName + 'Value select d3.id, Measure, Value, Rows from ' + @TableName + '_NewValue d1 ' +
								' inner join ' + @TableName + '_New d2 on d1.' + @TableName + '=d2.id ' +
								' inner join ' + @TableName + ' d3 on '
	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd2.d' + cast(@d as nvarchar) + '=d3.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END


	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--Now we need to update the values in the value table
delete from @TablesToUpdate
insert into @TablesToUpdate
select tablename, Dimensions from DynamicDimension_New


while (select count(*) from @TablesToUpdate) >0
BEGIN
	SET @MinID = (select min(id) from @TablesToUpdate)
	SET @TableName = (select tablename from @TablesToUpdate where id=@MinID)
	SET @Dimensions = (select dimensions from @TablesToUpdate where id=@MinID)


	SET @ToInsert  = 'Update ' + @TableName + 'Value set Measure=m, Value=v, Rows=r from (select d3.id as did, Measure as m, Value as v, Rows as r from ' + @TableName + '_NewValue d1 ' +
								' inner join ' + @TableName + '_New d2 on d1.' + @TableName + '=d2.id ' +
								' inner join ' + @TableName + ' d3 on '

	SET @d=1

	while @d<=@Dimensions
	BEGIN
		SET @ToInsert = @ToInsert + 'd2.d' + cast(@d as nvarchar) + '=d3.d' + cast(@d as nvarchar)

		if @d<@Dimensions
		BEGIN
			SET @ToInsert = @ToInsert + ' and '
		END

		SET @d=@d+1
	END

	SET @ToInsert=@ToInsert + ') A where id=did'


	SET @CurQueryForError=@ToInsert
	exec(@ToInsert)

	delete from @TablesToUpdate where id=@MinID
END


--14)  Update/add rows to _Base tables
--Add new rows first
DECLARE @BaseInsert table (id int identity(1,1), tablename nvarchar(max))

insert into @BaseInsert
select distinct DimensionTableName from DynamicDimension_New

while (select count(*) from @BaseInsert) > 0
BEGIN
	DECLARE @curId int = (select min(id) from @BaseInsert)

	DECLARE @CurTable nvarchar(max)=(select tablename from @BaseInsert where id=@curId)


	DECLARE @ColList table (id int identity(1,1), value nvarchar(max))

	delete from @ColList

	insert into @ColList
	SELECT COLUMN_NAME
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = @CurTable+'_Base_New'

	select * from @ColList

	DECLARE @CurCol int =1
	DECLARE @ColEnum nvarchar(max)=''
	set @ColEnum=''

	while @CurCol <= (select max(id) from @ColList)
	BEGIN
		DECLARE @ThisCol nvarchar(max)=(select Value from @ColList where id=@CurCol)

		if len(@ColEnum) > 0
		BEGIN
			SET @ColEnum=@ColEnum+','
		END

		SET @ColEnum=@ColEnum + @ThisCol

		SET @CurCol=@CurCol+1
	END



	DECLARE @ToRun nvarchar(max)=replace('insert into %TABLENAME%_Base (%COLLIST%) select %COLLIST% from %TABLENAME%_Base_New where id not in (select id from %TABLENAME%_Base)','%TABLENAME%',@CurTable)

	SET @ToRun = replace(@ToRun,'%COLLIST%',@ColEnum)

	SET @CurQueryForError=@ToRun
	exec(@ToRun)

	delete from @BaseInsert where id=@curId
END



--Now we need to update values in the _Base tables
DECLARE @BaseUpdate table(id int identity(1,1), tablename nvarchar(max), columnName nvarchar(max), alias nvarchar(max))

insert into @BaseUpdate
select distinct d1.DimensionTableName, 'DimensionValue' + cast(d2.id as nvarchar), 'd' + cast(d2.id as nvarchar) from DynamicDimension_New d1
inner join Dimension d2 on d1.DimensionTableName=d2.TableName 
where d2.IsDeleted=0
order by d1.DimensionTableName

DECLARE @Columns nvarchar(max)=''
DECLARE @ColumnsSub nvarchar(max)=''
SET @curID = (select min(id) from @BaseUpdate)
SET @CurTable =(select tablename from @BaseUpdate where id=@curID)

DECLARE @BaseColumns table(id int identity(1,1), tablename nvarchar(max), columnNames nvarchar(max), subColumns nvarchar(max))

while @curID <= (select max(id) from @BaseUpdate) + 1
BEGIN

	DECLARE @RowTable nvarchar(max)=(select tablename from @BaseUpdate where id=@curID)

	if @RowTable<>@CurTable or @CurID=(select max(id) from @BaseUpdate)
	BEGIN
		insert into @BaseColumns select @CurTable, @Columns, @ColumnsSub

		SET @CurTable=@RowTable
		SET @Columns=''
		set @ColumnsSub=''
	END

	if len(@Columns)>0
	BEGIN

		set @Columns = @Columns + ', '
		set @ColumnsSub = @ColumnsSub + ', '
	END

	SET @Columns = @Columns + (select columnName + '=' + alias from @BaseUpdate where id=@curID)
	SET @ColumnsSub = @ColumnsSub + (select columnName + ' as ' + alias from @BaseUpdate where id=@curID)

	SET @curID=@CurID+1
END


SET @curID=(select min(id) from @BaseColumns)

while @curID<=(select max(id) from @BaseColumns)
BEGIN

	DECLARE @QueryToRun nvarchar(max)='update %TABLENAME%_Base
				set %COLUMNS%
				from (select id as i, %ALIAS% from %TABLENAME%_Base_New) A
				where id=i and Dirty=1'

	SET @Columns = (select columnNames from @BaseColumns where id=@curID)
	SET @ColumnsSub = (select subColumns from @BaseColumns where id=@curID)
	set @CurTable = (select tablename from @BaseColumns where id=@curID)

	SET @QueryToRun = Replace(@QueryToRun,'%TABLENAME%',@CurTable)
	SET @QueryToRun = Replace(@QueryToRun,'%COLUMNS%',@Columns)
	SET @QueryToRun = Replace(@QueryToRun,'%ALIAS%',@ColumnsSub)

	--print @QueryToRun

	SET @CurQueryForError=@QueryToRun

	exec(@QueryToRun)

	SET @curID=@curID+1
END


--15)  Update/add rows to _Base_Valuex tables
--Let's add the new rows first
DECLARE @ExtendedInsertData table(id int identity(1,1), QueryToRun nvarchar(max))

insert into @ExtendedInsertData
select distinct 'insert into ' + DimensionValueTableName + ' (' + DimensionTableName + ') select ' + DimensionTableName + ' from ' + DimensionValueTableName + '_New where ' +
 DimensionTableName + ' not in(
select ' + DimensionTableName + ' from ' + DimensionValueTableName + ')' from DynamicDimension_New


while (select count(*) from @ExtendedInsertData)>0
BEGIN
	
	SET @CurID=(select min(id) from @ExtendedInsertData)
	SET @ToRun=(select QueryToRun from @ExtendedInsertData where id=@CurID)

	SET @CurQueryForError=@ToRun
	exec(@ToRun)

	delete from @ExtendedInsertData where id=@CurID

END


--Now update the values



DECLARE @BaseUpdate2 table(id int identity(1,1), tablename nvarchar(max), columnName nvarchar(max), alias nvarchar(max), basename nvarchar(max))

insert into @BaseUpdate2
select d1.DimensionValueTableName, d1.tablename, d1.tablename + '_updated', DimensionTableName from DynamicDimension_New d1
where containsdatedimension=1
order by d1.DimensionValueTableName

SET @Columns =''
SET @ColumnsSub =''
DECLARE @BaseName nvarchar(max)=''
SET @curID = (select min(id) from @BaseUpdate2)
SET @CurTable =(select tablename from @BaseUpdate2 where id=@curID)

DECLARE @BaseColumns2 table(id int identity(1,1), tablename nvarchar(max), columnNames nvarchar(max), subColumns nvarchar(max), basename nvarchar(max))

SET @BaseName=(select basename from @BaseUpdate2 where id=@curID)


while @curID <= (select max(id) from @BaseUpdate2) + 1
BEGIN

	SET @RowTable =(select tablename from @BaseUpdate2 where id=@curID)

	if @RowTable<>@CurTable or @CurID=(select max(id) from @BaseUpdate)
	BEGIN
		insert into @BaseColumns2 select @CurTable, @Columns, @ColumnsSub, @BaseName

		SET @CurTable=@RowTable
		SET @Columns=''
		set @ColumnsSub=''
		SET @BaseName=(select basename from @BaseUpdate2 where id=@curID)
	END

	if len(@Columns)>0
	BEGIN

		set @Columns = @Columns + ', '
		set @ColumnsSub = @ColumnsSub + ', '
	END

	SET @Columns = @Columns + (select columnName + '= isnull(' + alias + ','+ columnName + ') ' from @BaseUpdate2 where id=@curID)
	SET @ColumnsSub = @ColumnsSub + (select columnName + ' as ' + alias from @BaseUpdate2 where id=@curID)

	SET @curID=@CurID+1
END


SET @curID=(select min(id) from @BaseColumns)

while @curID<=(select max(id) from @BaseColumns)
BEGIN

	SET @QueryToRun ='update %TABLENAME%
				set %COLUMNS%
				from (select %BASENAME% as i, %ALIAS% from %TABLENAME%_New) A
				where %BASENAME%=i'

	SET @Columns = (select columnNames from @BaseColumns2 where id=@curID)
	SET @ColumnsSub = (select subColumns from @BaseColumns2 where id=@curID)
	set @CurTable = (select tablename from @BaseColumns2 where id=@curID)
	set @BaseName = (select basename from @BaseColumns2 where id=@curID)

	SET @QueryToRun = Replace(@QueryToRun,'%TABLENAME%',@CurTable)
	SET @QueryToRun = Replace(@QueryToRun,'%COLUMNS%',@Columns)
	SET @QueryToRun = Replace(@QueryToRun,'%ALIAS%',@ColumnsSub)
	SET @QueryToRun = Replace(@QueryToRun,'%BASENAME%',@BaseName)

	--print @QueryToRun

	SET @CurQueryForError=@QueryToRun
	exec(@QueryToRun)

	SET @curID=@curID+1
END



--16)  Update/add/remove rows to DynamicDimension
insert into DynamicDimension
select TableName, Dimensions, DimensionTableName, ComputeAllValues, DimensionValueTableName, ContainsDateDimension from DynamicDimension_New where TableName not in (select TableName from DynamicDimension)

delete from DynamicDimension where id in(
select id from DynamicDimension where tablename not in (select tablename from DynamicDimension_New) and DimensionTableName in (select DimensionTableName from DynamicDimension_New)
)



--17)  Drop views
DECLARE @VIEWNAME nvarchar(max)

DECLARE ViewCursor Cursor for 
select distinct Dimensiontablename + '_VIEW' from DynamicDimension_New OPEN ViewCursor
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



--18)  Create views
SET @QueryToRun=''
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


--19)  Mark everything as clean
SET @QueryToRun=''
DECLARE CleanCursor Cursor for
select distinct 'update ' + DimensionTableName + '_Base set dirty=0' from DynamicDimension_New
OPEN CleanCursor
FETCH next from CleanCursor
into @QueryToRun
while @@FETCH_STATUS = 0
	BEGIN

	SET @CURQUERYFORERROR=@QueryToRun
	exec(@QueryToRun)

	FETCH next from CleanCursor
	into @QueryToRun
	END
CLOSE CleanCursor
DEALLOCATE CleanCursor

END TRY

BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CopyOverAggregationPartial',@CURQUERYFORERROR);
	THROW;
END CATCH


END
GO

