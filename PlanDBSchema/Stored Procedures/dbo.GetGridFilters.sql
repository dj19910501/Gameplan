/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGridFilters]
GO
/****** Object:  StoredProcedure [dbo].[GetGridFilters]    Script Date: 11/29/2016 4:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGridFilters]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetGridFilters] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[GetGridFilters] 
	@userId int
	,@ClientId int
	,@IsDefaultCustomRestrictionsViewable bit 
AS --Todo: New user login then need to some more 
BEGIN
	
	--EXEC GetGridFilters 549,4,0

	SET NOCOUNT ON;

		Declare @PlanId NVARCHAR(MAX) = ''
		Declare @OwnerIds NVARCHAR(MAX) = ''
		Declare @TacticTypeIds varchar(max)=''
		Declare @StatusIds varchar(max)=''
		Declare @customFields varchar(max)=''

		Declare @viewname  varchar(max)

		Declare @tblUserSavedViews Table(
		Id INT
		,ViewName NVARCHAR(max)
		,FilterName NVARCHAR(1000)
		,FilterValues NVARCHAR(max)
		,LastModifiedDate DATETIME
		,IsDefaultPreset BIT
		,Userid INT
		)

		Declare @keyPlan varchar(30)='Plan'
		Declare @keyOwner varchar(30)='Owner'
		Declare @keyStatus varchar(30)='Status'
		Declare @keyTacticType varchar(30)='TacticType'
		Declare @keyAll varchar(10)='All'
		Declare @keyCustomField varchar(100)='CustomField'
		

		select TOP 1 @viewname =  ViewName from Plan_UserSavedViews where Userid=@userId AND IsDefaultPreset = 1
		SET @viewname = ISNULL(@viewname,'')

		-- Insert user filters to local variables.
		INSERT INTO @tblUserSavedViews(Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid)
		select Id,ViewName,FilterName,FilterValues,LastModifiedDate,IsDefaultPreset,Userid from Plan_UserSavedViews where Userid=@userId AND ISNULL(ViewName,'') = IsNull(@viewname,'')

		IF EXISTS(select Id from @tblUserSavedViews)
		BEGIN
			
		

		-- Get PlanIds that user has selected under filter.
		SELECT TOP 1 @PlanId = FilterValues from @tblUserSavedViews where FilterName=@keyPlan

		-- Get OwnerIds that user has selected under filter.
		SELECT TOP 1 @OwnerIds = FilterValues from @tblUserSavedViews where FilterName=@keyOwner

		-- Get TacticTypeIds that user has selected under filter.
		SELECT TOP 1 @TacticTypeIds = FilterValues from @tblUserSavedViews where FilterName=@keyTacticType

		-- Get Status that user has selected under filter.
		SELECT TOP 1 @StatusIds = FilterValues from @tblUserSavedViews where FilterName=@keyStatus

		-- Get Status that user has selected under filter.
		SET @customFields = ''


		BEGIN

			Declare @customFieldId varchar(100)
			Declare @FilterValues varchar(max)
			Declare @cntFiltr INT
			Declare @cntPermsn INT
			Declare @CustomFilters varchar(max) 


			  Declare @CustomFieldIDs varchar(max)=''

			 SELECT  @CustomFieldIDs = COALESCE(@CustomFieldIDs + ',', '') + CONVERT(varchar(100), CF.[CustomFieldId])  + '_null'  FROM [Hive9ProdGP].[dbo].[CustomField] CF
			 inner join CustomFieldType CT on CT.CustomFieldTypeId = CF.CustomFieldTypeId and CT.name = 'DropDownList'
			  where  CF.EntityType = 'Tactic'  and CF.isdeleted = 0 and CF.IsDisplayForFilter = 1 AND CF.ClientId = @ClientId 
             AND ( CF.[CustomFieldId] NOT IN
			  (select  CAST((CASE WHEN REPLACE(FilterName,'CF_','') NOT LIKE '%[^0-9]%' THEN REPLACE(FilterName,'CF_','') END) AS INT)  from [Hive9ProdGP].DBO.Plan_UserSavedViews where Userid=@userId and FilterName like'CF_%')
			  )

			   if( LEFT(@CustomFieldIDs,1) = ',')
			     SET  @CustomFieldIDs = substring(@CustomFieldIDs,2,LEN(@CustomFieldIDs))

			select @cntPermsn = count(*) from CustomRestriction as CR Where UserId = @userId

			


			
			DECLARE db_cursor CURSOR FOR  
			select REPLACE(FilterName,'CF_',''),FilterValues from @tblUserSavedViews where Userid=@userId and FilterName like'CF_%'
			
			OPEN db_cursor   
			FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues   
			
			WHILE @@FETCH_STATUS = 0   
			BEGIN   
				
				IF(IsNull(@cntPermsn,0) > 0)
				BEGIN
				   select @cntFiltr = count(*) from CustomRestriction as CR
					JOIN CustomField as C on CR.CustomFieldId = C.CustomFieldId and C.ClientId=@ClientId and IsDeleted='0' and C.IsRequired='1' and ( (CR.Permission = 1)  OR (CR.Permission = 2) )
					where UserId = @userId and C.CustomFieldId=Cast(@customFieldId as INT) and cr.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
			
				

				END
				ELSE IF(@IsDefaultCustomRestrictionsViewable = '1' )
				BEGIN
						SELECT @cntFiltr = count(*) from CustomField as C
						Join CustomFieldType as CT on C.CustomFieldTypeId = C.CustomFieldTypeId and CT.Name ='DropDownList'
						JOIN CustomFieldOption as CO on C.CustomFieldId = CO.CustomFieldId and CO.IsDeleted='0' and CO.CustomFieldOptionId not in (select val from comma_split(@FilterValues,','))
						where ClientId=@ClientId and C.IsDeleted='0' and  EntityType='Tactic' and IsDisplayForFilter='1' and C.CustomFieldId= Cast(@customFieldId as INT)
				END

				IF (IsNull(@cntFiltr,0) > 0)
				BEGIN
					SET @customFields = @customFields + ',' + @customFieldId + '_' + REPLACE(@FilterValues,',',','+@customFieldId + '_' ) 
				END
				
			       FETCH NEXT FROM db_cursor INTO @customFieldId,@FilterValues      
			END   
			
			CLOSE db_cursor   
			DEALLOCATE db_cursor
			if(LEN(@customFields) > 2)
			SET @customFields = SUBSTRING(@customFields,2, LEN(@customFields)-1) 

		END

		END
		ELSE
		BEGIN
			
			SELECT Top 1 @PlanId = p.PlanId from Model as M
			JOIN [Plan] as P on M.ModelId = P.ModelId and M.ClientId=@ClientId and P.IsDeleted='0' and M.IsDeleted='0' and P.[Year]<=Datepart(yyyy,GETDATE())
			Order by P.Year desc, P.Title
			
			SET @OwnerIds=@keyAll
			SET @TacticTypeIds=@keyAll
			SET @StatusIds='Created,Submitted,Approved,In-Progress,Complete,Declined'
			SET @customFields = ''
		END
		
		IF(@CustomFieldIDs != '')
		BEGIN
		 IF(@customFields = '')
				    BEGIN
				       SET @customFields =  @CustomFieldIDs
				   END
				   ELSE
				   BEGIN
				      SET @customFields = @customFields + ',' + @CustomFieldIDs
				   END
		END
	             	
				
			

		select @PlanId PlanIds,@OwnerIds OwnerIds,@StatusIds StatusIds,@TacticTypeIds TacticTypeIds,@customFields CustomFieldIds

    
END



GO
