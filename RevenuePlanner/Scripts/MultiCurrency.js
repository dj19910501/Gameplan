// Add By Nishant Sheth

$(document).ready(function () {
    setCurrencyDetails();
});

// Add By Nishant Sheth // Get list of plan currency details 
function setCurrencyDetails()
{
    if (GetCurrencyURL != undefined) {
        $.ajax({
            type: 'get',
            url: GetCurrencyURL,
            success: function (data) {
                if (data != null && data != '') {
                    if (data.CurrencySymbol != null && data.CurrencySymbol != '') {
                        if (CurrencySybmol != undefined) {
                            CurrencySybmol = data.CurrencySymbol;
                        }
                    }
                    if (data.ExchangeRate != null && data.ExchangeRate != '') {
                        if (PlanExchangeRate != undefined) {
                            PlanExchangeRate = data.ExchangeRate;
                        }
                    }
                }
            }
        });
    }
}