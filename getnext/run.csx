#r "Newtonsoft.Json"
#r "System.Configuration"
#r "Microsoft.Azure.Documents.Client.dll"

#load "NumeratorCommon.csx"

using System.Linq;
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
    log.Info("Starting to retrieve the next 'number'...");
    string rootloc = context.FunctionDirectory;
    bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
    string fncname = rootloc.Substring(rootloc.Replace("\\", "/").LastIndexOf("/") + 1);
    if (isLocal)
    {
        rootloc = Directory.GetParent(rootloc).FullName;
        fncname = "GetNext";
    }
    

    JObject response_body = new JObject();

    //JObject numerator_info = new JObject { };
    string getNext_info = null;
    HttpStatusCode statusCode = HttpStatusCode.OK;
    String statusMessage = null;
    // Initialize response info object
    JObject createNumeratorInfo = new JObject { };

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
                    firstErrorMsg = "getNext will not be executed because the following error occurred when trying to download configuration file: " + ex.Message;
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
                            firstErrorMsg = "getNext will not be executed because the following error occurred when trying to download configuration file: " + ex.Message;
                        }
                    }
                }
            }
        }
        createNumeratorInfo.Add("lang", lang);

        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, Configuration_transf, false, false, false);

        ParamInfo CosmosDBEndpoint_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBEndpoint", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBEndpoint_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, CosmosDBEndpoint_transf, false, false, false);

        ParamInfo CosmosDBAuthorizationKey_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBAuthorizationKey", false, null, CommonNumeratorUtilities.DEFAULT_CosmosDBAuthorizationKey_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, CosmosDBAuthorizationKey_transf, true, false, false);

        ParamInfo CosmosDBDatabaseId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBDatabaseId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBDatabaseId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, CosmosDBDatabaseId_transf, false, false, false);

        ParamInfo CosmosDBCollectionId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBCollectionId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBCollectionId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, CosmosDBCollectionId_transf, false, false, false);

        ParamInfo numeratorId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorId", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, numeratorId_transf, false, false, false);

        ParamInfo numeratorName_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorName", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, numeratorName_transf, false, false, false);

        ParamInfo numeratorInfo_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorInfo", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(createNumeratorInfo, numeratorInfo_transf, false, false, false);
        
        
        /*
        JToken numeratorInfo_req = null;
        if (json != null && json.numeratorInfo != null)
        {
            numeratorInfo_req = json.numeratorInfo;
        }
        createNumeratorInfo.Add("numeratorInfo", numeratorInfo_req);
        */

        if (firstErrorMsg != null)
        {
            throw new Exception(firstErrorMsg);
        }

        getNext_info = await getNext(CosmosDBEndpoint_transf.value, CosmosDBAuthorizationKey_transf.value, CosmosDBDatabaseId_transf.value, CosmosDBCollectionId_transf.value, numeratorId_transf.value, numeratorName_transf.value, numeratorInfo_transf.value, fncname, log);

        statusCode = HttpStatusCode.OK;
        statusMessage = "Numerator successfully created.";
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
    response_body.Add("getNext", getNext_info);
    response_body.Add("getNextInfo", createNumeratorInfo);
    response_body.Add("statusCode", (int)statusCode);
    response_body.Add("statusMessage", statusMessage);

    return req.CreateResponse(statusCode, response_body);
}

private async static Task<string> getNext(string endpoint, string authorizationKey, string databaseId, string collectionId, string id, string name, dynamic info, string fncname, TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        _client = new DocumentClient(new Uri(endpoint), authorizationKey);
        var dbSetup = new DatabaseSetup(_client, log);
        await dbSetup.Init(databaseId, collectionId);

        string ret = await dbSetup.GetNext(id, name, info, fncname);
        return ret;
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


