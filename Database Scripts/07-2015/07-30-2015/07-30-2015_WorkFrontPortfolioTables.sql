USE [BulldogGameplanMay2015]
GO

/****** Object:  Table [dbo].[IntegrationWorkFrontPortfolios]    Script Date: 7/30/2015 3:55:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontPortfolios'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontPortfolios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IntegrationInstanceId] [int] NOT NULL,
	[PortfolioId] [nvarchar](50) NOT NULL,
	[PortfolioName] [nvarchar](250) NOT NULL,
	[PlanProgramId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontPortfolios_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[IntegrationWorkFrontPortolio_Mapping]    Script Date: 7/30/2015 3:55:49 PM ******/
IF (not EXISTS (SELECT * FROM sys.TABLES WHERE name = 'IntegrationWorkFrontPortfolio_Mapping'))
begin
CREATE TABLE [dbo].[IntegrationWorkFrontPortolio_Mapping](
	[ID] [int] NOT NULL,
	[PortfolioTableId] [int] NOT NULL,
	[ProjectId] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_IntegrationWorkFrontPortolio_Mapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF not exists (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
	 WHERE CONSTRAINT_NAME ='FK_PortfolioTableId' )
begin
ALTER TABLE [dbo].[IntegrationWorkFrontPortolio_Mapping]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioTableId] FOREIGN KEY([PortfolioTableId])
REFERENCES [dbo].[IntegrationWorkFrontPortfolios] ([Id])
ALTER TABLE [dbo].[IntegrationWorkFrontPortolio_Mapping] CHECK CONSTRAINT [FK_PortfolioTableId]
end
GO



