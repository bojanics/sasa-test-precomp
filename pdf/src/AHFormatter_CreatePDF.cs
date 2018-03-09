using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using XfoDotNetCtl;
using System;
using System.Configuration;
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
            // The ahlpath1 is for Azure portal when we deploy pre-compiled function
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

            // Some constants for default settings and code defaults
            const string DEFAULT_xsl_APPSETTING_NAME = "PDFGEN_DEFAULT_xsl";
            const string DEFAULT_xslPre_APPSETTING_NAME = "PDFGEN_DEFAULT_xslPre";
            const string DEFAULT_PDFTemplate_APPSETTING_NAME = "PDFGEN_DEFAULT_PDFTemplate";
            const string DEFAULT_signPDF_APPSETTING_NAME = "PDFGEN_DEFAULT_signPDF";
            const string DEFAULT_signPDFReason_APPSETTING_NAME = "PDFGEN_DEFAULT_signPDFReason";
            const string DEFAULT_signPDFLocation_APPSETTING_NAME = "PDFGEN_DEFAULT_signPDFLocation";
            const string DEFAULT_signPDFContact_APPSETTING_NAME = "PDFGEN_DEFAULT_signPDFContact";
            const string DEFAULT_signPDFHashAlgorithm_APPSETTING_NAME = "PDFGEN_DEFAULT_signPDFHashAlgorithm";
            const string DEFAULT_certificateFile_APPSETTING_NAME = "PDFGEN_DEFAULT_certificateFile";
            const string DEFAULT_certificatePassword_CONNECTIONSTRING_NAME = "PDFGEN_DEFAULT_certificatePassword";
            const string DEFAULT_lockPDF_APPSETTING_NAME = "PDFGEN_DEFAULT_lockPDF";
            const string DEFAULT_lockPDFPassword_CONNECTIONSTRING_NAME = "PDFGEN_DEFAULT_lockPDFPassword";

            const string DEFAULT_xsl_CODE = "/defaults/default.xsl";
            const string DEFAULT_signPDF_CODE = "False";
            const string DEFAULT_signPDFHashAlgorithm_CODE = "SHA-1";
            const string DEFAULT_certificateFile_CODE = "/defaults/default.pfx";
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
                dynamic body = req.Content.ReadAsStringAsync().Result;
                dynamic json = JsonConvert.DeserializeObject(body);

                // converting JSON object into XML
                string data = JsonConvert.SerializeObject(json.data);
                string xsl = null;
                try
                {
                    xsl = json.xsl;
                }
                catch (Exception ex) { }
                string xsl_SettingName = null;
                try
                {
                    xsl_SettingName = json.xsl_SettingName;
                }
                catch (Exception ex) { }
                string xslPre = null;
                try
                {
                    xslPre = json.xslPre;
                }
                catch (Exception ex) { }
                string xslPre_SettingName = null;
                try
                {
                    xslPre_SettingName = json.xslPre_SettingName;
                }
                catch (Exception ex) { }
                string PDFTemplate = null;
                try
                {
                    PDFTemplate = json.PDFTemplate;
                }
                catch (Exception ex) { }
                string PDFTemplate_SettingName = null;
                try
                {
                    PDFTemplate_SettingName = json.PDFTemplate_SettingName;
                }
                catch (Exception ex) { }

                string signPDF = null;
                try
                {
                    bool b = json.signPDF;
                    signPDF = b.ToString();
                }
                catch (Exception ex) { }
                string signPDF_SettingName = null;
                try
                {
                    signPDF_SettingName = json.signPDF_SettingName;
                }
                catch (Exception ex) { }
                string signPDFReason = null;
                try
                {
                    signPDFReason = json.signPDFReason;
                }
                catch (Exception ex) { }
                string signPDFReason_SettingName = null;
                try
                {
                    signPDFReason_SettingName = json.signPDFReason_SettingName;
                }
                catch (Exception ex)
                {
                }
                string signPDFLocation = null;
                try
                {
                    signPDFLocation = json.signPDFLocation;
                }
                catch (Exception ex) { }
                string signPDFLocation_SettingName = null;
                try
                {
                    signPDFLocation_SettingName = json.signPDFLocation_SettingName;
                }
                catch (Exception ex)
                {
                }
                string signPDFContact = null;
                try
                {
                    signPDFContact = json.signPDFContact;
                }
                catch (Exception ex) { }
                string signPDFContact_SettingName = null;
                try
                {
                    signPDFContact_SettingName = json.signPDFContact_SettingName;
                }
                catch (Exception ex)
                {
                }
                string signPDFHashAlgorithm = null;
                try
                {
                    signPDFHashAlgorithm = json.signPDFHashAlgorithm;
                }
                catch (Exception ex) { }
                string signPDFHashAlgorithm_SettingName = null;
                try
                {
                    signPDFHashAlgorithm_SettingName = json.signPDFHashAlgorithm_SettingName;
                }
                catch (Exception ex)
                {
                }
                string certificateFile = null;
                try
                {
                    certificateFile = json.certificateFile;
                }
                catch (Exception ex) { }
                string certificateFile_SettingName = null;
                try
                {
                    certificateFile_SettingName = json.certificateFile_SettingName;
                }
                catch (Exception ex)
                {
                }
                string certificatePassword = null;
                try
                {
                    certificatePassword = json.certificatePassword;
                }
                catch (Exception ex) { }
                string certificatePassword_ConnectionStringName = null;
                try
                {
                    certificatePassword_ConnectionStringName = json.certificatePassword_ConnectionStringName;
                }
                catch (Exception ex)
                {
                }

                string lockPDF = null;
                try
                {
                    bool b = json.lockPDF;
                    lockPDF = b.ToString();
                }
                catch (Exception ex) { }
                string lockPDF_SettingName = null;
                try
                {
                    lockPDF_SettingName = json.lockPDF_SettingName;
                }
                catch (Exception ex) { }
                string lockPDFPassword = null;
                try
                {
                    lockPDFPassword = json.lockPDFPassword;
                }
                catch (Exception ex) { }
                string lockPDFPassword_ConnectionStringName = null;
                try
                {
                    lockPDFPassword_ConnectionStringName = json.lockPDFPassword_ConnectionStringName;
                }
                catch (Exception ex)
                {
                }

                string paramsloginfo = "{\n" +
                    "   xsl: " + xsl + ",\n" +
                    "   xsl_SettingName: " + xsl_SettingName + ",\n" +
                    "   xslPre: " + xslPre + ",\n"+
                    "   xslPre_SettingName: " + xslPre_SettingName + ",\n" +
                    "   PDFTemplate: " + PDFTemplate + ",\n"+
                    "   PDFTemplate_SettingName: " + PDFTemplate_SettingName + ",\n" +
                    "   signPDF: " + signPDF + ",\n"+
                    "   signPDF_SettingName: " + signPDF_SettingName + ",\n" +
                    "   signPDFReason: " + signPDFReason + ",\n" +
                    "   signPDFReason_SettingName: " + signPDFReason_SettingName + ",\n" +
                    "   signPDFLocation: " + signPDFLocation + ",\n" +
                    "   signPDFLocation_SettingName: " + signPDFLocation_SettingName + ",\n" +
                    "   signPDFContact: " + signPDFReason + ",\n" +
                    "   signPDFContact_SettingName: " + signPDFContact_SettingName + ",\n" +
                    "   signPDFHashAlgorithm: " + signPDFHashAlgorithm+ ",\n" +
                    "   signPDFHashAlgorithm_SettingName: " + signPDFHashAlgorithm_SettingName + ",\n" +
                    "   certificateFile: " + certificateFile + ",\n" +
                    "   certificateFile_SettingName: " + certificateFile_SettingName + ",\n" +
                    "   certificatePassword: " + (certificatePassword != null ? "xxxxxx" : certificatePassword) + ",\n" +
                    "   certificatePassword_ConnectionStringName: " + certificatePassword_ConnectionStringName + "\n" +
                    "   lockPDF: " + lockPDF + ",\n" +
                    "   lockPDF_SettingName: " + lockPDF_SettingName + ",\n" +
                    "   lockPDFPassword: " + (lockPDFPassword != null ? "xxxxxx" : lockPDFPassword) + ",\n" +
                    "   lockPDFPassword_ConnectionStringName: " + lockPDFPassword_ConnectionStringName + "\n" +
                "}";
                log.Info("PDF will be created according to the following parameters:" + paramsloginfo);

                
                // byte[] of resulting PDF
                byte[] pdfByteArray = null;

                // Dictionary values of JSON's data
                IDictionary<string, string> data_dic = null;

                string firstErrorMsg = null;
                // Determine value of all the parameters using request parameters, default settings and code default and also update pdfinfo response
                ParamInfo xsl_transf = handleParameter(xsl, xsl_SettingName, "xsl_SettingName", DEFAULT_xsl_APPSETTING_NAME, true, rootloc, DEFAULT_xsl_CODE, log);
                AddResponseParam(pdfInfo, "xsl", xsl_transf,true,true,false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, xsl_transf);
                ParamInfo xslPre_transf = handleParameter(xslPre, xslPre_SettingName, "xslPre_SettingName", DEFAULT_xslPre_APPSETTING_NAME, true, rootloc, null, log);
                AddResponseParam(pdfInfo,"xslPre", xslPre_transf,true,true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, xslPre_transf);
                ParamInfo PDFTemplate_transf = handleParameter(PDFTemplate,PDFTemplate_SettingName,"PDFTemplate_SettingName",DEFAULT_PDFTemplate_APPSETTING_NAME,true,rootloc,null,log);
                AddResponseParam(pdfInfo, "PDFTemplate", PDFTemplate_transf,true,true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, PDFTemplate_transf);
                ParamInfo signPDF_transf = handleParameter(signPDF, signPDF_SettingName, "signPDF_SettingName", DEFAULT_signPDF_APPSETTING_NAME, true, null, DEFAULT_signPDF_CODE, log);
                AddResponseParam(pdfInfo, "signPDF", signPDF_transf,true,true, false, true);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDF_transf);
                bool doSigning = Boolean.Parse(signPDF_transf.value);

                ParamInfo signPDFReason_transf = signPDFReason_transf = handleParameter(signPDFReason, signPDFReason_SettingName, "signPDFReason_SettingName", DEFAULT_signPDFReason_APPSETTING_NAME, true, null, null, log);
                AddResponseParam(pdfInfo, "signPDFReason", signPDFReason_transf,true,true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFReason_transf);
                ParamInfo signPDFLocation_transf = handleParameter(signPDFLocation, signPDFLocation_SettingName, "signPDFLocation_SettingName", DEFAULT_signPDFLocation_APPSETTING_NAME, true, null, null, log);
                AddResponseParam(pdfInfo, "signPDFLocation", signPDFLocation_transf, true, true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFLocation_transf);
                ParamInfo signPDFContact_transf = handleParameter(signPDFContact, signPDFContact_SettingName, "signPDFContact_SettingName", DEFAULT_signPDFContact_APPSETTING_NAME, true, null, null, log);
                AddResponseParam(pdfInfo, "signPDFContact", signPDFContact_transf, true, true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFContact_transf);
                ParamInfo signPDFHashAlgorithm_transf = handleParameter(signPDFHashAlgorithm, signPDFHashAlgorithm_SettingName, "signPDFHashAlgorithm_SettingName", DEFAULT_signPDFHashAlgorithm_APPSETTING_NAME, true, null, DEFAULT_signPDFHashAlgorithm_CODE, log);
                AddResponseParam(pdfInfo, "signPDFHashAlgorithm", signPDFHashAlgorithm_transf, true, true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, signPDFHashAlgorithm_transf);
                ParamInfo certificateFile_transf = handleParameter(certificateFile, certificateFile_SettingName, "certificateFile_SettingName", DEFAULT_certificateFile_APPSETTING_NAME, true, rootloc, DEFAULT_certificateFile_CODE, log);
                AddResponseParam(pdfInfo, "certificateFile", certificateFile_transf, true, true, false, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, certificateFile_transf);
                ParamInfo certificatePassword_transf = handleParameter(certificatePassword, certificatePassword_ConnectionStringName, "certificatePassword_ConnectionStringName", DEFAULT_certificatePassword_CONNECTIONSTRING_NAME, false, null, DEFAULT_certificatePassword_CODE, log);
                AddResponseParam(pdfInfo, "certificatePassword", certificatePassword_transf, true, true, true, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, certificatePassword_transf);

                ParamInfo lockPDF_transf = handleParameter(lockPDF, lockPDF_SettingName, "lockPDF_SettingName", DEFAULT_lockPDF_APPSETTING_NAME, true, null, DEFAULT_lockPDF_CODE, log);
                AddResponseParam(pdfInfo, "lockPDF", lockPDF_transf,true,true, false, true);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, lockPDF_transf);
                bool doLocking = Boolean.Parse(lockPDF_transf.value);
                ParamInfo lockPDFPassword_transf = handleParameter(lockPDFPassword, lockPDFPassword_ConnectionStringName, "lockPDFPassword_ConnectionStringName", DEFAULT_lockPDFPassword_CONNECTIONSTRING_NAME, false, null, DEFAULT_lockPDFPassword_CODE, log);
                AddResponseParam(pdfInfo, "lockPDFPassword", lockPDFPassword_transf,true,true, true, false);
                firstErrorMsg = handleFirstErrorMessage(firstErrorMsg, lockPDFPassword_transf);

                if (firstErrorMsg!=null)
                {
                    throw new Exception(firstErrorMsg);
                }

                // if PDFTemplate is specified, use it
                if (PDFTemplate_transf.value != null && !xsl_transf.param_provided) 
                {
                    AddResponseParam(pdfInfo, "PDFTemplate", PDFTemplate_transf, true, false, false, false);
                    using (WebClient wclient = new WebClient())
                    {
                        pdfByteArray = wclient.DownloadData(PDFTemplate_transf.value);
                    }
                    data_dic = JsonConvert.DeserializeObject<IDictionary<string, string>>(data);
                }
                // otherwise, do XSL transformation (with optional pre-transformation)
                else
                {
                    AddResponseParam(pdfInfo, "xsl", xsl_transf, true, false, false, false);
                    string xml = JsonConvert.DeserializeXmlNode(data, "data").OuterXml;
                    // converting Plain XML into other XML format by using XSL pre transformation if exists
                    if (xslPre_transf.value != null)
                    {
                        AddResponseParam(pdfInfo, "xslPre", xslPre_transf, true, false, false, false);
                        MemoryStream os = doXSLT20(xml, xslPre_transf.value);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    pdfByteArray = doPDFGen(xml, xsl_transf.value, log);
                }

                AddResponseParam(pdfInfo, "signPDF", signPDF_transf, true, false, false, true);
                AddResponseParam(pdfInfo, "lockPDF", lockPDF_transf, true, false, false, true);
                // if PDF should be signed or locked or it is from the template so must be filled-in with the parameters from the request
                if (doSigning || doLocking || PDFTemplate_transf.value != null)
                {
                    MemoryStream ss = new MemoryStream();
                    Stream certificate_transf = null;
                    if (doSigning)
                    {
                        AddResponseParam(pdfInfo, "signPDFReason", signPDFReason_transf, true, false, false, false);
                        AddResponseParam(pdfInfo, "signPDFLocation", signPDFLocation_transf, true, false, false, false);
                        AddResponseParam(pdfInfo, "signPDFContact", signPDFContact_transf, true, false, false, false);
                        AddResponseParam(pdfInfo, "signPDFHashAlgorithm", signPDFHashAlgorithm_transf, true, false, false, false);
                        AddResponseParam(pdfInfo, "certificateFile", certificateFile_transf, true, false, false, false);
                        AddResponseParam(pdfInfo, "certificatePassword", certificatePassword_transf, true, false, true, false);

                        certificate_transf = new FileStream(certificateFile_transf.value, FileMode.Open);
                        
                    }
                    if (doLocking)
                    {
                        AddResponseParam(pdfInfo, "lockPDFPassword", lockPDFPassword_transf, true, false, true, false);
                    }
                    DigiSignPdf(pdfByteArray, ss, data_dic, certificate_transf, certificatePassword_transf.value, signPDFHashAlgorithm_transf.value, signPDFReason_transf.value, signPDFLocation_transf.value, signPDFContact_transf.value, doSigning, doLocking ? lockPDFPassword_transf.value : null, false);
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

        private static string handleFirstErrorMessage (string currentErrorMessage,ParamInfo info)
        {
            if (currentErrorMessage==null)
            {
                return info.errorMsg;
            }
            return currentErrorMessage;
        }

        private static void AddResponseParam (JObject pdfinfo, string name,ParamInfo param, bool do_matching_with_param_provided,bool match_with_param_provided,bool isPassword, bool isBoolean)
        {
            string val = (isPassword ? "*****" : (param.orig_value));
            bool addparam = false;
            if (do_matching_with_param_provided)
            {
                if (param.param_provided == match_with_param_provided)
                {
                    if (param.value != null || param.param_provided)
                    {
                        addparam = true;
                    }
                }
            } else
            {
                if (param.value != null || param.param_provided)
                {
                    addparam = true;
                }
            }
            if (addparam)
            {
                if (isBoolean)
                {
                    pdfinfo.Add(name, Boolean.Parse(val));
                }
                else
                {
                    pdfinfo.Add(name, val);
                }
            }
        }

        static ParamInfo handleParameter(string param, string param_SettingOrConnectionStringName, string settingOrConnectionStringName, string config_DEFAULT_SettingOrConnectionStringName, Boolean isSetting, string rootloc, string defaultVal, TraceWriter log)
        {
            ParamInfo ret = new ParamInfo();
            if (param != null)
            {
                ret.source = "param";
                ret.param_provided = true;
                ret.value = param;
                ret.orig_value = param;
            }
            else if (param_SettingOrConnectionStringName != null)
            {
                ret.source = (isSetting ? "Application setting" : "Connection string") + " param";
                ret.param_provided = true;
                if (isSetting)
                {
                    ret.value = System.Environment.GetEnvironmentVariable(param_SettingOrConnectionStringName);
                } else
                {
                    var cs = ConfigurationManager.ConnectionStrings[param_SettingOrConnectionStringName];
                    if (cs != null)
                    {
                        ret.value = cs.ConnectionString;
                    }
                }
                if (ret.value == null)
                {
                    ret.errorMsg = "PDF will not be generated because " + (isSetting ? "Application setting" : "Connection string") + "'" + param_SettingOrConnectionStringName + "' defined as the value of the parameter '" + settingOrConnectionStringName + "' is not found in the configuration!";                    
                    return ret;
                }
                ret.orig_value = ret.value;
                if (ret.value != null && rootloc != null)
                {
                    ret.value = rootloc + "/" + ret.value;
                }
            }
            else if (config_DEFAULT_SettingOrConnectionStringName!=null) 
            {
                if (isSetting)
                {
                    ret.value = System.Environment.GetEnvironmentVariable(config_DEFAULT_SettingOrConnectionStringName);
                } else
                {
                    var cs = ConfigurationManager.ConnectionStrings[config_DEFAULT_SettingOrConnectionStringName];
                    if (cs != null)
                    {
                        ret.value = cs.ConnectionString;
                    }
                }
                ret.orig_value = ret.value;
                if (ret.value != null && rootloc != null)
                {
                    ret.value = rootloc + "/" + ret.value;
                }
                ret.source = "PDFGEN_DEFAULT "+(isSetting ? "Application setting" : "Connection string");
            }
            if (ret.value==null) { 
                ret.value = defaultVal;
                ret.orig_value = defaultVal;
                if (ret.value != null && rootloc != null)
                {
                    ret.value = rootloc + "/"+ ret.value;
                }
                ret.source = "DEFAULT Value";
            } else if (param!=null && settingOrConnectionStringName.StartsWith("certificateFile_") && rootloc!=null) {
                ret.value = rootloc + "/" + ret.value;
            }
            //log.Info("["+ (ret.orig_value != null ? ret.orig_value : "null")+","+(ret.value!=null ? ret.value : "null") +","+ret.param_provided+"] returned from source '"+ret.source+"' for call determineParameter(" + param + "," + param_SettingOrConnectionStringName + "," + settingOrConnectionStringName + "," + config_DEFAULT_SettingOrConnectionStringName + "," + isSetting + "," + rootloc + "," + defaultVal + ")");
            return ret;
        }

        static byte[] doPDFGen(string xml, string xsl, TraceWriter log)
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

        static MemoryStream doXSLT20(string xml, string xsl)
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


        public static void DigiSignPdf(byte[] source,
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
        public string value { get; set; }
        public string orig_value { get; set; }
        public bool param_provided { get; set; }
        public string source { get; set; }

        public string errorMsg { get; set; }
    }

}
