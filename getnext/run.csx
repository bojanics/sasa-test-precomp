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
    
    const string DEFAULT_retriesNumber_CODE = "1";

    JObject response_body = new JObject();

    string getNext_info = null;
    HttpStatusCode statusCode = HttpStatusCode.OK;
    String statusMessage = null;
    // Initialize response info object
    JObject getNextInfo = new JObject { };

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
        getNextInfo.Add("lang", lang);

        CommonNumeratorUtilities.AddResponseParam(getNextInfo, Configuration_transf, false, false, false);

        ParamInfo CosmosDBEndpoint_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBEndpoint", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBEndpoint_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, CosmosDBEndpoint_transf, false, false, false);

        ParamInfo CosmosDBAuthorizationKey_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBAuthorizationKey", false, null, CommonNumeratorUtilities.DEFAULT_CosmosDBAuthorizationKey_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, CosmosDBAuthorizationKey_transf, true, false, false);

        ParamInfo CosmosDBDatabaseId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBDatabaseId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBDatabaseId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, CosmosDBDatabaseId_transf, false, false, false);

        ParamInfo CosmosDBCollectionId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBCollectionId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBCollectionId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, CosmosDBCollectionId_transf, false, false, false);

        ParamInfo numeratorId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorId", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, numeratorId_transf, false, false, false);

        ParamInfo numeratorName_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorName", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, numeratorName_transf, false, false, false);

        JToken numeratorInfo_req = null;
        if (json != null && json.numeratorInfo != null)
        {
            numeratorInfo_req = json.numeratorInfo;
        }
        getNextInfo.Add("numeratorInfo", numeratorInfo_req);

        ParamInfo retriesNumber_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "retriesNumber", true, null, DEFAULT_retriesNumber_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(getNextInfo, retriesNumber_transf, false, false, true);
        int retriesNumber = 1;
        if (retriesNumber_transf.value != null)
        {
            try
            {
                retriesNumber = Int32.Parse(retriesNumber_transf.value);
            }
            catch (Exception ex)
            {
                if (firstErrorMsg == null)
                {
                    firstErrorMsg = "getNext won't be executed because the 'retriesNumber' parameter value is not an integer!";
                }
            }
        }

        if (firstErrorMsg != null)
        {
            throw new Exception(firstErrorMsg);
        }

        if (CosmosDBEndpoint_transf.value == null)
        {
            throw new Exception("Can't execute getNext without specifying the 'CosmosDBEndpoint' parameter!");
        }
        if (CosmosDBAuthorizationKey_transf.value == null)
        {
            throw new Exception("Can't execute getNext without specifying the 'CosmosDBAuthorizationKey' parameter!");
        }
        if (CosmosDBDatabaseId_transf.value == null)
        {
            throw new Exception("Can't execute getNext without specifying the 'CosmosDBDatabaseId' parameter!");
        }
        if (CosmosDBCollectionId_transf.value == null)
        {
            throw new Exception("Can't execute getNext without specifying the 'CosmosDBCollectionId' parameter!");
        }
        if (numeratorId_transf.value==null && numeratorName_transf.value==null)
        {
            throw new Exception("Can't execute getNext without specifying either 'numeratorId' or 'numeratorName' parameter (in the request, JSON config or as default app setting)!");
        }

        // determine if Id or Name should be used...if it is a name, then search if there is appsetting NUMERATOR_%namevalue% ... it is the Id value
        bool useName = false;
        if (numeratorName_transf.value != null)
        {
            if (numeratorId_transf.value == null)
            {
                useName = true;
            } else if (numeratorId_transf.source>numeratorName_transf.source)
            {
                    useName = true;
            }                    
        }
        string id = numeratorId_transf.value;
        if (useName)
        {
            id = null;
            // now search if there is appsetting NUMERATOR_ % namevalue % ...it is the Id value
            string vv = System.Environment.GetEnvironmentVariable(CommonNumeratorUtilities.NUMERATOR_PREFIX + numeratorName_transf.value);
            if (vv != null)
            {
                id = vv;
            }
        }

        getNext_info = await getNext(CosmosDBEndpoint_transf.value, CosmosDBAuthorizationKey_transf.value, CosmosDBDatabaseId_transf.value, CosmosDBCollectionId_transf.value, id, numeratorName_transf.value, numeratorInfo_req, fncname, retriesNumber, req.RequestUri.Scheme, log);

        statusCode = HttpStatusCode.OK;
        statusMessage = "getNext numerator number successfully returned.";
    }
    catch (Exception e)
    {
        statusCode = HttpStatusCode.InternalServerError;
        statusMessage = "Failed to execute getNext! Error message: " + CommonNumeratorUtilities.getInnerExceptionMessage(e, null) + ", stackTrace=" + e.StackTrace + ".";
    }
    log.Info(statusMessage);
    //log.Info("getNextInfo="+getNextInfo.ToString());
    response_body.Add("getNext", getNext_info);
    response_body.Add("getNextInfo", getNextInfo);
    response_body.Add("statusCode", (int)statusCode);
    response_body.Add("statusMessage", statusMessage);

    return req.CreateResponse(statusCode, response_body);
}

private async static Task<string> getNext(string endpoint, string authorizationKey, string databaseId, string collectionId, string id, string name, dynamic info, string fncname, int retriesNumber, string reqschema, TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        _client = new DocumentClient(new Uri(endpoint), authorizationKey);
        var dbSetup = new DatabaseSetup(_client, log);
        await dbSetup.Init(databaseId, collectionId);

        NumeratorInfo ni = await dbSetup.GetNext(id, name, info, fncname, retriesNumber);
        processActions(ni.doc, ni.pool, endpoint, authorizationKey, databaseId, collectionId, id, name, info, fncname, reqschema,log);
        log.Info("Numerator retrieved...");
        return ni.value;
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

private async static Task processActions(dynamic doc, dynamic pool, string endpoint, string authorizationKey, string databaseId, string collectionId, string id, string name, dynamic info, string fncname, string reqschema, TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        log.Info("Started processing actions...");

        if (pool.actions!=null)
        {
            foreach (dynamic act in pool.actions)
            {
                //log.Info("T=" + act.threshold);
                //log.Info("U=" + act.actionCall);
                if (act.threshold != null && act.threshold >= pool.next-pool.from && act.actionHandled!=null && !act.actionHandled)
                {
                    log.Info("ACTION WILL BE EXECUTED!");
                    var content = new StringContent(JsonConvert.SerializeObject(doc), System.Text.Encoding.UTF8, "application/json");
                    string fullurl = reqschema+"://" + System.Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")+act.actionCall;
                    log.Info("FURL=" + fullurl);
                    HttpResponseMessage response = await client.PostAsync(fullurl, content);
                    if (response.StatusCode != HttpStatusCode.OK) 
                    {

                    }
                    log.Info("RESP=" + response);
                }
            }
        }
        log.Info("Ended processing actions...");

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


