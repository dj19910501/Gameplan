

UPDATE Notification SET EmailContent = REPLACE(EmailContent,'apporved','approved') WHERE NotificationInternalUseOnly = 'TacticApproved' 
																		
