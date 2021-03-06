IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[DimensionValuePopulate]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[DimensionValuePopulate]  
END
GO


CREATE PROCEDURE [dbo].[DimensionValuePopulate]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY


	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MeasureValue_New]') AND type in (N'U'))
		BEGIN
			DROP tABLE MeasureValue_New 
		END

DECLARE @CREATEMEASUREVALUE varchar(max)

SET @CREATEMEASUREVALUE = 
'CREATE TABLE [dbo].[MeasureValue_New](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Measure] [int] NOT NULL,
	[DimensionValue] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[RecordCount] [float] NULL,
	CONSTRAINT [PK_MeasureValue_New' + cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar) + '] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]'

exec(@CREATEMEASUREVALUE)
exec('delete from dimensionvalue_New')

--reset the identity seeds
DBCC CHECKIDENT('MeasureValue_New', RESEED, 0)
DBCC CHECKIDENT('DimensionValue_New', RESEED, 0)

DECLARE @DID varchar(30), @DFORMULA varchar(max);
DECLARE @COMPUTEALLVALUES bit, @STARTDATE datetime, @ENDDATE datetime;
DECLARE @ISDATEDIMENSION bit;

DECLARE Dimension_Cursor CURSOR FOR
select dimension.id, Formula, isnull(ComputeAllValues,0), isnull(d1.DateTime,cast('1/1/2010' as datetime)), isnull(d2.DateTime,cast(cast(getdate() as date) as datetime)), isnull(isDateDimension,0) from dimension
left outer join dimtime d1 on d1.DateTime=StartDate
left outer join dimtime d2 on d2.DateTime=EndDate
where IsDeleted=0
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
while @@FETCH_STATUS = 0
	Begin
		declare @Insert_Query varchar(max);


		--We need to know if this table has a compute all values set on it and if this is a date dimension.  If so, we need to incorporate the full range of values.
		--We will require date dimensions to use the DimTime ids as values in order to be supported.

		if @ISDATEDIMENSION=0 or @COMPUTEALLVALUES=0
		BEGIN
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (' + @DFORMULA + ') B cross join (select ' + @DID + ' as ID) A;';
		END
		ELSE
		BEGIN --If this is a data dimension that we need to populate all values for
			DECLARE @SDATE datetime, @EDATE datetime;

			SET @SDATE=(select [DateTime] from dimTime where id=@STARTDATE)
			SET @EDATE=(select dateadd(day, 1, [DateTime]) from dimTime where id=@ENDDATE) -- To avoid missing the last date
			set @Insert_Query=N' insert into dimensionvalue_New (Value, DisplayValue, OrderValue, DimensionID) select * from (select id,[DateTime],id as id2 from dimtime where [DateTime] between ''' + cast(cast(@SDATE as date) as varchar) + ''' and ''' + cast(cast(@EDATE as date) as varchar) + ''') B cross join (select ' + @DID + ' as ID) A;';
		END

		--print @Insert_Query;
	
		SET @CURQUERYFORERROR=@Insert_Query
		if @DEBUG=1
		BEGIN
			print @Insert_Query
		END
		exec(@Insert_Query);
		--print @Insert_Query;
		--print '--**************************'
		FETCH NEXT FROM Dimension_Cursor
		INTO @DID, @DFORMULA, @COMPUTEALLVALUES, @STARTDATE, @ENDDATE, @ISDATEDIMENSION
	end;
Close Dimension_Cursor
Deallocate Dimension_Cursor




DECLARE @COLUMNNAME varchar(100);
DECLARE @DIMENSIONID varchar(10);
DECLARE @COLUMNVALUE varchar(100);
DECLARE @VALUEFORMULA varchar(max);
DECLARE @TABLENAME varchar(250);

DECLARE Dimension_Cursor CURSOR FOR
select distinct d2.TableName as tablename, 'DimensionValue' + cast(d2.id as varchar) as colname, d2.id as did, d2.ColumnName, d2.ValueFormula from dimensionvalue_new d1
inner join dimension d2 on d1.DimensionID=d2.id
where d2.IsDeleted=0
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
while @@FETCH_STATUS = 0
	BEGIN

	DECLARE @UPDATEQUERY varchar(max);


	DECLARE @rowCheckQuery varchar(max) = 'select count(*) from ' +  @TABLENAME + '_Base_New';
	DECLARE @rowCheck table (c int)
	delete from @rowCheck
	insert into @rowCheck 
	exec(@rowCheckQuery)

	DECLARE @rowCheckCount int = (select c from @rowCheck)

	if @rowCheckCount = 0
		BEGIN
		--Populate the base_new table if it hasn't been populated before
		if @DEBUG=1
		BEGIN
			print 'insert into ' + @TABLENAME + '_Base_New (id) select id from ' + @TABLENAME
		END

		exec('insert into ' + @TABLENAME + '_Base_New (id) select id from ' + @TABLENAME)
		END

	--We need to check if the column exists.  If not, we need to create it.  This should never happen.
	DECLARE @exists bit;

	set @exists= (select cast(count(*) as bit) from sys.columns 
            where Name = N'' + @COLUMNNAME + '' and Object_ID = Object_ID(N'' + @TABLENAME  + '_Base_New'));


	if @exists=0
		BEGIN
		DECLARE @ADDQUERY varchar(max);
		set @ADDQUERY = N'ALTER TABLE ' + @TABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		exec(@ADDQUERY)
		END


--select d1.id as bid, d2.id as did from DummyContacts d1
--inner join DimensionValue d2 on d1.Industry=d2.Value and d2.DimensionID=45
	
	SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New 
		set ' + @COLUMNNAME + ' = A.did
		from
		(select d1.id as bid, d2.id as did from ' + @TABLENAME + ' d1 
		inner join DimensionValue_New d2 on d1.' + @COLUMNVALUE + '=d2.Value and d2.DimensionID=' + @DIMENSIONID + '
		
		) A
		where A.bid=id'




	if @VALUEFORMULA is not null 
		BEGIN
		SET @UPDATEQUERY=N'update ' + @TABLENAME + '_Base_New
			set ' + @COLUMNNAME + ' = did 
			from
			(select  joinID,  D.ID as did from
			(' + @VALUEFORMULA + ') A inner join DimensionValue_New D on D.Value=A.Value
			where D.DimensionID=' + @DIMENSIONID + ') B
			where B.joinID=id'
		END

	--print @UPDATEQUERY
	SET @CURQUERYFORERROR=@UPDATEQUERY
	if @DEBUG=1
	BEGIN
		print @UPDATEQUERY
	END
	exec(@UPDATEQUERY)
	--print '--/////////////////////////'


	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @COLUMNNAME, @DIMENSIONID, @COLUMNVALUE, @VALUEFORMULA
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor

--We need to make sure there is a dirty bit on all of the base tables so partial aggregation will run


DECLARE Dimension_Cursor CURSOR FOR
select distinct DimensiontableName + '_Base_New' from DynamicDimension_New
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	set @exists= (select cast(count(*) as bit) from sys.columns 
			where Name = N'dirty' and Object_ID = Object_ID( @TABLENAME  ));


	if @exists=0
		BEGIN
		set @ADDQUERY = N'ALTER TABLE ' + @TABLENAME + ' ADD dirty bit';
		set @CURQUERYFORERROR=@ADDQUERY
		exec(@ADDQUERY)
		SET @UPDATEQUERY = N'Update ' + @TABLENAME + ' set dirty=0';
		set @CURQUERYFORERROR=@UPDATEQUERY
		exec(@UPDATEQUERY)
		END
	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME
	END
Close Dimension_Cursor
Deallocate Dimension_Cursor


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DimensionValuePopulate',@CURQUERYFORERROR);
	THROW;
END CATCH

END


GO