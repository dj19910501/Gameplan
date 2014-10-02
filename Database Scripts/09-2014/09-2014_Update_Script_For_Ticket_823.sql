
Go

UPDATE dbo.Model_Funnel_Stage SET AllowedTargetStage = 1
FROM dbo.Model m
INNER JOIN dbo.BusinessUnit b on m.BusinessUnitId = b.BusinessUnitId
WHERE m.IsDeleted = 0 AND StageId IN (SELECT StageId FROM dbo.Stage WHERE [Level]=1 AND ClientId = b.ClientId)
AND
ModelId NOT IN
(SELECT DISTINCT ModelId FROM dbo.Model_Funnel_Stage
INNER JOIN dbo.Model_Funnel ON dbo.Model_Funnel.ModelFunnelId = dbo.Model_Funnel_Stage.ModelFunnelId
WHERE AllowedTargetStage = 1 )

Go

UPDATE dbo.TacticType SET StageId = mfs.StageId
FROM dbo.TacticType tt
INNER JOIN dbo.Model_Funnel mf ON mf.ModelId = tt.ModelId
INNER JOIN dbo.Model_Funnel_Stage mfs ON mfs.ModelFunnelId = mf.ModelFunnelId
WHERE tt.IsDeleted = 0 AND mfs.AllowedTargetStage = 1 AND tt.StageId IS NULL 

Go

---Script for updating inconsistant model data
DECLARE @modelId INT
DECLARE @Parent_modelId INT
DECLARE @modelCounter INT
DECLARE @tacticTypeId INT
DECLARE @parentTacticTypeId INT

---modelcounter is variable for count number of models which have inconsistant data
SET @modelCounter= (SELECT COUNT(*) FROM(SELECT DISTINCT ModelId
FROM            (SELECT        dbo.[Plan].PlanId, dbo.[Plan].ModelId, dbo.TacticType.TacticTypeId, dbo.TacticType.ModelId AS TacticTypeModelId
                          FROM            dbo.Plan_Campaign_Program_Tactic INNER JOIN
                                                    dbo.Plan_Campaign_Program ON dbo.Plan_Campaign_Program_Tactic.PlanProgramId = dbo.Plan_Campaign_Program.PlanProgramId INNER JOIN
                                                    dbo.Plan_Campaign ON dbo.Plan_Campaign_Program.PlanCampaignId = dbo.Plan_Campaign.PlanCampaignId INNER JOIN
                                                    dbo.[Plan] ON dbo.Plan_Campaign.PlanId = dbo.[Plan].PlanId INNER JOIN
                                                    dbo.TacticType ON dbo.Plan_Campaign_Program_Tactic.TacticTypeId = dbo.TacticType.TacticTypeId AND dbo.[Plan].ModelId <> dbo.TacticType.ModelId
                          WHERE        (dbo.[Plan].IsDeleted = 0)) AS A) As InconsistantModel)
WHILE(@modelCounter>0)
BEGIN
				
               
				
   SET @modelId= (SELECT TOP(1) ModelId FROM (SELECT TOP (@modelCounter)* FROM  (SELECT        dbo.[Plan].PlanId, dbo.[Plan].ModelId, dbo.TacticType.TacticTypeId, dbo.TacticType.ModelId AS TacticTypeModelId
         FROM            dbo.Plan_Campaign_Program_Tactic INNER JOIN
         dbo.Plan_Campaign_Program ON dbo.Plan_Campaign_Program_Tactic.PlanProgramId = dbo.Plan_Campaign_Program.PlanProgramId INNER JOIN
         dbo.Plan_Campaign ON dbo.Plan_Campaign_Program.PlanCampaignId = dbo.Plan_Campaign.PlanCampaignId INNER JOIN
         dbo.[Plan] ON dbo.Plan_Campaign.PlanId = dbo.[Plan].PlanId INNER JOIN
         dbo.TacticType ON dbo.Plan_Campaign_Program_Tactic.TacticTypeId = dbo.TacticType.TacticTypeId AND dbo.[Plan].ModelId <> dbo.TacticType.ModelId
         WHERE        (dbo.[Plan].IsDeleted = 0))AS joinedTable)AS A)

  

			    WHILE(@modelId IS NOT NULL)
                BEGIN
				
		
				SET @Parent_modelId= (SELECT ParentModelId FROM dbo.Model WHERE ModelId=@modelId)
				---Cursor for inconsistant data in tactictype table
                DECLARE diff_Model_Data CURSOR LOCAL FOR
                SELECT A.TacticTypeId,prevTactictypeID from (SELECT TacticTypeId,Title,PreviousTacticTypeId FROM dbo.TacticType WHERE ModelId=@modelId) A INNER JOIN
                (SELECT TacticTypeId AS prevTactictypeID,Title FROM dbo.TacticType WHERE ModelId=@Parent_modelId)B ON A.Title=B.Title AND (A.PreviousTacticTypeId<>B.prevTactictypeID OR A.PreviousTacticTypeId IS NULL)

                OPEN diff_Model_Data
                FETCH NEXT FROM diff_Model_Data INTO @tacticTypeId,@parentTacticTypeId
                             WHILE @@FETCH_STATUS =0
                             BEGIN

                             UPDATE dbo.TacticType SET PreviousTacticTypeId=@parentTacticTypeId WHERE TacticTypeId=@tacticTypeId
                             
                             FETCH NEXT FROM diff_Model_Data INTO @tacticTypeId,@parentTacticTypeId
                             END
							 CLOSE diff_Model_Data
                             DEALLOCATE diff_Model_Data

							 SET @modelId=@Parent_modelId
               END

 SET @modelCounter=@modelCounter-1

