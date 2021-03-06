IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetMeasures') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION GetMeasures
GO

CREATE FUNCTION [GetMeasures] 
(   @ReportGraphID int,
 @ViewByValue nvarchar(10),
  @DimensionCount int,
 @GType NVARCHAR(100),
 @DisplayStatSignificance NVARCHAR(15) = NULL
) 
RETURNS @MeasureTable TABLE(SelectTableColumn NVARCHAR(2000),--Select Column
CreateTableColumn NVARCHAR(2000),--CreateColumn
MeasureName NVARCHAR(2000),--MeasureName
SymbolType NVARCHAR(50)--MeasureName
 )
BEGIN
  declare @MeasureName nvarchar(100)
  declare @Column nvarchar(max)
  set @Column=''
  declare @SymbolType nvarchar(50)
  declare @MeasureTableColumn nvarchar(max)
  set @MeasureTableColumn=''

  set @Column=''
			set @Column=@Column+','+'0 as Rows'
			set @MeasureTableColumn=@MeasureTableColumn+','+'Rows INT'
 DECLARE @MeasureCursor CURSOR
			SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT Measurename,SymbolTyPe FROM dbo.GetGraphMeasure(@ReportGraphId,@ViewByValue) order by columnorder
			OPEN @MeasureCursor
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@SymbolType
			WHILE @@FETCH_STATUS = 0 
			BEGIN
			IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND  LOWER(@DisplayStatSignificance)!='rate')
			SET @MeasureName='Measure_'+@MeasureName
			set @Column=@Column+',0 as ['+@MeasureName+']'
			set @MeasureTableColumn=@MeasureTableColumn+', ['+@MeasureName+'] FLOAT'
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@SymbolType
			END
			CLOSE @MeasureCursor
			DEALLOCATE @MeasureCursor
			Insert into @MeasureTable(SelectTableColumn,CreateTableColumn,MeasureName,SymbolType)values(@column,@MeasureTableColumn,@MeasureName,@SymbolType)
			return 
END
--ENd GetMeasure

GO
