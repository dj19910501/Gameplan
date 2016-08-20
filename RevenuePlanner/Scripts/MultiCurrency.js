// Add By Nishant Sheth

$(document).ready(function () {
    setCurrencyDetails();
});

// Add By Nishant Sheth // Get list of plan currency details 
function setCurrencyDetails()
{
    if (typeof GetCurrencyURL != undefined) {
        $.ajax({
            type: 'get',
            url: GetCurrencyURL,
            success: function (data) {
                if (data != null && data != '') {
                    if (data.CurrencySymbol != null && data.CurrencySymbol != '') {
                        if (typeof CurrencySybmol != undefined) {
                            CurrencySybmol = data.CurrencySymbol;
                        }
                    }
                    if (data.ExchangeRate != null && data.ExchangeRate != '') {
                        if (typeof PlanExchangeRate != undefined) {
                            PlanExchangeRate = data.ExchangeRate;
                        }
                    }
                }
            }
        });
    }
}
//Added by Rahul Shah to Apply multicurrency on client side
function GetValueByExchangeRate(value) {
    value = value.toString().replace(",", '');
    value = parseFloat(value);
    if (value > 0) {        
        value = value * PlanExchangeRate;
        value = Math.round(value * Math.pow(10, 2)) / Math.pow(10, 2);
    }
    return value;   
}

function SetValueByExchangeRate(value) {
    value = value.toString().replace(",", '');
    value = parseFloat(value);
    if (value > 0) {
        value = value / PlanExchangeRate;
        value = Math.round(value * Math.pow(10, 2)) / Math.pow(10, 2);
    }
    return value;
}