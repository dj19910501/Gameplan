IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Update_MQL')
	DROP PROCEDURE Update_MQL
GO
/****** Object:  StoredProcedure [dbo].[Update_MQL]    Script Date: 2/28/2014 3:56:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Bhavesh Dobariya>
-- Create date: <28 Feb 2014>
-- Description:	<Update MQL of Tactic Based on Conversion Rate Define in Model.>
-- =============================================
-- EXEC dbo.Update_MQL 342,'464EB808-AD1F-4481-9365-6AADA15023BD','INQ','MQL','CR',0
Create PROCEDURE [dbo].[Update_MQL]
	-- Add the parameters for the stored procedure here
	@ModelId int,
	@ClientId varchar(255),
	@INQ Varchar(10),
	@MQL Varchar(10),
	@StageTypeCR Varchar(10),
	@ReturnValue INT = 0 OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @INQLevel int
	Declare @MQLLevel int
	SET @INQLevel = (SELECT [Level] FROM Stage WHERE ClientId = @ClientId AND Code = @INQ)
	SET @MQLLevel = (SELECT [Level] FROM Stage WHERE ClientId = @ClientId AND Code = @MQL)
	BEGIN TRANSACTION UpdateMQL 

	;WITH _CTE AS(
		SELECT 
				T.StartDate, 
				T.PlanTacticId,
				ISNULL(T.INQs, 0) AS INQ,
				[Plan].ModelId,
				M.ParentModelId,
				M.EffectiveDate
		FROM Plan_Campaign_Program_Tactic T
			INNER JOIN Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId 
			INNER JOIN Plan_Campaign C ON C.PlanCampaignId = P.PlanCampaignId
			INNER JOIN [Plan] ON [Plan].PlanId = C.PlanId
			INNER JOIN Model M ON M.ModelId = [Plan].ModelId
		WHERE M.ModelId = @ModelId

		UNION ALL
		SELECT 
				C.StartDate, 
				C.PlanTacticId,
				C.INQ,
				M.ModelId,
				M.ParentModelId,
				M.EffectiveDate
		FROM Model M
			INNER JOIN _CTE C ON C.ParentModelId = M.ModelId
		),
		_CTEDetail AS(
			SELECT 
				ROWNUM = ROW_NUMBER() OVER(PARTITION BY C.PlanTacticId ORDER BY C.ModelId DESC),
				C.StartDate, 
				C.PlanTacticId,
				C.INQ,
				C.ModelId,
				C.ParentModelId,
				C.EffectiveDate
			FROM _CTE C
			WHERE C.StartDate >= C.EffectiveDate
		),
		_CTEFinal AS (
		SELECT 
			C.PlanTacticId,
			MFS.Value /100 AS RATE
		FROM _CTEDetail C
			INNER JOIN Model_Funnel MF ON MF.ModelId = C.ModelId
			INNER JOIN Model_Funnel_Stage MFS ON MFS.ModelFunnelId = MF.ModelFunnelId
			INNER JOIN [Stage] on MFS.StageId = [Stage].StageId
		WHERE C.ROWNUM = 1 AND [Stage].[Level] >= @INQLevel AND [Stage].[Level] < @MQLLevel AND [Stage].ClientId = @ClientID AND MFS.StageType = @stageTypeCR
		)
		,
		_CTEUse AS
		(
		SELECT DISTINCT T1.PlanTacticId, CASE WHEN (SELECT COUNT(*) FROM _CTEFinal T2 WHERE T2.RATE = 0 AND T2.PlanTacticId = T1.PlanTacticId) >= 1 THEN 0 ELSE (SELECT EXP(SUM(LOG(ABS(T2.RATE)))) FROM _CTEFinal T2 WHERE T2.PlanTacticId = T1.PlanTacticId GROUP BY T2.PlanTacticId) END  AS Rate FROM _CTEFinal T1  
		)
		Update PT SET PT.MQLs = ROUND(PT.INQs * CF.RATE ,0) FROM Plan_Campaign_Program_Tactic PT
		INNER JOIN _CTEUse CF ON CF.PlanTacticId = PT.PlanTacticId

		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END      

		Update P SET P.MQLs = (SELECT SUM(MQLs) FROM Plan_Campaign_Program_Tactic WHERE PlanProgramId = P.PlanProgramId AND IsDeleted = 0) FROM Plan_Campaign_Program P
		INNER JOIN Plan_Campaign ON P.PlanCampaignId = Plan_Campaign.PlanCampaignId
		INNER JOIN [Plan] ON [Plan].PlanId = Plan_Campaign.PlanId
		WHERE ModelId = @ModelId
		
		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END   

		Update C SET C.MQLs = (SELECT SUM(MQLs) FROM Plan_Campaign_Program WHERE PlanCampaignId = C.PlanCampaignId AND IsDeleted = 0) FROM Plan_Campaign C
		INNER JOIN [Plan] ON [Plan].PlanId = C.PlanId
		WHERE ModelId = @ModelId

		IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION UpdateMQL                    
					RETURN                
				END
		COMMIT TRANSACTION PlanDuplicate                  
		SET @ReturnValue = 1;        
		RETURN @ReturnValue

END

GO
/*
Update MQL Based on conversion rate
DATE: 2nd April 2014
*/
DECLARE @PlanTacticId int,
@PlanId int,
@ModelId int,
@StartDate datetime

