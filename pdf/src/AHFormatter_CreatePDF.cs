using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using XfoDotNetCtl;
using System;
using Saxon.Api;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AHFormatter_CreatePDF
{
    public static class AHFormatter_CreatePDF
    {
        [FunctionName("AHFormatter_CreatePDF")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("Starting creation of PDF...");
            string homeloc = context.FunctionDirectory;
            string rootloc = Directory.GetParent(homeloc).FullName;

            string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process);
            string ahlpath1 = homeloc + "/ahflibs";
            string ahlpath2 = rootloc + "/ahflibs";
            // The ahlpath1 is for Azure portal when we deploy pre-compiled function
            if (!path.Contains(ahlpath1) && Directory.Exists(ahlpath1))
            {
                path += ";" + ahlpath1;
                Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
                log.Info("location " + ahlpath1 + " added to PATH");
            }
            path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process);
            // The ahlpath2 is for the local VS development
            if (!path.Contains(ahlpath2) && Directory.Exists(ahlpath2))
            {
                path += ";" + ahlpath2;
                Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
                log.Info("location " + ahlpath2 + " added to PATH");
            }
            log.Info("PATH=" + Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process));

            const string DEFAULT_lockPdfPassword_APPSETTING_NAME = "PDFGEN_DEFAULT_lockPdfPassword";
            const string SIGN_PDF_REASON_APPSETTING_NAME = "PDFGEN_SIGN_PDF_REASON";
            const string SIGN_PDF_LOCATION_APPSETTING_NAME = "PDFGEN_SIGN_PDF_LOCATION";
            const string SIGN_PDF_CONTACT_APPSETTING_NAME = "PDFGEN_SIGN_PDF_CONTACT";
            const string SIGN_PDF_CERTIFICATE_HASH_ALGORITHM_APPSETTING_NAME = "PDFGEN_SIGN_PDF_CERTIFICATE_HASH_ALGORITHM";

            JObject response_body = new JObject();

            String pdfbase64 = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            String statusMessage = null;
            JObject pdfinfo = null;

            try
            {

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
                string xslpre = null;
                try
                {
                    xslpre = json.xslPre;
                }
                catch (Exception ex) { }
                string pdftemplate = null;
                try
                {
                    pdftemplate = json.pdfTemplate;
                }
                catch (Exception ex) { }

                bool signPdf = false;
                try
                {
                    signPdf = json.signPdf;
                }
                catch (Exception ex) { }
                bool lockPdfWithPassword = false;
                try
                {
                    lockPdfWithPassword = json.lockPdfWithPassword;
                }
                catch (Exception ex) { }
                string lockPdfPassword = null;
                try
                {
                    lockPdfPassword = json.lockPdfPassword;
                }
                catch (Exception ex) { }
                string lockPdfPassword_AppSettingName = null;
                try
                {
                    lockPdfPassword_AppSettingName = json.lockPdfPassword_AppSettingName;
                }
                catch (Exception ex)
                {
                }

                pdfinfo = new JObject{
                    {"isSigned", signPdf},
                    {"isLockedWithPassword", lockPdfWithPassword},
                    {"isFromTemplate", pdftemplate!=null}
                };

                byte[] byteArray = null;
                IDictionary<string, string> data_dic = null;

                if (pdftemplate != null)
                {
                    using (WebClient wclient = new WebClient())
                    {
                        byteArray = wclient.DownloadData(pdftemplate);
                    }
                    data_dic = JsonConvert.DeserializeObject<IDictionary<string, string>>(data);
                }
                else
                {
                    string xml = JsonConvert.DeserializeXmlNode(data, "data").OuterXml;
                    // converting Plain XML into other XML format by using XSL pre transformation if exists
                    if (xslpre != null)
                    {
                        MemoryStream os = doXSLT20(xml, xslpre);
                        byte[] ba = os.ToArray();
                        xml = Encoding.UTF8.GetString(ba, 0, ba.Length);
                    }
                    byteArray = doPDFGen(xml, xsl, log);
                }
                // Read stream into byte array.
                string lockpwd = null;
                if (signPdf || lockPdfWithPassword || pdftemplate != null)
                {
                    MemoryStream ss = new MemoryStream();
                    if (lockPdfWithPassword)
                    {
                        if (lockPdfPassword != null)
                        {
                            lockpwd = lockPdfPassword;
                        }
                        else
                        {
                            if (lockPdfPassword_AppSettingName != null)
                            {
                                lockpwd = System.Environment.GetEnvironmentVariable(lockPdfPassword_AppSettingName);
                            }
                            else
                            {
                                lockpwd = System.Environment.GetEnvironmentVariable(DEFAULT_lockPdfPassword_APPSETTING_NAME);
                            }
                        }
                        if (lockpwd == null)
                        {
                            throw new Exception("PDF will not be generated because user didn't provide its own password and " + (lockPdfPassword_AppSettingName != null ? "" : "default ") + "Application Setting with the name " + (lockPdfPassword_AppSettingName != null ? lockPdfPassword_AppSettingName : DEFAULT_lockPdfPassword_APPSETTING_NAME) + " is not found!");
                        }
                    }
                    string signReason = null;
                    string signLocation = null;
                    string signContact = null;
                    X509Certificate2 cert = null;
                    string hashAlgorithm = null;
                    if (signPdf)
                    {
                        signReason = System.Environment.GetEnvironmentVariable(SIGN_PDF_REASON_APPSETTING_NAME);
                        signLocation = System.Environment.GetEnvironmentVariable(SIGN_PDF_LOCATION_APPSETTING_NAME);
                        signContact = System.Environment.GetEnvironmentVariable(SIGN_PDF_CONTACT_APPSETTING_NAME);
                        string certificate = System.Environment.GetEnvironmentVariable("WEBSITE_LOAD_CERTIFICATES");
                        cert = GetCertificate(certificate);
                        hashAlgorithm = System.Environment.GetEnvironmentVariable(SIGN_PDF_CERTIFICATE_HASH_ALGORITHM_APPSETTING_NAME);
                        if (hashAlgorithm == null)
                        {
                            hashAlgorithm = "SHA-1";
                        }
                    }
                    DigiSignPdf(byteArray, ss, data_dic, cert, hashAlgorithm, signReason, signLocation, signContact, signPdf, lockPdfWithPassword ? lockpwd : null, false);
                    byteArray = ss.ToArray();
                }

                pdfbase64 = Convert.ToBase64String(byteArray);
                statusCode = HttpStatusCode.OK;
                statusMessage = "PDF successfully created.";
                log.Info(statusMessage);
            }
            catch (Exception e)
            {
                pdfbase64 = null;
                pdfinfo = new JObject{
                    {"isSigned", false},
                    {"isLockedWithPassword", false},
                    {"isFromTemplate", false}
                };

                statusCode = HttpStatusCode.InternalServerError;
                statusMessage = "Failed to create PDF! Error message: " + e.Message + ", st=" + e.StackTrace;
                log.Info(statusMessage);
            }
            response_body.Add("pdf", pdfbase64);
            response_body.Add("pdfinfo", pdfinfo);
            response_body.Add("statusCode", (int)statusCode);
            response_body.Add("statusMessage", statusMessage);

            return req.CreateResponse(statusCode, response_body);
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
            X509Certificate2 certificate,
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
                    stamper.FormFlattening = true;
                }
            }

            if (signPdf)
            {
                Org.BouncyCastle.X509.X509Certificate bccert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(certificate);
                IExternalSignature es = new X509Certificate2Signature(certificate, digestAlgorithm);

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

                MakeSignature.SignDetached(appearance, es, new Org.BouncyCastle.X509.X509Certificate[] { bccert }, null, null, null, 0, CryptoStandard.CMS);
            }
            stamper.Close();
            reader.Close();
            reader.Dispose();
        }

        static X509Certificate2 GetCertificate(string thumbprint)
        {

            if (string.IsNullOrEmpty(thumbprint))
                throw new ArgumentNullException("thumbprint", "Argument 'thumbprint' cannot be 'null' or 'string.empty'");

            X509Certificate2 retVal = null;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certCollection.Count > 0)
            {
                retVal = certCollection[0];
            }

            certStore.Close();

            return retVal;
        }
    }
}
