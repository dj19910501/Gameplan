
--IMPORTANT: PLEASE FOLLOW THE INSTRUCTIONS BELOW BEFORE YOU RUN THE SCRIPT, OR ELSE YOU MAY CAUSE DAMAGES TO YOUR EXISTING DATABASE
--BACK UP YOUR DATABASE FIRST BEFORE YOU EXECUTE THE SCRIPT

DECLARE @authDb nvarchar(256);
SET @authDb = 'Hive9AuthJuly2016'


DECLARE @sql nvarchar (max)

DECLARE @exclusions table (Name nvarchar (128));
INSERT INTO @exclusions (Name) VALUES ('ELMAH_Error');
INSERT INTO @exclusions 
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'VIEW'

--CREATE CLIENTID2/USERID2 ETC. COLUMNS ON ALL TABLES THAT HAVE CLIENTID COLUMN. 
--NOTE - can only run this section once 
SELECT  @sql = COALESCE(@sql + '', '') + 
'If NOT EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS C WHERE TABLE_NAME = ''' + A.TABLE_NAME +''' and COLUMN_NAME = '''+ A.COLUMN_NAME+'2zz'') 
ALTER TABLE dbo.[' + A.TABLE_NAME +']
  ADD '+ A.COLUMN_NAME+'2zz INT NOT NULL DEFAULT (0);
  ' 
FROM INFORMATION_SCHEMA.COLUMNS A 
WHERE DATA_TYPE = 'uniqueidentifier'
	AND A.TABLE_NAME NOT IN (SELECT NAME FROM @exclusions)

--PRINT @sql;
EXECUTE sp_executesql @sql;



--COPY ID COLUMN FROM CLIENT TABLE TO CLIENTID2 COLOUMNS ON ALL TABLES THAT HAS CLIENTID COLUMN 
SET @sql = null;
select  @sql = COALESCE(@sql + '', '') + '
UPDATE dbo.[' + A.TABLE_NAME +'] 
  SET ' + A.COLUMN_NAME + '2zz = C.ID
  FROM [' + @authDb+ '].dbo.[Client] C
  WHERE C.ClientID = dbo.[' + A.TABLE_NAME + '].[' + A.COLUMN_NAME + '];' 
FROM INFORMATION_SCHEMA.COLUMNS A 
WHERE DATA_TYPE = 'uniqueidentifier'
	AND A.TABLE_NAME NOT IN (SELECT NAME FROM @exclusions)

--print @sql;
EXECUTE sp_executesql @sql;



--COPY ID FROM USER TABLE - similar to Client table 
SET @sql = null;
SELECT  @sql = COALESCE(@sql + '', '') + '
UPDATE dbo.[' + A.TABLE_NAME +'] 
  SET ' + A.COLUMN_NAME + '2zz = U.ID
  FROM [' + @authDb+ '].dbo.[User] U
  WHERE U.UserID = dbo.[' + A.TABLE_NAME + '].[' + A.COLUMN_NAME + '];' 
FROM INFORMATION_SCHEMA.COLUMNS A 
WHERE A.DATA_TYPE = 'uniqueidentifier'
	AND A.TABLE_NAME NOT IN (SELECT NAME FROM @exclusions)

--print @sql;
EXECUTE sp_executesql @sql;




--REMOVE PK CONSTRAINTS 
SET @sql = null;
SELECT  @sql = COALESCE(@sql + '', '') + '
ALTER TABLE [' + sys.tables.name + ']
DROP CONSTRAINT [' + sys.indexes.name + '];' 

FROM sys.tables, sys.indexes, sys.index_columns, sys.columns 
WHERE (sys.tables.object_id = sys.indexes.object_id AND sys.tables.object_id = sys.index_columns.object_id AND sys.tables.object_id = sys.columns.object_id
	AND sys.indexes.index_id = sys.index_columns.index_id AND sys.index_columns.column_id = sys.columns.column_id) 
	AND sys.columns.system_type_id = 36
	AND sys.tables.name NOT IN (SELECT NAME FROM @exclusions)
	AND sys.indexes.is_primary_key = 1;
--PRINT @sql;
EXECUTE sp_executesql @sql;



--REMOVE INDEXES USING UNIQUEIDENTIFIER TYPED COLUMNS
SET @sql = null;
SELECT  @sql = COALESCE(@sql + '', '') + '
DROP INDEX [' + sys.indexes.name + '] ON ' + sys.tables.name + 
'
'
FROM sys.tables, sys.indexes, sys.index_columns, sys.columns 
WHERE (sys.tables.object_id = sys.indexes.object_id AND sys.tables.object_id = sys.index_columns.object_id AND sys.tables.object_id = sys.columns.object_id
	AND sys.indexes.index_id = sys.index_columns.index_id AND sys.index_columns.column_id = sys.columns.column_id) 
	AND sys.columns.system_type_id = 36
	AND sys.tables.name NOT IN (SELECT NAME FROM @exclusions)
--PRINT @sql;
EXECUTE sp_executesql @sql;


--DELETE ALLL UNIQUEIDENTIFIER TYPED COLUMNS FROM ALL REFERENCE TABLES
SET @sql = null;
SELECT  @sql = COALESCE(@sql + '', '') + 
'ALTER TABLE dbo.[' + A.TABLE_NAME +']
  DROP Column ' + A.COLUMN_NAME + ';
  ' 
FROM INFORMATION_SCHEMA.COLUMNS A 
WHERE DATA_TYPE = 'uniqueidentifier'
	AND A.TABLE_NAME NOT IN (SELECT NAME FROM @exclusions)

PRINT @sql;
EXECUTE sp_executesql @sql;


--RENAME CLIENTID2 TO CLIENTID 
SET @sql = null;
select  @sql = COALESCE(@sql + '', '') + 
'
EXEC sp_RENAME ''dbo.[' + A.TABLE_NAME + '].[' + A.COLUMN_NAME + ']'', ''' + SUBSTRING(A.COLUMN_NAME, 0, LEN(A.COLUMN_NAME)-2) + ''', ''COLUMN''
' 
FROM INFORMATION_SCHEMA.COLUMNS A 
WHERE RIGHT(A.COLUMN_NAME, 3) = '2zz'
--print @sql;
EXECUTE sp_executesql @sql;
