// Add By Nishant Sheth
// Declare Global variables for currency symbols and plan currencies rates etc.
var CurrencySybmol = '$';
var PlanExchangeRate = 1;

$(document).ready(function () {
    setCurrencyDetails();
});

// Add By Nishant Sheth // Get list of plan currency details 
function setCurrencyDetails()
{
    $.ajax({
        type: 'get',
        url: '/Currency/GetPlanCurrencyDetail',
        success: function (data) {
            if (data != null && data != '') {
                if (data.CurrencySymbol != null && data.CurrencySymbol != '') {
                    CurrencySybmol = data.CurrencySymbol;
                }
                if (data.ExchangeRate != null && data.ExchangeRate != '') {
                    PlanExchangeRate = data.ExchangeRate;
                }
            }
        }
    });
}