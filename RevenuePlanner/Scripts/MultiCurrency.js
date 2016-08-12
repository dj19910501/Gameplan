﻿// Add By Nishant Sheth

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