DECLARE @INQ VARCHAR(10) = 'INQ'
DECLARE @MQL VARCHAR(10) = 'MQL'
DECLARE @StageType varchar(10) = 'CR'


	DECLARE innnercursor CURSOR
        FOR
            SELECT  PlanTacticId, Plan_Campaign_Program_Tactic.StartDate, [Plan].PlanId, [Plan].ModelId
            FROM    Plan_Campaign_Program_Tactic
			INNER JOIN Plan_Campaign_Program ON Plan_Campaign_Program.PlanProgramId = Plan_Campaign_Program_Tactic.PlanProgramId
			INNER JOIN Plan_Campaign ON Plan_Campaign.PlanCampaignId = Plan_Campaign_Program.PlanCampaignId
			INNER JOIN [Plan] ON [Plan].PlanId = Plan_Campaign.PlanId
        OPEN innnercursor
					
        FETCH NEXT FROM innnercursor INTO @PlanTacticId, @StartDate, @PlanId, @ModelId
                   
        WHILE ( @@FETCH_STATUS = 0 ) 
            BEGIN
				DECLARE @ParentModelId int
				DECLARE @EffectiveDate datetime
				SET @ParentModelId = @ModelId
				DECLARE @FinalModelId int
				
					WHILE @ParentModelId IS NOT NULL
						BEGIN
						SET @FinalModelId = @ParentModelId
							SET @EffectiveDate = (SELECT EffectiveDate FROM Model WHERE ModelId = @ParentModelId)
							IF(@StartDate >= @EffectiveDate OR @EffectiveDate IS NULL)
								BEGIN
									break;
								END
							ELSE
								BEGIN
									SET @ParentModelId = (SELECT ParentModelId FROM Model WHERE ModelId = @ParentModelId)
								END
							
						END
					Declare @INQLevel int
	Declare @MQLLevel int
	DECLARE @FinalClientId varchar(255)
	SET @FinalClientId = (SELECT ClientId FROM Model
							INNER JOIN BusinessUnit ON Model.BusinessUnitId = BusinessUnit.BusinessUnitId
							 WHERE ModelId = @FinalModelId
							)
	SET @INQLevel = (SELECT [Level] FROM Stage WHERE ClientId = @FinalClientId AND Code = @INQ)
	SET @MQLLevel = (SELECT [Level] FROM Stage WHERE ClientId = @FinalClientId AND Code = @MQL)
	
	;with CTETable AS (SELECT 
			MFS.Value /100 AS RATE
		FROM Model_Funnel_Stage MFS
			INNER JOIN Model_Funnel MF ON MFS.ModelFunnelId = MF.ModelFunnelId
			INNER JOIN [Stage] on MFS.StageId = [Stage].StageId
		WHERE  MF.ModelId = @FinalModelId AND [Stage].[Level] >= @INQLevel AND [Stage].[Level] < @MQLLevel AND [Stage].ClientId = @FinalClientId AND MFS.StageType = @StageType
		),
		_CTEUSE AS
		(SELECT CASE WHEN (SELECT COUNT(*) FROM CTETable T2 WHERE T2.RATE = 0) >= 1 THEN 0 ELSE (SELECT EXP(SUM(LOG(ABS(T2.RATE)))) FROM CTETable T2 ) END  AS Rate 
		)
		--SELECT PlanTacticId  ,MQLs,INQs * (SELECT ISNULL(Rate,0) FROM _CTEUSE) AS CALMQL FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @PlanTacticId
		Update Plan_Campaign_Program_Tactic SET MQLs = ROUND(INQs * (SELECT ISNULL(Rate,0) FROM _CTEUSE) ,0) WHERE PlanTacticId = @PlanTacticId
		
			 FETCH NEXT FROM innnercursor INTO @PlanTacticId, @StartDate, @PlanId, @ModelId
            END
        CLOSE innnercursor
        DEALLOCATE innnercursor

