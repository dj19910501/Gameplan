//<!-- MixPanel Tracking Events - Login Page -->

$(window).load(function(){
	//Logins
	var loginForm = $('#frmLogin');
	loginForm.submit(function(){
        mixpanel.track("Logged In");
		var mixpanelID = $('#UserEmail').val();
        //Identify Users
        mixpanel.identify(mixpanelID);
		mixpanel.people.set({
			"$email": mixpanelID
		});
		mixpanel.people.set_once({
    		'Last Login Date': new Date(),
			'# of Logins': 1
		});
		mixpanel.people.increment("# of Logins");
	});
});
