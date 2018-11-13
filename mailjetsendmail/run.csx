#r "Newtonsoft.Json"
#r "System.Configuration"
#r "Mailjet.Client.dll"
#r "MimeTypesMap.dll"
#r "saxon9he-api.dll"


using System.Net; 

using System;
using Mailjet.Client;
using Mailjet.Client.Resources;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq; 
using Newtonsoft.Json;
using System.Xml.Xsl; 
using System.Text;
using System.Xml;
using Saxon.Api;
using System.Text.RegularExpressions; 
using System.Configuration;
using HeyRed.Mime; 

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, ExecutionContext context)
{
            RequestBody _req = new RequestBody();
            bool hasError = false;
            string errorMessage = string.Empty;
            const string _contentType = "ContentType";
            const string _fileName = "Filename";
            const string _content = "Base64Content";
            JArray mail_data = new JArray();
            List<string> default_app_setting_error = new List<string>();

            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                _req = JsonConvert.DeserializeObject<RequestBody>(JsonConvert.SerializeObject(PrepareParameter(data, context, req)));

                JObject template_vars = new JObject();

                if (!string.IsNullOrEmpty(TryParseString(_req.data)))
                    template_vars = JObject.Parse(TryParseString(_req.data));

                if(_req.Variables != null && !string.IsNullOrEmpty(_req.Variables.ToString()))
                {
                    try
                    {
                        JObject tmp = JObject.Parse(_req.Variables.ToString());

                        template_vars.Merge(tmp);
                    }
                    catch
                    {
                        throw new Exception($"{_Parameters.Variables} is not JSON format.");
                    }
                    
                }

                string mail_body_text = string.Empty;
                string mail_body_html = string.Empty;

                if (string.IsNullOrEmpty(_req.TemplateID))
                {
                    if (string.IsNullOrEmpty(_req.Body_HTML))
                    {
                        if (!string.IsNullOrEmpty(_req.Template_HTML))
                            mail_body_html = GetTemplate_HTML(_req, context);
                        else if (!string.IsNullOrEmpty(_req.xslHTML))
                            mail_body_html = GetXSL_HTML(context, _req);
                    }
                    else
                        mail_body_html = _req.Body_HTML;

                    if (string.IsNullOrEmpty(mail_body_html))
                    {
                        if (string.IsNullOrEmpty(_req.Body_TextPlain))
                        {
                            if (!string.IsNullOrEmpty(_req.Template))
                                mail_body_text = GetTemplate(_req, context);
                            else
                                mail_body_text = GetXSL(context, _req);
                        }
                        else
                            mail_body_text = _req.Body_TextPlain;
                    }
                }

                if (!string.IsNullOrEmpty(mail_body_html))
                    _req.Body_HTML = mail_body_html;
                else if (!string.IsNullOrEmpty(mail_body_text))
                    _req.Body_TextPlain = mail_body_text;

                #region -- Checking Test_To, Test_Cc and Test_Bcc parameters --
                object _to_ = _req.To;
                object _cc_ = _req.Cc;
                object _bcc_ = _req.Bcc;

                string ENVIRONMENT_DEVOPS = GetParameterInApplicationSetting("ENVIRONMENT_DEVOPS");

                if (!string.IsNullOrEmpty(ENVIRONMENT_DEVOPS) && (_req.Test_To != null && !string.IsNullOrEmpty(_req.Test_To.ToString())))
                {
                    if (!ENVIRONMENT_DEVOPS.Equals("prd"))
                    {
                        _to_ = _req.Test_To;
                    }
                }

                if (!string.IsNullOrEmpty(ENVIRONMENT_DEVOPS) && (_req.Test_Cc != null && !string.IsNullOrEmpty(_req.Test_Cc.ToString())))
                {
                    if (!ENVIRONMENT_DEVOPS.Equals("prd"))
                    {
                        _cc_ = _req.Test_Cc;
                    }
                }

                if (!string.IsNullOrEmpty(ENVIRONMENT_DEVOPS) && (_req.Test_Bcc != null && !string.IsNullOrEmpty(_req.Test_Bcc.ToString())))
                {
                    if (!ENVIRONMENT_DEVOPS.Equals("prd"))
                    {
                        _bcc_ = _req.Test_Bcc;
                    }
                }
                #endregion

                List<JObject> attachmentMails = new List<JObject>();

                #region -- Prepare attachments --

                if (!string.IsNullOrEmpty(TryParseString(_req.Attachments)))
                {

                    string[] attachments = ConvertRequestValue(TryParseString(_req.Attachments)).Split(',');

                    foreach (string at in attachments)
                    {
                        string fileName = string.Empty;
                        string content = string.Empty;
                        string contentType = string.Empty;

                        try
                        {
                            using (WebClient wc = new WebClient())
                            {
                                Uri path = new Uri(at, UriKind.Absolute);
                                fileName = Path.GetFileName(path.AbsolutePath);
                                byte[] file_content = wc.DownloadData(at);
                                content = Convert.ToBase64String(file_content);

                                var _re = HttpWebRequest.Create(at) as HttpWebRequest;
                                if (_re != null)
                                {
                                    var _rep = _re.GetResponse() as HttpWebResponse;

                                    if (_rep != null)
                                        contentType = _rep.ContentType;
                                }
                            }
                        }
                        catch
                        {
                            try
                            {
                                //local file
                                Uri path;

                                //Full local file path
                                if (at.Contains(":"))
                                    path = new Uri(at, UriKind.Absolute);
                                else
                                    path = new Uri(Path.Combine(context.FunctionDirectory, at), UriKind.Absolute);

                                fileName = Path.GetFileName(path.LocalPath);
                                contentType = MimeTypesMap.GetMimeType(fileName);
                                content = Convert.ToBase64String(File.ReadAllBytes(path.AbsolutePath));
                            }
                            catch
                            {
                                throw new Exception("Error in Attachments, file name '" + at + "'");
                            }
                        }

                        attachmentMails.Add(new JObject {{_contentType, contentType},
                                          {_fileName,fileName},
                                          {_content,content}});
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(TryParseString(_req.Attachments_Base64)))
                {
                    List<Base64Attachment> lst = JsonConvert.DeserializeObject<List<Base64Attachment>>(TryParseString(_req.Attachments_Base64));

                    foreach (Base64Attachment at in lst)
                    {
                        try
                        {
                            Convert.FromBase64String(at.Base64_Content);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Error in Attachments_Base64, file name '" + at.FileName + "' : " + e.Message);
                        }
                        if (!string.IsNullOrEmpty(at.FileName) && !string.IsNullOrEmpty(at.Content_Type) && !string.IsNullOrEmpty(at.Base64_Content))
                            attachmentMails.Add(new JObject {{_contentType, at.Content_Type},
                                          {_fileName,at.FileName},
                                          {_content,at.Base64_Content}});
                    }
                }

                JObject _tmp;
                string[] to = TryParseString(_to_).Split(',');
                JArray to_info = new JArray();
                foreach (string _to in to)
                {
                    _tmp = new JObject();
                    _tmp.Add("Email", _to);
                    _tmp.Add("Name", GetNameFromEmail(_to));
                    to_info.Add(_tmp);
                }

                string[] cc = TryParseString(_cc_).Split(',');
                JArray cc_info = new JArray();
                foreach (string _cc in cc)
                {
                    _tmp = new JObject();
                    _tmp.Add("Email", _cc);
                    _tmp.Add("Name", GetNameFromEmail(_cc));
                    cc_info.Add(_tmp);
                }

                string[] bcc = TryParseString(_bcc_).Split(',');
                JArray bcc_info = new JArray();
                foreach (string _bcc in bcc)
                {
                    _tmp = new JObject();
                    _tmp.Add("Email", _bcc);
                    _tmp.Add("Name", GetNameFromEmail(_bcc));
                    bcc_info.Add(_tmp);
                }

                JObject replyto = new JObject();
                string reply_email = string.Empty;
                string reply_name = string.Empty;

                if (!string.IsNullOrEmpty(_req.ReplyTo))
                {
                    reply_email = _req.ReplyTo;
                    reply_name = GetNameFromEmail(_req.ReplyTo);
                }

                JObject mail = new JObject();
                mail.Add("From", new JObject
                {
                    {"Email", TryParseString(_req.SenderMail)},
                    {"Name", TryParseString(_req.SenderName) }
                });
                mail.Add("Subject", TryParseString(_req.Subject));


                if (to.Length > 0 && !string.IsNullOrEmpty(to[0]))
                    mail.Add("To", to_info);

                if (cc.Length > 0 && !string.IsNullOrEmpty(cc[0]))
                    mail.Add("Cc", cc_info);

                if (bcc.Length > 0 && !string.IsNullOrEmpty(bcc[0]))
                    mail.Add("Bcc", bcc_info);

                if (!string.IsNullOrEmpty(_req.ReplyTo))
                {
                    mail.Add("ReplyTo", new JObject
                    {
                        { "Email", reply_email },
                        { "Name", reply_name}
                    });
                }

                int templateID;
                if (int.TryParse(_req.TemplateID, out templateID))
                {
                    mail.Add("TemplateID", templateID);
                    mail.Add("TemplateLanguage", true);
                }
                else
                {
                    if (!string.IsNullOrEmpty(mail_body_html))
                        mail.Add("HTMLPart", mail_body_html);
                    else
                        mail.Add("TextPart", mail_body_text);
                }

                if (attachmentMails.Count > 0)
                    mail.Add("Attachments", new JArray(attachmentMails));

                if (template_vars != null && template_vars.Count > 0)
                {
                    mail.Add("Variables", template_vars);
                }

                if (!string.IsNullOrEmpty(_req.CustomID))
                {
                    mail.Add("CustomID", _req.CustomID);
                }

                if (!string.IsNullOrEmpty(_req.CustomCampaign))
                {
                    mail.Add("CustomCampaign", _req.CustomCampaign);
                }

                if (_req.DeduplicateCampaign != null && !string.IsNullOrEmpty(_req.DeduplicateCampaign.ToString()))
                {
                    bool _val = false;

                    if (bool.TryParse(_req.DeduplicateCampaign.ToString(), out _val))
                    {
                        mail.Add("DeduplicateCampaign", _val);
                    }
                    else
                    {
                        throw new Exception($"{_Parameters.DeduplicateCampaign} is not Boolean.");
                    }
                }

                if (!string.IsNullOrEmpty(_req.TrackOpens))
                {
                    string tmp = _req.TrackOpens.ToLower();

                    switch (tmp)
                    {
                        case "account_default":
                            mail.Add("TrackOpens", "account_default");
                            break;
                        case "disabled":
                            mail.Add("TrackOpens", "disabled");
                            break;
                        case "enabled":
                            mail.Add("TrackOpens", "enabled");
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(_req.TrackClicks))
                {
                    string tmp = _req.TrackOpens.ToLower();

                    switch (tmp)
                    {
                        case "account_default":
                            mail.Add("TrackClicks", "account_default");
                            break;
                        case "disabled":
                            mail.Add("TrackClicks", "disabled");
                            break;
                        case "enabled":
                            mail.Add("TrackClicks", "enabled");
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(_req.EventPayload))
                {
                    mail.Add("EventPayload", _req.EventPayload);
                }

                if (!string.IsNullOrEmpty(_req.MonitoringCategory))
                {
                    mail.Add("MonitoringCategory", _req.MonitoringCategory);
                }

                if (!string.IsNullOrEmpty(_req.URLTags))
                {
                    mail.Add("URLTags", _req.URLTags);
                }

                if(_req.Headers != null && !string.IsNullOrEmpty(_req.Headers.ToString()))
                {
                    try
                    {
                        JObject tmp = JObject.Parse(_req.Headers.ToString());

                        mail.Add("Headers", tmp);
                    }
                    catch
                    {
                        throw new Exception($"{_Parameters.Headers} is not JSON format.");
                    }
                }

                //Mail sending via MailJet API
                MailjetClient client = new MailjetClient(_req.MJAPI_PublicKey, _req.MJAPI_PrivateKey)
                {
                    Version = ApiVersion.V3_1,
                };
                MailjetRequest request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }.Property(Send.Messages, new JArray { mail });

                MailjetResponse response = await client.PostAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    mail_data = response.GetData();
                    hasError = false;
                }
                else
                {
                    mail_data = response.GetData();
                    hasError = true;

                    try
                    {
                        errorMessage = (mail_data[0]["Errors"] as JArray)[0]["ErrorMessage"].ToString();
                    }
                    catch { }

                    return req.CreateResponse((System.Net.HttpStatusCode)response.StatusCode, BuildResponseMessage(HttpStatusCode.InternalServerError, errorMessage, mail_data, _req));
                }
            }
            catch (Exception ex)
            {
                hasError = true;

                if (default_app_setting_error.Count > 0)
                {
                    string msg = string.Empty;
                    for (int i = 0; i < default_app_setting_error.Count; i++)
                    {
                        if (i > 0 && i == default_app_setting_error.Count - 1)
                            msg += " and ";
                        else if (i > 0)
                            msg += ", ";

                        msg += default_app_setting_error[i];
                    }

                    if (default_app_setting_error.Count > 1)
                        msg += " are missing.";
                    else
                        msg += " is missing.";

                    errorMessage = msg;
                }
                else
                    errorMessage = ex.Message;
            }

            return hasError
                ? req.CreateResponse(HttpStatusCode.InternalServerError, BuildResponseMessage(HttpStatusCode.InternalServerError, errorMessage, mail_data, _req))
                : req.CreateResponse(HttpStatusCode.OK, BuildResponseMessage(HttpStatusCode.OK, "Emails have been sent.", mail_data, _req));
        }

        #region -- Class and Parameters --
        public class RequestBody
        {
            public string Configuration { get; set; }
            public string Configuration_lang { get; set; }
            public string MJAPI_PublicKey { get; set; }
            public string MJAPI_PrivateKey { get; set; }
            public string SenderMail { get; set; }
            public string SenderName { get; set; }
            public string Subject { get; set; }
            public string Body_TextPlain { get; set; }
            public string Body_HTML { get; set; }
            public string CustomID { get; set; }
            public object To { get; set; }
            public object Cc { get; set; }
            public object Bcc { get; set; }
            public object Test_To { get; set; }
            public object Test_Cc { get; set; }
            public object Test_Bcc { get; set; }
            public string ReplyTo { get; set; }
            public object Attachments { get; set; }
            public object Attachments_Base64 { get; set; }
            public string TemplateID { get; set; }
            public object data { get; set; }
            public string Template { get; set; }
            public string Template_lang { get; set; }
            public string Template_HTML { get; set; }
            public string Template_HTML_lang { get; set; }
            public string xsl { get; set; }
            public string xsl_lang { get; set; }
            public string xslHTML { get; set; }
            public string xslHTML_lang { get; set; }
            public string xslPre { get; set; }
            public string xslPre_lang { get; set; }
            public string xslPre_HTML { get; set; }
            public string xslPre_HTML_lang { get; set; }
            public string lang { get; set; }
            public string CustomCampaign { get; set; }
            public object DeduplicateCampaign { get; set; }
            public string TrackOpens { get; set; }
            public string TrackClicks { get; set; }
            public string EventPayload { get; set; }
            public string MonitoringCategory { get; set; }
            public string URLTags { get; set; }
            public object Headers { get; set; }
            public object Variables { get; set; }
        }

        public static class _Parameters
        {
            public static string Configuration = "Configuration";
            public static string Configuration_lang = "Configuration_lang";
            public static string Configuration_SettingName = "Configuration_SettingName";
            public static string MJAPI_PublicKey = "MJAPI_PublicKey";
            public static string MJAPI_PublicKey_ConnectionStringName = "MJAPI_PublicKey_ConnectionStringName";
            public static string MJAPI_PrivateKey = "MJAPI_PrivateKey";
            public static string MJAPI_PrivateKey_ConnectionStringName = "MJAPI_PrivateKey_ConnectionStringName";
            public static string SenderMail = "SenderMail";
            public static string SenderMail_SettingName = "SenderMail_SettingName";
            public static string SenderName = "SenderName";
            public static string SenderName_SettingName = "SenderName_SettingName";
            public static string Subject = "Subject";
            public static string Subject_SettingName = "Subject_SettingName";
            public static string Body_TextPlain = "Body_TextPlain";
            public static string Body_TextPlain_SettingName = "Body_TextPlain_SettingName";
            public static string Body_HTML = "Body_HTML";
            public static string Body_HTML_SettingName = "Body_HTML_SettingName";
            public static string CustomID = "CustomID";
            public static string CustomID_SettingName = "CustomID_SettingName";
            public static string To = "To";
            public static string To_SettingName = "To_SettingName";
            public static string Cc = "Cc";
            public static string Cc_SettingName = "Cc_SettingName";
            public static string Bcc = "Bcc";
            public static string Bcc_SettingName = "Bcc_SettingName";
            public static string Test_To = "Test_To";
            public static string Test_To_SettingName = "Test_To_SettingName";
            public static string Test_Cc = "Test_Cc";
            public static string Test_Cc_SettingName = "Test_Cc_SettingName";
            public static string Test_Bcc = "Test_Bcc";
            public static string Test_Bcc_SettingName = "Test_Bcc_SettingName";
            public static string ReplyTo = "ReplyTo";
            public static string ReplyTo_SettingName = "ReplyTo_SettingName";
            public static string Attachments = "Attachments";
            public static string Attachments_SettingName = "Attachments_SettingName";
            public static string Attachments_Base64 = "Attachments_Base64";
            public static string Attachments_Base64_SettingName = "Attachments_Base64_SettingName";
            public static string TemplateID = "TemplateID";
            public static string TemplateID_SettingName = "TemplateID_SettingName";
            public static string data = "data";
            public static string Template = "Template";
            public static string Template_lang = "Template_lang";
            public static string Template_SettingName = "Template_SettingName";
            public static string Template_HTML = "Template_HTML";
            public static string Template_HTML_lang = "Template_HTML_lang";
            public static string Template_HTML_SettingName = "Template_HTML_SettingName";
            public static string xsl = "xsl";
            public static string xsl_lang = "xsl_lang";
            public static string xsl_SettingName = "xsl_SettingName";
            public static string xslHTML = "xslHTML";
            public static string xslHTML_lang = "xslHTML_lang";
            public static string xslHTML_SettingName = "xslHTML_SettingName";
            public static string xslPre = "xslPre";
            public static string xslPre_lang = "xslPre_lang";
            public static string xslPre_SettingName = "xslPre_SettingName";
            public static string xslPre_HTML = "xslPre_HTML";
            public static string xslPre_HTML_lang = "xslPre_HTML_lang";
            public static string xslPre_HTML_SettingName = "xslPre_HTML_SettingName";
            public static string lang = "lang";
            public static string lang_SettingName = "lang_SettingName";
            public static string CustomCampaign = "CustomCampaign";
            public static string CustomCampaign_SettingName = "CustomCampaign_SettingName";
            public static string DeduplicateCampaign = "DeduplicateCampaign";
            public static string DeduplicateCampaign_SettingName = "DeduplicateCampaign_SettingName";
            public static string TrackOpens = "TrackOpens";
            public static string TrackOpens_SettingName = "TrackOpens_SettingName";
            public static string TrackClicks = "TrackClicks";
            public static string TrackClicks_SettingName = "TrackClicks_SettingName";
            public static string EventPayload = "EventPayload";
            public static string EventPayload_SettingName = "EventPayload_SettingName";
            public static string MonitoringCategory = "MonitoringCategory";
            public static string MonitoringCategory_SettingName = "MonitoringCategory_SettingName";
            public static string URLTags = "URLTags";
            public static string URLTags_SettingName = "URLTags_SettingName";
            public static string Headers = "Headers";
            public static string Headers_SettingName = "Headers_SettingName";
            public static string Variables = "Variables";
            public static string Variables_SettingName = "Variables_SettingName";
        }

        public class Base64Attachment
        {
            public string FileName { get; set; }
            public string Content_Type { get; set; }
            public string Base64_Content { get; set; }
        }
        #endregion

        #region -- Template functions --
        public static string GetTemplate(RequestBody _req, ExecutionContext context)
        {
            string TextTemplate = string.Empty;

            if (_req != null)
            {
                if (!string.IsNullOrEmpty(_req.Template_lang))
                    TextTemplate = GetContentFile(_req.Template_lang, context);
                else
                    TextTemplate = GetContentFile(_req.Template, context);

                if (_req.data != null)
                {
                    try
                    {
                        JObject data = new JObject();

                        try
                        {
                            data = JObject.Parse(_req.data.ToString());
                        }
                        catch { }

                        foreach (KeyValuePair<string, JToken> k in data)
                        {
                            if (k.Value is JValue)
                                TextTemplate = TextTemplate.Replace("%" + k.Key + "%", k.Value.ToString());
                        }
                    }
                    catch { }
                }
            }

            return TextTemplate;
        }

        public static string GetTemplate_HTML(RequestBody _req, ExecutionContext context)
        {
            string TextTemplate = string.Empty;

            if (_req != null)
            {
                if (!string.IsNullOrEmpty(_req.Template_HTML_lang))
                    TextTemplate = GetContentFile(_req.Template_HTML_lang, context);
                else
                    TextTemplate = GetContentFile(_req.Template_HTML, context);

                if (_req.data != null)
                {
                    try
                    {
                        JObject data = new JObject();

                        try
                        {
                            data = JObject.Parse(_req.data.ToString());
                        }
                        catch { }

                        foreach (KeyValuePair<string, JToken> k in data)
                        {
                            if (k.Value is JValue)
                                TextTemplate = TextTemplate.Replace("%" + k.Key + "%", k.Value.ToString());
                        }
                    }
                    catch { }
                }
            }

            return TextTemplate;
        }

        public static string GetXSL(ExecutionContext context, RequestBody _req)
        {
            string output = string.Empty;

            if (context != null && _req != null)
            {
                string data_variable = string.Empty;

                try
                {
                    data_variable = JsonConvert.SerializeObject(_req.data);
                }
                catch { }

                string xmlstr = "<data />";
                try
                {
                    xmlstr = convertJSON2XML(JObject.Parse(_req.data.ToString()));
                }
                catch { }

                var xml = xmlstr;

                if (!string.IsNullOrEmpty(_req.xslPre) || !string.IsNullOrEmpty(_req.xslPre_lang))
                {
                    string path = string.Empty;
                    string tmp_uri = string.Empty;

                    try
                    {
                        if (!string.IsNullOrEmpty(_req.xslPre_lang))
                            tmp_uri = _req.xslPre_lang;
                        else if (!string.IsNullOrEmpty(_req.xslPre))
                            tmp_uri = _req.xslPre;

                        Uri _uri = new Uri(tmp_uri);
                        path = _uri.AbsolutePath;
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(tmp_uri))
                            path = Path.Combine(context.FunctionDirectory, tmp_uri);
                    }

                    try
                    {
                        MemoryStream os = doXSLT20(xmlstr, path);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    catch { }
                }

                XslCompiledTransform transform = new XslCompiledTransform();

                string xsltString = string.Empty;

                if (!string.IsNullOrEmpty(_req.xsl_lang))
                    xsltString = GetContentFile(_req.xsl_lang, context);
                else if (!string.IsNullOrEmpty(_req.xsl))
                    xsltString = GetContentFile(_req.xsl, context);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;

                using (XmlReader reader = XmlReader.Create(new StringReader(xsltString), settings))
                {
                    try
                    {
                        transform.Load(reader);
                    }
                    catch { }
                }

                StringWriter results = new StringWriter();
                using (XmlReader reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    try
                    {
                        transform.Transform(reader, null, results);
                    }
                    catch { }
                }

                try
                {
                    output = results.ToString();

                    if (output.Contains("<?xml version="))
                    {
                        // remove xml tag    
                        if (string.Compare(output.Substring(0, 14), "<?xml version=") == 0)
                        {
                            output = output.Substring(output.IndexOf('>') + 1);
                        }
                    }
                }
                catch
                {
                    if (!string.IsNullOrEmpty(_req.xsl))
                        throw new Exception("XSL Template does not support Pre-Transformation.");
                }
            }

            return output;
        }

        public static string GetXSL_HTML(ExecutionContext context, RequestBody _req)
        {
            string output = string.Empty;

            if (context != null && _req != null)
            {
                string data_variable = string.Empty;

                try
                {
                    data_variable = JsonConvert.SerializeObject(_req.data);
                }
                catch { }

                string xmlstr = "<data />";
                try
                {
                    //xmlstr = convertJSON2XML(data_dic);
                    xmlstr = convertJSON2XML(JObject.Parse(_req.data.ToString()));
                }
                catch { }

                var xml = xmlstr;

                if (!string.IsNullOrEmpty(_req.xslPre_HTML) || !string.IsNullOrEmpty(_req.xslPre_HTML_lang))
                {
                    string path = string.Empty;
                    string tmp_uri = string.Empty;

                    try
                    {
                        if (!string.IsNullOrEmpty(_req.xslPre_HTML_lang))
                            tmp_uri = _req.xslPre_HTML_lang;
                        else if (!string.IsNullOrEmpty(_req.xslPre_HTML))
                            tmp_uri = _req.xslPre_HTML;

                        Uri _uri = new Uri(tmp_uri);
                        path = _uri.AbsolutePath;
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(tmp_uri))
                            path = Path.Combine(context.FunctionDirectory, tmp_uri);
                    }

                    try
                    {
                        MemoryStream os = doXSLT20(xmlstr, path);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    catch { }
                }

                XslCompiledTransform transform = new XslCompiledTransform();

                string xsltString = string.Empty;

                if (!string.IsNullOrEmpty(_req.xslHTML_lang))
                    xsltString = GetContentFile(_req.xslHTML_lang, context);
                else if (!string.IsNullOrEmpty(_req.xslHTML))
                    xsltString = GetContentFile(_req.xslHTML, context);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;

                using (XmlReader reader = XmlReader.Create(new StringReader(xsltString), settings))
                {
                    try
                    {
                        transform.Load(reader);
                    }
                    catch { }
                }

                StringWriter results = new StringWriter();
                using (XmlReader reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    try
                    {
                        transform.Transform(reader, null, results);
                    }
                    catch { }
                }

                try
                {
                    output = results.ToString();

                    if (output.Contains("<?xml version="))
                    {
                        // remove xml tag    
                        if (string.Compare(output.Substring(0, 14), "<?xml version=") == 0)
                        {
                            output = output.Substring(output.IndexOf('>') + 1);
                        }
                    }
                }
                catch
                {
                    if (!string.IsNullOrEmpty(_req.xslHTML))
                        throw new Exception("XSL Template does not support Pre-Transformation.");
                }
            }

            return output;
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

                        string val = value.ToString();
                        if (value.Type == JTokenType.Boolean || value.Type == JTokenType.Float || value.Type == JTokenType.Date)
                        {
                            val = JsonConvert.SerializeObject(value);
                            if (value.Type == JTokenType.Date)
                            {
                                val = val.Substring(1, val.Length - 2);
                            }
                        }
                        propertyElement.SetAttribute("value", val);
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
                        JValue value = (JValue)item;

                        string val = value.ToString();
                        if (value.Type == JTokenType.Boolean || value.Type == JTokenType.Float || value.Type == JTokenType.Date)
                        {
                            val = JsonConvert.SerializeObject(value);
                            if (value.Type == JTokenType.Date)
                            {
                                val = val.Substring(1, val.Length - 2);
                            }
                        }
                        memberElement.SetAttribute("value", val);
                        el.AppendChild(memberElement);
                    }
                }
            }
        }

        public static string convertJSON2XML(JObject obj)
        {
            XmlDocument doc = new XmlDocument();
            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            convertJSON2XML("data", null, obj, doc);
            return doc.OuterXml;
        }

        public static MemoryStream doXSLT20(string xml, string xsl)
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
        #endregion

        #region -- Function for get request body and configuration files --
        public static JObject BuildResponseMessage(HttpStatusCode status, string status_message, JArray mailjet_response, RequestBody _req)
        {
            JObject obj = new JObject();
            obj.Add("status", (int)status);
            obj.Add("status_message", status_message);
            obj.Add("mailjet_response", mailjet_response);

            try
            {
                string[] a = TryParseString(_req.To).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.To = null;
                else
                    _req.To = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Cc).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Cc = null;
                else
                    _req.Cc = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Bcc).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Bcc = null;
                else
                    _req.Bcc = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Test_To).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Test_To = null;
                else
                    _req.Test_To = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Test_Cc).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Test_Cc = null;
                else
                    _req.Test_Cc = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Test_Bcc).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Test_Bcc = null;
                else
                    _req.Test_Bcc = a;
            }
            catch { }
            try
            {
                string[] a = TryParseString(_req.Attachments).Split(',');
                if (string.IsNullOrEmpty(a[0]))
                    _req.Attachments = null;
                else
                    _req.Attachments = a;
            }
            catch { }
            try
            {
                string a = TryParseString(_req.Attachments_Base64).Replace("{", string.Empty).Replace("}", string.Empty).Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                if (string.IsNullOrEmpty(a))
                    _req.Attachments_Base64 = null;
                else
                    _req.Attachments_Base64 = JArray.Parse(TryParseString(_req.Attachments_Base64));
            }
            catch { }
            try
            {
                string a = TryParseString(_req.data).Replace("{", string.Empty).Replace("}", string.Empty).Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                if (string.IsNullOrEmpty(a))
                    _req.data = null;
                else
                    _req.data = JObject.Parse(TryParseString(_req.data));
            }
            catch { }

            if(_req.DeduplicateCampaign != null && !string.IsNullOrEmpty(_req.DeduplicateCampaign.ToString()))
            {
                bool _val = false;

                if(bool.TryParse(_req.DeduplicateCampaign.ToString(), out _val))
                {
                    _req.DeduplicateCampaign = _val;
                }
            }

            if (!string.IsNullOrEmpty(_req.Configuration_lang))
                _req.Configuration = _req.Configuration_lang;

            if (!string.IsNullOrEmpty(_req.Template_lang))
                _req.Template = _req.Template_lang;

            if (!string.IsNullOrEmpty(_req.xsl_lang))
                _req.xsl = _req.xsl_lang;

            if (!string.IsNullOrEmpty(_req.xslPre_lang))
                _req.xslPre = _req.xslPre_lang;

            if (!string.IsNullOrEmpty(_req.Template_HTML_lang))
                _req.Template = _req.Template_HTML_lang;

            if (!string.IsNullOrEmpty(_req.xslHTML_lang))
                _req.xsl = _req.xslHTML_lang;

            if (!string.IsNullOrEmpty(_req.xslPre_HTML_lang))
                _req.xslPre = _req.xslPre_HTML_lang;

            if (string.IsNullOrEmpty(_req.xsl))
                _req.xslPre = null;

            _req.MJAPI_PublicKey = string.IsNullOrEmpty(_req.MJAPI_PublicKey) ? null : "*****";
            _req.MJAPI_PrivateKey = string.IsNullOrEmpty(_req.MJAPI_PrivateKey) ? null : "*****";
            _req.Body_HTML = string.IsNullOrEmpty(_req.Body_HTML) ? null : _req.Body_HTML;
            _req.Body_TextPlain = string.IsNullOrEmpty(_req.Body_TextPlain) ? null : _req.Body_TextPlain;

            obj.Add("mailjetInfo", JObject.Parse(JsonConvert.SerializeObject(_req)));
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.Configuration_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.Template_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.xsl_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.xslPre_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.Template_HTML_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.xslHTML_lang);
            (obj["mailjetInfo"] as JObject).Remove(_Parameters.xslPre_HTML_lang);

            return obj;
        }

        public static string ConvertRequestValue(object input)
        {
            string tmp = input == null ? string.Empty : input.ToString();
            if (string.IsNullOrEmpty(tmp))
                return null;

            return tmp.Replace("\r\n", string.Empty).Replace("\"", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace(" ", string.Empty);
        }

        public static JObject GetConfigurationFile(JObject obj, ExecutionContext context)
        {
            JObject result = new JObject();

            try
            {
                string content = string.Empty;

                if (obj[_Parameters.Configuration_lang] != null && !string.IsNullOrEmpty(obj[_Parameters.Configuration_lang].ToString()))
                {
                    content = GetContentFile(obj[_Parameters.Configuration_lang].ToString(), context);
                }
                else
                {
                    content = GetContentFile(obj[_Parameters.Configuration].ToString(), context);
                }

                result = JObject.Parse(content);
            }
            catch { }

            return result;
        }

        public static string GetParameterInApplicationSetting(string key)
        {
            string result = null;
            try
            {
                result = System.Environment.GetEnvironmentVariable(key);
            }
            catch { }

            return result;
        }

        public static string GetParameterInConnectionString(string key)
        {
            string result = null;
            try
            {
                result = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }
            catch { }

            return result;
        }

        public static string GetContentFile(string file_path, ExecutionContext context)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(file_path))
            {
                try
                {
                    //Assume the template is url
                    using (WebClient wclient = new WebClient())
                    {
                        result = wclient.DownloadString(file_path);
                    }
                }
                catch
                {
                    try
                    {
                        //Assume the template is local file
                        string path = Path.Combine(context.FunctionDirectory, file_path);
                        result = File.ReadAllText(path);
                    }
                    catch { }
                }

            }

            return result;
        }

        public static string TryParseString(object input)
        {
            if (input == null)
                return string.Empty;

            return input.ToString();
        }

        public static string GetNameFromEmail(string mail)
        {
            string result = null;

            try
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    if (mail.Contains("<") && mail.Contains(">"))
                    {
                        //Anat < a.usa - ampai@togetherteam.co.th >
                        result = mail.Trim().Substring(0, mail.IndexOf("<")).Trim();
                        if (string.IsNullOrEmpty(result))
                        {
                            result = mail.Trim().Substring(1, mail.IndexOf("<"));
                        }
                    }
                    else
                    {
                        //a.usa - ampai@togetherteam.co.th
                        result = mail.Trim().Substring(0, mail.IndexOf("@"));
                    }
                }
            }
            catch { }

            return result;
        }

        public static JObject PrepareParameter(JObject direct_param, ExecutionContext context, HttpRequestMessage req)
        {
            JObject obj = new JObject();
            string[] keys = new string[]
            {
                _Parameters.Configuration,
                _Parameters.Configuration_lang,
                _Parameters.Configuration_SettingName,
                _Parameters.MJAPI_PublicKey,
                _Parameters.MJAPI_PublicKey_ConnectionStringName,
                _Parameters.MJAPI_PrivateKey,
                _Parameters.MJAPI_PrivateKey_ConnectionStringName,
                _Parameters.SenderMail,
                _Parameters.SenderMail_SettingName,
                _Parameters.SenderName,
                _Parameters.SenderName_SettingName,
                _Parameters.Subject,
                _Parameters.Subject_SettingName,
                _Parameters.CustomID,
                _Parameters.CustomID_SettingName,
                _Parameters.To,
                _Parameters.To_SettingName,
                _Parameters.Cc,
                _Parameters.Cc_SettingName,
                _Parameters.Bcc,
                _Parameters.Bcc_SettingName,
                _Parameters.Test_To,
                _Parameters.Test_To_SettingName,
                _Parameters.Test_Cc,
                _Parameters.Test_Cc_SettingName,
                _Parameters.Test_Bcc,
                _Parameters.Test_Bcc_SettingName,
                _Parameters.ReplyTo,
                _Parameters.ReplyTo_SettingName,
                _Parameters.Attachments,
                _Parameters.Attachments_SettingName,
                _Parameters.Attachments_Base64,
                _Parameters.Attachments_Base64_SettingName,
                _Parameters.xslPre,
                _Parameters.xslPre_lang,
                _Parameters.xslPre_SettingName,
                _Parameters.xslPre_HTML,
                _Parameters.xslPre_HTML_lang,
                _Parameters.xslPre_HTML_SettingName,
                _Parameters.lang,
                _Parameters.lang_SettingName,
                _Parameters.CustomCampaign,
                _Parameters.CustomCampaign_SettingName,
                _Parameters.DeduplicateCampaign,
                _Parameters.DeduplicateCampaign_SettingName,
                _Parameters.TrackOpens,
                _Parameters.TrackOpens_SettingName,
                _Parameters.TrackClicks,
                _Parameters.TrackClicks_SettingName,
                _Parameters.EventPayload,
                _Parameters.EventPayload_SettingName,
                _Parameters.MonitoringCategory,
                _Parameters.MonitoringCategory_SettingName,
                _Parameters.URLTags,
                _Parameters.lang_SettingName,
                _Parameters.URLTags_SettingName,
                _Parameters.Headers,
                _Parameters.Headers_SettingName,
                _Parameters.Variables,
                _Parameters.Variables_SettingName
            };

            string[] keys_msg = new string[] {
                _Parameters.TemplateID,
                _Parameters.TemplateID_SettingName,
                _Parameters.Body_HTML,
                _Parameters.Body_HTML_SettingName,
                _Parameters.Template_HTML,
                _Parameters.Template_HTML_lang,
                _Parameters.Template_HTML_SettingName,
                _Parameters.xslHTML,
                _Parameters.xslHTML_lang,
                _Parameters.xslHTML_SettingName,
                _Parameters.Body_TextPlain,
                _Parameters.Body_TextPlain_SettingName,
                _Parameters.Template,
                _Parameters.Template_lang,
                _Parameters.Template_SettingName,
                _Parameters.xsl,
                _Parameters.xsl_lang,
                _Parameters.xsl_SettingName
            };

            string[] keys_lang = new string[]
            {
                _Parameters.Template_lang,
                _Parameters.Template_HTML_lang,
                _Parameters.xsl_lang,
                _Parameters.xslHTML_lang,
                _Parameters.xslPre_lang,
                _Parameters.xslPre_HTML_lang
            };

            Dictionary<string, bool> dicApplyConfig = new Dictionary<string, bool>();
            JObject config = new JObject();
            bool hasLang = false;

            try
            {
                // --------- Request Body -------------
                foreach (string key in keys)
                {
                    obj.Add(key, null);
                }

                foreach (string key in keys_msg)
                {
                    obj.Add(key, null);
                }

                if (direct_param != null)
                {
                    foreach (KeyValuePair<string, JToken> t in direct_param)
                    {
                        obj[t.Key] = t.Value;
                    }
                }

                if (string.IsNullOrEmpty(obj[_Parameters.Configuration_SettingName].ToString()))
                    obj[_Parameters.Configuration_SettingName] = "MailJet_DEFAULT_Configuration";

                obj = GetValueFromAppSetting(obj, keys);
                obj = GetValueFromAppSetting(obj, keys_msg);

                if (obj[_Parameters.lang] != null && !string.IsNullOrEmpty(obj[_Parameters.lang].ToString()))
                {
                    obj = ApplyLanguage(obj, new string[] { _Parameters.Configuration_lang }, null, false);
                    hasLang = true;
                }

                // ---------- Configuration File -----------
                config = GetConfigurationFile(obj, context);
                config = GetValueFromAppSetting(config, keys);
                config = GetValueFromAppSetting(config, keys_msg);

                if (!hasLang)
                {
                    if (config[_Parameters.lang] != null && !string.IsNullOrEmpty(config[_Parameters.lang].ToString()))
                    {
                        hasLang = true;
                        obj = ParameterMapping(obj, config, keys, keys_msg, out dicApplyConfig, true);
                    }
                }
                else
                {
                    obj = ParameterMapping(obj, config, keys, keys_msg, out dicApplyConfig, true);
                }

                // ----------- Default Application Setting --------------
                JObject _default = new JObject();

                foreach (string key in keys)
                {
                    _default.Add(key, null);
                }

                foreach (string key in keys_msg)
                {
                    _default.Add(key, null);
                }

                _default[_Parameters.MJAPI_PublicKey_ConnectionStringName] = "MailJet_DEFAULT_MJAPI_PublicKey";
                _default[_Parameters.MJAPI_PrivateKey_ConnectionStringName] = "MailJet_DEFAULT_MJAPI_PrivateKey";
                _default[_Parameters.SenderMail_SettingName] = "MailJet_DEFAULT_SenderMail";
                _default[_Parameters.SenderName_SettingName] = "MailJet_DEFAULT_SenderName";
                _default[_Parameters.Subject_SettingName] = "MailJet_DEFAULT_Subject";
                _default[_Parameters.Body_TextPlain_SettingName] = "MailJet_DEFAULT_Body_TextPlain";
                _default[_Parameters.Body_HTML_SettingName] = "MailJet_DEFAULT_Body_HTML";
                _default[_Parameters.CustomID_SettingName] = "MailJet_DEFAULT_CustomID";
                _default[_Parameters.To_SettingName] = "MailJet_DEFAULT_To";
                _default[_Parameters.Cc_SettingName] = "MailJet_DEFAULT_Cc";
                _default[_Parameters.Bcc_SettingName] = "MailJet_DEFAULT_Bcc";
                _default[_Parameters.Test_To_SettingName] = "MailJet_DEFAULT_Test_To";
                _default[_Parameters.Test_Cc_SettingName] = "MailJet_DEFAULT_Test_Cc";
                _default[_Parameters.Test_Bcc_SettingName] = "MailJet_DEFAULT_Test_Bcc";
                _default[_Parameters.ReplyTo_SettingName] = "MailJet_DEFAULT_ReplyTo";
                _default[_Parameters.Attachments_SettingName] = "MailJet_DEFAULT_Attachments";
                _default[_Parameters.Attachments_Base64_SettingName] = "MailJet_DEFAULT_Attachments_Base64";
                _default[_Parameters.TemplateID_SettingName] = "MailJet_DEFAULT_TemplateID";
                _default[_Parameters.Template_SettingName] = "MailJet_DEFAULT_Template";
                _default[_Parameters.Template_HTML_SettingName] = "MailJet_DEFAULT_Template_HTML";
                _default[_Parameters.xsl_SettingName] = "MailJet_DEFAULT_xsl";
                _default[_Parameters.xslHTML_SettingName] = "MailJet_DEFAULT_xslHTML";
                _default[_Parameters.xslPre_SettingName] = "MailJet_DEFAULT_xslPre";
                _default[_Parameters.xslPre_HTML_SettingName] = "MailJet_DEFAULT_xslPre_HTML";
                _default[_Parameters.lang_SettingName] = "MailJet_DEFAULT_lang";
                _default[_Parameters.CustomCampaign_SettingName] = "MailJet_DEFAULT_CustomCampaign";
                _default[_Parameters.DeduplicateCampaign_SettingName] = "MailJet_DEFAULT_DeduplicateCampaign";
                _default[_Parameters.TrackOpens_SettingName] = "MailJet_DEFAULT_TrackOpens";
                _default[_Parameters.TrackClicks_SettingName] = "MailJet_DEFAULT_TrackClicks";
                _default[_Parameters.EventPayload_SettingName] = "MailJet_DEFAULT_EventPayload";
                _default[_Parameters.MonitoringCategory_SettingName] = "MailJet_DEFAULT_MonitoringCategory";
                _default[_Parameters.URLTags_SettingName] = "MailJet_DEFAULT_URLTags";
                _default[_Parameters.Headers_SettingName] = "MailJet_DEFAULT_Headers";
                _default[_Parameters.Variables_SettingName] = "MailJet_DEFAULT_Variables";

                _default = GetValueFromAppSetting(_default, keys);
                _default = GetValueFromAppSetting(_default, keys_msg);

                if (!hasLang)
                {
                    if (_default[_Parameters.lang] != null && !string.IsNullOrEmpty(_default[_Parameters.lang].ToString()))
                    {
                        obj[_Parameters.lang] = _default[_Parameters.lang];
                        obj = ApplyLanguage(obj, new string[] { _Parameters.Configuration_lang }, null, false);

                        config = new JObject();
                        config = GetConfigurationFile(obj, context);
                        config = GetValueFromAppSetting(config, keys);
                        config = GetValueFromAppSetting(config, keys_msg);
                        obj = ParameterMapping(obj, config, keys, keys_msg, out dicApplyConfig, true);
                        hasLang = true;
                    }
                    else
                    {
                        obj = ParameterMapping(obj, config, keys, keys_msg, out dicApplyConfig, true);
                    }
                }
                // -------------------------------------------------

                Dictionary<string, bool> tmp = new Dictionary<string, bool>();
                obj = ParameterMapping(obj, _default, keys, keys_msg, out tmp, false);

                if (string.IsNullOrEmpty(obj[_Parameters.MJAPI_PublicKey].ToString()))
                    obj[_Parameters.MJAPI_PublicKey] = "fe985d14b0ed6a04152df6407593287a";

                if (string.IsNullOrEmpty(obj[_Parameters.MJAPI_PrivateKey].ToString()))
                    obj[_Parameters.MJAPI_PrivateKey] = "5ff23cdfddf6a8a9e0592c8042553005";

                if (string.IsNullOrEmpty(obj[_Parameters.Subject].ToString()))
                    obj[_Parameters.Subject] = "Test mail sending from MailJet function.";

                if (string.IsNullOrEmpty(obj[_Parameters.SenderMail].ToString()))
                    obj[_Parameters.SenderMail] = "office@togetherteam.co.th";

                if (string.IsNullOrEmpty(obj[_Parameters.SenderName].ToString()))
                    obj[_Parameters.SenderName] = "Office";

                if (string.IsNullOrEmpty(obj[_Parameters.To].ToString()))
                    obj[_Parameters.To] = "office@togetherteam.co.th";

                if (!HasMessageValue(obj, keys_msg))
                    obj[_Parameters.xsl] = "default.xsl";

                obj[_Parameters.To] = ConvertRequestValue(obj[_Parameters.To]);
                obj[_Parameters.Cc] = ConvertRequestValue(obj[_Parameters.Cc]);
                obj[_Parameters.Bcc] = ConvertRequestValue(obj[_Parameters.Bcc]);
                obj[_Parameters.Test_To] = ConvertRequestValue(obj[_Parameters.Test_To]);
                obj[_Parameters.Test_Cc] = ConvertRequestValue(obj[_Parameters.Test_Cc]);
                obj[_Parameters.Test_Bcc] = ConvertRequestValue(obj[_Parameters.Test_Bcc]);
                obj[_Parameters.Attachments] = ConvertRequestValue(obj[_Parameters.Attachments]);

                obj = ApplyLanguage(obj, keys_lang, dicApplyConfig, true);

                // --------------- Apply 'data' parameter --------------------
                JObject val1 = ConvertToJObject(obj[_Parameters.data]);
                JObject val2 = ConvertToJObject(config[_Parameters.data]);

                if (val1 != null && val2 != null)
                {
                    val2.Merge(val1);
                    obj[_Parameters.data] = val2;
                }
                else if (val1 != null)
                    obj[_Parameters.data] = val1;
                else if (val2 != null)
                    obj[_Parameters.data] = val2;
                else
                    obj[_Parameters.data] = null;
                // ----------------------------------------------------------
            }
            catch { }

            return obj;
        }

        public static JObject GetValueFromAppSetting(JObject obj, string[] keys)
        {
            try
            {
                foreach (string key in keys)
                {
                    string tmp = key.Replace("_SettingName", string.Empty).Replace("_ConnectionStringName", string.Empty).Trim();

                    JToken _token;
                    if (!obj.TryGetValue(key, out _token))
                        obj.Add(key, null);

                    if (key.EndsWith("_SettingName"))
                    {
                        if (string.IsNullOrEmpty(obj[tmp].ToString()))
                            obj[tmp] = GetParameterInApplicationSetting(obj[key].ToString());
                    }
                    else if (key.EndsWith("_ConnectionStringName"))
                    {
                        if (string.IsNullOrEmpty(obj[tmp].ToString()))
                            obj[tmp] = GetParameterInConnectionString(obj[key].ToString());
                    }
                }
            }
            catch { }

            return obj;
        }

        public static JObject ConvertToJObject(object input)
        {
            try
            {
                JObject obj = new JObject();
                JObject tmp = JObject.Parse(input.ToString());

                foreach (KeyValuePair<string, JToken> t in tmp)
                {
                    if (!string.IsNullOrEmpty(t.Value.ToString()))
                        obj.Add(t.Key, t.Value);
                }

                return obj;
            }
            catch
            {
                return null;
            }
        }

        public static bool HasMessageValue(JObject source, string[] keys)
        {
            bool result = false;

            if (source != null)
            {
                foreach (string t in keys)
                {
                    if (source[t] != null && !string.IsNullOrEmpty(source[t].ToString()) && !t.EndsWith("_SettingName"))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public static JObject ParameterMapping(JObject source1, JObject source2, string[] keys, string[] keys_msg, out Dictionary<string, bool> dicApplyConfig, bool isApplyConfig)
        {
            dicApplyConfig = new Dictionary<string, bool>();

            foreach (string k in keys)
            {
                if (!k.Contains($"_{_Parameters.lang}") && source1[$"{k}_{_Parameters.lang}"] != null)
                {
                    if (string.IsNullOrEmpty(source1[k].ToString()))
                    {
                        if (!string.IsNullOrEmpty(source2[k].ToString()))
                        {
                            source1[k] = source2[k];
                            if (isApplyConfig)
                                dicApplyConfig.Add($"{k}_{_Parameters.lang}", true);
                        }
                    }
                }
                else if (k.Contains($"_{_Parameters.lang}")) { }
                else
                {
                    if (string.IsNullOrEmpty(source1[k].ToString()))
                        source1[k] = source2[k];
                }
            }

            if (!HasMessageValue(source1, keys_msg))
            {
                foreach (string k in keys_msg)
                {
                    if (!string.IsNullOrEmpty(source2[k].ToString()))
                    {
                        source1[k] = source2[k];

                        if (source1[$"{k}_{_Parameters.lang}"] != null && isApplyConfig)
                            dicApplyConfig.Add($"{k}_{_Parameters.lang}", true);
                    }
                }
            }

            return source1;
        }

        public static JObject ApplyLanguage(JObject obj, string[] keys, Dictionary<string, bool> dicApplyConfig, bool isApplyFull)
        {
            JObject output = obj;

            if (obj != null)
            {
                if (obj[_Parameters.lang] != null && !string.IsNullOrEmpty(obj[_Parameters.lang].ToString()))
                {
                    string lang = obj[_Parameters.lang].ToString();

                    foreach (string key in keys)
                    {
                        string main_key = key.Replace($"_{_Parameters.lang}", string.Empty);
                        string main_param = obj[main_key] == null ? null : obj[main_key].ToString();
                        bool isConvert = false;

                        if (!string.IsNullOrEmpty(main_param))
                        {
                            if (main_param.Contains("\\"))
                            {
                                main_param = main_param.Replace("\\", "/");
                                isConvert = true;
                            }

                            //Case: ../aaaa/aaa.aaa
                            int index_dot = main_param.LastIndexOf(".");
                            int index_slash = main_param.LastIndexOf("/");

                            if (index_dot > 0 && index_dot > index_slash)
                            {
                                string prefix = main_param.Substring(0, index_dot);
                                string suffix = main_param.Substring(index_dot);
                                main_param = $"{prefix}_{lang}{suffix}";

                                bool isSkip = false;


                                if (dicApplyConfig != null && isApplyFull)
                                {
                                    foreach (KeyValuePair<string, bool> a in dicApplyConfig)
                                    {
                                        if (key.Equals(a.Key))
                                        {
                                            isSkip = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isSkip)
                                    output[key] = isConvert ? main_param.Replace("/", "\\") : main_param;
                            }
                            else
                            {
                                //Case: ../aaaaa/aaaa
                                string tmp = main_param + $"_{lang}";

                                bool isSkip = false;

                                if (dicApplyConfig != null && isApplyFull)
                                {
                                    foreach (KeyValuePair<string, bool> a in dicApplyConfig)
                                    {
                                        if (key.Equals(a.Key))
                                        {
                                            isSkip = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isSkip)
                                    output[key] = isConvert ? tmp.Replace("/", "\\") : tmp;
                            }
                        }
                    }

                }
            }

            return output;
        }
        #endregion