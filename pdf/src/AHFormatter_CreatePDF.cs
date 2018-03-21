using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using XfoDotNetCtl;
using System;
using System.Configuration;
using System.Xml;
using Saxon.Api;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Pkcs;

namespace AHFormatter_CreatePDF
{
    public static class AHFormatter
    {

        [FunctionName("AHFormatter")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("Starting creation of PDF...");
            string rootloc = context.FunctionDirectory;
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                rootloc = Directory.GetParent(rootloc).FullName;
            }


            string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process);
            string ahlpath = rootloc + "/ahflibs";
            // The ahlpath is to put AHFormatter DLLs in the PATH when we deploy pre-compiled function
            if (!path.Contains(ahlpath))
            {
                {
                    if (Directory.Exists(ahlpath))
                    {
                        path += ";" + ahlpath;
                        Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
                        log.Info("location " + ahlpath + " added to PATH");
                    }
                    else
                    {
                        log.Warning("location " + ahlpath + " does not exist!");
                    }
                }
            }

            // Some constants for code defaults
            const string DEFAULT_xsl_CODE = "./defaults/default.xsl";
            const string DEFAULT_signPDF_CODE = "False";
            const string DEFAULT_signPDFHashAlgorithm_CODE = "SHA-1";
            const string DEFAULT_certificateFile_CODE = "./defaults/default.pfx";
            const string DEFAULT_certificatePassword_CODE = "password";
            const string DEFAULT_lockPDF_CODE = "False";
            const string DEFAULT_lockPDFPassword_CODE = "password";

            JObject response_body = new JObject();

            String pdfBase64 = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            String statusMessage = null;
            // Initialize response info object
            JObject pdfInfo = new JObject { };

