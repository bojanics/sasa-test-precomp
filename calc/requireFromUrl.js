'use strict';

var parse = require("url").parse;
var fetch = require("./fetchFromUrl.js");

var requireFromString = require("./requireFromString.js");

function requireFromUrl(urlString,callback) {
   var url = parse(urlString),
   href = url.href;
   fetch(parse(href), function(err, str, filename) {
      if(err) {
         callback(err);
      } else {
         try {
            var result = requireFromString(str, filename);
            callback(null,result);
         } catch (e) {
            callback(e);
         }
      }         
   });
}

module.exports = requireFromUrl;
