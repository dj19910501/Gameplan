/* To be executed on BDS Authentication database*/

Update Role_Permission set PermissionCode = 0 where 
RoleId In (select RoleId from Role where Code IN ('SA', 'CA', 'D')) and
MenuApplicationId in (select MenuApplicationId from Menu_Application where Code = 'BOOST' and 
ApplicationId In (select ApplicationId from [Application] where Code = 'MRP'))
