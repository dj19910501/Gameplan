-- =============================================
-- Author:		Dhvani Raval
-- Create date: 09/26/2016
-- Description:	This store proc. is used to senf alerts mail
-- =============================================
IF EXISTS( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'sp_Send_Mail_For_Alerts') AND type IN ( N'P', N'PC' ) )
BEGIN
	DROP PROCEDURE dbo.sp_Send_Mail_For_Alerts
END
GO

CREATE PROCEDURE [dbo].[sp_Send_Mail_For_Alerts]
        @from Nvarchar(500),
		@pwd Nvarchar(30),
		@Url Nvarchar(max),
		@SMTPPort Nvarchar(10),
		@SMTPServerAddress Nvarchar(100)	
AS 
BEGIN

  DECLARE @imsg int
  DECLARE @hr int
  DECLARE @source varchar(255)
  DECLARE @description varchar(500)
  DECLARE @bodytype varchar(10)
  DECLARE @to varchar(500) 
  DECLARE @Comparision varchar(500)
  DECLARE @body varchar(max) 
  DECLARE @subject varchar(max) 
  DECLARE @output_desc varchar(1000)
  DECLARE @Result varchar(1000)
  DECLARE @UrlString varchar(max)
  DECLARE @SubjectComparision varchar(500)
  DECLARE @Query Nvarchar(max)
   
  SET @bodytype = 'htmlbody'
  
  BEGIN
  DECLARE @AlertId int, @AlertDescription nvarchar(max), @Email  nvarchar(255),@Indicator nvarchar(50), @IndicatorComparision nvarchar(10),@IndicatorGoal int, @EntityTitle nvarchar(500),@DisplayDate DateTime, @ActualGoal float, @CurrentGoal float,@PlanName nvarchar(255),@Entity nvarchar(255),@PlanId int,@EntityId int
  			
  DECLARE Cur_Mail CURSOR FOR	
  --Select alert data that has display date same as current date or less than that, email not sent and that has user email address 
               SELECT  al.AlertId, al.Description, ar.UserEmail, 
			   CASE WHEN ar.Indicator = 'PLANNEDCOST' 
					THEN 'PLANNED COST'
					when ar.Indicator = 'REVENUE' 
					THEN 'REVENUE'
					ELSE Stage.Title END AS
					 Indicator, ar.IndicatorComparision,ar.IndicatorGoal, vw.EntityTitle,al.DisplayDate,al.ActualGoal,al.CurrentGoal, vw.PlanTitle,vw.Entity, vw.PlanId,vw.EntityId
			   FROM  [dbo].Alerts al
			   INNER JOIN dbo.[Alert_Rules] AS ar ON ar.RuleId = al.RuleId  AND ar.UserId = al.UserId
			   LEFT JOIN dbo.Stage ON Stage.Code = ar.Indicator  AND ar.ClientId = Stage.ClientId
			   LEFT JOIN dbo.vClientWise_EntityList as vw on vw.EntityId = ar.EntityId and vw.Entity = ar.EntityType
			   WHERE al.DisplayDate <= GETDATE() AND (isnull(al.IsEmailSent,'') <> 'Success') AND (ar.UserEmail IS NOT NULL AND  ar.UserEmail <> '')
  
    
  OPEN Cur_Mail
  
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate,@ActualGoal,@CurrentGoal,@PlanName,@Entity,@PlanId,@EntityId
        
		
        WHILE @@FETCH_STATUS=0
        BEGIN


		Begin
		--print  @Email
        EXEC @hr = sp_oacreate 'cdo.message', @imsg out
        
        --SendUsing Specifies Whether to send using port (2) or using pickup directory (1)
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusing").value','2'
        
        --SMTP Server
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserver").value',@SMTPServerAddress 
        
		--PORT 
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpserverport").value',@SMTPPort

        --UserName
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendusername").value',@from
        
        --Password
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/sendpassword").value',@pwd
  
        --UseSSL
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpusessl").value','True' 
        			       
        --Requires Aunthentication None(0) / Basic(1)
        EXEC @hr = sp_oasetproperty @imsg, 'configuration.fields("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate").value','1' 	
        
	END
      
        IF @IndicatorComparision = 'GT'
		BEGIN
              set  @Comparision = '<b> greater than </b>'
			  set @SubjectComparision = 'above goal'
		END
        ELSE IF @IndicatorComparision = 'LT'
			  	 BEGIN
                  set @Comparision = '<b>  less than </b>' 
				  set @SubjectComparision = 'below goal'
				 END
              ELSE 
			     BEGIN
                  set @Comparision = '<b> equal to </b>' 
				  set @SubjectComparision = 'equal goal'
				END
		
		
        Set @subject = @EntityTitle + ' is performing ' + @SubjectComparision 

		SET @body = 'Hi, <br><br>'+ @EntityTitle + '''s <b>' + @Indicator +' </b> is '+ @Comparision + ' ' + CONVERT(nvarchar(50),@IndicatorGoal) +'% of the goal as of <b>'+ CONVERT(VARCHAR(11),@DisplayDate,106)  + '</b><br><br>Item : ' +  @EntityTitle + '<br>Plan Name : '+ @PlanName 
	    
		IF (@ActualGoal IS NOT NULL AND @ActualGoal <> '' )
		SET @body = @body +  '<br>Goal : $'+ cast(Format(@ActualGoal, '##,##0') as varchar) 

		IF (@CurrentGoal IS NOT NULL AND @CurrentGoal <> '')
		SET @body = @body + '<br>Current : $' + cast(Format(@CurrentGoal, '##,##0') as varchar)
		 

		 IF (@Url <> '' AND @Url IS NOT NULL)
		 BEGIN
			 IF @Entity <> 'Plan' 
			 BEGIN
			 print @Entity
			     SET @UrlString = @Url +'home?currentPlanId='+convert(nvarchar(max), @planId)+'&plan'+ Replace(@Entity,' ' ,'')+'Id='+convert(nvarchar(max),@EntityId)+'&activeMenu=Home&ShowPopup=true'
			 END
			 ELSE
			 BEGIN
			     SET @UrlString = @Url +'home?currentPlanId='+convert(nvarchar(max), @planId)+'&activeMenu=Home&ShowPopup=true'
			 END
		 END

		set @body = @body + '<br><br><html><body> URL : <a href=' + @UrlString +'>'+@UrlString+'</a></body></html>' 

		set @body = @body + '<br> Thank you,<br>Hive9 Plan Admin.'
        
        EXEC @hr = sp_oamethod @imsg, 'configuration.fields.update', null
        EXEC @hr = sp_oasetproperty @imsg, 'to', @Email
        EXEC @hr = sp_oasetproperty @imsg, 'from', @from
        EXEC @hr = sp_oasetproperty @imsg, 'subject', @subject
        EXEC @hr = sp_oasetproperty @imsg, @bodytype, @body
        EXEC @hr = sp_oamethod @imsg, 'send',null        
		
        -- sample error handling.	
        IF @hr <> 0   
        	BEGIN
			    EXEC @hr = sp_oageterrorinfo null, out, @description out
			    IF @hr = 0
        		BEGIN			
        		    update Alerts
            		set IsEmailSent =  @description  --'Not Sent'
        			where AlertId = @AlertId
				END
				ELSE
				BEGIN
        		    update Alerts
            		set IsEmailSent = 'FAIL'
        			where AlertId = @AlertId
				END
			END
        ELSE
        	BEGIN
        		update  Alerts
        		set IsEmailSent = 'Success'
        		where AlertId = @AlertId
        	END

             
        EXEC @hr = sp_oadestroy @imsg
        
        FETCH NEXT FROM Cur_Mail INTO  @AlertId, @AlertDescription, @Email, @Indicator, @IndicatorComparision, @IndicatorGoal, @EntityTitle, @DisplayDate, @ActualGoal, @CurrentGoal, @PlanName,@Entity,@PlanId,@EntityId
  
		END
  CLOSE Cur_Mail
  DEALLOCATE Cur_Mail
  END
  END