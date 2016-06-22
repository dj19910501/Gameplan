IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[CreateBaseTableTriggers]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[CreateBaseTableTriggers]  
END
GO

CREATE PROCEDURE [dbo].[CreateBaseTableTriggers]    @DEBUG bit = 0 
AS
BEGIN
	DECLARE @TableList table (id int identity(1,1), TableName nvarchar(max))

	insert into @TableList
	select distinct tablename from Dimension

	DECLARE @c int=(select count(*) from @TableList)

	while @c > 0
	BEGIN
		DECLARE @CurRow int = (select min(id) from @TableList)
		DECLARE @CurTable nvarchar(max) = (select TableName from @TableList where id=@CurRow)

		--We've got the table that we want, now we just need to create the appropriate trigger for that table
		DECLARE @DropQuery nvarchar(max) = 'IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N''[dbo].[' + @CurTable + 'AfterUpdate]''))
			BEGIN
				DROP TRIGGER ' + @CurTable + 'AfterUpdate
			END'
			


			DECLARE @CreateQuery nvarchar(max)='CREATE TRIGGER ' + @CurTable + 'AfterUpdate on dbo.[' + @CurTable + ']
			FOR UPDATE
			AS
				IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = ''dbo'' 
                 AND  TABLE_NAME = ''' + @CurTable + '_Base''))
				BEGIN
					IF NOT EXISTS(SELECT * FROM sys.columns 
								WHERE Name = N''dirty'' AND Object_ID = Object_ID(''' + @CurTable + '_Base''))
					BEGIN
						ALTER TABLE ' + @CurTable + '_Base add dirty bit
					END

					update ' + @CurTable + '_Base set dirty = 1 where id in (select id from inserted)
				END'
			

		--print @DropQuery
		--print @CreateQuery

		--If the table referenced in the dimension table doesn't exist, don't try to create a trigger for it.
		IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = @CurTable))
		BEGIN
			--If there is no base table
			IF(NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = @CurTable + '_Base'))
			BEGIN
				exec ('CREATE TABLE [dbo].[' + @CurTable + '_Base](
						[id] [bigint] NOT NULL,
						[dirty] [bit] NULL
					) ON [PRIMARY]')


			END

			--If the dirty bit doesn't exist, create it
			IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'dirty' AND Object_ID = Object_ID(@CurTable + '_Base'))
			BEGIN
				exec('ALTER TABLE ' + @CurTable + '_Base add dirty bit')
			END

			exec(@DropQuery)
			exec(@CreateQuery)
		END




		delete from @TableList where id=@CurRow
		SET @c = (select count(*) from @TableList)
	END

END