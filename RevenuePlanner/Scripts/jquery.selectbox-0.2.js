(function ($, undefined) {
    var PROP_NAME = 'selectbox',
        FALSE = false,
        TRUE = true,
        selectbox_keys = "",
        selectbox_keys_timeout = 0;
    /**
     * Selectbox manager.
     * Use the singleton instance of this class, $.selectbox, to interact with the select box.
     * Settings for (groups of) select boxes are maintained in an instance object,
     * allowing multiple different settings on the same page
     */
    function Selectbox() {
        this._state = [];
        this._defaults = { // Global defaults for all the select box instances
            classHolder: "sbHolder",
            classHolderDisabled: "sbHolderDisabled",
            classSelector: "sbSelector",
            classOptions: "sbOptions",
            classGroup: "sbGroup",
            classSub: "sbSub",
            classDisabled: "sbDisabled",
            classToggleOpen: "sbToggleOpen",
            classToggle: "sbToggle",
            classFocus: "sbFocus",
            speed: 200,
            effect: "slide", // "slide" or "fade"
            onChange: null, //Define a callback function when the selectbox is changed
            onOpen: null, //Define a callback function when the selectbox is open
            onClose: null //Define a callback function when the selectbox is closed
        };
    }

    $.extend(Selectbox.prototype, {
        /**
         * Is the first field in a jQuery collection open as a selectbox
         * 
         * @param {Object} target
         * @return {Boolean}
         */
        _isOpenSelectbox: function (target) {
            if (!target) {
                return FALSE;
            }
            var inst = this._getInst(target);
            return inst.isOpen;
        },
        /**
         * Is the first field in a jQuery collection disabled as a selectbox
         * 
         * @param {HTMLElement} target
         * @return {Boolean}
         */
        _isDisabledSelectbox: function (target) {
            if (!target) {
                return FALSE;
            }
            var inst = this._getInst(target);
            return inst.isDisabled;
        },
        /**
         * Attach the select box to a jQuery selection.
         * 
         * @param {HTMLElement} target
         * @param {Object} settings
         */
        _attachSelectbox: function (target, settings) {
            if (this._getInst(target)) {
                return FALSE;
            }
            var $target = $(target),
                self = this,
                inst = self._newInst($target),
                sbHolder, sbSelector, sbToggle, sbOptions,
                s = FALSE, optGroup = $target.find("optgroup"), opts = $target.find("option"), olen = opts.length;

            $target.attr("sb", inst.uid);

            $.extend(inst.settings, self._defaults, settings);
            self._state[inst.uid] = FALSE;
            $target.hide();

            function closeOthers() {
                var key, sel,
                    uid = this.attr("id").split("_")[1];
                for (key in self._state) {
                    if (key !== uid) {
                        if (self._state.hasOwnProperty(key)) {
                            sel = $("select[sb='" + key + "']")[0];
                            if (sel) {
                                self._closeSelectbox(sel);
                            }
                        }
                    }
                }
            }

            sbHolder = $("<div>", {
                "id": "sbHolder_" + inst.uid,
                "class": inst.settings.classHolder,
                "tabindex": $target.attr("tabindex")
            });

            sbSelector = $("<a>", {
                "id": "sbSelector_" + inst.uid,
                "href": "#",
                "class": inst.settings.classSelector,
                "click": function (e) {
                    e.preventDefault();
                    closeOthers.apply($(this), []);
                    var uid = $(this).attr("id").split("_")[1];
                    if (self._state[uid]) {
                        self._closeSelectbox(target);
                    } else {
                        self._openSelectbox(target);
                        $(this).focus();
                    }
                }
            });

            sbToggle = $("<a>", {
                "id": "sbToggle_" + inst.uid,
                "href": "#",
                "class": inst.settings.classToggle,
                "click": function (e) {
                    e.preventDefault();
                    closeOthers.apply($(this), []);
                    var uid = $(this).attr("id").split("_")[1];
                    if (self._state[uid]) {
                        self._closeSelectbox(target);
                    } else {
                        self._openSelectbox(target);
                        $(this).next().focus();
                    }
                }
            });
            sbToggle.appendTo(sbHolder);

            sbOptions = $("<ul>", {
                "id": "sbOptions_" + inst.uid,
                "class": inst.settings.classOptions,
                "css": {
                    "display": "none"
                }
            });

            $target.children().each(function (i) {
                var that = $(this), li, config = {};
                if (that.is("option")) {
                    getOptions(that);
                } else if (that.is("optgroup")) {
                    li = $("<li>");
                    $("<span>", {
                        "text": that.attr("label")
                    }).addClass(inst.settings.classGroup).appendTo(li);
                    li.appendTo(sbOptions);
                    if (that.is(":disabled")) {
                        config.disabled = true;
                    }
                    config.sub = true;
                    getOptions(that.find("option"), config);
                }
            });

            function getOptions() {
                var sub = arguments[1] && arguments[1].sub ? true : false,
                    disabled = arguments[1] && arguments[1].disabled ? true : false;
                arguments[0].each(function (i) {
                    var that = $(this),
                        li = $("<li>"),
                        child;
                    if (that.is(":selected")) {
                        sbSelector.text(that.text());
                        s = TRUE;
                    }
                    if (i === olen - 1) {
                        li.addClass("last");
                    }
                    if (!that.is(":disabled") && !disabled) {
                        child = $("<a>", {
                            "href": "#" + that.val(),
                            "rel": that.val()
                        }).text(that.text()).bind("click.sb", function (e) {
                            if (e && e.preventDefault) {
                                e.preventDefault();
                            }
                            var t = sbToggle,
                                $this = $(this),
                                uid = t.attr("id").split("_")[1];
                            self._changeSelectbox(target, $this.attr("rel"), $this.text());
                            self._closeSelectbox(target);
                        }).bind("mouseover.sb", function () {
                            var $this = $(this);
                            $this.parent().siblings().find("a").removeClass(inst.settings.classFocus);
                            $this.addClass(inst.settings.classFocus);
                        }).bind("mouseout.sb", function () {
                            $(this).removeClass(inst.settings.classFocus);
                        });
                        if (sub) {
                            child.addClass(inst.settings.classSub);
                        }
                        if (that.is(":selected")) {
                            child.addClass(inst.settings.classFocus);
                        }
                        child.appendTo(li);
                    } else {
                        child = $("<span>", {
                            "text": that.text()
                        }).addClass(inst.settings.classDisabled);
                        if (sub) {
                            child.addClass(inst.settings.classSub);
                        }
                        child.appendTo(li);
                    }
                    li.appendTo(sbOptions);
                });
            }

            if (!s) {
                sbSelector.text(opts.first().text());
            }

            $.data(target, PROP_NAME, inst);
            
            //sbHolder.data("uid", inst.uid).bind("keydown.sb", function (e) {
            //sbHolder.data("uid", inst.uid).keydown(function (e) { alert('down'); });
            //sbHolder.data("uid", inst.uid).keyup(function (e) { alert('up'); });
            var $prevtext = '';
            var $nextdata = -1;
            var $_prevdata = -1;
            sbHolder.data("uid", inst.uid).keypress(function (e) {
                var key = e.charCode ? e.charCode : e.keyCode ? e.keyCode : 0,
                    $this = $(this),
                    uid = $this.data("uid"),
                    inst = $this.siblings("select[sb='" + uid + "']").data(PROP_NAME),
                    trgt = $this.siblings(["select[sb='", uid, "']"].join("")).get(0),
                    $f = $this.find("ul").find("a." + inst.settings.classFocus);
                switch (key) {
                    case 37: //Arrow Left
                    case 38: //Arrow Up
                        if ($f.length > 0) {
                            var $next;
                            $("a", $this).removeClass(inst.settings.classFocus);
                            $next = $f.parent().prevAll("li:has(a)").eq(0).find("a");
                            if ($next.length > 0) {
                                $next.addClass(inst.settings.classFocus).focus();
                                $("#sbSelector_" + uid).text($next.text());
                            }
                        }
                        break;
                    case 39: //Arrow Right
                    case 40: //Arrow Down
                        var $next;
                        $("a", $this).removeClass(inst.settings.classFocus);
                        if ($f.length > 0) {
                            $next = $f.parent().nextAll("li:has(a)").eq(0).find("a");
                        } else {
                            $next = $this.find("ul").find("a").eq(0);
                        }
                        if ($next.length > 0) {
                            $next.addClass(inst.settings.classFocus).focus();
                            $("#sbSelector_" + uid).text($next.text());
                        }
                        break;
                    case 13: //Enter
                        if ($f.length > 0) {
                            self._changeSelectbox(trgt, $f.attr("rel"), $f.text());
                        }
                        self._closeSelectbox(trgt);
                        break;
                    case 9: //Tab
                        if (trgt) {
                            var inst = self._getInst(trgt);
                            if (inst/* && inst.isOpen*/) {
                                if ($f.length > 0) {
                                    self._changeSelectbox(trgt, $f.attr("rel"), $f.text());
                                }
                                self._closeSelectbox(trgt);
                            }
                        }
                        var i = parseInt($this.attr("tabindex"), 10);
                        if (!e.shiftKey) {
                            i++;
                        } else {
                            i--;
                        }
                        $("*[tabindex='" + i + "']").focus();
                        break;
                    case 27: //Escape
                        self._closeSelectbox(trgt);
                        break;
                    default:
                        var theChar = String.fromCharCode(key);
                        var cur_check = '';
                        cur_check = selectbox_keys + theChar.toLowerCase();
                        $this.find("ul").find("a").each(function () {
                            var text = jQuery(this).text().toLowerCase();
                            if ($prevtext == '' || cur_check.length > 1) {
                                if (text.indexOf(cur_check) == 0) {
                                    $prevtext = text;
                                    if ($_prevdata != null && $_prevdata != 'undefined' && $_prevdata != -1) {
                                        $($_prevdata).parent().removeClass('setFocusli');
                                    }
                                    $_prevdata = jQuery(this);
                                    $nextdata = jQuery(this);
                                    $nextdata.parent().addClass('setFocusli');
                                    selectbox_keys = cur_check;
                                    return false;
                                }
                            }
                            else {
                                /// find search_text from current li to downward direction in dropdown list.
                                var _nextdatas = '';
                                //_nextdatas = $('.setFocusli').nextAll("li");    // get next all "li" list from current "li". //Commented by Rahul shah on 18/11/2015 for PL#1742.
                                _nextdatas = $this.find('.setFocusli').nextAll("li");    //added by Rahul Shah on 17/11/2015 for PL #1742 to get next all "li" list from current "DropDown".
                                var cntr = 0; //// counter to check that any "li" exist or not that has search_text. if it's 0 then compare search_text from start of the dropdown list.
                                $.each(_nextdatas, function (key, value) {
                                    var _anchor = $(value).find('a');
                                    var _atext = $(_anchor).text().toLowerCase();

                                    //// Compare current "li" anchor text is with previous selected anchor text.
                                    if (_atext != $prevtext && _atext.indexOf(cur_check) == 0) {
                                        $prevtext = _atext; // set previous text variable with selected text.
                                        $($_prevdata).parent().removeClass('setFocusli');   // remove class 'setFocusli' from previously selected list object. 
                                        $_prevdata = _anchor;
                                        $nextdata = _anchor;
                                        $($nextdata).parent().addClass('setFocusli');   // add class 'setFocusli' to currently selected list object. 
                                        selectbox_keys = cur_check;
                                        cntr = cntr + 1;
                                        return false;
                                    }
                                });

                                /// find search_text from start of the dropdown list.
                                if (cntr == 0) {
                                    _nextdatas = $(this).parent().nextAll("li");
                                    $.each(_nextdatas, function (key, value) {
                                        var _anchor = $(value).find('a');
                                        var _atext = $(_anchor).text().toLowerCase();

                                        //// Compare current "li" anchor text is with previous selected anchor text.
                                        if (_atext != $prevtext && _atext.indexOf(cur_check) == 0) {
                                            $prevtext = _atext;
                                            $($_prevdata).parent().removeClass('setFocusli');   // remove class 'setFocusli' from previously selected list object.
                                            $_prevdata = _anchor;
                                            $nextdata = _anchor;
                                            $($nextdata).parent().addClass('setFocusli');   // add class 'setFocusli' to currently selected list object. 
                                            selectbox_keys = cur_check;
                                            return false;
                                        }
                                    });
                                }
                                if (cur_check.length <= 1) {
                                    return false;
                                }
                            }
                        });
                        if ($nextdata != -1) {
                            $("a", $this).removeClass(inst.settings.classFocus);
                            $nextdata.addClass(inst.settings.classFocus).focus();
                            $("#sbSelector_" + uid).text($nextdata.text());
                        }
                        clearTimeout(selectbox_keys_timeout);
                        var selectbox_keys_timeout = setTimeout(function () {
                            selectbox_keys = "";
                        }, 400);
                        break;
                }
                e.stopPropagation();
                return false;
            }).delegate("a", "mouseover", function (e) {
                $(this).addClass(inst.settings.classFocus);
            }).delegate("a", "mouseout", function (e) {
                $(this).removeClass(inst.settings.classFocus);
            });

            sbSelector.appendTo(sbHolder);
            sbOptions.appendTo(sbHolder);
            sbHolder.insertAfter($target);

            $("html").on('mousedown', function (e) {
                ////Modified by Mitesh Vaishnav for budgeting screen in report
                ////jquery.multiselect and jquery.selectbox conflicted with each other because of it multiselect box not closed when clicking outside the screen
                //e.stopPropagation();
                $("select").selectbox('close');
            });
            $([".", inst.settings.classHolder, ", .", inst.settings.classSelector].join("")).mousedown(function (e) {
                e.stopPropagation();
            });
        },
        /**
         * Remove the selectbox functionality completely. This will return the element back to its pre-init state.
         * 
         * @param {HTMLElement} target
         */
        _detachSelectbox: function (target) {
            var inst = this._getInst(target);
            if (!inst) {
                return FALSE;
            }
            $("#sbHolder_" + inst.uid).remove();
            $.data(target, PROP_NAME, null);
            $(target).show();
        },
        /**
         * Change selected attribute of the selectbox.
         * 
         * @param {HTMLElement} target
         * @param {String} value
         * @param {String} text
         */
        _changeSelectbox: function (target, value, text) {
            var onChange,
                inst = this._getInst(target);
            if (inst) {
                onChange = this._get(inst, 'onChange');
                $("#sbSelector_" + inst.uid).text(text);
            }
            value = value.replace(/\'/g, "\\'");
            $(target).find("option[value='" + value + "']").attr("selected", TRUE);
            if (inst && onChange) {
                onChange.apply((inst.input ? inst.input[0] : null), [value, inst]);
            } else if (inst && inst.input) {
                inst.input.trigger('change');
            }
        },
        /**
         * Enable the selectbox.
         * 
         * @param {HTMLElement} target
         */
        _enableSelectbox: function (target) {
            var inst = this._getInst(target);
            if (!inst || !inst.isDisabled) {
                return FALSE;
            }
            $("#sbHolder_" + inst.uid).removeClass(inst.settings.classHolderDisabled);
            inst.isDisabled = FALSE;
            $.data(target, PROP_NAME, inst);
        },
        /**
         * Disable the selectbox.
         * 
         * @param {HTMLElement} target
         */
        _disableSelectbox: function (target) {
            var inst = this._getInst(target);
            if (!inst || inst.isDisabled) {
                return FALSE;
            }
            $("#sbHolder_" + inst.uid).addClass(inst.settings.classHolderDisabled);
            inst.isDisabled = TRUE;
            $.data(target, PROP_NAME, inst);
        },
        /**
         * Get or set any selectbox option. If no value is specified, will act as a getter.
         * 
         * @param {HTMLElement} target
         * @param {String} name
         * @param {Object} value
         */
        _optionSelectbox: function (target, name, value) {
            var inst = this._getInst(target);
            if (!inst) {
                return FALSE;
            }
            //TODO check name
            inst[name] = value;
            $.data(target, PROP_NAME, inst);
        },
        /**
         * Call up attached selectbox
         * 
         * @param {HTMLElement} target
         */
        _openSelectbox: function (target) {
            var inst = this._getInst(target);
            //if (!inst || this._state[inst.uid] || inst.isDisabled) {
            if (!inst || inst.isOpen || inst.isDisabled) {
                return;
            }
            var el = $("#sbOptions_" + inst.uid),
                viewportHeight = parseInt($(window).height(), 10),
                offset = $("#sbHolder_" + inst.uid).offset(),
                scrollTop = $(window).scrollTop(),
                height = el.prev().height(),
                diff = viewportHeight - (offset.top - scrollTop) - height / 2 - 15,
                onOpen = this._get(inst, 'onOpen');
            el.css({
                "top": height + "px",
                "maxHeight": (diff - height)>100? (diff - height)+ "px":"100px"
            });
            inst.settings.effect === "fade" ? el.fadeIn(inst.settings.speed) : el.slideDown(inst.settings.speed);
            $("#sbHolder_" + inst.uid).addClass(inst.settings.classToggleOpen);
            this._state[inst.uid] = TRUE;
            inst.isOpen = TRUE;
            if (onOpen) {
                onOpen.apply((inst.input ? inst.input[0] : null), [inst]);
            }
            $.data(target, PROP_NAME, inst);
        },
        /**
         * Close opened selectbox
         * 
         * @param {HTMLElement} target
         */
        _closeSelectbox: function (target) {
            var inst = this._getInst(target);
            //if (!inst || !this._state[inst.uid]) {
            if (!inst || !inst.isOpen) {
                return;
            }
            var onClose = this._get(inst, 'onClose');
            inst.settings.effect === "fade" ? $("#sbOptions_" + inst.uid).fadeOut(inst.settings.speed) : $("#sbOptions_" + inst.uid).slideUp(inst.settings.speed);
            $("#sbHolder_" + inst.uid).removeClass(inst.settings.classToggleOpen);
            this._state[inst.uid] = FALSE;
            inst.isOpen = FALSE;
            if (onClose) {
                onClose.apply((inst.input ? inst.input[0] : null), [inst]);
            }
            $.data(target, PROP_NAME, inst);
        },
        /**
         * Create a new instance object
         * 
         * @param {HTMLElement} target
         * @return {Object}
         */
        _newInst: function (target) {
            var id = target[0].id.replace(/([^A-Za-z0-9_-])/g, '\\\\$1');
            return {
                id: id,
                input: target,
                uid: Math.floor(Math.random() * 99999999),
                isOpen: FALSE,
                isDisabled: FALSE,
                settings: {}
            };
        },
        /**
         * Retrieve the instance data for the target control.
         * 
         * @param {HTMLElement} target
         * @return {Object} - the associated instance data
         * @throws error if a jQuery problem getting data
         */
        _getInst: function (target) {
            try {
                return $.data(target, PROP_NAME);
            }
            catch (err) {
                throw 'Missing instance data for this selectbox';
            }
        },
        /**
         * Get a setting value, defaulting if necessary
         * 
         * @param {Object} inst
         * @param {String} name
         * @return {Mixed}
         */
        _get: function (inst, name) {
            return inst.settings[name] !== undefined ? inst.settings[name] : this._defaults[name];
        }
    });

    /**
     * Invoke the selectbox functionality.
     * 
     * @param {Object|String} options
     * @return {Object}
     */
    $.fn.selectbox = function (options) {

        var otherArgs = Array.prototype.slice.call(arguments, 1);
        if (typeof options == 'string' && options == 'isDisabled') {
            return $.selectbox['_' + options + 'Selectbox'].apply($.selectbox, [this[0]].concat(otherArgs));
        }

        if (options == 'option' && arguments.length == 2 && typeof arguments[1] == 'string') {
            return $.selectbox['_' + options + 'Selectbox'].apply($.selectbox, [this[0]].concat(otherArgs));
        }

        return this.each(function () {
            typeof options == 'string' ?
                $.selectbox['_' + options + 'Selectbox'].apply($.selectbox, [this].concat(otherArgs)) :
                $.selectbox._attachSelectbox(this, options);
        });
    };

    $.selectbox = new Selectbox(); // singleton instance
    $.selectbox.version = "0.2";
})(jQuery);