--01_PL_159_UI_for_Best_in_class_data_entry
update BestInClass set Value=Value*100 from BestInClass
inner join metric on BestInClass .MetricId = metric.MetricId
where MetricType='CR' and Value > 0 and Value <= 1