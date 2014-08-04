/****** Object:  StoredProcedure [dbo].[Plan_Task_Delete]    Script Date: 8/4/2014 16:38:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--Stored Procedure
	ALTER PROCEDURE [dbo].[Plan_Task_Delete]
	(
		   @PlanCampaignId INT = NULL,
		   @PlanProgramId INT = NULL,
		   @PlanTacticId INT = NULL,
		   @IsDelete BIT,
		   @ModifiedDate DateTime,    
		   @ModifiedBy UNIQUEIDENTIFIER, 
		@ReturnValue INT = 0 OUT,
		@PlanLineItemId INT =NULL     
	)
	AS
	BEGIN

		   DECLARE @TranName VARCHAR(20);
		   SELECT @TranName = 'Plan_Task_Delete';
		   BEGIN TRANSACTION @TranName;
		   BEGIN TRY
						 IF(@PlanCampaignId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId IN (SELECT PlanTacticId FROM dbo.Plan_Campaign_Program_Tactic WHERE PlanProgramId IN (SELECT PlanProgramId FROM Plan_Campaign_Program WHERE PlanCampaignId = @PlanCampaignId))

							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId IN (SELECT PlanProgramId FROM Plan_Campaign_Program WHERE PlanCampaignId = @PlanCampaignId)
              
							   UPDATE Plan_Campaign_Program SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanCampaignId = @PlanCampaignId

							   UPDATE Plan_Campaign SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanCampaignId = @PlanCampaignId

						 END
						 ELSE IF(@PlanProgramId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId IN (SELECT PlanTacticId FROM dbo.Plan_Campaign_Program_Tactic WHERE PlanProgramId= @PlanProgramId)

							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId = @PlanProgramId
              
							   UPDATE Plan_Campaign_Program SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanProgramId = @PlanProgramId
						 END
						 ELSE IF(@PlanTacticId IS NOT NULL)
						 BEGIN
						       UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanTacticId=@PlanTacticId
							   UPDATE Plan_Campaign_Program_Tactic SET IsDeleted = @IsDelete, ModifiedDate=@ModifiedDate, ModifiedBy=@ModifiedBy WHERE PlanTacticId = @PlanTacticId
						 END
						 ELSE IF(@PlanLineItemId IS NOT NULL)
						 BEGIN
						 UPDATE dbo.Plan_Campaign_Program_Tactic_LineItem SET IsDeleted=@IsDelete,ModifiedBy=@ModifiedBy,ModifiedDate=@ModifiedDate WHERE PlanLineItemId=@PlanLineItemId
						 END

				  ---Successfully deleted
				  COMMIT TRANSACTION @TranName;
			   SET @ReturnValue = 1;        
			   RETURN @ReturnValue 
                
		   END TRY
		   BEGIN CATCH
				  ---Unsuccess
				  ROLLBACK TRANSACTION @TranName;
				  RETURN @ReturnValue  
		   END CATCH 
	END

