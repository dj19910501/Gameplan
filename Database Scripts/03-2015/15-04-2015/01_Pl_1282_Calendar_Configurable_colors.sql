-- Run This script on MRP

GO
/****** Object:  Table [dbo].[EntityTypeColor]    Script Date: 04/14/2015 12:00:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'EntityTypeColor'))
BEGIN
	CREATE TABLE [dbo].[EntityTypeColor](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[EntityType] [nvarchar](50) NULL,
		[ColorCode] [nvarchar](50) NULL,
	 CONSTRAINT [PK_EntityTypeColor] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

GO

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'EntityTypeColor'))
BEGIN

	IF (NOT EXISTS (SELECT * from dbo.EntityTypeColor where EntityType = 'Plan'))
	   INSERT dbo.EntityTypeColor (EntityType,ColorCode) VALUES  ('Plan','3db9d3')

	IF (NOT EXISTS (SELECT * from dbo.EntityTypeColor	where EntityType = 'Campaign'))
	   INSERT dbo.EntityTypeColor (EntityType,ColorCode) VALUES  ('Campaign','3db9d3')

	IF (NOT EXISTS (SELECT * from dbo.EntityTypeColor	where EntityType = 'Program'))
	   INSERT dbo.EntityTypeColor (EntityType,ColorCode) VALUES  ('Program','3db9d3')

	IF (NOT EXISTS (SELECT * from dbo.EntityTypeColor	where EntityType = 'Tactic'))
	   INSERT dbo.EntityTypeColor (EntityType,ColorCode) VALUES  ('Tactic','3db9d3')

	IF (NOT EXISTS (SELECT * from dbo.EntityTypeColor where EntityType = 'ImprovementTactic')) 
	   INSERT dbo.EntityTypeColor  (EntityType,	ColorCode) VALUES  ('ImprovementTactic','3db9d3')

End
        
Go
   