import $ from 'jquery';
import * as planPicker from 'planPicker/apiUri';

class NewLinkModel {
    _state = {
        years: undefined,
        plans: undefined,
        campaigns: undefined,
        programs: undefined,
        tactics: undefined,
        lineItems: undefined,

        selectedYear: undefined,
        selectedPlan: undefined,
        selectedCampaign: undefined,
        selectedProgram: undefined,
        selectedTactic: undefined,
    };

    _cache = {
        plans: {},
        campaigns: {},
        programs: {},
        tactics: {},
        lineItems: {},
    };

    constructor() {
        $.getJSON(planPicker.GET_YEARS).then(result => this._update("years", result));
    }

    /**
     * Subscribes to updates.  Triggered immediately with current value
     * @param which
     * @param handler
     */
    subscribe(which, handler) {
        $(this).on(which, handler);
        handler(this._makeEvent(which));
    }

    unsubscribe(which, handler) {
        $(this).off(which, handler);
    }

    _makeEvent(which) {
        return $.Event(which, {value: this._state[which]});
    }

    _update(which, data, force) {
        if (force || this._state[which] !== data) {
            this._state[which] = data;
            this._notify(which);
        }
    }

    _notify(which) {
        $(this).triggerHandler(this._makeEvent(which));
    }

    _setSelectedValue(property, value, linkedProperty, linkedDataProperty, uri, makePayload) {
        if (value !== this._state[property]) {
            this._update(property, value);
            if (linkedProperty) {
                this[linkedProperty] = undefined;
            }
            if (value === undefined) {
                // clear out the campaigns
                this._update(linkedDataProperty, undefined);
            }
            else {
                let cached = this._cache[linkedDataProperty][value];
                if (!cached) {
                    // no cached results
                    cached = this._cache[linkedDataProperty][value] = $.getJSON(uri, makePayload(value))
                        .then(result => this._cache[linkedDataProperty][value] = result);
                }
                if (cached.then) {
                    // we have a pending query we need to await
                    this._update(linkedDataProperty, undefined);
                    cached.then(result => {
                        if (this[property] === value) {
                            this._update(linkedDataProperty, result);
                        }
                    });
                }
                else {
                    // we have a cached result
                    this._update(linkedDataProperty, cached);
                }
            }
        }
    }

    get plans() { return this._state.plans; }
    get campaigns() { return this._state.campaigns; }
    get programs() { return this._state.programs; }
    get tactics() { return this._state.tactics; }
    get lineItems() { return this._state.lineItems; }

    refreshLineItems() {
        this._update("lineItems", this.lineItems, true);
    }

    get selectedYear() { return this._state.selectedYear; }

    set selectedYear(value) {
        this._setSelectedValue("selectedYear", value, "selectedPlan", "plans", planPicker.GET_PLANS, y => ({year: y}));
    }

    get selectedPlan() { return this._state.selectedPlan; }

    set selectedPlan(value) {
        this._setSelectedValue("selectedPlan", value, "selectedCampaign", "campaigns", planPicker.GET_CAMPAIGNS, p => ({planId: p}));
    }

    get selectedCampaign() { return this._state.selectedCampaign; }

    set selectedCampaign(value) {
        this._setSelectedValue("selectedCampaign", value, "selectedProgram", "programs", planPicker.GET_PROGRAMS, c => ({campaignId: c}))
    }

    get selectedProgram() { return this._state.selectedProgram; }

    set selectedProgram(value) {
        this._setSelectedValue("selectedProgram", value, "selectedTactic", "tactics", planPicker.GET_TACTICS, p => ({programId: p}));
    }

    get selectedTactic() { return this._state.selectedTactic; }

    set selectedTactic(value) {
        this._setSelectedValue("selectedTactic", value, null, "lineItems", planPicker.GET_LINE_ITEMS, t => ({tacticId: t}));
    }
}

export default function createNewLinkModel() {
    return new NewLinkModel();
}
