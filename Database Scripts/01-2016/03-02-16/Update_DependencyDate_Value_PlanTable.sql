/* --------- Start Script of PL ticket #1943 --------- */
-- Created by : Komal Rawal
-- Created On : 2/3/2016
-- Description : Update dependency Date for Plans of a particular client.
-- Note: Parameters  @ClientId and @DependencyDate needs to be changed as per requirement
Declare @ClientId uniqueidentifier = 'C251AB18-0683-4D1D-9F1E-06709D59FD53'
Declare @DependencyDate datetime = '2016-02-04 00:00:00.000'
Update [Plan] set DependencyDate = @DependencyDate where PlanId In (Select PlanId from [Plan] where ModelId IN ( Select ModelId from [Model] where ClientId = @ClientId and IsDeleted='0') and IsDeleted='0')

/* --------- End Script of PL ticket #1943--------- */