/****** Object:  StoredProcedure [dbo].[SavePlanDefaultFilters]    Script Date: 11/10/2016 7:57:06 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavePlanDefaultFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SavePlanDefaultFilters]
GO
/****** Object:  StoredProcedure [dbo].[SavePlanDefaultFilters]    Script Date: 11/10/2016 7:57:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavePlanDefaultFilters]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SavePlanDefaultFilters] AS' 
END
GO
-- =============================================
-- Author: Viral
-- Create date: 11/10/2016
-- Description:	Save default filters while user created new plan.
-- =============================================
ALTER PROCEDURE [dbo].[SavePlanDefaultFilters] 
	@userId int
	,@ClientId int
	,@IsDefaultCustomRestrictionsViewable bit
	,@newPlanId int
AS --Todo: New user login then need to some more 
BEGIN
	
	--EXEC [SavePlanDefaultFilters] 549,4,'1',226

	SET NOCOUNT ON;

		Declare @PlanId NVARCHAR(MAX) = ''
		Declare @OwnerIds NVARCHAR(MAX) = ''
		Declare @TacticTypeIds varchar(max)=''
		Declare @StatusIds varchar(max)='Created,Submitted,Approved,In-Progress,Complete,Declined'
		Declare @Year varchar(max)=''
		

		Declare @keyPlan varchar(30)='Plan'
		Declare @keyOwner varchar(30)='Owner'
		Declare @keyStatus varchar(30)='Status'
		Declare @keyTacticType varchar(30)='TacticType'
		Declare @keyAll varchar(10)='All'
		Declare @keyCustomField varchar(100)='CustomField'
		Declare @keyYear varchar(30)='Year'
		
		-- IF Default view already exist then delete the records.
		DELETE FROM Plan_UserSavedViews where Userid=@userId AND IsNull(IsDefaultPreset,0) = 0 and IsNull(ViewName,'') =''
		

		-- Set Filter Fields

		---- Set PlanIds under filter.
		Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid) 
		Select Null,@keyPlan,@newPlanId,GETDATE(),0,@userId

		---- SET Status under filter.
		Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid) 
		Select Null,@keyStatus,@StatusIds,GETDATE(),0,@userId

		---- Get Status that user has selected under filter.
		Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid) 
		Select Null,@keyYear,[Year],GETDATE(),0,@userId FROM [Plan] where PlanId = @newPlanId

		---- Set PlanIds under filter.
		Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid) 
		Select Null,@keyOwner,'',GETDATE(),0,@userId

		---- Set PlanIds under filter.
		Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid) 
		Select Null,@keyTacticType,'',GETDATE(),0,@userId
		


		BEGIN

			Declare @FilterValues varchar(max)
			Declare @cntFiltr INT
			Declare @cntPermsn INT
			
			select @cntPermsn = count(*) from CustomRestriction as CR Where UserId = @userId


			Declare @tblCustmFilters table(
				CustomFieldId int,
				FilterValues varchar(max)
			)

			IF(@IsDefaultCustomRestrictionsViewable = '1' )
			BEGIN
					INSERT INTO @tblCustmFilters(CustomFieldId)
					SELECT Distinct C.CustomFieldId from CustomField as C
					Join CustomFieldType as CT on C.CustomFieldTypeId = C.CustomFieldTypeId and CT.Name ='DropDownList'
					JOIN CustomFieldOption as CO on C.CustomFieldId = CO.CustomFieldId and CO.IsDeleted='0'
					where ClientId=@ClientId and C.IsDeleted='0' and  EntityType='Tactic' and IsDisplayForFilter='1'
			END
			ELSE IF(IsNull(@cntPermsn,0) > 0)
			BEGIN
			
				INSERT INTO @tblCustmFilters(CustomFieldId)
				select Distinct C.CustomFieldId as 'FilterValues'  from CustomRestriction as CR
				JOIN CustomField as C on CR.CustomFieldId = C.CustomFieldId and C.ClientId=@ClientId and IsDeleted='0' and C.IsRequired='1' and ( (CR.Permission = 1)  OR (CR.Permission = 2) )
				where UserId = @userId 

			END

			BEGIN   
				Insert INTO Plan_UserSavedViews(ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid)
				SELECT Null,'CF_'+Cast(C.CustomFieldId as varchar(50)),
				STUFF((SELECT ',' + COALESCE(LTRIM(RTRIM(CO.CustomFieldOptionId)), '') 
				FROM @tblCustmFilters fC
				INNER JOIN CustomFieldOption CO
				on fC.CustomFieldId = CO.CustomFieldId
				WHERE fC.CustomFieldId = C.CustomFieldId
				FOR XML PATH('') ), 1, 1, '') as 'FilterValues',
				GETDATE(),
				'0',
				@userId
				FROM @tblCustmFilters C
				
			END   

		END
    
END



GO
