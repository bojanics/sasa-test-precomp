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
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

private static readonly HttpClient client = new HttpClient();

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, ExecutionContext context)
{
    log.Info("Starting deletion of numerator...");
    string rootloc = context.FunctionDirectory;
    bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
    if (isLocal)
    {
        rootloc = Directory.GetParent(rootloc).FullName;
    }

    JObject response_body = new JObject();

    string numerator_info = null;
    HttpStatusCode statusCode = HttpStatusCode.OK;
    String statusMessage = null;
    // Initialize response info object
    JObject deleteNumeratorInfo = new JObject { };

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
                    firstErrorMsg = "Numerator will not be deleted because the following error occurred when trying to download configuration file: " + ex.Message;
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
                            firstErrorMsg = "Numerator will not be deleted because the following error occurred when trying to download configuration file: " + ex.Message;
                        }
                    }
                }
            }
        }
        deleteNumeratorInfo.Add("lang", lang);

        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, Configuration_transf, false, false, false);

        ParamInfo CosmosDBEndpoint_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBEndpoint", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBEndpoint_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, CosmosDBEndpoint_transf, false, false, false);

        ParamInfo CosmosDBAuthorizationKey_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBAuthorizationKey", false, null, CommonNumeratorUtilities.DEFAULT_CosmosDBAuthorizationKey_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, CosmosDBAuthorizationKey_transf, true, false, false);

        ParamInfo CosmosDBDatabaseId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBDatabaseId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBDatabaseId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, CosmosDBDatabaseId_transf, false, false, false);

        ParamInfo CosmosDBCollectionId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "CosmosDBCollectionId", true, null, CommonNumeratorUtilities.DEFAULT_CosmosDBCollectionId_CODE, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, CosmosDBCollectionId_transf, false, false, false);

        ParamInfo numeratorId_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorId", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, numeratorId_transf, false, false, false);

        ParamInfo numeratorName_transf = CommonNumeratorUtilities.handleParameter(json, config_json, "numeratorName", true, null, null, null, log);
        CommonNumeratorUtilities.AddResponseParam(deleteNumeratorInfo, numeratorName_transf, false, false, false);


        if (firstErrorMsg != null)
        {
            throw new Exception(firstErrorMsg);
        }

        if (CosmosDBEndpoint_transf.value == null)
        {
            throw new Exception("Can't delete a pool to numerator without specifying the 'CosmosDBEndpoint' parameter!");
        }
        if (CosmosDBAuthorizationKey_transf.value == null)
        {
            throw new Exception("Can't delete a pool to numerator without specifying the 'CosmosDBAuthorizationKey' parameter!");
        }
        if (CosmosDBDatabaseId_transf.value == null)
        {
            throw new Exception("Can't delete a pool to numerator without specifying the 'CosmosDBDatabaseId' parameter!");
        }
        if (CosmosDBCollectionId_transf.value == null)
        {
            throw new Exception("Can't delete a pool to numerator without specifying the 'CosmosDBCollectionId' parameter!");
        }
        if (numeratorId_transf.value == null && numeratorName_transf.value == null)
        {
            throw new Exception("Can't delete a pool to numerator without specifying either 'numeratorId' or 'numeratorName' parameter (in the request, JSON config or as default app setting)!");
        }

        // determine if Id or Name should be used...if it is a name(when Id is null), then search if there is appsetting NUMERATOR_%namevalue% ... it is the Id value
        string id = numeratorId_transf.value;
        if (id == null)
        {
            // now search if there is appsetting NUMEREATOR_ % namevalue % ...it is the Id value
            string vv = System.Environment.GetEnvironmentVariable(CommonNumeratorUtilities.NUMERATOR_PREFIX + numeratorName_transf.value);
            if (vv != null)
            {
                id = vv;
            }
        }

        numerator_info = await deleteNumerator(CosmosDBEndpoint_transf.value, CosmosDBAuthorizationKey_transf.value, CosmosDBDatabaseId_transf.value, CosmosDBCollectionId_transf.value, id, numeratorName_transf.value, log);

        statusCode = HttpStatusCode.OK;
        statusMessage = "Numerator(s) successfully deleted.";
    }
    catch (Exception e)
    {
        statusCode = HttpStatusCode.InternalServerError;
        statusMessage = "Failed to delete numerator! Error message: " + CommonNumeratorUtilities.getInnerExceptionMessage(e, null) + ", stackTrace=" + e.StackTrace + ".";
    }
    log.Info(statusMessage);
    //log.Info("addPoolInfo="+addPoolInfo.ToString());

    response_body.Add("docids", numerator_info);
    response_body.Add("deleteNumeratorInfo", deleteNumeratorInfo);
    response_body.Add("statusCode", (int)statusCode);
    response_body.Add("statusMessage", statusMessage);

    return req.CreateResponse(statusCode, response_body);
}

private async static Task<string> deleteNumerator(string endpoint, string authorizationKey, string databaseId, string collectionId, string numeratorid, string numeratorname, TraceWriter log)
{
    DocumentClient _client = null;
    try
    {
        _client = new DocumentClient(new Uri(endpoint), authorizationKey);
        var dbSetup = new DatabaseSetup(_client, log);
        await dbSetup.Init(databaseId, collectionId);

        List<string> docs = new List<string>();
        if (numeratorid != null && !numeratorid.Trim().Equals(""))
        {
            try
            {
                ResourceResponse<Document> response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, numeratorid));
                dynamic doc = response.Resource;
                docs.Add(doc._self);
            }
            catch (Exception ex)
            {
            }
        }
        else
        {
            IQueryable<dynamic> inum = _client.CreateDocumentQuery<dynamic>(dbSetup.Collection.SelfLink, "SELECT * from c WHERE c.name='" + numeratorname + "' AND c.label='numerator' AND c.type='pool'");
            foreach (dynamic doc in inum)
            {
                docs.Add(doc._self);
            }

        }

        string ret = "";
        foreach (string did in docs)
        {
            await _client.DeleteDocumentAsync(did);
            ret = ret + did + ";";
        }

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

