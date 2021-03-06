IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'normsinv') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION normsinv
GO

CREATE FUNCTION [normsinv](@p FLOAT)
RETURNS FLOAT
AS
BEGIN
DECLARE @a1 FLOAT = -39.6968302866538
DECLARE @a2 FLOAT = 220.946098424521
DECLARE @a3 FLOAT = -275.928510446969
DECLARE @a4 FLOAT = 138.357751867269
DECLARE @a5 FLOAT = -30.6647980661472
DECLARE @a6 FLOAT = 2.50662827745924
DECLARE @b1 FLOAT = -54.4760987982241
DECLARE @b2 FLOAT = 161.585836858041
DECLARE @b3 FLOAT = -155.698979859887
DECLARE @b4 FLOAT = 66.8013118877197
DECLARE @b5 FLOAT = -13.2806815528857
DECLARE @c1 FLOAT = -0.00778489400243029
DECLARE @c2 FLOAT = -0.322396458041136
DECLARE @c3 FLOAT = -2.40075827716184
DECLARE @c4 FLOAT = -2.54973253934373
DECLARE @c5 FLOAT = 4.37466414146497
DECLARE @c6 FLOAT = 2.93816398269878
DECLARE @d1 FLOAT = 0.00778469570904146
DECLARE @d2 FLOAT = 0.32246712907004
DECLARE @d3 FLOAT = 2.445134137143
DECLARE @d4 FLOAT = 3.75440866190742
DECLARE @plow FLOAT = 0.02425
DECLARE @phigh FLOAT = 1-@plow
DECLARE @q FLOAT
DECLARE @r FLOAT
DECLARE @result FLOAT
IF (@p<@plow)
BEGIN
SET @q = Sqrt(-2 * LOG(@p))
SET @result=(((((@c1 * @q + @c2) * @q + @c3) * @q + @c4) * @q + @c5) * @q + @c6) / ((((@d1 * @q + @d2) * @q + @d3) * @q + @d4) * @q + 1)
END
ELSE
BEGIN
IF (@p<@phigh)
BEGIN
SET @q =@p - 0.5
SET @r = @q * @q
SET @result= (((((@a1 * @r + @a2) * @r + @a3) * @r + @a4) * @r + @a5) * @r + @a6) * @q / (((((@b1 * @r + @b2) * @r + @b3) * @r + @b4) * @r + @b5) * @r + 1)
END
ELSE
BEGIN
SET @q = SQRT(-2 * LOG(1 - @p))
SET @result= -(((((@c1 * @q + @c2) * @q + @c3) * @q + @c4) * @q + @c5) * @q + @c6) / ((((@d1 * @q + @d2) * @q + @d3) * @q + @d4) * @q + 1)
END
END
RETURN @result
END

GO
