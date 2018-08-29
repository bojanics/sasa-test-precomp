const PARAM_NAME_LANG = 'lang';
const PARAM_NAME_CONFIGURATION = 'Configuration';
const PARAM_NAME_CALCULATION = 'calculation';
const PARAM_NAME_DATA = 'data';
const DEFAULT_SETTING_PREFIX = "CALC_DEFAULT_";
const SETTING_NAME_SUFFIX = "_SettingName";

const DEFAULT_calculation_CODE = "./defaults/default.js";

module.exports = function (context, req) {
    context.log('Starting calculation...');
    context.log("REQBODY="+JSON.stringify(req.body));
    context.log("REQHDRS="+JSON.stringify(req.headers));

    var calcinfo = {};

    try {
        var rootloc = context.executionContext.functionDirectory;

        var lang = null;
        try
        {
            lang = req.body.lang;
        }
        catch (ex) {}
        if (lang == null)
        {
            var lang_SettingName = null;
            try
            {
                lang_SettingName = req.body.lang_SettingName;
            }
            catch (ex){}
            if (lang_SettingName != null)
            {
                var vv = process.env[lang_SettingName];
                if (vv != null)
                {
                    lang = vv;
                }
            }

        }

        var config_param = handleParameter(req.body,null,PARAM_NAME_CONFIGURATION,rootloc,null,lang,context);
        if (config_param.value!=null) {
            requireLocalOrRemote(config_param.value,config_param.isRemoteURL,context,function(err,config_json){onConfigRequired(context,req,calcinfo,err,config_json,config_param,lang);});
        } else {
           onConfigRequired(context,req,calcinfo,null,null,config_param,lang);
        }
    } catch (e) {
        var statusCode = 500;
        var statusMessage = 'Failed to execute calculation! Error name='+e.name+', Error message: '+e.message+', Error stack='+e.stack;
        handleResponse(context,req,null,calcinfo,statusCode,statusMessage);
    }           
}

// This function is executed if configuration parameter is specified, and after it is loaded or failed to load
function onConfigRequired(context,req,calcinfo,err,config_json,config_param,lang) {
    if (lang==null) {
        var lang_param = handleParameter(req.body,config_json,PARAM_NAME_LANG,null,null,null,context);
        lang = lang_param.value;
        // if lang JSON is not null, and it didn't come from JSON configuration (it came from default settings), handle Configuration again
        if (lang!=null && lang_param.source==5) {
            // now handle Configuration again with the language parameter
            config_param = handleParameter(req.body,null,PARAM_NAME_CONFIGURATION,rootloc,null,lang,context);
            if (config_param.value!=null) {
                requireLocalOrRemote(config_param.value,config_param.isRemoteURL,context,function(err,config_json){onConfigRequired(context,req,calcinfo,err,config_json,config_param,lang);});
            } else {
                onConfigRequired(context,req,calcinfo,null,null,config_param,lang);
            }
            return;
        }              
   }
   calcinfo[PARAM_NAME_LANG]=lang;
   AddResponseParam(calcinfo, config_param);

   var rootloc = context.executionContext.functionDirectory;   
   var calc_param = handleParameter(req.body,config_json,PARAM_NAME_CALCULATION,rootloc,DEFAULT_calculation_CODE,null,context)
   AddResponseParam(calcinfo, calc_param);

   var data = handleData(context,req,config_json,calcinfo);

   if (err) {
      var statusCode = 500;
      var statusMessage = 'Failed to execute calculation! Error name='+err.name+', Error message: '+err.message+', Error stack='+err.stack;
      handleResponse(context,req,null,calcinfo,statusCode,statusMessage);      
   } else {
      requireLocalOrRemote(calc_param.value,calc_param.isRemoteURL,context,function(err,calc){onCalcRequired(context,req,calcinfo,data,err,calc);});
   }

}

// This function is executed after calculation is loaded or failed to load
function onCalcRequired(context,req,calcinfo,data,err,calc) {
   if (err) {
      var statusCode = 500;
      var statusMessage = 'Failed to execute calculation! Error name='+err.name+', Error message: '+err.message+', Error stack='+err.stack;
      handleResponse(context,req,null,calcinfo,statusCode,statusMessage);      
   } else {
      handleCalc(context,req,calcinfo,data,calc);
   }
}

// handles calculation and writes response      
function handleCalc(context,req,calcinfo,data,calc) {
   var calcres = null;
   var statusCode = null;
   var statusMessage = null;
   try {
      context.log("INPUT DATA="+JSON.stringify(data));
      var crstr = calc.calculate(data);
      context.log("OUTPUT DATA="+crstr);
      calcres = JSON.parse(crstr);
      for (var p in calcres) {
         if (p.toLowerCase().startsWith("xlew_")) {
            delete calcres[p];
         }
      }    
      statusCode = 200;
      statusMessage = 'Calculation successfully executed.';
   } catch (e) {
      statusCode = 500;
      statusMessage = 'Failed to execute calculation! Error name='+e.name+', Error message: '+e.message+', Error stack='+e.stack;
   }
   handleResponse(context,req,calcres,calcinfo,statusCode,statusMessage);
}

// writes response (error or success)
function handleResponse(context,req,calcres,calcinfo,statusCode,statusMessage) {
    context.res = {
        body: {"calcResult":calcres,"calcInfo":calcinfo,"statusCode":statusCode, "statusMessage":statusMessage},
        status: statusCode
    };
    
    context.log(statusMessage);
    context.log('calcres='+JSON.stringify(calcres));
    context.done();
}

