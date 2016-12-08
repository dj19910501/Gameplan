import resolveAppUri from 'util/resolveAppUri';

function constructUri(endpoint) {
    return resolveAppUri(`api/PlanPicker/${endpoint}`);
}

export const GET_YEARS = constructUri("GetYears");
export const GET_PLANS = constructUri("GetPlans");
export const GET_CAMPAIGNS = constructUri("GetCampaigns");
export const GET_PROGRAMS = constructUri("GetPrograms");
export const GET_TACTICS = constructUri("GetTactics");
export const GET_LINE_ITEMS = constructUri("GetLineItems");
