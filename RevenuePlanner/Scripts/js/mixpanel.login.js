$(window).load(function(){
	//Logins
	var loginForm = $('#frmLogin');
	loginForm.submit(function(){
        mixpanel.track("Logged In");
		var mixpanelID = $('#UserEmail').val();
		//Cookies
		$.cookie('email', mixpanelID, { expires: 14 });
		//
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