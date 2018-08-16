#r "Newtonsoft.Json"
#r "System.Configuration"

#load "../Shared/NumeratorCommon.csx"

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Configuration;
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
    log.Info("Exec func: " + Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") + ", RL=" + rootloc);

    JObject response_body = new JObject();

    //JObject numerator_info = new JObject { };
    Document numerator_info = new Document();
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

        ParamInfo numeratorInfo_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorInfo", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, numeratorInfo_transf, false, false, false);

        ParamInfo poolPrefix_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolPrefix", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolPrefix_transf, false, false, false);

        ParamInfo poolFrom_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolFrom", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolFrom_transf, false, false, true);

        ParamInfo poolTo_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolTo", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolTo_transf, false, false, true);

        ParamInfo poolDigits_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolDigits", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolDigits_transf, false, false, true);

        ParamInfo poolSuffix_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolSuffix", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolSuffix_transf, false, false, false);

        ParamInfo poolWho_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolWho", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolWho_transf, false, false, false);

        ParamInfo poolWhen_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolWhen", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolWhen_transf, false, false, false);

        ParamInfo poolComment_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolComment", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolComment_transf, false, false, false);

        ParamInfo poolInfo_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "poolInfo", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(addPoolInfo, poolInfo_transf, false, false, false);

        JToken poolActions_req = null;
        if (json != null && json.poolActions != null)
        {
            poolActions_req = json.poolActions;
        }
        addPoolInfo.Add("poolActions", poolActions_req);

        if (firstErrorMsg != null)
        {
            throw new Exception(firstErrorMsg);
        }

        int? from = null;
        if (poolFrom_transf.value!=null)
        {
            from = Int32.Parse(poolFrom_transf.value);
        }
        int? to = null;
        if (poolTo_transf.value != null)
        {
            to = Int32.Parse(poolTo_transf.value);
        }
        int? digits = null;
        if (poolDigits_transf.value != null)
        {
            digits = Int32.Parse(poolDigits_transf.value);
        }
        numerator_info = await addPool(CosmosDBEndpoint_transf.value, CosmosDBAuthorizationKey_transf.value, CosmosDBDatabaseId_transf.value, CosmosDBCollectionId_transf.value, numeratorId_transf.value, numeratorName_transf.value, numeratorInfo_transf.value, poolPrefix_transf.value,from,to, digits, poolSuffix_transf.value, poolWho_transf.value, poolWhen_transf.value, poolComment_transf.value, poolActions_req,poolInfo_transf.value, log);

        statusCode = HttpStatusCode.OK;
        statusMessage = "Pool successfully created.";
    }
    catch (Exception e)
    {
        statusCode = HttpStatusCode.InternalServerError;
        statusMessage = "Failed to create numerator! Error message: " + CommonNumeratorUtilities.getInnerExceptionMessage(e, null) + ", stackTrace=" + e.StackTrace + ".";
    }
    log.Info(statusMessage);
    //log.Info("fileInfo="+fileInfo.ToString());
    JObject ni = new JObject();
    //ni.Add(numerator_info.ToString());
    response_body.Add("pool", JObject.Parse(numerator_info.ToString()));
    response_body.Add("addPoolInfo", addPoolInfo);
    response_body.Add("statusCode", (int)statusCode);
    response_body.Add("statusMessage", statusMessage);

    return req.CreateResponse(statusCode, response_body);
}

private async static Task<Document> addPool(string endpoint, string authorizationKey, string databaseId, string collectionId, string numeratorid, string numeratorname, string numeratorupdateinfo,string prefix, int? from, int? to, int? digits, string suffix, string who, string when, string comment, dynamic actions, dynamic info, TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        _client = new DocumentClient(new Uri(endpoint), authorizationKey);
        var dbSetup = new DatabaseSetup(_client, log);
        await dbSetup.Init(databaseId, collectionId);

        var document = await dbSetup.AddPool(numeratorid, numeratorname, numeratorupdateinfo, prefix, from, to, digits, suffix, who, when, comment, actions, info);

        return document;
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

