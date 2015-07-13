/*
This software is allowed to use under GPL or you need to obtain Commercial or Enterise License 
 to use it in non-GPL project. Please contact sales@dhtmlx.com for details
*/
function dtmlXMLLoaderObject(t, e, n, i) {
    return this.xmlDoc = "", this.async = "undefined" != typeof n ? n : !0, this.onloadAction = t || null, this.mainObject = e || null, this.waitCall = null, this.rSeed = i || !1, this
}
function callerFunction(t, e) {
    return this.handler = function (n) { return n || (n = window.event), t(n, e), !0 }, this.handler
}
function getAbsoluteLeft(t) {
    return getOffset(t).left
}
function getAbsoluteTop(t) {
    return getOffset(t).top
}
function getOffsetSum(t) {
    for (var e = 0, n = 0; t;) e += parseInt(t.offsetTop), n += parseInt(t.offsetLeft), t = t.offsetParent; return { top: e, left: n }
}
function getOffsetRect(t) {
    var e = t.getBoundingClientRect(), n = document.body, i = document.documentElement, a = window.pageYOffset || i.scrollTop || n.scrollTop, s = window.pageXOffset || i.scrollLeft || n.scrollLeft, r = i.clientTop || n.clientTop || 0, o = i.clientLeft || n.clientLeft || 0, d = e.top + a - r, l = e.left + s - o; return { top: Math.round(d), left: Math.round(l) }
}

function getOffset(t) {
    return t.getBoundingClientRect ? getOffsetRect(t) : getOffsetSum(t)
}
function convertStringToBoolean(t) {
    switch ("string" == typeof t && (t = t.toLowerCase()), t) { case "1": case "true": case "yes": case "y": case 1: case !0: return !0; default: return !1 }
}
function getUrlSymbol(t) {
    return -1 != t.indexOf("?") ? "&" : "?"
}
function dhtmlDragAndDropObject() {
    return window.dhtmlDragAndDrop ? window.dhtmlDragAndDrop : (this.lastLanding = 0, this.dragNode = 0, this.dragStartNode = 0, this.dragStartObject = 0, this.tempDOMU = null, this.tempDOMM = null, this.waitDrag = 0, window.dhtmlDragAndDrop = this, this)
}

function _dhtmlxError() {
    return this.catches || (this.catches = new Array), this
}

function dhtmlXHeir(t, e) {
    for (var n in e) "function" == typeof e[n] && (t[n] = e[n]); return t
}

function dhtmlxEvent(t, e, n) {
    t.addEventListener ? t.addEventListener(e, n, !1) : t.attachEvent && t.attachEvent("on" + e, n)
}

