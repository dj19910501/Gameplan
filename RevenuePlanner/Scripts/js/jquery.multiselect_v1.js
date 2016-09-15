/* jshint forin:true, noarg:true, noempty:true, eqeqeq:true, boss:true, undef:true, curly:true, browser:true, jquery:true */
/*
 * jQuery MultiSelect UI Widget 1.14pre
 * Copyright (c) 2012 Eric Hynds
 *
 * http://www.erichynds.com/jquery/jquery-ui-multiselect-widget/
 *
 * Depends:
 *   - jQuery 1.4.2+
 *   - jQuery UI 1.8 widget factory
 *
 * Optional:
 *   - jQuery UI effects
 *   - jQuery UI position utility
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 *
 */
var $prevtext = '';
var $nextdata = -1;
var $_prevdata = -1;
var selectbox_keys = '';
(function ($, undefined) {
    var multiselectID = 0;
    var $doc = $(document);
    $.widget("ech.multiselect", {

        // default options
        options: {
            header: true,
            height: 175,
            minWidth: 225,
            classes: '',
            checkAllText: 'Select All',
            uncheckAllText: 'Deselect All',
            //noneSelectedText: 'Select Options',//Modified by Mitesh for PL ticket #727
            //selectedText: '# selected',
            selectedList: 0,
            show: null,
            hide: null,
            autoOpen: false,
            multiple: true,
            position: {},
            appendTo: "body",
            ////Added by Mitesh Vaishnav - Additional perameter to identify customfields Ids
            CustomCampaignName: "",
            CustomLineItemName: "",
            CustomProgramName: "",
            CustomTacticName: "",
            CustomColumnGroupName:"",
            CustomName: "",
            IsTacticSection: ""
        },

        _create: function () {
        
            var el = this.element.hide();
            var o = this.options;
            var id = this.element.attr('id');////Added by Mitesh Vaishnav for PL ticket #727
            this.speed = $.fx.speeds._default; // default speed for effects
            this._isOpen = false; // assume no

            // create a unique namespace for events that the widget
            // factory cannot unbind automatically. Use eventNamespace if on
            // jQuery UI 1.9+, and otherwise fallback to a custom string.
            this._namespaceID = this.eventNamespace || ('multiselect' + multiselectID);

            var button = (this.button = $('<button type="button" id="btnMultiselect_' + id + '"><span class="ui-icon ui-icon-triangle-1-s"></span></button>'))
              .addClass('ui-multiselect ui-widget ui-state-default ui-corner-all')
              .addClass(o.classes)
              .attr({ 'title': el.attr('title'), 'aria-haspopup': true, 'tabIndex': el.attr('tabIndex') })
              .insertAfter(el),

              buttonlabel = (this.buttonlabel = $('<span />'))
                .html(o.noneSelectedText)
                .prependTo(button),

              menu = (this.menu = $('<div />'))
                .attr('id', 'multipleselect_' + id)//Modified by Mitesh for PL ticket #727
                .addClass('ui-multiselect-menu ui-widget ui-widget-content ui-corner-all')
                .addClass(o.classes)
                .appendTo($(o.appendTo)),

              header = (this.header = $('<div />'))
                .addClass('ui-widget-header ui-corner-all ui-multiselect-header ui-helper-clearfix')
                .appendTo(menu),

              checkboxContainer = (this.checkboxContainer = $('<ul />'))
                .addClass('ui-multiselect-checkboxes ui-helper-reset')
                .appendTo(menu)

            footer = (this.footer = $('<div />'))
              .addClass('ui-widget-footer ui-corner-all ui-multiselect-footer ui-helper-clearfix')
              .appendTo(menu),

            headerLinkContainer = (this.headerLinkContainer = $('<ul />'))
              .addClass('ui-helper-reset')
              .html(function () {
                  if (o.header === true) {
                      ////   Modified by Mitesh for PL ticket #727 - set id for "SelectAll" and "DeselectAll"
                      return '<li><a id="selectAll_' + id + '" class="ui-multiselect-all" href="#"><span class="ui-icon ui-icon-check"></span><span>' + o.checkAllText + '</span></a></li><li><a id="deselectAll_' + id + '" class="ui-multiselect-none" href="#"><span class="ui-icon ui-icon-closethick"></span><span>' + o.uncheckAllText + '</span></a></li>';
                  } else if (typeof o.header === "string") {
                      return '<li>' + o.header + '</li>';
                  } else {
                      return '';
                  }
              })
              //.append('<li class="ui-multiselect-close"><a href="#" class="ui-multiselect-close"><span class="ui-icon ui-icon-circle-close"></span></a></li>')
              .appendTo(footer),////Modified by Mitesh for PL ticket #727

            // perform event bindings
            this._bindEvents();

            // build menu
            this.refresh(true);

            // some addl. logic for single selects
            if (!o.multiple) {
                menu.addClass('ui-multiselect-single');
            }

            // bump unique ID
            multiselectID++;
        },

        _init: function () {
            if (this.options.header === false) {
                this.header.hide();
            }
            if (!this.options.multiple) {
                this.headerLinkContainer.find('.ui-multiselect-all, .ui-multiselect-none').hide();
            }
            if (this.options.autoOpen) {
                this.open();
            }
            if (this.element.is(':disabled')) {
                this.disable();
            }
        },

        refresh: function (init) {
          
            var el = this.element;
            var o = this.options;
            var menu = this.menu;
            var checkboxContainer = this.checkboxContainer;
            var optgroups = [];
            var html = "";
            var id = el.attr('id') || multiselectID++; // unique ID for the label & option tags
            //Start: Added by Mitesh Vaishnav for PL ticket #959 Filter changes for Revenue report
            var isCampaignCustomField = false;
            var isLineitemCustomField = false;
            var isProgramCustomField = false;
            var isTacticCustomField = false;
            var isCustomField = false;
            var istacticSectionDisplay = false;
            var isColumnview = false;
            var isGridView = false;
            //End: Added by Mitesh Vaishnav for PL ticket #959 Filter changes for Revenue report
            // build items
            el.find('option').each(function (i) {
                var $this = $(this);
                var parent = this.parentNode;
                var description = this.innerHTML;
                var title = this.title;
                var value = this.value;
                var inputID = 'ui-multiselect-' + (this.id || id + '-option-' + i);
                var isDisabled = this.disabled;
                var isSelected = this.selected;
                var labelClasses = ['ui-corner-all'];
                var liClasses = (isDisabled ? 'ui-multiselect-disabled ' : ' ') + this.className;
                var optLabel;
                ////start Added by Mitesh for creating border between custom fields and other 
                var borderTopClass = "";
             
                if (o.CustomName != '') {
                    if (!isCustomField) {
                        if (value.substring(0, o.CustomName.length) == o.CustomName) {
                            borderTopClass = "borderTop1px";
                            isCustomField = true;
                        }
                    }
                }
                ////End Added by Mitesh for creating border between custom fields and other 
                // is this an optgroup?
                if (parent.tagName === 'OPTGROUP') {
                    optLabel = parent.getAttribute('label');

                    // has this optgroup been added already?
                    if ($.inArray(optLabel, optgroups) === -1) {
                        html += '<li class="ui-multiselect-optgroup-label ' + parent.className + '"><a href="#">' + optLabel + '</a></li>';
                        optgroups.push(optLabel);
                    }
                }

                if (isDisabled) {
                    labelClasses.push('ui-state-disabled');
                }

                // browsers automatically select the first option
                // by default with single selects
                if (isSelected && !o.multiple) {
                    labelClasses.push('ui-state-active');
                }
                //Start: Added by Mitesh Vaishnav for PL ticket #959 Filter changes for Revenue report
                if (!isCampaignCustomField && o.CustomCampaignName != '') {
                    if (value.substring(0, o.CustomCampaignName.length) == o.CustomCampaignName) {
                        isCampaignCustomField = true;
                        var tempId = $this.parent().attr('id');
                        html += '<div class="report-seperator" id="' + tempId + '_campaignCustomHeader"><span>Campaign Fields</span></div>'
                    }
                }
                if (!isProgramCustomField && o.CustomProgramName != '') {
                    if (value.substring(0, o.CustomProgramName.length) == o.CustomProgramName) {
                        isProgramCustomField = true;
                        var tempId = $this.parent().attr('id');
                        html += '<div class="report-seperator" id="' + tempId + '_programCustomHeader"><span>Program Fields</span></div>'
                    }
                }

                if ((!isTacticCustomField && o.CustomTacticName != '') || (!istacticSectionDisplay && o.IsTacticSection != '')) {
                    if ((value.substring(0, o.CustomTacticName.length) == o.CustomTacticName) || o.IsTacticSection != '') {
                        isTacticCustomField = true;
                        istacticSectionDisplay = true;
                        var tempId = $this.parent().attr('id');
                        html += '<div class="report-seperator" id="' + tempId + '_tacticCustomHeader"><span>Tactic Fields</span></div>'
                    }
                }
                if (!isLineitemCustomField && o.CustomLineItemName != '') {
                    if (value.substring(0, o.CustomLineItemName.length) == o.CustomLineItemName) {
                        isLineitemCustomField = true;
                        var tempId = $this.parent().attr('id');
                        html += '<div class="report-seperator" id="' + tempId + '_LineitemCustomHeader"><span>Line Item Fields</span></div>'
                    }
                }
                //if (o.CustomColumnGroupName == '')
                //{
                //    isColumnview = true;
                    
                //}
                //else if (!isColumnview && o.CustomColumnGroupName != '')  {
                //    if (value.substring(0, o.CustomColumnGroupName.length) == o.CustomColumnGroupName)  {
                //        isColumnview = true;
                //        isGridView = true;
                //        var tempId = $this.parent().attr('id');
                //        html += '<div class="report-seperator" id="' + tempId + '_ColumnViewCustomHeader"><span>Column View</span></div>'
                //    }
                //}
            

                //End: Start: Added by Mitesh Vaishnav for PL ticket #959 Filter changes for Revenue report
                html += '<li class="' + liClasses + ' ' + borderTopClass + '">';

                // create the label
                html += '<label for="' + inputID + '" title="' + title + '" class="' + labelClasses.join(' ') + '">';
                html += '<input id="' + inputID + '" name="multiselect_' + id + '" type="' + (o.multiple ? "checkbox" : "radio") + '" value="' + value + '" title="' + title + '"';

                // pre-selected?
                if (isSelected) {
                    html += ' checked="checked"';
                    html += ' aria-selected="true"';
                }

                // disabled?
                if (isDisabled) {
                    html += ' disabled="disabled"';
                    html += ' aria-disabled="true"';
                }

                // add the title and close everything off
                html += ' /><span class="tipsyTitle" title="' + description + '">' + description + '</span></label></li>';

            });
           
            //if (!isColumnview) {
            //    isColumnview = true;
            //    var tempId = el.attr('id');
            //    html += '<div class="report-seperator" id="' + tempId + '_ColumnViewCustomHeader"><span>Column View</span></div>'
            //    html += '<a href="javascript:void(0)" onclick="OpenCreateNew();" class="addNewViewLink"><i class="fa fa-plus-circle" aria-hidden="true"></i> Add New View</a>'
            //}
            //else if (isGridView)
            //{
            //    html += '<a href="javascript:void(0)" onclick="OpenCreateNew();" class="addNewViewLink"><i class="fa fa-plus-circle" aria-hidden="true"></i> Add New View</a>'
            //}

            // insert into the DOM
            checkboxContainer.html(html);

            // cache some moar useful elements
            this.labels = menu.find('label');
            this.inputs = this.labels.children('input');

            // set widths
            //this._setButtonWidth();
            //this._setMenuWidth();

            // remember default value
            this.button[0].defaultValue = this.update();

            // broadcast refresh event; useful for widgets
            if (!init) {
                this._trigger('refresh');
            }
        },

        // updates the button text. call refresh() to rebuild
        update: function () {
            var o = this.options;
            var $inputs = this.inputs;
            var $checked = $inputs.filter(':checked');
            var numChecked = $checked.length;
            var value;

            if (numChecked === 0) {
                value = o.noneSelectedText;
            } else {
                if ($.isFunction(o.selectedText)) {
                    value = o.selectedText.call(this, numChecked, $inputs.length, $checked.get());
                } else if (/\d/.test(o.selectedList) && o.selectedList > 0 && numChecked <= o.selectedList) {
                    //value = $checked.map(function () { return $(this).next().html(); }).get().join(', ');
                    value = $checked.map(function () { return $(this).next().text(); }).get().join(', ');
                    // Change By Rahul Shah Desc: Reg issue on budget dropdown encode.
                } else {
                    ////Modified by Mitesh for PL ticket #727
                    if (typeof o.selectedText != 'undefined') {
                        value = o.selectedText.replace('#', numChecked).replace('#', $inputs.length);
                    }
                }
            }

            this._setButtonValue(value);

            return value;
        },

        // this exists as a separate method so that the developer 
        // can easily override it.
        _setButtonValue: function (value) {
            this.button.attr('title', value);
            this.buttonlabel.text(value);
        },

        // binds events
        _bindEvents: function () {
            var self = this;
            var button = this.button;

            function clickHandler() {
                $('.dropdown-wrapper').css('display', 'none');
                self[self._isOpen ? 'close' : 'open']();
                var searchInput = self.menu.find('.ui-multiselect-filter input[type="search"]');
                
                if (typeof searchInput != 'undefined') {
                    searchInput.val('');
                    self.menu.find('ul li').css('display', 'list-item');

                }
                return false;
            }

            // webkit doesn't like it when you click on the span :(
            button
              .find('span')
              .bind('click.multiselect', clickHandler);

            // button events
            button.bind({
                click: clickHandler,
                keypress: function (e) {
                    $prevtext = '';
                    $nextdata = -1;
                    $_prevdata = -1;
                    selectbox_keys = ''
                    switch (e.which) {
                        case 27: // esc
                        case 38: // up
                        case 37: // left
                            self.close();
                            break;
                        case 39: // right
                        case 40: // down
                            self.open();
                            break;
                    }
                },
                mouseenter: function () {
                    if (!button.hasClass('ui-state-disabled')) {
                        $(this).addClass('ui-state-hover');
                    }
                },
                mouseleave: function () {
                    $(this).removeClass('ui-state-hover');
                },
                focus: function () {
                    if (!button.hasClass('ui-state-disabled')) {
                        $(this).addClass('ui-state-focus');
                    }
                },
                blur: function () {
                    $(this).removeClass('ui-state-focus');
                }
            });

            // header links
            //Modified by Mitesh for PL ticket #727
            this.footer.delegate('a', 'click.multiselect', function (e) {
                // close link
                if ($(this).hasClass('ui-multiselect-close')) {
                    self.close();

                    // check all / uncheck all
                } else {
                    self[$(this).hasClass('ui-multiselect-all') ? 'checkAll' : 'uncheckAll']();
                }

                e.preventDefault();
            });

            // optgroup label toggle support
            this.menu.delegate('li.ui-multiselect-optgroup-label a', 'click.multiselect', function (e) {
                e.preventDefault();

                var $this = $(this);
                var $inputs = $this.parent().nextUntil('li.ui-multiselect-optgroup-label').find('input:visible:not(:disabled)');
                var nodes = $inputs.get();
                var label = $this.parent().text();

                // trigger event and bail if the return is false
                if (self._trigger('beforeoptgrouptoggle', e, { inputs: nodes, label: label }) === false) {
                    return;
                }

                // toggle inputs
                self._toggleChecked(
                  $inputs.filter(':checked').length !== $inputs.length,
                  $inputs
                );

                self._trigger('optgrouptoggle', e, {
                    inputs: nodes,
                    label: label,
                    checked: nodes[0].checked
                });
            })
          .delegate('label', 'mouseenter.multiselect', function () {
              if (!$(this).hasClass('ui-state-disabled')) {
                  self.labels.removeClass('ui-state-hover');
                  $(this).addClass('ui-state-hover').find('input').focus();
              }
          })
          .delegate('label', 'keydown.multiselect', function (e) {
              e.preventDefault();
              // alert(e.which);
              var t = e.which;
              switch (e.which) {
                  case 9: // tab
                  case 27: // esc
                      self.close();
                      break;
                  case 38: // up
                  case 40: // down
                  case 37: // left
                  case 39: // right
                      self._traverse(e.which, this);
                      break;
                  case 13: // enter
                      $(this).find('input')[0].click();
                      break;
              }

              //ui-state-hover -->setfocus
              $(this).removeClass('ui-state-hover');
              var key = e.charCode ? e.charCode : e.keyCode ? e.keyCode : 0;             
              if (key == 38 || key == 40) {
                  //Insertation Start Kausha 08/06/2016 #2274 Getting Elmah Logs on Home Page (Added following if condition(if ($nextdata!=-1)))
                 if ($nextdata!=-1)
                  $nextdata.removeClass('ui-state-hover');
                  return false;
                  //var tt = $_prevdata;
                  //var cu = $(this);
              }

              // delta to convert num-pad key codes to QWERTY codes.
              var numPadToKeyPadDelta = 48;
              // if a numeric key on the num pad was pressed.
              if (key >= 96 && key <= 105) {
                  key = key - numPadToKeyPadDelta;
                  //return String.fromCharCode(keyCode);
              }

              var theChar = String.fromCharCode(key);
              // alert(theChar);
              var cur_check = '';
              cur_check = selectbox_keys + theChar.toLowerCase();
              //  alert(cur_check);
              var $select = self.menu.find('ul');
              var $selectlist = $select.eq(0);
              // var cur_check = theChar;

              $selectlist.each(function () {
                  var list = $(this).find('li');
                  $(this).find('li').removeClass('ui-state-hover');
                  var count = 0;

                  var selectarr = [];
                  $(list).each(function (index, val) {
                      var currentLine = $(this).find('span').text();
                      // if (currentLine != "Select All" || currentLine !="Deselect All") {
                      var _mainText = currentLine.toLowerCase();
                      var _fText = _mainText.substring(0, 1);
                      // if (count == 0) {
                      if ($prevtext == '' || cur_check.length > 1) {
                          if (_mainText.indexOf(cur_check) == 0) {
                              count++;
                              $prevtext = _mainText;
                              if ($_prevdata != null && $_prevdata != 'undefined' && $_prevdata != -1) {
                                  $($_prevdata).removeClass('setfocus');
                              }
                              $_prevdata = jQuery(this);
                              $nextdata = jQuery(this);
                              $nextdata.addClass('setfocus');

                              //$(this).addClass('ui-state-focus');
                              //self.labels.removeClass('ui-state-focus');
                              $($nextdata).addClass('ui-state-hover');
                              $($nextdata).find('input').focus();
                              //$(this).addClass('ui-state-hover').find('input').focus();
                              button.find('span').eq(0).text(currentLine);

                              selectbox_keys = cur_check;
                              // precode = currCode;

                              return false;
                              //button.prop('title', currentLine);
                              //button.text(currentLine);

                              //alert(button);
                              //self.labels.removeClass('ui-state-hover');
                              // $(this).addClass('ui-state-hover').find('input').focus();
                              //

                              //$(this).siblings().removeClass('ui-state-hover');
                              // }
                              // var t= $(this).parent().addClass('ui-state-hover');
                          }
                      }
                      else {
                          var nextdata = '';
                          //var nextdata = $(list).eq(listIndex).nextAll();
                          var nextdata = $('.setfocus').nextAll();
                          var cntr = 0;
                          $.each(nextdata, function (key, value) {
                              var nextdataline = $(value).find('span').text();
                              var _nextlineText = nextdataline.toLowerCase();
                              var _fText = _nextlineText.substring(0, 1);
                              if (_nextlineText.indexOf(cur_check) == 0) {
                                  //if (_nextlineText != $prevtext && _nextlineText.indexOf(cur_check) == 0) {
                                  $prevtext = _nextlineText;
                                  selectbox_keys = cur_check;
                                  //_prevdata = nextdataline;
                                  $($_prevdata).removeClass('setfocus');   // remove class 'setFocusli' from previously selected list object. 
                                  $_prevdata = jQuery(this);
                                  $nextdata = jQuery(this);
                                  $($nextdata).addClass('setfocus');
                                  //$(this).addClass('ui-state-focus');
                                  //self.labels.removeClass('ui-state-focus');
                                  $($nextdata).addClass('ui-state-hover');
                                  $($nextdata).find('input').focus();
                                  //$(this).addClass('ui-state-hover').find('input').focus();
                                  button.find('span').eq(0).text(nextdataline);
                                  // listIndex = key;
                                  cntr = cntr + 1;
                                  return false;
                              }
                          });
                          if (cntr == 0) {
                              var nextdata = list;
                              $.each(nextdata, function (key, value) {
                                  var nextdataline = $(this).find('span').text();
                                  var _nextlineText = nextdataline.toLowerCase();
                                  var _fText = _nextlineText.substring(0, 1);
                                  if (_nextlineText != $prevtext && _nextlineText.indexOf(cur_check) == 0) {
                                      $prevtext = _nextlineText;
                                      selectbox_keys = cur_check;
                                      $($_prevdata).removeClass('setfocus');   // remove class 'setFocusli' from previously selected list object.
                                      $_prevdata = jQuery(this);
                                      $nextdata = jQuery(this);
                                      $($nextdata).addClass('setfocus');   // add class 'setFocusli' to currently selected list object. 
                                      //$(this).addClass('ui-state-focus');
                                      //self.labels.removeClass('ui-state-focus');
                                      $($nextdata).addClass('ui-state-hover');
                                      $($nextdata).find('input').focus();
                                      //$(this).addClass('ui-state-hover').find('input').focus();
                                      button.find('span').eq(0).text(nextdataline);
                                      return false;
                                  }
                              });

                          }
                          if (cur_check.length <= 1) {
                              return false;
                          }

                      }
                      // }
                      //  else {
                      // }
                  })
              });
              if ($nextdata != -1) {
                  $nextdata.removeClass('ui-state-hover');
                  $nextdata.addClass('ui-state-hover').focus();
                  $($nextdata).find('input').focus();
                  //button.find('span').eq(0).text($nextdata);
                  //self.labels.removeClass('ui-state-hover');
              }

              clearTimeout(selectbox_keys_timeout);
              var selectbox_keys_timeout = setTimeout(function () {
                  selectbox_keys = "";
              }, 500);
              e.stopPropagation();
              return false;

          })
          .delegate('input[type="checkbox"], input[type="radio"]', 'click.multiselect', function (e) {
              var $this = $(this);
              var val = this.value;
              var checked = this.checked;
              var tags = self.element.find('option');

              // bail if this input is disabled or the event is cancelled
              if (this.disabled || self._trigger('click', e, { value: val, text: this.title, checked: checked }) === false) {
                  e.preventDefault();
                  return;
              }

              // make sure the input has focus. otherwise, the esc key
              // won't close the menu after clicking an item.
              $this.focus();

              // toggle aria state
              $this.attr('aria-selected', checked);

              // change state on the original option tags
              tags.each(function () {
                  if (this.value === val) {
                      this.selected = checked;
                  } else if (!self.options.multiple) {
                      this.selected = false;
                  }
              });

              // some additional single select-specific logic
              if (!self.options.multiple) {
                  self.labels.removeClass('ui-state-active');
                  $this.closest('label').toggleClass('ui-state-active', checked);

                  // close menu
                  self.close();
              }

              // fire change on the select box
              self.element.trigger("change");

              // setTimeout is to fix multiselect issue #14 and #47. caused by jQuery issue #3827
              // http://bugs.jquery.com/ticket/3827
              setTimeout($.proxy(self.update, self), 10);
          });

            // close each widget when clicking on any other element/anywhere else on the page
            $doc.bind('mousedown.' + this._namespaceID, function (event) {
                var target = event.target;

                if (self._isOpen
                    && target !== self.button[0]
                    && target !== self.menu[0]
                    && !$.contains(self.menu[0], target)
                    && !$.contains(self.button[0], target)
                  ) {
                    self.close();
                }
            });

            // deal with form resets.  the problem here is that buttons aren't
            // restored to their defaultValue prop on form reset, and the reset
            // handler fires before the form is actually reset.  delaying it a bit
            // gives the form inputs time to clear.
            $(this.element[0].form).bind('reset.multiselect', function () {
                setTimeout($.proxy(self.refresh, self), 10);
            });
        },

        // set button width
        _setButtonWidth: function () {
            var width = this.element.outerWidth();
            var o = this.options;

            if (/\d/.test(o.minWidth) && width < o.minWidth) {
                width = o.minWidth;
            }

            // set widths
            this.button.outerWidth(width);
        },

        // set menu width
        _setMenuWidth: function () {
            var m = this.menu;
            m.outerWidth(this.button.outerWidth());
        },

        // move up or down within the menu
        _traverse: function (which, start) {

            var $start = $(start);
            var moveToLast = which === 38 || which === 37;
            // select the first li that isn't an optgroup label / disabled
            var $next = $start.parent()[moveToLast ? 'prevAll' : 'nextAll']('li:not(.ui-multiselect-disabled, .ui-multiselect-optgroup-label)').first();
            //start Modified by Mitesh Vaishnav for PL ticket #906 Reports Drop Down List: Down Arrow Key Issue via Keyboard
            //Check if next object has display none property then mouseover set to next visible object
            var i = 0;
            while (i == 0) {
                if ($next.css('display') == 'none') {
                    if (moveToLast) {
                        $next = $next.prev()[moveToLast ? 'prevAll' : 'nextAll']('li:not(.ui-multiselect-disabled, .ui-multiselect-optgroup-label)').first();
                    }
                    else {
                        $next = $next.next()[moveToLast ? 'prevAll' : 'nextAll']('li:not(.ui-multiselect-disabled, .ui-multiselect-optgroup-label)').first();
                    }
                }
                else {
                    i = i + 1;
                }
            }
            //End Modified by Mitesh Vaishnav for PL ticket #906 Reports Drop Down List: Down Arrow Key Issue via Keyboard
            // if at the first/last element
            if (!$next.length) {
                var $container = this.menu.find('ul').last();

                // move to the first/last
                this.menu.find('label')[moveToLast ? 'last' : 'first']().trigger('mouseover');

                // set scroll position
                $container.scrollTop(moveToLast ? $container.height() : 0);

            } else {
                $next.find('label').trigger('mouseover');
            }
        },

        // This is an internal function to toggle the checked property and
        // other related attributes of a checkbox.
        //
        // The context of this function should be a checkbox; do not proxy it.
        _toggleState: function (prop, flag) {
            return function () {
                if (!this.disabled) {
                    this[prop] = flag;
                }

                if (flag) {
                    this.setAttribute('aria-selected', true);
                } else {
                    this.removeAttribute('aria-selected');
                }
            };
        },

        _toggleChecked: function (flag, group) {
            var $inputs = (group && group.length) ? group : this.inputs;
            var self = this;

            // toggle state on inputs
            $inputs.each(this._toggleState('checked', flag));

            // give the first input focus
            $inputs.eq(0).focus();

            // update button text
            this.update();

            // gather an array of the values that actually changed
            var values = $inputs.map(function () {
                return this.value;
            }).get();

            // toggle state on original option tags
            this.element
              .find('option')
              .each(function () {
                  if (!this.disabled && $.inArray(this.value, values) > -1) {
                      self._toggleState('selected', flag).call(this);
                  }
              });

            // trigger the change event on the select
            if ($inputs.length) {
                this.element.trigger("change");
            }
        },

        _toggleDisabled: function (flag) {
            this.button.attr({ 'disabled': flag, 'aria-disabled': flag })[flag ? 'addClass' : 'removeClass']('ui-state-disabled');

            var inputs = this.menu.find('input');
            var key = "ech-multiselect-disabled";

            if (flag) {
                // remember which elements this widget disabled (not pre-disabled)
                // elements, so that they can be restored if the widget is re-enabled.
                inputs = inputs.filter(':enabled').data(key, true)
            } else {
                inputs = inputs.filter(function () {
                    return $.data(this, key) === true;
                }).removeData(key);
            }

            inputs
              .attr({ 'disabled': flag, 'arial-disabled': flag })
              .parent()[flag ? 'addClass' : 'removeClass']('ui-state-disabled');

            this.element.attr({
                'disabled': flag,
                'aria-disabled': flag
            });
        },

        // open the menu
        open: function (e) {
            var self = this;
            var button = this.button;
            var menu = this.menu;
            var speed = this.speed;
            var o = this.options;
            var args = [];

            // bail if the multiselectopen event returns false, this widget is disabled, or is already open
            if (this._trigger('beforeopen') === false || button.hasClass('ui-state-disabled') || this._isOpen) {
                return;
            }

            var $container = menu.find('ul').last();
            var effect = o.show;

            // figure out opening effects/speeds
            if ($.isArray(o.show)) {
                effect = o.show[0];
                speed = o.show[1] || self.speed;
            }

            // if there's an effect, assume jQuery UI is in use
            // build the arguments to pass to show()
            if (effect) {
                args = [effect, speed];
            }

            // set the scroll of the checkbox container
            $container.scrollTop(0).height(o.height);

            // positon
            this.position();

            // show the menu, maybe with a speed/effect combo
            $.fn.show.apply(menu, args);

            // select the first not disabled option
            // triggering both mouseover and mouseover because 1.4.2+ has a bug where triggering mouseover
            // will actually trigger mouseenter.  the mouseenter trigger is there for when it's eventually fixed
            this.labels.filter(':not(.ui-state-disabled)').eq(0).trigger('mouseover').trigger('mouseenter').find('input').trigger('focus');

            button.addClass('ui-state-active');
            this._isOpen = true;
            this._trigger('open');
            menu.find('li').removeClass('ui-state-hover');
        },

        // close the menu
        close: function () {
            if (this._trigger('beforeclose') === false) {
                return;
            }

            var o = this.options;
            var effect = o.hide;
            var speed = this.speed;
            var args = [];

            // figure out opening effects/speeds
            if ($.isArray(o.hide)) {
                effect = o.hide[0];
                speed = o.hide[1] || this.speed;
            }

            if (effect) {
                args = [effect, speed];
            }

            $.fn.hide.apply(this.menu, args);
            this.button.removeClass('ui-state-active').trigger('blur').trigger('mouseleave');
            this._isOpen = false;
            this._trigger('close');
        },

        enable: function () {
            this._toggleDisabled(false);
        },

        disable: function () {
            this._toggleDisabled(true);
        },

        checkAll: function (e) {
            this._toggleChecked(true);
            this._trigger('checkAll');
        },

        uncheckAll: function () {
            this._toggleChecked(false);
            this._trigger('uncheckAll');
        },

        getChecked: function () {
            return this.menu.find('input').filter(':checked');
        },

        destroy: function () {
            // remove classes + data
            $.Widget.prototype.destroy.call(this);

            // unbind events
            $doc.unbind(this._namespaceID);

            this.button.remove();
            this.menu.remove();
            this.element.show();

            return this;
        },

        isOpen: function () {
            return this._isOpen;
        },

        widget: function () {
            return this.menu;
        },

        getButton: function () {
            return this.button;
        },

        position: function () {
            var o = this.options;

            // use the position utility if it exists and options are specifified
            if ($.ui.position && !$.isEmptyObject(o.position)) {
                o.position.of = o.position.of || this.button;

                this.menu
                  .show()
                  .position(o.position)
                  .hide();

                // otherwise fallback to custom positioning
            } else {
                /* Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #934 */
                var btnLeftPos = this.button.offset().left;
                var btnWidth = this.button.width();
                var bthRightPos = (btnLeftPos + btnWidth);
                var leftAdjusted = false;
                /* End - Added by Sohel Pathan on 12/11/2014 for PL ticket #934 */
                var windowWidth = $(window).width();
                var popupLeftPosition = this.menu.width();
                var pos = this.button.offset();

                /* Start - Added by Sohel Pathan on 11/11/2014 for PL ticket #934 */
                var rightPos = windowWidth - (pos.left + this.button.width()) - 30;
                if (rightPos < 5) {
                    rightPos = 5;
                }
                else if (rightPos > 70) {
                    rightPos = 70;
                }
                /* End - Added by Sohel Pathan on 11/11/2014 for PL ticket #934 */

                popupLeftPosition += pos.left;

                if (popupLeftPosition > windowWidth) {
                    leftAdjusted = true;
                    pos.left = (pos.left + this.button.width()) - (this.menu.width());
                }

                /* Start - Added by Sohel Pathan on 11/11/2014 for PL ticket #934 */
                if (rightPos == 5) {
                    var menuLeftPos = this.menu.offset().left;
                    var menuWidth = this.menu.width();
                    var menuRightPos = (menuLeftPos + menuWidth);

                    if ((menuRightPos >= bthRightPos)) {
                        pos.left -= 30;
                    }
                    if (!leftAdjusted) {
                        pos.left -= 30;
                    }
                }

                if (this.options.classes.toString() == 'custom-right-pos') {
                    this.menu.css({
                        top: pos.top + this.button.outerHeight(),
                        left: pos.left,
                        right: rightPos
                    });
                }
                else {
                    /* End - Added by Sohel Pathan on 11/11/2014 for PL ticket #934 */
                    this.menu.css({
                        top: pos.top + this.button.outerHeight(),
                        left: pos.left
                    });
                }
            }
        },

        // react to option changes after initialization
        _setOption: function (key, value) {
            var menu = this.menu;

            switch (key) {
                case 'header':
                    menu.find('div.ui-multiselect-header')[value ? 'show' : 'hide']();
                    break;
                case 'checkAllText':
                    menu.find('a.ui-multiselect-all span').eq(-1).text(value);
                    break;
                case 'uncheckAllText':
                    menu.find('a.ui-multiselect-none span').eq(-1).text(value);
                    break;
                case 'height':
                    //menu.find('ul').last().height(parseInt(value, 10));
                    break;
                case 'minWidth':
                    this.options[key] = parseInt(value, 10);
                    this._setButtonWidth();
                    this._setMenuWidth();
                    break;
                case 'selectedText':
                case 'selectedList':
                case 'noneSelectedText':
                    this.options[key] = value; // these all needs to update immediately for the update() call
                    this.update();
                    break;
                case 'classes':
                    menu.add(this.button).removeClass(this.options.classes).addClass(value);
                    break;
                case 'multiple':
                    menu.toggleClass('ui-multiselect-single', !value);
                    this.options.multiple = value;
                    this.element[0].multiple = value;
                    this.refresh();
                    break;
                case 'position':
                    this.position();
            }

            $.Widget.prototype._setOption.apply(this, arguments);
        }
    });

})(jQuery);
