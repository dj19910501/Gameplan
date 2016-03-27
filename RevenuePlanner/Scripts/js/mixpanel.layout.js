$(window).load(function(){
	//Identify via Cookie
	var mixpanelID = $.cookie('email');
	mixpanel.identify(mixpanelID);
	//
	var currentURL = window.location.href;
	mixpanel.track("Page Load",
		{
			"Status" : "Logged In",
		});
	//Nav Usage
	$('.main-nav li a').on('click', function(){
	var navLabel = $(this).text();
		mixpanel.track("Used Nav: " + navLabel);
	});
	//Add to User Profile
	var mixFirstName = $('.username p').html().trim().split(' ').slice(0, -1);
	var mixLastName = $('.username p').html().trim().split(' ').slice(-1);
	mixpanel.people.set({
		"$first_name": mixFirstName,
		"$last_name": mixLastName
	});
	//Filter Usage
	$('.accordion-heading h2').on('click', function(){
	var filterLabel = $(this).text();
		mixpanel.track("Used Filter: " + filterLabel);
	});
	//Grid View
	$('#liGrid').on('click', function(){
		mixpanel.track("Used Grid View");
	});
	//Org Name
	var mixOrg = $('#header span.title').text();
	mixpanel.people.set({
   		'Organization': mixOrg,
	});
	//Success Text
	$('.market-activity-main').on('click', 'li.new-prog', function(){
		$('#SuccessMsg').hide();
		setTimeout(trackSuccess1, 7000);
	});
	var trackSuccess1 = function(){
		var successText1 = $('#spanMsgSuccess').text().trim();
  		mixpanel.track(successText1 + " | Calendar");
	};
});