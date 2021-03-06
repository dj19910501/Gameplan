/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/24/2016 1:53:46 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSFDCFieldMappings]
GO
/****** Object:  StoredProcedure [dbo].[GetSFDCFieldMappings]    Script Date: 06/24/2016 1:53:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCFieldMappings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetSFDCFieldMappings] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetSFDCFieldMappings] 
	@clientId uniqueidentifier,
	@integrationTypeId int,
	@id int=0,
	@isSFDCMarketoIntegration bit='0'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Exec GetSFDCFieldMappings '464EB808-AD1F-4481-9365-6AADA15023BD',2,1203

BEGIN
	Declare @Table TABLE (sourceFieldName NVARCHAR(250),destinationFieldName NVARCHAR(250),fieldType varchar(255))
	Declare @ColumnName nvarchar(max)
	Declare @trgtCampaignFolder varchar(255)='Id'
	Declare @actMode varchar(255)='Mode'
	Declare @trgtMode varchar(255)=''
	Declare @modeCREATE varchar(20)='Create'
	Declare @modeUPDATE varchar(20)='Update'
	Declare @actCost varchar(30)='Cost'
	Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'
	Declare @tblProgram varchar(255)='Plan_Campaign_Program'
	Declare @tblCampaign varchar(255)='Plan_Campaign'
	Declare @tblImprvTactic varchar(255)='Plan_Improvement_Campaign_Program_Tactic'
	Declare @varGlobal varchar(100)='Global'
	Declare @varGlobalImprovemnt varchar(100)='GlobalImprovement'
	Declare @actActivityType varchar(255)='ActivityType'
	Declare @actCreatedBy varchar(255)='CreatedBy'
	Declare @actPlanName varchar(255)='PlanName'
	Declare @actStatus varchar(255)='Status'
	Declare @entityTypeTac varchar(255)='Tactic'
	Declare @entityTypeProg varchar(255)='Program'
	Declare @entityTypeCampgn varchar(255)='Campaign'
	Declare @entityTypeImprvTac varchar(255)='ImprovementTactic'
	Declare @entityTypeImprvProg varchar(255)='ImprovementProgram'
	Declare @entityTypeImprvCamp varchar(255)='ImprovementCampaign'
	Declare @entityTypeIntegrationInstance varchar(255)='IntegrationInstance'
	Declare @actsfdcParentId varchar(50)='ParentId'
END

;With ResultTable as(

(
		-- Add Campaign,Program, Tactic, Improvement Campaign,Program & Tactic Or Global fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				CASE 
					WHEN gpDataType.TableName=@tblTactic THEN @entityTypeTac
					WHEN gpDataType.TableName=@tblProgram THEN @entityTypeProg
					WHEN gpDataType.TableName=@tblCampaign THEN @entityTypeCampgn
					WHEN gpDataType.TableName=@tblImprvTactic THEN @entityTypeImprvTac
					ELSE @varGlobal
				END as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@tblTactic OR gpDataType.TableName=@tblProgram OR gpDataType.TableName=@tblCampaign OR gpDataType.TableName=@tblImprvTactic OR gpDataType.TableName=@varGlobal) and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- Add Improvement Global Fields
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				 @varGlobalImprovemnt as fieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and gpDataType.TableName=@varGlobal and IsImprovement='1' and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- CustomField Query
		SELECT  custm.Name as sourceFieldName,
				TargetDataType as destinationFieldName,
				custm.EntityType as fieldType
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and ((custm.EntityType=@entityTypeTac) OR (custm.EntityType=@entityTypeProg) OR (custm.EntityType=@entityTypeCampgn))
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
--IF(@isSFDCMarketoIntegration='1')
--BEGIN
--	-- Insert ParentId field for Campaign,Program & Tactic
--	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobal as fieldType

--	-- Insert ParentId field for Improvement Campaign,Program & Tactic
--	INSERT INTO @Table SELECT @actsfdcParentId as sourceFieldName,@actsfdcParentId as destinationFieldName,@varGlobalImprovemnt as fieldType
--END
select * from @Table
END


GO
