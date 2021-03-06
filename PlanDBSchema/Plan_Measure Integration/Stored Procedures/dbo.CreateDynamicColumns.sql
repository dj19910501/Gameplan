IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CreateDynamicColumns') AND xtype IN (N'P'))
    DROP PROCEDURE CreateDynamicColumns
GO


CREATE PROCEDURE [CreateDynamicColumns]  @DEBUG bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY


DECLARE @COLUMNNAME varchar(max);
DECLARE @CONSTRAINTTABLENAME varchar(max);
DECLARE @DIMENSIONTABLENAME varchar(max);
DECLARE @DIMENSIONVALUETABLENAME varchar(max);
DECLARE @LASTDIMENSIONVALUETABLENAME varchar(max);
DECLARE @CREATEVIEWSQL varchar(max);
DECLARE @CREATEVIEWJOINSQL varchar(max);
DECLARE @LASTTABLENAME varchar(max);
DECLARE @TOTALCREATEVIEWSQL varchar(max)
DECLARE @ADDEDBASENEW bit;


SET @ADDEDBASENEW=0
set @LASTDIMENSIONVALUETABLENAME=null;
SET @LASTTABLENAME=null;


truncate table AggregationQueries;

--We need to drop and then recreate all of the DimensionValueTables
DECLARE DimensionValue_Cursor Cursor for
select TableName, DimensionValueTableName + '_New', DimensionTableName from DynamicDimension_New where containsdatedimension=1 order by DimensionValueTableName, TableName
OPEN DimensionValue_Cursor
FETCH NEXT FROM DimensionValue_Cursor
INTO @COLUMNNAME, @DIMENSIONVALUETABLENAME, @DIMENSIONTABLENAME
while @@FETCH_STATUS = 0
	BEGIN

	--If we have a new DimensionValueTableName
	if @LASTDIMENSIONVALUETABLENAME is null or @LASTDIMENSIONVALUETABLENAME<>@DIMENSIONVALUETABLENAME
		BEGIN

		if @LASTTABLENAME is null or @LASTTABLENAME <> @DIMENSIONTABLENAME
			BEGIN


			if LEN(@CREATEVIEWSQL) > 0
				BEGIN


				DECLARE @ColsToAdd table (ToAdd nvarchar(max))

				Delete from  @ColsToAdd
				insert into @ColsToAdd
				exec ('select TableName + ''_Base_New.[DimensionValue'' + cast(id as varchar) + '']'' from Dimension where TableName = ''' + @LASTTABLENAME + ''' and isDeleted=0')

				DECLARE @ccount int = (select count(*) from @ColsToAdd)

				while @ccount > 0
				BEGIN
					DECLARE @toAdd nvarchar(max) = (select top 1 ToAdd from @ColsToAdd)

					SET @CREATEVIEWSQL = @CREATEVIEWSQL + ',' + @toAdd + ' '

					delete from @ColsToAdd where toAdd=@ToAdd

				set @ccount = (select count(*) from @ColsToAdd)
				END


				SET @TOTALCREATEVIEWSQL = @CREATEVIEWSQL + ' from ' + @LASTTABLENAME + ' '  + @CREATEVIEWJOINSQL

				SET @CURQUERYFORERROR = @TOTALCREATEVIEWSQL
				if @DEBUG=1
				BEGIN
					print @TOTALCREATEVIEWSQL
				END
				exec(@TOTALCREATEVIEWSQL)
				SET @ADDEDBASENEW=0

				insert into AggregationQueries select replace(@TOTALCREATEVIEWSQL,'_New','')

				END

			SET @LASTTABLENAME=@DIMENSIONTABLENAME
			SET @CREATEVIEWSQL = ' CREATE VIEW [dbo].[' + @DIMENSIONTABLENAME + '_VIEW_New] AS SELECT '
			SET @CREATEVIEWJOINSQL=''

			--Need all of the columns in the base table
			DECLARE @CURCOL varchar(max);
			DECLARE @CURTABLE varchar(max);
			DECLARE @FOUNDCOL int;

			SET @FOUNDCOL=0

			DECLARE Col_Cursor Cursor for
			select COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS 
			Open Col_Cursor
			Fetch Next from Col_Cursor
			into @CURCOL, @CURTABLE
			while @@FETCH_STATUS = 0
				BEGIN
				if @CURTABLE=@DIMENSIONTABLENAME
					BEGIN
					
					if left(@CURCOL,14)<>'DimensionValue' -- We don't want the old DimensionValue fields in here
					BEGIN
										
						if @FOUNDCOL=1
							BEGIN
							SET @CREATEVIEWSQL = @CREATEVIEWSQL + ', '
							END

						SET @FOUNDCOL=1

						SET @CREATEVIEWSQL = @CREATEVIEWSQL + ' [' + @DIMENSIONTABLENAME + '].[' + @CURCOL + '] '
						END
					END

				Fetch Next from Col_Cursor
				into @CURCOL, @CURTABLE
				END

			CLOSE Col_Cursor
			DEALLOCATE Col_Cursor

			END

		SET @LASTDIMENSIONVALUETABLENAME=@DIMENSIONVALUETABLENAME;

		--If the table exists, we need to drop it
		DECLARE @DROPSQL varchar(max);
		SET @DROPSQL='IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ''' + @DIMENSIONVALUETABLENAME + ''' AND TABLE_SCHEMA = ''dbo'')	BEGIN DROP TABLE ' + @DIMENSIONVALUETABLENAME + ' END'

		SET @DROPSQL=  @DROPSQL + ' IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = ''' + @DIMENSIONTABLENAME + '_VIEW_New'' AND TABLE_SCHEMA = ''dbo'')	BEGIN DROP VIEW ' + @DIMENSIONTABLENAME + '_VIEW_New END'
	
		SEt @CURQUERYFORERROR = @DROPSQL
		if @DEBUG=1
		BEGIN
			print @DROPSQL
		END

		exec(@DROPSQL)

		--Now we need to create the table
		DECLARE @CREATESQL varchar(max);

		
		SET @CREATESQL='CREATE TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + '](			[id] [bigint] IDENTITY(1,1) NOT NULL, [' + @DIMENSIONTABLENAME + '] [bigint] NOT NULL 		 CONSTRAINT [PK_' + @DIMENSIONVALUETABLENAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +  '] PRIMARY KEY CLUSTERED 		(			[id] ASC		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]		) ON [PRIMARY] '


		SET @CREATESQL = @CREATESQL + ' ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME +']  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @DIMENSIONTABLENAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + '1] FOREIGN KEY([' + @DIMENSIONTABLENAME + ']) REFERENCES [dbo].[' + @DIMENSIONTABLENAME + '] ([id]) ';

		SET @CREATESQL = @CREATESQL + 
			'CREATE NONCLUSTERED INDEX [' + @DIMENSIONVALUETABLENAME + '_BaseTableReference] ON [dbo].[' + @DIMENSIONVALUETABLENAME + ']
			(
			[' + @DIMENSIONTABLENAME + '] ASC
			)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY] '



		SET @CURQUERYFORERROR = @CREATESQL

		if @DEBUG=1
		BEGIN
			print @CREATESQL
		END
		exec(@CREATESQL)

		SET @CREATEVIEWJOINSQL = @CREATEVIEWJOINSQL + ' inner join ' + @DIMENSIONVALUETABLENAME + ' on ' + @DIMENSIONVALUETABLENAME + '.' + @DIMENSIONTABLENAME + '=' + @DIMENSIONTABLENAME + '.id '
		if @ADDEDBASENEW=0
		BEGIN

			SET @CREATEVIEWJOINSQL = @CREATEVIEWJOINSQL + ' inner join ' + @DIMENSIONTABLENAME + '_Base_New on ' + @DIMENSIONTABLENAME + '_Base_New.id=' + @DIMENSIONTABLENAME + '.id '
			SET @ADDEDBASENEW=1
		END

		END


	--Now we need to add the column on to the table
	DECLARE @ADDCOLUMNSQL varchar(max);

	SET @ADDCOLUMNSQL = 'ALTER TABLE ' + @DIMENSIONVALUETABLENAME + ' ADD ' + @COLUMNNAME + ' int '

	SET @ADDCOLUMNSQL = @ADDCOLUMNSQL + 
	' ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + ']  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +'] FOREIGN KEY([' + @COLUMNNAME + '])
							REFERENCES [dbo].[' + @COLUMNNAME + '_New] ([id])
							ALTER TABLE [dbo].[' + @DIMENSIONVALUETABLENAME + '] CHECK CONSTRAINT [FK_' + @DIMENSIONVALUETABLENAME + '_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) +']'

	SET @CURQUERYFORERROR = @ADDCOLUMNSQL

	if @DEBUG=1
	BEGIN
		print @ADDCOLUMNSQL
	END
	exec(@ADDCOLUMNSQL)

	SET @CREATEVIEWSQL = @CREATEVIEWSQL + ', [' + @DIMENSIONVALUETABLENAME + '].[' + @COLUMNNAME + ']'

	FETCH NEXT FROM DimensionValue_Cursor
	INTO @COLUMNNAME, @DIMENSIONVALUETABLENAME, @DIMENSIONTABLENAME
	END