GO
WITH Tactics AS (
SELECT DISTINCT
              T.PlanProgramId PlanProgramId1,
              INQSum = (SELECT SUM(T1.INQs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(T1.MQLs) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0),
              CostSum = (SELECT SUM(T1.Cost) FROM Plan_Campaign_Program_Tactic T1 WHERE T1.PlanProgramId = T.PlanProgramId AND IsDeleted = 0)
FROM Plan_Campaign_Program_Tactic T
WHERE T.IsDeleted = 0
),
AllData AS(
SELECT 
              T.PlanProgramId1, 
              P.PlanProgramId, 
              INQSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanProgramId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              P.INQs, 
              P.MQLs, 
              P.Cost 
FROM Tactics T 
RIGHT JOIN Plan_Campaign_Program P ON P.PlanProgramId = T.PlanProgramId1 AND P.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign_Program p  RIGHT JOIN AllData T ON P.PlanProgramId = T.PlanProgramId AND P.IsDeleted = 0;



WITH Programs AS (
SELECT DISTINCT
              P.PlanCampaignId PlanCampaignId1,
              INQSum = (SELECT SUM(P1.INQs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              MQLSum = (SELECT SUM(P1.MQLs) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0),
              CostSum = (SELECT SUM(P1.Cost) FROM Plan_Campaign_Program P1 WHERE P1.PlanCampaignId = P.PlanCampaignId AND IsDeleted = 0)
FROM Plan_Campaign_Program P
WHERE P.IsDeleted = 0
)
,
AllData AS(
SELECT 
              T.PlanCampaignId1, 
              C.PlanCampaignId, 
              INQSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.INQSum ELSE 0 END, 
              MQLSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.MQLSum ELSE 0 END, 
              CostSum = CASE WHEN T.PlanCampaignId1 IS NOT NULL THEN T.CostSum ELSE 0 END, 
              C.INQs, 
              C.MQLs, 
              C.Cost 
FROM Programs T 
RIGHT JOIN Plan_Campaign C ON C.PlanCampaignId = T.PlanCampaignId1 AND C.IsDeleted = 0
)
UPDATE p  SET p.INQs = T.INQSum, p.MQLs = T.MQLSum, p.Cost = T.CostSum FROM Plan_Campaign p  RIGHT JOIN AllData T ON P.PlanCampaignId = T.PlanCampaignId AND P.IsDeleted = 0;
GO