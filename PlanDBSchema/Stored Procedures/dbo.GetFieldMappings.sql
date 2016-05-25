/****** Object:  StoredProcedure [dbo].[GetFieldMappings]    Script Date: 05/25/2016 4:42:53 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFieldMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFieldMappings]
GO
/****** Object:  StoredProcedure [dbo].[GetFieldMappings]    Script Date: 05/25/2016 4:42:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFieldMappings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetFieldMappings] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetFieldMappings] 
	@entityType varchar(255)='Tactic',
	@clientId uniqueidentifier,
	@integrationTypeId int,
	@id int=0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Exec GetFieldMappings 'Tactic','464EB808-AD1F-4481-9365-6AADA15023BD',3,1190

BEGIN
	Declare @Table TABLE (sourceFieldName NVARCHAR(250),destinationFieldName NVARCHAR(250),marketoFieldType varchar(255))
	Declare @ColumnName nvarchar(max)
	Declare @actCampaignFolder varchar(255)='CampaignFolder'
	Declare @trgtCampaignFolder varchar(255)='Id'
	Declare @actNote varchar(255)='Note'
	Declare @trgtNote varchar(255)='note'
	Declare @actMarketoProgramId varchar(255)='MarketoProgramId'
	Declare @trgtMarketoProgramId varchar(255)=''
	Declare @actCostStartDate varchar(255)='CostStartDate'
	Declare @trgtCostStartDate varchar(255)='CostStartDate'
	Declare @actMode varchar(255)='Mode'
	Declare @trgtMode varchar(255)=''
	Declare @modeCREATE varchar(20)='Create'
	Declare @modeUPDATE varchar(20)='Update'
	Declare @actCost varchar(30)='Cost'
	Declare @tblTactic varchar(255)='Plan_Campaign_Program_Tactic'
	Declare @varGlobal varchar(100)='Global'
	Declare @costFieldType varchar(255)='costs'
	Declare @tagsFieldType varchar(255)='tags'
	Declare @folderFieldType varchar(255)='Folder'
	Declare @actActivityType varchar(255)='ActivityType'
	Declare @actCreatedBy varchar(255)='CreatedBy'
	Declare @actPlanName varchar(255)='PlanName'
	Declare @actStatus varchar(255)='Status'

END

;With ResultTable as(

(
		-- Actualfield Query
		Select  gpDataType.ActualFieldName as sourceFieldName,
				TargetDataType as destinationFieldName,
				CASE 
					WHEN gpDataType.ActualFieldName=@actCost THEN @costFieldType
					WHEN gpDataType.ActualFieldName=@actActivityType THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actCreatedBy THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actPlanName THEN @tagsFieldType
					WHEN gpDataType.ActualFieldName=@actStatus THEN @tagsFieldType
				END as marketoFieldType
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=@tblTactic OR gpDataType.TableName=@varGlobal) and IsNull(gpDataType.IsGet,'0') = '0' and gpDataType.GamePlanDataTypeId >0
	)
	UNION
	(
		-- CustomField Query
		SELECT  custm.Name as sourceFieldName,
				TargetDataType as destinationFieldName,
				@tagsFieldType as marketoFieldType
		FROM IntegrationInstanceDataTypeMapping as mapp
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
--INSERT INTO @Table SELECT @actMarketoProgramId as sourceFieldName,@trgtMarketoProgramId as destinationFieldName,Null as marketoFieldType

IF EXISTS (Select sourceFieldName from @Table where sourceFieldName =@actCost)
BEGIN
	INSERT INTO @Table SELECT @actNote as sourceFieldName,@trgtNote as destinationFieldName,@costFieldType as marketoFieldType
	INSERT INTO @Table SELECT @actCostStartDate as sourceFieldName,@trgtCostStartDate as destinationFieldName,@costFieldType as marketoFieldType
END
INSERT INTO @Table SELECT @actCampaignFolder as sourceFieldName,@trgtCampaignFolder as destinationFieldName,@folderFieldType as marketoFieldType

select * from @Table
END


GO