CLOSE DimensionValue_Cursor
DEALLOCATE DimensionValue_Cursor

--Need to add the _Base_New columns in here

Delete from  @ColsToAdd
insert into @ColsToAdd
exec ('select TableName + ''_Base_New.DimensionValue'' + cast(id as varchar) from Dimension where isDeleted=0 and TableName = ''' + @DIMENSIONTABLENAME + '''')

SET @ccount = (select count(*) from @ColsToAdd)

while @ccount > 0
BEGIN
	SET @toAdd  = (select top 1 ToAdd from @ColsToAdd)

	SET @CREATEVIEWSQL = @CREATEVIEWSQL + ',' + @toAdd + ' '

	delete from @ColsToAdd where toAdd=@ToAdd

set @ccount = (select count(*) from @ColsToAdd)
END



if LEN(@CREATEVIEWSQL) > 0
	BEGIN

	SET @TOTALCREATEVIEWSQL = @CREATEVIEWSQL + ' from ' + @DIMENSIONTABLENAME + ' '  + @CREATEVIEWJOINSQL

	SET @CURQUERYFORERROR = @TOTALCREATEVIEWSQL
	if @DEBUG=1
	BEGIN
		print @TOTALCREATEVIEWSQL
	END
	exec(@TOTALCREATEVIEWSQL)
	insert into AggregationQueries select replace(@TOTALCREATEVIEWSQL,'_New','')

	END


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','CreateDynamicColumns',@CURQUERYFORERROR);
	THROW;
END CATCH

END




GO
