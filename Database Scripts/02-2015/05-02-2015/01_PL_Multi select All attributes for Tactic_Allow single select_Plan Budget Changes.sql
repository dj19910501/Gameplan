--------------- Start: Add Column 'CostWeightage' to Table [dbo].[CustomField_Entity] --------------- 
--- Execute this script in MRP Database.
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'CostWeightage' AND [object_id] = OBJECT_ID(N'[dbo].[CustomField_Entity]'))
BEGIN
    ALTER TABLE [dbo].[CustomField_Entity]
	ADD CostWeightage tinyint NULL
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Attribute wise weight of entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CostWeightage'
END
GO
IF (SELECT Count(*) FROM CustomField_Entity WHERE CostWeightage > 0) = 0
BEGIN
	Update [dbo].[CustomField_Entity] Set CostWeightage = 100
END
GO
--------------- End: Add Column 'CostWeightage' to Table [dbo].[CustomField_Entity] --------------- 
