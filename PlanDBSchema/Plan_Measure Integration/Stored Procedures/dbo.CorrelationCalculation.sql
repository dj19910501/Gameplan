IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CorrelationCalculation') AND xtype IN (N'P'))
    DROP PROCEDURE CorrelationCalculation
GO


CREATE Procedure [CorrelationCalculation] (@QueryToRun varchar(max))
as
BEGIN
	--Query passed in as value must return two fields (both floats) and no nulls for every row.  Procedure will return null if data is degenerate.
	DECLARE @Retval table(d1 int, d2 int, val float) 
	BEGIN TRY
		--insert into @Retval
		exec('DECLARE @Results table(d1 int, d2 int, val1 float, val2 float); insert into @Results ' + @QueryToRun + ' 	
		
		DECLARE @mu1 table (d1 int, d2 int, val float) 

insert into @mu1 select d1, d2, avg(val1) from @Results group by d1, d2;	

DECLARE @mu2 table (d1 int, d2 int, val float)  

insert into @mu2 select d1, d2, avg(val2) from @Results group by d1, d2; 	

select r.d1, r.d2, sum((val1-m1.val)*(val2- m2.val))/sqrt(sum((val1-m1.val)*(val1-m1.val))*sum((val2-m2.val)*(val2-m2.val))) from @Results r 

inner join @mu1 m1 on r.d1=m1.d1 and r.d2=m1.d2 
inner join @mu2 m2 on r.d1=m2.d1 and r.d2=m2.d2 
group by r.d1, r.d2');
		--select d1,d2,round(val,2) val from @Retval
		return
	END TRY
	BEGIN CATCH
		select null,null,null 
		
		return 
	END CATCH
END;


GO
