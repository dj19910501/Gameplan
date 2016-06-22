IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'DimensionBaseQuery') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION DimensionBaseQuery
GO

CREATE FUNCTION [DimensionBaseQuery] 
( 
    @DIMENSIONTABLENAME NVARCHAR(1000), 
	@STARTDATE date='01/01/2014', 
	@ENDDATE date='12/31/2014',
	@ReportGraphId int ,
	@FilterValues nvarchar(MAX) =null ,
	@IsOnlyDateDimension bit =0,
	@IsDimension bit=0,
	@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
	@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
) 
RETURNS  NVARCHAR(MAX)
BEGIN
DECLARE @OrderByCount int=1
DECLARE @DimensionCount int=0
DECLARE @Dimensionid int,@DimensionIndex NVARCHAR(100),@IsDateDimension BIT, @i INT =3
DECLARE @ExcludeQuery nvarchar(MAX)
SET @ExcludeQuery=''

DECLARE @QueryToRun NVARCHAR(MAX) 
--Chek is only Date dimension then it will get values from dimension value table
IF(@IsOnlyDateDimension=1 AND @FilterValues IS NULL)
BEGIN
SET @DimensionId=(SELECT TOP 1 DimensionId from ReportAxis where reportgraphid=@ReportGraphID )
SET @QueryToRun='';
IF(@IsDimension=0)
SET @QueryToRun =  'SELECT ' + 'DISTINCT #COLUMNS# '  + ' FROM MeasureValue D1 INNER JOIN DimensionValue D3 ON D1.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','') 
ELSE
SET @QueryToRun ='SELECT '+'DISTINCT #COLUMNS# '+'  from dimensionvalue D3 where DimensionID=' + REPLACE(@DIMENSIONTABLENAME,'D','')+' and CAST(DisplayValue AS DATE) between '''+CAST(@STARTDATE AS NVARCHAR)+''' and '''+CAST(@ENDDATE AS NVARCHAR)+''' order by ordervalue1'
END
ELSE
BEGIN
SET @QueryToRun= 'SELECT distinct '+' #COLUMNS# '+' FROM ' + @DIMENSIONTABLENAME + 'Value D1'
       SET @QueryToRun = @QueryToRun + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' D2 ON D2.id = D1.' + @DIMENSIONTABLENAME 

DECLARE Column_Cursor CURSOR FOR
SELECT D.id,D.IsDateDimension,FD.cnt
FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D') FD
INNER JOIN Dimension D ON D.id = FD.dimension
OPEN Column_Cursor
       FETCH NEXT FROM Column_Cursor
       INTO @Dimensionid,@IsDateDimension, @DimensionIndex
       WHILE @@FETCH_STATUS = 0
       BEGIN
	    
		/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
		-- Restrict dimension values as per UserId and RoleId
		--Insertation Start 24/02/2016 Kausha Added if conditionDue to resolve performance issue we have removed unneccesary join for ticket no #729,#730
		IF(@IsDateDimension=1 OR ((SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphID=@ReportGraphId AND DimensionId=@Dimensionid)>0))
    BEGIN
		DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
		IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
		END
		ELSE 
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
		END
		IF(CHARINDEX(',',@RestrictedDimensionValues) = 2)
			BEGIN
				SET @RestrictedDimensionValues = SUBSTRING(@RestrictedDimensionValues,4,LEN(@RestrictedDimensionValues))
			END
		/* End - Added by Arpita Soni for ticket #511 on 10/30/2015 */

	   IF(@ExcludeQuery IS NOT NULL)
	   SET @ExcludeQuery=@ExcludeQuery+' and '
       SET @ExcludeQuery=@ExcludeQuery+'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue not in (select Exclude FROM ReportGraphRowExclude WHERE ReportGraphId=' +cast(@ReportGraphId as nvarchar)+')'

	   /* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
	   IF(ISNULL(@RestrictedDimensionValues,'') != '')
			SET @ExcludeQuery = @ExcludeQuery + ' AND ' + 'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue NOT IN (''' + ISNULL(@RestrictedDimensionValues,'') + ''')'
		/* End - Added by Arpita Soni for ticket #511 on 11/02/2015 */

	   SET @QueryToRun = @QueryToRun + ' INNER JOIN DimensionValue D' + CAST(@i AS NVARCHAR) + ' ON D' + CAST(@i AS NVARCHAR) + '.id = D2.D' + CAST(@DimensionIndex AS NVARCHAR) 
                                                         + ' AND D'+ CAST(@i  AS NVARCHAR) +'.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR) 

       IF(@IsDateDimension = 1)
       BEGIN
              SET @QueryToRun = @QueryToRun + ' and CAST(d' + CAST(@i  AS NVARCHAR) + '.DisplayValue AS DATE) between '''+cast(@STARTDATE as nvarchar)+''' and '''+cast(@ENDDATE as nvarchar)+''''
			  SET @OrderbyCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId and AxisName='X'and Dimensionid=@Dimensionid );
			  SET @DimensionCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId );
			  --Following code is written to identify order by value
			  if(@OrderbyCount=0 and @DimensionCount>1)
			     SET @OrderByCount=2
				 else
				 SET @OrderByCount=1
       END
	      SET @RestrictedDimensionValues= ''
	    END
		SET @i = @i + 1
       FETCH NEXT FROM Column_Cursor
       INTO @Dimensionid,@IsDateDimension, @DimensionIndex
       END
Close Column_Cursor
Deallocate Column_Cursor
--Filters
	DECLARE @FilterCondition NVARCHAR(MAX);
	SET @FilterCondition = ''
	IF(@FilterValues IS NOT NULL)
	BEGIN
	SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
	END
	SET @FilterCondition = ISNULL(@FilterCondition,'')
	IF(@FilterCondition != '')
	BEGIN
	SET @FilterCondition=' where'+@FilterCondition
	SET @FilterCondition =  REPLACE(@FilterCondition,'#',' AND ')
	END

	IF(@FilterCondition is null and @ExcludeQuery is not null)
	SET @ExcludeQuery=' where '+@ExcludeQuery
--Exclude
--Deletion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue no need to pass filter and exclude in both dimension and measure table>
--set @QueryToRun=@QueryToRun+'  '+@FilterCondition+@ExcludeQuery
--Deletion End: <17/02/2016> <Kausha> <Ticket #729,#730>

--Insertion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue we have removed filter from dimension table and exculd from measure table>
IF(@IsDimension=1)
set @QueryToRun=@QueryToRun+'  '+@ExcludeQuery
ELSE
set @QueryToRun=@QueryToRun+'  '+@FilterCondition
--Insertion End: <17/02/2016> <Kausha> <Ticket #729,#730>

IF(@IsDimension=1)
set @QueryToRun=@QueryToRun+' order by ordervalue'+CAST(@OrderByCount as nvarchar)

END
RETURN @QueryToRun 
END

GO
