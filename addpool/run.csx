#r "Newtonsoft.Json"
#r "System.Configuration"
#r "Microsoft.Azure.Documents.Client.dll"

#load "NumeratorCommon.csx"

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, ExecutionContext context)
{
    log.Info("Starting creation of numerator...");
    string rootloc = context.FunctionDirectory;
    bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
    if (isLocal)
    {
        rootloc = Directory.GetParent(rootloc).FullName;
    }

    const string DEFAULT_numberOfRetries_CODE = "1";

    JObject response_body = new JObject();

    dynamic numerator_info = null;
    HttpStatusCode statusCode = HttpStatusCode.OK;
    String statusMessage = null;
    // Initialize response info object
    JObject addPoolInfo = new JObject { };

    try
    {

        // READ ALL THE POSSIBLE PARAMETERS FROM THE REQUEST
        dynamic body = null;
        dynamic json = null;
        body = req.Content.ReadAsStringAsync().Result;
        if (body != null)
        {
            //log.Info("Body=" + body);
            try
            {
                json = JsonConvert.DeserializeObject(body);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid JSON body! " + ex.Message, ex);
            }
        }

        // Check if language is set by normal/setting parameter
        string lang = null;
        try
        {
            lang = json.lang;
        }
        catch (Exception ex) { }
        if (lang == null)
        {
            string lang_SettingName = null;
            try
            {
                lang_SettingName = json.lang_SettingName;
            }
            catch (Exception ex)
            {
            }
            if (lang_SettingName != null)
            {
                string vv = System.Environment.GetEnvironmentVariable(lang_SettingName);
                if (vv != null)
                {
                    lang = vv;
                }
            }

        }

        string firstErrorMsg = null;
        // Determine value of all the parameters using request parameters, default settings and code default and also update fileinfo response
        ParamInfo Configuration_transf = CommonNumeratorUtilities.handleParameter(json, null, "Configuration", true, rootloc, null, lang, log);
        dynamic config_json = null;
        if (Configuration_transf.value != null)
        {
            using (WebClient wclient = new WebClient())
            {
                try
                {
                    string config_str = wclient.DownloadString(Configuration_transf.value);
                    config_json = JsonConvert.DeserializeObject(config_str);
                }
                catch (Exception ex)
                {
                    firstErrorMsg = "Pool will not be added because the following error occurred when trying to download configuration file: " + ex.Message;
                }
            }
        }
        // if language was not set by normal/setting parameters, check in Configuration and default settings
        if (lang == null)
        {
            ParamInfo lang_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "lang", true, null, null, null, log);
            lang = lang_transf.value;
            // if lang JSON is not null, and it didn't come from JSON configuration (it came from default settings), handle Configuration again
            if (lang != null && lang_transf.source == 5)
            {
                // now handle Configuration again with the language parameter
                firstErrorMsg = null;
                Configuration_transf = CommonNumeratorUtilities.handleParameter(json, null, "Configuration", true, rootloc, null, lang, log);
                if (Configuration_transf.value != null)
                {
                    using (WebClient wclient = new WebClient())
                    {
                        try
                        {
                            string config_str = wclient.DownloadString(Configuration_transf.value);
                            config_json = JsonConvert.DeserializeObject(config_str);
                        }
                        catch (Exception ex)
                        {
                            firstErrorMsg = "Pool will not be added because the following error occurred when trying to download configuration file: " + ex.Message;
                        }
                    }
                }
            }
        }
        addPoolInfo.Add("lang", lang);

        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, Configuration_transf, false, false, false);

        ParamInfo CosmosDBEndpoint_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBEndpoint", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBEndpoint_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, CosmosDBEndpoint_transf, false, false, false);

        ParamInfo CosmosDBAuthorizationKey_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBAuthorizationKey", false, null, CommonNumeratorUtilities.DEFAULT_CosmosDBAuthorizationKey_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, CosmosDBAuthorizationKey_transf, true, false, false);

        ParamInfo CosmosDBDatabaseId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBDatabaseId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBDatabaseId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, CosmosDBDatabaseId_transf, false, false, false);

        ParamInfo CosmosDBCollectionId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBCollectionId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBCollectionId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, CosmosDBCollectionId_transf, false, false, false);

        ParamInfo numeratorId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorId", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, numeratorId_transf, false, false, false);

        ParamInfo numeratorName_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorName", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, numeratorName_transf, false, false, false);

        ParamInfo poolPrefix_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolPrefix", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolPrefix_transf, false, false, false);

        string poolFrom = null;
        if (json != null && json.poolFrom != null)
        {
            poolFrom = json.poolFrom;
        }
        int? from = null;
        try
        {
            from = Int32.Parse(poolFrom);
        } catch (Exception ex)
        {
            if (firstErrorMsg==null)
            {
                firstErrorMsg = "Pool will not be added because the 'poolFrom' parameter value " + (poolFrom==null ? "is not specified!" : "is not an integer!");
            }
        }
        addPoolInfo.Add("poolFrom", from);

        string poolTo = null;
        if (json != null && json.poolTo != null)
        {
            poolTo = json.poolTo;
        }
        int? to = null;
        if (poolTo != null)
        {
            try
            {
                to = Int32.Parse(poolTo);
            }
            catch (Exception ex)
            {
                if (firstErrorMsg == null)
                {
                    firstErrorMsg = "Pool will not be added because the 'poolTo' parameter value is not an integer!";
                }
            }
        }
        addPoolInfo.Add("poolTo", to);

        ParamInfo poolDigits_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolDigits", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolDigits_transf, false, false, true);
        int? digits = null;
        if (poolDigits_transf.value != null)
        {
            try
            {
                digits = Int32.Parse(poolDigits_transf.value);
            }
            catch (Exception ex)
            {
                if (firstErrorMsg == null)
                {
                    firstErrorMsg = "Pool will not be added because the 'poolDigits' parameter value is not an integer!";
                }
            }                    
        }

        ParamInfo poolSuffix_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolSuffix", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolSuffix_transf, false, false, false);

        string poolWho = null;
        if (json != null && json.poolWho != null)
        {
            poolWho = json.poolWho;
        }
        addPoolInfo.Add("poolWho", poolWho);

        string poolWhen = null;
        JProperty p = json!=null ? json.Property("poolWhen") : null;
        if (p != null)
        {
            if (p.Value.Type == JTokenType.Date)
            {
                poolWhen = JsonConvert.SerializeObject(p.Value);
                poolWhen = poolWhen.Substring(1, poolWhen.Length - 2);
            }
            else
            {
                poolWhen = p.Value.Value<string>();
            }
        }
        addPoolInfo.Add("poolWhen", poolWhen);

        string poolComment = null;
        if (json != null && json.poolComment != null)
        {
            poolComment = json.poolComment;
        }
        addPoolInfo.Add("poolComment", poolComment);

        JToken poolInfo_req = null;
        if (json != null && json.poolInfo != null)
        {
            poolInfo_req = json.poolInfo;
        }
        addPoolInfo.Add("poolInfo", poolInfo_req);

        JArray poolActions_req = new JArray();
        if (json != null && json.poolActions != null)
        {
            poolActions_req = json.poolActions;
        }
        JArray poolActionsObj = new JArray();
        // Take default poolActions from configuration (if exists)
        if (config_json != null && config_json.poolActions != null)
        {
            poolActionsObj = config_json.poolActions;
        }                

        // merge objects?
        poolActionsObj.Merge(poolActions_req, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });

        addPoolInfo.Add("poolActions", poolActionsObj);

        ParamInfo numberOfRetries_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numberOfRetries", true, null, DEFAULT_numberOfRetries_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, numberOfRetries_transf, false, false, true);
        int numberOfRetries = 1;
        if (numberOfRetries_transf.value != null)
        {
            try
            {
                numberOfRetries = Int32.Parse(numberOfRetries_transf.value);
            }
            catch (Exception ex)
            {
                if (firstErrorMsg == null)
                {
                    firstErrorMsg = "Pool will not be added because the 'numberOfRetries' parameter value is not an integer!";
                }
            }
        }


        if (firstErrorMsg != null)
        {
            throw new Exception(firstErrorMsg);
        }

        if (CosmosDBEndpoint_transf.value == null)
        {
            throw new Exception("Can't add a pool to numerator without specifying the 'CosmosDBEndpoint' parameter!");
        }
        if (CosmosDBAuthorizationKey_transf.value == null)
        {
            throw new Exception("Can't add a pool to numerator without specifying the 'CosmosDBAuthorizationKey' parameter!");
        }
        if (CosmosDBDatabaseId_transf.value == null)
        {
            throw new Exception("Can't add a pool to numerator without specifying the 'CosmosDBDatabaseId' parameter!");
        }
        if (CosmosDBCollectionId_transf.value == null)
        {
            throw new Exception("Can't add a pool to numerator without specifying the 'CosmosDBCollectionId' parameter!");
        }
        if (numeratorId_transf.value == null && numeratorName_transf.value == null)
        {
            throw new Exception("Can't add a pool to numerator without specifying either 'numeratorId' or 'numeratorName' parameter (in the request, JSON config or as default app setting)!");
        }

        // determine if Id or Name should be used...if it is a name(when Id is null), then search if there is appsetting NUMERATOR_%namevalue% ... it is the Id value
        string id = numeratorId_transf.value;
        if (id==null)
        {
            // now search if there is appsetting NUMEREATOR_ % namevalue % ...it is the Id value
            string vv = System.Environment.GetEnvironmentVariable(CommonNumeratorUtilities.NUMERATOR_PREFIX + numeratorName_transf.value);
            if (vv != null)
            {
                id = vv;
            }
        }

        numerator_info = await addPool(CosmosDBEndpoint_transf.value, CosmosDBAuthorizationKey_transf.value, CosmosDBDatabaseId_transf.value, CosmosDBCollectionId_transf.value, id, numeratorName_transf.value, poolPrefix_transf.value,from,to, digits, poolSuffix_transf.value, poolWho, poolWhen, poolComment, poolActionsObj, poolInfo_req, numberOfRetries, log);

        statusCode = HttpStatusCode.OK;
        statusMessage = "Pool successfully created.";
    }
    catch (Exception e)
    {
        statusCode = HttpStatusCode.InternalServerError;
        statusMessage = "Failed to add a pool to numerator! Error message: " + CommonNumeratorUtilities.getInnerExceptionMessage(e, null) + ", stackTrace=" + e.StackTrace + ".";
    }
    log.Info(statusMessage);
    //log.Info("addPoolInfo="+addPoolInfo.ToString());
    JObject pool = null;
    try
    {
        if (numerator_info != null)
        {
            pool = JObject.Parse(JsonConvert.SerializeObject(numerator_info));
        }
    }
    catch (Exception ex)
    {
        log.Warning("Failed to convert numerator's pool to JSON!");
    }

    response_body.Add("pool", pool);
    response_body.Add("addPoolInfo", addPoolInfo);
    response_body.Add("statusCode", (int)statusCode);
    response_body.Add("statusMessage", statusMessage);

    return req.CreateResponse(statusCode, response_body);
}

private async static Task<dynamic> addPool(string endpoint, string authorizationKey, string databaseId, string collectionId, string numeratorid, string numeratorname, string prefix, int? from, int? to, int? digits, string suffix, string who, string when, string comment, dynamic actions, dynamic info, int retriesNumber,TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        _client = new DocumentClient(new Uri(endpoint), authorizationKey);
        var dbSetup = new DatabaseSetup(_client, log);
        await dbSetup.Init(databaseId, collectionId);

        NumeratorInfo ni = await dbSetup.AddPool(numeratorid, numeratorname, prefix, from, to, digits, suffix, who, when, comment, actions, info, retriesNumber);

        return ni.pool;
    }
    catch (Exception ex)
    {
        throw ex;
    }
    finally
    {
        if (_client != null)
        {
            _client.Dispose();
        }
    }
}

