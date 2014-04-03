/*set AllowedTargetStage true which Model stage are used in Tactic_Type table */
Update Model_Funnel_Stage set AllowedTargetStage=1 
 where ModelFunnelId in
 (
	select ModelFunnelId from Model_Funnel where ModelId In 
	(
		select ModelId from TacticType where StageId Is Not NuLL
	)
 ) and StageType='CR'