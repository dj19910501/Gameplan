/* --------- Start Script of PL ticket #1833 --------- */
-- Created by : Brad Gray
-- Created On : 12/20/2015
-- Description : Update API URL for WorkFront to v4.0

  update [dbo].[IntegrationType] set APIURL = '.attask-ondemand.com/attask/api/v4.0' where APIURL = '.attask-ondemand.com/attask/api'
  update [dbo].[IntegrationType] set APIVersion = '4.0' where APIURL = '.attask-ondemand.com/attask/api/v4.0'