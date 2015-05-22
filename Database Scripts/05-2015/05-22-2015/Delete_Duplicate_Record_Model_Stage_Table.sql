WITH tblTemp as
(
SELECT ROW_NUMBER() Over(PARTITION BY ModelId,StageId,StageType ORDER BY StageId)
   As RowNumber,* FROM Model_Stage
)
DELETE From Model_Stage WHERE ModelStageId in 
(
SELECT ModelStageId FROM tblTemp
WHERE RowNumber > 1
)
