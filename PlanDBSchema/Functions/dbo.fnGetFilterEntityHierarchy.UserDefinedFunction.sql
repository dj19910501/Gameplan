/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 09/13/2016 12:55:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[fnGetFilterEntityHierarchy]
(
	@planIds varchar(max)='''',
	--@customfields varchar(max)=''71_104,71_105'',
	@ownerIds nvarchar(max)='''',
	@tactictypeIds varchar(max)='''',
	@statusIds varchar(max)=''''
)

--select * from fnGetFilterEntityHierarchy(''20220'','''',''56_null'',''41F64F4B-531E-4CAA-8F5F-328E36D9B202'',''31104'',''Created'')
RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15), 
			ColorCode		NVARCHAR(7),
			[Status]		NVARCHAR(15), 
			StartDate		DATETIME, 
			EndDate			DATETIME, 
			CreatedBy		UNIQUEIDENTIFIER
		)
AS
BEGIN


Declare @entTactic varchar(8)=''Tactic''
Declare @entLineItem varchar(10)=''LineItem''

	-- Fill the table variable with the rows for your result set
	

	;WITH FilteredEnt AS(
Select * from fnGetEntitieHirarchyByPlanId(@planIds)
)
,tac as (
	Select distinct ent.* 
	FROM FilteredEnt as ent
	Join [Plan_Campaign_Program_Tactic] as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType=@entTactic AND tac.[Status] IN (select val from comma_split(@statusIds,'','')) and  tac.[CreatedBy] IN (select case when val = '''' then null else Convert(uniqueidentifier,val) end from comma_split(@ownerIds,'',''))
	Join [TacticType] as typ on tac.TacticTypeId = typ.TacticTypeId and typ.IsDeleted=''0'' and typ.[TacticTypeId] IN (select val from comma_split(@tactictypeIds,'',''))
	where ent.EntityType = @entTactic
)
,line as (
	SELECT ent.* 
	FROM FilteredEnt as ent
	JOIN tac on ent.ParentEntityId = tac.EntityId and ent.EntityType=@entLineItem

)

INSERT INTO @Entities
select * from FilteredEnt where EntityType not in (''Tactic'',''LineItem'')
union all
SELECT * FROM tac 
union all
select * from line

RETURN

END
' 
END

GO
