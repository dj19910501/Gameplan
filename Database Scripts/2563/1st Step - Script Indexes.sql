
 --
 --Script all indexes and primary kerys we will drop before we execute 
 --
 --IMPORTANT: Please save the outputs from this script in a file for later use 
 --
 
  DECLARE  @IncludeFileGroup  bit ;
  DECLARE @IncludeDrop       bit;
  DECLARE @IncludeFillFactor bit;

	SET @IncludeDrop = 1;
	SET @IncludeFileGroup = 1;
	SET @IncludeFillFactor = 1;
 
    -- Get all existing indexes, but NOT the primary keys
   DECLARE Indexes_cursor CURSOR FOR 
   SELECT SC.Name          AS      SchemaName,
                   SO.Name          AS      TableName,
                   SI.OBJECT_ID     AS      TableId,
                   SI.[Name]         AS  IndexName,
                   SI.Index_ID       AS  IndexId,
                   FG.[Name]       AS FileGroupName,
                   CASE WHEN SI.Fill_Factor = 0 THEN 100 ELSE SI.Fill_Factor END  Fill_Factor
              FROM sys.indexes SI	              LEFT JOIN sys.filegroups FG
                     ON SI.data_space_id = FG.data_space_id
              INNER JOIN sys.objects SO
                      ON SI.OBJECT_ID = SO.OBJECT_ID
              INNER JOIN sys.schemas SC
                      ON SC.schema_id = SO.schema_id
					  
			  JOIN sys.index_columns SIC ON SIC.object_id = SO.object_id and SI.index_id = SIC.index_id
			  JOIN sys.columns SM ON SM.object_id = SO.object_id

             WHERE OBJECTPROPERTY(SI.OBJECT_ID, 'IsUserTable') = 1
               AND SI.[Name] IS NOT NULL
               AND SI.is_primary_key = 0
               --AND SI.is_unique_constraint = 0
               AND INDEXPROPERTY(SI.OBJECT_ID, SI.[Name], 'IsStatistics') = 0
			    AND (SO.object_id = SI.object_id AND SO.object_id = SIC.object_id AND SO.object_id = SM.object_id
				AND SI.index_id = SIC.index_id AND SIC.column_id = SM.column_id) 
				AND SM.system_type_id = 36
				AND SO.name <> 'ELMAH_Error'
             ORDER BY OBJECT_NAME(SI.OBJECT_ID), SI.Index_ID

    DECLARE @SchemaName     sysname
    DECLARE @TableName      sysname
    DECLARE @TableId        int
    DECLARE @IndexName      sysname
    DECLARE @FileGroupName  sysname
    DECLARE @IndexId        int
    DECLARE @FillFactor     int

    DECLARE @NewLine nvarchar(4000)
    SET @NewLine = char(13) + char(10)
    DECLARE @Tab  nvarchar(4000)
    SET @Tab = SPACE(4)

    -- Loop through all indexes
    OPEN Indexes_cursor

    FETCH NEXT
     FROM Indexes_cursor
     INTO @SchemaName, @TableName, @TableId, @IndexName, @IndexId, @FileGroupName, @FillFactor

    WHILE (@@FETCH_STATUS = 0)
        BEGIN

            DECLARE @sIndexDesc nvarchar(4000)
            DECLARE @sCreateSql nvarchar(4000)
            DECLARE @sDropSql           nvarchar(4000)

            SET @sIndexDesc = '-- Index ' + @IndexName + ' on table ' + @TableName
            SET @sDropSql = 'IF EXISTS(SELECT 1' + @NewLine
                          + '            FROM sysindexes si' + @NewLine
                          + '            INNER JOIN sysobjects so' + @NewLine
                          + '                   ON so.id = si.id' + @NewLine
                          + '           WHERE si.[Name] = N''' + @IndexName + ''' -- Index Name' + @NewLine
                          + '             AND so.[Name] = N''' + @TableName + ''')  -- Table Name' + @NewLine
                          + 'BEGIN' + @NewLine
                          + '    DROP INDEX [' + @IndexName + '] ON [' + @SchemaName + '].[' + @TableName + ']' + @NewLine
                          + 'END' + @NewLine

            SET @sCreateSql = 'CREATE '

            -- Check if the index is unique
            IF (IndexProperty(@TableId, @IndexName, 'IsUnique') = 1)
                BEGIN
                    SET @sCreateSql = @sCreateSql + 'UNIQUE '
                END
            --END IF
            -- Check if the index is clustered
            IF (IndexProperty(@TableId, @IndexName, 'IsClustered') = 1)
                BEGIN
                    SET @sCreateSql = @sCreateSql + 'CLUSTERED '
                END
            --END IF

            SET @sCreateSql = @sCreateSql + 'INDEX [' + @IndexName + '] ON [' + @SchemaName + '].[' + @TableName + ']' + @NewLine + '(' + @NewLine

            -- Get all columns of the index
            DECLARE IndexColumns_cursor CURSOR
                FOR SELECT SC.[Name],
                           IC.[is_included_column],
                           IC.is_descending_key
                      FROM sys.index_columns IC
                     INNER JOIN sys.columns SC
                             ON IC.OBJECT_ID = SC.OBJECT_ID
                            AND IC.Column_ID = SC.Column_ID
                     WHERE IC.OBJECT_ID = @TableId
                       AND Index_ID = @IndexId
                     ORDER BY IC.[is_included_column],
                              IC.key_ordinal

            DECLARE @IxColumn      sysname
            DECLARE @IxIncl        bit
            DECLARE @Desc          bit
            DECLARE @IxIsIncl      bit
            SET @IxIsIncl = 0
            DECLARE @IxFirstColumn   bit
            SET @IxFirstColumn = 1

            -- Loop through all columns of the index and append them to the CREATE statement
            OPEN IndexColumns_cursor
            FETCH NEXT
             FROM IndexColumns_cursor
             INTO @IxColumn, @IxIncl, @Desc

            WHILE (@@FETCH_STATUS = 0)
                BEGIN
                    IF (@IxFirstColumn = 1)
                        BEGIN
                            SET @IxFirstColumn = 0
                        END
                    ELSE
                        BEGIN
                            --check to see if it's an included column
                            IF (@IxIsIncl = 0) AND (@IxIncl = 1)
                                BEGIN
                                    SET @IxIsIncl = 1
                                    SET @sCreateSql = @sCreateSql + @NewLine + ')' + @NewLine + 'INCLUDE' + @NewLine + '(' + @NewLine
                                END
                            ELSE
                                BEGIN
                                    SET @sCreateSql = @sCreateSql + ',' + @NewLine
                                END
                            --END IF
                        END
                    --END IF

                    SET @sCreateSql = @sCreateSql + @Tab + '[' + @IxColumn + ']'
                    -- check if ASC or DESC
                    IF @IxIsIncl = 0
                        BEGIN
                            IF @Desc = 1
                                BEGIN
                                    SET @sCreateSql = @sCreateSql + ' DESC'
                                END
                            ELSE
                                BEGIN
                                    SET @sCreateSql = @sCreateSql + ' ASC'
                                END
                            --END IF
                        END
                    --END IF
                    FETCH NEXT
                     FROM IndexColumns_cursor
                     INTO @IxColumn, @IxIncl, @Desc
                END
            --END WHILE
            CLOSE IndexColumns_cursor
            DEALLOCATE IndexColumns_cursor

            SET @sCreateSql = @sCreateSql + @NewLine + ') '

            IF @IncludeFillFactor = 1
                BEGIN
                    SET @sCreateSql = @sCreateSql + @NewLine + 'WITH (FillFactor = ' + CAST(@FillFactor AS varchar(13)) + ')' + @NewLine
                END
            --END IF

            IF @IncludeFileGroup = 1
                BEGIN
                    SET @sCreateSql = @sCreateSql + 'ON ['+ @FileGroupName + ']' + @NewLine
                END
            ELSE
                BEGIN
                    SET @sCreateSql = @sCreateSql + @NewLine
                END
            --END IF

            PRINT '-- **********************************************************************'
            PRINT @sIndexDesc
            PRINT '-- **********************************************************************'

            IF @IncludeDrop = 1
                BEGIN
                    PRINT @sDropSql
                    PRINT 'GO'
                END
            --END IF

            PRINT @sCreateSql
            PRINT 'GO' + @NewLine  + @NewLine

            FETCH NEXT
             FROM Indexes_cursor
             INTO @SchemaName, @TableName, @TableId, @IndexName, @IndexId, @FileGroupName, @FillFactor
        END
    --END WHILE
    CLOSE Indexes_cursor
    DEALLOCATE Indexes_cursor


	--------------------------------------------------

	----Generate primary indexes


DECLARE @object_id int;
DECLARE @parent_object_id int;
DECLARE @TSQL NVARCHAR(4000);
DECLARE @COLUMN_NAME SYSNAME;
DECLARE @is_descending_key bit;
DECLARE @col1 BIT;
DECLARE @action CHAR(6);
 
--SET @action = 'DROP';
SET @action = 'CREATE';
 
DECLARE PKcursor CURSOR FOR
    select kc.object_id, kc.parent_object_id
    from sys.key_constraints kc
    inner join sys.objects o
    on kc.parent_object_id = o.object_id
    where kc.type = 'PK' and o.type = 'U'
    and o.name not in ('dtproperties','sysdiagrams')  -- not true user tables

	AND kc.name IN (   SELECT sys.indexes.name
			FROM sys.tables, sys.indexes, sys.index_columns, sys.columns 
			WHERE (sys.tables.object_id = sys.indexes.object_id AND sys.tables.object_id = sys.index_columns.object_id AND sys.tables.object_id = sys.columns.object_id
				AND sys.indexes.index_id = sys.index_columns.index_id AND sys.index_columns.column_id = sys.columns.column_id) 
				AND sys.columns.system_type_id = 36
				AND sys.tables.name <> 'ELMAH_Error'
				AND sys.indexes.is_primary_key = 1)

    order by QUOTENAME(OBJECT_SCHEMA_NAME(kc.parent_object_id))
            ,QUOTENAME(OBJECT_NAME(kc.parent_object_id));
 
OPEN PKcursor;
FETCH NEXT FROM PKcursor INTO @object_id, @parent_object_id;
  
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @action = 'DROP'
        SET @TSQL = 'ALTER TABLE '
                  + QUOTENAME(OBJECT_SCHEMA_NAME(@parent_object_id))
                  + '.' + QUOTENAME(OBJECT_NAME(@parent_object_id))
                  + ' DROP CONSTRAINT ' + QUOTENAME(OBJECT_NAME(@object_id))
    ELSE
        BEGIN
        SET @TSQL = 'ALTER TABLE '
                  + QUOTENAME(OBJECT_SCHEMA_NAME(@parent_object_id))
                  + '.' + QUOTENAME(OBJECT_NAME(@parent_object_id))
                  + ' ADD CONSTRAINT ' + QUOTENAME(OBJECT_NAME(@object_id))
                  + ' PRIMARY KEY'
                  + CASE INDEXPROPERTY(@parent_object_id
                                      ,OBJECT_NAME(@object_id),'IsClustered')
                        WHEN 1 THEN ' CLUSTERED'
                        ELSE ' NONCLUSTERED'
                    END
                  + ' (';
 
        DECLARE ColumnCursor CURSOR FOR
            select COL_NAME(@parent_object_id,ic.column_id), ic.is_descending_key
            from sys.indexes i
            inner join sys.index_columns ic
            on i.object_id = ic.object_id and i.index_id = ic.index_id
            where i.object_id = @parent_object_id
            and i.name = OBJECT_NAME(@object_id)
            order by ic.key_ordinal;
 
        OPEN ColumnCursor;
 
        SET @col1 = 1;
 
        FETCH NEXT FROM ColumnCursor INTO @COLUMN_NAME, @is_descending_key;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF (@col1 = 1)
                SET @col1 = 0
            ELSE
                SET @TSQL = @TSQL + ',';
 
            SET @TSQL = @TSQL + QUOTENAME(@COLUMN_NAME)
                      + ' '
                      + CASE @is_descending_key
                            WHEN 0 THEN 'ASC'
                            ELSE 'DESC'
                        END;
 
            FETCH NEXT FROM ColumnCursor INTO @COLUMN_NAME, @is_descending_key;
        END;
 
        CLOSE ColumnCursor;
        DEALLOCATE ColumnCursor;
 
        SET @TSQL = @TSQL + ');';
 
        END;
 
    PRINT @TSQL;
 
    FETCH NEXT FROM PKcursor INTO @object_id, @parent_object_id;
END;
 
CLOSE PKcursor;
DEALLOCATE PKcursor;