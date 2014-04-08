if not exists(select * from sys.columns where Name = N'AllowedTargetStage' and Object_ID = Object_ID(N'Model_Funnel_Stage'))
begin
             ALTER TABLE [Model_Funnel_Stage] ADD [AllowedTargetStage] BIT NOT NULL  DEFAULT(0)
end
Go
/*set AllowedTargetStage true which Model stage are used in Tactic_Type table */
Update Model_Funnel_Stage set AllowedTargetStage=1 
 where ModelFunnelId in
 (
	select ModelFunnelId from Model_Funnel where ModelId In 
	(
		select ModelId from TacticType where StageId Is Not NuLL
	)
 ) and StageType='CR'
Go 
/****** Update Description column of  Stage table by client wise and stage code.  ******/
Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='464EB808-AD1F-4481-9365-6AADA15023BD'

Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='092F54DF-4C71-4F2F-9D21-0AE16155E5C1'

Update Stage set Description='Outbound Inquiries' where  Code='SUS' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Inbound Inquiries' where  Code='INQ' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Automation Qualified lead' where Code='AQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Teleprospecting Accepted Lead' where Code='TAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Traditional MQL or Marketing Qualified Lead' where Code='TQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Sales Accepted Lead' where  Code='SAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Sales Qualified Lead' where Code='SQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Closed Won' where  Code='CW' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Marketing Qualified Lead' where  Code='MQL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'
Update Stage set Description='Marketing Accepted Lead' where  Code='MAL' and ClientId='C251AB18-0683-4D1D-9F1E-06709D59FD53'





