IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasuresForMeasureTable') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasuresForMeasureTable
GO
CREATE FUNCTION [dbo].[GetMeasuresForMeasureTable] 
(   
	@ReportGraphID int,
    @MeasureId int,
	@RateList NVARCHAR(50) = ''
) 
RETURNS  nvarchar(1000)
BEGIN
declare @MeasureName nvarchar(100)
declare @AggrigationType nvarchar(100)
declare @Column nvarchar(max)
declare @DimensionTableMeasure nvarchar(max)
declare @SymbolType nvarchar(max)
declare @Count int =1
     

  set @Column=''
			set @Column=',SUM(d1.Rows) as Rows'
 DECLARE @MeasureCursor CURSOR
			SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT Top 1 m.name,m.AggregationType,rc.SymbolType from reportgraphcolumn rc inner join measure m on    rc.reportgraphid=@ReportGraphId and m.id=@MeasureId
			OPEN @MeasureCursor
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			WHILE @@FETCH_STATUS = 0 
			BEGIN
			if(@AggrigationType='AVG')
			BEGIN
			IF(@SymbolType = '%'  )
			BEGIN
			set @Column=@Column+','+' ISNULL(ROUND((sum(D1.Value*d1.rows)/sum(d1.rows))*100,2),0) AS  ['+@MeasureName+']'			
			END
			ELSE
			BEGIN
				IF(@RateList = '' OR @RateList IS NULL)
				BEGIN
					set @Column=@Column+','+ 'ISNULL(ROUND(sum(D1.Value*d1.rows)/sum(d1.rows),2),0) AS ['+@MeasureName+']'
				END
				ELSE
				BEGIN
					SET @Column=@Column+','+ 'ISNULL(ROUND((sum(D1.Value*d1.rows)/sum(d1.rows))*avg(ISNULL(rt.Rate,0)),2),0) AS ['+@MeasureName+']'
				END
			END
			END
			ELSE
				IF(@RateList = '' OR @RateList IS NULL)
				BEGIN
					set @Column=@Column+','+'ISNULL(ROUND(SUM(d1.Value),2),0) as ['+@MeasureName+']'
				END
				ELSE
				BEGIN
					set @Column=@Column+','+'ISNULL(ROUND(SUM(d1.Value)* avg(ISNULL(rt.Rate,0)),2),0) as ['+@MeasureName+']'
				END
			--Add Rows
			set @Count=@count+1
			
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			END
			CLOSE @MeasureCursor
			DEALLOCATE @MeasureCursor
			return @Column
END

GO
