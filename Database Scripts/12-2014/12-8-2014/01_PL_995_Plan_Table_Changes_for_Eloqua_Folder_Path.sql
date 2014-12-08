/*
   Tuesday, December 2, 20147:12:51 PM
   User: mrp
   Server: stage-new\sql2012
   Database: MRPDev
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'EloquaFolderPath' AND [object_id] = OBJECT_ID(N'Plan'))
BEGIN
  ALTER TABLE dbo.[Plan] DROP COLUMN EloquaFolderPath 
END

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Plan] ADD
	EloquaFolderPath nvarchar(4000) NULL
GO
ALTER TABLE dbo.[Plan] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


