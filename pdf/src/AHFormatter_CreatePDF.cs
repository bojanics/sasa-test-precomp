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
                    try
                    {
                        json = JsonConvert.DeserializeObject(body);
                    } catch (Exception ex) 
                    {
                        throw new Exception("Invalid JSON body! "+ex.Message, ex);
                    }
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
                ParamInfo signPDF_transf = handleParameter(json, config_json, "signPDF", true, null, DEFAULT_signPDF_CODE, log);
                AddResponseParam(pdfInfo, signPDF_transf, false, true);
                bool doSigning = Boolean.Parse(signPDF_transf.value);

                ParamInfo signPDFReason_transf = signPDFReason_transf = handleParameter(json, config_json, "signPDFReason", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFReason_transf, false, false);
                ParamInfo signPDFLocation_transf = handleParameter(json, config_json, "signPDFLocation", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFLocation_transf, false, false);
                ParamInfo signPDFContact_transf = handleParameter(json, config_json, "signPDFContact", true, null, null, log);
                AddResponseParam(pdfInfo, signPDFContact_transf, false, false);
                ParamInfo signPDFHashAlgorithm_transf = handleParameter(json, config_json, "signPDFHashAlgorithm", true, null, DEFAULT_signPDFHashAlgorithm_CODE, log);
                AddResponseParam(pdfInfo, signPDFHashAlgorithm_transf, false, false);
                ParamInfo certificateFile_transf = handleParameter(json, config_json, "certificateFile", true, rootloc, DEFAULT_certificateFile_CODE, log);
                AddResponseParam(pdfInfo, certificateFile_transf, false, false);
                ParamInfo certificatePassword_transf = handleParameter(json, config_json, "certificatePassword", false, null, DEFAULT_certificatePassword_CODE, log);
                AddResponseParam(pdfInfo, certificatePassword_transf, true, false);

                ParamInfo lockPDF_transf = handleParameter(json, config_json, "lockPDF", true, null, DEFAULT_lockPDF_CODE, log);
                AddResponseParam(pdfInfo, lockPDF_transf, false, true);
                bool doLocking = Boolean.Parse(lockPDF_transf.value);
                ParamInfo lockPDFPassword_transf = handleParameter(json, config_json, "lockPDFPassword", false, null, DEFAULT_lockPDFPassword_CODE, log);
                AddResponseParam(pdfInfo, lockPDFPassword_transf, true, false);

                if (firstErrorMsg != null)
                {
                    throw new Exception(firstErrorMsg);
                }

                // Now merge data payload of the defaults in configuration and the ones from the request (request overrides configuration)
                
                JObject dataobj_req = new JObject();
                // Take the data from the request (if exists) 
                if (json != null && json.data != null)
                {
                    dataobj_req = json.data;
                }
                JObject dataobj = new JObject();
                // Take default data from configuration (if exists)
                if (config_json != null && config_json.data != null)
                {
                    dataobj = config_json.data;
                }                

                // merge objects - request data override default data
                dataobj.Merge(dataobj_req);
                // Add to pdfInfo
                pdfInfo.Add("data", dataobj);


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
                    //log.Info("XMLBEF="+xml);
                    // converting default XML format into other XML format by using XSL pre transformation if exists
                    if (xslPre_transf.value != null)
                    {
                        MemoryStream os = doXSLT20(xml, xslPre_transf.value);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    //log.Info("XMLAFT=" + xml);
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

        private static void AddResponseParam(JObject pdfinfo, ParamInfo param, bool isPassword, bool isBoolean)
        {
            // Add "normal" parameter from the request
            string val = (isPassword && param.orig_value!=null ? "*****" : param.orig_value);
            if (isBoolean)
            {
                pdfinfo.Add(param.name, Boolean.Parse(val));
            }
            else
            {
                pdfinfo.Add(param.name, val);
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
                    string v = p.Value.Value<string>();
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
                string config_DEFAULT_SettingOrConnectionStringName = "PDFGEN_DEFAULT_" + name;
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
            Uri uri = new Uri(xsl);
            XsltExecutable executable = null;
            try
            {
                executable = compiler.Compile(uri);
            } catch (Exception ex)
            {
                throw new Exception("XSL '"+xsl+"' can't be used - maybe it is not accessible! "+ ex.Message, ex);
            }

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
        public bool isSetting { get; set; }
        public int source { get; set; }
    }

}
