# blr.js

Poor man's BLR.


## Bookmarklet

(powered by [Closure Compiler](https://developers.google.com/closure/compiler))

~~~
var $jscomp=$jscomp||{};$jscomp.scope={};$jscomp.createTemplateTagFirstArg=function(g){return g.raw=g};$jscomp.createTemplateTagFirstArgWithRaw=function(g,h){g.raw=h;return g};
(function(g,h){function n(b,a,c){var d=b+"."+a.nodeName;1===a.nodeType&&(b=a.nodeName.toUpperCase(),r.has(b)||"none"===window.getComputedStyle(a).display||a.childNodes.forEach(function(f){if(1===f.nodeType)n(d,f,found);else if(3===f.nodeType){a:{var e=f.textContent;for(var l=0;l<e.length;l++)if(!t.has(e[l])){e=!1;break a}e=!0}e||c.processText(f)}}))}function m(b){return b.split(")")[0].split("(")[1].split(", ").map(function(a){return parseInt(a,10)})}function u(){Array.from(document.styleSheets).forEach(function(b){try{Array.from(b.cssRules).map(function(a,
c){return[a,c]}).filter(function(a){return void 0!==a[0].selectorText&&a[0].selectorText.startsWith("span.prmnsblr")}).map(function(a){return a[1]}).sort(function(a,c){return c-a}).forEach(function(a){document.styleSheets[0].deleteRule(a)})}catch(a){}})}var r=new Set("H1 H2 H3 H4 H5 H6 PRE CODE SCRIPT FORM A BUTTON TH INPUT NAV BR IMG SVG DT NOSCRIPT HR".split(" ")),t=new Set([" ","\t","\r","\n"]),p="rgb(0, 0, 0)",q=["rgb(0, 0, 255)","rgb(255, 0, 0)"];(function(){var b=!1,a=document.querySelector(g);
null!==a&&(a=window.getComputedStyle(a).color,a.startsWith("rgb")&&(a=m(a),128<(299*a[0]+587*a[1]+114*a[2])/1E3&&(b=!0)));null!==document.querySelector(".prmnsblr0")&&(b=!b);return b})()&&(p="rgb(255, 255, 255)",q=["rgb(24, 255, 0)","rgb(227, 55, 77)"]);var k=function(){};k.prototype.prepareStyle=function(){var b=-1,a=document.createElement("style");document.head.append(a);var c=a.sheet;q.forEach(function(d){var f=m(p);d=m(d);for(var e=0;1>e;e+=1/h){var l=parseInt(f[0]*(1-e)+d[0]*e),v=parseInt(f[1]*
(1-e)+d[1]*e),w=parseInt(f[2]*(1-e)+d[2]*e);b+=1;c.insertRule("span.prmnsblr"+b+" { color: rgb("+l+", "+v+", "+w+"); }",c.cssRules.length)}});return b};k.prototype.prepareProcess=function(){this.getClass=function(b){var a=-1;return function(){a+=1;a>=2*b&&(a=0);var c=parseInt(a/h),d=a%h;return"prmnsblr"+(0===c?d:1===c?h-d-1:2===c?h+d:2*h-d-1)}}(this.prepareStyle())};k.prototype.processText=function(b){for(var a=document.createElement("span"),c=0;c<b.textContent.length;c++){var d=b.textContent[c],
f=document.createElement("span");f.setAttribute("class",this.getClass());f.textContent=d;a.appendChild(f)}b.parentNode.replaceChild(a,b)};k=new k;null!==document.querySelector(".prmnsblr0")?(u(),k.prepareStyle()):(k.prepareProcess(),n("",document.querySelector(g),k))})("body",60);
~~~

