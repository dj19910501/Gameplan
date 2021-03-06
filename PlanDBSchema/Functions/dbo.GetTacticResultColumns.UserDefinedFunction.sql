
/****** Object:  UserDefinedFunction [dbo].[GetTacticResultColumns]    Script Date: 05/24/2016 6:59:20 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacticResultColumns]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacticResultColumns]    Script Date: 05/24/2016 6:59:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacticResultColumns]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[GetTacticResultColumns]
(
	@id int,
	@clientId uniqueidentifier,
	@integrationTypeId int
)
RETURNS nvarchar(max)
AS
BEGIN

declare @Table TABLE (IntegrationInstanceID INT,GameplanDataTypeId INT,TableName NVARCHAR(250),ActualFieldName NVARCHAR(250),TargetDataType NVARCHAR(250),CustomFieldId INT)
declare @ColumnName nvarchar(max)

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
		JOIN Customfield as custm ON mapp.CustomFieldId = custm.CustomFieldId and custm.ClientId=@clientId and custm.IsDeleted=0 and custm.EntityType=''Tactic''
		WHERE  mapp.IntegrationInstanceId=@id and mapp.CustomFieldId >0
	)

)

insert into @Table 
select * from ResultTable
  
  SELECT @ColumnName= ISNULL(@ColumnName + '','','''') 
       + QUOTENAME(ActualFieldName)
FROM (Select DISTINCT ActualFieldName FROM @Table) AS ActualFields
RETURN @ColumnName
END
' 
END

GO
