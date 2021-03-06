
/****** Object:  UserDefinedFunction [dbo].[GetSourceTargetMappingData]    Script Date: 05/24/2016 6:59:20 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSourceTargetMappingData]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSourceTargetMappingData]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSourceTargetMappingData]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetSourceTargetMappingData]
(
	@entityType varchar(255)=''Tactic'',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)='''',
	@integrationTypeId int,
	@id int=0,
	@SFDClength int=255	-- default value 255
)

--SELECT * from  [GetSourceTargetMappingData](''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''94016,94028,94029,94030'',3,1176,255)
RETURNS @src_trgt_mappdata Table(
ActualFieldName varchar(max),
CustomFieldId int,
TacValue varchar(max),
SourceId int
)
AS
BEGIN

Declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
Declare @ColumnName nvarchar(max)
Declare @actCampaignFolder varchar(255)=''CampaignFolder''
Declare @trgtCampaignFolder varchar(255)=''Id''
Declare @actMarketoProgramId varchar(255)=''MarketoProgramId''
Declare @trgtMarketoProgramId varchar(255)=''''
Declare @actMode varchar(255)=''Mode''
Declare @trgtMode varchar(255)=''''
Declare @modeCREATE varchar(20)=''Create''
Declare @modeUPDATE varchar(20)=''Update''
Declare @actNote varchar(255)=''Note''
Declare @trgtNote varchar(255)=''note''
Declare @actCostStartDate varchar(255)=''CostStartDate''
Declare @trgtCostStartDate varchar(255)=''CostStartDate''
Declare @actCreatedBy varchar(255)=''CreatedBy''
Declare @tblTactic varchar(255)=''Plan_Campaign_Program_Tactic''

-------- START: Get Standard & CustomField Mappings data --------
BEGIN
	;With ResultTable as(
	
	(
			Select  IntegrationInstanceID,
					IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
					TableName,
					gpDataType.ActualFieldName,
					TargetDataType,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId
			FROM GamePlanDataType as gpDataType
			JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
			Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
		)
		UNION
		(
			SELECT  mapp.IntegrationInstanceId,
					0 as GameplanDataTypeId,
					Null as TableName,
					custm.Name as ActualFieldName,
					TargetDataType,
					IsNull(mapp.CustomFieldId,0) as CustomFieldId
					
			FROM IntegrationInstanceDataTypeMapping as mapp
			JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=@entityType
			WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
		)
	
	)
	insert into @Table 
	select * from ResultTable
END
-------- END: Get Standard & CustomField Mappings data --------
-------- START: Insert fixed Marketo fields to Mapping list. -------- 
BEGIN
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMarketoProgramId as ActualFieldName,@trgtMarketoProgramId as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actMode as ActualFieldName,@trgtMode as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCampaignFolder as ActualFieldName,@trgtCampaignFolder as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actNote as ActualFieldName,@trgtNote as TargetDataType,0 as CustomFieldId
	INSERT INTO @Table SELECT @id,0,@tblTactic as TableName,@actCostStartDate as ActualFieldName,@trgtCostStartDate as TargetDataType,0 as CustomFieldId
END
-------- END: Insert fixed Marketo fields to Mapping list. -------- 

;WITH tact as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''Tactic_Mapp'' as Link
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
IntegMapp as(
	SELECT 
		Mapp.*,
		''Tactic_Mapp'' as Link
	FROM @Table as Mapp 
),
 CustomFieldValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as Value, 
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value
			when Extent3.[Name]=''DropDownList'' then 
												CASE
													 WHEN Extent4.Abbreviation IS nOT NULL THEN Extent4.Abbreviation 
													 ELSE Extent4.Value 
													 END   
												END as CustomName 
from CustomField_Entity Extent1 
INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] AND [Extent2].[IsDeleted] = 0 
INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and cast(Extent1.Value as nvarchar)=cast(Extent4.CustomFieldOptionID as nvarchar)
Left Outer join ( 
					select SUBSTRING(@entityType,1,1) +''-''  + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyi  from CustomField_Entity Extent1
					INNER JOIN [dbo].[CustomField] AS [Extent2] ON [Extent1].[CustomFieldId] = [Extent2].[CustomFieldId] 
					INNER Join CustomFieldType Extent3 on Extent2.CustomFieldTypeId=Extent3.CustomFieldTypeId 
					Left Outer join CustomFieldOption Extent4 on Extent4.CustomFieldId=Extent2.CustomFieldId and Extent1.Value=Extent4.CustomFieldOptionID 
					WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))) 
					Group by SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
					having count(*) > 1 
				) A on A.keyi=SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) 
WHERE ([Extent1].[EntityId] IN (select val from comma_split(@EntityIds,'',''))
)
)

INSERT INTO @src_trgt_mappdata
select Mapp.ActualFieldName,
		Mapp.CustomFieldId,
		 CASE 
			WHEN Mapp.ActualFieldName=@actMarketoProgramId THEN ISNull(Tac.IntegrationInstanceMarketoID,'''')
			WHEN Mapp.ActualFieldName=@actMode THEN (CASE 
														WHEN ISNULL(Tac.IntegrationInstanceMarketoID,'''')='''' THEN @modeCREATE 
														ELSE @modeUPDATE 
													 END)  
			WHEN Mapp.ActualFieldName=''ProgramType'' THEN ISNull(marktType.ProgramType,'''')
			WHEN Mapp.ActualFieldName=''Channel'' THEN ISNull(marktType.Channel,'''')
			WHEN Mapp.ActualFieldName=''Status'' THEN ISNull(Tac.[Status],'''')
			WHEN Mapp.ActualFieldName=''ActivityType'' THEN ''Tactic''
			WHEN Mapp.ActualFieldName=''PlanName'' THEN ISNull(pln.Title,'''')
			WHEN Mapp.ActualFieldName=''Cost'' THEN ISNull(Cast(Tac.Cost as varchar(255)),'''')
			WHEN Mapp.ActualFieldName=''TacticType'' THEN ISNull(TT.Title,'''')
			WHEN Mapp.ActualFieldName=''Name'' THEN ISNull(SUBSTRING(Tac.TacticCustomName,1,@SFDClength),'''')
			WHEN Mapp.ActualFieldName=''Description'' THEN ISNull(Tac.[Description],'''')
			WHEN Mapp.ActualFieldName=@actCampaignFolder THEN ISNull(marktFolder.MarketoCampaignFolderId,'''')
			WHEN Mapp.CustomFieldId >0 THEN ISNULL(custm.Value,''None'')
			WHEN Mapp.ActualFieldName=@actNote THEN ''Planned Cost''
			WHEN Mapp.ActualFieldName=@actCostStartDate THEN ISNull(CONVERT(VARCHAR(19),Tac.StartDate),'''')
			WHEN Mapp.ActualFieldName=@actCreatedBy THEN ISNull(Cast(Tac.CreatedBy as varchar(100)),'''')
		END AS TacValue,
		T.PlanTacticId as SourceId
from IntegMapp as Mapp
INNER JOIN tact as T ON Mapp.Link = T.Link
LEFT JOIN Plan_Campaign_Program_Tactic as Tac ON T.PlanTacticId = Tac.PlanTacticId
LEFT JOIN Plan_Campaign_Program as prg ON Tac.PlanProgramId = prg.PlanProgramId and prg.IsDeleted=0
LEFT JOIN Plan_Campaign as cmpgn ON cmpgn.PlanCampaignId = prg.PlanCampaignId and cmpgn.IsDeleted=0
LEFT JOIN [Plan] as pln ON pln.PlanId = cmpgn.PlanId and pln.IsDeleted=0
LEFT JOIN TacticType as TT ON Tac.TacticTypeId = TT.TacticTypeId and TT.IsDeleted=0
LEFT JOIN CustomFieldValues as custm ON Mapp.CustomFieldId=custm.CustomFieldId and T.PlanTacticId = custm.EntityId
LEFT JOIN MarketoEntityValueMapping as marktFolder ON pln.PlanId = marktFolder.EntityID and marktFolder.EntityType=''Plan'' and marktFolder.IntegrationInstanceId =@id 
LEFT JOIN MarketoEntityValueMapping as marktType ON TT.TacticTypeId = marktType.EntityID and marktType.EntityType=''TacticType'' 
	RETURN 
END' 
END

GO
