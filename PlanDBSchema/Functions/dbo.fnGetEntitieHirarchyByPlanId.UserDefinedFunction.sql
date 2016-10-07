
/****** Object:  UserDefinedFunction [dbo].[fnGetEntitieHirarchyByPlanId]    Script Date: 10/07/2016 12:42:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetEntitieHirarchyByPlanId]    Script Date: 10/07/2016 12:42:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetEntitieHirarchyByPlanId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'--This function will return all the enties with hirarchy
--Multiple plan ids can be passed saperated by comma
--If we pass null then it will retuen all plans hirarchy data
CREATE FUNCTION [dbo].[fnGetEntitieHirarchyByPlanId] ( @PlanIds NVARCHAR(MAX),@TimeFrame VARCHAR(20)='''',@isGrid bit=0)
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
			CreatedBy		INT,
			AltId			NVARCHAR(500),
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ModelId			BIGINT,
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN

	DECLARE @TimeFrameDatesAndYear Table
	(
		PlanYear varchar(10)
	)

	DECLARE @MinYear VARCHAR(10)--minimum year passed in timeframe perameter
	DECLARE @MaxYear VARCHAR(10)--Maximum year passed in timeframe perameter
	DECLARE @StartDate DateTime --Start date perameter to filter entities e.g. campaign,program and tactic
	DECLARE @EndDate DateTime   --End date perameter to filter entities e.g. campaign,program and tactic

	--Set first year with first start date of year and last year with last date of year
	IF (@TimeFrame IS NOT NULL AND @TimeFrame <>'''')
		BEGIN
			INSERT INTO @TimeFrameDatesAndYear
			SELECT Item as PlanYear from dbo.SplitString(@TimeFrame,''-'')--split timeframe parameter e.g. 2015-2016
	
			SELECT @MinYear=CONVERT(VARCHAR,( MIN(CONVERT(INT,PlanYear)))) FROM @TimeFrameDatesAndYear --Set Minimum year
			SELECT @MaxYear=CONVERT(VARCHAR,( MAX(CONVERT(INT,PlanYear)))) FROM @TimeFrameDatesAndYear --Set Maximum year
	
			SET @StartDate= CONVERT(DATETIME,@MinYear+''-01-01 00:00:00'') --Set first date of minimum year
			SET @EndDate= CONVERT(DATETIME,@MaxYear+''-12-31 00:00:00'')   --Set Last date of maximum year

			IF (CHARINDEX(''-'',@TimeFrame)=0)
				BEGIN
					INSERT INTO @TimeFrameDatesAndYear
					SELECT CONVERT(VARCHAR(10),(CONVERT(INT,@TimeFrame)-1)) --split timeframe parameter e.g. 2015-2016
				END
		END
	

	;WITH FilteredPlan AS(
		SELECT ''Plan'' EntityType,''P_'' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId,P.PlanId EntityId, P.Title EntityTitle,NULL ParentEntityId,NULL ParentUniqueId, P.Status, NULL StartDate, NULL EndDate,P.CreatedBy 
		,CAST(P.PlanId AS NVARCHAR(50)) AS AltId
		,''L''+CAST(P.PlanId AS NVARCHAR(50)) AS TaskId
		,NULL AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM [Plan] P 
			--INNER JOIN Model M ON M.ModelId = P.ModelId AND M.ClientId = @ClientId
		WHERE P.IsDeleted = 0 
				AND 
				(@isGrid=1 OR P.[Year] in (SELECT PlanYear from @TimeFrameDatesAndYear))
			AND (
					@PlanIds IS NULL 
					OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,'',''))
				)
	),
	Campaigns AS (
		SELECT ''Campaign'' EntityType,''P_C_'' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId,C.PlanCampaignId EntityId, C.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, C.Status, C.StartDate StartDate, C.EndDate EndDate,C.CreatedBy 
		,CAST(C.PlanId AS NVARCHAR(500))+''_''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_C''+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS TaskId
		,''L''+CAST(C.PlanId  AS NVARCHAR(500)) AS ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign C
			INNER JOIN FilteredPlan P ON P.EntityId = C.PlanId 
		WHERE C.IsDeleted = 0 AND (@isGrid=1 OR (C.StartDate>=@StartDate AND C.StartDate<=@EndDate) OR (C.EndDate>=@StartDate AND C.EndDate<=@EndDate))
	),
	Programs AS (
		SELECT ''Program'' EntityType,''P_C_P_'' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy 
		,CAST(P.PlanCampaignId AS NVARCHAR(500))+''_''+CAST(P.PlanProgramId AS NVARCHAR(50)) As AltId
		,CAST(C.TaskId AS NVARCHAR(500))+''_P''+CAST(P.PlanProgramId AS NVARCHAR(50)) As TaskId
		,CAST(C.ParentTaskId AS NVARCHAR(500))+''_C''+CAST(P.PlanCampaignId AS NVARCHAR(50)) As ParentTaskId
		,C.PlanId
		,C.ModelId
		FROM Plan_Campaign_Program P
			INNER JOIN Campaigns C ON C.EntityId = P.PlanCampaignId
		WHERE P.IsDeleted = 0 AND (@isGrid=1 OR (P.StartDate>=@StartDate AND P.StartDate<=@EndDate) OR (P.EndDate>=@StartDate AND P.EndDate<=@EndDate))
	),
	Tactics AS (
		SELECT ''Tactic'' EntityType,''P_C_P_T_'' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy 
		,CAST(T.PlanProgramId AS NVARCHAR(500))+''_''+CAST(T.PlanTacticId AS NVARCHAR(50)) As AltId
		,CAST(P.TaskId AS NVARCHAR(500))+''_T''+CAST(T.PlanTacticId AS NVARCHAR(50)) As TaskId
		,CAST(P.ParentTaskId AS NVARCHAR(500))+''_P''+CAST(T.PlanProgramId AS NVARCHAR(50)) As ParentTaskId
		,P.PlanId
		,P.ModelId
		FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Programs P ON P.EntityId = T.PlanProgramId
		WHERE T.IsDeleted = 0 AND (@isGrid=1 OR (T.StartDate>=@StartDate AND T.StartDate<=@EndDate) OR (T.EndDate>=@StartDate AND T.EndDate<=@EndDate))
	),
	LineItems AS (
		SELECT ''LineItem'' EntityType,''P_C_P_T_L_'' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId, NULL Status, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy 
		,CAST(L.PlanTacticId AS NVARCHAR(500))+''_''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As AltId
		,CAST(T.TaskId AS NVARCHAR(500))+''_X''+CAST(L.PlanLineItemId AS NVARCHAR(50)) As TaskId
		,CAST(T.ParentTaskId AS NVARCHAR(500))+''_T''+CAST(L.PlanTacticId AS NVARCHAR(50)) As ParentTaskId
		,T.PlanId
		,T.ModelId
		FROM Plan_Campaign_Program_Tactic_LineItem L
			INNER JOIN Tactics T ON T.EntityId = L.PlanTacticId
		WHERE L.IsDeleted = 0
	),
	AllEntities AS (    
		SELECT * FROM FilteredPlan UNION ALL
		SELECT * FROM Campaigns UNION ALL
		SELECT * FROM Programs UNION ALL
		SELECT * FROM Tactics UNION ALL
		SELECT * FROM LineItems
	)
	INSERT INTO @Entities (UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,Status,StartDate,EndDate,CreatedBy,AltId,TaskId,ParentTaskId,PlanId,ModelId)
	SELECT E.UniqueId, E.EntityId,E.EntityTitle, E.ParentEntityId,E.ParentUniqueId,E.EntityType, C.ColorCode,E.Status,E.StartDate,E.EndDate,E.CreatedBy,E.AltId,E.TaskId,E.ParentTaskId,E.PlanId,E.ModelId FROM AllEntities E
	LEFT JOIN EntityTypeColor C ON C.EntityType = E.EntityType

	
	
	-- Update ROIPackageIds column values
	Declare @entTactic varchar(20)=''Tactic''
	
	Declare @tblROI Table(
	AnchorTacticId int,
	PackageTacticids varchar(max)
	)
		
	BEGIN			
		INSERT INTO @tblROI	SELECT H.EntityId,RT.PlanTacticId
			FROM @Entities as H
			JOIN (SELECT AnchorTacticID,PlanTacticId=
			STUFF((SELECT '', '' + Cast(PlanTacticId as varchar)
			       FROM ROI_PackageDetail b 
			       WHERE b.AnchorTacticID = a.AnchorTacticID 
			      FOR XML PATH('''')), 1, 2, '''')
			FROM @Entities as T1
			JOIN ROI_PackageDetail as a on T1.EntityId = a.AnchorTacticID and T1.EntityType=@entTactic
			GROUP BY a.AnchorTacticID
			) as RT on H.EntityId = RT.AnchorTacticID and H.EntityType=@entTactic
		
	
	
		Update @Entities
		SET ROIPackageIds = R.PackageTacticids
		FROM @Entities as H
		JOIN @tblROI as R on H.EntityId = R.AnchorTacticId and H.EntityType=@entTactic
	END
	RETURN
END

' 
END

GO
