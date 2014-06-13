-- Run this script on MRP Database

GO
/****** Object:  StoredProcedure [dbo].[SaveModelInboundOutboundEvent]    Script Date: 6/10/2014 6:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[SaveModelInboundOutboundEvent] 
	 (    
      @OldModelId INT,    
	  @NewModelId INT,    
      @CreatedDate DateTime,    
	  @CreatedBy UNIQUEIDENTIFIER,   
	  @ReturnValue INT = 0 OUT              
    ) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	 BEGIN TRANSACTION SaveTransaction


    INSERT INTO Model_Audience_Outbound ([ModelId],[NormalErosion],[UnsubscribeRate],[NumberofTouches],[CTRDelivered],[RegistrationRate],[Quarter],[ListAcquisitions],[ListAcquisitionsNormalErosion],[ListAcquisitionsUnsubscribeRate],[ListAcquisitionsCTRDelivered],[Acquisition_CostperContact],[Acquisition_NumberofTouches],[Acquisition_RegistrationRate],[CreatedDate],[CreatedBy])
	SELECT @NewModelId,[NormalErosion],[UnsubscribeRate],[NumberofTouches],[CTRDelivered],[RegistrationRate],[Quarter],[ListAcquisitions],[ListAcquisitionsNormalErosion],[ListAcquisitionsUnsubscribeRate],[ListAcquisitionsCTRDelivered],[Acquisition_CostperContact],[Acquisition_NumberofTouches],[Acquisition_RegistrationRate],@CreatedDate,@CreatedBy FROM Model_Audience_Outbound where ModelId = @OldModelId 

	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION SaveTransaction                    
					RETURN                
				END

	INSERT INTO Model_Audience_Inbound ([ModelId],[Quarter],[Impressions],[ClickThroughRate],[Visits],[RegistrationRate],[PPC_ClickThroughs],[PPC_CostperClickThrough],[PPC_RegistrationRate],[GC_GuaranteedCPLBudget],[GC_CostperLead],[CSC_NonGuaranteedCPLBudget],[CSC_CostperLead],[TDM_DigitalMediaBudget],[TDM_CostperLead],[TP_PrintMediaBudget],[TP_CostperLead],[CreatedDate],[CreatedBy])
	SELECT @NewModelId,[Quarter],[Impressions],[ClickThroughRate],[Visits],[RegistrationRate],[PPC_ClickThroughs],[PPC_CostperClickThrough],[PPC_RegistrationRate],[GC_GuaranteedCPLBudget],[GC_CostperLead],[CSC_NonGuaranteedCPLBudget],[CSC_CostperLead],[TDM_DigitalMediaBudget],[TDM_CostperLead],[TP_PrintMediaBudget],[TP_CostperLead],@CreatedDate,@CreatedBy FROM Model_Audience_Inbound where ModelId = @OldModelId 

	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION SaveTransaction                    
					RETURN                
				END

	INSERT INTO Model_Audience_Event ([ModelId],[Quarter],[NumberofContacts],[ContactToInquiryConversion],[EventsBudget],[CreatedDate],[CreatedBy])
	SELECT @NewModelId,[Quarter],[NumberofContacts],[ContactToInquiryConversion],[EventsBudget],@CreatedDate,@CreatedBy FROM Model_Audience_Event where ModelId = @OldModelId 

	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION SaveTransaction                    
					RETURN                
				END


	INSERT INTO TacticType ([Title],[Description],[ClientId],[ColorCode],[ModelId],[StageId],[ProjectedRevenue],[CreatedDate],[CreatedBy],[PreviousTacticTypeId],[ProjectedStageValue])
	SELECT [Title],[Description],[ClientId],[ColorCode],@NewModelId,[StageId],[ProjectedRevenue],@CreatedDate,@CreatedBy,[TacticTypeId],[ProjectedStageValue] FROM TacticType where ModelId = @OldModelId 

	IF @@ERROR <> 0     
				BEGIN                
					SET @ReturnValue = 0 -- 0 ERROR                
					ROLLBACK TRANSACTION SaveTransaction                    
					RETURN                
				END

	COMMIT TRANSACTION SaveTransaction                  
    SET @ReturnValue = @NewModelId;        
    RETURN @ReturnValue 

END
