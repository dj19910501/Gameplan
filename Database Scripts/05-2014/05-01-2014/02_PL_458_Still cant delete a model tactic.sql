
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE Name = N'IsDeleted' AND OBJECT_ID = OBJECT_ID(N'TacticType'))
BEGIN
    ALTER TABLE TacticType ADD IsDeleted bit
END
