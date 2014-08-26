--------------1_Internal_Review_Point.sql
-- Created by : Kalpesh Sharma 
-- Ticket : Internal Reviews Point  
-- This script will be check that ClientID field is exists or not in Role Table . 
-- if it will be not in that table at that time we have to insert that field with mirgation of Existing data.

--Check that ClientId is exists in Role Table or not.
IF NOT EXISTS (
		SELECT *
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = 'Role'
			AND COLUMN_NAME = 'ClientId'
		)
	begin
	ALTER TABLE dbo.ROLE ADD ClientId UNIQUEIDENTIFIER CONSTRAINT ClientId_fk REFERENCES dbo.Client (ClientId)
	end
go 

Declare @Application_ID UNIQUEIDENTIFIER
Declare @Client_ID UNIQUEIDENTIFIER
Set @Application_ID = (Select ApplicationId from Application where Code ='MRP')
Set @Client_ID = (Select ClientId from Client where Code = 'BLD')


IF ((SELECT count(*) FROM [ROLE] WHERE ClientId is not null) = 0)
	begin
-- Check that Role_Transform table is exists or not.
-- If table is exists at that time we have apply Cross join on Role and Client Table


IF OBJECT_ID(N'TempDB.dbo.#Role_Temp', N'U') IS NOT NULL
BEGIN
  DROP TABLE #Role_Temp
END

Create table #Role_Temp
(
  RoleId uniqueidentifier,
  Code nvarchar(25),
  Title nvarchar(255),
  Description nvarchar(4000),
 IsDeleted bit,
 CreatedDate datetime,
 CreatedBy uniqueidentifier,
 ModifiedDate DateTime,
 ModifiedBy	uniqueidentifier,
 ColorCode nchar(10),
 ClientId uniqueidentifier
)

insert into #Role_Temp
SELECT r.RoleId
		,r.Code
		,r.Title
		,r.Description
		,r.IsDeleted
		,r.CreatedDate
		,r.CreatedBy
		,r.ModifiedDate
		,r.ModifiedBy
		,r.ColorCode
		,c.ClientId
	FROM Client c
	CROSS JOIN ROLE r
	WHERE c.ClientId <>  @Client_ID

-- Update the exisiting role with Default Client ID (BullDog Client )
UPDATE ROLE
SET ClientID = @Client_ID
WHERE ClientId IS NULL

	INSERT INTO ROLE
	SELECT NEWID() RoleId
		,Code
		,Title
		,Description
		,IsDeleted
		,CreatedDate
		,CreatedBy
		,ModifiedDate
		,ModifiedBy
		,ColorCode
		,ClientId
	FROM #Role_Temp

--Insert New RoleID into the Application Role Table
INSERT INTO Application_Role
SELECT AppRole.ApplicationId
	,SecondRole.RoleId
	,SecondRole.CreatedDate
	,SecondRole.CreatedBy
	,SecondRole.ModifiedDate
	,SecondRole.ModifiedBy
	,SecondRole.IsDeleted
FROM Application_Role AppRole
INNER JOIN ROLE firstRole ON firstRole.RoleId = AppRole.RoleId
INNER JOIN ROLE SecondRole ON SecondRole.Title = firstRole.Title
WHERE AppRole.ApplicationId = @Application_ID
	AND SecondRole.ClientId <> @Client_ID AND firstRole.RoleId <> SecondRole.RoleId

--Insert New RoleID into the Application Role Table
INSERT INTO Role_Permission
SELECT SecondRole.RoleId
	,ROLEPER.MenuApplicationId
	,ROLEPER.PermissionCode
	,SecondRole.CreatedDate
	,SecondRole.CreatedBy
	,SecondRole.ModifiedDate
	,SecondRole.ModifiedBy
	,SecondRole.IsDeleted
FROM Role_Permission ROLEPER
INNER JOIN ROLE firstRole ON firstRole.RoleId = ROLEPER.RoleId
INNER JOIN ROLE SecondRole ON SecondRole.Title = firstRole.Title
WHERE SecondRole.ClientId <>  @Client_ID AND firstRole.RoleId <> SecondRole.RoleId  --(Select ClientId from Client where Code = 'BLD')


--Insert New RoleID into the Application Role Table

update User_Application
Set RoleId = NewRoles.RoleId
from (
SELECT UA.UserId,R2.RoleId
FROM User_Application UA
INNER JOIN [User] U ON U.UserId = UA.UserId
INNER JOIN Role R1 ON R1.RoleId = UA.RoleId
INNER JOIN Role R2 ON R2.Title = R1.Title AND R2.ClientId = U.ClientId
WHERE R1.RoleId <> R2.RoleId) as NewRoles
where User_Application.UserId = NewRoles.UserId

end
	else
	begin
	print('Script is already executed.')
end
 GO

--------------01_PL_688_New_Permission_Approve_Own_Tactic.sql
--Run this script on BDSAuth database

--==============================Start Script-1 (Update dbo.Application_Activity table) ===============================================
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Application_Activity')
BEGIN
	IF NOT EXISTS(SELECT 1 FROM Application_Activity where Code='TacticApproveOwn')
	BEGIN
		INSERT INTO  dbo.Application_Activity
			(ApplicationActivityId, ApplicationId, ParentId, ActivityTitle, Code,
			 CreatedDate)
				VALUES
						(21,'1c10d4b9-7931-4a7c-99e9-a158ce158951',7,'Approve Own Tactic','TacticApproveOwn', GETDATE())
	END
END
--==============================End Script-1 (Update dbo.Application_Activity table) ===============================================
