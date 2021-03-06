/* Start- Added by Viral for Ticket #2595 on 10/14/2016 */

/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 10/14/2016 2:19:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spViewByDropDownList]
GO
/****** Object:  StoredProcedure [dbo].[spViewByDropDownList]    Script Date: 10/14/2016 2:19:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spViewByDropDownList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spViewByDropDownList] AS' 
END
GO


ALTER PROCEDURE  [dbo].[spViewByDropDownList] 
	-- Add the parameters for the stored procedure here
	@PlanId NVARCHAR(max),
	@ClientId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tblCustomerFieldIDs TABLE ( EntityType NVARCHAR(120), EntityID INT, CustomFieldID INT) 
	DECLARE @entCampaign varchar(20)='Campaign'
	DECLARE @entProgram varchar(20)='Program'
	DECLARE @entTactic varchar(20)='Tactic'
	DECLARE @PlanIds Table(
		PlanId int
	)

	INSERT INTO @PlanIds SELECT Cast(IsNUll(val,0) as int) FROM dbo.comma_split(@Planid, ',')

	INSERT INTO @tblCustomerFieldIDs
	SELECT @entCampaign, A.PlanCampaignId, B.CustomFieldId 
		FROM Plan_Campaign A 
		JOIN CustomField_Entity B WITH (NOLOCK) ON A.PlanCampaignId = B.EntityId
		JOIN CustomField CS WITH (NOLOCK) ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entCampaign
		WHERE A.IsDeleted='0' and A.PlanId IN (SELECT PlanId FROM @PlanIds)
	
	INSERT INTO @tblCustomerFieldIDs
	SELECT @entProgram, A.PlanProgramId, B.CustomFieldId 
		FROM Plan_Campaign_Program A 
		JOIN CustomField_Entity B WITH (NOLOCK) ON A.PlanProgramId = B.EntityId
		JOIN CustomField CS WITH (NOLOCK) ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entProgram
		JOIN Plan_Campaign C WITH (NOLOCK) ON A.PlanCampaignId = C.PlanCampaignId and C.IsDeleted='0' and C.PlanId IN (SELECT PlanId FROM @PlanIds)
		WHERE A.IsDeleted='0'
	
	INSERT INTO @tblCustomerFieldIDs
	SELECT @entTactic, A.PlanTacticId, B.CustomFieldId 
		FROM Plan_Campaign_Program_Tactic A 
		JOIN CustomField_Entity B WITH (NOLOCK) ON A.PlanTacticId = B.EntityId
		JOIN CustomField CS WITH (NOLOCK) ON B.CustomFieldId = CS.CustomFieldId and CS.EntityType=@entTactic
		JOIN Plan_Campaign_Program P WITH (NOLOCK)  ON A.PlanProgramId = P.PlanProgramId and P.IsDeleted='0'
		JOIN Plan_Campaign C WITH (NOLOCK) ON P.PlanCampaignId = C.PlanCampaignId and C.IsDeleted='0' and C.PlanId IN (SELECT PlanId FROM @PlanIds)
		WHERE A.IsDeleted='0'
	
	    SELECT DISTINCT(A.Name) AS [Text],A.EntityType +'Custom'+ Cast(A.CustomFieldId as nvarchar(50)) as Value  
		FROM CustomField A WITH (NOLOCK) JOIN CustomFieldType B WITH (NOLOCK) 
		ON A.CustomFieldTypeId = B.CustomFieldTypeId JOIN @tblCustomerFieldIDs C 
		ON C.CustomFieldID = A.CustomFieldId
		WHERE A.ClientId=@ClientId AND A.IsDeleted=0 AND A.IsDisplayForFilter=1 AND A.EntityType IN ('Tactic','Campaign','Program') and B.Name='DropDownList' 
		ORDER BY Value DESC 

 END

GO

/* End- Added by Viral for Ticket #2595 on 10/14/2016 */