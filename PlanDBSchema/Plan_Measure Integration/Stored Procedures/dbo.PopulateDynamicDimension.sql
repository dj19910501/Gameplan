IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'PopulateDynamicDimension') AND xtype IN (N'P'))
    DROP PROCEDURE PopulateDynamicDimension
GO


CREATE PROCEDURE [PopulateDynamicDimension] @DEBUG bit = 0
AS
BEGIN

DECLARE @CURQUERYFORERROR varchar(max)=null;
BEGIN TRY

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

	--truncate table DynamicDimension

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicDimension_New]') AND type in (N'U'))
		CREATE TABLE [dbo].[DynamicDimension_New](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[TableName] [nvarchar](500) NOT NULL,
		[Dimensions] [int] NULL,
		[DimensionTableName] [nvarchar](50) NULL,
		[ComputeAllValues] [bit] NULL,
		[DimensionValueTableName] [nvarchar](100) NULL,
		[ContainsDateDimension] [bit] NULL
	) ON [PRIMARY]

	truncate table DynamicDimension_New
	truncate table AggregationQueries
	DBCC CHECKIDENT ('dbo.AggregationQueries', RESEED, 1);
	



	DECLARE @DCount int;
	set @DCOUNT=(select count(*) from dimension);

	DECLARE @c int;
	set @c=2

	while @c<= @DCount
		Begin
		
			DECLARE @CurQuery varchar(MAX);
		
			DECLARE @Part1 varchar(MAX);
			DECLARE @Part2 varchar(MAX);
			DECLARE @Part3 varchar(MAX);
			DECLARE @Part4 varchar(MAX);
			DECLARE @Part5 varchar(MAX);

			DECLARE @Column_Drop varchar(MAX);
			DECLARE @Column_Add varchar(MAX);

			set @Part1=''
			set @Part2=''
			set @Part3=''
			set @Part4=''
			set @Part5=''

			set @Column_Drop=''
			set @Column_Add=''

			DECLARE @sCount int;
			set @sCount=1
			while @sCount <= @c
				Begin
					set @Part1 = @Part1 + ' ''d'' + cast(d' + cast(@sCount as varchar) + '.id as varchar)';
					set @Part2 = @Part2 + ' dimension d' + cast(@sCount as varchar) + ' ';
					set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.IsDeleted=0 '
					if @sCount < @c
						BEGIN
						set @Part1 = @Part1 + ' + '
						set @Part2 = @Part2 + ', '
						set @Part3 = @Part3 + ' and d' + cast(@sCount as varchar) + '.id<d' + cast((@sCount+1) as varchar) + '.id and '
						set @Part3 = @Part3 + ' d' + cast(@sCount as varchar) + '.TableName=d' + cast((@sCount+1) as varchar) + '.TableName '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) + '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) + '
						set @Part3 = @Part3 + ' and ';
						END
					ELSE
						BEGIN
						set @Part1 = @Part1 + ', d' + cast(@sCount as varchar) + '.TableName ';
						set @Part2 = @Part2 + ' '
						set @Part4 = @Part4 + ' cast(isNull( d' + cast(@sCount as varchar) +'.ComputeAllValues,0) as int) '
						set @Part5 = @Part5 + ' cast(isNull( d' + cast(@sCount as varchar) +'.IsDateDimension,0) as int) '
						END

					set @sCount = @sCount + 1
				end;




			set @CurQuery = N'insert into DynamicDimension_New (TableName, DimensionTableName, Dimensions, ComputeAllValues, containsDateDimension) select ' + @Part1 + ', ' + cast(@c as varchar) + ', case when ' + @Part4 + '=0 then 0 else 1 end, case when ' + @Part5 + '=0 then 0 else 1 end from ' + @Part2 + ' where ' + @Part3;
		
			--print @CurQuery + ' '  +  @Part1  + ' ' +  @Part2 + ' ' +  @Part3


			set @CURQUERYFORERROR=@CurQuery
			if @DEBUG=1
			BEGIN
				print @CurQuery
			END
			exec (@CurQuery);
			--print @CurQuery;

			set @c=@c+1
		end;

--Added for work on #189 by BCG
DECLARE @Count int;
DECLARE @CurID int;
DECLARE @DimensionTableName varchar(MAX);
DECLARE @DimensionValueTableName varchar(MAX);
DECLARE @LastDimensionTableName varchar(MAX);

SET @Count=0;
SET @LastDimensionTableName=null;

DECLARE Dimension_Cursor CURSOR FOR
select DimensionTableName, id from DynamicDimension_New
order by DimensionTableName
OPEN Dimension_Cursor
FETCH NEXT FROM Dimension_Cursor
INTO @DimensionTableName, @CurID
while @@FETCH_STATUS = 0
	BEGIN
	if @LastDimensionTableName is null or @LastDimensionTableName<>@DimensionTableName
		BEGIN
		SET @LastDimensionTableName=@DimensionTableName
		SET @Count=0;
		END
	
	SET @DimensionValueTableName = @DimensionTableName + '_Value' + cast(@Count/250 as varchar)

	DECLARE @CurSQL varchar(max);

	SET @CurSQL='Update DynamicDimension_New set DimensionValueTableName=''' + @DimensionValueTableName + ''' where ID =' + cast(@CurID as varchar)


	set @CURQUERYFORERROR=@CurSQL
	if @DEBUG=1
	BEGIN
		print @CurSQL
	END
	exec(@CurSQL)

	SET @Count=@Count+1

	FETCH NEXT FROM Dimension_Cursor
	INTO @DimensionTableName, @CurID
	END

Close Dimension_Cursor
Deallocate Dimension_Cursor





END TRY
BEGIN CATCH
	INSERT INTO AggregationProcessLog VALUES (GETDATE(),GETDATE(),'ERROR',ERROR_MESSAGE(),GETDATE(),'bcg','PopulateDynamicDimension',@CURQUERYFORERROR);
	THROW;
END CATCH
END



GO
