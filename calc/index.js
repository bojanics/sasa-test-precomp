const PARAM_NAME_CONFIGURATION = 'Configuration';
const PARAM_NAME_CALCULATION = 'calculation';
const PARAM_NAME_DATA = 'data';
const DEFAULT_SETTING_PREFIX = "CALC_DEFAULT_";
const SETTING_NAME_SUFFIX = "_SettingName";

const DEFAULT_calculation_CODE = "./defaults/default.js";

module.exports = function (context, req) {
    context.log('Starting calculation...');
    //context.log("REQBODY="+JSON.stringify(req.body));

    var calcinfo = {};

    try {
        var rootloc = context.executionContext.functionDirectory;
        var config_param = handleParameter(req.body,null,PARAM_NAME_CONFIGURATION,rootloc,null,context);
        AddResponseParam(calcinfo, config_param);
        if (config_param.value!=null) {
            requireLocalOrRemote(config_param.value,config_param.isRemoteURL,context,function(err,config_json){onConfigRequired(context,req,calcinfo,err,config_json);});
        } else {
           onConfigRequired(context,req,calcinfo,null,null);
        }
    } catch (e) {
        var statusCode = 500;
        var statusMessage = 'Failed to execute calculation! Error name='+e.name+', Error message: '+e.message+', Error stack='+e.stack;
        handleResponse(context,req,null,calcinfo,statusCode,statusMessage);
    }           
}

// This function is executed if configuration parameter is specified, and after it is loaded or failed to load
function onConfigRequired(context,req,calcinfo,err,config_json) {
   var rootloc = context.executionContext.functionDirectory;   
   var calc_param = handleParameter(req.body,config_json,PARAM_NAME_CALCULATION,rootloc,DEFAULT_calculation_CODE,context)
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
      var crstr = calc.calculate(data);
      calcres = JSON.parse(crstr);
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
    //context.log('calcres='+crstr)
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
   calcinfo[PARAM_NAME_DATA] = data!=null ? JSON.parse(JSON.stringify(data)) : data;
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
// if non of them is found, it tries to use DEFAULT application setting...it 
function handleParameter(json,config_json,name,rootloc,defaultVal,context) {
    //context.log('handling param '+name);
    var ret = {};
    ret.name = name;
    ret.isRemoteURL = false;
    var paramResolved = false;
    if (json!=null) {
        var v = json[name];
        if (v!=null) {
            ret.value = v;
            ret.orig_value = v;
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
            paramResolved = true;
        }
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
