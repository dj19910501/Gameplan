/* script for remove null constrint in Plan_Campaign and Plan_Campaign_Program table for VerticalId, AudienceId and GeographyId column   */
ALTER TABLE Plan_Campaign ALTER COLUMN  VerticalId int  NULL 
ALTER TABLE Plan_Campaign ALTER COLUMN  AudienceId int  NULL 
ALTER TABLE Plan_Campaign ALTER COLUMN  GeographyId uniqueidentifier  NULL 

ALTER TABLE Plan_Campaign_Program ALTER COLUMN  VerticalId int  NULL 
ALTER TABLE Plan_Campaign_Program ALTER COLUMN  AudienceId int  NULL 
ALTER TABLE Plan_Campaign_Program ALTER COLUMN  GeographyId uniqueidentifier  NULL 

