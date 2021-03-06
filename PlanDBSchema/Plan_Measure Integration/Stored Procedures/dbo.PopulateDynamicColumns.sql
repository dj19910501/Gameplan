IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'PopulateDynamicColumns') AND xtype IN (N'P'))
    DROP PROCEDURE PopulateDynamicColumns
GO


CREATE PROCEDURE [PopulateDynamicColumns]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY




DECLARE @TABLENAME varchar(max);
DECLARE @DIMENSIONS int;
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @DIMENSIONVALUETABLENAME varchar(max);
DECLARE @LASTDIMENSIONVALUETABLENAME varchar(max);

SET @LASTDIMENSIONVALUETABLENAME=null;


DECLARE Dimension_Cursor CURSOR FOR
select TableName, Dimensions, DimensionTableName, DimensionValueTableName + '_New' from DynamicDimension_New
where containsDateDimension=1
 order by DimensionValueTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
while @@FETCH_STATUS = 0
	Begin

	--If this is the first row with the DimensionValueTableName value, we need to populate that table
	if @LASTDIMENSIONVALUETABLENAME is null or @LASTDIMENSIONVALUETABLENAME<>@DIMENSIONVALUETABLENAME
		BEGIN
		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME

		DECLARE @POPSQL varchar(max);
		SET @POPSQL = 'Insert into ' + @DIMENSIONVALUETABLENAME + ' ( ' + @DIMENSIONTABLENAME + ' ) select id from ' + @DIMENSIONTABLENAME + ';';
		
		SET @CURQUERYFORERROR = @POPSQL

		exec(@POPSQL)
		

		END

	DECLARE @POPULATEQUERY varchar(max); -- To populate the target table (e.g. CampaignValue0)
	DECLARE @POPULATELASTJOIN varchar(max);
	DECLARE @dCount int;
	DECLARE @dIndex int;
	DECLARE @dNextIndex int;

	set @POPULATEQUERY = N'Update ' + @DIMENSIONVALUETABLENAME + ' set ' + @TABLENAME + '=' + @TABLENAME + 'Val from (select c1.ID as CID, d1.id as ' + @TABLENAME + 'Val from ' + 
		@DIMENSIONTABLENAME + '_Base_New c1'
	set @POPULATELASTJOIN = N' inner join ' + @TABLENAME + '_New d1 on ';


	set @dCount=1;
	set @dIndex = CHARINDEX('d',@TABLENAME);
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

	while @dCount <= @DIMENSIONS
		BEGIN
			--select @TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1, @dIndex, @dNextIndex
			set @POPULATELASTJOIN = @POPULATELASTJOIN + ' c1.DimensionValue' + SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1) + '=d1.d' + cast(@dCount as varchar) + ' '


			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@TABLENAME)+1;
				END

			if @dCount <= @DIMENSIONS
				BEGIN
				set @POPULATELASTJOIN = @POPULATELASTJOIN + ' and ';

				END
		END
	set @POPULATEQUERY = @POPULATEQUERY + @POPULATELASTJOIN + ') a where a.CID=' + @DIMENSIONTABLENAME;

	--select @POPULATEQUERY

	
	SET @CURQUERYFORERROR=@POPULATEQUERY
	if @DEBUG=1
	BEGIN
		print @POPULATEQUERY
	END

	exec(@POPULATEQUERY)

	--print @POPULATEQUERY
	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @DIMENSIONVALUETABLENAME
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor


delete from MeasureValue_New

--Let's populate the MeasureValue table now
DECLARE @MEASUREID int;
DECLARE @AGGREGATIONQUERY varchar(max);
DECLARE @MEASURETABLENAME varchar(max);
DECLARE @DIMENSIONID varchar(max);
DECLARE @COMPUTEALLVALUES bit;
DECLARE @COMPUTEALLVALUESFORMULA varchar(max);
--reuse DIMENSIONID



DECLARE MEASURE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW') , MeasureTableName, Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW') from Measure
inner join Dimension on MeasureTableName=TableName
where Dimension.IsDeleted=0 and Measure.IsDeleted=0
OPEN MEASURE_CURSOR
FETCH NEXT FROM MEASURE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA

