GO
--Added By Preet Shah on 22/12/2016. For Save Marketing Budget Column Attribute
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'User_CoulmnView' AND COLUMN_NAME = 'MarketingBudgetAttribute')
BEGIN

    ALTER TABLE User_CoulmnView ADD [MarketingBudgetAttribute] [xml] NULL 
   
END

GO
--Replace thsi line with your script 

----UPDATE DB SCHEMA VERSION FOR THE RELEASE 
declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'December.2016'
set @version = 'December.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END



GO