END


---End Script for updating inconsistant model data


Go


-----Script for updating inconsistant data of plan's tactic

DECLARE @PlanTacticID INT
DECLARE @PlanTacticTypeId INT
DECLARE @PlanModelId INT
DECLARE @NewTacticTypeId INT
DECLARE @NewStageId INT

--Cursor which have inconsistant data 
DECLARE diff_Data CURSOR LOCAL FOR
SELECT        dbo.[Plan].ModelId, dbo.TacticType.TacticTypeId,dbo.Plan_Campaign_Program_Tactic.PlanTacticId
         FROM            dbo.Plan_Campaign_Program_Tactic INNER JOIN
         dbo.Plan_Campaign_Program ON dbo.Plan_Campaign_Program_Tactic.PlanProgramId = dbo.Plan_Campaign_Program.PlanProgramId INNER JOIN
         dbo.Plan_Campaign ON dbo.Plan_Campaign_Program.PlanCampaignId = dbo.Plan_Campaign.PlanCampaignId INNER JOIN
         dbo.[Plan] ON dbo.Plan_Campaign.PlanId = dbo.[Plan].PlanId INNER JOIN
         dbo.TacticType ON dbo.Plan_Campaign_Program_Tactic.TacticTypeId = dbo.TacticType.TacticTypeId AND dbo.[Plan].ModelId <> dbo.TacticType.ModelId
         WHERE        (dbo.[Plan].IsDeleted = 0) AND (dbo.Plan_Campaign_Program_Tactic.IsDeleted = 0)
		 ORDER BY dbo.Plan_Campaign_Program_Tactic.PlanTacticId

		 OPEN diff_Data
		 ----set inconsistant data's values
		 FETCH NEXT FROM diff_Data INTO @PlanModelId,@PlanTacticTypeId,@PlanTacticID
		 WHILE @@FETCH_STATUS=0
		 BEGIN

		 SET @NewTacticTypeId= (SELECT A.TacticTypeId FROM ((SELECT TacticTypeId,Title FROM dbo.TacticType WHERE ModelId=@PlanModelId) A INNER JOIN 
		  (SELECT Title FROM dbo.TacticType WHERE TacticTypeId=@PlanTacticTypeId) B 
		  ON A.Title=B.Title))
		  UPDATE dbo.Plan_Campaign_Program_Tactic SET TacticTypeId=@NewTacticTypeId WHERE PlanTacticId=@PlanTacticID

		  SET @NewStageId= (SELECT B.StageId  FROM ((SELECT TacticTypeId,Title FROM dbo.TacticType WHERE ModelId=@PlanModelId) A INNER JOIN 
		  (SELECT Title,StageId FROM dbo.TacticType WHERE TacticTypeId=@PlanTacticTypeId) B 
		  ON A.Title=B.Title))
		  UPDATE dbo.Plan_Campaign_Program_Tactic SET StageId=@NewStageId WHERE PlanTacticId=@PlanTacticID

		  --PRINT(CONVERT(VARCHAR(50), @NewTacticTypeId)+' '+CONVERT(VARCHAR(50),@PlanTacticID) )
		  FETCH NEXT FROM diff_Data INTO @PlanModelId,@PlanTacticTypeId,@PlanTacticID
		  
		 END
		 
		  CLOSE diff_Data
          DEALLOCATE diff_Data


 -----End Script for updating inconsistant data of plan's tactic

 
 
 
 GO