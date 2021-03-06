IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[BaseDynamicColumnCreationPartial]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[BaseDynamicColumnCreationPartial]  
END
GO


CREATE PROCEDURE [dbo].[BaseDynamicColumnCreationPartial]  @DEBUG bit = 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

	--select TableName, DimensionTableName from DynamicDimension

	DECLARE @COLUMNNAME varchar(200);
	DECLARE @CONSTRAINTTABLENAME varchar(200);
	DECLARE @DIMENSIONTABLENAME varchar(200);
	DECLARE @DIMENSIONCOLUMNNAME varchar(200);
	DECLARE @DIMENSIONID varchar(20);

		CREATE TABLE #Keys
		(
			PKTABLE_QUALIFIER varchar(150),
			PKTABLE_OWNER varchar(150),
			PKTABLE_NAME varchar(150),
			PKCOLUMN_NAME varchar(150),
			FKTABLE_QUALIFIER varchar(150),
			FKTABLE_OWNER varchar(150),
			FKTABLE_NAME varchar(150),
			FKCOLUMN_NAME varchar(150),
			KEY_SEQ int,
			UPDATE_RULE int,
			DELETE_RULE int,
			FK_NAME varchar(150),
			PK_NAME varchar(150),
			DEFERRABILITY int
		)
		insert #Keys exec sp_fkeys @pktable_name= 'DimensionValue_New'

		DECLARE @FKTABLE varchar(150);
		DECLARE @FKNAME varchar(150);
		DECLARE @FKCOLUMN varchar(150);

		DECLARE Constraint_Cursor CURSOR FOR
		select FKTABLE_NAME, FK_NAME, FKCOLUMN_NAME from #Keys
		OPEN Constraint_Cursor
		FETCH NEXT FROM Constraint_Cursor
		INTO @FKTABLE, @FKNAME, @FKCOLUMN
		while @@FETCH_STATUS = 0
			Begin

			DECLARE @DROPCONSTRAINT varchar(1000);
			SET @DROPCONSTRAINT = N'ALTER TABLE ' + @FKTABLE + ' DROP CONSTRAINT ' + @FKNAME;


			SET @CURQUERYFORERROR=@DROPCONSTRAINT
			if @DEBUG=1
			BEGIN
				print @DROPCONSTRAINT
			END

			exec(@DROPCONSTRAINT)



			FETCH NEXT FROM Constraint_Cursor
			INTO @FKTABLE, @FKNAME, @FKCOLUMN
			End
		Close Constraint_Cursor
		Deallocate Constraint_Cursor

		drop table #Keys

	DECLARE @TableDropCreate nvarchar(max) 
	
	SET @TableDropCreate=' IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[DimensionValue_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE DimensionValue_New 
		END
		CREATE TABLE [dbo].[DimensionValue_New](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[DimensionID] [int] NOT NULL,
			[Value] [nvarchar](1000) NULL,
			[DisplayValue] [nvarchar](1000) NULL,
			[OrderValue] [nvarchar](1000) NULL,
		 CONSTRAINT [PK_DimensionValue_New_' + cast(DATEDIFF(second, '2015-01-01', GETDATE()) as varchar) + '] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
		) ON [PRIMARY]'

	exec(@TableDropCreate)

	DECLARE Base_Table_Cursor CURSOR FOR
	select distinct DimensionTableName from DynamicDimension_New
	OPEN Base_Table_Cursor
	FETCH NEXT FROM Base_Table_Cursor
	INTO @DIMENSIONTABLENAME
	while @@FETCH_STATUS = 0
		BEGIN

		--We need to create the _base tables
		exec('IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[' + @DIMENSIONTABLENAME + '_Base_New]'') AND type in (N''U''))
		BEGIN
			DROP tABLE ' + @DIMENSIONTABLENAME + '_Base_New 
		END
		CREATE TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New](
		[id] [bigint] NOT NULL
		) ON [PRIMARY]')



		FETCH NEXT FROM Base_Table_Cursor
		INTO @DIMENSIONTABLENAME
		END
	Close Base_Table_Cursor
	Deallocate Base_Table_Cursor


	DECLARE @TablesToAddDirtyBitTo table (tablename nvarchar(max))


	DECLARE Dimension_Cursor CURSOR FOR
	select 'DimensionValue'+ cast(id as varchar), 'DimensionValue', TableName, ColumnName, id from dimension where IsDeleted=0 and tablename in (select dimensiontablename from DynamicDimension_New)
	OPEN Dimension_Cursor
	FETCH NEXT FROM Dimension_Cursor
	INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
	while @@FETCH_STATUS = 0
		Begin

		if (select count(*) from @TablesToAddDirtyBitTo where tablename=@DIMENSIONTABLENAME)=0
		BEGIN
			insert into @TablesToAddDirtyBitTo select @DIMENSIONTABLENAME
		END

		DECLARE @ADDQUERY varchar(1000);
		set @ADDQUERY = N'ALTER TABLE ' + @DIMENSIONTABLENAME + '_Base_New ADD ' + @COLUMNNAME + ' int';
		SET @CURQUERYFORERROR=@ADDQUERY
		if @DEBUG=1
		BEGIN
			print @ADDQUERY
		END
		exec(@ADDQUERY)

		DECLARE @ADDCONSTRAINT varchar(1000);
		set @ADDCONSTRAINT = N'ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New]  WITH CHECK ADD  CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + '] FOREIGN KEY([' + @COLUMNNAME + '])
								REFERENCES [dbo].[' + @CONSTRAINTTABLENAME + '_New] ([id])
								ALTER TABLE [dbo].[' + @DIMENSIONTABLENAME + '_Base_New] CHECK CONSTRAINT [FK_' + @DIMENSIONTABLENAME + '_Base_New_' + @COLUMNNAME + cast(datediff(second,'1/1/2015',getdate()) as varchar) + ']'

		SET @CURQUERYFORERROR=@ADDCONSTRAINT
		if @DEBUG=1
		BEGIN
			print @ADDCONSTRAINT
		END
		exec(@ADDCONSTRAINT)


		--Need to populate the column now!!!!!!!!!!!!!

		--DECLARE @POPULATEQUERY nvarchar(4000);
	
		--SET @POPULATEQUERY= N'Update ' + @DIMENSIONTABLENAME + ' SET ' + @COLUMNNAME + '=did from (select d.id as did, c.id as cid from DimensionValue d inner join ' + @DIMENSIONTABLENAME + ' c on c.' + @DIMENSIONCOLUMNNAME + '=d.Value where DimensionID=' + @DIMENSIONID + ') A where cid=id'

		--select @POPULATEQUERY

		--print @POPULATEQUERY

		--execute sp_executesql @POPULATEQUERY


		FETCH NEXT FROM Dimension_Cursor
		INTO @COLUMNNAME, @CONSTRAINTTABLENAME, @DIMENSIONTABLENAME, @DIMENSIONCOLUMNNAME, @DIMENSIONID
		End
	Close Dimension_Cursor
	Deallocate Dimension_Cursor

	while (select count(*) from @TablesToAddDirtyBitTo) > 0
	BEGIN
		DECLARE @TableToAdd nvarchar(max)= (select top 1 tablename from @TablesToAddDirtyBitTo)

		exec('ALTER TABLE ' + @TableToAdd + '_Base_New ADD dirty bit ')

		delete from @TablesToAddDirtyBitTo where tablename=@TableToAdd
	END


END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','BaseDynamicColumnCreation',@CURQUERYFORERROR);
	THROW;
END CATCH
END

GO