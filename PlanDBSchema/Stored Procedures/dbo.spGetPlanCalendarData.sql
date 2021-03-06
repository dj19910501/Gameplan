/****** Object:  StoredProcedure [dbo].[spGetPlanCalendarData]    Script Date: 10/13/2016 4:58:12 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetPlanCalendarData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetPlanCalendarData]
GO
/****** Object:  StoredProcedure [dbo].[spGetPlanCalendarData]    Script Date: 10/13/2016 4:58:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetPlanCalendarData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spGetPlanCalendarData] AS' 
END
GO
ALTER PROCEDURE [dbo].[spGetPlanCalendarData]
	@planIds varchar(max),
	@ownerIds varchar(max),
	@tactictypeIds varchar(max),
	@statusIds varchar(max),
	@timeframe varchar(20)='',
	@planYear varchar(255)='',
	@viewBy	varchar(1000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Exec spGetPlanCalendarData '20220','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thismonth','','Tactic'
	--Exec spGetPlanCalendarData '20365','104','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','2016-2017','','Tactic'
	Declare @dblDash varchar(10)='--'

	Declare @tblResult Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear int		-- PlanYear
			)

	Declare @entTactic varchar(20)='Tactic'
	Declare @entProgram varchar(20)='Program'
	Declare @entCampaign varchar(20)='Campaign'
	Declare @entPlan varchar(20)='Plan'
	Declare @tacColorCode varchar(20)='317232'
	Declare @isGrid bit =0	-- To seprate out that current screen is Grid or not. Set 'false' for calendar. This variable passed to common View By function 'fnViewByEntityHierarchy' to identify the current screen is Calendar, Plan Grid or Budget.
	-- GetPlanGanttStartEndDate
	BEGIN
		Declare @calStartDate datetime
		Declare @calEndDate datetime

		SELECT TOP 1 @calStartDate=startdate,@calEndDate=enddate from [dbo].[fnGetPlanGanttStartEndDate](@timeframe)
	END
	
    -- Insert statements for procedure here

	
	Declare @Entities TABLE (
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
			TaskId			NVARCHAR(500),
			ParentTaskId	NVARCHAR(500),
			PlanId			BIGINT,
			ROIPackageIds	Varchar(max),
			PYear			INT				-- Plan Year
		)
	
	Declare @varThisYear varchar(10)='thisyear'
	Declare @varThisQuarter varchar(15)='thisquarter'
	Declare @varThisMonth varchar(10)='thismonth'
	Declare @varCurntTimeframe varchar(20)

	IF( (@timeframe = @varThisMonth) OR (@timeframe = @varThisQuarter) OR (@timeframe = @varThisYear) )
	BEGIN
		SET @varCurntTimeframe = DATEPART(yyyy,GETDATE())
	END
	ELSE
	BEGIN
		SET @varCurntTimeframe = @timeframe
	END
	
	INSERT INTO @Entities 
	SELECT		UniqueId, EntityId,EntityTitle, ParentEntityId,ParentUniqueId,EntityType, ColorCode,H.Status,StartDate,EndDate,H.CreatedBy,TaskId,ParentTaskId,H.PlanId,ROIPackageIds,P.[Year] 
	FROM		[dbo].fnViewByEntityHierarchy(@planIds,@ownerIds,@tactictypeIds,@statusIds,@viewBy,@varCurntTimeframe,@isGrid) as H
	LEFT JOIN	[Plan] as P on H.PlanId = P.PlanId
	
	-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			Declare @tblImprvPlan Table(
				PlanId int,
				MinEffectiveDate datetime
			)

			Insert Into @tblImprvPlan
			SELECT Distinct ent.PlanId, Min(T.EffectiveDate) as EffectiveDate
			FROM @Entities as ent
			Inner Join Plan_Improvement_Campaign as C on ent.PlanId = C.ImprovePlanId
			Inner Join Plan_Improvement_Campaign_Program as P on C.ImprovementPlanCampaignId = P.ImprovementPlanCampaignId
			Inner Join Plan_Improvement_Campaign_Program_Tactic as T on P.ImprovementPlanProgramId = T.ImprovementPlanProgramId
			GROUP BY ent.PlanId
		END
		
	-- Get All tactics
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @submitStatus varchar(20)='Submitted'
			Declare @declineStatus varchar(20)='Decline'
			Declare @tblTactics Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy int,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear int		-- PlanYear
			)
		END

		-- Insert Tactic Data to local table @tblTactics
		BEGIN
			INSERT INTO @tblTactics

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					tac.TacticCustomName as 'machineName',
					CASE 
						WHEN (tac.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),tac.StartDate,101) 
					END AS 'start_date',
					tac.EndDate as 'endDate',
					Null as 'duration',
					0 as 'progress',
					'0' as 'open',
					CASE
						WHEN (tac.[Status] = @submitStatus) THEN '1' ELSE '0'
					END as 'isSubmitted',
					CASE
						WHEN (tac.[Status] = @declineStatus) THEN '1' ELSE '0'
					END as 'isDeclined',
					'0' as 'projectedStageValue',
					'0' as 'mqls',
					tac.Cost as 'cost',
					0 as 'cws',
					ent.ParentTaskId as 'parent',
					NULL as 'color',
					ent.ColorCode as 'colorcode',
					tac.PlanTacticId as 'PlanTacticId',
					NULL as 'PlanProgramId',
					NULL as 'PlanCampaignId',
					tac.[Status] as 'Status',
					tac.TacticTypeId as 'TacticTypeId',
					TP.Title as 'TacticType',
					tac.CreatedBy as 'CreatedBy',
					CASE
						WHEN ( (DATEPART(YYYY,tac.EndDate) -  DATEPART(YYYY,tac.StartDate) ) > 0 ) 
						THEN '1' ELSE '0'
					END as 'LinkTacticPermission',
					tac.LinkedTacticId as 'LinkedTacticId',
					IsNull(P.Title,'') as 'LinkedPlanName',
					@entTactic as [type],
					TP.AssetType as 'ROITacticType',
					NULL as 'OwnerName',
					RP.AnchorTacticID as 'IsAnchorTacticId',
					ent.ROIPackageIds as  'CalendarHoneycombpackageIDs',
					Null as 'Permission',
					ent.PlanId,
					ent.PYear				-- PlanYear
			FROM @Entities as ent
			INNER JOIN Plan_Campaign_Program_Tactic as tac on ent.EntityId = tac.PlanTacticId and ent.EntityType='Tactic'
			LEFT JOIN TacticType as TP on tac.TacticTypeId = TP.TacticTypeId and TP.IsDeleted='0'
			LEFT JOIN ROI_PackageDetail as RP on tac.PlanTacticId = RP.PlanTacticId 
			LEFT JOIN  [Plan] as P on tac.LinkedPlanId = P.PlanId 
			WHERE EntityType=@entTactic
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblTactics SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END

		-- Update Progress
		BEGIN
			Update @tblTactics SET progress= CASE
												 WHEN (CAST(T1.[start_date] as datetime) >= I.MinEffectiveDate) 
												 THEN 1 ELSE 0
											 END 
			FROM @tblTactics as T1
			JOIN @tblImprvPlan as I on T1.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblTactics SET color =	CASE
												 WHEN (progress = 1) 
												 THEN 'stripe' ELSE ''
											END
											--WHERE 1=1 
		END
		
	END
	
	-- Get All Programs
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblPrograms Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Tactic Data to local table @tblPrograms
		BEGIN
			INSERT INTO @tblPrograms(id,[text],machineName,[start_date],endDate,progress,[open],parent,colorcode,PlanProgramId,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					'' as 'machineName',
					CASE 
						WHEN (ent.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),ent.StartDate,101) 
					END AS 'start_date',
					ent.EndDate as 'endDate',
					--Null as 'duration',
					0 as 'progress',
					'0' as [open],
					ent.ParentTaskId as 'parent',
					--NULL as 'color',
					ent.ColorCode as 'colorcode',
					ent.EntityId as 'PlanProgramId',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entProgram as [type],
					ent.PlanId,
					ent.PYear				-- PlanYear
			FROM @Entities as ent
			WHERE EntityType=@entProgram 
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblPrograms SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END
		
		-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			Declare @tblEntImprvPlan Table(
				EntityId Int,
				MinTacticDate datetime,
				PlanId Int
			)

			Insert Into @tblEntImprvPlan
			SELECT prg.PlanProgramId as EntityId,Min(tac.StartDate), prg.PlanId
			FROM @tblPrograms as prg
			Inner Join @Entities as tac on prg.PlanProgramId = tac.ParentEntityId and tac.EntityType='Tactic'
			GROUP BY prg.PlanId,prg.PlanProgramId
		END

		-- Calculate Progress for each Program
		BEGIN
			Update @tblPrograms SET progress = 
												CASE 
													WHEN (IP.MinTacticDate >= I.MinEffectiveDate)
													THEN 
														CASE
															WHEN (DATEDIFF(DAY,@calStartDate,IP.MinTacticDate)) > 0
															THEN (	
																	DATEDIFF(DAY,@calStartDate,IP.MinTacticDate) / P.duration
																 )
															ELSE 1
														END
													ELSE 0
												END 
			FROM @tblPrograms as P
			JOIN @tblEntImprvPlan as IP on P.PlanProgramId = IP.EntityId
			JOIN @tblImprvPlan as I on IP.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblPrograms SET color =	CASE
												 WHEN (progress = 1) 
												 THEN ' stripe stripe-no-border ' 
												 ELSE 
													 CASE
														WHEN (progress > 0)
														THEN 'partialStripe' ELSE ''
													 END
											END
											--WHERE 1=1 
		END
		
		--select * from @tblPrograms
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	-- Get All Campaigns
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblCampaigns Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Campaign Data to local table @tblCampaigns
		BEGIN
			INSERT INTO @tblCampaigns(id,[text],[start_date],endDate,progress,[open],parent,colorcode,PlanCampaignId,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					CASE 
						WHEN (ent.StartDate < @calStartDate) 
						THEN CONVERT(VARCHAR(10),@calStartDate,101) 
						ELSE CONVERT(VARCHAR(10),ent.StartDate,101) 
					END AS 'start_date',
					ent.EndDate as 'endDate',
					--Null as 'duration',
					0 as 'progress',
					'1' as [open],
					ent.ParentTaskId as 'parent',
					ent.ColorCode as 'colorcode',
					ent.EntityId as 'PlanCampaignId',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entCampaign as [type],
					ent.PlanId,
					ent.PYear						-- Plan Year
			FROM @Entities as ent
			WHERE EntityType=@entCampaign 
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
			order by [text]
		END

		-- Update duration field
		BEGIN
			Update @tblCampaigns SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END
		
		-- Get Plan wise MinStartEffective date from Improvement Tactic
		BEGIN
			DELETE FROM @tblEntImprvPlan

			Insert Into @tblEntImprvPlan
			SELECT camp.PlanCampaignId as EntityId,Min(tac.StartDate), camp.PlanId
			FROM @tblCampaigns as camp
			Inner Join @Entities as prg on camp.PlanCampaignId = prg.ParentEntityId and prg.EntityType='Program'
			Inner Join @Entities as tac on prg.EntityId = tac.ParentEntityId and tac.EntityType='Tactic'
			GROUP BY camp.PlanId,camp.PlanCampaignId
		END

		-- Calculate Progress for each Campaign
		BEGIN
			Update @tblCampaigns SET progress = 
												CASE 
													WHEN (IC.MinTacticDate >= I.MinEffectiveDate)
													THEN 
														CASE
															WHEN (DATEDIFF(DAY,@calStartDate,IC.MinTacticDate)) > 0
															THEN (	
																	DATEDIFF(DAY,@calStartDate,IC.MinTacticDate) / C.duration
																 )
															ELSE 1
														END
													ELSE 0
												END 
			FROM @tblCampaigns as C
			JOIN @tblEntImprvPlan as IC on C.PlanCampaignId = IC.EntityId
			JOIN @tblImprvPlan as I on IC.PlanId = I.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblCampaigns SET color = CASE
											  WHEN (progress = 1) 
											  THEN ' stripe' 
											  ELSE 
											 	 CASE
											 		WHEN (progress > 0)
											 		THEN 'stripe' ELSE ''
											 	 END
											 END
											 --WHERE 1=1 
		END
		
		--select * from @tblCampaigns
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	-- Get All Plans
	BEGIN

		-- Declare Local Varialbles
		BEGIN
			Declare @tblPlans Table(
				id varchar(255),
				[text] nvarchar(1000),
				machineName nvarchar(1000),
				[start_date] varchar(100),
				endDate datetime,
				duration float,
				progress float,
				[open] bit,
				isSubmitted bit,
				isDeclined bit,
				projectedStageValue float,
				mqls float,
				cost float,
				cws float,
				parent varchar(255),
				color varchar(255),
				colorcode varchar(50),
				PlanTacticId int,
				PlanProgramId int,
				PlanCampaignId int,
				[Status] varchar(20),
				TacticTypeId int,
				TacticType varchar(500),
				CreatedBy INT,
				LinkTacticPermission bit,
				LinkedTacticId int,
				LinkedPlanName nvarchar(1000),
				[type] varchar(255),
				ROITacticType varchar(500),
				OwnerName nvarchar(1000),
				IsAnchorTacticId int,
				CalendarHoneycombpackageIDs varchar(max),
				Permission bit,
				PlanId bigint,
				PYear  int		-- PlanYear
			)
		END

		-- Insert Plan Data to local table @tblPlans
		BEGIN
			INSERT INTO @tblPlans(id,[text],progress,[open],parent,colorcode,[Status],TacticType,CreatedBy,[type],PlanId,PYear)

			SELECT 
					ent.TaskId as 'id',
					ent.EntityTitle as 'text',
					0 as 'progress',
					'1' as [open],
					ent.ParentTaskId as 'parent',
					ent.ColorCode as 'colorcode',
					ent.[Status] as 'Status',
					@dblDash as 'TacticType',
					ent.CreatedBy as 'CreatedBy',
					@entPlan as [type],
					ent.PlanId,
					ent.PYear
			FROM @Entities as ent
			WHERE ent.EntityType=@entPlan 
			order by [text]
			--AND (ent.EndDate >= @calStartDate AND ent.EndDate <= @calEndDate) AND (ent.StartDate >= @calStartDate AND ent.StartDate <= @calEndDate)
		END

		BEGIN
			-- Update start_date column for Plan
			UPDATE @tblPlans SET [start_date]= ISNull( D.[start_date],DATEFROMPARTS (DATEPART(yyyy,@calStartDate), 1, 1))
			FROM @tblPlans as TP
			JOIN 
				(
					SELECT P.PlanId,
													CASE 
														WHEN (MIN(C.StartDate) < @calStartDate) 
														THEN CONVERT(VARCHAR(10),@calStartDate,101) 
														ELSE CONVERT(VARCHAR(10),MIN(C.StartDate),101) 
													END as 'start_date'
					FROM @tblPlans as P
					LEFT Join @Entities as C on P.PlanId = C.PlanId and C.EntityType='Campaign'
					GROUP BY P.PlanId
				) as D on TP.PlanId = D.PlanId
		END

		BEGIN
			-- Update enddate column for Plan
			UPDATE @tblPlans SET endDate= ISNull( D.endDate,DATEFROMPARTS (DATEPART(yyyy,@calEndDate), 12, 31))
			FROM @tblPlans as TP
			JOIN 
				(
					SELECT P.PlanId, MAX(C.EndDate) as 'endDate'
					FROM @tblPlans as P
					LEFT Join @Entities as C on P.PlanId = C.PlanId and C.EntityType='Campaign'
					GROUP BY P.PlanId
				) as D on TP.PlanId = D.PlanId
		END

		-- Update duration field
		BEGIN
			Update @tblPlans SET duration= CASE
												 WHEN (endDate > @calEndDate) 
												 THEN DATEDIFF(DAY,CAST([start_date] as datetime),@calEndDate)
												 ELSE DATEDIFF(DAY,CAST([start_date] as datetime),endDate)
											 END 
											 --WHERE 1=1
		END

		-- Calculate Progress for each Plan
		BEGIN
			Update @tblPlans SET progress = 
											CASE 
												WHEN ( Cast(T.[start_date] as datetime) >= I.MinEffectiveDate)
												THEN 
													CASE
														WHEN ( DATEDIFF(DAY, Cast( P.[start_date] as datetime) ,Cast(T.[start_date] as datetime) ) ) > 0
														THEN (	
																DATEDIFF(DAY,Cast( P.[start_date] as datetime),Cast(T.[start_date] as datetime)) / P.duration
															 )
														ELSE 1
													END
												ELSE 0
											END 
			FROM @tblPlans as P
			JOIN @tblImprvPlan as I on P.PlanId = I.PlanId
			JOIN @tblTactics as T on P.PlanId = T.PlanId
		END

		-- Update Color
		BEGIN
			Update @tblPlans SET color = CASE
											  WHEN (progress >0) 
											  THEN ' stripe' ELSE ''
										END
										--WHERE 1=1 
		END
		
		--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
	END

	--INSERT INTO @tblResult

	-- Check that ViewBy is not 'Tactic' type then set 1st most parent record fields values as Plan entity
	IF(@viewBy <> @entTactic)
	BEGIN
		INSERT INTO @tblResult(id,[text],[start_date],duration,progress,color,colorcode)
		SELECT	IsNull(E.TaskId,'') as id 
				,IsNull(E.EntityTitle,'') as [text],
				P.[start_date],
				duration = P.duration,
				progress = P.progress,
				color=p.color,
				colorcode=@tacColorCode		-- if ViewBy is not 'Tactic' then set parent node color code same as 'Tactic' entity color code.
		FROM	@Entities as E
		JOIN	@tblPlans as P on E.TaskId = P.parent
		WHERE	E.EntityType = @viewBy
	END

	-- Prepared Final Result Set.
	BEGIN

		INSERT INTO @tblResult
					(id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear)
		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear
		
		FROM		@tblPlans
		)
		 
		UNION ALL

		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear
		 
		FROM		@tblCampaigns
		)
		 
		UNION ALL
		
		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

		FROM		@tblPrograms 
		)

		UNION ALL

		(
		SELECT		id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
					,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
					,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

		FROM		@tblTactics 
		)

	END

	SELECT	id,[text],machineName,[start_date],endDate,duration,progress,[open],isSubmitted,isDeclined,projectedStageValue,mqls,cost,cws,parent,color,colorcode
			,PlanTacticId,PlanProgramId,PlanCampaignId,[Status],TacticTypeId,TacticType,CreatedBy,LinkTacticPermission,LinkedTacticId,LinkedPlanName,[type]
			,ROITacticType,OwnerName,IsAnchorTacticId,CalendarHoneycombpackageIDs,Permission,PlanId,PYear

	FROM	@tblResult

	--Exec spGetPlanCalendarData '20220','41F64F4B-531E-4CAA-8F5F-328E36D9B202','31104,31121','Created,Complete,Approved,Declined,Submitted,In-Progress','thisyear','','',''
END




GO
