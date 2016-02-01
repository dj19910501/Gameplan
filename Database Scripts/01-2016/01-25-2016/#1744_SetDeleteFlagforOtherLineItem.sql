IF (EXISTS(SELECT * FROM [Plan_Campaign_Program_Tactic_Lineitem] WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0)) 
BEGIN 
	Update [Plan_Campaign_Program_Tactic_Lineitem] Set isDeleted = 1 WHERE LineItemTypeId is Null and Cost = 0 and isDeleted = 0
End
