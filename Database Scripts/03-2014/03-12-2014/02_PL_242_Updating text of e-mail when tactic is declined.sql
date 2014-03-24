	Update [Notification] set [EmailContent]=REPLACE(EmailContent,'Dear User','Dear [NameToBeReplaced]') 
	where  [NotificationInternalUseOnly] In 
			(
				'CampaignApproved','CampaignDeclined','CampaignCommentAdded','CampaignSubmitted',
				'ProgramApproved','ProgramDeclined','ProgramCommentAdded','ProgramSubmitted',
				'TacticApproved','TacticDeclined','TacticCommentAdded','TacticSubmitted'
			 )

		Update [Notification] set [EmailContent]=REPLACE(EmailContent,'Gameplan Admin','Bulldog Gameplan Admin')
		where  [NotificationInternalUseOnly] In 
				(
					'CampaignApproved','CampaignDeclined','CampaignCommentAdded','CampaignSubmitted',
					'ProgramApproved','ProgramDeclined','ProgramCommentAdded','ProgramSubmitted',
					'TacticApproved','TacticDeclined','TacticCommentAdded','TacticSubmitted'
				) and EmailContent like '%<br><br>Thank You,<br>Gameplan Admin'



