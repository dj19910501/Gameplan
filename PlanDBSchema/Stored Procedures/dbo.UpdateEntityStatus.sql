/****** Object:  StoredProcedure [dbo].[UpdateEntityStatus]    Script Date: 11/08/2016 5:13:17 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateEntityStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateEntityStatus]
GO
/****** Object:  StoredProcedure [dbo].[UpdateEntityStatus]    Script Date: 11/08/2016 5:13:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateEntityStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateEntityStatus] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 11-8-2016
-- Description:	Method to update entity(tactic,Program,Campign) status as per current date and child status.
-- =============================================
ALTER PROCEDURE [dbo].[UpdateEntityStatus]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @PlanTacticId int,@StartDate datetime,@enddate datetime, @TacticStatus NVARCHAR(MAX),@PlanProgramId int

DECLARE @ParentProgramTable TABLE(PlanProgramId int) 
DECLARE @ParentCampaignTable TABLE(PlanCampaignId int) 

DECLARE @TacticTable TABLE ( PlanTacticID INT, Status NVARCHAR(120),PlanProgramId int) 
DECLARE @ProgramTable TABLE(PlanCampaignId int,Status NVARCHAR(120), startdate datetime,enddate datetime,PlanProgramId int) 

DECLARE @CampaingProgramTable TABLE ( PlanProgramId INT, Status NVARCHAR(120),PlanCampaignId int ) 
DECLARE @CampaignTable TABLE(Status NVARCHAR(120), startdate datetime,enddate datetime,PlanCampaignId int) 

Declare @SatatusInProgress nvarchar(25)='In-Progress',
		@SatatusApproved nvarchar(25)='Approved',
		@SatatusComplete nvarchar(25)='Complete',
		@SatatusDecline nvarchar(25)='Decline',
		@SatatusCreated nvarchar(25)='Created',
		@SatatusSubmitted nvarchar(25)='Submitted'

-- cursor to update tactic
DECLARE TacticCursor CURSOR FOR
SELECT  PlanTacticId,[Status], StartDate,enddate,PlanProgramId from Plan_Campaign_Program_Tactic 
where IsDeleted=0 and status in ('In-Progress','Approved') and ((GETDATE()> StartDate and GETDATE()<enddate) or (GETDATE()>enddate))


OPEN TacticCursor
FETCH NEXT FROM TacticCursor
INTO @PlanTacticId, @TacticStatus, @StartDate,@enddate ,@PlanProgramId
	WHILE @@FETCH_STATUS = 0
		BEGIN
		if(GETDATE()> @StartDate and GETDATE()<@enddate and @TacticStatus!=@SatatusInProgress)
		begin
		
			update  Plan_Campaign_Program_Tactic --update status to in-progress
			set status =@SatatusInProgress
			where  PlanTacticId=@PlanTacticId
			If not exists(select * from @ParentProgramTable where PlanProgramId=@PlanProgramId)
			insert into @ParentProgramTable(PlanProgramId)values(@PlanProgramId)
		end
		else if(GETDATE()>@enddate and @TacticStatus!=@SatatusComplete)
		Begin
			update  Plan_Campaign_Program_Tactic  --update status to complete
			set status =@SatatusComplete
			where PlanTacticId=@PlanTacticId
			If not exists(select * from @ParentProgramTable where PlanProgramId=@PlanProgramId)
			insert into @ParentProgramTable(PlanProgramId)values(@PlanProgramId)
		End

		FETCH NEXT FROM TacticCursor
		INTO @PlanTacticId, @TacticStatus, @StartDate,@enddate ,@PlanProgramId
	END
CLOSE TacticCursor
DEALLOCATE TacticCursor
--end


Declare @TacticCnt int, @cntAllCreateTacticStatus int, @cntAllSumbitTacticStatus int,@cntAllApproveTacticStatus int,@cntAllDeclineTacticStatus int, @PlanCampignID int,
@cntSubmitTacticStatus int ,@cntApproveTacticStatus int,@cntDeclineTacticStatus int,@flag int, @ProgramStatus nvarchar(50),@ProgramNewStatus nvarchar(50)='',
@cntCompleteTacticStatus int,@cntInProgressTacticStatus int

DECLARE @tblProgramTactics TABLE ( TacticId INT, Status NVARCHAR(120) ) 

insert into @TacticTable
select a.PlanTacticId,[Status],b.PlanProgramId from Plan_Campaign_Program_Tactic a
inner join @ParentProgramTable b on a.PlanProgramId=b.PlanProgramId
where a.IsDeleted=0

insert into @ProgramTable
select PlanCampaignId, [Status],StartDate,EndDate,a.PlanProgramId from Plan_Campaign_Program a
inner join @ParentProgramTable b on a.PlanProgramId=b.PlanProgramId
where a.IsDeleted=0

--cursor to modify program whose tactics get updated
DECLARE ProgramCursor CURSOR FOR
select  PlanProgramId from @ParentProgramTable

OPEN ProgramCursor
FETCH NEXT FROM ProgramCursor
INTO @PlanProgramId
	WHILE @@FETCH_STATUS = 0
		BEGIN

		select @TacticCnt=COUNT(*) from @TacticTable where PlanProgramId=@PlanProgramId
		If(@TacticCnt>0)
		Begin
			Insert into @tblProgramTactics
			select PlanTacticId,[Status] from @TacticTable where PlanProgramId=@PlanProgramId 

			set @cntAllCreateTacticStatus=(select count(*) from @tblProgramTactics where [status]!=@SatatusCreated)
			set @cntAllSumbitTacticStatus=(select count(*) from @tblProgramTactics where [status]!=@SatatusSubmitted)
			set @cntAllApproveTacticStatus=(select count(*) from @tblProgramTactics where [status]!=@SatatusApproved )
			set @cntAllDeclineTacticStatus=(select count(*) from @tblProgramTactics where [status]!=@SatatusDecline)

			set @cntSubmitTacticStatus=(select count(*) from @tblProgramTactics where [status]=@SatatusSubmitted)
			set @cntApproveTacticStatus=(select count(*) from @tblProgramTactics where [status]=@SatatusApproved)
			set @cntDeclineTacticStatus=(select count(*) from @tblProgramTactics where [status]=@SatatusDecline)
			set @cntCompleteTacticStatus=(select count(*) from @tblProgramTactics where [status]=@SatatusComplete)
			set @cntInProgressTacticStatus=(select count(*) from @tblProgramTactics where [status]=@SatatusInProgress)

			select @ProgramStatus=[status],@StartDate=StartDate,@enddate=EndDate,@PlanCampignID=PlanCampaignId from @ProgramTable where PlanProgramId=@PlanProgramId
			set @ProgramNewStatus=@ProgramStatus

			 if (@cntAllSumbitTacticStatus = 0)
                 set @ProgramNewStatus = @SatatusSubmitted
             else if (@cntAllApproveTacticStatus = 0)
                 set @ProgramNewStatus = @SatatusApproved
			 else if (@cntAllDeclineTacticStatus = 0)
                 set @ProgramNewStatus = @SatatusDecline
             else if (@cntAllCreateTacticStatus = 0)
                 set @ProgramNewStatus = @SatatusCreated
			 else
			 begin
				select @flag=count(*) from @tblProgramTactics where Status=@ProgramStatus
				if(@flag=0)
				Begin
					if (@cntSubmitTacticStatus > 0)
                          set @ProgramNewStatus = @SatatusSubmitted
                    else if (@cntApproveTacticStatus > 0)
                          set @ProgramNewStatus = @SatatusApproved
                    else if (@cntDeclineTacticStatus > 0)
                          set @ProgramNewStatus =@SatatusDecline
					else if((@cntCompleteTacticStatus=@TacticCnt) OR (@cntInProgressTacticStatus=@TacticCnt))
					begin
					 if(GETDATE()> @StartDate and GETDATE()<@enddate)
						set @ProgramNewStatus =@SatatusInProgress
					 else if(GETDATE()>@enddate)
						set @ProgramNewStatus =@SatatusComplete
					end
					
				End
			 end
			 if(@ProgramNewStatus!=@ProgramStatus)
			 Begin
				 Update  Plan_Campaign_Program set [Status]= @ProgramNewStatus  where PlanProgramId=@PlanProgramId
				 If not exists(select PlanCampaignId from @ParentCampaignTable  where PlanCampaignId=@PlanCampignID)
				 Begin
				 insert into @ParentCampaignTable(PlanCampaignId)values(@PlanCampignID)
				 End
			 End
		End

		FETCH NEXT FROM ProgramCursor
		INTO @PlanProgramId
	END
CLOSE ProgramCursor
DEALLOCATE ProgramCursor
--end


Declare @ProgramCnt int, @cntAllCreateProgramStatus int, @cntAllSumbitProgramStatus int,@cntAllApproveProgramStatus int,@cntAllDeclineProgramStatus int, @PlanCampaignId int,
@cntSubmitProgramStatus int ,@cntApproveProgramStatus int,@cntDeclineProgramStatus int, @CampaignStatus nvarchar(50),@CampaignNewStatus nvarchar(50)='',
@cntCompleteProgramStatus int,@cntInProgressProgramStatus int

DECLARE @tblCampaignProgram TABLE ( ProgramID INT, Status NVARCHAR(120) ) 

insert into @CampaingProgramTable
select PlanProgramId,[Status],a.PlanCampaignId from Plan_Campaign_Program a
inner join @ParentCampaignTable b on a.PlanCampaignId=b.PlanCampaignId
where a.IsDeleted=0

insert into @CampaignTable
select  [Status],StartDate,EndDate,b.PlanCampaignId from Plan_Campaign a
inner join @ParentCampaignTable b on a.PlanCampaignId=b.PlanCampaignId
where a.IsDeleted=0

--cursor to modify Campaign whose Program get updated
DECLARE CampaignCursor CURSOR FOR
select  PlanCampaignId from @ParentCampaignTable

OPEN CampaignCursor
FETCH NEXT FROM CampaignCursor
INTO @PlanCampaignId
	WHILE @@FETCH_STATUS = 0
		BEGIN

		select @ProgramCnt=COUNT(*) from @CampaingProgramTable where PlanCampaignId=@PlanCampaignId 
		If(@ProgramCnt>0)
		Begin
			Insert into @tblCampaignProgram
			select PlanProgramId,[Status] from @CampaingProgramTable where PlanCampaignId=@PlanCampaignId 

			set @cntAllCreateProgramStatus=(select count(*) from @tblCampaignProgram where [status]!=@SatatusCreated)
			set @cntAllSumbitProgramStatus=(select count(*) from @tblCampaignProgram where [status]!=@SatatusSubmitted)
			set @cntAllApproveProgramStatus=(select count(*) from @tblCampaignProgram where [status]!=@SatatusApproved )
			set @cntAllDeclineProgramStatus=(select count(*) from @tblCampaignProgram where [status]!=@SatatusDecline)

			set @cntSubmitProgramStatus=(select count(*) from @tblCampaignProgram where [status]=@SatatusSubmitted)
			set @cntApproveProgramStatus=(select count(*) from @tblCampaignProgram where [status]=@SatatusApproved)
			set @cntDeclineProgramStatus=(select count(*) from @tblCampaignProgram where [status]=@SatatusDecline)
			set @cntCompleteProgramStatus=(select count(*) from @tblCampaignProgram where [status]=@SatatusComplete)
			set @cntInProgressProgramStatus=(select count(*) from @tblCampaignProgram where [status]=@SatatusInProgress)

			select @CampaignStatus=[status],@StartDate=StartDate,@enddate=EndDate from @CampaignTable where PlanCampaignId=@PlanCampaignId
			set @CampaignNewStatus=@CampaignStatus

			 if (@cntAllSumbitProgramStatus = 0)
                 set @CampaignNewStatus =@SatatusSubmitted
             else if (@cntAllApproveProgramStatus = 0)
                 set @CampaignNewStatus =@SatatusApproved
			 else if (@cntAllDeclineProgramStatus = 0)
                 set @CampaignNewStatus =@SatatusDecline
			 else
			 begin
				select @flag=count(*) from @tblCampaignProgram where Status=@CampaignStatus
				if(@flag=0)
				Begin
			
					if (@cntSubmitProgramStatus > 0)
                          set @CampaignNewStatus = @SatatusSubmitted
                    else if (@cntApproveProgramStatus > 0)
                          set @CampaignNewStatus = @SatatusApproved
                    else if (@cntDeclineProgramStatus > 0)
                          set @CampaignNewStatus = @SatatusDecline
					else if((@cntCompleteProgramStatus=@ProgramCnt) OR (@cntInProgressProgramStatus=@ProgramCnt))
					begin
						  if(GETDATE()> @StartDate and GETDATE()<@enddate)
							set @CampaignNewStatus =@SatatusInProgress
						  else if(GETDATE()>@enddate)
							set @CampaignNewStatus =@SatatusComplete
					end
				End
			 end
			 if(@CampaignNewStatus!=@CampaignStatus)
			 Begin
					Update  Plan_Campaign set [Status]= @CampaignNewStatus  where PlanCampaignId=@PlanCampaignId
			 End
		End

		FETCH NEXT FROM CampaignCursor
		INTO @PlanCampaignId
	END
CLOSE CampaignCursor
DEALLOCATE CampaignCursor
--end
END

GO