// merges request and configuration data payload
function handleData(context,req,config_json,calcinfo) {
   var data = null;
   var data_req = null;
   var data_config_file = null;
   try {
      data_req = req.body[PARAM_NAME_DATA];
   } catch (e) {
   }
   if (config_json) {
      data_config_file = config_json[PARAM_NAME_DATA];
   }
   if (data_config_file!=null) {
      // clone data_config_file so that original stays untouched
      data_config_file = JSON.parse(JSON.stringify(data_config_file));
      if (data_req!=null) {
          data = Object.assign(data_config_file, data_req);
      } else {
          data = data_config_file;
      }
   } else {
      data = data_req;
   }
   // clone data so that it stays untouched in calcinfo object
   calcinfo[PARAM_NAME_DATA] = data;
   return data;
}

// this function loads local or remote configuration/calculation
function requireLocalOrRemote(uri,isRemoteURL,context,onRequireLocalOrRemote) {
    //context.log("REQ FROM "+(isRemoteURL ? "URL" : "FILE")+": "+uri)
    if (isRemoteURL) {
        var requireFromUrl = require('./requireFromUrl.js');
        requireFromUrl(uri,onRequireLocalOrRemote);
    } else {
        // must clear the cache because calculation js can change
        delete require.cache[require.resolve(uri)];
        onRequireLocalOrRemote(null,require(uri));
    }    
}
   
// add parameter to response   
function AddResponseParam(calcinfo,paraminfo)
{
    calcinfo[paraminfo.name]=paraminfo.orig_value;
}


// returns object representing parameter for calculation (either Configuration or calculation)
// it considers "normal" request parameter, and "setting" request parameter, as well as the same parameter given in JSON configuration
// if non of them is found, it tries to use DEFAULT application setting. "lang" parameter is also considered if provided.
function handleParameter(json,config_json,name,rootloc,defaultVal,lang,context) {
    //context.log('handling param '+name);
    var ret = {};
    ret.name = name;
    ret.isRemoteURL = false;
    ret.source = 0;
    var paramResolved = false;
    if (json!=null) {
        var v = json[name];
        if (v!=null) {
            ret.value = v;
            ret.orig_value = v;
            ret.source = 1;
            paramResolved = true;
        }
        if (!paramResolved) {
            var psn = name + SETTING_NAME_SUFFIX;
            v = json[psn];
            if (v != null)
            {
                var vv = process.env[v];
                if (vv != null)
                {
                    ret.value = vv;
                    ret.orig_value = vv;
                    ret.source = 2;
                    paramResolved = true;
                }
            }
        }
    }
    if (!paramResolved && config_json!=null) {
        var v = config_json[name];
        if (v!=null) {
            ret.value = v;
            ret.orig_value = v;
            ret.source = 3;
            paramResolved = true;
        }
        if (!paramResolved) {
            var psn = name + SETTING_NAME_SUFFIX;
            v = config_json[psn];
            if (v != null)
            {
                var vv = process.env[v];
                if (vv != null)
                {
                    ret.value = vv;
                    ret.orig_value = vv;
                    ret.source = 4;
                    paramResolved = true;
                }
            }
        }
    }
    // handle DEFAULT setting
    if (!paramResolved)
    {
        var config_DEFAULT_SettingName = DEFAULT_SETTING_PREFIX + name;
        var v = process.env[config_DEFAULT_SettingName];
        if (v != null)
        {
            ret.value = v;
            ret.orig_value = v;
            ret.source = 5;
            paramResolved = true;            
        }
    }

    // handle default CODE value
    if (!paramResolved)
    {
        if (defaultVal != null)
        {
            ret.value = defaultVal;
            ret.orig_value = defaultVal;
            ret.source = 6;
            paramResolved = true;
        }
    }

    // handle language (modify the result if the language parameter exists and the value for the parameter is not from config JSON)
    if (lang!=null && ret.value!=null && ret.source!=3 && ret.source!=4) {
        var val = ret.value;
        val = val.replace(/\\/g, "/")
        var index_dot = val.lastIndexOf(".");
        var index_slash = val.lastIndexOf("/");

        if (index_dot > 0 && index_dot > index_slash) {
            // if there is a dot and it is after slash or backslash
            var prefix = ret.value.substring(0, index_dot);
            var suffix = ret.value.substring(index_dot);
            ret.value = prefix+"_"+lang+suffix;
        } else {
            // if there is no dot
            ret.value = ret.value + "_"+lang;
        }
        ret.orig_value = ret.value;
    }

    if (ret.value != null)
    {
        if (rootloc != null)
        {
            var url = require("url");
            var isurl = false;
            try
            {
                var result = url.parse(ret.value);
                if (result.hostname!=null) {
                    isurl = true;
                    if (result.hostname!='') {
                        ret.isRemoteURL = true;
                    }
                }
            }
            catch (e){}
            if (!isurl) {
                try
                {
                    var result = url.parse(rootloc+"/"+ret.value);
                    if (result.hostname!=null) {
                        isurl = true;
                        ret.value = rootloc + "/" + ret.value;
                    }
                }
                catch (e2){}
            }
        }
    }

    return ret;
}
