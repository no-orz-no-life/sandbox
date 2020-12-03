# blr.js

Poor man's BLR.


## Bookmarklet

(powered by [Closure Compiler](https://developers.google.com/closure/compiler))

~~~
javascript:var $jscomp=$jscomp||{};$jscomp.scope={};$jscomp.createTemplateTagFirstArg=function(d){return d.raw=d};$jscomp.createTemplateTagFirstArgWithRaw=function(d,h){d.raw=h;return d};
(function(d,h){function t(){var b=0,a=0;return function(c){0===b?"<"===c&&(a+=1,b=1):1===b?b="/"===c?2:3:2===b?">"===c&&(a-=2,b=0):3==b&&">"===c&&(b=0);return a}}function n(b){var a=document.styleSheets[document.styleSheets.length-1];void 0===a&&(a=document.createElement("style"),document.head.append(a),a=a.sheet);a.insertRule(b,a.cssRules.length)}function u(){Array.from(document.styleSheets).filter(function(b){return!b.href||b.href.startsWith(window.location.origin)}).forEach(function(b){Array.from(b.cssRules).map(function(a,
c){return[a,c]}).filter(function(a){return void 0!==a[0].selectorText&&a[0].selectorText.startsWith("span.prmnsblr")}).map(function(a){return a[1]}).sort(function(a,c){return c-a}).forEach(function(a){try{document.styleSheets[0].deleteRule(a)}catch(c){}})})}function p(){function b(c,e){a+=1;return"span.prmnsblr"+a+" { background: linear-gradient(90deg, "+c+", "+e+") !important; -webkit-background-clip: text !important; -webkit-text-fill-color: transparent !important; }"}var a=-1;q.forEach(function(c){n(b(k,
c));n(b(c,k))});return a}var k="black",q=["blue","red"];(function(){var b=!1,a=document.querySelector(d);null!==a&&(a=window.getComputedStyle(a).color,a.startsWith("rgb")&&(a=a.split(")")[0].split("(")[1].split(", ").map(function(c){return parseInt(c,10)}),128<(299*a[0]+587*a[1]+114*a[2])/1E3&&(b=!0)));null!==document.querySelector(".prmnsblr0")&&(b=!b);return b})()&&(k="white",q=["#14ff00","#e32d4d"]);if(null!==document.querySelector(".prmnsblr0"))u(),p();else{var r=function(b){var a=-1;return function(){a+=
1;a>=b&&(a=0);return'class="prmnsblr'+a+'"'}}(p());document.querySelectorAll(d).forEach(function(b){for(var a=0,c=t(),e="<span "+r()+">",l=b.innerHTML,f=0,m=0;m<l.length;m++){var g=l[m];f=c(g);0===f&&">"!==g&&(a+=1);a===h?(a=0,e+="</span><span "+r()+">"+g):e+=g}b.innerHTML=e+"</span>";0<f&&(console.log("level = "+f),console.log(l))})}})("p",60);
~~~

