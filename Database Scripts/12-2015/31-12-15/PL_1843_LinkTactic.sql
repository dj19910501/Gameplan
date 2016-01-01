/* --------- Start Script of PL ticket #1749 --------- */
-- Created by : Komal Rawal
-- Created On : 12/28/2015
-- Description : Insert columns  in Plan_Campaign_Program_Tactic table for linking tactic .

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]') 
         AND name = 'LinkedTacticId'
)
BEGIN
                ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic]
                ADD LinkedTacticId INT NULL
END
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic]') 
         AND name = 'LinkedPlanId'
)
BEGIN
                ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic]
                ADD LinkedPlanId INT NULL
END
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Plan_Campaign_Program_Tactic_LineItem]') 
         AND name = 'LinkedLineItemId'
)
BEGIN
                ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem]
                ADD LinkedLineItemId INT NULL
END
GO
