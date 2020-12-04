# blr.js

Poor man's BLR.


## Bookmarklet

(powered by [Closure Compiler](https://developers.google.com/closure/compiler))

~~~
var $jscomp=$jscomp||{};$jscomp.scope={};$jscomp.createTemplateTagFirstArg=function(e){return e.raw=e};$jscomp.createTemplateTagFirstArgWithRaw=function(e,k){e.raw=k;return e};
(function(e,k){function r(){var b=0,a=0;return function(c){0===b?"<"===c&&(a+=1,b=1):1===b?b="/"===c?2:3:2===b?">"===c&&(a-=2,b=0):3==b&&">"===c&&(b=0);return a}}function t(){Array.from(document.styleSheets).forEach(function(b){try{Array.from(sheet.cssRules).map(function(a,c){return[a,c]}).filter(function(a){return void 0!==a[0].selectorText&&a[0].selectorText.startsWith("span.prmnsblr")}).map(function(a){return a[1]}).sort(function(a,c){return c-a}).forEach(function(a){document.styleSheets[0].deleteRule(a)})}catch(a){}})}
function n(){function b(d,f){a+=1;return"span.prmnsblr"+a+" { background: linear-gradient(90deg, "+d+", "+f+") !important; -webkit-background-clip: text !important; -webkit-text-fill-color: transparent !important; }"}var a=-1,c=document.createElement("style");document.head.append(c);var g=c.sheet;p.forEach(function(d){var f=b(l,d);g.insertRule(f,g.cssRules.length);d=b(d,l);g.insertRule(d,g.cssRules.length)});return a}var l="black",p=["blue","red"];(function(){var b=!1,a=document.querySelector(e);
null!==a&&(a=window.getComputedStyle(a).color,a.startsWith("rgb")&&(a=a.split(")")[0].split("(")[1].split(", ").map(function(c){return parseInt(c,10)}),128<(299*a[0]+587*a[1]+114*a[2])/1E3&&(b=!0)));null!==document.querySelector(".prmnsblr0")&&(b=!b);return b})()&&(l="white",p=["#14ff00","#e32d4d"]);if(null!==document.querySelector(".prmnsblr0"))t(),n();else{var q=function(b){var a=-1;return function(){a+=1;a>b&&(a=0);return'class="prmnsblr'+a+'"'}}(n());document.querySelectorAll(e).forEach(function(b){for(var a=
0,c=r(),g="<span "+q()+">",d=b.innerHTML,f=0,m=0;m<d.length;m++){var h=d[m];f=c(h);0===f&&">"!==h&&(a+=1);a===k?(a=0,g+="</span><span "+q()+">"+h):g+=h}b.innerHTML=g+"</span>";0<f&&(console.log("level = "+f),console.log(d))})}})("p",60);
~~~

