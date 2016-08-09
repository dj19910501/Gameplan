//<!-- MixPanel Tracking Events - Inspection Window -->

	//Inspection Actions
$('#EditForm').on('click', function () {
    mixpanel.track("Save & Close button clicked under Tactic inspection window."); // log Save & Close button click event to mixpanel. Added by Viral regarding PL ticket #2434.
		setTimeout(trackInspectMsg, 100);
	});
	var trackInspectMsg = function(){
		var successText2 = $('#spanMessageSuccess').text().trim();
		var errorText2 = $('#spanMessageError').text().trim();
		if ($("#successMessage").css('display') == 'block'){
			mixpanel.track(successText2 + " | Inspect");
		} else if ($("#errorMessage").css('display') == 'block') {
			mixpanel.track(errorText2 + " | Inspect");
		}
	};
	$('#deleteTactic').on('click', function(){
		mixpanel.track("Hit Delete");
	});
