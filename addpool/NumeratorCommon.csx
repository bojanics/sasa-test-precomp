#r "Newtonsoft.Json"
#r "System.Configuration"

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class CommonNumeratorUtilities
{
    // Some constants for code defaults
    public const string DEFAULT_CosmosDBEndpoint_CODE = "https://sasa-test.documents.azure.com:443";
    public const string DEFAULT_CosmosDBAuthorizationKey_CODE = "XRysJNSllsVfZV5LAdh8YIrzOIGigtL6J5Y02syXquUh1SE7bFP9vdkJLFBsrGqRyd4wYsQgoP6SN5yxgDcjOQ==";
    public const string DEFAULT_CosmosDBDatabaseId_CODE = "test-numerator";
    public const string DEFAULT_CosmosDBCollectionId_CODE = "numberpools";

    public static string getInnerExceptionMessage(Exception ex, string oldMessage)
    {
        if (ex.InnerException != null)
        {
            return getInnerExceptionMessage(ex.InnerException, ex.Message);
        }

        return ex.Message != null ? ex.Message + (oldMessage != null ? ";" + oldMessage : "") : (oldMessage != null ? oldMessage : "");
    }

    public static void AddResponseParam(JObject fileinfo, ParamInfo param, bool isPassword, bool isBoolean, bool isInteger)
    {
        // Add "normal" parameter from the request
        string val = (isPassword && param.orig_value != null ? "*****" : param.orig_value);
        if (isBoolean)
        {
            fileinfo.Add(param.name, Boolean.Parse(val));
        } else if (isInteger)
        {
            if (param.orig_value == null)
            {
                fileinfo.Add(param.name, null);
            }
            else
            {
                try
                {
                    fileinfo.Add(param.name, Int32.Parse(param.orig_value));
                }
                catch (Exception ex)
                {
                    throw new Exception("Value for parameter " + param.name + " is not a valid integer!", ex);
                }
            }
        }
        else
        {
            fileinfo.Add(param.name, val);
        }
    }

    public static ParamInfo handleParameter(JObject json, JObject config_json, string name, Boolean isSetting, string rootloc, string defaultVal, string lang, TraceWriter log)
    {
        ParamInfo ret = new ParamInfo();
        bool paramResolved = false;
        ret.isSetting = isSetting;
        ret.name = name;
        ret.source = 0;
        // handle "normal" and "setting" param from the request 
        if (json != null)
        {
            JProperty p = json.Property(name);
            if (p != null)
            {
                string v = null;
                if (p.Value.Type == JTokenType.Date)
                {
                    v = JsonConvert.SerializeObject(p.Value);
                    v = v.Substring(1, v.Length - 2);
                }
                else
                {
                    v = p.Value.Value<string>();
                }
                if (v != null)
                {
                    ret.value = v;
                    ret.orig_value = v;
                    ret.source = 1;
                    paramResolved = true;
                }
            }

            if (!paramResolved)
            {
                string psnorcsn = name + (isSetting ? "_SettingName" : "_ConnectionStringName");
                p = json.Property(psnorcsn);
                if (p != null)
                {
                    string v = p.Value.Value<string>();
                    if (v != null)
                    {
                        string vv = null;
                        if (isSetting)
                        {
                            vv = System.Environment.GetEnvironmentVariable(v);
                        }
                        else
                        {
                            var cs = ConfigurationManager.ConnectionStrings[v];
                            if (cs != null)
                            {
                                vv = cs.ConnectionString;
                            }
                        }
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
        }

        // handle "normal" and "setting" param from the configuration
        if (!paramResolved && config_json != null)
        {
            JProperty p = config_json.Property(name);
            if (p != null)
            {
                string v = p.Value.Value<string>();
                if (v != null)
                {
                    ret.value = v;
                    ret.orig_value = v;
                    ret.source = 3;
                    paramResolved = true;
                }
            }

            if (!paramResolved)
            {
                string psnorcsn = name + (isSetting ? "_SettingName" : "_ConnectionStringName");
                p = config_json.Property(psnorcsn);
                if (p != null)
                {
                    string v = p.Value.Value<string>();
                    if (v != null)
                    {
                        string vv = null;
                        if (isSetting)
                        {
                            vv = System.Environment.GetEnvironmentVariable(v);
                        }
                        else
                        {
                            var cs = ConfigurationManager.ConnectionStrings[v];
                            if (cs != null)
                            {
                                vv = cs.ConnectionString;
                            }
                        }
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
        }

        // handle DEFAULT setting
        if (!paramResolved)
        {
            string config_DEFAULT_SettingOrConnectionStringName = "NUMERATOR_DEFAULT_" + name;
            string v = null;
            if (isSetting)
            {
                v = System.Environment.GetEnvironmentVariable(config_DEFAULT_SettingOrConnectionStringName);
            }
            else
            {
                var cs = ConfigurationManager.ConnectionStrings[config_DEFAULT_SettingOrConnectionStringName];
                if (cs != null)
                {
                    v = cs.ConnectionString;
                }
            }
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
            }
        }

        // handle language (modify the result if the language parameter exists and the value for the parameter is not from config JSON)
        if (lang != null && ret.value != null && ret.source != 3 && ret.source != 4)
        {
            string val = ret.value;
            val = val.Replace("\\", "/");
            int index_dot = val.LastIndexOf(".");
            int index_slash = val.LastIndexOf("/");

            if (index_dot > 0 && index_dot > index_slash)
            {
                // if there is a dot and it is after slash or backslash
                string prefix = ret.value.Substring(0, index_dot);
                string suffix = ret.value.Substring(index_dot);
                ret.value = $"{prefix}_{lang}{suffix}";
            }
            else
            {
                // if there is no dot
                ret.value = ret.value + $"_{lang}";
            }
            ret.orig_value = ret.value;
        }

        // Resolve relative paths if neccessary
        if (ret.value != null)
        {
            if (rootloc != null)
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(ret.value);
                }
                catch (UriFormatException ex)
                {
                    try
                    {
                        uri = new Uri(rootloc + "/" + ret.value);
                        ret.value = rootloc + "/" + ret.value;
                    }
                    catch (Exception ex2)
                    {
                    }
                }
            }
        }

        //log.Info("["+ (ret.orig_value != null ? ret.orig_value : "null")+","+(ret.value!=null ? ret.value : "null") +","+ret.param_provided+"] returned from source '"+ret.source+"' for call determineParameter(" + param + "," + param_SettingOrConnectionStringName + "," + settingOrConnectionStringName + "," + config_DEFAULT_SettingOrConnectionStringName + "," + isSetting + "," + rootloc + "," + defaultVal + ")");
        return ret;
    }

}


public class ParamInfo
{
    public string name { get; set; }
    public string value { get; set; }
    public string orig_value { get; set; }
    public bool isSetting { get; set; }
    public int source { get; set; }
}

public class DatabaseSetup
{
    public DocumentClient Client { get; }
    public DocumentCollection Collection { get; private set; }

    private TraceWriter log;

    private string databaseId;

    private string collectionId;

    public DatabaseSetup(DocumentClient client,TraceWriter log)
    {
        Client = client;
        this.log = log;
    }

    private async Task<Database> GetOrCreateDatabaseAsync(string databaseId)
    {
        try
        {
            var database = Client.CreateDatabaseQuery()
                               .Where(db => db.Id == databaseId)
                               .ToArray()
                               .FirstOrDefault() ?? await Client.CreateDatabaseAsync(new Database { Id = databaseId });

            return database;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get or create database with Id=" + databaseId, ex);
        }
    }

    private async Task<DocumentCollection> GetOrCreateCollectionAsync(string databaseId, string collectionId)
    {
        try
        {
            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);

        var collection = Client.CreateDocumentCollectionQuery(databaseUri)
                             .Where(c => c.Id == collectionId)
                             .AsEnumerable()
                             .FirstOrDefault() ??
                         await Client.CreateDocumentCollectionAsync(databaseUri, new DocumentCollection { Id = collectionId });

        return collection;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get or create collection with Id=" + collectionId+" for databaseId="+databaseId, ex);
        }
    }

    public async Task Init(string databaseId, string collectionId)
    {
        this.databaseId = databaseId;
        this.collectionId = collectionId;
        await GetOrCreateDatabaseAsync(databaseId);
        Collection = await GetOrCreateCollectionAsync(databaseId, collectionId);
    }

    public async Task<Document> CreateNumerator(string id, string name, dynamic info)
    {
        try
        {
            dynamic nullvalue = null;
            var doc = await Client.CreateDocumentAsync(Collection.SelfLink, new { id = id, name = name, created = info, label = "numerator", type = "pool", updated = nullvalue, pools = new string[] { }, history = new string[] { } });
            return doc;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create numerator document [Id=" + id + ", name=" + name + ", created=" + info+"]", ex);
        }
    }

    private async Task<Document> GetNumerator(string numeratorid, string numeratorname, dynamic numeratorupdateinfo)
    {
        dynamic doc = null;
        if (numeratorid != null && !numeratorid.Trim().Equals(""))
        {
            ResourceResponse<Document> response = await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, numeratorid));
            doc = response.Resource;
        }
        else
        {
            log.Info("Query: SELECT * from c WHERE c.name='" + numeratorname + "' AND c.label='numerator' AND c.type='pool'");
            doc = Client.CreateDocumentQuery<dynamic>(Collection.SelfLink, "SELECT * from c WHERE c.name='" + numeratorname + "' AND c.label='numerator' AND c.type='pool'").AsEnumerable().FirstOrDefault();
        }
        if (doc == null)
        {
            doc = await CreateNumerator(numeratorid, numeratorname, numeratorupdateinfo);
        }
        return doc;
    }

    public async Task<Document> AddPool(string numeratorid, string numeratorname, dynamic numeratorupdateinfo, string prefix, int? from, int? to, int? digits, string suffix, string who, string when, string comment, dynamic actions, dynamic info)
    {
        bool beforeConcurrentReq = false;
        bool afterConcurrentReq = false;
        try
        {
            dynamic doc = await GetNumerator(numeratorid, numeratorname, numeratorupdateinfo);
            int cnt = 0;
            try
            {
                cnt = doc.pools.Count;
            }
            catch (Exception ex) {
            }
            dynamic eop = new ExpandoObject();
            eop.pools = new dynamic[cnt + 1];
            for (int i = 0; i < cnt; i++)
            {
                eop.pools[i] = doc.pools[i];
            }
            dynamic nullvalue = null;
            eop.pools[cnt] = new { prefix = prefix, from = from, to = to, next = from, digits=digits,suffix=suffix,who=who,when=when,comment=comment,actions=actions,created=info,updated = nullvalue};
            doc.pools = eop.pools;

            doc.updated = numeratorupdateinfo;

            var ac = new AccessCondition { Condition = doc.ETag, Type = AccessConditionType.IfMatch };
            beforeConcurrentReq = true;
            doc = await Client.ReplaceDocumentAsync(doc._self, doc, new RequestOptions { AccessCondition = ac });
            afterConcurrentReq = true;

            return doc;
        }
        catch (Exception ex)
        {
            string addMsg = "";
            if (beforeConcurrentReq && !afterConcurrentReq)
            {
                addMsg = ". Problem with the concurrent requests!";
            }
            throw new Exception("Failed to create pool for numerator[Id=" + numeratorid + ", name=" + numeratorname + ", label=numerator, type=pool]"+addMsg, ex);
        }
    }

    public async Task<string> GetNext(string numeratorid, string numeratorname, dynamic numeratorupdateinfo, string fncname)
    {
        long t3 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bool beforeConcurrentReq = false;
        bool afterConcurrentReq = false;
        try
        {
            long t1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            log.Info("Getting numerator doc for thread " + System.Threading.Thread.CurrentThread.Name);
            dynamic doc = await GetNumerator(numeratorid, numeratorname, numeratorupdateinfo);
            long t2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
//            log.Info((t2-t1)+": Numerator retrieved for thread " + System.Threading.Thread.CurrentThread.Name);
            int cnt = 0;
            try
            {
                cnt = doc.pools.Count;
            }
            catch (Exception ex)
            {
            }
            if (cnt == 0)
            {
                doc = await AddPool(numeratorid, numeratorname, numeratorupdateinfo, null, 1, null, null, null, null, null, "Numerator document was not found and was created by function '"+fncname+"'", null, null);
                cnt = 1;
            }
            doc.updated = numeratorupdateinfo;

            dynamic mypool = null;
            List<int> useduppools = new List<int>();
            int pi = 0;
            // the next number
            int nn = -1;
            foreach (dynamic p in doc.pools)
            {
                if (p.to==null || p.next<=p.to)
                {
                    nn = p.next;
                    // update next number in the pool
                    p.next = p.next + 1;
                    mypool = p;
                    if (p.to!=null && p.next>p.to)
                    {
                        useduppools.Add(pi);
                    }
                    break;
                } else
                {
                    useduppools.Add(pi);
                }
                pi++;                    
            }
            if (mypool==null)
            {
                throw new Exception("Failed to get next number, all the pools are used up for the numerator[Id=" + numeratorid + ",Name=" + numeratorname + ", label=numerator, type=pool]");
            }

            // Now update pools and history arrays in original document 
            dynamic eop = new ExpandoObject();

            dynamic eoh = new ExpandoObject();
            int cnthistory = 0;
            try
            {
                cnthistory = doc.history.Count;
            }
            catch (Exception ex)
            {
            }
            eoh.history = new dynamic[cnthistory + useduppools.Count];
            if (useduppools.Count>0)
            {
                for (int i = 0; i < cnthistory; i++)
                {
                    eoh.history[i] = doc.history[i];
                }
            }
            eop.pools = new dynamic[cnt-useduppools.Count];
            var j = 0;
            var k = cnthistory;
            for (int i = 0; i < cnt; i++)
            {
                if (i==pi)
                {
                    if (!useduppools.Contains(pi))
                    {
                        eop.pools[j] = mypool;
                        j++;
                    } else
                    {
                        eoh.history[k] = mypool;
                        k++;
                    }
                }
                else if (!useduppools.Contains(i))
                {
                    eop.pools[j] = doc.pools[i];
                    j++;
                } else
                {
                    eoh.history[k] = doc.pools[i];
                    k++;
                }
            }
            doc.pools = eop.pools;
            if (useduppools.Count > 0)
            {
                doc.history = eoh.history;
            }

            var ac = new AccessCondition { Condition = doc.ETag, Type = AccessConditionType.IfMatch };
            t3 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
//            log.Info((t3 - t2) + ": GetNext logic for thread " + System.Threading.Thread.CurrentThread.Name);
            beforeConcurrentReq = true;
            await Client.ReplaceDocumentAsync(doc._self, doc, new RequestOptions { AccessCondition = ac }); // update CosmosDB
            afterConcurrentReq = true;
            long t4 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
//            log.Info((t4 - t3) + ": replace docu logic for thread " + System.Threading.Thread.CurrentThread.Name);


            string nextnum = nn.ToString();
            int? digits = mypool.digits;
            if (digits!=null)
            {
                nextnum = nn.ToString("D"+digits); // add leading zeros
            }
            if (mypool.prefix!=null)
            {
                nextnum = mypool.prefix + nextnum; // add prefix
            }
            if (mypool.suffix != null)
            {
                nextnum += mypool.suffix; // add suffix
            }

            return nextnum;
        }
        catch (Exception ex)
        {
            string addMsg = "";
            if (beforeConcurrentReq && !afterConcurrentReq)
            {
                addMsg = ". Problem with the concurrent requests!";
            }
            long t4 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //log.Info((t4 - t3) + ": exception when replace docu logic for thread " + System.Threading.Thread.CurrentThread.Name);
            throw new Exception("Failed to get number for numerator[Id=" + numeratorid + ", name=" + numeratorname + ", label=numerator, type=pool]"+addMsg, ex);
        }
    }
}
