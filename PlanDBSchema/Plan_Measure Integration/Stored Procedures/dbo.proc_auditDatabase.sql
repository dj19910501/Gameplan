IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'proc_auditDatabase') AND xtype IN (N'P'))
    DROP PROCEDURE proc_auditDatabase
GO


-- =============================================
-- Author:		Nate Lee
-- Create date: 2011 September 15
-- Description:	This procedure will poll a table loaded with data and run the standard data audit metrics
-- =============================================
CREATE PROCEDURE [proc_auditDatabase]
	-- Add the parameters for the stored procedure here
	@tableName nvarchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @columnName nvarchar(255);
	DECLARE @unique int;
	DECLARE @singleValue int;
	DECLARE @top20 int;
	DECLARE @notNull int;
	
	DECLARE @uniqueTable table (
		tempColumnName nvarchar(255)
	)
	
	DECLARE @singleValueTable table (
		tempColumnName nvarchar(255)
		, tempAmount int
	)

	DECLARE @top20Table table (
		tempColumnName nvarchar(255)
		, tempAmount int
	)

	DECLARE @notNullTable table (
		tempColumnName nvarchar(255)
	)	

	-- get the column names
	DECLARE cur CURSOR FOR
	SELECT syscolumns.name AS ColumnName
	FROM sysobjects 
	JOIN syscolumns ON sysobjects.id = syscolumns.id
	WHERE sysobjects.xtype = 'U'
	AND sysobjects.name = @tableName
	ORDER BY sysobjects.name, syscolumns.colid
	
	OPEN cur
	
	FETCH NEXT FROM cur INTO @columnName;
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		DECLARE @sql1 nvarchar(4000)
		DECLARE @ParmDefinition1 nvarchar(500);
		DECLARE @sql2 nvarchar(4000)
		DECLARE @ParmDefinition2 nvarchar(500);
		DECLARE @sql3 nvarchar(4000)
		DECLARE @ParmDefinition3 nvarchar(500);
		DECLARE @sql4 nvarchar(4000)
		DECLARE @ParmDefinition4 nvarchar(500);
		
		-- Unique Values
		SET @sql1 = N'SELECT @singleValueOUT = COUNT(*) FROM (
				SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field, Count(*) AS Amounta
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				GROUP BY RTRIM(LTRIM([' + @columnName + ']))
				) as x'
		SET @ParmDefinition1 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql1, @ParmDefinition1, @singleValueOUT=@unique OUTPUT;

		---- Single Values
		--SET @sql2 = N'SELECT @singleValueOUT = COUNT(*) FROM (
		--		SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field, Count(*) AS Amounta
		--		FROM Nate..[' + @tableName + ']
		--		WHERE [' + @columnName + '] IS NOT NULL
		--		AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
		--		GROUP BY RTRIM(LTRIM([' + @columnName + ']))
		--		HAVING COUNT(*) = 1 ) as x'
		--SET @ParmDefinition2 = N'@singleValueOUT int OUTPUT';			 
		--EXEC sp_executesql @sql2, @ParmDefinition2, @singleValueOUT=@singleValue OUTPUT;

		-- Not NULL
		SET @sql3 = N'SELECT @singleValueOUT = COUNT(*) FROM (
				SELECT RTRIM(LTRIM([' + @columnName + '])) AS Field
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				) as x'
		SET @ParmDefinition3 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql3, @ParmDefinition3, @singleValueOUT=@notNull OUTPUT;
		
		-- Top 20
		SET @sql4 = N'SELECT @singleValueOUT = SUM(x.AMT) FROM (
				SELECT TOP 20 RTRIM(LTRIM([' + @columnName + '])) AS Field, COUNT(*) AS AMT
				FROM TDA..[' + @tableName + ']
				WHERE [' + @columnName + '] IS NOT NULL
				AND RTRIM(LTRIM([' + @columnName + '])) <> ''''
				GROUP BY RTRIM(LTRIM([' + @columnName + ']))
				ORDER BY AMT DESC
				) as x'
		SET @ParmDefinition4 = N'@singleValueOUT int OUTPUT';			 
		EXEC sp_executesql @sql4, @ParmDefinition4, @singleValueOUT=@top20 OUTPUT;

		
		--INSERT INTO @notNullTable (tempColumnName)
		--	SELECT @columnName
		--	FROM Nate..[ZEBRA - Middle Tier]
		--	WHERE @columnName IS NOT NULL
		--	AND @columnName <> ''

		--SET @unique = (SELECT COUNT(*) FROM @uniqueTable);
		--SET @singleValue = (SELECT COUNT(*) FROM @singleValueTable WHERE tempAmount = 1);
		--SET @notNull = (SELECT COUNT(*) FROM @notNullTable);
		
		-- Insert statements for procedure here
		-- How many unique records?
		--INSERT INTO DataAuditResults ([Dataset], [column Name], [Unique Values], [Single Value], [Not Null], [Top 20 Coverage]) 
		--VALUES (@tableName, @columnName, @unique, @singleValue, @notNull, @top20);	
		INSERT INTO DataAuditResults ([Dataset], [column Name], [Unique Values], [Not Null], [Top 20 Coverage]) 
		VALUES (@tableName, @columnName, @unique, @notNull, @top20);	
		
		DELETE FROM @uniqueTable
--		DELETE FROM @singleValueTable
		DELETE FROM @notNullTable
		DELETE FROM @top20Table
		
		FETCH NEXT FROM cur INTO @columnName
	END
	
	CLOSE cur
	DEALLOCATE cur

END


GO