while @@FETCH_STATUS = 0
	BEGIN

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END
	ELSE  --We need to deal with the computeallvalues stuff here as well
	BEGIN
		

		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END


		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#', 'DimensionValue' + cast(@DIMENSIONID as varchar))
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#', 'DimensionValue_New')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#', 'id')
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#', 'd1.DimensionID=' + cast(@DIMENSIONID as varchar))

		SET @AGGREGATIONQUERY = N'insert into MeasureValue_New select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END

	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASURE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA
	END;
CLOSE MEASURE_CURSOR
DEALLOCATE MEASURE_CURSOR



--Now we need to populate the extended dimension value tables
--We need to deal with the computeallvalues stuff here as well

DECLARE @LastDimensionTableName varchar(500)=null;
DECLARE @UseRowCountFromFormula bit=0;

DECLARE MEASUREVALUE_CURSOR CURSOR FOR
select Measure.id, replace(AggregationQuery,'_VIEW','_VIEW_NEW'), D1.TableName + '_New', Dimension.id, 

case 
	when Dimension.ComputeAllValues is null then 0
	when Measure.ComputeAllValuesFormula is null then 0
	when Dimension.ComputeAllValues=1 and Measure.ComputeAllValues=1 then 1
	else 0
End, replace(ComputeAllValuesFormula,'_VIEW','_VIEW_NEW'), D1.Dimensions, D1.DimensionValueTableName + '_New', UseRowCountFromFormula from Measure
inner join DynamicDimension_New d1 on MeasureTableName=DimensionTableName
left outer join Dimension  on Dimension.TableName=DimensionTableName and Dimension.ComputeAllValues=1 and Dimension.IsDeleted=0
where containsdatedimension=1 and not exists (select d2.id from DynamicDimension_New d2 where d1.DimensionTableName=d2.DimensionTableName and d2.Dimensions > d1.Dimensions)
and Measure.IsDeleted=0
order by D1.Dimensions, D1.TableName
OPEN MEASUREVALUE_CURSOR
FETCH NEXT FROM MEASUREVALUE_CURSOR
INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
while @@FETCH_STATUS = 0
	BEGIN


	DECLARE @shortTableName nvarchar(max)= (left(@MEASURETABLENAME, len(@MEASURETABLENAME)-4)) 

	if @LastDimensionTableName is null or @LastDimensionTableName<> @MEASURETABLENAME
	BEGIN
		if @LastDimensionTableName is not null
		BEGIN
			SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

			exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
		END


		if @DEBUG=1
		BEGIN
			print 'CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'
		END

		SET @CURQUERYFORERROR='CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)'

		exec('CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']	( [' + @shortTableName + '] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)')

		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME
		SET @LastDimensionTableName=@MEASURETABLENAME
	END

	if @COMPUTEALLVALUES=0
	BEGIN

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  @shortTableName)
		--SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  '')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END
	ELSE
	BEGIN

		--select d1.id, count(*) as Inventory, count(*) as RecordCount from #DIMENSIONTABLE# d1 
		--inner join DEXAllLeadHistory d3 on d3.#DIMENSIONFIELD#<=d1.#DIMENSIONTABLEFIELD# 
		--where  #DIMENSIONWHERE# d1.d1=d3.DimensionValue12 and not exists ( 
		--select d2.LeadID from DEXAllLeadHistory d2 
		--where d2.OldValue=d3.NewValue and d2.OldValue<>d2.NewValue and cast(d2.CreatedDate as datetime) > cast(d3.CreatedDate as datetime) and d2.LeadID=d3.LeadID) group by d1.id


		if @COMPUTEALLVALUESFORMULA is not null
		BEGIN
			SET @AGGREGATIONQUERY = @COMPUTEALLVALUESFORMULA
		END
		
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONFIELD#',  'DimensionValue' + @DIMENSIONID)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLE#',  @MEASURETABLENAME)

		--Need to loop through the dimensions in the table with the exception of the computeallvalues dimension
		DECLARE @DIMENSIONTABLEFIELD varchar(max);
		DECLARE @DIMENSIONWHERE varchar(max);

		DECLARE @OLDMEASURETABLENAME varchar(max) = left(@MEASURETABLENAME,LEN(@MEASURETABLENAME)-4) --Strip the _New off the name

		set @dCount=1;
		set @dIndex = CHARINDEX('d',@OLDMEASURETABLENAME);
		set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);
		set @DIMENSIONWHERE=''
		set @DIMENSIONTABLEFIELD='d1'


		while @dCount <= @DIMENSIONS
		BEGIN
			DECLARE @CURDIMENSION varchar(10);

			set @CURDIMENSION=SUBSTRING(@OLDMEASURETABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1);

			if @CURDIMENSION<>@DIMENSIONID
			BEGIN

				if len(@DIMENSIONWHERE) > 0
				BEGIN
					set @DIMENSIONWHERE= @DIMENSIONWHERE + ' and '
				END

				set @DIMENSIONWHERE = @DIMENSIONWHERE + ' d1.d' + cast(@dCount as varchar) + '=d2.DimensionValue' + cast(@CURDIMENSION as varchar) + ' '; 
			END
			ELSE
			BEGIN
				set @DIMENSIONTABLEFIELD = 'd' + cast(@dCount as varchar) + ' '
			END

			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@OLDMEASURETABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@OLDMEASURETABLENAME)+1;
				END
		END

		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONTABLEFIELD#',  @DIMENSIONTABLEFIELD)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,'#DIMENSIONWHERE#',  @DIMENSIONWHERE)
		SET @AGGREGATIONQUERY=REPLACE(@AGGREGATIONQUERY,', COUNT(*) as RecordCount',  ', 1 as RecordCount')
		SET @AGGREGATIONQUERY = N'delete from ' + @MEASURETABLENAME + 'Value where Measure=' + cast(@MEASUREID as varchar) + ' insert into ' + @MEASURETABLENAME + 'Value (Measure,' + @shortTableName + ',Value, rows) select ' + cast(@MEASUREID as varchar) + ', A.* from (' + @AGGREGATIONQUERY + ') A';
			--update d38d45d53d65d66Value
			--set rows=rcount from (
			--select d38d45d53d65d66 as d, count(*) as rcount from DummyContacts_Value0
			--group by d38d45d53d65d66) A
			--where d38d45d53d65d66=d

		if @UseRowCountFromFormula is null or @UseRowCountFromFormula=0
		BEGIN
			SET @AGGREGATIONQUERY = @AGGREGATIONQUERY + ' Update ' + @MEASURETABLENAME + 'VALUE set rows=rcount from ( select ' + @shortTableName + ' as d, count(*) as rcount from ' + @DIMENSIONVALUETABLENAME + ' group by ' + @shortTableName + ' ) A where ' + @shortTableName + ' =d  and Measure=' + cast(@MEASUREID as varchar) 
		END
	END

	--select @AGGREGATIONQUERY

	--print @AGGREGATIONQUERY
	SET @CURQUERYFORERROR=@AGGREGATIONQUERY
	if @DEBUG=1
	BEGIN
		print @AGGREGATIONQUERY
	END
	exec(@AGGREGATIONQUERY)

	FETCH NEXT FROM MEASUREVALUE_CURSOR
	INTO @MEASUREID, @AGGREGATIONQUERY, @MEASURETABLENAME, @DIMENsIONID, @COMPUTEALLVALUES, @COMPUTEALLVALUESFORMULA, @DIMENSIONS, @DIMENSIONVALUETABLENAME, @UseRowCountFromFormula
	END;
