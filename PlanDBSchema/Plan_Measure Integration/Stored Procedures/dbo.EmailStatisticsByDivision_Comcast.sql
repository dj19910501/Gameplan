
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'EmailStatisticsByDivision_Comcast') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE EmailStatisticsByDivision_Comcast
END
GO

CREATE PROCEDURE EmailStatisticsByDivision_Comcast
AS
BEGIN

IF OBJECT_ID('EmailStatisticsByDivision_Comcast_Table', 'U') IS NOT NULL 
  DROP TABLE EmailStatisticsByDivision_Comcast_Table; 

CREATE TABLE EmailStatisticsByDivision_Comcast_Table 
(AssetName NVARCHAR(255),
Division NVARCHAR(255),
Sends INT,
OpenRate FLOAT,
ClickThruRate FLOAT,
BounceRate FLOAT,
UnsubscribeRate FLOAT)

INSERT INTO EmailStatisticsByDivision_Comcast_Table
SELECT ea.assetName
	, ISNULL(ec.Division, '(blank)') AS Division
	, COUNT(*) AS Sends
	, CAST(SUM(CAST(ea.isOpened AS INT)) AS FLOAT)/COUNT(*) AS OpenRate
	, CAST(SUM(CAST(ea.isClickThrough AS INT)) AS float)/CASE WHEN SUM(CAST(isOpened AS INT)) = 0 THEN 0.000000001 ELSE SUM(CAST(isOpened AS INT)) END AS ClickThruRate
	, CAST(SUM(CAST(ea.isBounceback AS INT)) AS float)/COUNT(*) AS BounceRate
	, CAST(SUM(CAST(ea.isunsubscribed AS INT)) AS float)/COUNT(*) AS UnsubscribeRate
FROM _Emails_View d1
INNER JOIN _EmailActivity ea ON (d1.EmailName = ea.AssetName)
LEFT JOIN _Eloqua_Contacts ec ON (ea.EmailAddress = ec.EmailAddress)
GROUP BY ea.assetName, ec.Division

END