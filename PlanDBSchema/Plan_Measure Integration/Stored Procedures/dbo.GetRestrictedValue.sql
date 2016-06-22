CREATE PROCEDURE [dbo].[GetRestrictedValues]

@SelectedDiemsnionId INT=12,
@DashboardId INT=3,
@dashBoardPageId INT=0,
@Filters NVARCHAR(MAX)='<filters><filter ID="2">2554</filter><filter ID="2">2556</filter></filters>'
AS

BEGIN
DECLARE @DynamicFilters TABLE
(
 DiemnsionId INt,
 RestrictedDiemnsionId INT,
 RestrictedDiemnsionValue NVARCHAR(1000)   
)
BEGIN TRY
DECLARE @TableName NVARCHAR(1000)=''
Declare @SelectedDiemsnionCount INT=0
Declare @SelectedDiemsnionNo INT=0
DECLARE @DimensionIdTemp INT
--Following is cursor to prepare Table Name
DECLARE @TablNameCursor CURSOR
SET @TablNameCursor = CURSOR FOR
SELECT D.id from Dimension D
INNER JOIN Dimension D1 On D1.TableName=D.TableName
where D1.id =@SelectedDiemsnionId Order By id
OPEN @TablNameCursor
FETCH NEXT
FROM @TablNameCursor INTO @DimensionIdTemp
WHILE @@FETCH_STATUS = 0
BEGIN
--PRINT @DimensionId
SET @SelectedDiemsnionNo=@SelectedDiemsnionNo+1;
 SET @TableName=@TableName+'D'+CAST(@DimensionIdTemp AS NVARCHAR)
 IF(@DimensionIdTemp=@SelectedDiemsnionId)
 SET @SelectedDiemsnionCount=@SelectedDiemsnionNo

FETCH NEXT
FROM @TablNameCursor INTO @DimensionIdTemp
END
CLOSE @TablNameCursor
PRINT @TableName
DEALLOCATE @TablNameCursor
--End TableName Cursor

DECLARE @DimensionId INT
DECLARE @IsDateDiemnsion Bit=0
DECLARE @Count INT=0

DECLARE @FilterCursor CURSOR
SET @FilterCursor = CURSOR FOR
SELECT D.id,D.IsDateDimension from Dimension D
INNER JOIN Dimension D1 On D1.TableName=D.TableName
where D1.id =@SelectedDiemsnionId Order By id
OPEN @FilterCursor
FETCH NEXT
FROM @FilterCursor INTO @DimensionId,@IsDateDiemnsion
WHILE @@FETCH_STATUS = 0
BEGIN
--PRINT @DimensionId
SET @Count=@Count+1

IF(@IsDateDiemnsion=0 AND @DimensionId!=@SelectedDiemsnionId )

BEGIN

DECLARE @Query NVARCHAR(MAX)='SELECT '+CAST(@SelectedDiemsnionId AS NVARCHAR)+',DimensionId,Value from dimensionValue WHere Dimensionid in ('+CAST(@DimensionId as NVARCHAR) +' ) 
AND Value not in(
select dv2.value from '+@TableName+' dv 
Inner Join Dimensionvalue Dv2 on dv2.id=dv.d'+CAST(@Count AS NVARCHAR)+ '
Inner Join Dimension Di on di.id=dv2.DimensionID  
AND di.id='+CAST(@DimensionId AS nvarchar)+' WHERE dv.d'+CAST(@SelectedDiemsnionCount AS NVARCHAR)+' In(select DimensionValueId from dbo.ExtractTableFromXml('''+@Filters+''')))'


INSERT INTO @DynamicFilters EXEC(@Query)
END

FETCH NEXT
FROM @FilterCursor INTO @DimensionId,@IsDateDiemnsion
END
CLOSE @FilterCursor
DEALLOCATE @FilterCursor

SELECT * FROM @DynamicFilters
END TRY


BEGIN CATCH

SELECT * FROM @DynamicFilters

END CATCH
END