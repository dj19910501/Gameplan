/****** Object:  UserDefinedFunction [dbo].[GetTacCustomNameMappingList]    Script Date: 05/26/2016 9:45:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacCustomNameMappingList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTacCustomNameMappingList]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTacCustomNameMappingList]    Script Date: 05/26/2016 9:45:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTacCustomNameMappingList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[GetTacCustomNameMappingList]
(
	@entityType varchar(255)=''Tactic'',
	@clientId uniqueidentifier,
	@EntityIds varchar(max)=''''
)

--SELECT * from  GetTacCustomNameMappingList(''Tactic'',''464EB808-AD1F-4481-9365-6AADA15023BD'',N''94028,94016'')
RETURNS @tac_Type_Cust Table(
PlanTacticId int,
CustomName varchar(max)
)
AS
BEGIN
	
--	Declare @entityType varchar(255)=''Tactic''
--Declare @clientId uniqueidentifier=''464EB808-AD1F-4481-9365-6AADA15023BD''
--Declare @TacticIds varchar(max)=''''
Declare @actTitleField varchar(50)=''Title''
Declare @actPlanTacticIdField varchar(50)=''PlanTacticId''

Declare @tbl_Tac_Custm_Type table(
PlanTacticId int,
TacticTitle varchar(max),
CustomFieldId int,
CustomFieldValue varchar(max),
TacticType varchar(max),
TableName varchar(1000),
[Sequence] int
)


;WITH tacTitle as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''Plan_Campaign_Program_Tactic'' as TableName
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
tacType as(
SELECT 
	T.PlanTacticId,
	T.Title,
	''TacticType'' as TableName,
	T.TacticTypeId
FROM Plan_Campaign_Program_Tactic T 
WHERE PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
),
EntityTableWithValues AS (
select distinct SUBSTRING(@entityType,1,1) +''-'' + cast(EntityId as nvarchar) + ''-'' + cast(Extent1.CustomFieldID as nvarchar) as keyv, 
		cast([Extent1].[CustomFieldId] as nvarchar) as CustomFieldId,
		cast(EntityId as nvarchar) as EntityId,
		case      
			when A.keyi is not null then Extent2.AbbreviationForMulti
			when Extent3.[Name]=''TextBox'' then Extent1.Value     
			when Extent3.[Name]=''DropDownList'' then Extent4.Value 
		End as ValueV, 
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
INSERT INTO @tbl_Tac_Custm_Type
SElect * from (

-- START: Tactic Title ---
(SELECT 
	T.PlanTacticId,
	CASE 
		WHEN CNC.CustomNameCharNo is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](CASE WHEN FieldName=@actPlanTacticIdField THEN Cast(T.PlanTacticId as varchar(10)) ELSE T.Title END) +''_'' 
		ELSE SUBSTRING([dbo].[RemoveSpaceAndUppercaseFirst](CASE WHEN FieldName=@actPlanTacticIdField THEN Cast(T.PlanTacticId as varchar(10)) ELSE T.Title END),1,CNC.CustomNameCharNo) +''_'' 
	END as ''TacticTitle'',
	Null as CustomFieldId,
	Null as CustomFieldValue,
	Null as TacticType,
	CNC.TableName,
	CNC.[Sequence]
FROM tacTitle as T
Inner JOIN CampaignNameConvention as CNC on T.TableName = CNC.TableName and CNC.IsDeleted=0 and CNC.ClientId=@clientId
where T.PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
)
-- END: Tactic Title ---
UNION 
-- START: Tactic Type ---
(SELECT 
	T.PlanTacticId,
	NULL as ''TacticTitle'',
	Null as CustomFieldId,
	Null as CustomFieldValue,
	CASE 
		WHEN CNC.CustomNameCharNo is null THEN 
											CASE 
												WHEN TP.Abbreviation is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](TP.Title) +''_'' 
												ELSE [dbo].[RemoveSpaceAndUppercaseFirst](TP.Abbreviation) +''_''
											END
		ELSE SUBSTRING(
						CASE 
							WHEN TP.Abbreviation is null THEN [dbo].[RemoveSpaceAndUppercaseFirst](TP.Title)
							ELSE [dbo].[RemoveSpaceAndUppercaseFirst](TP.Abbreviation)
						END
					   ,1,CNC.CustomNameCharNo) +''_'' 
	END as TacticType,
	CNC.TableName,
	CNC.[Sequence]
FROM tacType as T
Inner JOIN TacticType as TP ON T.TacticTypeId = TP.TacticTypeId and TP.IsDeleted=0
Inner JOIN CampaignNameConvention as CNC on T.TableName = CNC.TableName and CNC.IsDeleted=0 and CNC.ClientId=@clientId
where T.PlanTacticId IN (select val from comma_split(@EntityIds,'',''))
)
-- END: Tactic Type ---
UNION 

--order by keyv
--select * from EntityTableWithValues;

(SELECT 
	    E.EntityId as PlanTacticId,
		NULL as ''TacticTitle'',
		E.CustomFieldId as CustomFieldId,
		CASE 
			WHEN CNC.CustomNameCharNo is null THEN E.CustomName +''_'' 
			ELSE SUBSTRING(E.CustomName,1,CNC.CustomNameCharNo) +''_'' 
		END as CustomFieldValue,
		Null as TacticType,
		CNC.TableName,
		CNC.[Sequence]
FROM EntityTableWithValues as E
Inner JOIN CampaignNameConvention as CNC on E.CustomFieldId = CNC.CustomFieldId and CNC.IsDeleted=0 and CNC.ClientId=@clientId
WHERE E.EntityId IN (select val from comma_split(@EntityIds,'',''))
)
) as tac_type_custm
Order by PlanTacticId,[Sequence]
;
--select * from @tbl_Tac_Custm_Type
INSERT INTO @tac_Type_Cust
Select Main.PlanTacticId,
       Left(Main.[subcustm],Len(Main.[subcustm])-1) As CustomName
From
    (
 SELECT distinct ST2.PlanTacticId, 
            (
                SELECT ST1.CustomName + '''' AS [text()]
                FROM (
						SELECT T1.PlanTacticId,
							CASE 
								WHEN T1.TableName =''CustomField'' THEN T1.CustomFieldValue 
								WHEN T1.TableName =''Plan_Campaign_Program_Tactic'' THEN T1.TacticTitle 
								WHEN T1.TableName =''TacticType'' THEN T1.TacticType 
							 END  as CustomName	
						FROM @tbl_Tac_Custm_Type T1
					) ST1
                Where ST1.PlanTacticId = ST2.PlanTacticId
                ORDER BY ST1.PlanTacticId
                For XML PATH ('''')
            ) [subcustm]
        From (
				SELECT T1.PlanTacticId,
							CASE 
								WHEN T1.TableName =''CustomField'' THEN T1.CustomFieldValue 
								WHEN T1.TableName =''Plan_Campaign_Program_Tactic'' THEN T1.TacticTitle 
								WHEN T1.TableName =''TacticType'' THEN T1.TacticType 
							 END  as CustomName	
					from @tbl_Tac_Custm_Type T1
) ST2
)Main
	
	RETURN 
END
' 
END

GO