            try
            {

                // READ ALL THE POSSIBLE PARAMETERS FROM THE REQUEST
                dynamic body = null;
                dynamic json = null;
                body = req.Content.ReadAsStringAsync().Result;
                if (body != null)
                {
                    json = JsonConvert.DeserializeObject(body);
                }
                string Configuration = null;
                try
                {
                    Configuration = json.Configuration;
                }
                catch (Exception ex) { }
                string Configuration_SettingName = null;
                try
                {
                    Configuration_SettingName = json.Configuration_SettingName;
                }
                catch (Exception ex)
                {
                }

                // byte[] of resulting PDF
                byte[] pdfByteArray = null;

                string firstErrorMsg = null;
                // Determine value of all the parameters using request parameters, default settings and code default and also update pdfinfo response
                ParamInfo Configuration_transf = handleParameter(json, null, "Configuration", true, rootloc, null, log);
                AddResponseParam(pdfInfo, Configuration_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, Configuration_transf);
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
                            if (firstErrorMsg == null)
                            {
                                firstErrorMsg = "PDF will not be generated because the following error occurred when trying to download configuration file: " + ex.Message;
                            }
                        }
                    }
                }

                ParamInfo xsl_transf = handleParameter(json,config_json,"xsl", true, rootloc, DEFAULT_xsl_CODE, log);
                AddResponseParam(pdfInfo, xsl_transf, false, false);
                ParamInfo xslPre_transf = handleParameter(json, config_json, "xslPre", true, rootloc, null, log);
                AddResponseParam(pdfInfo, xslPre_transf, false, false);
                ParamInfo PDFTemplate_transf = handleParameter(json, config_json, "PDFTemplate", true, rootloc, null, log);
                AddResponseParam(pdfInfo, PDFTemplate_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, PDFTemplate_transf);
                // handle possible error for unexisting xsl/xslPre settings only if it will be used (the case when PDF is NOT generated from the template)
                if (PDFTemplate_transf.source==0 || PDFTemplate_transf.source>4)
                {
                    firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, xsl_transf);
                    firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, xslPre_transf);
                }
                ParamInfo signPDF_transf = handleParameter(json, config_json, "signPDF", true, null, DEFAULT_signPDF_CODE, log);
                AddResponseParam(pdfInfo, signPDF_transf, false, true);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDF_transf);
                bool doSigning = Boolean.Parse(signPDF_transf.value);

                ParamInfo signPDFReason_transf = signPDFReason_transf = handleParameter(json, config_json, "signPDFReason", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFReason_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFReason_transf);
                ParamInfo signPDFLocation_transf = handleParameter(json, config_json, "signPDFLocation", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFLocation_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFLocation_transf);
                ParamInfo signPDFContact_transf = handleParameter(json, config_json, "signPDFContact", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFContact_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFContact_transf);
                ParamInfo signPDFHashAlgorithm_transf = handleParameter(json, config_json, "signPDFHashAlgorithm", true, null, DEFAULT_signPDFHashAlgorithm_CODE, log);
                AddResponseParam(pdfInfo, signPDFHashAlgorithm_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFHashAlgorithm_transf);
                ParamInfo certificateFile_transf = handleParameter(json, config_json, "certificateFile", true, rootloc, DEFAULT_certificateFile_CODE, log);
                AddResponseParam(pdfInfo, certificateFile_transf, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, certificateFile_transf);
                ParamInfo certificatePassword_transf = handleParameter(json, config_json, "certificatePassword", false, null, DEFAULT_certificatePassword_CODE, log);
                AddResponseParam(pdfInfo, certificatePassword_transf, true, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, certificatePassword_transf);

                ParamInfo lockPDF_transf = handleParameter(json, config_json, "lockPDF", true, null, DEFAULT_lockPDF_CODE, log);
                AddResponseParam(pdfInfo, lockPDF_transf, false, true);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, lockPDF_transf);
                bool doLocking = Boolean.Parse(lockPDF_transf.value);
                ParamInfo lockPDFPassword_transf = handleParameter(json, config_json, "lockPDFPassword", false, null, DEFAULT_lockPDFPassword_CODE, log);
                AddResponseParam(pdfInfo, lockPDFPassword_transf, true, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, lockPDFPassword_transf);
                /*
                string paramsloginfo = "{\n" +
                    "   xsl: " + xsl_transf.param_value + ",\n" +
                    "   xsl_SettingName: " + xsl_transf.settingOrConfig_param_value + ",\n" +
                    "   xslPre: " + xslPre_transf.param_value + ",\n" +
                    "   xslPre_SettingName: " + xslPre_transf.settingOrConfig_param_value+ ",\n" +
                    "   PDFTemplate: " + PDFTemplate_transf.param_value + ",\n" +
                    "   PDFTemplate_SettingName: " + PDFTemplate_transf.settingOrConfig_param_value + ",\n" +
                    "   signPDF: " + signPDF_transf.param_value + ",\n" +
                    "   signPDF_SettingName: " + signPDF_transf.settingOrConfig_param_value + ",\n" +
                    "   signPDFReason: " + signPDFReason_transf.param_value + ",\n" +
                    "   signPDFReason_SettingName: " + signPDFReason_transf.settingOrConfig_param_value + ",\n" +
                    "   signPDFLocation: " + signPDFLocation_transf.param_value + ",\n" +
                    "   signPDFLocation_SettingName: " + signPDFLocation_transf.settingOrConfig_param_value + ",\n" +
                    "   signPDFContact: " + signPDFReason_transf.param_value + ",\n" +
                    "   signPDFContact_SettingName: " + signPDFContact_transf.settingOrConfig_param_value + ",\n" +
                    "   signPDFHashAlgorithm: " + signPDFHashAlgorithm_transf.param_value + ",\n" +
                    "   signPDFHashAlgorithm_SettingName: " + signPDFHashAlgorithm_transf.settingOrConfig_param_value + ",\n" +
                    "   certificateFile: " + certificateFile_transf.param_value + ",\n" +
                    "   certificateFile_SettingName: " + certificateFile_transf.settingOrConfig_param_value + ",\n" +
                    "   certificatePassword: " + (certificatePassword_transf.param_value != null ? "xxxxxx" : certificatePassword_transf.param_value) + ",\n" +
                    "   certificatePassword_ConnectionStringName: " + certificatePassword_transf.settingOrConfig_param_value + "\n" +
                    "   lockPDF: " + lockPDF_transf.param_value + ",\n" +
                    "   lockPDF_SettingName: " + lockPDF_transf.settingOrConfig_param_value + ",\n" +
                    "   lockPDFPassword: " + (lockPDFPassword_transf.param_value != null ? "xxxxxx" : lockPDFPassword_transf.param_value) + ",\n" +
                    "   lockPDFPassword_ConnectionStringName: " + lockPDFPassword_transf.settingOrConfig_param_value + "\n" +
                    "   Configuration: " + Configuration_transf.param_value + ",\n" +
                    "   Configuration_SettingName: " + Configuration_transf.settingOrConfig_param_value + ",\n" +
                "}";
                log.Info("PDF will be created according to the following parameters:" + paramsloginfo);
                */

                if (firstErrorMsg != null)
                {
                    throw new Exception(firstErrorMsg);
                }

                // Now merge data payload of the defaults in configuration and the ones from the request (request overrides configuration)
                JObject dataobj = new JObject();
                // First take default data from configuration (if exists)
                if (config_json != null)
                {
                    JProperty p = config_json.Property("data");
                    if (p != null)
                    {
                        dataobj = p.Value.Value<JObject>();
                    }
                }

                // Now take the data from the request (if exists) and merge it - request data override default data
                if (json != null && json.data != null)
                {
                    JObject dataobj_req = json.data;
                    // merge objects
                    dataobj.Merge(dataobj_req);
                }

                //data = JsonConvert.SerializeObject(dataobj);

                // Dictionary values of JSON's data
                IDictionary<string, string> data_dic = null;
                bool doPDFTempl = false;
                // if PDFTemplate is specified, use it
                if (PDFTemplate_transf.value != null && (PDFTemplate_transf.source > 0 && PDFTemplate_transf.source<5 || xsl_transf.source==0 || xsl_transf.source>4))
                {
                    data_dic = getRootProperties(dataobj);
                    using (WebClient wclient = new WebClient())
                    {
                        pdfByteArray = wclient.DownloadData(PDFTemplate_transf.value);
                    }
                    doPDFTempl = true;
                }
                // otherwise, do XSL transformation (with optional pre-transformation)
                else
                {
                    string xml = convertJSON2XML(dataobj);
                    //log.Info(xml);
                    // converting default XML format into other XML format by using XSL pre transformation if exists
                    if (xslPre_transf.value != null)
                    {
                        MemoryStream os = doXSLT20(xml, xslPre_transf.value);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    pdfByteArray = doPDFGen(xml, xsl_transf.value, log);
                }

                // if PDF should be signed or locked or it is from the template so must be filled-in with the parameters from the request
                if (doSigning || doLocking || PDFTemplate_transf.value != null)
                {
                    MemoryStream ss = new MemoryStream();
                    Stream certificate_transf = null;
                    if (doSigning)
                    {
                        using (WebClient wclient = new WebClient())
                        {
                            certificate_transf = new MemoryStream(wclient.DownloadData(certificateFile_transf.value));
                        }

                    }
                    DigiSignPdf(pdfByteArray, ss, doPDFTempl ? data_dic : null, certificate_transf, certificatePassword_transf.value, signPDFHashAlgorithm_transf.value, signPDFReason_transf.value, signPDFLocation_transf.value, signPDFContact_transf.value, doSigning, doLocking ? lockPDFPassword_transf.value : null, false);
                    pdfByteArray = ss.ToArray();
                }

                pdfBase64 = Convert.ToBase64String(pdfByteArray);
                statusCode = HttpStatusCode.OK;
                statusMessage = "PDF successfully created.";
                log.Info(statusMessage);
            }
            catch (Exception e)
            {
                pdfBase64 = null;
                statusCode = HttpStatusCode.InternalServerError;
                statusMessage = "Failed to create PDF! Error message: " + e.Message + ", stackTrace=" + e.StackTrace;
                log.Info(statusMessage);
            }
            response_body.Add("PDF", pdfBase64);
            response_body.Add("PDFInfo", pdfInfo);
            response_body.Add("statusCode", (int)statusCode);
            response_body.Add("statusMessage", statusMessage);

            return req.CreateResponse(statusCode, response_body);
        }

        private static IDictionary<string,string> getRootProperties(JObject data)
        {
            IDictionary<string, string> rootprops = new Dictionary<string, string>();
            foreach (var x in data)
            {
                string name = x.Key;
                JToken value = x.Value;
                if (value is JValue)
                {
                    rootprops.Add(new KeyValuePair<string, string>(name, value.ToString()));
                }
            }
            return rootprops;
        }

        private static void convertJSON2XML(string joname, string nameattr, JContainer json, XmlNode parent)
        {
            XmlDocument doc = parent.OwnerDocument != null ? parent.OwnerDocument : (XmlDocument)parent;
            XmlElement el = doc.CreateElement(string.Empty, joname, string.Empty);
            if (nameattr != null)
            {
                el.SetAttribute("name", nameattr);
            }
            parent.AppendChild(el);
            if (json is JObject)
            {
                foreach (var x in (JObject)json)
                {
                    string name = x.Key;
                    JToken value = x.Value;
                    if (value is JArray || value is JObject)
                    {
                        if (value is JObject)
                        {
                            convertJSON2XML("complexobject", name, (JObject)value, el);
                        }
                        else
                        {
                            convertJSON2XML("array", name, (JArray)value, el);
                        }
                    }
                    else
                    {
                        XmlElement propertyElement = doc.CreateElement(string.Empty, "property", string.Empty);
                        propertyElement.SetAttribute("name", name);
                        propertyElement.SetAttribute("value", value.ToString());
                        el.AppendChild(propertyElement);
                    }
                }
            }
            else if (json is JArray)
            {
                foreach (object item in json)
                {
                    if (item is JObject)
                    {
                        convertJSON2XML("complexobject", nameattr, (JObject)item, el);
                    }
                    else if (item is JArray)
                    {
                        convertJSON2XML("array", nameattr, (JArray)item, el);
                    }
                    else if (item is JValue)
                    {
                        XmlElement memberElement = doc.CreateElement(string.Empty, "member", string.Empty);
                        memberElement.SetAttribute("value", item.ToString());
                        el.AppendChild(memberElement);
                    }
                }
            }
        }

        private static string convertJSON2XML(JObject dataobj)
        {
            XmlDocument doc = new XmlDocument();
            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            convertJSON2XML("data", null, dataobj, doc);
            return doc.OuterXml;
        }

        private static string handleFirstErrorMessage(string currentErrorMessage, ParamInfo info)
        {
            if (currentErrorMessage == null)
            {
                return info.errorMsg;
            }
            return currentErrorMessage;
        }

        private static void AddResponseParam(JObject pdfinfo, ParamInfo param, bool isPassword, bool isBoolean)
        {
            // Add "normal" parameter from the request
            if (param.param_provided)
            {
                _AddResponseParam(pdfinfo, param.name, param.param_value, isPassword, isBoolean);
            }
            // Add "setting" parameter from the request
            if (param.settingOrConfigParam_provided)
            {
                _AddResponseParam(pdfinfo, param.name + (param.isSetting ? "_SettingName" : "_ConnectionStringName"), param.settingOrConfig_param_value, false, false);
                _AddResponseParam(pdfinfo, param.name + (param.isSetting ? "_SettingValue" : "_ConnectionStringValue"), param.settingOrConfig_value, isPassword, false);
            }
            // Add "configuration normal" parameter
            if (param.config_param_provided)
            {
                _AddResponseParam(pdfinfo, "Configuration_"+param.name, param.config_param_value, isPassword, isBoolean);
            }
            // Add "configuration setting" parameter
            if (param.config_settingOrConfigParam_provided)
            {
                _AddResponseParam(pdfinfo, "Configuration_" + param.name + (param.isSetting ? "_SettingName" : "_ConnectionStringName"), param.config_settingOrConfig_param_value, false, false);
                _AddResponseParam(pdfinfo, "Configuration_" + param.name + (param.isSetting ? "_SettingValue" : "_ConnectionStringValue"), param.config_settingOrConfig_value, isPassword, false);
            }
            // Add "default setting" value
            _AddResponseParam(pdfinfo, "PDFGEN_DEFAULT_"+param.name, param.DEFAULT_settingOrConfig_value, isPassword, false);
            // Add "default code" value
            if (param.DEFAULT_code_value != null)
            {
                _AddResponseParam(pdfinfo, "PDFGEN_CODE_DEFAULT_" + param.name, param.DEFAULT_code_value, isPassword, isBoolean);
            }

        }
        private static void _AddResponseParam(JObject pdfinfo, string name, string value, bool isPassword, bool isBoolean)
        {
            string val = (isPassword && value!=null ? "*****" : value);
            if (isBoolean)
            {
                pdfinfo.Add(name, Boolean.Parse(val));
            }
            else
            {
                pdfinfo.Add(name, val);
            }
        }

        private static ParamInfo handleParameter(JObject json, JObject config_json, string name, Boolean isSetting, string rootloc, string defaultVal, TraceWriter log)
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
                    ret.param_provided = true;
                    string v = p.Value.Value<string>();
                    ret.param_value = v;
                    if (v != null)
                    {
                        ret.value = v;
                        ret.orig_value = v;
                        ret.source = 1;
                        paramResolved = true;
                    }
                }

                string psnorcsn = name + (isSetting ? "_SettingName" : "_ConnectionStringName");
                p = json.Property(psnorcsn);
                if (p != null)
                {
                    ret.settingOrConfigParam_provided = true;
                    string v = p.Value.Value<string>();
                    ret.settingOrConfig_param_value = v;
                    if (v != null)
                    {
                        if (isSetting)
                        {
                            ret.settingOrConfig_value = System.Environment.GetEnvironmentVariable(v);
                        }
                        else
                        {
                            var cs = ConfigurationManager.ConnectionStrings[v];
                            if (cs != null)
                            {
                                ret.settingOrConfig_value = cs.ConnectionString;
                            }
                        }
                        if (!paramResolved)
                        {
                            ret.value = ret.settingOrConfig_value;
                            ret.orig_value = ret.settingOrConfig_value;
                            if (ret.value == null)
                            {
                                ret.errorMsg = "PDF will not be generated because " + (isSetting ? "Application setting" : "Connection string") + " '" + v + "' defined as the value of the parameter '" + psnorcsn + "' is not found or has undefined value!";
                            }
                            ret.source = 2;
                            paramResolved = true;
                        }
                    }
                }
            }

            // handle "normal" and "setting" param from the configuration
            if (config_json != null)
            {
                JProperty p = config_json.Property(name);
                if (p != null)
                {
                    ret.config_param_provided = true;
                    string v = p.Value.Value<string>();
                    ret.config_param_value = v;
                    if (v != null)
                    {
                        if (!paramResolved)
                        {
                            ret.value = v;
                            ret.orig_value = v;
                            ret.source = 3;
                            paramResolved = true;
                        }
                    }
                }

                string psnorcsn = name + (isSetting ? "_SettingName" : "_ConnectionStringName");
                p = config_json.Property(psnorcsn);
                if (p != null)
                {
                    ret.config_settingOrConfigParam_provided = true;
                    string v = p.Value.Value<string>();
                    ret.config_settingOrConfig_param_value = v;
                    if (v != null)
                    {
                        if (isSetting)
                        {
                            ret.config_settingOrConfig_value = System.Environment.GetEnvironmentVariable(v);
                        }
                        else
                        {
                            var cs = ConfigurationManager.ConnectionStrings[v];
                            if (cs != null)
                            {
                                ret.config_settingOrConfig_value = cs.ConnectionString;
                            }
                        }
                        if (!paramResolved)
                        {
                            ret.value = ret.config_settingOrConfig_value;
                            ret.orig_value = ret.config_settingOrConfig_value;
                            if (ret.value == null)
                            {
                                ret.errorMsg = "PDF will not be generated because " + (isSetting ? "Application setting" : "Connection string") + " '" + v + "' defined in Configuration file as the value of the parameter '" + psnorcsn + "' is not found or has undefined value!";
                            }
                            ret.source = 4;
                            paramResolved = true;
                        }
                    }
                }
            }

            // handle DEFAULT setting
            string config_DEFAULT_SettingOrConnectionStringName = "PDFGEN_DEFAULT_" + name;
            if (isSetting)
            {
                ret.DEFAULT_settingOrConfig_value = System.Environment.GetEnvironmentVariable(config_DEFAULT_SettingOrConnectionStringName);
            }
            else
            {
                var cs = ConfigurationManager.ConnectionStrings[config_DEFAULT_SettingOrConnectionStringName];
                if (cs != null)
                {
                    ret.DEFAULT_settingOrConfig_value = cs.ConnectionString;
                }
            }
            if (!paramResolved)
            {
                ret.value = ret.DEFAULT_settingOrConfig_value;
                ret.orig_value = ret.DEFAULT_settingOrConfig_value;
                if (ret.value != null)
                {
                    ret.source = 5;
                    paramResolved = true;
                }
            }

            // handle default CODE value
            ret.DEFAULT_code_value = defaultVal;
            if (!paramResolved)
            {
                ret.value = defaultVal;
                ret.orig_value = defaultVal;
                if (ret.value != null)
                {
                    ret.source = 6;
                }
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

        private static byte[] doPDFGen(string xml, string xsl, TraceWriter log)
        {
            MemoryStream outFs = new MemoryStream();

            XfoObj obj = null;
            try
            {
                obj = new XfoObj();
                obj.ErrorStreamType = 2;
                obj.ExitLevel = 4;

                MemoryStream inFo = doXSLT20(xml, xsl);
                byte[] ba = inFo.ToArray();
                string str = Encoding.UTF8.GetString(ba, 0, ba.Length);
                //log.Info("INFO=" + str);
                obj.Render(inFo, outFs);

                return outFs.ToArray();
            }
            catch (XfoException e)
            {
                log.Error("ErrorLevel = " + e.ErrorLevel + "\nErrorCode = " + e.ErrorCode + "\n" + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw e;
            }
            finally
            {
                if (outFs != null)
                    outFs.Close();
                if (obj != null)
                    obj.Dispose();
            }
        }

        private static MemoryStream doXSLT20(string xml, string xsl)
        {
            // Compile stylesheet
            var processor = new Processor();
            var compiler = processor.NewXsltCompiler();
            var executable = compiler.Compile(new Uri(xsl));

            // Load the source document
            byte[] byteArray = Encoding.UTF8.GetBytes(xml);
            MemoryStream xmlstream = new MemoryStream(byteArray);

            // Do transformation to a destination
            var transformer = executable.Load();
            transformer.SetInputStream(xmlstream, new Uri(xsl));

            MemoryStream inFo = new MemoryStream();
            Serializer serializer = new Serializer();
            serializer.SetOutputStream(inFo);
            transformer.Run(serializer);

            return inFo;
        }


        private static void DigiSignPdf(byte[] source,
            Stream destinationStream,
            IDictionary<string, string> data,
            Stream privateKeyStream,
            string keyPassword,
            string digestAlgorithm,
            string reason,
            string location,
            string contact,
            bool signPdf,
            string pdfpassword,
            bool isVisibleSignature)
        {
            // reader and stamper
            PdfReader reader = new PdfReader(source);
            PdfStamper stamper = null;
            if (signPdf)
            {
                stamper = PdfStamper.CreateSignature(reader, destinationStream, '\0');
            }
            else
            {
                stamper = new PdfStamper(reader, destinationStream);
            }
            // password protection
            if (pdfpassword != null)
            {
                byte[] pwd = Encoding.UTF8.GetBytes(pdfpassword);
                stamper.SetEncryption(pwd, pwd, PdfWriter.AllowPrinting, PdfWriter.ENCRYPTION_AES_128);
            }

            // This is used when filling-in the PDF template with the information from the request
            if (data != null)
            {
                AcroFields pdfFormFields = stamper.AcroFields;
                foreach (KeyValuePair<string, string> dic in data)
                {
                    AcroFields.Item afld = pdfFormFields.GetFieldItem(dic.Key);
                    if (afld != null)
                    {
                        pdfFormFields.SetField(dic.Key, dic.Value);
                    }
                    stamper.FormFlattening = true; // Make the PDF read-only
                }
            }

            if (signPdf)
            {
                Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());
                privateKeyStream.Dispose();

                //then Iterate throught certificate entries to find the private key entry
                string alias = null;
                foreach (string tAlias in pk12.Aliases)
                {
                    if (pk12.IsKeyEntry(tAlias))
                    {
                        alias = tAlias;
                        break;
                    }
                }
                var pk = pk12.GetKey(alias).Key;
                IExternalSignature es = new PrivateKeySignature(pk, digestAlgorithm);

                // appearance
                PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                //appearance.Image = new iTextSharp.text.pdf.PdfImage();
                appearance.Reason = reason;
                appearance.Location = location;
                appearance.Contact = contact;
                if (isVisibleSignature)
                {
                    appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(20, 10, 170, 60), reader.NumberOfPages, null);
                }
                // digital signature

                MakeSignature.SignDetached(appearance, es, new Org.BouncyCastle.X509.X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, CryptoStandard.CMS);
            }
            stamper.Close();
            reader.Close();
            reader.Dispose();
        }

    }


    public class ParamInfo
    {
        public string name { get; set; }
        public string value { get; set; }
        public string orig_value { get; set; }
        public bool param_provided { get; set; }
        public string param_value { get; set; }
        public bool settingOrConfigParam_provided { get; set; }
        public string settingOrConfig_param_value { get; set; }
        public string settingOrConfig_value { get; set; }
        public bool config_param_provided { get; set; }
        public string config_param_value { get; set; }
        public bool config_settingOrConfigParam_provided { get; set; }
        public string config_settingOrConfig_param_value { get; set; }
        public string config_settingOrConfig_value { get; set; }
        public string DEFAULT_settingOrConfig_value { get; set; }
        public string DEFAULT_code_value { get; set; }
        public bool isSetting { get; set; }
        public int source { get; set; }
        public string errorMsg { get; set; }
    }

}
