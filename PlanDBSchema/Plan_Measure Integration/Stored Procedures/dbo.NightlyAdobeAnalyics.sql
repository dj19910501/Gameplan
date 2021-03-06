IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'NightlyAdobeAnalyics') AND xtype IN (N'P'))
    DROP PROCEDURE NightlyAdobeAnalyics
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [NightlyAdobeAnalyics]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_WebPageAnalytics_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_WebPageAnalytics_Value0]

	DELETE FROM _WebPageAnalytics WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer1]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer2]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeAll3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeCountry3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeKeyword3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOS3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeOSType3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobePaidKeyword3]  WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebPageAnalytics ([reportSuite],[datestamp],[Element],[ElementValue],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] as date),[Element],[Value],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Bouncerate],[TimeOnPage]
	FROM [dbo].[raw_adobeReferrer3]  WHERE datestamp >= '2015-10-28';


	/** Populate all of the values found in the rel tables **/
	UPDATE w
	SET w.PlatformType = rel.PlatformType
	FROM _WebPageAnalytics w
	INNER JOIN [rel_PlatformType] rel ON (w.ElementValue = rel.OperatingSystem)
	;

	UPDATE w
	SET w.DeviceType = rel.DeviceType
	FROM _WebPageAnalytics w
	INNER JOIN [rel_DeviceType] rel ON (w.ElementValue = rel.OperatingSystem)
	;


	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_WebsitePageAnalytics_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_WebsitePageAnalytics_Value0];

	DELETE FROM _WebsitePageAnalytics WHERE datestamp >= '2015-10-28';

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage1 WHERE datestamp >= '2015-10-28'
	;

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage2 WHERE datestamp >= '2015-10-28'
	;

	INSERT INTO _WebsitePageAnalytics ([reportSuite],[datestamp],[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage])
	SELECT [reportSuite],CAST([datestamp] AS date),[PageTitle],[PageViews],[Visits],[Visitors],[UniqueVisitors],[Entries],[Exits],[Reloads],[Bots],[Bouncerate],[TimeOnPage]
	FROM raw_adobePage3 WHERE datestamp >= '2015-10-28'
	;

END

GO
