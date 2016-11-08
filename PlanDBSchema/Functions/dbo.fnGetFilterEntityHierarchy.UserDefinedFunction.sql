
/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 11/08/2016 15:33:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetFilterEntityHierarchy]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	DROP FUNCTION [dbo].[fnGetFilterEntityHierarchy]
END
GO

/****** Object:  UserDefinedFunction [dbo].[fnGetFilterEntityHierarchy]    Script Date: 11/08/2016 15:33:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fnGetFilterEntityHierarchy]
(
	@planIds varchar(max)='',
	@ownerIds nvarchar(max)='',
	@tactictypeIds varchar(max)='',
	@statusIds varchar(max)='',
	@TimeFrame varchar(20)='',
	@isGrid bit=0
)

RETURNS @Entities TABLE (
			UniqueId		NVARCHAR(30), 
			EntityId		BIGINT,
			EntityTitle		NVARCHAR(1000),
			ParentEntityId	BIGINT, 
			ParentUniqueId	NVARCHAR(30),
			EntityType		NVARCHAR(15),
			EntityTypeID	INT, 
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
			--EloquaId		nvarchar(100),
			--MarketoId		nvarchar(100),
			--WorkfrontID		nvarchar(100),
			--SalesforceId	nvarchar(100),
			ROIPackageIds	Varchar(max)
		)
AS
BEGIN


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
	IF (@TimeFrame IS NOT NULL AND @TimeFrame <>'')
		BEGIN
			INSERT INTO @TimeFrameDatesAndYear
			SELECT Item as PlanYear from dbo.SplitString(@TimeFrame,'-')--split timeframe parameter e.g. 2015-2016
			
			--Set Minimum & Maximumyear
			SELECT @MinYear=CONVERT(VARCHAR,( MIN(CONVERT(INT,PlanYear)))),@MaxYear=CONVERT(VARCHAR,( MAX(CONVERT(INT,PlanYear)))) FROM @TimeFrameDatesAndYear 
	
			SET @StartDate= CONVERT(DATETIME,@MinYear+'-01-01 00:00:00') --Set first date of minimum year
			SET @EndDate= CONVERT(DATETIME,@MaxYear+'-12-31 00:00:00')   --Set Last date of maximum year

		END
	

	-- Insert Plan Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,EntityType,[Status],CreatedBy,AltId
										,TaskId,PlanId,ModelId)
		SELECT 'P_' + CAST(P.PlanId AS NVARCHAR(10)) UniqueId
				,P.PlanId EntityId
				, P.Title EntityTitle
				,'Plan' EntityType
				, P.Status
				, P.CreatedBy 
				,CAST(P.PlanId AS NVARCHAR(50)) AS AltId
				,'L'+CAST(P.PlanId AS NVARCHAR(50)) AS TaskId
				,P.PlanId
				,P.ModelId
		FROM [Plan] P WITH(NOLOCK)
		WHERE P.IsDeleted = 0 
				AND (
						@PlanIds IS NULL 
						OR P.PlanId IN (SELECT DISTINCT dimension FROM dbo.fnSplitString(@PlanIds,','))
					) 
	END

	-- Insert Campaign Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId)
		SELECT 'P_C_' + CAST(C.PlanCampaignId AS NVARCHAR(10)) UniqueId
				,C.PlanCampaignId EntityId
				, C.Title EntityTitle
				, P.EntityId ParentEntityId
				,P.UniqueId ParentUniqueId
				,'Campaign' EntityType
				, C.Status
				, C.StartDate StartDate
				, C.EndDate EndDate
				,C.CreatedBy 
			,CAST(C.PlanId AS NVARCHAR(500))+'_'+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS AltId
			,CAST(P.TaskId AS NVARCHAR(500))+'_C'+CAST(C.PlanCampaignId AS NVARCHAR(50)) AS TaskId
			,'L'+CAST(C.PlanId  AS NVARCHAR(500)) AS ParentTaskId
			,P.PlanId
			,P.ModelId
			
			FROM Plan_Campaign C WITH(NOLOCK)
			INNER JOIN @Entities P ON P.EntityId = C.PlanId and P.EntityType='Plan'
			WHERE C.IsDeleted = 0 AND (@isGrid=1 OR (C.StartDate>=@StartDate AND C.StartDate<=@EndDate) OR (C.EndDate>=@StartDate AND C.EndDate<=@EndDate))
	END

	-- Insert Program Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId)

		SELECT 'P_C_P_' + CAST(P.PlanProgramId AS NVARCHAR(10)) UniqueId
			,P.PlanProgramId EntityId, P.Title EntityTitle, C.EntityId ParentEntityId,C.UniqueId ParentUniqueId,'Program' EntityType, P.Status, P.StartDate StartDate, P.EndDate EndDate,P.CreatedBy 
			,CAST(P.PlanCampaignId AS NVARCHAR(500))+'_'+CAST(P.PlanProgramId AS NVARCHAR(50)) As AltId
			,CAST(C.TaskId AS NVARCHAR(500))+'_P'+CAST(P.PlanProgramId AS NVARCHAR(50)) As TaskId
			,CAST(C.ParentTaskId AS NVARCHAR(500))+'_C'+CAST(P.PlanCampaignId AS NVARCHAR(50)) As ParentTaskId
			,C.PlanId
			,C.ModelId
		
			FROM Plan_Campaign_Program P WITH(NOLOCK)
			INNER JOIN @Entities C ON C.EntityId = P.PlanCampaignId and C.EntityType='Campaign'
			WHERE P.IsDeleted = 0 AND (@isGrid=1 OR (P.StartDate>=@StartDate AND P.StartDate<=@EndDate) OR (P.EndDate>=@StartDate AND P.EndDate<=@EndDate))
	END

	-- Insert Tactic Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,[Status],StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId,ROIPackageIds)

		SELECT 'P_C_P_T_' + CAST(T.PlanTacticId AS NVARCHAR(10)) UniqueId,T.PlanTacticId EntityId, T.Title EntityTitle, P.EntityId ParentEntityId,P.UniqueId ParentUniqueId,'Tactic' EntityType, T.Status, T.StartDate StartDate, T.EndDate EndDate,T.CreatedBy 
			,CAST(T.PlanProgramId AS NVARCHAR(500))+'_'+CAST(T.PlanTacticId AS NVARCHAR(50)) As AltId
			,CAST(P.TaskId AS NVARCHAR(500))+'_T'+CAST(T.PlanTacticId AS NVARCHAR(50)) As TaskId
			,CAST(P.ParentTaskId AS NVARCHAR(500))+'_P'+CAST(T.PlanProgramId AS NVARCHAR(50)) As ParentTaskId
			,P.PlanId
			,P.ModelId
			
			,R.PackageIds as ROIPackageIds
			FROM Plan_Campaign_Program_Tactic T WITH(NOLOCK)
			INNER JOIN @Entities P ON P.EntityId = T.PlanProgramId and P.EntityType='Program'
			INNER JOIN [TacticType]  as typ WITH(NOLOCK) on T.TacticTypeId = typ.TacticTypeId and typ.IsDeleted='0' and 
			(@tactictypeIds = 'All' OR typ.[TacticTypeId] IN (select val from comma_split(@tactictypeIds,',')))

			LEFT JOIN (SELECT AnchorTacticID,PackageIds=
					STUFF((SELECT ', ' + Cast(PlanTacticId as varchar)
					       FROM ROI_PackageDetail b 
					       WHERE b.AnchorTacticID = a.AnchorTacticID 
					      FOR XML PATH('')), 1, 2, '')
					FROM @Entities as P1
					JOIN Plan_Campaign_Program_Tactic T2 WITH(NOLOCK) on P1.EntityId = T2.PlanProgramId and P1.EntityType='Program'
					JOIN ROI_PackageDetail as a WITH(NOLOCK)on T2.PlanTacticId = a.AnchorTacticID
					GROUP BY a.AnchorTacticID
					
				) as R on T.PlanTacticId = R.AnchorTacticId

			WHERE T.IsDeleted = 0 AND (@isGrid=1 OR (T.StartDate>=@StartDate AND T.StartDate<=@EndDate) OR (T.EndDate>=@StartDate AND T.EndDate<=@EndDate))
					AND T.[Status] IN (select val from comma_split(@statusIds,',')) and  
					(@ownerIds = 'All' OR T.[CreatedBy] IN (select case when val = '' then null else Convert(int,val) end from comma_split(@ownerIds,',')))
	END
	
	

	-- Insert LineItem Data
	BEGIN
		INSERT INTO @Entities(UniqueId,EntityId,EntityTitle,ParentEntityId,ParentUniqueId,EntityType,StartDate,EndDate,CreatedBy,AltId
										,TaskId,ParentTaskId,PlanId,ModelId)
		SELECT 'P_C_P_T_L_' + CAST(L.PlanLineItemId AS NVARCHAR(10)) UniqueId,L.PlanLineItemId EntityId, L.Title EntityTitle, T.EntityId ParentEntityId,T.UniqueId ParentUniqueId,'LineItem' EntityType, L.StartDate StartDate, L.EndDate EndDate,L.CreatedBy 
			,CAST(L.PlanTacticId AS NVARCHAR(500))+'_'+CAST(L.PlanLineItemId AS NVARCHAR(50)) As AltId
			,CAST(T.TaskId AS NVARCHAR(500))+'_X'+CAST(L.PlanLineItemId AS NVARCHAR(50)) As TaskId
			,CAST(T.ParentTaskId AS NVARCHAR(500))+'_T'+CAST(L.PlanTacticId AS NVARCHAR(50)) As ParentTaskId
			,T.PlanId
			,T.ModelId
			
			FROM Plan_Campaign_Program_Tactic_LineItem L WITH(NOLOCK)
			INNER JOIN @Entities T ON T.EntityId = L.PlanTacticId and T.EntityType='Tactic'
			WHERE L.IsDeleted = 0
	END

	-- Update ColorCode & EntityTypeId value
	BEGIN
		Update @Entities SET ColorCode = C.ColorCode, EntityTypeID = T.EntityTypeID
		FROM @Entities E
		LEFT JOIN EntityTypeColor C  WITH(NOLOCK) ON C.EntityType = E.EntityType
		LEFT JOIN EntityType T WITH(NOLOCK) ON T.Name = E.EntityType
	END
END


RETURN

END


GO


