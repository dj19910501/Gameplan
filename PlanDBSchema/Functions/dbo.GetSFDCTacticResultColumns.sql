/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:00:39 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetSFDCTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetSFDCTacticResultColumns]    Script Date: 06/10/2016 11:00:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSFDCTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- SElect [GetSFDCTacticResultColumns] (1203,''464EB808-AD1F-4481-9365-6AADA15023BD'',2)
CREATE FUNCTION [dbo].[GetSFDCTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN
Declare @imprvCost varchar(20)=''ImprvCost''
Declare @actImprvCost varchar(20)=''Cost''
declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

	;With ResultTable as(

(
		Select  IntegrationInstanceID,
				IsNull(gpDataType.GameplanDataTypeId,0) as GameplanDataTypeId,
				TableName,
				-- Rename actualfield ''Cost'' to ''ImprvCost''  in case of Table Name ''Plan_Improvement_Campaign_Program_Tactic'' to ignore conflict of same name ''Cost'' actual field of both Tactic & Improvement Tactic table
				CASE 
					WHEN  ((gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'') AND (gpDataType.ActualFieldName=@actImprvCost)) THEN @imprvCost 
					ELSE gpDataType.ActualFieldName
				END AS ActualFieldName,
				TargetDataType,
				IsNull(mapp.CustomFieldId,0) as CustomFieldId
		FROM GamePlanDataType as gpDataType
		JOIN IntegrationInstanceDataTypeMapping as mapp ON gpDataType.GamePlanDataTypeId = mapp.GamePlanDataTypeId and mapp.IntegrationInstanceId=@id
		Where gpDataType.IsDeleted=0 and gpDataType.IntegrationTypeId = @integrationTypeId and (gpDataType.TableName=''Plan_Campaign_Program_Tactic'' OR gpDataType.TableName=''Plan_Campaign_Program'' OR gpDataType.TableName=''Plan_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program'' OR gpDataType.TableName=''Plan_Improvement_Campaign_Program_Tactic'' OR gpDataType.TableName=''Global'') and IsNull(gpDataType.IsGet,''0'') = ''0'' and gpDataType.GamePlanDataTypeId >0
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
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and (custm.EntityType=''Tactic'' or custm.EntityType=''Campaign'' or custm.EntityType=''Program'')
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select Distinct ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END

' 
END

GO
