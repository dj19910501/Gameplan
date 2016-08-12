/**
 * nlform.js v1.0.0
 * http://www.codrops.com
 *
 * Licensed under the MIT license.
 * http://www.opensource.org/licenses/mit-license.php
 * 
 * Copyright 2013, Codrops
 * http://www.codrops.com
 */

;( function( window ) {
	
	'use strict';

	var document = window.document;

	if (!String.prototype.trim) {
		String.prototype.trim=function(){return this.replace(/^\s+|\s+$/g, '');};
	}

	function NLForm( el ) {	
		this.el = el;
		this.overlay = this.el.querySelector( '.nl-overlay' );
		this.fields = [];
		this.fldOpen = -1;
		this._init();
	}

	NLForm.prototype = {
		_init : function() {
			var self = this;
			Array.prototype.slice.call( this.el.querySelectorAll( 'select' ) ).forEach( function( el, i ) {
				self.fldOpen++;
				self.fields.push( new NLField( self, el, 'dropdown', self.fldOpen ) );
			} );
			Array.prototype.slice.call( this.el.querySelectorAll( 'input' ) ).forEach( function( el, i ) {
				self.fldOpen++;
				self.fields.push(new NLField(self, el, 'input', self.fldOpen));
			});
		    try
		    {
		        this.overlay.addEventListener( 'click', function(ev) { self._closeFlds(); } );
		        this.overlay.addEventListener('touchstart', function (ev) { self._closeFlds(); });
		    }
			catch(err) {}
		    // Add For format Price & currency Value.
		    //Addded By Bhavesh Dobariya Date : 20-12-2013
			$(".priceValue").priceFormat({ prefix: '', centsSeparator: '', thousandsSeparator: ',', centsLimit: 0 });
			$('.currency_dollar').priceFormat({ prefix: CurrencySybmol, centsSeparator: '', thousandsSeparator: ',', centsLimit: 0 }); //Modified by Rahul Shah for PL #2498.
			$('.dollarValue').priceFormat({ prefix: '', centsSeparator: '', thousandsSeparator: ',', centsLimit: 0 });

			$('.percentValue').priceFormat({ prefix: '', suffix: '%', centsSeparator: '', thousandsSeparator: ',', centsLimit: 0, isDouble: true, });

		},
		_closeFlds : function() {
			if( this.fldOpen !== -1 ) {
				this.fields[ this.fldOpen ].close();
			}
		}
	}

	function NLField( form, el, type, idx ) {
		this.form = form;
		this.elOriginal = el;
		this.pos = idx;
		this.type = type;
		this._create();
		this._initEvents();
	}

	NLField.prototype = {
		_create : function() {
			if( this.type === 'dropdown' ) {
				this._createDropDown();	
			}
			else if( this.type === 'input' ) {
				this._createInput();	
			}
		},
		_createDropDown: function () {
			var self = this;
			this.fld = document.createElement( 'div' );
			this.fld.className = 'nl-field nl-dd';
			this.toggle = document.createElement( 'a' );
			this.toggle.innerHTML = this.elOriginal.options[this.elOriginal.selectedIndex].innerHTML;
			if (this.elOriginal.hasAttribute('maxlength')) {
			    var maxLength = this.elOriginal.getAttribute('maxlength');
			    if (maxLength.length > 0) {
			        if (this.toggle.innerHTML.length > maxLength) {
			            this.toggle.title = $('<div/>').html(this.toggle.innerHTML).text();
			            this.toggle.innerHTML = this.toggle.innerHTML.substring(0, maxLength) + '...';
			        }
			    }
			}

			this.toggle.className = 'nl-field-toggle';
			this.optionsList = document.createElement( 'ul' );
			var ihtml = '';
			Array.prototype.slice.call(this.elOriginal.querySelectorAll('option')).forEach(function (el, i) {
			    // Addded by Bhavesh Dobariya
			    // Text is large then Dropdown issue occur - resolved
			    // Date: 28-3-2014
			    var ddValue;
			    if (el.hasAttribute('value')) {
			        ddValue = el.getAttribute('value');
			    }
                var ddtextValue = el.innerHTML;
			    var isLarge = false;
			    if (ddtextValue.length > 30) {
			        isLarge = true;
			        ddtextValue = el.innerHTML.substring(0, 30) + '...';
			    }
                // Comment by bhavesh
			    //ihtml += self.elOriginal.selectedIndex === i ? '<li class="nl-dd-checked">' + el.innerHTML + '</li>' : '<li title="' + el.innerHTML  + '">' + el.innerHTML + '</li>';

			    ihtml += '<li';
				// selected index value
			    if (self.elOriginal.selectedIndex === i) {
			        ihtml += ' class="nl-dd-checked"';
			        self.selectedIdx = i;
			    }
			    if (isLarge) {
			        ihtml += ' title="' + el.innerHTML + '"';
			    }
			    ihtml += ' value="' + ddValue + '" originalValue="' + el.innerHTML + '" TextValue="' + ddValue + '">' + ddtextValue + '</li>'; //Modified by Rahul Shah for PL #2383

			} );
			this.optionsList.innerHTML = ihtml;
		    // Added by Bhavesh Dobariya
            // Date : 21-4-2014 , Overflow add in nl drop down
			this.optionsList.setAttribute('style', 'overflow:auto; max-height: 350px;');
			this.fld.appendChild( this.toggle );
			this.fld.appendChild( this.optionsList );
			this.elOriginal.parentNode.insertBefore( this.fld, this.elOriginal );
			this.elOriginal.style.display = 'none';
		},
		_createInput: function () {
			var self = this;
			this.fld = document.createElement( 'div' );
			this.fld.className = 'nl-field nl-ti-text';
			this.toggle = document.createElement('a');
			var originalValue = this.elOriginal.getAttribute('placeholder');
			var changeValue = originalValue;
			var getFormatType = this.elOriginal.getAttribute('formatType');
		
			if (getFormatType == 'priceValue' || getFormatType == 'dollarValue') {
			    changeValue = FormatCommas(originalValue, false);
			}
			if (getFormatType == 'currency_dollar')
			{
			    changeValue = FormatCurrency(originalValue, false);
			}
			
			if (getFormatType == 'percentValue') {
			    changeValue = FormatPercent(originalValue, true);
			   
			}
		    // Addded by Bhavesh Dobariya
		    // IN edit mode text is large then display short.
		    // Date: 28-3-2014
			var elvalue = changeValue;
			if (elvalue.length > 15) {
			    this.toggle.setAttribute('title', elvalue);
			    elvalue = elvalue.substring(0, 15) + '...';
			}
			else {
			    this.toggle.removeAttribute('title');
			}
			this.toggle.innerHTML = elvalue;

			this.toggle.className = 'nl-field-toggle';
			this.optionsList = document.createElement( 'ul' );
			this.getinput = document.createElement( 'input' );
			this.getinput.setAttribute( 'type', 'text' );
			this.getinput.setAttribute('placeholder', changeValue);
			this.getinput.setAttribute('maxlength', this.elOriginal.getAttribute('maxlength'));
		    // Addded by Bhavesh Dobariya
		    // Set Input value
		    // Date: 28-3-2014
			var IsEditable = this.elOriginal.getAttribute('isedit');
			if (IsEditable != null && IsEditable != 'undefined' && IsEditable != '' && IsEditable.toLowerCase() == "true") {
			    this.getinput.setAttribute('value', changeValue);
			}
			else {
			    this.getinput.setAttribute('value', '');
			}

			this.getinput.setAttribute('onblur', 'getblurvalue(this);');
			this.getinput.setAttribute('onfocus', 'OnNLTextFocus(this);');
            //Added By Bhavesh
			this.getinput.setAttribute('class', getFormatType);

			this.getinputWrapper = document.createElement( 'li' );
			this.getinputWrapper.className = 'nl-ti-input';
			this.inputsubmit = document.createElement( 'button' );
			this.inputsubmit.className = 'nl-field-go';
			this.inputsubmit.innerHTML = 'Go';
			this.getinputWrapper.appendChild( this.getinput );
			this.getinputWrapper.appendChild( this.inputsubmit );
			this.example = document.createElement( 'li' );
			this.example.className = 'nl-ti-example';
			this.example.innerHTML = this.elOriginal.getAttribute( 'datasubline' );
			this.optionsList.appendChild( this.getinputWrapper );
			this.optionsList.appendChild( this.example );
			this.fld.appendChild( this.toggle );
			this.fld.appendChild( this.optionsList );
			this.elOriginal.parentNode.insertBefore( this.fld, this.elOriginal );
			this.elOriginal.style.display = 'none';

		},
		_initEvents: function () {
			var self = this;
			this.toggle.addEventListener( 'click', function( ev ) { ev.preventDefault(); ev.stopPropagation(); self._open(); } );
			this.toggle.addEventListener( 'touchstart', function( ev ) { ev.preventDefault(); ev.stopPropagation(); self._open(); } );

			if( this.type === 'dropdown' ) {
				var opts = Array.prototype.slice.call( this.optionsList.querySelectorAll( 'li' ) );
				opts.forEach( function( el, i ) {
					el.addEventListener( 'click', function( ev ) { ev.preventDefault(); self.close( el, opts.indexOf( el ) ); } );
					el.addEventListener( 'touchstart', function( ev ) { ev.preventDefault(); self.close( el, opts.indexOf( el ) ); } );
				} );
			}
			else if (this.type === 'input') {
				this.getinput.addEventListener( 'keydown', function( ev ) {
					if ( ev.keyCode == 13 ) {
						self.close();
					}
				} );
				this.inputsubmit.addEventListener( 'click', function( ev ) { ev.preventDefault(); self.close(); } );
				this.inputsubmit.addEventListener( 'touchstart', function( ev ) { ev.preventDefault(); self.close(); } );
			}

		},
		_open: function () {
			if( this.open ) {
				return false;
			}
			this.open = true;
			this.form.fldOpen = this.pos;
			var self = this;
			this.fld.className += ' nl-field-open';
		},
		close: function (opt, idx) {
			if( !this.open ) {
				return false;
			}
			this.open = false;
			this.form.fldOpen = -1;
			this.fld.className = this.fld.className.replace(/\b nl-field-open\b/,'');

			if( this.type === 'dropdown' ) {
				if( opt ) {
					// remove class nl-dd-checked from previous option
					var selectedopt = this.optionsList.children[ this.selectedIdx ];
					selectedopt.className = '';
					opt.className = 'nl-dd-checked';
					this.toggle.innerHTML = opt.getAttribute('originalValue');
					// update selected index value
					this.selectedIdx = idx;
					// update original select element´s value
					this.elOriginal.value = this.elOriginal.children[this.selectedIdx].value;

				    // Added by bhavesh if change value of dropdown then update title & text as per maxlength defined
				    // Date 28-3-2014
					this.toggle.removeAttribute('title');
					if (this.elOriginal.hasAttribute('maxlength')) {
					    var maxLength = this.elOriginal.getAttribute('maxlength');
					    if (maxLength.length > 0) {
					        if (this.toggle.innerHTML.length > maxLength) {
					            this.toggle.title = this.toggle.innerHTML;
					            this.toggle.innerHTML = this.toggle.innerHTML.substring(0, maxLength) + '...';
					        }
					    }
					}

				}
			}
			else if (this.type === 'input') {
			    this.getinput.blur();
				this.elOriginal.value = this.getinput.value;
				var elvalue = this.getinput.value;
				if (elvalue.length > 15) {
				    this.toggle.setAttribute('title', elvalue);
				    elvalue = elvalue.substring(0, 15) + '...';
				}
				else {
				    this.toggle.removeAttribute('title');
				}
				this.toggle.innerHTML = this.getinput.value.trim() !== '' ? elvalue : this.getinput.getAttribute('placeholder');
				
			}
		}
	}

	// add to global namespace
	window.NLForm = NLForm;

} )( window );