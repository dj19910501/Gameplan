IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'StatisticallyInferredAttribution') AND xtype IN (N'P'))
    DROP PROCEDURE StatisticallyInferredAttribution
GO




CREATE PROCEDURE StatisticallyInferredAttribution 
	@StartDate Date ='1/1/1900', @EndDate Date = '1/1/2100', @BaseRateQuery varchar(max)=null,
	@CLOSED_WON varchar(max)='''Closed Won''', @CLOSED_STAGES nvarchar(255)='''Closed Lost'',''Closed Won''',
	 @SELECT_CAMPAIGN_COUNTS varchar(max)=null, @CAMPAIGN_TABLE varchar(max)='_Campaigns',
	 @CAMPAIGN_IDFIELD varchar(max)='CampaignID', @MIN_RESPONSES varchar(max)='49',
	 @CAMPAIGN_WHERE varchar(max)=null, @CAMPAIGN_START varchar(max)=null,
	 @FINAL_JOIN varchar(max)=null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @BaseRate float = 0.5
	if @BaseRateQuery<>null
	BEGIN
		SET @BaseRateQuery=replace(@BaseRateQuery,'#EndDate#',@EndDate)
		DECLARE @StorageTable table(val float)
		insert into @StorageTable
		exec(@BaseRateQuery)
		SET @BaseRate=(select top 1 val from @StorageTable)
	END

	SET @SELECT_CAMPAIGN_COUNTS=replace(@SELECT_CAMPAIGN_COUNTS, '#CLOSED_STAGES#', @CLOSED_STAGES)
	SET @SELECT_CAMPAIGN_COUNTS=replace(@SELECT_CAMPAIGN_COUNTS, '#EndDate#', @EndDate)

	SET @CAMPAIGN_WHERE=replace(@CAMPAIGN_WHERE,'#EndDate#',@EndDate)
	SET @CAMPAIGN_WHERE=replace(@CAMPAIGN_WHERE,'#StartDate#',@StartDate)

	DECLARE @CAMPAIGN_SELECT varchar(max)='
	select c2.CampaignId, o2.opportunityid, count(*) as respcount,w1.[Weight] * count(*) as totalWeight   
	from _OpportunityContactRole o1
	inner join _Opportunities o2 on o1.OpportunityId=o2.OpportunityID
	inner join _CampaignMembers c1 on o1.ContactID=c1.ContactID
	inner join _Campaigns c2 on c1.CampaignID=c2.CampaignID
	'


	DECLARE @FINAL_QUERY varchar(max)='
	select a1.CampaignID, c1.CampaignName, sum((a1.totalWeight/a2.AllWeight)*o1.Amount) as allocation from 
	(
	' + @CAMPAIGN_SELECT + '

	inner join 
	(
	select c.' + @CAMPAIGN_IDFIELD + ' as CampaignID, isnull(A.[Weight],0.5) as [weight] from ' + @CAMPAIGN_TABLE + ' c
	left outer join (

		select campaignID, 
		dbo.normsdist((sum(case when stagename=' + @CLOSED_WON + '  then campaign_count else 0 end)/cast(sum(campaign_count) as float)-' + cast(@BASERate as nvarchar) + ')/sqrt((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float))*(1-(sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)))/sum(campaign_count)))
		as [Weight]
		from (

			select campaignID, stagename, count(*) as campaign_count from (

			' + @SELECT_CAMPAIGN_COUNTS + '
			) a 
			group by campaignID, stagename
			) B



		group by campaignID
		having sum(campaign_count)>' + @MIN_RESPONSES + ') A on c.CampaignId=a.CampaignId
		where ' + @CAMPAIGN_START + '<= ''' + cast(@EndDate as nvarchar) + '''


		)
	 w1 on c2.CampaignID=w1.CampaignID

	where ' + @CAMPAIGN_WHERE + '
	group by c2.CampaignID, o2.Opportunityid, w1.[Weight]

	) A1
	inner join (

		select opportunityid, sum(totalWeight) as AllWeight from 
		(
		' + @CAMPAIGN_SELECT + '

			inner join 
			(
				select c.CampaignID, isnull(A.[Weight],0.5) as [weight] from _Campaigns c
				left outer join (

				select campaignID, 
				dbo.normsdist((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)-' + cast( @BASERate as nvarchar) + ')/sqrt((sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float))*(1-(sum(case when stagename=' + @CLOSED_WON + ' then campaign_count else 0 end)/cast(sum(campaign_count) as float)))/sum(campaign_count)))
				as [Weight]
				from (

				select campaignID, stagename, count(*) as campaign_count from (

			' + @SELECT_CAMPAIGN_COUNTS + '
			) a 
			group by campaignID, stagename
			) B


		group by campaignID
		having sum(campaign_count)>' + @MIN_RESPONSES + ') A on c.CampaignId=a.CampaignId
		where ' + @CAMPAIGN_START + '<=''' + cast( @EndDate as nvarchar) + '''


		)
		 w1 on c2.CampaignID=w1.CampaignID

		where ' + @CAMPAIGN_WHERE + '
		group by c2.CampaignID, o2.Opportunityid, w1.[Weight]

		) A
		group by opportunityid 

	) A2 on A1.opportunityid=a2.opportunityid

	' + @FINAL_JOIN + '
	group by a1.CampaignID, c1.CampaignName
	order by allocation desc'


	--print @Final_Query

	exec(@Final_Query)



END
GO