function dhtmlxDetachEvent(t, e, n) {
    t.removeEventListener ? t.removeEventListener(e, n, !1) : t.detachEvent && t.detachEvent("on" + e, n)
}
function dhtmlxDnD(t, e) {
    e && (this._settings = e), dhtmlxEventable(this), dhtmlxEvent(t, "mousedown", dhtmlx.bind(function (e) { this.dragStart(t, e) }, this))
}
function dataProcessor(t) {
    return this.serverProcessor = t, this.action_param = "!nativeeditor_status", this.object = null, this.updatedRows = [], this.autoUpdate = !0, this.updateMode = "cell", this._tMode = "GET", this.post_delim = "_", this._waitMode = 0, this._in_progress = {}, this._invalid = {}, this.mandatoryFields = [], this.messages = [], this.styles = { updated: "font-weight:bold;", inserted: "font-weight:bold;", deleted: "text-decoration : line-through;", invalid: "background-color:FFE0E0;", invalid_cell: "border-bottom:2px solid red;", error: "color:red;", clear: "font-weight:normal;text-decoration:none;" }, this.enableUTFencoding(!0), dhtmlxEventable(this), this
} dhtmlx = function (t) { for (var e in t) dhtmlx[e] = t[e]; return dhtmlx }, dhtmlx.extend_api = function (t, e, n) { var i = window[t]; i && (window[t] = function (t) { if (t && "object" == typeof t && !t.tagName) { var n = i.apply(this, e._init ? e._init(t) : arguments); for (var a in dhtmlx) e[a] && this[e[a]](dhtmlx[a]); for (var a in t) e[a] ? this[e[a]](t[a]) : 0 == a.indexOf("on") && this.attachEvent(a, t[a]) } else var n = i.apply(this, arguments); return e._patch && e._patch(this), n || this }, window[t].prototype = i.prototype, n && dhtmlXHeir(window[t].prototype, n)) }, dhtmlxAjax = { get: function (t, e) { var n = new dtmlXMLLoaderObject(!0); return n.async = arguments.length < 3, n.waitCall = e, n.loadXML(t), n }, post: function (t, e, n) { var i = new dtmlXMLLoaderObject(!0); return i.async = arguments.length < 4, i.waitCall = n, i.loadXML(t, !0, e), i }, getSync: function (t) { return this.get(t, null, !0) }, postSync: function (t, e) { return this.post(t, e, null, !0) } }, dtmlXMLLoaderObject.count = 0, dtmlXMLLoaderObject.prototype.waitLoadFunction = function (t) { var e = !0; return this.check = function () { if (t && null != t.onloadAction && (!t.xmlDoc.readyState || 4 == t.xmlDoc.readyState)) { if (!e) return; e = !1, dtmlXMLLoaderObject.count++, "function" == typeof t.onloadAction && t.onloadAction(t.mainObject, null, null, null, t), t.waitCall && (t.waitCall.call(this, t), t.waitCall = null) } }, this.check }, dtmlXMLLoaderObject.prototype.getXMLTopNode = function (t, e) { if (this.xmlDoc.responseXML) { var n = this.xmlDoc.responseXML.getElementsByTagName(t); if (0 == n.length && -1 != t.indexOf(":")) var n = this.xmlDoc.responseXML.getElementsByTagName(t.split(":")[1]); var i = n[0] } else var i = this.xmlDoc.documentElement; if (i) return this._retry = !1, i; if (!this._retry && _isIE) { this._retry = !0; var e = this.xmlDoc; return this.loadXMLString(this.xmlDoc.responseText.replace(/^[\s]+/, ""), !0), this.getXMLTopNode(t, e) } return dhtmlxError.throwError("LoadXML", "Incorrect XML", [e || this.xmlDoc, this.mainObject]), document.createElement("DIV") }, dtmlXMLLoaderObject.prototype.loadXMLString = function (t, e) { if (_isIE) this.xmlDoc = new ActiveXObject("Microsoft.XMLDOM"), this.xmlDoc.async = this.async, this.xmlDoc.onreadystatechange = function () { }, this.xmlDoc.loadXML(t); else { var n = new DOMParser; this.xmlDoc = n.parseFromString(t, "text/xml") } e || (this.onloadAction && this.onloadAction(this.mainObject, null, null, null, this), this.waitCall && (this.waitCall(), this.waitCall = null)) }, dtmlXMLLoaderObject.prototype.loadXML = function (t, e, n, i) { this.rSeed && (t += (-1 != t.indexOf("?") ? "&" : "?") + "a_dhx_rSeed=" + (new Date).valueOf()), this.filePath = t, this.xmlDoc = !_isIE && window.XMLHttpRequest ? new XMLHttpRequest : new ActiveXObject("Microsoft.XMLHTTP"), this.async && (this.xmlDoc.onreadystatechange = new this.waitLoadFunction(this)), this.xmlDoc.open(e ? "POST" : "GET", t, this.async), i ? (this.xmlDoc.setRequestHeader("User-Agent", "dhtmlxRPC v0.1 (" + navigator.userAgent + ")"), this.xmlDoc.setRequestHeader("Content-type", "text/xml")) : e && this.xmlDoc.setRequestHeader("Content-type", "application/x-www-form-urlencoded"), this.xmlDoc.setRequestHeader("X-Requested-With", "XMLHttpRequest"), this.xmlDoc.send(null || n), this.async || new this.waitLoadFunction(this)() }, dtmlXMLLoaderObject.prototype.destructor = function () { return this._filterXPath = null, this._getAllNamedChilds = null, this._retry = null, this.async = null, this.rSeed = null, this.filePath = null, this.onloadAction = null, this.mainObject = null, this.xmlDoc = null, this.doXPath = null, this.doXPathOpera = null, this.doXSLTransToObject = null, this.doXSLTransToString = null, this.loadXML = null, this.loadXMLString = null, this.doSerialization = null, this.xmlNodeToJSON = null, this.getXMLTopNode = null, this.setXSLParamValue = null, null }, dtmlXMLLoaderObject.prototype.xmlNodeToJSON = function (t) { for (var e = {}, n = 0; n < t.attributes.length; n++) e[t.attributes[n].name] = t.attributes[n].value; e._tagvalue = t.firstChild ? t.firstChild.nodeValue : ""; for (var n = 0; n < t.childNodes.length; n++) { var i = t.childNodes[n].tagName; i && (e[i] || (e[i] = []), e[i].push(this.xmlNodeToJSON(t.childNodes[n]))) } return e }, dhtmlDragAndDropObject.prototype.removeDraggableItem = function (t) { t.onmousedown = null, t.dragStarter = null, t.dragLanding = null }, dhtmlDragAndDropObject.prototype.addDraggableItem = function (t, e) { t.onmousedown = this.preCreateDragCopy, t.dragStarter = e, this.addDragLanding(t, e) }, dhtmlDragAndDropObject.prototype.addDragLanding = function (t, e) { t.dragLanding = e }, dhtmlDragAndDropObject.prototype.preCreateDragCopy = function (t) { return !t && !window.event || 2 != (t || event).button ? window.dhtmlDragAndDrop.waitDrag ? (window.dhtmlDragAndDrop.waitDrag = 0, document.body.onmouseup = window.dhtmlDragAndDrop.tempDOMU, document.body.onmousemove = window.dhtmlDragAndDrop.tempDOMM, !1) : (window.dhtmlDragAndDrop.dragNode && window.dhtmlDragAndDrop.stopDrag(t), window.dhtmlDragAndDrop.waitDrag = 1, window.dhtmlDragAndDrop.tempDOMU = document.body.onmouseup, window.dhtmlDragAndDrop.tempDOMM = document.body.onmousemove, window.dhtmlDragAndDrop.dragStartNode = this, window.dhtmlDragAndDrop.dragStartObject = this.dragStarter, document.body.onmouseup = window.dhtmlDragAndDrop.preCreateDragCopy, document.body.onmousemove = window.dhtmlDragAndDrop.callDrag, window.dhtmlDragAndDrop.downtime = (new Date).valueOf(), t && t.preventDefault ? (t.preventDefault(), !1) : !1) : void 0 }, dhtmlDragAndDropObject.prototype.callDrag = function (t) { if (t || (t = window.event), dragger = window.dhtmlDragAndDrop, !((new Date).valueOf() - dragger.downtime < 100)) { if (!dragger.dragNode) { if (!dragger.waitDrag) return dragger.stopDrag(t, !0); if (dragger.dragNode = dragger.dragStartObject._createDragNode(dragger.dragStartNode, t), !dragger.dragNode) return dragger.stopDrag(); dragger.dragNode.onselectstart = function () { return !1 }, dragger.gldragNode = dragger.dragNode, document.body.appendChild(dragger.dragNode), document.body.onmouseup = dragger.stopDrag, dragger.waitDrag = 0, dragger.dragNode.pWindow = window, dragger.initFrameRoute() } if (dragger.dragNode.parentNode != window.document.body && dragger.gldragNode) { var e = dragger.gldragNode; dragger.gldragNode.old && (e = dragger.gldragNode.old), e.parentNode.removeChild(e); var n = dragger.dragNode.pWindow; if (e.pWindow && e.pWindow.dhtmlDragAndDrop.lastLanding && e.pWindow.dhtmlDragAndDrop.lastLanding.dragLanding._dragOut(e.pWindow.dhtmlDragAndDrop.lastLanding), _isIE) { var i = document.createElement("Div"); i.innerHTML = dragger.dragNode.outerHTML, dragger.dragNode = i.childNodes[0] } else dragger.dragNode = dragger.dragNode.cloneNode(!0); dragger.dragNode.pWindow = window, dragger.gldragNode.old = dragger.dragNode, document.body.appendChild(dragger.dragNode), n.dhtmlDragAndDrop.dragNode = dragger.dragNode } if (dragger.dragNode.style.left = t.clientX + 15 + (dragger.fx ? -1 * dragger.fx : 0) + (document.body.scrollLeft || document.documentElement.scrollLeft) + "px", dragger.dragNode.style.top = t.clientY + 3 + (dragger.fy ? -1 * dragger.fy : 0) + (document.body.scrollTop || document.documentElement.scrollTop) + "px", t.srcElement) a = t.srcElement; else var a = t.target; dragger.checkLanding(a, t) } }, dhtmlDragAndDropObject.prototype.calculateFramePosition = function (t) {
    if (window.name) {
        for (var e = parent.frames[window.name].frameElement.offsetParent, n = 0, i = 0; e;)
            n += e.offsetLeft, i += e.offsetTop, e = e.offsetParent;
        if (parent.dhtmlDragAndDrop) {
            var a = parent.dhtmlDragAndDrop.calculateFramePosition(1); n += 1 * a.split("_")[0], i += 1 * a.split("_")[1]
        }
        if (t) return n + "_" + i; this.fx = n, this.fy = i
    }
    return "0_0"
}
, dhtmlDragAndDropObject.prototype.checkLanding = function (t, e) {
    t && t.dragLanding ? (this.lastLanding && this.lastLanding.dragLanding._dragOut(this.lastLanding), this.lastLanding = t, this.lastLanding = this.lastLanding.dragLanding._dragIn(this.lastLanding, this.dragStartNode, e.clientX, e.clientY, e), this.lastLanding_scr = _isIE ? e.srcElement : e.target) : t && "BODY" != t.tagName ? this.checkLanding(t.parentNode, e) : (this.lastLanding && this.lastLanding.dragLanding._dragOut(this.lastLanding, e.clientX, e.clientY, e), this.lastLanding = 0, this._onNotFound && this._onNotFound())
}, dhtmlDragAndDropObject.prototype.stopDrag = function (t, e) { if (dragger = window.dhtmlDragAndDrop, !e) { dragger.stopFrameRoute(); var n = dragger.lastLanding; dragger.lastLanding = null, n && n.dragLanding._drag(dragger.dragStartNode, dragger.dragStartObject, n, _isIE ? event.srcElement : t.target) } dragger.lastLanding = null, dragger.dragNode && dragger.dragNode.parentNode == document.body && dragger.dragNode.parentNode.removeChild(dragger.dragNode), dragger.dragNode = 0, dragger.gldragNode = 0, dragger.fx = 0, dragger.fy = 0, dragger.dragStartNode = 0, dragger.dragStartObject = 0, document.body.onmouseup = dragger.tempDOMU, document.body.onmousemove = dragger.tempDOMM, dragger.tempDOMU = null, dragger.tempDOMM = null, dragger.waitDrag = 0 }, dhtmlDragAndDropObject.prototype.stopFrameRoute = function (t) { t && window.dhtmlDragAndDrop.stopDrag(1, 1); for (var e = 0; e < window.frames.length; e++) try { window.frames[e] != t && window.frames[e].dhtmlDragAndDrop && window.frames[e].dhtmlDragAndDrop.stopFrameRoute(window) } catch (n) { } try { parent.dhtmlDragAndDrop && parent != window && parent != t && parent.dhtmlDragAndDrop.stopFrameRoute(window) } catch (n) { } }, dhtmlDragAndDropObject.prototype.initFrameRoute = function (t, e) { t && (window.dhtmlDragAndDrop.preCreateDragCopy(), window.dhtmlDragAndDrop.dragStartNode = t.dhtmlDragAndDrop.dragStartNode, window.dhtmlDragAndDrop.dragStartObject = t.dhtmlDragAndDrop.dragStartObject, window.dhtmlDragAndDrop.dragNode = t.dhtmlDragAndDrop.dragNode, window.dhtmlDragAndDrop.gldragNode = t.dhtmlDragAndDrop.dragNode, window.document.body.onmouseup = window.dhtmlDragAndDrop.stopDrag, window.waitDrag = 0, !_isIE && e && (!_isFF || 1.8 > _FFrv) && window.dhtmlDragAndDrop.calculateFramePosition()); try { parent.dhtmlDragAndDrop && parent != window && parent != t && parent.dhtmlDragAndDrop.initFrameRoute(window) } catch (n) { } for (var i = 0; i < window.frames.length; i++) try { window.frames[i] != t && window.frames[i].dhtmlDragAndDrop && window.frames[i].dhtmlDragAndDrop.initFrameRoute(window, !t || e ? 1 : 0) } catch (n) { } }, _isFF = !1, _isIE = !1, _isOpera = !1, _isKHTML = !1, _isMacOS = !1, _isChrome = !1, _FFrv = !1, _KHTMLrv = !1, _OperaRv = !1, -1 != navigator.userAgent.indexOf("Macintosh") && (_isMacOS = !0), navigator.userAgent.toLowerCase().indexOf("chrome") > -1 && (_isChrome = !0), -1 != navigator.userAgent.indexOf("Safari") || -1 != navigator.userAgent.indexOf("Konqueror") ? (_KHTMLrv = parseFloat(navigator.userAgent.substr(navigator.userAgent.indexOf("Safari") + 7, 5)), _KHTMLrv > 525 ? (_isFF = !0, _FFrv = 1.9) : _isKHTML = !0) : -1 != navigator.userAgent.indexOf("Opera") ? (_isOpera = !0, _OperaRv = parseFloat(navigator.userAgent.substr(navigator.userAgent.indexOf("Opera") + 6, 3))) : -1 != navigator.appName.indexOf("Microsoft") ? (_isIE = !0, -1 == navigator.appVersion.indexOf("MSIE 8.0") && -1 == navigator.appVersion.indexOf("MSIE 9.0") && -1 == navigator.appVersion.indexOf("MSIE 10.0") || "BackCompat" == document.compatMode || (_isIE = 8)) : (_isFF = !0, _FFrv = parseFloat(navigator.userAgent.split("rv:")[1])), dtmlXMLLoaderObject.prototype.doXPath = function (t, e, n, i) { if (_isKHTML || !_isIE && !window.XPathResult) return this.doXPathOpera(t, e); if (_isIE) return e || (e = this.xmlDoc.nodeName ? this.xmlDoc : this.xmlDoc.responseXML), e || dhtmlxError.throwError("LoadXML", "Incorrect XML", [e || this.xmlDoc, this.mainObject]), null != n && e.setProperty("SelectionNamespaces", "xmlns:xsl='" + n + "'"), "single" == i ? e.selectSingleNode(t) : e.selectNodes(t) || new Array(0); var a = e; e || (e = this.xmlDoc.nodeName ? this.xmlDoc : this.xmlDoc.responseXML), e || dhtmlxError.throwError("LoadXML", "Incorrect XML", [e || this.xmlDoc, this.mainObject]), -1 != e.nodeName.indexOf("document") ? a = e : (a = e, e = e.ownerDocument); var s = XPathResult.ANY_TYPE; "single" == i && (s = XPathResult.FIRST_ORDERED_NODE_TYPE); var r = new Array, o = e.evaluate(t, a, function () { return n }, s, null); if (s == XPathResult.FIRST_ORDERED_NODE_TYPE) return o.singleNodeValue; for (var d = o.iterateNext() ; d;) r[r.length] = d, d = o.iterateNext(); return r }, _dhtmlxError.prototype.catchError = function (t, e) { this.catches[t] = e }, _dhtmlxError.prototype.throwError = function (t, e, n) { return this.catches[t] ? this.catches[t](t, e, n) : this.catches.ALL ? this.catches.ALL(t, e, n) : (alert("Error type: " + arguments[0] + "\nDescription: " + arguments[1]), null) }, window.dhtmlxError = new _dhtmlxError, dtmlXMLLoaderObject.prototype.doXPathOpera = function (t, e) { var n = t.replace(/[\/]+/gi, "/").split("/"), i = null, a = 1; if (!n.length) return []; if ("." == n[0]) i = [e]; else { if ("" != n[0]) return []; i = (this.xmlDoc.responseXML || this.xmlDoc).getElementsByTagName(n[a].replace(/\[[^\]]*\]/g, "")), a++ } for (a; a < n.length; a++) i = this._getAllNamedChilds(i, n[a]); return -1 != n[a - 1].indexOf("[") && (i = this._filterXPath(i, n[a - 1])), i }, dtmlXMLLoaderObject.prototype._filterXPath = function (t, e) { for (var n = new Array, e = e.replace(/[^\[]*\[\@/g, "").replace(/[\[\]\@]*/g, ""), i = 0; i < t.length; i++) t[i].getAttribute(e) && (n[n.length] = t[i]); return n }, dtmlXMLLoaderObject.prototype._getAllNamedChilds = function (t, e) { var n = new Array; _isKHTML && (e = e.toUpperCase()); for (var i = 0; i < t.length; i++) for (var a = 0; a < t[i].childNodes.length; a++) _isKHTML ? t[i].childNodes[a].tagName && t[i].childNodes[a].tagName.toUpperCase() == e && (n[n.length] = t[i].childNodes[a]) : t[i].childNodes[a].tagName == e && (n[n.length] = t[i].childNodes[a]); return n }, dtmlXMLLoaderObject.prototype.xslDoc = null, dtmlXMLLoaderObject.prototype.setXSLParamValue = function (t, e, n) { n || (n = this.xslDoc), n.responseXML && (n = n.responseXML); var i = this.doXPath("/xsl:stylesheet/xsl:variable[@name='" + t + "']", n, "http://www.w3.org/1999/XSL/Transform", "single"); null != i && (i.firstChild.nodeValue = e) }, dtmlXMLLoaderObject.prototype.doXSLTransToObject = function (t, e) { if (t || (t = this.xslDoc), t.responseXML && (t = t.responseXML), e || (e = this.xmlDoc), e.responseXML && (e = e.responseXML), _isIE) { var n = new ActiveXObject("Msxml2.DOMDocument.3.0"); try { e.transformNodeToObject(t, n) } catch (i) { n = e.transformNode(t) } } else { this.XSLProcessor || (this.XSLProcessor = new XSLTProcessor, this.XSLProcessor.importStylesheet(t)); var n = this.XSLProcessor.transformToDocument(e) } return n }, dtmlXMLLoaderObject.prototype.doXSLTransToString = function (t, e) { var n = this.doXSLTransToObject(t, e); return "string" == typeof n ? n : this.doSerialization(n) }, dtmlXMLLoaderObject.prototype.doSerialization = function (t) { if (t || (t = this.xmlDoc), t.responseXML && (t = t.responseXML), _isIE) return t.xml; var e = new XMLSerializer; return e.serializeToString(t) }, dhtmlxEventable = function (obj) { obj.attachEvent = function (t, e, n) { return t = "ev_" + t.toLowerCase(), this[t] || (this[t] = new this.eventCatcher(n || this)), t + ":" + this[t].addEvent(e) }, obj.callEvent = function (t, e) { return t = "ev_" + t.toLowerCase(), this[t] ? this[t].apply(this, e) : !0 }, obj.checkEvent = function (t) { return !!this["ev_" + t.toLowerCase()] }, obj.eventCatcher = function (obj) { var dhx_catch = [], z = function () { for (var t = !0, e = 0; e < dhx_catch.length; e++) if (null != dhx_catch[e]) { var n = dhx_catch[e].apply(obj, arguments); t = t && n } return t }; return z.addEvent = function (ev) { return "function" != typeof ev && (ev = eval(ev)), ev ? dhx_catch.push(ev) - 1 : !1 }, z.removeEvent = function (t) { dhx_catch[t] = null }, z }, obj.detachEvent = function (t) { if (0 != t) { var e = t.split(":"); this[e[0]].removeEvent(e[1]) } }, obj.detachAllEvents = function () { for (var t in this) 0 == t.indexOf("ev_") && (this.detachEvent(t), this[t] = null) }, obj = null }, window.dhtmlx || (window.dhtmlx = {}), function () { function t(t, e) { var i = t.callback; n(!1), t.box.parentNode.removeChild(t.box), c = t.box = null, i && i(e) } function e(e) { if (c) { e = e || event; var n = e.which || event.keyCode; return dhtmlx.message.keyboard && ((13 == n || 32 == n) && t(c, !0), 27 == n && t(c, !1)), e.preventDefault && e.preventDefault(), !(e.cancelBubble = !0) } } function n(t) { n.cover || (n.cover = document.createElement("DIV"), n.cover.onkeydown = e, n.cover.className = "dhx_modal_cover", document.body.appendChild(n.cover)), document.body.scrollHeight, n.cover.style.display = t ? "inline-block" : "none" } function i(t, e) { var n = "dhtmlx_" + t.toLowerCase().replace(/ /g, "_") + "_button"; return "<div class='dhtmlx_popup_button " + n + "' result='" + e + "' ><div>" + t + "</div></div>" } function a(t) { g.area || (g.area = document.createElement("DIV"), g.area.className = "dhtmlx_message_area", g.area.style[g.position] = "5px", document.body.appendChild(g.area)), g.hide(t.id); var e = document.createElement("DIV"); return e.innerHTML = "<div>" + t.text + "</div>", e.className = "dhtmlx-info dhtmlx-" + t.type, e.onclick = function () { g.hide(t.id), t = null }, "bottom" == g.position && g.area.firstChild ? g.area.insertBefore(e, g.area.firstChild) : g.area.appendChild(e), t.expire > 0 && (g.timers[t.id] = window.setTimeout(function () { g.hide(t.id) }, t.expire)), g.pull[t.id] = e, e = null, t.id } function s(e, n, a) { var s = document.createElement("DIV"); s.className = " dhtmlx_modal_box dhtmlx-" + e.type, s.setAttribute("dhxbox", 1); var r = ""; if (e.width && (s.style.width = e.width), e.height && (s.style.height = e.height), e.title && (r += '<div class="dhtmlx_popup_title">' + e.title + "</div>"), r += '<div class="dhtmlx_popup_text"><span>' + (e.content ? "" : e.text) + '</span></div><div  class="dhtmlx_popup_controls">', n && (r += i(e.ok || "OK", !0)), a && (r += i(e.cancel || "Cancel", !1)), e.buttons) for (var o = 0; o < e.buttons.length; o++) r += i(e.buttons[o], o); if (r += "</div>", s.innerHTML = r, e.content) { var d = e.content; "string" == typeof d && (d = document.getElementById(d)), "none" == d.style.display && (d.style.display = ""), s.childNodes[e.title ? 1 : 0].appendChild(d) } return s.onclick = function (n) { n = n || event; var i = n.target || n.srcElement; if (i.className || (i = i.parentNode), "dhtmlx_popup_button" == i.className.split(" ")[0]) { var a = i.getAttribute("result"); a = "true" == a || ("false" == a ? !1 : a), t(e, a) } }, e.box = s, (n || a) && (c = e), s } function r(t, i, a) { var r = t.tagName ? t : s(t, i, a); t.hidden || n(!0), document.body.appendChild(r); var o = Math.abs(Math.floor(((window.innerWidth || document.documentElement.offsetWidth) - r.offsetWidth) / 2)), d = Math.abs(Math.floor(((window.innerHeight || document.documentElement.offsetHeight) - r.offsetHeight) / 2)); return r.style.top = "top" == t.position ? "-3px" : d + "px", r.style.left = o + "px", r.onkeydown = e, r.focus(), t.hidden && dhtmlx.modalbox.hide(r), r } function o(t) { return r(t, !0, !1) } function d(t) { return r(t, !0, !0) } function l(t) { return r(t) } function h(t, e, n) { return "object" != typeof t && ("function" == typeof e && (n = e, e = ""), t = { text: t, type: e, callback: n }), t } function _(t, e, n, i) { return "object" != typeof t && (t = { text: t, type: e, expire: n, id: i }), t.id = t.id || g.uid(), t.expire = t.expire || g.expire, t } var c = null; document.attachEvent ? document.attachEvent("onkeydown", e) : document.addEventListener("keydown", e, !0), dhtmlx.alert = function () { var t = h.apply(this, arguments); return t.type = t.type || "confirm", o(t) }, dhtmlx.confirm = function () { var t = h.apply(this, arguments); return t.type = t.type || "alert", d(t) }, dhtmlx.modalbox = function () { var t = h.apply(this, arguments); return t.type = t.type || "alert", l(t) }, dhtmlx.modalbox.hide = function (t) { for (; t && t.getAttribute && !t.getAttribute("dhxbox") ;) t = t.parentNode; t && (t.parentNode.removeChild(t), n(!1)) }; var g = dhtmlx.message = function (t) { t = _.apply(this, arguments), t.type = t.type || "info"; var e = t.type.split("-")[0]; switch (e) { case "alert": return o(t); case "confirm": return d(t); case "modalbox": return l(t); default: return a(t) } }; g.seed = (new Date).valueOf(), g.uid = function () { return g.seed++ }, g.expire = 4e3, g.keyboard = !0, g.position = "top", g.pull = {}, g.timers = {}, g.hideAll = function () { for (var t in g.pull) g.hide(t) }, g.hide = function (t) { var e = g.pull[t]; e && e.parentNode && (window.setTimeout(function () { e.parentNode.removeChild(e), e = null }, 2e3), e.className += " hidden", g.timers[t] && window.clearTimeout(g.timers[t]), delete g.pull[t]) } }(), gantt = { version: "2.0.0" }, dhtmlxEventable = function (obj) { obj._silent_mode = !1, obj._silentStart = function () { this._silent_mode = !0 }, obj._silentEnd = function () { this._silent_mode = !1 }, obj.attachEvent = function (t, e, n) { return t = "ev_" + t.toLowerCase(), this[t] || (this[t] = new this._eventCatcher(n || this)), t + ":" + this[t].addEvent(e) }, obj.callEvent = function (t, e) { return this._silent_mode ? !0 : (t = "ev_" + t.toLowerCase(), this[t] ? this[t].apply(this, e) : !0) }, obj.checkEvent = function (t) { return !!this["ev_" + t.toLowerCase()] }, obj._eventCatcher = function (obj) { var dhx_catch = [], z = function () { for (var t = !0, e = 0; e < dhx_catch.length; e++) if (null != dhx_catch[e]) { var n = dhx_catch[e].apply(obj, arguments); t = t && n } return t }; return z.addEvent = function (ev) { return "function" != typeof ev && (ev = eval(ev)), ev ? dhx_catch.push(ev) - 1 : !1 }, z.removeEvent = function (t) { dhx_catch[t] = null }, z }, obj.detachEvent = function (t) { if (0 != t) { var e = t.split(":"); this[e[0]].removeEvent(e[1]) } }, obj.detachAllEvents = function () { for (var t in this) 0 == t.indexOf("ev_") && delete this[t] }, obj = null }, dhtmlx.copy = function (t) { var e, n, i; if (t && "object" == typeof t) { for (i = {}, n = [Array, Date, Number, String, Boolean], e = 0; e < n.length; e++) t instanceof n[e] && (i = e ? new n[e](t) : new n[e]); for (e in t) Object.prototype.hasOwnProperty.apply(t, [e]) && (i[e] = dhtmlx.copy(t[e])) } return i || t }, dhtmlx.mixin = function (t, e, n) { for (var i in e) (!t[i] || n) && (t[i] = e[i]); return t }, dhtmlx.defined = function (t) { return "undefined" != typeof t }, dhtmlx.uid = function () { return this._seed || (this._seed = (new Date).valueOf()), this._seed++, this._seed }, dhtmlx.bind = function (t, e) { return function () { return t.apply(e, arguments) } }, gantt._get_position = function (t) { if (t.getBoundingClientRect) { var e = t.getBoundingClientRect(), n = document.body, i = document.documentElement, a = window.pageYOffset || i.scrollTop || n.scrollTop, s = window.pageXOffset || i.scrollLeft || n.scrollLeft, r = i.clientTop || n.clientTop || 0, o = i.clientLeft || n.clientLeft || 0, d = e.top + a - r, l = e.left + s - o; return { y: Math.round(d), x: Math.round(l), width: t.offsetHeight, height: t.offsetWidth } } for (var d = 0, l = 0; t;) d += parseInt(t.offsetTop, 10), l += parseInt(t.offsetLeft, 10), t = t.offsetParent; return { y: d, x: l, width: t.offsetHeight, height: t.offsetWidth } }, gantt._detectScrollSize = function () { var t = document.createElement("div"); t.style.cssText = "visibility:hidden;position:absolute;left:-1000px;width:100px;padding:0px;margin:0px;height:110px;min-height:100px;overflow-y:scroll;", document.body.appendChild(t); var e = t.offsetWidth - t.clientWidth; return document.body.removeChild(t), e }, dhtmlxEventable(gantt), gantt._click = {}, gantt._dbl_click = {}, gantt._context_menu = {}, gantt._on_click = function (t) { t = t || window.event; var e = t.target || t.srcElement, n = gantt.locate(t), i = gantt._find_ev_handler(t, e, gantt._click, n); if (i) if (null !== n) { var a = !gantt.checkEvent("onTaskClick") || gantt.callEvent("onTaskClick", [n, t]); a && gantt.config.select_task && gantt.selectTask(n) } else gantt.callEvent("onEmptyClick", [t]) }, gantt._on_contextmenu = function (t) { t = t || window.event; var e = t.target || t.srcElement, n = gantt.locate(e), i = gantt.locate(e, gantt.config.link_attribute), a = !gantt.checkEvent("onContextMenu") || gantt.callEvent("onContextMenu", [n, i, t]); return a || t.preventDefault(), a }, gantt._find_ev_handler = function (t, e, n, i) { for (var a = !0; e && e.parentNode;) { var s = e.className; if (s) { s = s.split(" "); for (var r = 0; r < s.length; r++) s[r] && n[s[r]] && (a = n[s[r]].call(gantt, t, i, e), a = !("undefined" != typeof a && a !== !0)) } e = e.parentNode } return a }, gantt._on_dblclick = function (t) { t = t || window.event; var e = t.target || t.srcElement, n = gantt.locate(t), i = gantt._find_ev_handler(t, e, gantt._dbl_click, n); if (i && null !== n) { var a = !gantt.checkEvent("onTaskDblClick") || gantt.callEvent("onTaskDblClick", [n, t]); a && gantt.config.details_on_dblclick && gantt.showLightbox(n) } }, gantt._on_mousemove = function (t) { if (gantt.checkEvent("onMouseMove")) { var e = gantt.locate(t); gantt._last_move_event = t, gantt.callEvent("onMouseMove", [e, t]) } }, dhtmlxDnD.prototype = { dragStart: function (t, e) { this.config = { obj: t, marker: null, started: !1, pos: this.getPosition(e), sensitivity: 4 }, this._settings && dhtmlx.mixin(this.config, this._settings, !0); var n = dhtmlx.bind(function (e) { return this.dragMove(t, e) }, this); dhtmlx.bind(function (e) { return this.dragScroll(t, e) }, this); var i = dhtmlx.bind(function (t) { return dhtmlx.defined(this.config.updates_per_second) && !gantt._checkTimeout(this, this.config.updates_per_second) ? !0 : n(t) }, this), a = dhtmlx.bind(function () { return dhtmlxDetachEvent(document.body, "mousemove", i), dhtmlxDetachEvent(document.body, "mouseup", a), this.dragEnd(t) }, this); dhtmlxEvent(document.body, "mousemove", i), dhtmlxEvent(document.body, "mouseup", a), document.body.className += " gantt_noselect" }, dragMove: function (t, e) { if (!this.config.marker && !this.config.started) { var n = this.getPosition(e), i = n.x - this.config.pos.x, a = n.y - this.config.pos.y, s = Math.sqrt(Math.pow(Math.abs(i), 2) + Math.pow(Math.abs(a), 2)); if (s > this.config.sensitivity) { if (this.config.started = !0, this.config.ignore = !1, this.callEvent("onBeforeDragStart", [t, e]) === !1) return this.config.ignore = !0, !0; var r = this.config.marker = document.createElement("div"); r.className = "gantt_drag_marker", r.innerHTML = "Dragging object", document.body.appendChild(r), this.callEvent("onAfterDragStart", [t, e]) } else this.config.ignore = !0 } this.config.ignore || (e.pos = this.getPosition(e), this.config.marker.style.left = e.pos.x + "px", this.config.marker.style.top = e.pos.y + "px", this.callEvent("onDragMove", [t, e])) }, dragEnd: function () { this.config.marker && (this.config.marker.parentNode.removeChild(this.config.marker), this.config.marker = null, this.callEvent("onDragEnd", [])), document.body.className = document.body.className.replace(" gantt_noselect", "") }, getPosition: function (t) { var e = 0, n = 0; return t = t || window.event, t.pageX || t.pageY ? (e = t.pageX, n = t.pageY) : (t.clientX || t.clientY) && (e = t.clientX + document.body.scrollLeft + document.documentElement.scrollLeft, n = t.clientY + document.body.scrollTop + document.documentElement.scrollTop), { x: e, y: n } } }, gantt._init_grid = function () {
    this._click.gantt_close = dhtmlx.bind(function (t, e) { this.close(e) }, this), this._click.gantt_open = dhtmlx.bind(function (t, e) { this.open(e) }, this), this._click.gantt_row = dhtmlx.bind(
        function (t, e, n) {
            if (null !== e) {
                var i = this.getTaskNode(e);
                var a = Math.max(i.offsetLeft - this.config.task_scroll_offset, 0);
                this._scroll_task_area(a), this.callEvent("onTaskRowClick", [e, n])
            }
        }, this), this._click.gantt_grid_head_cell = dhtmlx.bind(function (t, e, n) { var i = n.getAttribute("column_id"); if (this.callEvent("onGridHeaderClick", [i, t])) if ("add" == i) this._click.gantt_add(t, 0); else if (this.config.sort) { var a = this._sort && this._sort.direction && this._sort.name == i ? this._sort.direction : "desc"; a = "desc" == a ? "asc" : "desc", this._sort = { name: i, direction: a }, this._render_grid_header(), this.sort(i, "desc" == a) } }, this), !this.config.sort && this.config.order_branch && this._init_dnd() //, this._click.gantt_add = dhtmlx.bind(function (t, e) { var n = e ? this.getTask(e) : !1, i = ""; if (n) i = n.start_date; else { var a = this._order[0]; i = a ? this.getTask(a).start_date : this.getState().min_date } n && (n.$open = !0); var s = { text: gantt.locale.labels.new_task, start_date: this.templates.xml_format(i), duration: 1, progress: 0, parent: e }; this.callEvent("onTaskCreated", [s]), this.addTask(s), this.showTask(s.id), this.config.details_on_create && this.showLightbox(s.id) }, this)
}, gantt._render_grid = function ()
{ this._calc_grid_width(), this._render_grid_header() },
gantt._calc_grid_width = function () {
    if (this.config.autofit) {
        for (var t = this.config.columns, e = 0, n = [], i = [], a = 0; a < t.length; a++)
        { var s = parseInt(t[a].width, 10); window.isNaN(s) && (s = 50, n.push(a)), i[a] = s, e += s }
        var r = this.config.grid_width - e; if (r / (n.length > 0 ? n.length : i.length > 0 ? i.length : 1), n.length > 0)
            for (var o = r / (n.length ? n.length : 1), a = 0; a < n.length; a++)
            { var d = n[a]; i[d] += o } else for (var o = r / (i.length ? i.length : 1), a = 0; a < i.length; a++) i[a] += o;
        for (var a = 0; a < i.length; a++) t[a].width = i[a]
    }
}, gantt._render_grid_header = function () {
    for (var t = this.config.columns, e = [], n = 0, i = this.locale.labels,
      a = this.config.scale_height - 2, s = 0; s < t.length; s++) {
        var r = s == t.length - 1, o = t[s]; r && this.config.grid_width > n + o.width && (o.width = this.config.grid_width - n), n += o.width;
        var d = this._sort && o.name == this._sort.name ? "<div class='gantt_sort gantt_" + this._sort.direction + "'></div>" : "",
            l = "gantt_grid_head_cell" + (" gantt_grid_head_" + o.name) + (r ? " gantt_last_cell" : "") + this.templates.grid_header_class(o.name, o),
            h = "width:" + (o.width - (r ? 1 : 0)) + "px;", _ = o.label || i["column_" + o.name]; _ = _ || ""; var c = "<div class='" + l + "' style='" + h + "' column_id='" + o.name + "'>" + _ + d + "</div>";
        e.push(c)
    } this.$grid_scale.style.height = this.config.scale_height - 1 + "px", this.$grid_scale.style.lineHeight = a + "px", this.$grid_scale.style.width = n - 1 + "px", this.$grid_scale.innerHTML = e.join("")
},
gantt._render_grid_item = function (t) {
    for (var e = this.config.columns, n = [], i = 0; i < e.length; i++) {

        var a, s, r = i == e.length - 1, o = e[i]; "add" == o.name && i == e.length - 1 ? s = "<div id='" + t.type + "' class='gantt_add' Name='" + t.id + "' title='" + t.text + "'></div>  " : (s = o.template ? o.template(t) : t[o.name],
        s instanceof Date && (s = this.templates.date_grid(s)), s = "<div class='gantt_tree_content'>" + s + "</div>");
        var d = "gantt_cell" + (r ? " gantt_last_cell" : ""), l = ""; if (o.tree) {
            for (var h = 0; h < t.$level; h++) l += this.templates.grid_indent(t);
            var _ = this._branches[t.id] && this._branches[t.id].length > 0; _ ? (l += this.templates.grid_open(t), l += this.templates.grid_folder(t)) : (l += this.templates.grid_blank(t),
            l += this.templates.grid_file(t))

        } var c = "width:" + (o.width - (r ? 1 : 0)) + "px;"; dhtmlx.defined(o.align) && (c += "text-align:" + o.align + ";"), a = "<div class='" + d + "' style='" + c + "'>" + l + s + "</div>", n.push(a)
    } var d = 0 === t.$index % 2 ? "" : " odd"; if (d += t.$transparent ? " gantt_transparent" : "", this.templates.grid_row_class) { var g = this.templates.grid_row_class.call(this, t.start_date, t.end_date, t); g && (d += " " + g) } this.getState().selected_task == t.id && (d += " gantt_selected");


    var u = document.createElement("div"); return u.className = "gantt_row" + d, u.style.height = this.config.row_height + "px", u.style.lineHeight = gantt.config.row_height + "px", u.setAttribute(this.config.task_attribute, t.id), u.innerHTML = n.join(""), u
}, gantt.open = function (t) { gantt._set_item_state(t, !0), this.callEvent("onTaskOpened", [t]) }, gantt.close = function (t) { gantt._set_item_state(t, !1), this.callEvent("onTaskClosed", [t]) }, gantt._set_item_state = function (t, e) { t && this._pull[t] && (this._pull[t].$open = e, this.refreshData()) }, gantt.getTaskIndex = function (t) { for (var e = this._branches[this.getTask(t).parent], n = 0; n < e.length; n++) if (e[n] == t) return n; return -1 }, gantt.getGlobalTaskIndex = function (t) { for (var e = this._order, n = 0; n < e.length; n++) if (e[n] == t) return n; return -1 }, gantt.moveTask = function (t, e, n) {
    var i = arguments[3]; if (i) { if (i === t) return; n = this.getTask(i).parent, e = this.getTaskIndex(i) } n = n || 0; var a = this.getTask(t); this._branches[a.parent]; var s = this._branches[n]; if (-1 == e && (e = s.length + 1), a.parent == n) { var r = this.getTaskIndex(t); if (r == e) return; e > r && e-- } this._branch_update(a.parent, t), s = this._branches[n]; var o = s[e]; o ? s = s.slice(0, e).concat([t]).concat(s.slice(e)) : s.push(t), a.parent = n, this._branches[n] = s, this.refreshData()
}, gantt._init_dnd = function () {
    var t = new dhtmlxDnD(this.$grid_data, {
        updates_per_second: 60
    });
    dhtmlx.defined(this.config.dnd_sensitivity) && (t.config.sensitivity = this.config.dnd_sensitivity), t.attachEvent("onBeforeDragStart", dhtmlx.bind(function (t, e) {
        var n = this._locateHTML(e);
        return n ? (this.hideQuickInfo && this._hideQuickInfo(), void 0) : !1
    }, this)), t.attachEvent("onAfterDragStart", dhtmlx.bind(function (e, n) {
        var i = this._locateHTML(n);
        t.config.marker.innerHTML = i.outerHTML, t.config.id = this.locate(n);
        var a = this.getTask(t.config.id);
        a.$open = !1, a.$transparent = !0, this.refreshData()
    }, this)), t.attachEvent("onDragMove", dhtmlx.bind(function (e, n) {
        var i = t.config,
            a = this._get_position(this.$grid_data),
            s = a.x + 10,
            r = n.pos.y - 10;
        r < a.y && (r = a.y), r > a.y + this.$grid_data.offsetHeight - this.config.row_height && (r = a.y + this.$grid_data.offsetHeight - this.config.row_height), i.marker.style.left = s + "px", i.marker.style.top = r + "px";
        var o = document.elementFromPoint(a.x - document.body.scrollLeft + 1, r - document.body.scrollTop),
            d = this.locate(o);
        if (this.isTaskExists(d)) {
            var l = gantt._get_position(o),
                h = this.getTask(d),
                _ = this.getTask(t.config.id);
            if (l.y + o.offsetHeight / 2 < r) {
                var c = this.getGlobalTaskIndex(h.id),
                    g = this._pull[this._order[c + 1 + (h.id == _.id ? 1 : 0)]];
                if (g) {
                    if (g.id == _.id) return;
                    h = g
                } else if (g = this._pull[this._order[c + 1]], g.$level == _.$level) return this.moveTask(_.id, -1, g.parent), i.target = "next:" + g.id, void 0
            }
            if (h.$level == _.$level && _.id != h.id) this.moveTask(_.id, 0, 0, h.id), i.target = h.id;
            else {
                if (_.id == h.id) return;
                var c = this.getGlobalTaskIndex(h.id),
                    u = this._pull[this._order[c - 1]];
                u && u.$level == _.$level && _.id != u.id && (this.moveTask(_.id, -1, u.parent), i.target = "next:" + u.id)
            }
        }
        return !0
    }, this)), t.attachEvent("onDragEnd", dhtmlx.bind(function () {
        this.getTask(t.config.id).$transparent = !1, this.refreshData(), this.callEvent("onRowDragEnd", [t.config.id, t.config.target])
    }, this))
}, gantt._scale_helpers = {
    getSum: function (t, e, n) {
        void 0 === n && (n = t.length - 1), void 0 === e && (e = 0);
        for (var i = 0, a = e; n >= a; a++) i += t[a];
        return i
    },
    setSumWidth: function (t, e, n, i) {
        var a = e.width;
        void 0 === i && (i = a.length - 1), void 0 === n && (n = 0);
        var s = i - n + 1;
        if (!(n > a.length - 1 || 0 >= s || i > a.length - 1) && t) {
            var r = this.getSum(a, n, i),
                o = t - r;
            this.adjustSize(o, a, n, i), this.adjustSize(-o, a, i + 1), e.full_width = this.getSum(a)
        }
    },
    splitSize: function (t, e) {
        for (var n = [], i = 0; e > i; i++) n[i] = 0;
        return this.adjustSize(t, n), n
    },
    adjustSize: function (t, e, n, i) {
        n || (n = 0), void 0 === i && (i = e.length - 1);
        for (var a = i - n + 1, s = this.getSum(e, n, i), r = 0, o = n; i >= o; o++) {
            var d = Math.floor(t * (s ? e[o] / s : 1 / a));
            e[o] += d, r += d
        }
        e[e.length - 1] += t - r
    },
    sortScales: function (t) {
        function e(t, e) {
            var n = new Date(1970, 0, 1);
            return gantt.date.add(n, e, t) - n
        }
        t.sort(function (t, n) {
            return e(t.unit, t.step) < e(n.unit, n.step) ? 1 : -1
        })
    },
    primaryScale: function () {
        return gantt._init_template("date_scale"), {
            unit: gantt.config.scale_unit,
            step: gantt.config.step,
            template: gantt.templates.date_scale,
            date: gantt.config.date_scale,
            css: gantt.templates.scale_cell_class
        }
    },
    prepareConfigs: function (t, e, n, i) {
        for (var a = this.prepareScaleConfig(t[t.length - 1], e, n).full_width, s = this.splitSize(i, t.length), r = [], o = 0; o < t.length; o++) {
            var d = this.prepareScaleConfig(t[o], e, a, s[o]);
            this.formatScales(d), r.push(d)
        }
        for (var o = 0; o < r.length - 1; o++) this.alineScaleColumns(r[r.length - 1], r[o]);
        return r
    },
    prepareScaleConfig: function (t, e, n, i) {
        var a = dhtmlx.mixin({
            count: 0,
            col_width: 0,
            full_width: 0,
            height: i,
            width: [],
            trace_x: []
        }, t);
        this.eachColumn(t.unit, t.step, function (t) {
            a.count++, a.trace_x.push(new Date(t))
        });
        var s = n;
        return a.col_width = Math.floor(s / a.count), e && a.col_width < e && (a.col_width = e, s = a.col_width * a.count), a.width = this.splitSize(s, a.count), a.full_width = this.getSum(a.width), a
    },
    alineScaleColumns: function (t, e, n, i) {
        for (var a = e.trace_x, s = t.trace_x, r = n || 0, o = i || s.length - 1, d = 0, l = 1; l < a.length; l++)
            for (var h = r; o >= h; h++)
                if (+s[h] != +a[l]);
                else {
                    var _ = this.getSum(t.width, r, h - 1),
                        c = this.getSum(e.width, d, l - 1);
                    c != _ && this.setSumWidth(_, e, d, l - 1), r = h, d = l
                }
    },
    eachColumn: function (t, e, n) {
        var i = new Date(gantt._min_date),
            a = new Date(gantt._max_date);
        gantt.date[t + "_start"] && (i = gantt.date[t + "_start"](i));
        for (var s = new Date(i) ; +a > +s;) n.call(this, new Date(s)), s = gantt.date.add(s, e, t)
    },
    formatScales: function (t) {
        var e = t.trace_x,
            n = 0,
            i = t.width.length - 1,
            a = 0;
        if (+e[0] < +gantt._min_date && n != i) {
            var s = Math.floor(t.width[0] * ((e[1] - gantt._min_date) / (e[1] - e[0])));
            a += t.width[0] - s, t.width[0] = s, e[0] = new Date(gantt._min_date)
        }
        var r = e.length - 1,
            o = e[r],
            d = gantt.date.add(o, t.step, t.unit);
        if (+d > +gantt._max_date && r > 0) {
            var s = t.width[r] - Math.floor(t.width[r] * ((d - gantt._max_date) / (d - o)));
            a += t.width[r] - s, t.width[r] = s
        }
        if (a) {
            for (var l = this.getSum(t.width), h = 0, _ = 0; _ < t.width.length; _++) {
                var c = Math.floor(a * (t.width[_] / l));
                t.width[_] += c, h += c
            }
            this.adjustSize(a - h, t.width)
        }
    }
}, gantt._tasks_dnd = {
    drag: null,
    _events: {
        before_start: {},
        before_finish: {},
        after_finish: {}
    },
    _handlers: {},
    init: function () {
        this.clear_drag_state();
        var t = gantt.config.drag_mode;
        this.set_actions();
        var e = {
            before_start: "onBeforeTaskDrag",
            before_finish: "onBeforeTaskChanged",
            after_finish: "onAfterTaskDrag"
        };
        for (var n in this._events)
            for (var i in t) this._events[n][i] = e[n];
        this._handlers[t.move] = this._move, this._handlers[t.resize] = this._resize, this._handlers[t.progress] = this._resize_progress
    },
    set_actions: function () {
        var t = gantt.$task_data;
        dhtmlxEvent(t, "mousemove", dhtmlx.bind(function (t) {
            this.on_mouse_move(t || event)
        }, this)), dhtmlxEvent(t, "mousedown", dhtmlx.bind(function (t) {
            this.on_mouse_down(t || event)
        }, this)), dhtmlxEvent(t, "mouseup", dhtmlx.bind(function (t) {
            this.on_mouse_up(t || event)
        }, this))
    },
    clear_drag_state: function () {
        this.drag = {
            id: null,
            mode: null,
            pos: null,
            start: null,
            obj: null,
            left: null
        }
    },
    _drag_timeout: function () {
        return this._cancel_drag ? !1 : (setTimeout(dhtmlx.bind(function () {
            this._cancel_drag = !1
        }, this), 25), this._cancel_drag = !0)
    },
    _resize: function (t, e, n) {
        var i = gantt.config;
        n.left ? t.start_date = new Date(n.start.valueOf() + e) : t.end_date = new Date(n.start.valueOf() + e), t.end_date - t.start_date < i.min_duration && (n.left ? t.start_date = new Date(t.end_date.valueOf() - i.min_duration) : t.end_date = new Date(t.start_date.valueOf() + i.min_duration)), gantt._update_task_duration(t)
    },
    _resize_progress: function (t, e, n) {
        var i = n.obj_s_x = n.obj_s_x || gantt.posFromDate(t.start_date),
            a = n.obj_e_x = n.obj_e_x || gantt.posFromDate(t.end_date),
            s = Math.max(0, n.pos.x - i);
        t.progress = Math.min(1, s / (a - i))
    },
    _move: function (t, e, n) {
        t.start_date = new Date(n.obj.start_date.valueOf() + e), t.end_date = new Date(n.obj.end_date.valueOf() + e)
    },
    on_mouse_move: function (t) {
        this.drag.start_drag && this._start_dnd(t);
        var e = this.drag;
        if (e.mode) {
            if (!gantt._checkTimeout(this, 40)) return;
            this._update_on_move(t)
        }
    },
    _update_on_move: function (t) {
        var e = this.drag;
        if (e.mode) {
            var n = gantt._get_mouse_pos(t);
            if (e.pos && e.pos.x == n.x) return;
            e.pos = n;
            var i = gantt._date_from_pos(n.x);
            if (!i || isNaN(i.getTime())) return;
            var a = i - e.start,
                s = gantt.getTask(e.id);
            if (this._handlers[e.mode]) {
                var r = dhtmlx.mixin({}, s),
                    o = dhtmlx.mixin({}, s);
                this._handlers[e.mode].apply(this, [o, a, e]), dhtmlx.mixin(s, o, !0), gantt._update_parents(e.id, !0), gantt.callEvent("onTaskDrag", [s.id, e.mode, o, r, t]), dhtmlx.mixin(s, o, !0), gantt._update_parents(e.id), gantt.refreshTask(e.id)
            }
        }
    },
    on_mouse_down: function (t, e) {
        if (2 != t.button && !gantt.config.readonly && !this.drag.mode) {
            this.clear_drag_state(), e = e || t.target || t.srcElement;
            var n = gantt._trim(e.className || "");
            if (!n || !this._get_drag_mode(n)) return e.parentNode ? this.on_mouse_down(t, e.parentNode) : void 0;
            var i = this._get_drag_mode(n);
            if (i)
                if (i.mode && i.mode != gantt.config.drag_mode.ignore && gantt.config["drag_" + i.mode]) {
                    var a = gantt.locate(e),
                        s = dhtmlx.copy(gantt.getTask(a) || {});
                    if (gantt._is_flex_task(s) && i.mode != gantt.config.drag_mode.progress) return this.clear_drag_state(), void 0;
                    i.id = a;
                    var r = gantt._get_mouse_pos(t);
                    i.start = gantt._date_from_pos(r.x), i.obj = s, this.drag.start_drag = i
                } else this.clear_drag_state();
            else if (gantt.checkEvent("onMouseDown") && gantt.callEvent("onMouseDown", [n.split(" ")[0]]) && e.parentNode) return this.on_mouse_down(t, e.parentNode)
        }
    },
    on_mouse_up: function (t) {
        var e = this.drag;
        if (e.mode && e.id) {
            var n = gantt.getTask(e.id);
            if (this._fireEvent("before_finish", e.mode, [e.id, e.mode, dhtmlx.copy(e.obj), t])) {
                var i = e.id;
                if (this.clear_drag_state(), gantt.config.round_dnd_dates) {
                    var a = gantt._tasks;
                    gantt._round_task_dates(n, a.step, a.unit)
                } else gantt._round_task_dates(n, gantt.config.time_step, "minute");
                gantt._update_task_duration(n), gantt.updateTask(n.id), this._fireEvent("after_finish", e.mode, [i, e.mode, t])
            } else e.obj._dhx_changed = !1, dhtmlx.mixin(n, e.obj, !0), gantt.updateTask(n.id)
        }
        this.clear_drag_state()
    },
    _get_drag_mode: function (t) {
        var e = gantt.config.drag_mode,
            n = (t || "").split(" "),
            i = n[0],
            a = {
                mode: null,
                left: null
            };
        switch (i) {
            case "gantt_task_line":
            case "gantt_task_content":
                a.mode = e.move;
                break;
            case "gantt_task_drag":
                a.mode = e.resize, n[1] && -1 !== n[1].indexOf("left", n[1].length - "left".length) && (a.left = !0);
                break;
            case "gantt_task_progress_drag":
                a.mode = e.progress;
                break;
            case "gantt_link_control":
            case "gantt_link_point":
                a.mode = e.ignore;
                break;
            default:
                a = null
        }
        return a
    },
    _start_dnd: function (t) {
        var e = this.drag = this.drag.start_drag;
        delete e.start_drag;
        var n = gantt.config,
            i = e.id;
        n["drag_" + e.mode] && gantt.callEvent("onBeforeDrag", [i, e.mode, t]) && this._fireEvent("before_start", e.mode, [i, e.mode, t]) ? delete e.start_drag : this.clear_drag_state()
    },
    _fireEvent: function (t, e, n) {
        dhtmlx.assert(this._events[t], "Invalid stage:{" + t + "}");
        var i = this._events[t][e];
        return dhtmlx.assert(i, "Unknown after drop mode:{" + e + "}"), dhtmlx.assert(n, "Invalid event arguments"), gantt.checkEvent(i) ? gantt.callEvent(i, n) : !0
    }
}, gantt._render_link = function (t) {
    var e = this.getLink(t);
    gantt._linkRenderer.render_item(e, this.$task_links)
}, gantt._get_link_type = function (t, e) {
    var n;
    return t && e ? n = gantt.config.links.start_to_start : !t && e ? n = gantt.config.links.finish_to_start : t || e ? t && !e && (n = null) : n = gantt.config.links.finish_to_finish, n
}, gantt.isLinkAllowed = function (t, e, n, i) {
    if ("object" == typeof t) var a = t;
    else var a = {
        source: t,
        target: e,
        type: this._get_link_type(n, i)
    }; if (!a) return !1;
    if (!(a.source && a.target && a.type)) return !1;
    if (a.source == a.target) return !1;
    var s = !0;
    return this.checkEvent("onLinkValidation") && (s = this.callEvent("onLinkValidation", [a])), s
}, gantt._render_link_element = function (t) {
    if (gantt.isTaskVisible(t.source) && gantt.isTaskVisible(t.target)) {
        var e = this._path_builder.get_points(t),
            n = gantt._drawer,
            i = n.get_lines(e),
            a = document.createElement("div"),
            s = "gantt_task_link",
            r = this.templates.link_class ? this.templates.link_class(t) : "";
        r && (s += " " + r), a.className = s, a.setAttribute(gantt.config.link_attribute, t.id);
        for (var o = 0; o < i.length; o++) o == i.length - 1 && (i[o].size -= gantt.config.link_arrow_size), a.appendChild(n.render_line(i[o], i[o + 1]));
        var d = i[i.length - 1].direction,
            l = gantt._render_link_arrow(e[e.length - 1], d);
        return a.appendChild(l), a
    }
}, gantt._render_link_arrow = function (t, e) {
    var n = document.createElement("div"),
        i = gantt._drawer,
        a = t.y,
        s = t.x,
        r = gantt.config.link_arrow_size,
        o = gantt.config.row_height,
        d = "gantt_link_arrow gantt_link_arrow_" + e;
    switch (e) {
        case i.dirs.right:
            a -= (r - o) / 2, s -= r;
            break;
        case i.dirs.left:
            a -= (r - o) / 2;
            break;
        case i.dirs.up:
            s -= (r - o) / 2;
            break;
        case i.dirs.down:
            a -= r, s -= (r - o) / 2
    }
    return n.style.cssText = ["top:" + a + "px", "left:" + s + "px"].join(";"), n.className = d, n
}, gantt._drawer = {
    current_pos: null,
    dirs: {
        left: "left",
        right: "right",
        up: "up",
        down: "down"
    },
    path: [],
    clear: function () {
        this.current_pos = null, this.path = []
    },
    point: function (t) {
        this.current_pos = dhtmlx.copy(t)
    },
    get_lines: function (t) {
        this.clear(), this.point(t[0]);
        for (var e = 1; e < t.length; e++) this.line_to(t[e]);
        return this.get_path()
    },
    line_to: function (t) {
        var e = dhtmlx.copy(t),
            n = this.current_pos,
            i = this._get_line(n, e);
        this.path.push(i), this.current_pos = e
    },
    get_path: function () {
        return this.path
    },
    get_wrapper_sizes: function (t) {
        var e, n = gantt.config.link_wrapper_width,
            i = (gantt.config.link_line_width, t.y + (gantt.config.row_height - n) / 2);
        switch (t.direction) {
            case this.dirs.left:
                e = {
                    top: i,
                    height: n,
                    lineHeight: n,
                    left: t.x - t.size - n / 2,
                    width: t.size + n
                };
                break;
            case this.dirs.right:
                e = {
                    top: i,
                    lineHeight: n,
                    height: n,
                    left: t.x - n / 2,
                    width: t.size + n
                };
                break;
            case this.dirs.up:
                e = {
                    top: i - t.size,
                    lineHeight: t.size + n,
                    height: t.size + n,
                    left: t.x - n / 2,
                    width: n
                };
                break;
            case this.dirs.down:
                e = {
                    top: i,
                    lineHeight: t.size + n,
                    height: t.size + n,
                    left: t.x - n / 2,
                    width: n
                }
        }
        return e
    },
    get_line_sizes: function (t) {
        var e, n = gantt.config.link_line_width,
            i = gantt.config.link_wrapper_width,
            a = t.size + n;
        switch (t.direction) {
            case this.dirs.left:
            case this.dirs.right:
                e = {
                    height: n,
                    width: a,
                    marginTop: (i - n) / 2,
                    marginLeft: (i - n) / 2
                };
                break;
            case this.dirs.up:
            case this.dirs.down:
                e = {
                    height: a,
                    width: n,
                    marginTop: (i - n) / 2,
                    marginLeft: (i - n) / 2
                }
        }
        return e
    },
    render_line: function (t) {
        var e = this.get_wrapper_sizes(t),
            n = document.createElement("div");
        n.style.cssText = ["top:" + e.top + "px", "left:" + e.left + "px", "height:" + e.height + "px", "width:" + e.width + "px"].join(";"), n.className = "gantt_line_wrapper";
        var i = this.get_line_sizes(t),
            a = document.createElement("div");
        return a.style.cssText = ["height:" + i.height + "px", "width:" + i.width + "px", "margin-top:" + i.marginTop + "px", "margin-left:" + i.marginLeft + "px"].join(";"), a.className = "gantt_link_line_" + t.direction, n.appendChild(a), n
    },
    _get_line: function (t, e) {
        var n = this.get_direction(t, e),
            i = {
                x: t.x,
                y: t.y,
                direction: this.get_direction(t, e)
            };
        return i.size = n == this.dirs.left || n == this.dirs.right ? Math.abs(t.x - e.x) : Math.abs(t.y - e.y), i
    },
    get_direction: function (t, e) {
        var n = 0;
        return n = e.x < t.x ? this.dirs.left : e.x > t.x ? this.dirs.right : e.y > t.y ? this.dirs.down : this.dirs.up
    }
}, gantt._y_from_ind = function (t) {
    return t * gantt.config.row_height
}, gantt._path_builder = {
    path: [],
    clear: function () {
        this.path = []
    },
    current: function () {
        return this.path[this.path.length - 1]
    },
    point: function (t) {
        return t ? (this.path.push(dhtmlx.copy(t)), t) : this.current()
    },
    point_to: function (t, e, n) {
        n = n ? {
            x: n.x,
            y: n.y
        } : dhtmlx.copy(this.point());
        var i = gantt._drawer.dirs;
        switch (t) {
            case i.left:
                n.x -= e;
                break;
            case i.right:
                n.x += e;
                break;
            case i.up:
                n.y -= e;
                break;
            case i.down:
                n.y += e
        }
        return this.point(n)
    },
    get_points: function (t) {
        var e = this.get_endpoint(t),
            n = gantt.config,
            i = e.e_y - e.y,
            a = e.e_x - e.x,
            s = gantt._drawer.dirs;
        this.clear(), this.point({
            x: e.x,
            y: e.y
        });
        var r = 2 * n.link_arrow_size,
            o = e.e_x > e.x;
        if (t.type == gantt.config.links.start_to_start) this.point_to(s.left, r), o ? (this.point_to(s.down, i), this.point_to(s.right, a)) : (this.point_to(s.right, a), this.point_to(s.down, i)), this.point_to(s.right, r);
        else if (t.type == gantt.config.links.finish_to_start)
            if (o = e.e_x > e.x + 2 * r, this.point_to(s.right, r), o) a -= r, this.point_to(s.down, i), this.point_to(s.right, a);
            else {
                a -= 2 * r;
                var d = i > 0 ? 1 : -1;
                this.point_to(s.down, d * (n.row_height / 2)), this.point_to(s.right, a), this.point_to(s.down, d * (Math.abs(i) - n.row_height / 2)), this.point_to(s.right, r)
            } else t.type == gantt.config.links.finish_to_finish && (this.point_to(s.right, r), o ? (this.point_to(s.right, a), this.point_to(s.down, i)) : (this.point_to(s.down, i), this.point_to(s.right, a)), this.point_to(s.left, r));
        return this.path
    },
    get_endpoint: function (t) {
        var e, n, i = gantt.config.links,
            a = gantt._get_visible_order(t.source),
            s = gantt._get_visible_order(t.target);
        return t.type == i.start_to_start ? (e = gantt._pull[t.source].start_date, n = gantt._pull[t.target].start_date) : t.type == i.finish_to_finish ? (e = gantt._pull[t.source].end_date, n = gantt._pull[t.target].end_date) : t.type == i.finish_to_start ? (e = gantt._pull[t.source].end_date, n = gantt._pull[t.target].start_date) : dhtmlx.assert(!1, "Invalid link type"), {
            x: gantt.posFromDate(e),
            e_x: gantt.posFromDate(n),
            y: gantt._y_from_ind(a),
            e_y: gantt._y_from_ind(s)
        }
    }
}, gantt._init_links_dnd = function () {
    function t(t) {
        var e = n(),
            i = ["gantt_link_tooltip"];
        e.from && e.to && (gantt.isLinkAllowed(e.from, e.to, e.from_start, e.to_start) ? i.push("gantt_allowed_link") : i.push("gantt_invalid_link"));
        var a = gantt.templates.drag_link_class(e.from, e.from_start, e.to, e.to_start);
        a && i.push(a);
        var s = "<div class='" + a + "'>" + gantt.templates.drag_link(e.from, e.from_start, e.to, e.to_start) + "</div>";
        t.innerHTML = s
    }

    function e(t, e) {
        t.style.left = e.x + 5 + "px", t.style.top = e.y + 5 + "px"
    }

    function n() {
        return {
            from: gantt._link_source_task,
            to: gantt._link_target_task,
            from_start: gantt._link_source_task_start,
            to_start: gantt._link_target_task_start
        }
    }

    function i() {
        gantt._link_source_task = gantt._link_source_task_start = gantt._link_target_task = gantt._link_target_task_start = null
    }

    function a(t, e, i, a) {
        var d = o(),
            l = n(),
            h = ["gantt_link_direction"];
        gantt.templates.link_direction_class && h.push(gantt.templates.link_direction_class(l.from, l.from_start, l.to, l.to_start));
        var _ = Math.sqrt(Math.pow(i - t, 2) + Math.pow(a - e, 2));
        if (_ = Math.max(0, _ - 3)) {
            d.className = h.join(" ");
            var c = (a - e) / (i - t),
                g = Math.atan(c);
            2 == r(t, i, e, a) ? g += Math.PI : 3 == r(t, i, e, a) && (g -= Math.PI);
            var u = Math.sin(g),
                f = Math.cos(g),
                p = Math.round(e),
                m = Math.round(t),
                v = ["-webkit-transform: rotate(" + g + "rad)", "-moz-transform: rotate(" + g + "rad)", "-ms-transform: rotate(" + g + "rad)", "-o-transform: rotate(" + g + "rad)", "transform: rotate(" + g + "rad)", "width:" + Math.round(_) + "px"];
            if (-1 != window.navigator.userAgent.indexOf("MSIE 8.0")) {
                v.push('-ms-filter: "' + s(u, f) + '"');
                var x = Math.abs(Math.round(t - i)),
                    k = Math.abs(Math.round(a - e));
                switch (r(t, i, e, a)) {
                    case 1:
                        p -= k;
                        break;
                    case 2:
                        m -= x, p -= k;
                        break;
                    case 3:
                        m -= x
                }
            }
            v.push("top:" + p + "px"), v.push("left:" + m + "px"), d.style.cssText = v.join(";")
        }
    }

    function s(t, e) {
        return "progid:DXImageTransform.Microsoft.Matrix(M11 = " + e + "," + "M12 = -" + t + "," + "M21 = " + t + "," + "M22 = " + e + "," + "SizingMethod = 'auto expand'" + ")"
    }

    function r(t, e, n, i) {
        return e >= t ? n >= i ? 1 : 4 : n >= i ? 2 : 3
    }

    function o() {
        return l._direction || (l._direction = document.createElement("div"), gantt.$task_links.appendChild(l._direction)), l._direction
    }

    function d() {
        l._direction && (l._direction.parentNode.removeChild(l._direction), l._direction = null)
    }
    var l = new dhtmlxDnD(this.$task_bars, {
        sensitivity: 0,
        updates_per_second: 60
    }),
        h = "task_left",
        _ = "gantt_link_point",
        c = "gantt_link_control";
    l.attachEvent("onBeforeDragStart", dhtmlx.bind(function (t, e) {
        var n = e.target || e.srcElement;
        if (i(), gantt.getState().drag_id) return !1;
        if (gantt._locate_css(n, _)) {
            gantt._locate_css(n, h) && (gantt._link_source_task_start = !0);
            var a = gantt._link_source_task = this.locate(e),
                s = gantt.getTask(a);
            return this._dir_start = {
                y: gantt._y_from_ind(gantt._get_visible_order(a)) + gantt.config.row_height / 2,
                x: gantt.posFromDate(gantt._link_source_task_start ? s.start_date : s.end_date)
            }, !0
        }
        return !1
    }, this)), l.attachEvent("onAfterDragStart", dhtmlx.bind(function () {
        t(l.config.marker)
    }, this)), l.attachEvent("onDragMove", dhtmlx.bind(function (n, i) {
        var s = l.config,
            r = l.getPosition(i);
        e(s.marker, r);
        var o = gantt._is_link_drop_area(i),
            d = gantt._link_target_task,
            _ = gantt._link_landing,
            c = gantt._link_target_task_start,
            g = gantt.locate(i);
        if (o) var u = !1,
        u = !!gantt._locate_css(i, h), o = !!g;
        if (gantt._link_target_task = g, gantt._link_landing = o, gantt._link_target_task_start = u, o) {
            var f = gantt.getTask(g),
                p = Math.floor((i.srcElement || i.target).offsetWidth / 2);
            this._dir_end = {
                y: gantt._y_from_ind(gantt._get_visible_order(g)) + gantt.config.row_height / 2,
                x: gantt.posFromDate(u ? f.start_date : f.end_date) + (u ? -1 : 1) * p
            }
        } else this._dir_end = gantt._get_mouse_pos(i);
        var m = !(_ == o && d == g && c == u);
        return m && (d && gantt.refreshTask(d, !1), g && gantt.refreshTask(g, !1)), m && t(s.marker), a(this._dir_start.x, this._dir_start.y, this._dir_end.x, this._dir_end.y), !0
    }, this)), l.attachEvent("onDragEnd", dhtmlx.bind(function () {
        var t = n();
        if (t.from && t.to && t.from != t.to) {
            var e = gantt._get_link_type(t.from_start, t.to_start);
            e && gantt.addLink({
                source: t.from,
                target: t.to,
                type: e
            })
        }
        i(), t.from && gantt.refreshTask(t.from, !1), t.to && gantt.refreshTask(t.to, !1), d()
    }, this)), gantt._is_link_drop_area = function (t) {
        return !!gantt._locate_css(t, c)
    }
}, gantt._get_link_state = function () {
    return {
        link_landing_area: this._link_landing,
        link_target_id: this._link_target_task,
        link_target_start: this._link_target_task_start,
        link_source_id: this._link_source_task,
        link_source_start: this._link_source_task_start
    }
}, gantt._init_tasks = function () {
    function t(t, e, n, i) {
        for (var a = 0; a < t.length; a++) t[a].change_id(e, n), t[a].render_item(i)
    }
    this._tasks = {
        col_width: this.config.columnWidth,
        width: [],
        full_width: 0,
        trace_x: [],
        rendered: {}
    }, this._click.gantt_task_link = dhtmlx.bind(function (t) {
        var e = this.locate(t, gantt.config.link_attribute);
        e && this.callEvent("onLinkClick", [e, t])
    }, this), this._dbl_click.gantt_task_link = dhtmlx.bind(function (t, e) {
        var e = this.locate(t, gantt.config.link_attribute);
        this._delete_link_handler(e, t)
    }, this), this._dbl_click.gantt_link_point = dhtmlx.bind(function (t, e, n) {
        var e = this.locate(t),
            i = this.getTask(e),
            a = null;
        return n.parentNode && n.parentNode.className && (a = n.parentNode.className.indexOf("_left") > -1 ? i.$target[0] : i.$source[0]), a && this._delete_link_handler(a, t), !1
    }, this), this._tasks_dnd.init(), this._init_links_dnd(), this._taskRenderer = gantt._task_renderer("line", this._render_task_element, this.$task_bars), this._linkRenderer = gantt._task_renderer("links", this._render_link_element, this.$task_links), this._gridRenderer = gantt._task_renderer("grid_items", this._render_grid_item, this.$grid_data), this._bgRenderer = gantt._task_renderer("bg_lines", this._render_bg_line, this.$task_bg), this.attachEvent("onTaskIdChange", function (e, n) {
        var i = this._get_task_renderers();
        t(i, e, n, this.getTask(n))
    }), this.attachEvent("onLinkIdChange", function (e, n) {
        var i = this._get_link_renderers();
        t(i, e, n, this.getLink(n))
    })
}, gantt._get_task_renderers = function () {
    return [this._taskRenderer, this._gridRenderer, this._bgRenderer]
}, gantt._get_link_renderers = function () {
    return [this._linkRenderer]
}, gantt._delete_link_handler = function (t, e) {
    if (t && this.callEvent("onLinkDblClick", [t, e])) {
        var n = "",
            i = gantt.locale.labels.link + " " + this.templates.link_description(this.getLink(t)) + " " + gantt.locale.labels.confirm_link_deleting;
        window.setTimeout(function () {
            gantt._dhtmlx_confirm(i, n, function () {
                gantt.deleteLink(t)
            })
        }, gantt.config.touch ? 300 : 1)
    }
}, gantt.getTaskNode = function (t) {
    return this._taskRenderer.rendered[t]
}, gantt.getLinkNode = function (t) {
    return this._linkRenderer.rendered[t]
}, gantt._get_tasks_data = function () {
    for (var t = [], e = 0; e < this._order.length; e++) {
        var n = this._pull[this._order[e]];
        n.$index = e, this._update_parents(n.id, !0), t.push(n)
    }
    return t
}, gantt._get_links_data = function () {
    var t = [];
    for (var e in this._lpull) t.push(this._lpull[e]);
    return t
}, gantt._render_data = function () {
    this._update_layout_sizes();
    for (var t = this._get_tasks_data(), e = this._get_task_renderers(), n = 0; n < e.length; n++) e[n].render_items(t);
    var i = gantt._get_links_data();
    e = this._get_link_renderers();
    for (var n = 0; n < e.length; n++) e[n].render_items(i)
}, gantt._update_layout_sizes = function () {
    var t = this._tasks,
        e = this.config.task_height;
    "full" == e && (e = this.config.row_height - 5), e = Math.min(e, this.config.row_height), t.bar_height = e, this.$task_data.style.height = this.$task.offsetHeight - this.config.scale_height + "px", this.$task_bg.style.width = t.full_width + "px";
    for (var n = this.config.columns, i = 0, a = 0; a < n.length; a++) i += n[a].width;
    this.$grid_data.style.width = i - 1 + "px"
}, gantt._init_tasks_range = function () {
    var t = this.config.scale_unit;
    if (this.config.start_date && this.config.end_date) return this._min_date = this.date[t + "_start"](new Date(this.config.start_date)), this._max_date = this.date[t + "_start"](new Date(this.config.end_date)), void 0;
    var e = this._get_tasks_data(),
        n = this._init_task({
            id: 0
        });
    e.push(n);
    var i = -1 / 0,
        a = 1 / 0;
    this.eachTask(function (t) {
        t.end_date && +t.end_date > +i && (i = new Date(t.end_date))
    }, "0"), this.eachTask(function (t) {
        t.start_date && +t.start_date < +a && (a = new Date(t.start_date))
    }, "0"), this._min_date = a, this._max_date = i, i && i != -1 / 0 || (this._min_date = new Date, this._max_date = new Date(this._min_date)), this._min_date = this.date[t + "_start"](this._min_date), +this._min_date == +a && (this._min_date = this.date.add(this.date[t + "_start"](this._min_date), -1, t)), this._max_date = this.date[t + "_start"](this._max_date), this._max_date = this.date.add(this._max_date, 1, t)
}, gantt._prepare_scale_html = function (t) {
    var e = [],
        n = null,
        i = null,
        a = null;
    (t.template || t.date) && (i = t.template || this.date.date_to_str(t.date)), a = t.css || gantt.templates.scale_cell_class;
    for (var s = 0; s < t.count; s++) {
        n = new Date(t.trace_x[s]);
        var r = i.call(this, n),
            o = t.width[s],
            d = "width:" + o + "px;",
            l = "gantt_scale_cell" + (s == t.count - 1 ? " gantt_last_cell" : ""),
            h = a.call(this, n);
        h && (l += " " + h);
        var _ = "<div class='" + l + "' style='" + d + "'>" + r + "</div>";
        e.push(_)
    }
    return e.join("")
}, gantt._render_tasks_scales = function () {
    this._init_tasks_range(), this._scroll_resize(), this._set_sizes();
    var t = this._scale_helpers,
        e = [t.primaryScale()].concat(this.config.subscales);
    t.sortScales(e);
    for (var n = t.prepareConfigs(e, this.config.min_column_width, this.$task.offsetWidth, this.config.scale_height - 1), i = this._tasks = n[n.length - 1], a = [], s = this.templates.scale_row_class, r = 0; r < n.length; r++) {
        var o = "gantt_scale_line",
            d = s(n[r]);
        d && (o += " " + d), a.push('<div class="' + o + '" style="height:' + n[r].height + "px;line-height:" + n[r].height + 'px">' + this._prepare_scale_html(n[r]) + "</div>")
    }
    this.$task_scale.style.height = this.config.scale_height - 1 + "px", this.$task_data.style.width = this.$task_scale.style.width = i.full_width + this.$scroll_ver.offsetWidth + "px", this.$task_links.style.width = this.$task_bars.style.width = i.full_width + "px", e = a.join(""), this.$task_scale.innerHTML = e
}, gantt._render_bg_line = function (t) {
    for (var e = gantt._tasks, n = e.count, i = [], a = 0; n > a; a++) {
        var s = e.width[a],
            r = "width:" + s + "px;",
            o = "gantt_task_cell" + (a == n - 1 ? " gantt_last_cell" : "");
        h = this.templates.task_cell_class(t, e.trace_x[a]), h && (o += " " + h);
        var d = "<div class='" + o + "' style='" + r + "'></div>";
        i.push(d)
    }
    var l = 0 !== t.$index % 2,
        h = gantt.templates.task_row_class(t.start_date, t.end_date, t),
        _ = "gantt_task_row" + (l ? " odd" : "") + (h ? " " + h : "");
    this.getState().selected_task == t.id && (_ += " gantt_selected");
    var c = document.createElement("div");
    return c.className = _, c.style.height = gantt.config.row_height + "px", c.setAttribute(this.config.task_attribute, t.id), c.innerHTML = i.join(""), c
}, gantt._adjust_scales = function () {
    if (this.config.fit_tasks) {
        var t = +this._min_date,
            e = +this._max_date;
        if (this._init_tasks_range(), +this._min_date != t || +this._max_date != e) return this.render(), this.callEvent("onScaleAdjusted", []), !0
    }
    return !1
}, gantt.refreshTask = function (t, e) {
    var n = this._get_task_renderers(),
        i = this.getTask(t);
    if (i && this.isTaskVisible(t))
        for (var a = 0; a < n.length; a++) n[a].render_item(i);
    else
        for (var a = 0; a < n.length; a++) n[a].remove_item(t); if (void 0 === e || e) {
            for (var i = this.getTask(t), a = 0; a < i.$source.length; a++) gantt.refreshLink(i.$source[a]);
            for (var a = 0; a < i.$target.length; a++) gantt.refreshLink(i.$target[a])
        }
}, gantt.refreshLink = function (t) {
    this.isLinkExists(t) ? gantt._render_link(t) : gantt._linkRenderer.remove_item(t)
}, gantt._combine_item_class = function (t, e, n) {
    var i = [t];
    e && i.push(e);
    var a = gantt.getState();
    this._is_flex_task(this.getTask(n)) && i.push("gantt_dependent_task"), this.config.select_task && n == a.selected_task && i.push("gantt_selected"), n == a.drag_id && i.push("gantt_drag_" + a.drag_mode);
    var s = gantt._get_link_state();
    if (s.link_source_id == n && i.push("gantt_link_source"), s.link_target_id == n && i.push("gantt_link_target"), s.link_landing_area && s.link_target_id && s.link_source_id && s.link_target_id != s.link_source_id) {
        var r = s.link_source_id,
            o = s.link_source_start,
            d = s.link_target_start,
            l = gantt.isLinkAllowed(r, n, o, d),
            h = "";
        h = l ? d ? "link_start_allow" : "link_finish_allow" : d ? "link_start_deny" : "link_finish_deny", i.push(h)
    }
    return i.join(" ")
}, gantt._render_pair = function (t, e, n, i) {
    var a = gantt.getState(); +n.end_date <= +a.max_date && t.appendChild(i(e + " task_right")), +n.start_date >= +a.min_date && t.appendChild(i(e + " task_left"))
}, gantt._render_task_element = function (t) {
    if (!(+t.start_date < +this._max_date && +t.end_date > +this._min_date)) return !1;
    var e = this._get_task_coord(t),
        n = this.posFromDate(t.end_date),
        i = this.config,
        a = this._tasks.bar_height;
    a = Math.min(a, this.config.row_height);
    var s = Math.floor((this.config.row_height - a) / 2),
        r = document.createElement("div"),
        o = Math.round(n - e.x);
    var cssfortactic = t.colorcode;
    r.setAttribute(this.config.task_attribute, t.id), r.appendChild(gantt._render_task_content(t, o)), r.className = this._combine_item_class("gantt_task_line", this.templates.task_class(t.start_date, t.end_date, t), t.id), t.isImprovement != true ? r.style.cssText = ["left:" + e.x + "px", "top:" + (s + e.y) + "px", "height:" + a + "px", "line-height:" + a + "px", "width:" + o + "px", "background-color:#" + t.colorcode, "border-left: none", "border-right: none", "border-top: none", "border-bottom: none"].join(";") : r.style.cssText = ["left:" + e.x + "px", "top:" + (s + e.y) + "px", "height:" + a + "px", "line-height:" + a + "px", "width:" + o + "px", "border-top: none", "color:#" + t.colorcode].join(";");
    var d = this._render_leftside_content(t);
    return d && r.appendChild(d), d = this._render_rightside_content(t), d && r.appendChild(d), i.show_progress && this._render_task_progress(t, r, o), i.drag_resize && !this._is_flex_task(t) && gantt._render_pair(r, "gantt_task_drag", t, function (t) {
        var e = document.createElement("div");
        e.className = t;
        e.style.cssText = ["background:#" + cssfortactic, "display:block"].join(";");
        return e
    }), i.drag_links && gantt._render_pair(r, "gantt_link_control", t, function (t) {
        var e = document.createElement("div");
        e.className = t, e.style.cssText = ["height:" + a + "px", "line-height:" + a + "px"].join(";");
        var n = document.createElement("div");
        return n.className = "gantt_link_point", e.appendChild(n), e
    }), r
}, gantt._render_side_content = function (t, e, n) {
    if (!e) return null;
    var i = e(t.start_date, t.end_date, t);
    if (!i) return null;
    var a = document.createElement("div");
    return a.className = "gantt_side_content " + n, a.innerHTML = i, a
}, gantt._render_leftside_content = function (t) {
    var e = "gantt_left " + gantt._get_link_crossing_css(!0, t);
    return gantt._render_side_content(t, this.templates.leftside_text, e)
}, gantt._render_rightside_content = function (t) {
    var e = "gantt_right " + gantt._get_link_crossing_css(!1, t);
    return gantt._render_side_content(t, this.templates.rightside_text, e)
}, gantt._get_conditions = function (t) {
    return t ? {
        $source: [gantt.config.links.start_to_start],
        $target: [gantt.config.links.start_to_start, gantt.config.links.finish_to_start]
    } : {
        $source: [gantt.config.links.finish_to_start, gantt.config.links.finish_to_finish],
        $target: [gantt.config.links.finish_to_finish]
    }
}, gantt._get_link_crossing_css = function (t, e) {
    var n = gantt._get_conditions(t);
    for (var i in n)
        for (var a = e[i], s = 0; s < a.length; s++)
            for (var r = gantt.getLink(a[s]), o = 0; o < n[i].length; o++)
                if (r.type == n[i][o]) return "gantt_link_crossing";
    return ""
}, gantt._render_task_content = function (t, e) {
    var n = document.createElement("div");
    return n.innerHTML = this.templates.task_text(t.start_date, t.end_date, t), n.className = "gantt_task_content", n.style.width = e + "px", n
}, gantt._render_task_progress = function (t, e, n) {
    var i = 1 * t.progress || 0;
    n = Math.max(n - 2, 0);
    var a = document.createElement("div"),
        s = Math.round(n * i);
    if (a.style.width = s + "px", a.className = "gantt_task_progress", a.innerHTML = this.templates.progress_text(t.start_date, t.end_date, t), e.appendChild(a), this.config.drag_progress) {
        var r = document.createElement("div");
        r.style.left = s + "px", r.className = "gantt_task_progress_drag", a.appendChild(r), e.appendChild(r)
    }
}, gantt._get_line = function (t) {
    var e = {
        second: 1,
        minute: 60,
        hour: 3600,
        day: 86400,
        week: 604800,
        month: 2592e3,
        year: 31536e3
    };
    return e[t] || 0
}, gantt._date_from_pos = function (t) {
    var e = this._tasks;
    if (0 > t || t > e.full_width) return null;
    for (var n = 0, i = 0; i + e.width[n] < t;) n++, i += e.width[n];
    var a = (t - i) / e.width[n],
        s = gantt._get_coll_duration(e, e.trace_x[n]),
        r = new Date(e.trace_x[n].valueOf() + Math.round(a * s));
    return r
}, gantt.posFromDate = function (t) {
    var e = gantt._day_index_by_date(t);
    dhtmlx.assert(e >= 0, "Invalid day index");
    for (var n = Math.floor(e), i = e % 1, a = 0, s = 1; n >= s; s++) a += gantt._tasks.width[s - 1];
    return i && (a += n < gantt._tasks.width.length ? gantt._tasks.width[n] * (i % 1) : 1), a
}, gantt._day_index_by_date = function (t) {
    var e = new Date(t),
        n = gantt._tasks.trace_x;
    if (+e <= this._min_date) return 0;
    if (+e >= this._max_date) return n.length;
    for (var i = 0; i < n.length - 1 && !(+e < n[i + 1]) ; i++);
    return i + (t - n[i]) / gantt._get_coll_duration(gantt._tasks, n[i])
}, gantt._get_coll_duration = function (t, e) {
    return gantt.date.add(e, t.step, t.unit) - e
}, gantt._get_y_pos = function (t) {
    var e = this._get_visible_order(t);
    return dhtmlx.assert(-1 != e, "Task index not found"), e * this.config.row_height
}, gantt._get_task_coord = function (t) {
    return {
        x: gantt.posFromDate(t.start_date),
        y: gantt._get_y_pos(t.id)
    }
}, gantt._correct_shift = function (t, e) {
    return t -= 6e4 * (new Date(gantt._min_date).getTimezoneOffset() - new Date(t).getTimezoneOffset()) * (e ? -1 : 1)
}, gantt._scroll_task_area = function (t, e) {
    1 * t === t && (this.$task.scrollLeft = t), 1 * e === e && (this.$task_data.scrollTop = e)
}, gantt._get_mouse_pos = function (t) {
    if (t.pageX || t.pageY) var e = {
        x: t.pageX,
        y: t.pageY
    };
    var n = _isIE ? document.documentElement : document.body,
        e = {
            x: t.clientX + n.scrollLeft - n.clientLeft,
            y: t.clientY + n.scrollTop - n.clientTop
        }, i = gantt._get_position(gantt.$task_data);
    return e.x = e.x - i.x + gantt.$task_data.scrollLeft, e.y = e.y - i.y + gantt.$task_data.scrollTop, e
}, gantt._task_renderer = function (t, e, n) {
    return this._task_area_pulls || (this._task_area_pulls = {}), this._task_area_renderers || (this._task_area_renderers = {}), this._task_area_renderers[t] ? this._task_area_renderers[t] : (e || dhtmlx.assert(!1, "Invalid renderer call"), this._task_area_renderers[t] = {
        render_item: function (i, a) {
            var s = gantt._task_area_pulls[t];
            a = a || n;

            var r = e.call(gantt, i);
            r && (s[i.id] ? this.replace_item(i.id, r) : (s[i.id] = r, a.appendChild(r)))
        },
        render_items: function (e, i) {
            this.rendered = gantt._task_area_pulls[t] = {}, i = i || n, i.innerHTML = "";
            for (var a = document.createDocumentFragment(), s = 0, r = e.length; r > s; s++) this.render_item(e[s], a);
            i.appendChild(a)
        },
        replace_item: function (t, e) {
            var n = this.rendered[t];
            n && n.parentNode && n.parentNode.replaceChild(e, n), this.rendered[t] = e
        },
        remove_item: function (t) {
            var e = this.rendered[t];
            e && e.parentNode && e.parentNode.removeChild(e), delete this.rendered[t]
        },
        change_id: function (t, e) {
            this.rendered[e] = this.rendered[t], delete this.rendered[t]
        },
        rendered: this._task_area_pulls[t],
        node: n
    }, this._task_area_renderers[t])
}, gantt.showTask = function (t) {
    var e = this.getTaskNode(t),
        n = Math.max(e.offsetLeft - this.config.task_scroll_offset, 0),
        i = e.offsetTop - (this.$task_data.offsetHeight - this.config.row_height) / 2;
    this._scroll_task_area(n, i)
}, gantt._pull = {}, gantt._branches = {}, gantt._order = [], gantt._lpull = {}, gantt.load = function (t, e, n) {
    dhtmlx.assert(arguments.length, "Invalid load arguments"), this.callEvent("onLoadStart", []);
    var i = "json",
        a = null;
    arguments.length >= 3 ? (i = e, a = n) : "string" == typeof arguments[1] ? i = arguments[1] : "function" == typeof arguments[1] && (a = arguments[1]), dhtmlxAjax.get(t, dhtmlx.bind(function (t) {
        this.on_load(t, i), "function" == typeof a && a.call(this)
    }, this))
}, gantt.parse = function (t, e) {
    this.on_load({
        xmlDoc: {
            responseText: t
        }
    }, e)
}, gantt.serialize = function (t) {
    return t = t || "json", this[t].serialize()
}, gantt.on_load = function (t, e) {
    e || (e = "json"), dhtmlx.assert(this[e], "Invalid data type:'" + e + "'");
    var n = t.xmlDoc.responseText,
        i = this[e].parse(n, t);
    this._process_loading(i), this.callEvent("onLoadEnd", [])
}, gantt._process_loading = function (t) {
    t.collections && this._load_collections(t.collections);
    for (var e = {}, n = t.data, i = 0; i < n.length; i++) {
        var a = n[i];
        this._init_task(a), this.callEvent("onTaskLoading", [e[i]]) && (e[a.id] = a, this._branches[a.parent] || (this._branches[a.parent] = []), this._branches[a.parent].push(a.id))
    }
    dhtmlx.mixin(this._pull, e, !0), this._sync_order();
    for (var i in this._pull) this._pull[i].$level = this._item_level(this._pull[i]);
    this._init_links(t.links || (t.collections ? t.collections.links : []))
}, gantt._init_links = function (t) {
    if (t)
        for (var e = 0; e < t.length; e++) {
            var n = this._init_link(t[e]);
            this._lpull[n.id] = n
        }
    this._sync_links()
}, gantt._load_collections = function (t) {
    var e = !1;
    for (var n in t)
        if (t.hasOwnProperty(n)) {
            e = !0;
            var i = t[n],
                a = this.serverList[n];
            if (!a) continue;
            a.splice(0, a.length);
            for (var s = 0; s < i.length; s++) {
                var r = i[s],
                    o = dhtmlx.copy(r);
                o.key = o.value;
                for (var d in r)
                    if (r.hasOwnProperty(d)) {
                        if ("value" == d || "label" == d) continue;
                        o[d] = r[d]
                    }
                a.push(o)
            }
        }
    e && this.callEvent("onOptionsLoad", [])
}, gantt._sync_order = function () {
    this._order = [], this._sync_order_item({
        parent: 0,
        $open: !0,
        $ignore: !0,
        id: 0
    }), this._scroll_resize(), this._set_sizes()
}, gantt.attachEvent("onBeforeTaskDisplay", function (t, e) {
    return !e.$ignore
}), gantt._sync_order_item = function (t) {
    if (t.id && this.callEvent("onBeforeTaskDisplay", [t.id, t]) && this._order.push(t.id), t.$open) {
        var e = this._branches[t.id];
        if (e)
            for (var n = 0; n < e.length; n++) this._sync_order_item(this._pull[e[n]])
    }
}, gantt._get_visible_order = function (t) {
    dhtmlx.assert(t, "Invalid argument");
    for (var e = this._order, n = 0, i = e.length; i > n; n++)
        if (e[n] == t) return n;
    return -1
}, gantt.eachTask = function (t, e, n) {
    e = e || 0, n = n || this;
    var i = this._branches[e];
    if (i)
        for (var a = 0; a < i.length; a++) {
            var s = this._pull[i[a]];
            t.call(n, s), this._branches[s.id] && this.eachTask(t, s.id, n)
        }
}, gantt.json = {
    parse: function (data) {
        return dhtmlx.assert(data, "Invalid data"), "string" == typeof data && (window.JSON ? data = JSON.parse(data) : (gantt._temp = eval("(" + data + ")"), data = gantt._temp || {}, gantt._temp = null)), data.dhx_security && (dhtmlx.security_key = data.dhx_security), data
    },
    _copyLink: function (t) {
        var e = {};
        for (var n in t) e[n] = t[n];
        return e
    },
    _copyObject: function (t) {
        var e = {};
        for (var n in t) "$" != n.charAt(0) && (e[n] = t[n]);
        return e.start_date = gantt.templates.xml_format(e.start_date), e.end_date && (e.end_date = gantt.templates.xml_format(e.end_date)), e
    },
    serialize: function () {
        var t = [],
            e = [];
        gantt.eachTask(function (e) {
            t.push(this._copyObject(e))
        }, 0, this);
        for (var n in gantt._lpull) e.push(this._copyLink(gantt._lpull[n]));
        return {
            data: t,
            links: e
        }
    }
}, gantt.xml = {
    _xmlNodeToJSON: function (t, e) {
        for (var n = {}, i = 0; i < t.attributes.length; i++) n[t.attributes[i].name] = t.attributes[i].value;
        if (!e) {
            for (var i = 0; i < t.childNodes.length; i++) {
                var a = t.childNodes[i];
                1 == a.nodeType && (n[a.tagName] = a.firstChild ? a.firstChild.nodeValue : "")
            }
            n.text || (n.text = t.firstChild ? t.firstChild.nodeValue : "")
        }
        return n
    },
    _getCollections: function (t) {
        for (var e = {}, n = t.doXPath("//coll_options"), i = 0; i < n.length; i++)
            for (var a = n[i].getAttribute("for"), s = e[a] = [], r = t.doXPath(".//item", n[i]), o = 0; o < r.length; o++) {
                for (var d = r[o], l = d.attributes, h = {
                    key: r[o].getAttribute("value"),
                    label: r[o].getAttribute("label")
                }, _ = 0; _ < l.length; _++) {
                    var c = l[_];
                    "value" != c.nodeName && "label" != c.nodeName && (h[c.nodeName] = c.nodeValue)
                }
                s.push(h)
            }
        return e
    },
    _getXML: function (t, e, n) {
        if (n = n || "data", e.getXMLTopNode || (e = new dtmlXMLLoaderObject(function () { }), e.loadXMLString(t)), xml = e.getXMLTopNode(n), xml.tagName != n) throw "Invalid XML data";
        var i = xml.getAttribute("dhx_security");
        return i && (dhtmlx.security_key = i), e
    },
    parse: function (t, e) {
        e = this._getXML(t, e);
        var n = {}, i = n.data = [];
        xml = e.doXPath("//task");
        for (var a = 0; a < xml.length; a++) i[a] = this._xmlNodeToJSON(xml[a]);
        return n.collections = this._getCollections(e), n
    },
    _copyLink: function (t) {
        return "<item id='" + t.id + "' source='" + t.source + "' target='" + t.target + "' type='" + t.type + "' />"
    },
    _copyObject: function (t) {
        var e = gantt.templates.xml_format(t.start_date),
            n = gantt.templates.xml_format(t.end_date);
        return "<task id='" + t.id + "' parent='" + (t.parent || "") + "' start_date='" + e + "' duration='" + t.duration + "' open='" + !!t.open + "' progress='" + t.progress + "' end_date='" + n + "'><![CDATA[" + t.text + "]]></task>"
    },
    serialize: function () {
        var t = [],
            e = [];
        gantt.eachTask(function (e) {
            t.push(this._copyObject(e))
        }, 0, this);
        for (var n in gantt._lpull) e.push(this._copyLink(gantt._lpull[n]));
        return "<data>" + t.join("") + "<coll_options for='links'>" + e.join("") + "</coll_options></data>"
    }
}, gantt.oldxml = {
    parse: function (t, e) {
        e = gantt.xml._getXML(t, e, "projects");
        var n = {
            collections: {
                links: []
            }
        }, i = n.data = [];
        xml = e.doXPath("//task");
        for (var a = 0; a < xml.length; a++) {
            i[a] = gantt.xml._xmlNodeToJSON(xml[a]);
            var s = xml[a].parentNode;
            i[a].parent = "project" == s.tagName ? "project-" + s.getAttribute("id") : s.parentNode.getAttribute("id")
        }
        xml = e.doXPath("//project");
        for (var a = 0; a < xml.length; a++) {
            var r = gantt.xml._xmlNodeToJSON(xml[a], !0);
            r.id = "project-" + r.id, i.push(r)
        }
        for (var a = 0; a < i.length; a++) {
            var r = i[a];
            r.start_date = r.startdate || r.est, r.end_date = r.enddate, r.text = r.name, r.duration = r.duration / 8, r.open = 1, r.duration || r.end_date || (r.duration = 1), r.predecessortasks && n.collections.links.push({
                target: r.id,
                source: r.predecessortasks,
                type: gantt.config.links.finish_to_start
            })
        }
        return n
    },
    serialize: function () {
        webix.message("Serialization to 'old XML' is not implemented")
    }
}, gantt.serverList = function (t, e) {
    return this.serverList[t] = e ? e.slice(0) : this.serverList[t] || []
}, gantt.getTask = function (t) {
    return dhtmlx.assert(this._pull[t]), this._pull[t]
}, gantt.getTaskByTime = function (t, e) {
    var n = this._pull,
        i = [];
    if (t || e) {
        t = +t || -1 / 0, e = +e || 1 / 0;
        for (var a in n) {
            var s = n[a]; +s.start_date < e && +s.end_date > t && i.push(s)
        }
    } else
        for (var a in n) i.push(n[a]);
    return i
}, gantt.isTaskExists = function (t) {
    return dhtmlx.defined(this._pull[t])
}, gantt.isTaskVisible = function (t) {
    if (!this._pull[t]) return !1;
    if (!(+this._pull[t].start_date < +this._max_date && +this._pull[t].end_date > +this._min_date)) return !1;
    for (var e = 0, n = this._order.length; n > e; e++)
        if (this._order[e] == t) return !0;
    return !1
}, gantt.updateTask = function (t, e) {
    return dhtmlx.defined(e) || (e = this.getTask(t)), this.callEvent("onBeforeTaskUpdate", [t, e]) === !1 ? !1 : (this._pull[e.id] = e, this._update_parents(e.id), this.refreshTask(e.id), this.callEvent("onAfterTaskUpdate", [t, e]), this._sync_order(), this._adjust_scales(), void 0)
}, gantt.addTask = function (t, e) {
    return dhtmlx.defined(e) || (e = t.parent || 0), dhtmlx.defined(this._pull[e]) || (e = 0), t.parent = e, t = this._init_task(t), this.callEvent("onBeforeTaskAdd", [t.id, t]) === !1 ? !1 : (this._pull[t.id] = t, this._branches[t.parent] || (this._branches[t.parent] = []), this._branches[t.parent].push(t.id), this.refreshData(), this.callEvent("onAfterTaskAdd", [t.id, t]), this._adjust_scales(), t.id)
}, gantt.deleteTask = function (t) {
    return this._deleteTask(t)
}, gantt._deleteTask = function (t, e) {
    var n = this.getTask(t);
    if (!e && this.callEvent("onBeforeTaskDelete", [t, n]) === !1) return !1;
    !e && this._dp && this._dp.setUpdateMode("off");
    var i = this._branches[n.id] || [];
    this._selected_task == t && (this._selected_task = null);
    for (var a = 0; a < i.length; a++) this._silentStart(), this._deleteTask(i[a], !0), this._dp && (this._dp._ganttMode = "tasks", this._dp.setUpdated(i[a], !0, "deleted")), this._silentEnd();
    for (!e && this._dp && this._dp.setUpdateMode("cell") ; n.$source.length > 0;) this.deleteLink(n.$source[0]);
    for (; n.$target.length > 0;) this.deleteLink(n.$target[0]);
    return delete this._pull[t], delete this._branches[t], this._branch_update(n.parent, t), e || (this.callEvent("onAfterTaskDelete", [t, n]), this.refreshData()), !0
}, gantt.clearAll = function () {
    this._pull = {}, this._branches = {}, this._order = [], this._order_full = [], this._lpull = {}, this.refreshData(), this.callEvent("onClear", [])
}, gantt.changeTaskId = function (t, e) {
    var n = this._pull[e] = this._pull[t];
    this._pull[e].id = e, delete this._pull[t];
    for (var i in this._pull) this._pull[i].parent == t && (this._pull[i].parent = e);
    this._lightbox_id == t && (this._lightbox_id = e), this._branch_update(n.parent, t, e), this._sync_order(), this.callEvent("onTaskIdChange", [t, e])
}, gantt._branch_update = function (t, e, n) {
    var i = this._branches[t];
    if (i) {
        for (var a = [], s = 0; s < i.length; s++) i[s] != e ? a.push(i[s]) : n && a.push(n);
        this._branches[t] = a
    }
}, gantt._get_duration_unit = function () {
    return 1e3 * gantt._get_line(this.config.duration_unit) || this.config.duration_unit
}, gantt._init_task = function (t) {
    dhtmlx.defined(t.id) || (t.id = dhtmlx.uid()), t.start_date && (t.start_date = gantt.date.parseDate(t.start_date, "xml_date")), t.end_date && (t.end_date = gantt.date.parseDate(t.end_date, "xml_date"));
    var e = this._get_duration_unit();
    return t.start_date && (t.end_date ? this._update_task_duration(t) : t.duration && (t.end_date = new Date(t.start_date.valueOf() + t.duration * e))), gantt._init_task_timing(t), t.$source = [], t.$target = [], t.parent = t.parent || 0, t.$open = dhtmlx.defined(t.open) ? t.open : !1, t.$level = this._item_level(t), t
}, gantt._init_task_timing = function (t) {
    t.$no_end = !(t.end_date || t.duration), t.$no_start = !t.start_date
}, gantt._is_flex_task = function (t) {
    return !(!t.$no_end && !t.$no_start)
}, gantt._update_parents = function (t, e) {
    if (t) {
        for (var n = this.getTask(t) ; !n.$no_end && !n.$no_start && n.parent && this.isTaskExists(n.parent) ;) n = this.getTask(n.parent);
        if (n.$no_end) {
            var i = 0;
            this.eachTask(function (t) {
                +t.end_date > +i && (i = new Date(t.end_date))
            }, n.id), i && (n.end_date = i)
        }
        if (n.$no_start) {
            var a = 1 / 0;
            this.eachTask(function (t) {
                +t.start_date < +a && (a = new Date(t.start_date))
            }, n.id), 1 / 0 != a && (n.start_date = a)
        } (n.$no_end || n.$no_start) && (this._update_task_duration(n), e || this.refreshTask(n.id, !0)), n.parent && this.isTaskExists(n.parent) && this._update_parents(n.parent, e)
    }
}, gantt._round_date = function (t, e, n) {
    var i = gantt.date[n + "_start"](new Date(t)),
        a = gantt.date.add(i, e, n);
    return Math.abs(t - i) < Math.abs(a - t) ? i : a
}, gantt._round_task_dates = function (t, e, n) {
    t.start_date = this._round_date(+t.start_date, e, n), t.end_date = this._round_date(+t.end_date, e, n), t.end_date <= t.start_date && (e = e || gantt._tasks.step, n = n || gantt._tasks.unit, t.end_date = gantt.date.add(t.start_date, e, n))
}, gantt._update_task_duration = function (t) {
    t.start_date && t.end_date && (t.duration = Math.round((t.end_date - t.start_date) / (this.config.duration_step * this._get_duration_unit())))
}, gantt._item_level = function (t) {
    for (var e = 0; t.parent && dhtmlx.defined(this._pull[t.parent]) ;) t = this._pull[t.parent], e++;
    return e
}, gantt.sort = function (t, e, n) {
    var i = !arguments[3];
    dhtmlx.defined(n) || (n = 0), dhtmlx.defined(t) || (t = "order");
    var a = "string" == typeof t ? function (n, i) {
        var a = n[t] > i[t];
        return e && (a = !a), a ? 1 : -1
    } : t,
        s = this._branches[n];
    if (s) {
        for (var r = [], o = s.length - 1; o >= 0; o--) r[o] = this._pull[s[o]];
        r.sort(a);
        for (var o = 0; o < r.length; o++) s[o] = r[o].id, this.sort(t, e, s[o], !0)
    }
    i && this.refreshData()
}, gantt.getNext = function (t) {
    for (var e = 0; e < this._order.length - 1; e++)
        if (this._order[e] == t) return this._order[e + 1];
    return null
}, gantt.getPrev = function (t) {
    for (var e = 1; e < this._order.length; e++)
        if (this._order[e] == t) return this._order[e - 1];
    return null
}, gantt._dp_init = function (t) {
    t.setTransactionMode("POST", !0), t.serverProcessor += (-1 != t.serverProcessor.indexOf("?") ? "&" : "?") + "editing=true", t._serverProcessor = t.serverProcessor, t.styles = {
        updated: "gantt_updated",
        inserted: "gantt_inserted",
        deleted: "gantt_deleted",
        invalid: "gantt_invalid",
        error: "gantt_error",
        clear: ""
    }, t._methods = ["_row_style", "setCellTextStyle", "_change_id", "_delete_task"], this.attachEvent("onAfterTaskAdd", function (e) {
        t._ganttMode = "tasks", t.setUpdated(e, !0, "inserted")
    }), this.attachEvent("onAfterTaskUpdate", function (e) {
        t._ganttMode = "tasks", t.setUpdated(e, !0)
    }), this.attachEvent("onAfterTaskDelete", function (e) {
        t._ganttMode = "tasks", t.setUpdated(e, !0, "deleted")
    }), this.attachEvent("onAfterLinkUpdate", function (e) {
        t._ganttMode = "links", t.setUpdated(e, !0)
    }), this.attachEvent("onAfterLinkAdd", function (e) {
        t._ganttMode = "links", t.setUpdated(e, !0, "inserted")
    }), this.attachEvent("onAfterLinkDelete", function (e) {
        t._ganttMode = "links", t.setUpdated(e, !0, "deleted")
    }), this.attachEvent("onRowDragEnd", function (e, n) {
        t._ganttMode = "tasks", this.getTask(e).target = n, t.setUpdated(e, !0, "order")
    }), t.attachEvent("onBeforeDataSending", function () {
        return this.serverProcessor = this._serverProcessor + getUrlSymbol(this._serverProcessor) + "gantt_mode=" + this._ganttMode, !0
    }), t._getRowData = dhtmlx.bind(function (e) {
        var n;
        n = "tasks" == t._ganttMode ? this.isTaskExists(e) ? this.getTask(e) : {
            id: e
        } : this.isLinkExists(e) ? this.getLink(e) : {
            id: e
        };
        var i = {};
        for (var a in n)
            if ("$" != a.substr(0, 1)) {
                var s = n[a];
                i[a] = s instanceof Date ? this.templates.xml_format(s) : s
            }
        return n.$no_start && (n.start_date = "", n.duration = ""), n.$no_end && (n.end_date = "", n.duration = ""), i[t.action_param] = this.getUserData(e, t.action_param), i
    }, this), this._change_id = dhtmlx.bind(function (e, n) {
        "tasks" != t._ganttMode ? this.changeLinkId(e, n) : this.changeTaskId(e, n)
    }, this), this._row_style = function (e, n) {
        if ("tasks" == t._ganttMode) {
            var i = gantt.getTaskRowNode(e);
            if (i)
                if (n) i.className += " " + n;
                else {
                    var a = / (gantt_updated|gantt_inserted|gantt_deleted|gantt_invalid|gantt_error)/g;
                    i.className = i.className.replace(a, "")
                }
        }
    }, this._delete_task = function () { }, this._dp = t
}, gantt.getUserData = function (t, e) {
    return this.userdata || (this.userdata = {}), this.userdata[t] && this.userdata[t][e] ? this.userdata[t][e] : ""
}, gantt.setUserData = function (t, e, n) {
    this.userdata || (this.userdata = {}), this.userdata[t] || (this.userdata[t] = {}), this.userdata[t][e] = n
}, gantt._init_link = function (t) {
    return dhtmlx.defined(t.id) || (t.id = dhtmlx.uid()), t
}, gantt._sync_links = function () {
    for (var t in this._pull) this._pull[t].$source = [], this._pull[t].$target = [];
    for (var t in this._lpull) {
        var e = this._lpull[t];
        this._pull[e.source] && this._pull[e.source].$source.push(t), this._pull[e.target] && this._pull[e.target].$target.push(t)
    }
}, gantt.getLink = function (t) {
    return dhtmlx.assert(this._lpull[t], "Link doesn't exist"), this._lpull[t]
}, gantt.isLinkExists = function (t) {
    return dhtmlx.defined(this._lpull[t])
}, gantt.addLink = function (t) {
    return t = this._init_link(t), this.callEvent("onBeforeLinkAdd", [t.id, t]) === !1 ? !1 : (this._lpull[t.id] = t, this._sync_links(), this._render_link(t.id), this.callEvent("onAfterLinkAdd", [t.id, t]), t.id)
}, gantt.updateLink = function (t, e) {
    return dhtmlx.defined(e) || (e = this.getLink(t)), this.callEvent("onBeforeLinkUpdate", [t, e]) === !1 ? !1 : (this._lpull[t] = e, this._sync_links(), this._render_link(t), this.callEvent("onAfterLinkUpdate", [t, e]), !0)
}, gantt.deleteLink = function (t) {
    return this._deleteLink(t)
}, gantt._deleteLink = function (t, e) {
    var n = this.getLink(t);
    return e || this.callEvent("onBeforeLinkDelete", [t, n]) !== !1 ? (delete this._lpull[t], this._sync_links(), this.refreshLink(t), e || this.callEvent("onAfterLinkDelete", [t, n]), !0) : !1
}, gantt.changeLinkId = function (t, e) {
    this._lpull[e] = this._lpull[t], this._lpull[e].id = e, delete this._lpull[t], this._sync_links(), this.callEvent("onLinkIdChange", [t, e])
}, gantt.getChildren = function (t) {
    return dhtmlx.defined(this._branches[t]) ? this._branches[t] : []
}, gantt.hasChild = function (t) {
    return dhtmlx.defined(this._branches[t])
}, gantt.refreshData = function () {
    this._sync_order(), this._render_data()
}, gantt._configure = function (t, e) {
    for (var n in e) "undefined" == typeof t[n] && (t[n] = e[n])
}, gantt._init_skin = function () {
    if (!gantt.skin)
        for (var t = document.getElementsByTagName("link"), e = 0; e < t.length; e++) {
            var n = t[e].href.match("dhtmlxgantt_([a-z]+).css");
            if (n) {
                gantt.skin = n[1];
                break
            }
        }
    gantt.skin || (gantt.skin = "terrace");
    var i = gantt.skins[gantt.skin];
    this._configure(gantt.config, i.config);
    var a = gantt.config.columns;
    a[1] && "undefined" == typeof a[1].width && (a[1].width = i._second_column_width), a[2] && "undefined" == typeof a[2].width && (a[2].width = i._third_column_width), i._lightbox_template && (gantt._lightbox_template = i._lightbox_template), gantt._init_skin = function () { }
}, gantt.skins = {}, gantt._lightbox_methods = {}, gantt._lightbox_template = "<div class='dhx_cal_ltitle'><span class='dhx_mark'>&nbsp;</span><span class='dhx_time'></span><span class='dhx_title'></span></div><div class='dhx_cal_larea'></div>", gantt.showLightbox = function (t) {
    if (t && this.callEvent("onBeforeLightbox", [t])) {
        var e = this.getLightbox();
        this._center_lightbox(e), this.showCover(), this._fill_lightbox(t, e), this.callEvent("onLightbox", [t])
    }
}, gantt._get_timepicker_step = function () {
    if (this.config.round_dnd_dates) {
        var t = gantt._tasks,
            e = this._get_line(t.unit) * t.step / 60;
        return e >= 1440 && (e = this.config.time_step), e
    }
    return this.config.time_step
}, gantt.getLabel = function (t, e) {
    for (var n = this.config.lightbox.sections, i = 0; i < n.length; i++)
        if (n[i].map_to == t)
            for (var a = n[i].options, s = 0; s < a.length; s++)
                if (a[s].key == e) return a[s].label;
    return ""
}, gantt.updateCollection = function (t, e) {
    var n = scheduler.serverList(t);
    return n ? (n.splice(0, n.length), n.push.apply(n, e || []), gantt.resetLightbox(), void 0) : !1
}, gantt.getLightbox = function () {
    if (!this._lightbox) {
        var t = document.createElement("DIV");
        t.className = "dhx_cal_light";
        var e = this._is_lightbox_timepicker();
        (gantt.config.wide_form || e) && (t.className += " dhx_cal_light_wide"), e && (gantt.config.wide_form = !0, t.className += " dhx_cal_light_full"), t.style.visibility = "hidden";
        var n = this._lightbox_template,
            i = this.config.buttons_left;
        for (var a in i) n += "<div class='dhx_btn_set dhx_left_btn_set " + i[a] + "_set'><div dhx_button='1' class='" + i[a] + "'></div><div>" + this.locale.labels[i[a]] + "</div></div>";
        i = this.config.buttons_right;
        for (var a in i) n += "<div class='dhx_btn_set dhx_right_btn_set " + i[a] + "_set' style='float:right;'><div dhx_button='1' class='" + i[a] + "'></div><div>" + this.locale.labels[i[a]] + "</div></div>";
        n += "</div>", t.innerHTML = n, gantt.config.drag_lightbox && (t.firstChild.onmousedown = gantt._ready_to_dnd, t.firstChild.onselectstart = function () {
            return !1
        }, t.firstChild.style.cursor = "pointer", gantt._init_dnd_events()), document.body.insertBefore(t, document.body.firstChild), this._lightbox = t;
        var s = this.config.lightbox.sections;
        n = this._render_sections(s);
        for (var r = t.getElementsByTagName("div"), a = 0; a < r.length; a++) {
            var o = r[a];
            if ("dhx_cal_larea" == o.className) {
                o.innerHTML = n;
                break
            }
        }
        this.resizeLightbox(), this._init_lightbox_events(this), t.style.display = "none", t.style.visibility = "visible"
    }
    return this._lightbox
}, gantt._render_sections = function (t) {
    for (var e = "", n = 0; n < t.length; n++) {
        var i = this.form_blocks[t[n].type];
        if (i) {
            t[n].id = "area_" + dhtmlx.uid();
            var a = "";
            t[n].button && (a = "<div class='dhx_custom_button' index='" + n + "'><div class='dhx_custom_button_" + t[n].button + "'></div><div>" + this.locale.labels["button_" + t[n].button] + "</div></div>"), this.config.wide_form && (e += "<div class='dhx_wrap_section'>"), e += "<div id='" + t[n].id + "' class='dhx_cal_lsection'>" + a + this.locale.labels["section_" + t[n].name] + "</div>" + i.render.call(this, t[n]), e += "</div>"
        }
    }
    return e
}, gantt.resizeLightbox = function () {
    var t = this._lightbox;
    if (t) {
        var e = t.childNodes[1];
        e.style.height = "0px", e.style.height = e.scrollHeight + "px", t.style.height = e.scrollHeight + this.config.lightbox_additional_height + "px", e.style.height = e.scrollHeight + "px"
    }
}, gantt._center_lightbox = function (t) {
    if (t) {
        t.style.display = "block";
        var e = window.pageYOffset || document.body.scrollTop || document.documentElement.scrollTop,
            n = window.pageXOffset || document.body.scrollLeft || document.documentElement.scrollLeft,
            i = window.innerHeight || document.documentElement.clientHeight;
        t.style.top = e ? Math.round(e + Math.max((i - t.offsetHeight) / 2, 0)) + "px" : Math.round(Math.max((i - t.offsetHeight) / 2, 0) + 9) + "px", t.style.left = document.documentElement.scrollWidth > document.body.offsetWidth ? Math.round(n + (document.body.offsetWidth - t.offsetWidth) / 2) + "px" : Math.round((document.body.offsetWidth - t.offsetWidth) / 2) + "px"
    }
}, gantt.showCover = function () {
    this._cover = document.createElement("DIV"), this._cover.className = "dhx_cal_cover";
    var t = void 0 !== document.height ? document.height : document.body.offsetHeight,
        e = document.documentElement ? document.documentElement.scrollHeight : 0;
    this._cover.style.height = Math.max(t, e) + "px", document.body.appendChild(this._cover)
}, gantt._init_lightbox_events = function () {
    gantt.lightbox_events = {}, gantt.lightbox_events.dhx_save_btn = function () {
        gantt._save_lightbox()
    }, gantt.lightbox_events.dhx_delete_btn = function () {
        gantt.callEvent("onLightboxDelete", [gantt._lightbox_id]) && gantt.$click.buttons["delete"](gantt._lightbox_id)
    }, gantt.lightbox_events.dhx_cancel_btn = function () {
        /*var t = gantt.getLightboxValues();
        if(t.text=="New task")// a new task with default name, when the user cancel the task craeation
        {
            gantt.deleteTask(gantt._lightbox_id), gantt.hideLightbox()
        }
        else
        {*/
        gantt._cancel_lightbox()	//whrn the user cancel the edit task
        //	} 
    }, gantt.lightbox_events["default"] = function (t, e) {
        if (e.getAttribute("dhx_button")) gantt.callEvent("onLightboxButton", [e.className, e, t]);
        else {
            var n, i, a; -1 != e.className.indexOf("dhx_custom_button") && (-1 != e.className.indexOf("dhx_custom_button_") ? (n = e.parentNode.getAttribute("index"), a = e.parentNode.parentNode) : (n = e.getAttribute("index"), a = e.parentNode, e = e.firstChild)), n && (i = gantt.form_blocks[gantt.config.lightbox.sections[n].type], i.button_click(n, e, a, a.nextSibling))
        }
    }, dhtmlxEvent(gantt.getLightbox(), "click", function (t) {
        t = t || window.event;
        var e = t.target ? t.target : t.srcElement;
        if (e.className || (e = e.previousSibling), e && e.className && 0 === e.className.indexOf("dhx_btn_set") && (e = e.firstChild), e && e.className) {
            var n = dhtmlx.defined(gantt.lightbox_events[e.className]) ? gantt.lightbox_events[e.className] : gantt.lightbox_events["default"];
            return n(t, e)
        }
        return !1
    }), gantt.getLightbox().onkeydown = function (t) {
        switch ((t || event).keyCode) {
            case gantt.keys.edit_save:
                if ((t || event).shiftKey) return;
                gantt._save_lightbox();
                break;
            case gantt.keys.edit_cancel:
                gantt._cancel_lightbox()
        }
    }
}, gantt._cancel_lightbox = function () {
    this.callEvent("onLightboxCancel", [this._lightbox_id, this.$new]), this.hideLightbox()
}, gantt._save_lightbox = function () {
    console.log(this);
    var date_problem = false;
    var invalid_date = false;
    var t = this.getLightboxValues();

    var childrens = gantt.getChildren(t.id);
    var day = document.getElementById('day_select').value;
    var month = document.getElementById('month_select').value;
    var year = document.getElementById('year_select').value;
    month = (parseInt(month) + 1).toString();
    if (parseInt(month) < 10)
        month = '0' + month;
    if (parseInt(day) < 10)
        day = '0' + day;
    var date = new Date(year + '-' + month + '-' + day);
    date.setDate(date.getDate() + 1);
    if (date == 'Invalid Date') {
        invalid_date = true;
        alert('Invalid Date');
    }
    if (t.parent != 0) {
        var parent = gantt.getTask(t.parent);
        if (parent != null) {
            if (parent.start_date > t.start_date || parent.end_date < t.end_date) {
                date_problem = true;
                alert("The date should to be between " + parent.start_date.toDateString() + " and " + parent.end_date.toDateString());
            }
        }
    }

    if (childrens != null) {
        if (childrens.length != 0) {
            var limit_start_date = this.getTask(childrens[0]).start_date;
            var limit_end_date = this.getTask(childrens[0]).end_date;
            for (var i = 0; i < childrens.length - 1; i++)//search the limits dates in children dates
            {
                if (this.getTask(childrens[i + 1]).start_date < this.getTask(childrens[i]).start_date)
                    limit_start_date = this.getTask(childrens[i + 1]).start_date
                if (this.getTask(childrens[i + 1]).end_date > this.getTask(childrens[i]).end_date)
                    limit_end_date = this.getTask(childrens[i + 1]).end_date
            }
            if (t.start_date > limit_start_date || t.end_date < limit_end_date) {
                children_problem = true;
                alert("The date should be lower than " + parent.start_date.getDay().toDateString() + " and bigger than " + parent.end_date.toDateString());
            }
        }
    }
    if (date_problem === false && invalid_date === false && t.text != "New task")

        this.callEvent("onLightboxSave", [this._lightbox_id, t, !!t.$new]) && (t.$new ? this.addTask(t) : (dhtmlx.mixin(this.getTask(t.id), t, !0), this.updateTask(t.id)), this.refreshData(), this.hideLightbox())
    if (t.text === "New task") {
        alert("Please change the task description");
    }

}, gantt.getLightboxValues = function () {
    for (var t = dhtmlx.mixin({}, this.getTask(this._lightbox_id)), e = this.config.lightbox.sections, n = 0; n < e.length; n++) {
        var i = document.getElementById(e[n].id);
        i = i ? i.nextSibling : i;
        var a = this.form_blocks[e[n].type],
            s = a.get_value.call(this, i, t, e[n]);
        "auto" != e[n].map_to && (t[e[n].map_to] = s)
    }
    return t
}, gantt.hideLightbox = function () {
    var t = this.getLightbox();
    t && (t.style.display = "none"), this._lightbox_id = null, this.hideCover(), this.callEvent("onAfterLightbox", [])
}, gantt.hideCover = function () {
    this._cover && this._cover.parentNode.removeChild(this._cover), this._cover = null
}, gantt.resetLightbox = function () {
    gantt._lightbox && !gantt._custom_lightbox && gantt._lightbox.parentNode.removeChild(gantt._lightbox), gantt._lightbox = null
}, gantt._fill_lightbox = function (t, e) {
    var n = this.getTask(t),
        i = e.getElementsByTagName("span");
    gantt.templates.lightbox_header ? (i[1].innerHTML = "", i[2].innerHTML = gantt.templates.lightbox_header(n.start_date, n.end_date, n)) : (i[1].innerHTML = this.templates.task_time(n.start_date, n.end_date, n), i[2].innerHTML = (this.templates.task_text(n.start_date, n.end_date, n) || "").substr(0, 70));
    for (var a = this.config.lightbox.sections, s = 0; s < a.length; s++) {
        var r = a[s],
            o = document.getElementById(r.id).nextSibling,
            d = this.form_blocks[r.type],
            l = dhtmlx.defined(n[r.map_to]) ? n[r.map_to] : r.default_value;
        d.set_value.call(this, o, l, n, r), r.focus && d.focus.call(this, o)
    }
    gantt._lightbox_id = t
}, gantt.getLightboxSection = function (t) {
    var e = this.config.lightbox.sections,
        n = 0;
    for (n; n < e.length && e[n].name != t; n++);
    var i = e[n];
    this._lightbox || this.getLightbox();
    var a = document.getElementById(i.id),
        s = a.nextSibling,
        r = {
            section: i,
            header: a,
            node: s,
            getValue: function (t) {
                return this.form_blocks[i.type].get_value(s, t || {}, i)
            },
            setValue: function (t, e) {
                return this.form_blocks[i.type].set_value(s, t, e || {}, i)
            }
        }, o = this._lightbox_methods["get_" + i.type + "_control"];
    return o ? o(r) : r
}, gantt._lightbox_methods.get_template_control = function (t) {
    return t.control = t.node, t
}, gantt._lightbox_methods.get_select_control = function (t) {
    return t.control = t.node.getElementsByTagName("select")[0], t
}, gantt._lightbox_methods.get_textarea_control = function (t) {
    return t.control = t.node.getElementsByTagName("textarea")[0], t
}, gantt._lightbox_methods.get_time_control = function (t) {
    return t.control = t.node.getElementsByTagName("select"), t
}, gantt._init_dnd_events = function () {
    dhtmlxEvent(document.body, "mousemove", gantt._move_while_dnd), dhtmlxEvent(document.body, "mouseup", gantt._finish_dnd), gantt._init_dnd_events = function () { }
}, gantt._move_while_dnd = function (t) {
    if (gantt._dnd_start_lb) {
        document.dhx_unselectable || (document.body.className += " dhx_unselectable", document.dhx_unselectable = !0);
        var e = gantt.getLightbox(),
            n = t && t.target ? [t.pageX, t.pageY] : [event.clientX, event.clientY];
        e.style.top = gantt._lb_start[1] + n[1] - gantt._dnd_start_lb[1] + "px", e.style.left = gantt._lb_start[0] + n[0] - gantt._dnd_start_lb[0] + "px"
    }
}, gantt._ready_to_dnd = function (t) {
    var e = gantt.getLightbox();
    gantt._lb_start = [parseInt(e.style.left, 10), parseInt(e.style.top, 10)], gantt._dnd_start_lb = t && t.target ? [t.pageX, t.pageY] : [event.clientX, event.clientY]
}, gantt._finish_dnd = function () {
    gantt._lb_start && (gantt._lb_start = gantt._dnd_start_lb = !1, document.body.className = document.body.className.replace(" dhx_unselectable", ""), document.dhx_unselectable = !1)
}, gantt._focus = function (t, e) {
    t && t.focus && (gantt.config.touch || (e && t.select && t.select(), t.focus()))
}, gantt.form_blocks = {
    getTimePicker: function (t) {
        var e = t.time_format;
        if (!e) {
            var e = ["%d", "%m", "%Y"];
            gantt._get_line(gantt._tasks.unit) < gantt._get_line("day") && e.push("%H:%i")
        }
        t._time_format_order = {
            size: 0
        };
        var n = this.config,
            i = this.date.date_part(new Date(gantt._min_date.valueOf())),
            a = 1440,
            s = 0;
        gantt.config.limit_time_select && (a = 60 * n.last_hour + 1, s = 60 * n.first_hour, i.setHours(n.first_hour));
        for (var r = "", o = 0; o < e.length; o++) {
            var d = e[o];
            switch (o > 0 && (r += " "), d) {
                case "%Y":
                    t._time_format_order[2] = o, t._time_format_order.size++, r += "<select id=year_select>";
                    for (var l = i.getFullYear() - 5, h = 0; 10 > h; h++) r += "<option value='" + (l + h) + "'>" + (l + h) + "</option>";
                    r += "</select> ";
                    break;
                case "%m":
                    t._time_format_order[1] = o, t._time_format_order.size++, r += "<select id=month_select>";
                    for (var h = 0; 12 > h; h++) r += "<option value='" + h + "'>" + this.locale.date.month_full[h] + "</option>";
                    r += "</select>";
                    break;
                case "%d":
                    t._time_format_order[0] = o, t._time_format_order.size++, r += "<select id=day_select>";
                    for (var h = 1; 32 > h; h++) r += "<option value='" + h + "'>" + h + "</option>";
                    r += "</select>";
                    break;
                case "%H:%i":
                    var a = 1440,
                        s = 0;
                    t._time_format_order[3] = o, t._time_format_order.size++, r += "<select>";
                    var h = s,
                        _ = i.getDate();
                    for (t._time_values = []; a > h;) {
                        var c = this.templates.time_picker(i);
                        r += "<option value='" + h + "'>" + c + "</option>", t._time_values.push(h), i.setTime(i.valueOf() + 1e3 * 60 * this._get_timepicker_step());
                        var g = i.getDate() != _ ? 1 : 0;
                        h = 60 * 24 * g + 60 * i.getHours() + i.getMinutes()
                    }
                    r += "</select>"
            }
        }
        return r
    },
    _fill_lightbox_select: function (t, e, n, i) {
        if (t[e + i[0]].value = n.getDate(), t[e + i[1]].value = n.getMonth(), t[e + i[2]].value = n.getFullYear(), dhtmlx.defined(i[3])) {
            var a = 60 * n.getHours() + n.getMinutes();
            a = Math.round(a / gantt._get_timepicker_step()) * gantt._get_timepicker_step(), t[e + i[3]].value = a
        }
    },
    template: {
        render: function (t) {
            var e = (t.height || "30") + "px";
            return "<div class='dhx_cal_ltext dhx_cal_template' style='height:" + e + ";'></div>"
        },
        set_value: function (t, e) {
            t.innerHTML = e || ""
        },
        get_value: function (t) {
            return t.innerHTML || ""
        },
        focus: function () { }
    },
    textarea: {
        render: function (t) {
            var e = (t.height || "130") + "px";
            return "<div class='dhx_cal_ltext' style='height:" + e + ";'><textarea></textarea></div>"
        },
        set_value: function (t, e) {
            t.firstChild.value = e || ""
        },
        get_value: function (t) {
            return t.firstChild.value
        },
        focus: function (t) {
            var e = t.firstChild;
            gantt._focus(e, !0)
        }
    },
    select: {
        render: function (t) {
            for (var e = (t.height || "23") + "px", n = "<div class='dhx_cal_ltext' style='height:" + e + ";'><select style='width:100%;'>", i = 0; i < t.options.length; i++) n += "<option value='" + t.options[i].key + "'>" + t.options[i].label + "</option>";
            return n += "</select></div>"
        },
        set_value: function (t, e, n, i) {
            var a = t.firstChild;
            !a._dhx_onchange && i.onchange && (a.onchange = i.onchange, a._dhx_onchange = !0), "undefined" == typeof e && (e = (a.options[0] || {}).value), a.value = e || ""
        },
        get_value: function (t) {
            return t.firstChild.value
        },
        focus: function (t) {
            var e = t.firstChild;
            gantt._focus(e, !0)
        }
    },
    time: {
        render: function (t) {
            var e = this.form_blocks.getTimePicker.call(this, t),
                n = "<div style='height:30px;padding-top:0px;font-size:inherit;text-align:center;' class='dhx_section_time'>" + e + "<span style='font-weight:normal; font-size:10pt;'> &nbsp;&ndash;&nbsp; </span>" + e + "</div>";
            return n
        },
        set_value: function (t, e, n, i) {
            function a() {
                var t = new Date(r[o[2]].value, r[o[1]].value, r[o[0]].value, 0, 0),
                    e = new Date(t.getTime() + 1e3 * 60 * gantt.config.event_duration);
                this.form_blocks._fill_lightbox_select(r, o.size, e, o, s)
            }
            var s = this.config,
                r = t.getElementsByTagName("select"),
                o = i._time_format_order;
            if (i._time_format_size, s.auto_end_date && s.event_duration)
                for (var d = 0; 4 > d; d++) r[d].onchange = a;
            this.form_blocks._fill_lightbox_select(r, 0, n.start_date, o, s), this.form_blocks._fill_lightbox_select(r, o.size, n.end_date, o, s)
        },
        get_value: function (t, e, n) {
            var i = t.getElementsByTagName("select"),
                a = n._time_format_order,
                s = 0,
                r = 0;
            if (dhtmlx.defined(a[3])) {
                var o = parseInt(i[a[3]].value, 10);
                s = Math.floor(o / 60), r = o % 60
            }
            if (e.start_date = new Date(i[a[2]].value, i[a[1]].value, i[a[0]].value, s, r), s = r = 0, dhtmlx.defined(a[3])) {
                var o = parseInt(i[a.size + a[3]].value, 10);
                s = Math.floor(o / 60), r = o % 60
            }
            return e.end_date = new Date(i[a[2] + a.size].value, i[a[1] + a.size].value, i[a[0] + a.size].value, s, r), e.end_date <= e.start_date && (e.end_date = gantt.date.add(e.start_date, gantt._get_timepicker_step(), "minute")), {
                start_date: new Date(e.start_date),
                end_date: new Date(e.end_date)
            }
        },
        focus: function (t) {
            gantt._focus(t.getElementsByTagName("select")[0])
        }
    },
    duration: {
        render: function (t) {
            var e = this.form_blocks.getTimePicker.call(this, t);
            e = "<div class='dhx_time_selects'>" + e + "</div>";
            var n = this.locale.labels[this._tasks.unit + "s"],
                i = "<div class='dhx_gantt_duration'><input type='button' class='dhx_gantt_duration_dec' value='-'><input type='text' value='5' class='dhx_gantt_duration_value'><input type='button' class='dhx_gantt_duration_inc' value='+'> " + n + " <span></span></div>",
                a = "<div style='height:30px;padding-top:0px;font-size:inherit;' class='dhx_section_time'>" + e + " " + i + "</div>";
            return a
        },
        set_value: function (t, e, n, i) {
            function a() {
                var e = gantt.form_blocks.duration._get_start_date.call(gantt, t, i),
                    n = gantt.form_blocks.duration._get_duration.call(gantt, t, i),
                    a = gantt.date.add(e, n, gantt._tasks.unit);
                _.innerHTML = gantt.templates.task_date(a)
            }

            function s(t) {
                var e = l.value;
                e = parseInt(e, 10), window.isNaN(e) && (e = 0), e += t, 1 > e && (e = 1), l.value = e, a()
            }
            var r = this.config,
                o = t.getElementsByTagName("select"),
                d = t.getElementsByTagName("input"),
                l = d[1],
                h = [d[0], d[2]],
                _ = t.getElementsByTagName("span")[0],
                c = i._time_format_order;
            h[0].onclick = dhtmlx.bind(function () {
                s(-1 * this.config.duration_step)
            }, this), h[1].onclick = dhtmlx.bind(function () {
                s(1 * this.config.duration_step)
            }, this), o[0].onchange = a, o[1].onchange = a, o[2].onchange = a, o[3] && (o[3].onchange = a), l.onkeydown = dhtmlx.bind(function (t) {
                t = t || window.event;
                var e = t.charCode || t.keyCode || t.which;
                return 40 == e ? (s(-1 * this.config.duration_step), !1) : 38 == e ? (s(1 * this.config.duration_step), !1) : (window.setTimeout(function () {
                    a()
                }, 1), void 0)
            }, this), l.onchange = dhtmlx.bind(function () {
                a()
            }, this), this.form_blocks._fill_lightbox_select(o, 0, n.start_date, c, r);
            var g, u = gantt._tasks.unit;
            g = n.end_date ? (n.end_date.valueOf() - n.start_date.valueOf()) / (1e3 * this._get_line(u)) : n.duration, g = Math.round(g), l.value = g, a()
        },
        _get_start_date: function (t, e) {
            var n = t.getElementsByTagName("select"),
                i = e._time_format_order,
                a = 0,
                s = 0;
            if (dhtmlx.defined(i[3])) {
                var r = parseInt(n[i[3]].value, 10);
                a = Math.floor(r / 60), s = r % 60
            }
            return new Date(n[i[2]].value, n[i[1]].value, n[i[0]].value, a, s)
        },
        _get_duration: function (t) {
            var e = t.getElementsByTagName("input")[1];
            return e = parseInt(e.value, 10), window.isNaN(e) && (e = 1), 0 > e && (e *= -1), e
        },
        get_value: function (t, e, n) {
            e.start_date = this.form_blocks.duration._get_start_date(t, n);
            var i = this.form_blocks.duration._get_duration(t, n);
            return e.end_date = this.date.add(e.start_date, i, this._tasks.unit), e.duration = i, {
                start_date: new Date(e.start_date),
                end_date: new Date(e.end_date)
            }
        },
        focus: function (t) {
            gantt._focus(t.getElementsByTagName("select")[0])
        }
    }
}, gantt._is_lightbox_timepicker = function () {
    for (var t = this.config.lightbox.sections, e = 0; e < t.length; e++)
        if ("time" == t[e].name && "time" == t[e].type) return !0;
    return !1
}, gantt._dhtmlx_confirm = function (t, e, n, i) {
    if (!t) return n();
    var a = {
        text: t
    };
    e && (a.title = e), i && (a.ok = i), n && (a.callback = function (t) {
        t && n()
    }), dhtmlx.confirm(a)
}, dataProcessor.prototype = {
    setTransactionMode: function (t, e) {
        this._tMode = t, this._tSend = e
    },
    escape: function (t) {
        return this._utf ? encodeURIComponent(t) : escape(t)
    },
    enableUTFencoding: function (t) {
        this._utf = convertStringToBoolean(t)
    },
    setDataColumns: function (t) {
        this._columns = "string" == typeof t ? t.split(",") : t
    },
    getSyncState: function () {
        return !this.updatedRows.length
    },
    enableDataNames: function (t) {
        this._endnm = convertStringToBoolean(t)
    },
    enablePartialDataSend: function (t) {
        this._changed = convertStringToBoolean(t)
    },
    setUpdateMode: function (t, e) {
        this.autoUpdate = "cell" == t, this.updateMode = t, this.dnd = e
    },
    ignore: function (t, e) {
        this._silent_mode = !0, t.call(e || window), this._silent_mode = !1
    },
    setUpdated: function (t, e, n) {
        if (!this._silent_mode) {
            var i = this.findRow(t);
            n = n || "updated";
            var a = this.obj.getUserData(t, this.action_param);
            a && "updated" == n && (n = a), e ? (this.set_invalid(t, !1), this.updatedRows[i] = t, this.obj.setUserData(t, this.action_param, n), this._in_progress[t] && (this._in_progress[t] = "wait")) : this.is_invalid(t) || (this.updatedRows.splice(i, 1), this.obj.setUserData(t, this.action_param, "")), e || this._clearUpdateFlag(t), this.markRow(t, e, n), e && this.autoUpdate && this.sendData(t)
        }
    },
    _clearUpdateFlag: function () { },
    markRow: function (t, e, n) {
        var i = "",
            a = this.is_invalid(t);
        if (a && (i = this.styles[a], e = !0), this.callEvent("onRowMark", [t, e, n, a]) && (i = this.styles[e ? n : "clear"] + i, this.obj[this._methods[0]](t, i), a && a.details)) {
            i += this.styles[a + "_cell"];
            for (var s = 0; s < a.details.length; s++) a.details[s] && this.obj[this._methods[1]](t, s, i)
        }
    },
    getState: function (t) {
        return this.obj.getUserData(t, this.action_param)
    },
    is_invalid: function (t) {
        return this._invalid[t]
    },
    set_invalid: function (t, e, n) {
        n && (e = {
            value: e,
            details: n,
            toString: function () {
                return this.value.toString()
            }
        }), this._invalid[t] = e
    },
    checkBeforeUpdate: function () {
        return !0
    },
    sendData: function (t) {
        return !this._waitMode || "tree" != this.obj.mytype && !this.obj._h2 ? (this.obj.editStop && this.obj.editStop(), "undefined" == typeof t || this._tSend ? this.sendAllData() : this._in_progress[t] ? !1 : (this.messages = [], !this.checkBeforeUpdate(t) && this.callEvent("onValidationError", [t, this.messages]) ? !1 : (this._beforeSendData(this._getRowData(t), t), void 0))) : void 0
    },
    _beforeSendData: function (t, e) {
        return this.callEvent("onBeforeUpdate", [e, this.getState(e), t]) ? (this._sendData(t, e), void 0) : !1
    },
    serialize: function (t, e) {
        if ("string" == typeof t) return t;
        if ("undefined" != typeof e) return this.serialize_one(t, "");
        var n = [],
            i = [];
        for (var a in t) t.hasOwnProperty(a) && (n.push(this.serialize_one(t[a], a + this.post_delim)), i.push(a));
        return n.push("ids=" + this.escape(i.join(","))), dhtmlx.security_key && n.push("dhx_security=" + dhtmlx.security_key), n.join("&")
    },
    serialize_one: function (t, e) {
        if ("string" == typeof t) return t;
        var n = [];
        for (var i in t) t.hasOwnProperty(i) && n.push(this.escape((e || "") + i) + "=" + this.escape(t[i]));
        return n.join("&")
    },
    _sendData: function (t, e) {
        if (t) {
            if (!this.callEvent("onBeforeDataSending", e ? [e, this.getState(e), t] : [null, null, t])) return !1;
            e && (this._in_progress[e] = (new Date).valueOf());
            var n = new dtmlXMLLoaderObject(this.afterUpdate, this, !0),
                i = this.serverProcessor + (this._user ? getUrlSymbol(this.serverProcessor) + ["dhx_user=" + this._user, "dhx_version=" + this.obj.getUserData(0, "version")].join("&") : "");
            "POST" != this._tMode ? n.loadXML(i + (-1 != i.indexOf("?") ? "&" : "?") + this.serialize(t, e)) : n.loadXML(i, !0, this.serialize(t, e)), this._waitMode++
        }
    },
    sendAllData: function () {
        if (this.updatedRows.length) {
            this.messages = [];
            for (var t = !0, e = 0; e < this.updatedRows.length; e++) t &= this.checkBeforeUpdate(this.updatedRows[e]);
            if (!t && !this.callEvent("onValidationError", ["", this.messages])) return !1;
            if (this._tSend) this._sendData(this._getAllData());
            else
                for (var e = 0; e < this.updatedRows.length; e++)
                    if (!this._in_progress[this.updatedRows[e]]) {
                        if (this.is_invalid(this.updatedRows[e])) continue;
                        if (this._beforeSendData(this._getRowData(this.updatedRows[e]), this.updatedRows[e]), this._waitMode && ("tree" == this.obj.mytype || this.obj._h2)) return
                    }
        }
    },
    _getAllData: function () {
        for (var t = {}, e = !1, n = 0; n < this.updatedRows.length; n++) {
            var i = this.updatedRows[n];
            this._in_progress[i] || this.is_invalid(i) || this.callEvent("onBeforeUpdate", [i, this.getState(i)]) && (t[i] = this._getRowData(i, i + this.post_delim), e = !0, this._in_progress[i] = (new Date).valueOf())
        }
        return e ? t : null
    },
    setVerificator: function (t, e) {
        this.mandatoryFields[t] = e || function (t) {
            return "" != t
        }
    },
    clearVerificator: function (t) {
        this.mandatoryFields[t] = !1
    },
    findRow: function (t) {
        var e = 0;
        for (e = 0; e < this.updatedRows.length && t != this.updatedRows[e]; e++);
        return e
    },
    defineAction: function (t, e) {
        this._uActions || (this._uActions = []), this._uActions[t] = e
    },
    afterUpdateCallback: function (t, e, n, i) {
        var a = t,
            s = "error" != n && "invalid" != n;
        if (s || this.set_invalid(t, n), this._uActions && this._uActions[n] && !this._uActions[n](i)) return delete this._in_progress[a];
        "wait" != this._in_progress[a] && this.setUpdated(t, !1);
        var r = t;
        switch (n) {
            case "inserted":
            case "insert":
                e != t && (this.obj[this._methods[2]](t, e), t = e);
                break;
            case "delete":
            case "deleted":
                return this.obj.setUserData(t, this.action_param, "true_deleted"), this.obj[this._methods[3]](t), delete this._in_progress[a], this.callEvent("onAfterUpdate", [t, n, e, i])
        }
        "wait" != this._in_progress[a] ? (s && this.obj.setUserData(t, this.action_param, ""), delete this._in_progress[a]) : (delete this._in_progress[a], this.setUpdated(e, !0, this.obj.getUserData(t, this.action_param))), this.callEvent("onAfterUpdate", [r, n, e, i])
    },
    afterUpdate: function (t, e, n, i, a) {
        if (a.getXMLTopNode("data"), a.xmlDoc.responseXML) {
            for (var s = a.doXPath("//data/action"), r = 0; r < s.length; r++) {
                var o = s[r],
                    d = o.getAttribute("type"),
                    l = o.getAttribute("sid"),
                    h = o.getAttribute("tid");
                t.afterUpdateCallback(l, h, d, o)
            }
            t.finalizeUpdate()
        }
    },
    finalizeUpdate: function () {
        this._waitMode && this._waitMode--, ("tree" == this.obj.mytype || this.obj._h2) && this.updatedRows.length && this.sendData(), this.callEvent("onAfterUpdateFinish", []), this.updatedRows.length || this.callEvent("onFullSync", [])
    },
    init: function (t) {
        this.obj = t, this.obj._dp_init && this.obj._dp_init(this)
    },
    setOnAfterUpdate: function (t) {
        this.attachEvent("onAfterUpdate", t)
    },
    enableDebug: function () { },
    setOnBeforeUpdateHandler: function (t) {
        this.attachEvent("onBeforeDataSending", t)
    },
    setAutoUpdate: function (t, e) {
        t = t || 2e3, this._user = e || (new Date).valueOf(), this._need_update = !1, this._loader = null, this._update_busy = !1, this.attachEvent("onAfterUpdate", function (t, e, n, i) {
            this.afterAutoUpdate(t, e, n, i)
        }), this.attachEvent("onFullSync", function () {
            this.fullSync()
        });
        var n = this;
        window.setInterval(function () {
            n.loadUpdate()
        }, t)
    },
    afterAutoUpdate: function (t, e) {
        return "collision" == e ? (this._need_update = !0, !1) : !0
    },
    fullSync: function () {
        return 1 == this._need_update && (this._need_update = !1, this.loadUpdate()), !0
    },
    getUpdates: function (t, e) {
        return this._update_busy ? !1 : (this._update_busy = !0, this._loader = this._loader || new dtmlXMLLoaderObject(!0), this._loader.async = !0, this._loader.waitCall = e, this._loader.loadXML(t), void 0)
    },
    _v: function (t) {
        return t.firstChild ? t.firstChild.nodeValue : ""
    },
    _a: function (t) {
        for (var e = [], n = 0; n < t.length; n++) e[n] = this._v(t[n]);
        return e
    },
    loadUpdate: function () {
        var t = this,
            e = this.obj.getUserData(0, "version"),
            n = this.serverProcessor + getUrlSymbol(this.serverProcessor) + ["dhx_user=" + this._user, "dhx_version=" + e].join("&");
        n = n.replace("editing=true&", ""), this.getUpdates(n, function () {
            var e = t._loader.doXPath("//userdata");
            t.obj.setUserData(0, "version", t._v(e[0]));
            var n = t._loader.doXPath("//update");
            if (n.length) {
                t._silent_mode = !0;
                for (var i = 0; i < n.length; i++) {
                    var a = n[i].getAttribute("status"),
                        s = n[i].getAttribute("id"),
                        r = n[i].getAttribute("parent");
                    switch (a) {
                        case "inserted":
                            t.callEvent("insertCallback", [n[i], s, r]);
                            break;
                        case "updated":
                            t.callEvent("updateCallback", [n[i], s, r]);
                            break;
                        case "deleted":
                            t.callEvent("deleteCallback", [n[i], s, r])
                    }
                }
                t._silent_mode = !1
            }
            t._update_busy = !1, t = null
        })
    }
}, dhtmlx.assert = function (t, e) {
    t || dhtmlx.message({
        type: "error",
        text: e,
        expire: -1
    })
}, gantt.init = function (t, e, n) {
    e && n && (this.config.start_date = this._min_date = new Date(e), this.config.end_date = this._max_date = new Date(n)), this._init_skin(), this.config.scroll_size || (this.config.scroll_size = this._detectScrollSize()), this._reinit(t), this.attachEvent("onLoadEnd", this.render), dhtmlxEvent(window, "resize", this._on_resize), this.init = function (t) {
        this.$container && (this.$container.innerHTML = ""), this._reinit(t)
    }, this.callEvent("onGanttReady", [])
}, gantt._reinit = function (t) {
    this._init_html_area(t), this._set_sizes(), this._task_area_pulls = {}, this._task_area_renderers = {}, this._init_touch_events(), this._init_templates(), this._init_grid(), this._init_tasks(), this.render(), this._set_scroll_events(), dhtmlxEvent(this.$container, "click", this._on_click), dhtmlxEvent(this.$container, "dblclick", this._on_dblclick), dhtmlxEvent(this.$container, "mousemove", this._on_mousemove), dhtmlxEvent(this.$container, "contextmenu", this._on_contextmenu)
}, gantt._init_html_area = function (t) {
    this._obj = "string" == typeof t ? document.getElementById(t) : t, dhtmlx.assert(this._obj, "Invalid html container: " + t);
    var e = "<div class='gantt_container marketing-activitity-wrapper'><div class='gantt_grid'></div><div class='gantt_task'></div>";
    e += "<div class='gantt_ver_scroll'><div></div></div><div class='gantt_hor_scroll'><div></div></div></div>", this._obj.innerHTML = e, this.$container = this._obj.firstChild;
    var n = this.$container.childNodes;
    this.$grid = n[0], this.$task = n[1], this.$scroll_ver = n[2], this.$scroll_hor = n[3], this.$grid.innerHTML = "<div class='gantt_grid_scale'></div><div class='gantt_grid_data'></div>", this.$grid_scale = this.$grid.childNodes[0], this.$grid_data = this.$grid.childNodes[1], this.$task.innerHTML = "<div class='gantt_task_scale'></div><div class='gantt_data_area'><div class='gantt_task_bg'></div><div class='gantt_links_area'></div><div class='gantt_bars_area'></div></div>", this.$task_scale = this.$task.childNodes[0], this.$task_data = this.$task.childNodes[1], this.$task_bg = this.$task_data.childNodes[0], this.$task_links = this.$task_data.childNodes[1], this.$task_bars = this.$task_data.childNodes[2]
}, gantt.$click = {
    buttons: {
        edit: function (t) {
            gantt.showLightbox(t)
        },
        "delete": function (t) {
            var e = gantt.locale.labels.confirm_deleting,
                n = gantt.locale.labels.confirm_deleting_title;
            gantt._dhtmlx_confirm(e, n, function () {
                gantt.deleteTask(t), gantt.hideLightbox()
            })
        }
    }
}, gantt._set_sizes = function () {
    this._x = this._obj.clientWidth, this._y = this._obj.clientHeight, this._x < 20 || this._y < 20 || (this.$grid.style.height = this.$task.style.height = this._y - this.$scroll_hor.offsetHeight - 2 + "px", this.$grid_data.style.height = this.$task_data.style.height = this._y - (this.config.scale_height || 0) - this.$scroll_hor.offsetHeight - 2 + "px", this.$grid.style.width = this.config.grid_width - 1 + "px", this.$grid_data.style.width = this.config.grid_width - 1 + "px", this.$task.style.width = this._x - this.config.grid_width - 2 + "px")
}, gantt.getScrollState = function () {
    return {
        x: this.$task.scrollLeft,
        y: this.$task_data.scrollTop
    }
}, gantt.scrollTo = function (t, e) {
    null !== t && (this.$task.scrollLeft = t), null !== e && (this.$task_data.scrollTop = e)
}, gantt._on_resize = gantt.setSizes = function () {
    gantt._set_sizes(), gantt._scroll_resize()
}, gantt.render = function () {
    if (this._render_grid(), this._render_tasks_scales(), this._scroll_resize(), this._on_resize(), this._render_data(), this.config.initial_scroll) {
        var t = this._order[0] || 0;
        t && this.showTask(t)
    }
}, gantt._set_scroll_events = function () {
    dhtmlxEvent(this.$scroll_hor, "scroll", function () {
        if (!gantt._touch_scroll_active) {
            var t = gantt.$scroll_hor.scrollLeft;
            gantt._scroll_task_area(t)
        }
    }), dhtmlxEvent(this.$scroll_ver, "scroll", function () {
        if (!gantt._touch_scroll_active) {
            var t = gantt.$scroll_ver.scrollTop;
            gantt.$grid_data.scrollTop = t, gantt._scroll_task_area(null, t)
        }
    }), dhtmlxEvent(this.$task, "scroll", function () {
        var t = gantt.$task.scrollLeft;
        gantt.$scroll_hor.scrollLeft = t
    }), dhtmlxEvent(this.$task_data, "scroll", function () {
        var t = gantt.$task_data.scrollTop;
        gantt.$scroll_ver.scrollTop = gantt.$grid_data.scrollTop = t
    }), dhtmlxEvent(gantt.$container, "mousewheel", function (t) {
        if (t.wheelDeltaX) {
            var e = t.wheelDeltaX / -40,
                n = gantt.$task.scrollLeft + 30 * e;
            gantt._scroll_task_area(n, null), gantt.$scroll_hor.scrollTop = i
        } else {
            var e = t.wheelDelta / -40;
            "undefined" == typeof t.wheelDelta && (e = t.detail);
            var i = gantt.$grid_data.scrollTop + 30 * e;
            gantt._scroll_task_area(null, i), gantt.$scroll_ver.scrollTop = i
        }
        return t.preventDefault && t.preventDefault(), t.cancelBubble = !0, !1
    })
}, gantt._scroll_resize = function () {
    if (!(this._x < 20 || this._y < 20)) {
        var t = this.config.grid_width,
            e = this._x - t,
            n = this._y - this.config.scale_height,
            i = this.$task_data.offsetWidth - this.config.scroll_size,
            a = this.config.row_height * this._order.length,
            s = i > e,
            r = a > n;
        this.$scroll_hor.style.display = s ? "block" : "none", this.$scroll_hor.style.height = (s ? this.config.scroll_size : 0) + "px", this.$scroll_hor.style.width = this._x - (r ? this.config.scroll_size : 2) + "px", this.$scroll_hor.firstChild.style.width = i + t + this.config.scroll_size + 2 + "px", this.$scroll_ver.style.display = r ? "block" : "none", this.$scroll_ver.style.width = (r ? this.config.scroll_size : 0) + "px", this.$scroll_ver.style.height = this._y - (s ? this.config.scroll_size : 0) - this.config.scale_height + "px", this.$scroll_ver.style.top = this.config.scale_height + "px", this.$scroll_ver.firstChild.style.height = this.config.scale_height + a + "px"
    }
}, gantt.locate = function (t) {
    var e = gantt._get_target_node(t);
    if ("gantt_task_cell" == e.className) return null;
    for (var n = arguments[1] || this.config.task_attribute; e;) {
        if (e.getAttribute) {
            var i = e.getAttribute(n);
            if (i) return i
        }
        e = e.parentNode
    }
    return null
}, gantt._get_target_node = function (t) {
    var e;
    return t.tagName ? e = t : (t = t || window.event, e = t.target || t.srcElement), e
}, gantt._trim = function (t) {
    var e = String.prototype.trim || function () {
        return this.replace(/^\s+|\s+$/g, "")
    };
    return e.apply(t)
}, gantt._locate_css = function (t, e, n) {
    void 0 === n && (n = !0);
    for (var i = gantt._get_target_node(t), a = ""; i;) {
        if (a = i.className) {
            var s = a.indexOf(e);
            if (s >= 0) {
                if (!n) return i;
                var r = 0 === s || !gantt._trim(a.charAt(s - 1)),
                    o = s + e.length >= a.length || !gantt._trim(a.charAt(s + e.length));
                if (r && o) return i
            }
        }
        i = i.parentNode
    }
    return null
}, gantt._locateHTML = function (t, e) {
    var n = gantt._get_target_node(t);
    for (e = e || this.config.task_attribute; n;) {
        if (n.getAttribute) {
            var i = n.getAttribute(e);
            if (i) return n
        }
        n = n.parentNode
    }
    return null
}, gantt.getTaskRowNode = function (t) {
    for (var e = this.$grid_data.childNodes, n = this.config.task_attribute, i = 0; i < e.length; i++)
        if (e[i].getAttribute) {
            var a = e[i].getAttribute(n);
            if (a == t) return e[i]
        }
    return null
}, gantt.getState = function () {
    return {
        drag_id: this._tasks_dnd.drag.id,
        drag_mode: this._tasks_dnd.drag.mode,
        selected_task: this._selected_task,
        min_date: this._min_date,
        max_date: this._max_date,
        lightbox: this._lightbox_id
    }
}, gantt._checkTimeout = function (t, e) {
    if (!e) return !0;
    var n = 1e3 / e;
    return 1 > n ? !0 : t._on_timeout ? !1 : (setTimeout(function () {
        delete t._on_timeout
    }, n), t._on_timeout = !0)
}, gantt.selectTask = function (t) {
    if (this.config.select_task) {
        if (t) {
            if (this._selected_task == t) return this._selected_task;
            if (!this.callEvent("onBeforeTaskSelected", [t])) return !1;
            this.unselectTask(), this._selected_task = t, this.refreshTask(t), this.callEvent("onTaskSelected", [t])
        }
        return this._selected_task
    }
}, gantt.unselectTask = function () {
    var t = this._selected_task;
    t && (this._selected_task = null, this.refreshTask(t), this.callEvent("onTaskUnselected", [t]))
}, gantt.getSelectedId = function () {
    return dhtmlx.defined(this._selected_task) ? this._selected_task : null
}, gantt.date = {
    init: function () {
        for (var t = gantt.locale.date.month_short, e = gantt.locale.date.month_short_hash = {}, n = 0; n < t.length; n++) e[t[n]] = n;
        for (var t = gantt.locale.date.month_full, e = gantt.locale.date.month_full_hash = {}, n = 0; n < t.length; n++) e[t[n]] = n
    },
    date_part: function (t) {
        return t.setHours(0), t.setMinutes(0), t.setSeconds(0), t.setMilliseconds(0), t.getHours() && t.setTime(t.getTime() + 36e5 * (24 - t.getHours())), t
    },
    time_part: function (t) {
        return (t.valueOf() / 1e3 - 60 * t.getTimezoneOffset()) % 86400
    },
    week_start: function (t) {
        var e = t.getDay();
        return gantt.config.start_on_monday && (0 === e ? e = 6 : e--), this.date_part(this.add(t, -1 * e, "day"))
    },
    month_start: function (t) {
        return t.setDate(1), this.date_part(t)
    },
    year_start: function (t) {
        return t.setMonth(0), this.month_start(t)
    },
    day_start: function (t) {
        return this.date_part(t)
    },
    hour_start: function (t) {
        var e = t.getHours();
        return this.day_start(t), t.setHours(e), t
    },
    minute_start: function (t) {
        var e = t.getMinutes();
        return this.hour_start(t), t.setMinutes(e), t
    },
    add: function (t, e, n) {
        var i = new Date(t.valueOf());
        switch (n) {
            case "week":
                e *= 7;
            case "day":
                i.setDate(i.getDate() + e), !t.getHours() && i.getHours() && i.setTime(i.getTime() + 36e5 * (24 - i.getHours()));
                break;
            case "month":
                i.setMonth(i.getMonth() + e);
                break;
            case "year":
                i.setYear(i.getFullYear() + e);
                break;
            case "hour":
                i.setHours(i.getHours() + e);
                break;
            case "minute":
                i.setMinutes(i.getMinutes() + e);
                break;
            default:
                return gantt.date["add_" + n](t, e, n)
        }
        return i
    },
    to_fixed: function (t) {
        return 10 > t ? "0" + t : t
    },
    copy: function (t) {
        return new Date(t.valueOf())
    },
    date_to_str: function (t, e) {
        return t = t.replace(/%[a-zA-Z]/g, function (t) {
            switch (t) {
                case "%d":
                    return '"+gantt.date.to_fixed(date.getDate())+"';
                case "%m":
                    return '"+gantt.date.to_fixed((date.getMonth()+1))+"';
                case "%j":
                    return '"+date.getDate()+"';
                case "%n":
                    return '"+(date.getMonth()+1)+"';
                case "%y":
                    return '"+gantt.date.to_fixed(date.getFullYear()%100)+"';
                case "%Y":
                    return '"+date.getFullYear()+"';
                case "%D":
                    return '"+gantt.locale.date.day_short[date.getDay()]+"';
                case "%l":
                    return '"+gantt.locale.date.day_full[date.getDay()]+"';
                case "%M":
                    return '"+gantt.locale.date.month_short[date.getMonth()]+"';
                case "%F":
                    return '"+gantt.locale.date.month_full[date.getMonth()]+"';
                case "%h":
                    return '"+gantt.date.to_fixed((date.getHours()+11)%12+1)+"';
                case "%g":
                    return '"+((date.getHours()+11)%12+1)+"';
                case "%G":
                    return '"+date.getHours()+"';
                case "%H":
                    return '"+gantt.date.to_fixed(date.getHours())+"';
                case "%i":
                    return '"+gantt.date.to_fixed(date.getMinutes())+"';
                case "%a":
                    return '"+(date.getHours()>11?"pm":"am")+"';
                case "%A":
                    return '"+(date.getHours()>11?"PM":"AM")+"';
                case "%s":
                    return '"+gantt.date.to_fixed(date.getSeconds())+"';
                case "%W":
                    return '"+gantt.date.to_fixed(gantt.date.getISOWeek(date))+"';
                default:
                    return t
            }
        }), e && (t = t.replace(/date\.get/g, "date.getUTC")), new Function("date", 'return "' + t + '";')
    },
    str_to_date: function (t, e) {
        for (var n = "var temp=date.match(/[a-zA-Z]+|[0-9]+/g);", i = t.match(/%[a-zA-Z]/g), a = 0; a < i.length; a++) switch (i[a]) {
            case "%j":
            case "%d":
                n += "set[2]=temp[" + a + "]||1;";
                break;
            case "%n":
            case "%m":
                n += "set[1]=(temp[" + a + "]||1)-1;";
                break;
            case "%y":
                n += "set[0]=temp[" + a + "]*1+(temp[" + a + "]>50?1900:2000);";
                break;
            case "%g":
            case "%G":
            case "%h":
            case "%H":
                n += "set[3]=temp[" + a + "]||0;";
                break;
            case "%i":
                n += "set[4]=temp[" + a + "]||0;";
                break;
            case "%Y":
                n += "set[0]=temp[" + a + "]||0;";
                break;
            case "%a":
            case "%A":
                n += "set[3]=set[3]%12+((temp[" + a + "]||'').toLowerCase()=='am'?0:12);";
                break;
            case "%s":
                n += "set[5]=temp[" + a + "]||0;";
                break;
            case "%M":
                n += "set[1]=gantt.locale.date.month_short_hash[temp[" + a + "]]||0;";
                break;
            case "%F":
                n += "set[1]=gantt.locale.date.month_full_hash[temp[" + a + "]]||0;"
        }
        var s = "set[0],set[1],set[2],set[3],set[4],set[5]";
        return e && (s = " Date.UTC(" + s + ")"), new Function("date", "var set=[0,0,1,0,0,0]; " + n + " return new Date(" + s + ");")
    },
    getISOWeek: function (t) {
        if (!t) return !1;
        var e = t.getDay();
        0 === e && (e = 7);
        var n = new Date(t.valueOf());
        n.setDate(t.getDate() + (4 - e));
        var i = n.getFullYear(),
            a = Math.round((n.getTime() - new Date(i, 0, 1).getTime()) / 864e5),
            s = 1 + Math.floor(a / 7);
        return s
    },
    getUTCISOWeek: function (t) {
        return this.getISOWeek(t)
    },
    convert_to_utc: function (t) {
        return new Date(t.getUTCFullYear(), t.getUTCMonth(), t.getUTCDate(), t.getUTCHours(), t.getUTCMinutes(), t.getUTCSeconds())
    },
    parseDate: function (t, e) {
        return "string" == typeof t && (dhtmlx.defined(e) && (e = "string" == typeof e ? dhtmlx.defined(gantt.templates[e]) ? gantt.templates[e] : gantt.date.str_to_date(e) : gantt.templates.xml_date), t = e(t)), t
    }
}, gantt.config || (gantt.config = {}), gantt.config || (gantt.config = {}), gantt.templates || (gantt.templates = {}),
function () {
    dhtmlx.mixin(gantt.config, {
        links: {
            finish_to_start: "0",
            start_to_start: "1",
            finish_to_finish: "2"
        },
        duration_unit: "day",
        min_duration: 36e5,
        xml_date: "%d-%m-%Y %H:%i",
        api_date: "%d-%m-%Y %H:%i",
        start_on_monday: !0,
        server_utc: !1,
        show_progress: !0,
        fit_tasks: !1,
        select_task: !0,
        readonly: !1,
        date_grid: "%Y-%m-%d",
        drag_links: !0,
        drag_progress: !0,
        drag_resize: !0,
        drag_move: !0,
        drag_mode: {
            resize: "resize",
            progress: "progress",
            move: "move",
            ignore: "ignore"
        },
        round_dnd_dates: !0,
        link_wrapper_width: 20,
        autofit: !0,
        columns: [{
            name: "text",
            tree: !0,
            width: "*"
        }, {
            name: "start_date",
            align: "center"
        }, {
            name: "duration",
            align: "center"
        }, {
            name: "add",
            width: "44"
        }],
        step: 1,
        scale_unit: "day",
        subscales: [],
        time_step: 60,
        duration_step: 1,
        date_scale: "%d %M",
        task_date: "%d %F %Y",
        time_picker: "%H:%i",
        task_attribute: "task_id",
        link_attribute: "link_id",
        buttons_left: ["dhx_save_btn", "dhx_cancel_btn"],
        buttons_right: ["dhx_delete_btn"],
        lightbox: {
            sections: [{
                name: "description",
                height: 70,
                map_to: "text",
                type: "textarea",
                focus: !0
            }, {
                name: "time",
                height: 72,
                type: "duration",
                map_to: "auto"
            }]
        },
        drag_lightbox: !0,
        sort: !1,
        details_on_create: !0,
        details_on_dblclick: !0,
        initial_scroll: !0,
        task_scroll_offset: 100,
        task_height: "full",
        min_column_width: 70
    }), gantt.keys = {
        edit_save: 13,
        edit_cancel: 27
    }, gantt._init_template = function (t) {
        this.config[t] && !this.config[t].$used && (this.templates[t] = this.date.date_to_str(this.config[t]), this.config[t] = new String(this.config[t]), this.config[t].$used = !0)
    }, gantt._init_templates = function () {
        var t = gantt.locale.labels;
        t.dhx_save_btn = t.icon_save, t.dhx_cancel_btn = t.icon_cancel, t.dhx_delete_btn = t.icon_delete;
        var e = this.date.date_to_str,
            n = this.config;
        gantt._init_template("date_scale"), gantt._init_template("date_grid"), gantt._init_template("task_date"), dhtmlx.mixin(this.templates, {
            xml_date: this.date.str_to_date(n.xml_date, n.server_utc),
            xml_format: e(n.xml_date, n.server_utc),
            api_date: this.date.str_to_date(n.api_date),
            progress_text: function () {
                return ""
            },
            grid_header_class: function () {
                return ""
            },
            task_text: function (t, e, n) {
                return n.text
            },
            task_class: function () {
                return ""
            },
            grid_row_class: function () {
                return ""
            },
            task_row_class: function () {
                return ""
            },
            task_cell_class: function () {
                return ""
            },
            scale_cell_class: function () {
                return ""
            },
            scale_row_class: function () {
                return ""
            },
            task_class: function () {
                return ""
            },
            grid_indent: function () {
                return "<div class='gantt_tree_indent'></div>"
            },
            grid_folder: function (t) {
                return "<div class='gantt_tree_icon gantt_folder_" + (t.$open ? "open" : "closed") + "'></div>"
            },
            grid_file: function () {
                return "<div class='gantt_tree_icon gantt_file'></div>"
            },
            grid_open: function (t) {
                return "<div class='gantt_tree_icon gantt_" + (t.$open ? "close" : "open") + "'></div>"
            },
            grid_blank: function () {
                return "<div class='gantt_tree_icon gantt_blank'></div>"
            },
            task_time: function (t, e) {
                return gantt.templates.task_date(t) + " - " + gantt.templates.task_date(e)
            },
            time_picker: e(n.time_picker),
            link_class: function () {
                return ""
            },
            link_description: function (t) {
                var e = gantt.getTask(t.source),
                    n = gantt.getTask(t.target);
                return "<b>" + e.text + "</b> &ndash;  <b>" + n.text + "</b>"
            },
            drag_link: function (t, e, n, i) {
                t = gantt.getTask(t);
                var a = gantt.locale.labels,
                    s = "<b>" + t.text + "</b> " + (e ? a.link_start : a.link_end) + "<br/>";
                return n && (n = gantt.getTask(n), s += "<b> " + n.text + "</b> " + (i ? a.link_start : a.link_end) + "<br/>"), s
            },
            drag_link_class: function (t, e, n, i) {
                var a = "";
                if (t && n) {
                    var s = gantt.isLinkAllowed(t, n, e, i);
                    a = " " + (s ? "gantt_link_allow" : "gantt_link_deny")
                }
                return "gantt_link_tooltip" + a
            }
        }), this.callEvent("onTemplatesReady", [])
    }
}(), window.jQuery && ! function (t) {
    var e = [];
    t.fn.dhx_gantt = function (n) {
        if ("string" != typeof n) {
            var i = [];
            return this.each(function () {
                if (this && this.getAttribute && !this.getAttribute("dhxgantt")) {
                    for (var t in n) "data" != t && (gantt.config[t] = n[t]);
                    gantt.init(this), n.data && gantt.parse(n.data), i.push(gantt)
                }
            }), 1 === i.length ? i[0] : i
        }
        return e[n] ? e[n].apply(this, []) : (t.error("Method " + n + " does not exist on jQuery.dhx_gantt"), void 0)
    }
}(jQuery), gantt.locale = {
    date: {
        month_full: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
        month_short: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
        day_full: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
        day_short: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]
    },
    labels: {
        new_task: "New task",
        icon_save: "Save",
        icon_cancel: "Cancel",
        icon_details: "Details",
        icon_edit: "Edit",
        icon_delete: "Delete",
        confirm_closing: "",
        confirm_deleting: "Task will be deleted permanently, are you sure?",
        section_description: "Description",
        section_time: "Time period",
        column_text: "Task name",
        column_start_date: "Start time",
        column_duration: "Duration",
        column_add: "",
        link: "Link",
        confirm_link_deleting: "will be deleted",
        link_start: " (start)",
        link_end: " (end)",
        minutes: "Minutes",
        hours: "Hours",
        days: "Days",
        weeks: "Week",
        months: "Months",
        years: "Years"
    }
}, gantt.skins.skyblue = {
    config: {
        grid_width: 350,
        row_height: 27,
        scale_height: 27,
        task_height: 24,
        link_line_width: 1,
        link_arrow_size: 8,
        lightbox_additional_height: 75
    },
    _second_column_width: 95,
    _third_column_width: 80
}, gantt.skins.meadow = {
    config: {
        grid_width: 350,
        row_height: 27,
        scale_height: 30,
        task_height: 24,
        link_line_width: 2,
        link_arrow_size: 6,
        lightbox_additional_height: 72
    },
    _second_column_width: 95,
    _third_column_width: 80
}, gantt.skins.terrace = {
    config: {
        grid_width: 360,
        row_height: 35,
        scale_height: 35,
        task_height: 24,
        link_line_width: 2,
        link_arrow_size: 6,
        lightbox_additional_height: 75
    },
    _second_column_width: 90,
    _third_column_width: 70
}, gantt.skins.broadway = {
    config: {
        grid_width: 360,
        row_height: 35,
        scale_height: 35,
        task_height: 24,
        link_line_width: 1,
        link_arrow_size: 7,
        lightbox_additional_height: 86
    },
    _second_column_width: 90,
    _third_column_width: 80,
    _lightbox_template: "<div class='dhx_cal_ltitle'><span class='dhx_mark'>&nbsp;</span><span class='dhx_time'></span><span class='dhx_title'></span><div class='dhx_cancel_btn'></div></div><div class='dhx_cal_larea'></div>",
    _config_buttons_left: {},
    _config_buttons_right: {
        dhx_delete_btn: "icon_delete",
        dhx_save_btn: "icon_save"
    }
}, gantt.config.touch_drag = 50, gantt.config.touch = !0, gantt._init_touch_events = function () {
    "force" != this.config.touch && (this.config.touch = this.config.touch && (-1 != navigator.userAgent.indexOf("Mobile") || -1 != navigator.userAgent.indexOf("iPad") || -1 != navigator.userAgent.indexOf("Android") || -1 != navigator.userAgent.indexOf("Touch"))), this.config.touch && (window.navigator.msPointerEnabled ? this._touch_events(["MSPointerMove", "MSPointerDown", "MSPointerUp"], function (t) {
        return t.pointerType == t.MSPOINTER_TYPE_MOUSE ? null : t
    }, function (t) {
        return !t || t.pointerType == t.MSPOINTER_TYPE_MOUSE
    }) : this._touch_events(["touchmove", "touchstart", "touchend"], function (t) {
        return t.touches && t.touches.length > 1 ? null : t.touches[0] ? {
            target: t.target,
            pageX: t.touches[0].pageX,
            pageY: t.touches[0].pageY
        } : t
    }, function () {
        return !1
    }))
}, gantt._touch_events = function (t, e, n) {
    function i(t) {
        return t && t.preventDefault && t.preventDefault(), (t || event).cancelBubble = !0, !1
    }
    var a, s = 0,
        r = !1,
        o = !1,
        d = null;
    this._gantt_touch_event_ready || (this._gantt_touch_event_ready = 1, dhtmlxEvent(document.body, t[0], function (t) {
        if (!n(t) && r) {
            var l = e(t);
            if (l && d) {
                var h = d.pageX - l.pageX,
                    _ = d.pageY - l.pageY;
                !o && (Math.abs(h) > 5 || Math.abs(_) > 5) && (gantt._touch_scroll_active = o = !0, s = 0, a = gantt.getScrollState()), o && gantt.scrollTo(a.x + h, a.y + _)
            }
            return i(t)
        }
    })), dhtmlxEvent(this.$container, "contextmenu", function (t) {
        return r ? i(t) : void 0
    }), dhtmlxEvent(this.$container, t[1], function (t) {
        if (!n(t)) {
            if (t.touches && t.touches.length > 1) return r = !1, void 0;
            if (r = !0, d = e(t), d && s) {
                var a = new Date;
                500 > a - s ? (gantt._on_dblclick(d), i(t)) : s = a
            } else s = new Date
        }
    }), dhtmlxEvent(this.$container, t[2], function (t) {
        n(t) || (gantt._touch_scroll_active = r = o = !1)
    })
};