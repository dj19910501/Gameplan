IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RebuildIndexes') AND xtype IN (N'P'))
    DROP PROCEDURE RebuildIndexes
GO

CREATE PROCEDURE [RebuildIndexes] @DEBUG bit=0
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TableName varchar(255) 
	DECLARE TableCursor CURSOR FOR 
	--added braces "[" and "]" to the table name PL Ticket # 812 Issue in RebuildIndexes stored procedure in Mongo database
	SELECT '[' + table_name + ']' FROM information_schema.tables 
	WHERE table_type = 'base table' and (table_name like 'd%d%' or TABLE_NAME in ('DimensionValue','DynamicDimension','MeasureValue') or table_name like '%_Value%') 
	OPEN TableCursor 
 
	FETCH NEXT FROM TableCursor INTO @TableName 
	WHILE @@FETCH_STATUS = 0 
	BEGIN 
		--added try catch PL Ticket # 812 Issue in RebuildIndexes stored procedure in Mongo database
		BEGIN TRY
			DBCC DBREINDEX(@TableName,' ',90) 
			exec ('alter table ' +  @TableName + ' REBUILD')
		END TRY
		BEGIN CATCH
		END CATCH
		FETCH NEXT FROM TableCursor INTO @TableName 
	END 
 
	CLOSE TableCursor 
	DEALLOCATE TableCursor 
	exec sp_updatestats
END

GO
