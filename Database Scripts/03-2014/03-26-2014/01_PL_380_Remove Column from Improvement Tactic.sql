Go
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Model_BusinessUnit]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	  ALTER TABLE Model ADD CONSTRAINT FK_Model_BusinessUnit FOREIGN KEY (BusinessUnitId) REFERENCES BusinessUnit(BusinessUnitId)
END

Go
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'BusinessUnitId' AND [object_id] = OBJECT_ID(N'Model'))
BEGIN
    Alter table Model Alter Column BusinessUnitId uniqueidentifier NOT NULL
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Audience]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Audience
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'AudienceId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column AudienceId
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Geography]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Geography
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'GeographyId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column GeographyId
END

GO
IF EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_Plan_Improvement_Campaign_Program_Tactic_Vertical]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
   Alter table Plan_Improvement_Campaign_Program_Tactic drop constraint FK_Plan_Improvement_Campaign_Program_Tactic_Vertical
END

GO
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'VerticalId' AND [object_id] = OBJECT_ID(N'Plan_Improvement_Campaign_Program_Tactic'))
BEGIN
    Alter table Plan_Improvement_Campaign_Program_Tactic drop column VerticalId
END
