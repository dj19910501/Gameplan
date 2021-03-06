IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[DynamicTableCreationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[DynamicTableCreationPartial]  
END
GO

CREATE PROCEDURE [dbo].[DynamicTableCreationPartial]  @DEBUG bit = 0
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
DECLARE @ComputeAllValues bit;
DECLARE @DATEDIMENSION int;



DECLARE Dimension_Cursor CURSOR FOR
select TableName, Dimensions, DimensionTableName, ComputeAllValues, 
case when ComputeAllValues=1 then dbo.FindDateDimension(TableName,Dimensions) else 0 end as DateDimension
from DynamicDimension_New 
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues, @DATEDIMENSION
while @@FETCH_STATUS = 0
	Begin

	DECLARE @CREATE_QUERY varchar(max);
	DECLARE @INSERTQUERY varchar(max); --To populate the d1d2 etc. tables
	DECLARE @CREATEVALUEQUERY varchar(max);
	DECLARE @CurDate varchar(100) = (select cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar));

	--There are two cases
	--1) Non compute all values
	--2) Compute All Values

	--The table and value creates are the same for both cases

	--In the case of non compute all values we just need to get all of the right values, here are the steps
	--1) Turn Identity insert on
	--2) Run a query to find the values that exist in the current table and insert with those identities
	--3) Turn Identity insert off
	--4) Set the identity seed to one more than the one from the old table
	--5) Insert any new rows

	DECLARE @DimensionList nvarchar(max) = ''--'d1,d2'
	DECLARE @DimensionValueWhere nvarchar(max) = ''--'d1.d1=d2.DimensionValue67 and d1.d2=d2.DimensionValue68'
	DECLARE @DimensionSelect nvarchar(max) = ''--'d1.DimensionValue67, d1.DimensionValue68'
	DECLARE @DimensionSelect2 nvarchar(max) = ''--'d2.DimensionValue67, d2.DimensionValue68'

	DECLARE @ComputeAllValuesWhere nvarchar(max)=''--'d1.d2=d2.DimensionValue68 and d3.dimensionID=67 and d1.d1=d3.id'
	DECLARE @ComputeAllValuesSelect nvarchar(max)=''--'d3.id, d1.DimensionValue68'
	DECLARE @DateDimensionNumber nvarchar(max)=''--'67'
	DECLARE @ComputeAllValuesNewWhere nvarchar(max)=''--'d2.d1=d3.id and d2.d2=d1.DimensionValue68'

	DECLARE @Part2 nvarchar (max)=''
	DECLARE @Part3 nvarchar (max)=''



	DECLARE @DimensionValues table(dv int, val int)

	delete from @DimensionValues

	insert into @DimensionValues
	select * from dbo.FindDimensionsForTable(@TABLENAME,@DIMENSIONS)


	DECLARE @i int=1
	while @i<=@DIMENSIONS
	BEGIN

		set @PART2 = @PART2 + 	'ALTER TABLE [dbo].[' + @TABLENAME + '_New]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + '_New_DimensionValue' + cast(@i as nvarchar) + @CurDate + '] FOREIGN KEY([d' + cast(@i as nvarchar) + '])
		REFERENCES [dbo].[DimensionValue_New] ([id])

		ALTER TABLE [dbo].[' + @TABLENAME + '_New] CHECK CONSTRAINT [FK_' + @TABLENAME + '_New_DimensionValue' + cast(@i as nvarchar) + @CurDate + ']';

		set @PART3 = @PART3 + '[d' + cast(@i as nvarchar) + '] [int] NOT NULL, ';



		SET @DimensionList = @DimensionList + 'd' + cast(@i as nvarchar)
		DECLARE @curDimensionNum int = (select val from @DimensionValues where dv=@i)
		SET @DimensionValueWhere = @DimensionValueWhere + 'd1.d' + cast(@i as nvarchar) + '=d2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
		SET @DimensionSelect = @DimensionSelect + 'd1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
		SET @DimensionSelect2 = @DimensionSelect2 + 'd2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '

		if @i<>@DateDimension
		BEGIN
			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + 'd1.d' + cast(@i as nvarchar) + '=d2.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + 'd1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + 'd2.d' + cast(@i as nvarchar) + '=d1.DimensionValue' + cast(@curDimensionNum as nvarchar) + ' '

		END
		else
		BEGIN
			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + 'd3.ID=' + cast(@curDimensionNum as nvarchar) + ' and d1.d' + cast(@i as nvarchar) + '=d3.id '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + 'd3.id '
			SET @DateDimensionNumber=cast(@curDimensionNum as nvarchar)
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + 'd2.d' + cast(@i as nvarchar) + '=d3.id '
		END

		if @i<@DIMENSIONS
		BEGIN
			SET @DimensionList = @DimensionList + ', '
			SET @DimensionValueWhere = @DimensionValueWhere + ' and '
			SET @DimensionSelect = @DimensionSelect + ', '
			SET @DimensionSelect2 = @DimensionSelect2 + ', '

			SET @ComputeAllValuesWhere=@ComputeAllValuesWhere + ' and '
			SET @ComputeAllValuesSelect=@ComputeAllValuesSelect + ', '
			SET @ComputeAllValuesNewWhere=@ComputeAllValuesNewWhere + ' and '
		END

		SET @i=@i+1
	END

	--Union added for ticket #699
	DECLARE @NonComputeAllValuesUnion nvarchar(max)='	union 
	select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
	where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base d2 where ' + @DimensionValueWhere + ' and d2.dirty=1)
	'
	IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = @TABLENAME AND Object_ID = Object_ID(@DIMENSIONTABLENAME + '_Base'))
	BEGIN
		--If this is a new dimension and it didn't exist before
		SET @NonComputeAllValuesUnion=''
	END



	DECLARE @NonComputeAllValuesQuery nvarchar(max)=

	'SET IDENTITY_INSERT ' + @TABLENAME + '_New on

	insert into ' + @TABLENAME + '_New (id, ' + @DimensionList + ')

	select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
	where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base_New d2 where ' + @DimensionValueWhere + ')

	' + @NonComputeAllValuesUnion + '

	SET IDENTITY_INSERT ' + @TABLENAME + '_New off

	DECLARE @Seed int = (Select IDENT_CURRENT(''' + @TABLENAME + '''))+1

	DBCC CHECKIDENT(''' + @TABLENAME + '_New'', RESEED, @Seed)

	insert into ' + @TABLENAME + '_New 

	select distinct ' + @DimensionSelect2 + ' from ' + @DIMENSIONTABLENAME + '_Base_New d2
	where not exists (select d1.id from ' + @TABLENAME + '_New d1 where ' + @DimensionValueWhere + ')'


	--In the case of compute all values we just need to get all of the right values, here are the steps
	--1) Turn Identity insert on
	--2) Run a query to find the values that exist in the current table and insert with those identities
	--3) Turn Identity insert off
	--4) Set the identity seed to one more than the one from the old table
	--5) Insert any new rows

	--Union added for ticket #699
	DECLARE @ComputeAllValuesUnion nvarchar(max)='
			union
		select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
		where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base d2 
					cross join @cVals d3
		where ' + @ComputeAllValuesWhere + ' and d2.dirty=1)'

	IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = @TABLENAME AND Object_ID = Object_ID(@DIMENSIONTABLENAME + '_Base'))
	BEGIN
		--If this is a new dimension and it didn't exist before
		SET @ComputeAllValuesUnion=''
	END


	DECLARE @ComputeAllValuesQuery nvarchar(max)=

	'DECLARE @dVals table(id int)
	DECLARE @cVals table(id int)

	insert into @dVals
	select id from DimensionValue where dimensionID=' + @DATEDIMENSIONNUMBER + '

	while (select count(*) from @dVals) > 0
	BEGIN
		DELETE from @cVals
		insert into @cVals
		select top 100 id from @dVals

		SET IDENTITY_INSERT ' + @TABLENAME + '_New on

		insert into ' + @TABLENAME + '_New (id, ' + @DimensionList +')

		select id, ' + @DimensionList + ' from ' + @TABLENAME + ' d1
		where exists (select d2.id from ' + @DIMENSIONTABLENAME + '_Base_New d2 
					cross join @cVals d3
		where ' + @ComputeAllValuesWhere + ')

		' + @ComputeAllValuesUnion + '

		SET IDENTITY_INSERT ' + @TABLENAME + '_New off

		DECLARE @Seed int = (Select IDENT_CURRENT(''' + @TABLENAME + '''))+1

		DBCC CHECKIDENT(''' + @TABLENAME + '_New'', RESEED, @Seed)

		insert into ' + @TABLENAME + '_New 

		select distinct ' + @ComputeAllValuesSelect + ' from ' + @DIMENSIONTABLENAME + '_Base_New d1
		cross join @cVals d3
		where d3.ID=' + @DateDimensionNumber + ' and
		not exists (select d2.id from ' + @TABLENAME + '_New d2 where ' + @ComputeAllValuesNewWhere + ')

		delete from @dVals where id in (select id from @cVals)
	END
	'




	if @ComputeAllValues=0
	BEGIN
		SET @INSERTQUERY = @NonComputeAllValuesQuery;
	END
	ELSE
	BEGIN
		SET @INSERTQUERY = @ComputeAllValuesQuery;
	END


	SET @TABLENAME=@TABLENAME+'_New'

	SET @CREATE_QUERY = N'

			if object_id(N''' + @TABLENAME + ''',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + ']
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + '](
				[id] [int] IDENTITY(1,1) NOT NULL, ' + @PART3 + '
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + '] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + '] REBUILD PARTITION = ALL
			WITH (DATA_COMPRESSION = ROW); 

			' + @PART2;
	
	SET @CREATEVALUEQUERY=N'
			if object_id(N''' + @TABLENAME + 'Value'',N''U'') is not null
			BEGIN

				DROP TABLE [dbo].[' + @TABLENAME + 'Value]
			END

			SET ANSI_NULLS ON

			SET QUOTED_IDENTIFIER ON

			CREATE TABLE [dbo].[' + @TABLENAME + 'Value](
				[id] [int] IDENTITY(1,1) NOT NULL, 
				[' + left(@TABLENAME, len(@TABLENAME)-4) + '] int NOT NULL, 
				[Measure] [int] NOT NULL,
				[Value] [float] NOT NULL,
				[Rows] [float] null,
			 CONSTRAINT [PK_' + @TABLENAME + @CurDate + 'Value] PRIMARY KEY CLUSTERED 
			(
				[id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate + '] FOREIGN KEY([' + left(@TABLENAME, len(@TABLENAME)-4) + '])
			REFERENCES [dbo].[' + @TABLENAME + '] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_' + @TABLENAME + @CurDate +']
			
			ALTER TABLE [dbo].[' + @TABLENAME + 'Value]  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + '] FOREIGN KEY([Measure])
			REFERENCES [dbo].[Measure] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + 'Value] CHECK CONSTRAINT [FK_' + @TABLENAME + 'Value_Measure' + @CurDate + ']'
			
		--Before we drop the table, we need to eliminate all constraints on the table

		CREATE TABLE #Keys
		(
		 PKTABLE_QUALIFIER varchar(max),
		 PKTABLE_OWNER varchar(max),
		 PKTABLE_NAME varchar(max),
		 PKCOLUMN_NAME varchar(max),
		 FKTABLE_QUALIFIER varchar(max),
		 FKTABLE_OWNER varchar(max),
		 FKTABLE_NAME varchar(max),
		 FKCOLUMN_NAME varchar(max),
		 KEY_SEQ int,
		 UPDATE_RULE int,
		 DELETE_RULE int,
		 FK_NAME varchar(max),
		 PK_NAME varchar(max),
		 DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= @TABLENAME

		DECLARE @FKTABLE varchar(max);
		DECLARE @FKNAME varchar(max);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(max);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;

			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END
			exec (@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor



		drop table #Keys


		--select @CREATE_QUERY
		SET @CURQUERYFORERROR=@CREATE_QUERY
		if @DEBUG=1
		BEGIN
			print @CREATE_QUERY
		END
		exec (@CREATE_QUERY);

		--Okay, now we need to populate the table
		--select @INSERTQUERY

		SET @CURQUERYFORERROR=@INSERTQUERY
		if @DEBUG=1
		BEGIN
			print @INSERTQUERY
		END
		exec (@INSERTQUERY)

		--Release space used in the insert query
		dbcc shrinkdatabase (tempdb, 10) 


		SET @CURQUERYFORERROR=@CREATEVALUEQUERY
		if @DEBUG=1
		BEGIN
			print @CREATEVALUEQUERY
		END
		exec (@CREATEVALUEQUERY)

		print 'After everything'

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues, @DATEDIMENSION
	END;
Close Dimension_Cursor
Deallocate Dimension_Cursor
END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','DynamicTableCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END


GO
