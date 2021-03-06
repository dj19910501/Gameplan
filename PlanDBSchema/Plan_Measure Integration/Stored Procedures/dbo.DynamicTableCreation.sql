IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DynamicTableCreation') AND xtype IN (N'P'))
    DROP PROCEDURE DynamicTableCreation
GO


CREATE PROCEDURE [DynamicTableCreation]  @DEBUG bit = 0
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


DECLARE Dimension_Cursor CURSOR FOR
select TableName + '_New', Dimensions, DimensionTableName, ComputeAllValues from DynamicDimension_New
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues
while @@FETCH_STATUS = 0
	Begin

	DECLARE @CREATE_QUERY varchar(max);

	DECLARE @PART2 varchar(max);
	DECLARE @PART3 varchar(max);
	DECLARE @INSERTQUERY varchar(max); --To populate the d1d2 etc. tables
	DECLARE @INSERTFROM varchar(max);
	DECLARE @INSERTWHERE varchar(max);
	DECLARE @dCount int;
	DECLARE @dIndex int;
	DECLARE @dNextIndex int;
	DECLARE @CREATEVALUEQUERY varchar(max);
	DECLARE @CurDate varchar(100) = (select cast(DATEDIFF(second,'1/1/2015',getdate()) as varchar));



	--Need to do something different if this is a table for which all values should be populated.
	SET @INSERTQUERY = N'Insert into ' + @TABLENAME + ' select distinct ';

	if @ComputeAllValues=0
	BEGIN
		SET @INSERTFROM = N' FROM ' + @DIMENSIONTABLENAME + '_Base_New ';
	END
	ELSE
	BEGIN
		SET @INSERTFROM = N' FROM ';
	END
	SET @INSERTWHERE = N' WHERE ';
	set @PART2='';
	set @PART3='';
	set @dCount=1;
	set @dIndex = CHARINDEX('d',@TABLENAME);
	set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

	DECLARE @FOUNDDATEDIMENSION bit;
	DECLARE @SUBTABLENAME varchar(max);
	DECLARE @DATEDIMENSION varchar(max);

	SET @FOUNDDATEDIMENSION=0;
	SET @SUBTABLENAME='';
	SET @DATEDIMENSION='0';

	if @ComputeAllValues=1 and @DIMENSIONS>2
	BEGIN
		SET @INSERTQUERY='DECLARE @t table (dv int)

		insert into @t
		select id from DimensionValue_New where DimensionID=%DATEDIMENSION%

		DECLARE @rc int=(select count(*) from @t)


		while @rc > 0
		BEGIN

			DECLARE @u table (id int)

			delete from @u

			insert into @u
			select top 100 dv from @t

			Insert into ' + @TABLENAME + ' 
			select distinct '
	END

	while @dCount <= @DIMENSIONS
		BEGIN

			set @PART2 = @PART2 + 	'ALTER TABLE [dbo].[' + @TABLENAME + ']  WITH CHECK ADD  CONSTRAINT [FK_' + @TABLENAME + '_DimensionValue' + cast(@dCount as nvarchar) + @CurDate + '] FOREIGN KEY([d' + cast(@dCount as nvarchar) + '])
			REFERENCES [dbo].[DimensionValue_New] ([id])

			ALTER TABLE [dbo].[' + @TABLENAME + '] CHECK CONSTRAINT [FK_' + @TABLENAME + '_DimensionValue' + cast(@dCount as nvarchar) + @CurDate + ']';

			set @PART3 = @PART3 + '[d' + cast(@dCount as nvarchar) + '] [int] NOT NULL, ';

			if @ComputeAllValues=0
			BEGIN

				set @INSERTQUERY = @INSERTQUERY + 'DimensionValue' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1) + ' '
				--set @INSERTFROM = @INSERTFROM + ' DimensionValue d' + cast(@dCount as nvarchar) + ' ';
				--set @INSERTWHERE = @INSERTWHERE + 'd' + cast(@dCount as nvarchar) + '.DimensionId =' + SUBSTRING(@TABLENAME,@dIndex + 1,@dNextIndex - @dIndex - 1);
			END
			else
			BEGIN

				if @DIMENSIONS=2
				BEGIN
					set @INSERTQUERY = @INSERTQUERY + 'd' + cast(@dCount as varchar) + '.id '
					set @INSERTFROM = @INSERTFROM + ' DimensionValue_New d' + cast(@dCount as varchar) + ' ';
					set @INSERTWHERE = @INSERTWHERE + 'd' + cast(@dCount as varchar) + '.DimensionId =' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1);
				END
				ELSE
				BEGIN

					if @FOUNDDATEDIMENSION=1
					BEGIN
						set @INSERTQUERY = @INSERTQUERY + 'd1.d' + cast((@dCount-1) as varchar) + ' '
						set @SUBTABLENAME= @SUBTABLENAME + 'd' + SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1)
					END
					else
					BEGIN
						DECLARE @DIMENSIONTOCHECK varchar(10);
						SET @DIMENSIONTOCHECK=SUBSTRING(Left(@TABLENAME,len(@TABLENAME)-4),@dIndex + 1,@dNextIndex - @dIndex - 1);
						SET @FOUNDDATEDIMENSION=(select isnull(computeAllValues,0) from Dimension where id=@DIMENSIONTOCHECK)	

						if @FOUNDDATEDIMENSION=0
						BEGIN
							set @INSERTQUERY = @INSERTQUERY + 'd1.d' + cast(@dCount as varchar) + ' '
							set @SUBTABLENAME= @SUBTABLENAME + 'd' + @DIMENSIONTOCHECK
						END
						ELSE
						BEGIN
							set @INSERTQUERY = @INSERTQUERY + 'd2.id '
							SET @DATEDIMENSION=@DIMENSIONTOCHECK
							SET @INSERTQUERY=Replace(@INSERTQUERY,'%DATEDIMENSION%',cast(@DIMENSIONTOCHECK as nvarchar))
						END
	
					END

					set @INSERTFROM = ' FROM ' + @SUBTABLENAME + '_New d1 cross join @u d2 '
					set @INSERTWHERE = ' 	delete from @t where dv in (select id from @u)

											set @rc= (select count(*) from @t)

											END'
				END
			END


			set @dCount = @dCount + 1;
			set @dIndex = @dNextIndex;
			set @dNextIndex = CHARINDEX('d',@TABLENAME,@dIndex+1);

			if @dNextIndex = 0
				BEGIN
				set @dNextIndex = LEN(@TABLENAME)+1;
				END

			if @dCount <= @DIMENSIONS
				BEGIN
				set @INSERTQUERY = @INSERTQUERY + ', ';

				END
					if @ComputeAllValues=1 and @DIMENSIONS=2
					BEGIN

						if @dCount = 2
						BEGIN
							set @INSERTFROM = @INSERTFROM + ', ';
							set @INSERTWHERE = @INSERTWHERE + ' and ';
						END
					END



		END


	SET @INSERTQUERY = @INSERTQUERY + @INSERTFROM; -- + @INSERTWHERE;

	if @ComputeAllValues=1
	BEGIN
		SET @INSERTQUERY = @INSERTQUERY + @INSERTWHERE;
	END

	--print @INSERTQUERY

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

	FETCH NEXT FROM Dimension_Cursor
	INTO @TABLENAME, @DIMENSIONS, @DIMENSIONTABLENAME, @ComputeAllValues
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
