/* ------------- Start - Related to PL ticket #1449 ------------- 
Created By: Viral
Created Date: 03/16/2016
Description: Add Columns 'LastAutoSyncDate' & 'ForceSyncUser' in table IntegrationInstance
*/

IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'LastAutoSyncDate' AND OBJECT_ID = OBJECT_ID(N'[IntegrationInstance]'))
BEGIN
Alter Table [dbo].[IntegrationInstance] 
ADD  LastAutoSyncDate DateTime
END 
GO

IF NOT EXISTS(SELECT * FROM sys.columns
WHERE Name = N'ForceSyncUser' AND OBJECT_ID = OBJECT_ID(N'[IntegrationInstance]'))
BEGIN
Alter Table [dbo].[IntegrationInstance] 
ADD  ForceSyncUser uniqueidentifier
END 
GO

/* ------------- End - Related to PL ticket #1449 ------------- */

-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](50) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](50) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(10)
declare @release nvarchar(10)
set @release = 'Mar23.2016'
set @version = 'Mar23.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO