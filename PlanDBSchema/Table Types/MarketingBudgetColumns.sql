-- ================================
-- Create User-defined Table Type
-- Created By Nishant Sheth
-- ================================
IF TYPE_ID('[MarketingBudgetColumns]') IS NOT NULL
   BEGIN
     DROP TYPE [MarketingBudgetColumns]
   END
GO  
-- Create the data type
CREATE TYPE [dbo].[MarketingBudgetColumns] AS TABLE(
	[Month] [nvarchar](15) NULL,
	[ColumnName] [nvarchar](255) NULL,
	[ColumnIndex] [bigint] NULL
)
GO