CLOSE MEASUREVALUE_CURSOR
DEALLOCATE MEASUREVALUE_CURSOR

--Clean up from Aggreation process 
--Fix to issue #310
if @LastDimensionTableName is not null
BEGIN
	SET @CURQUERYFORERROR='DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']'

	exec('DROP INDEX [' + @LASTDIMENSIONVALUETABLENAME + '_AggregationIndex] ON [dbo].[' + @LASTDIMENSIONVALUETABLENAME + ']')
END


--For performance Improvement

DECLARE @MasterTable varchar(max);

--SET @MasterTable=(select top 1 tablename from DynamicDimension where DimensionTableName='DummyContacts'
--order by Dimensions desc)


DECLARE @dtable table (id int, did int)

--insert into @dtable select id from Dimension where IsDeleted=0 and TableName='DummyContacts'

--select * from @dtable

DECLARE @ctable table (id int, did int)
SET @LASTDIMENSIONTABLENAME ='';

DECLARE @InsertQuery varchar(max);
DECLARE @iCount int;


DECLARE Dimension_Cursor CURSOR FOR
select TableName+'_New', Dimensions, DimensionTableName from DynamicDimension_New 
where ContainsDateDimension=1
order by DimensionTableName, Dimensions desc
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
while @@FETCH_STATUS = 0
	BEGIN
	delete from @ctable

	if @LASTDIMENSIONTABLENAME<>@DIMENSIONTABLENAME
	BEGIN
		SET @MasterTable = @TABLENAME
		insert into @dtable select 0, id from Dimension where IsDeleted=0 and TableName=@DIMENSIONTABLENAME order by id
		SET @LASTDIMENSIONTABLENAME = @DIMENSIONTABLENAME

		SET @iCount=1

		while (select count(*) from @dtable where id=0) > 0
		BEGIN
			update @dtable set id=@iCount where did=(select min(did) from @dtable where id=0)
			set @iCount = @iCount + 1
		END

	END


	if @TABLENAME<>@MasterTable
	BEGIN

		set @dCount=1;
		set @dIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4));
		set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);
		while @dCount <= @DIMENSIONS
			BEGIN

				--if @DEBUG=1
				--BEGIN
				--	print SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1)
				--END

				insert into @ctable select @dCount, cast(SUBSTRING(left(@TABLENAME, len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1) as int)

				set @dCount = @dCount + 1;
				set @dIndex = @dNextIndex;
				set @dNextIndex = CHARINDEX('d',left(@TABLENAME, len(@TABLENAME)-4),@dIndex+1);

				if @dNextIndex = 0
					BEGIN
					set @dNextIndex = LEN(left(@TABLENAME, len(@TABLENAME)-4))+1;
					END

			END

		--select d3.id, m1.id,
		--case 
			--when m1.AggregationType='AVG' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			--else sum(Value)
		--END as Generated,
		--sum(d1.rows) as recordCount
		 --from d38d45d53d65d66Value d1
		--inner join d38d45d53d65d66 d2 on d1.d38d45d53d65d66=d2.id
		--inner join d38d45d53d65 d3 on d2.d1=d3.d1 and d2.d2=d3.d2 and d2.d3=d3.d3 and d2.d4=d3.d4
		--inner join Measure m1 on d1.Measure=m1.id
		--group by d3.id, m1.id, m1.AggregationType


		set @InsertQuery='truncate table ' + @TABLENAME + 'VALUE insert into ' + @TABLENAME + 'VALUE select d3.id, m1.id, case 
			when m1.AggregationType=''AVG'' then sum((cast(d1.rows as float)*d1.Value))/sum(cast(d1.rows as float))
			else sum(Value)
		END as Generated, sum(rows) as recordCount from ' + @MasterTable + 'Value d1 '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @MasterTable + ' d2 on d1.' + left(@MasterTable, len(@MasterTable)-4) + '=d2.id '
		SET @InsertQuery = @InsertQuery + ' inner join ' + @TABLENAME + ' d3 on '

		while (select count(*) from @ctable) > 0
		BEGIN

			SET @InsertQuery = @InsertQuery +  (select 'd3.d' + cast(c1.id as varchar) + '=d2.d' + cast(d1.id as varchar) from @ctable c1
				inner join @dtable d1 on c1.did=d1.did
				where c1.id=(select min(id) from @ctable))
			delete from @ctable where id=(select min(id) from @ctable)
			if (select count(*) from @ctable) > 0
			BEGIN
				SET @InsertQuery = @InsertQuery + ' AND '
			END
		END

		SET @InsertQuery = @InsertQuery + ' inner join Measure m1 on d1.Measure=m1.id Group by d3.id, m1.id, m1.AggregationType'

		if @DEBUG=1
		BEGIN
			print @InsertQuery
		END

		SET @CURQUERYFORERROR=@InsertQuery

		exec( @InsertQuery)
	END

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME
	END

close Dimension_Cursor
Deallocate Dimension_Cursor

--END INSERT of performance code





END TRY

BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicColumns',@CURQUERYFORERROR);
	THROW;
END CATCH

END






GO
