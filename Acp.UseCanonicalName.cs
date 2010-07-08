using System;
using System.Web;
using System.Configuration;
using System.Collections;
using System.IO;


/// <summary>
/// UseCanonicalNameModule implements the Apache UseCanonicalName and SiteName directives
/// as an IIS Module.
/// </summary>
namespace Acp
{
    public class UseCanonicalNameModule : IHttpModule
    {

        private bool _useCanonicalName;
        private string _canonicalServerName;
        private string _appRoot;

        public static string VERSION = "0.1";

        public UseCanonicalNameModule()
        {
            //
            // Constructor logic goes here
            //
        }

        public String ModuleName
        {
            get { return "UseCanonicalNameModule"; }
        }

        public void Init(HttpApplication r_objApplication)
        {
            _canonicalServerName = String.Empty;
            _useCanonicalName = false;

            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = r_objApplication;
            HttpContext context = application.Context;

            _appRoot = context.Request.ApplicationPath;

            LogMessageToFile("Application path is " + _appRoot);

            GetConfig();

            // Register our event handler with Application object.
            // TODO: See if we can register a handler only if UseCanonicalName is configured
            r_objApplication.PreSendRequestHeaders += new EventHandler(this.PreSendRequestHeaders);

        }

        public void Dispose()
        {
            // Left blank because we dont have to do anything.
        }


        // For LogMessageToFile
        public string GetTempPath()
        {
            string path = System.Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        public void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
                GetTempPath() + "UseCanonialNameDebug.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }


        //
        // private methods
        //

        private void PreSendRequestHeaders(object r_objSender, EventArgs r_objEventArgs)
        {

            LogMessageToFile("Beginning request handling");

            if (!_useCanonicalName)
            {
                LogMessageToFile("Not configured, returning");
                return;
            }

            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = (HttpApplication)r_objSender;
            HttpContext context = application.Context;
            int responseStatus = context.Response.StatusCode;

            bool responseIsRedirect = IsRedirectStatusCode(responseStatus);

            if (!responseIsRedirect)
            {
                LogMessageToFile("Response is not redirect, returning");
                return;
            }

            LogMessageToFile("Response is redirect and UseCanonical name is configured");
            LogMessageToFile("Response headers follow:");

            string requestHost = context.Request.ServerVariables["HTTP_HOST"];
            string locationUrl = GetRedirectUrl(context);

            // If request host matches canonical host there's nothing for us to do
            // TODO: Test if HTTP_HOST will always match host in location header
            if (requestHost == _canonicalServerName)
            {
                LogMessageToFile("Request host matches canonical server name" );
                return;
            }

            string canonicalUrl = ReplaceHostInURL(locationUrl, _canonicalServerName);
            ReplaceLocationUrlInResponseHeaders(canonicalUrl, context);

            LogMessageToFile("Server name is " + _canonicalServerName);
            LogMessageToFile("Status Code is " + responseStatus);
            context.Response.AppendHeader("X-UseCanonicalName", Convert.ToString(_useCanonicalName));
            context.Response.AppendHeader("X-ServerName", _canonicalServerName);

        }

        private void ReplaceLocationUrlInResponseHeaders(string canonicalUrl, HttpContext context)
        {
            try
            {
                context.Response.Headers.Remove("Location");
                context.Response.Headers.Add("Location", canonicalUrl);
                LogMessageToFile("Set Location to " + canonicalUrl);

            }
            catch (PlatformNotSupportedException ex)
            {
                LogMessageToFile("WARNING: PlatformNotSupportedException; ensure running under IIS7/.Net 3.5");
                LogMessageToFile(ex.Message);
                LogMessageToFile("WARNING: Can't replace Location header");
            }
   
        }




        private string GetRedirectUrl(HttpContext context)
        {

            // fake location header for development
            string locationUrl = "http://localhost/current-issue/";
            try
            {
                locationUrl = context.Response.Headers["Location"];
                LogMessageToFile("Location: " + locationUrl);

            }
            catch (PlatformNotSupportedException ex)
            {
                LogMessageToFile("WARNING: PlatformNotSupportedException; ensure running under IIS7/.Net 3.5");
                LogMessageToFile(ex.Message);
                LogMessageToFile("WARNING: Using fake location header");
            }

            return locationUrl;
        }


        private bool IsRedirectStatusCode(int stCode)
        {
            if (stCode == 301 || stCode == 302)
                return true;

            return false;
        }

        private string ReplaceHostInURL(string url, string host)
        {
            UriBuilder newUri = new UriBuilder(url);
            newUri.Host = host;
            if (newUri.Port == 80)
                newUri.Port = -1; // omit port from final url
            return newUri.ToString();
        }

        private void GetConfig()
        {

            LogMessageToFile("Init: reading configuration from " + _appRoot + "/web.config");

            // Assume web.config is in app root. This may be fragile; I don't know enough about
            // all the configuration permutations of IIS. Possible to not have a web.config in an app root?
            System.Configuration.Configuration rootWebConfig1 =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(_appRoot + "/web.config");

            if (rootWebConfig1.AppSettings.Settings.Count > 0)
            {
                System.Configuration.KeyValueConfigurationElement canonicalSettingConfig =
                    rootWebConfig1.AppSettings.Settings["UseCanonicalName"];

                System.Configuration.KeyValueConfigurationElement serverNameConfig =
                    rootWebConfig1.AppSettings.Settings["ServerName"];

                if (serverNameConfig.Value != null
                    && canonicalSettingConfig.Value != null
                    && string.Equals(canonicalSettingConfig.Value, "On", StringComparison.CurrentCultureIgnoreCase))
                {

                    _useCanonicalName = true;
                    _canonicalServerName = serverNameConfig.Value;
                    LogMessageToFile("Config found, server name is " + _canonicalServerName);
                }
                else
                {
                    LogMessageToFile("No valid config found for UseCanonicalName");

                }
            }
            else
            {
                LogMessageToFile("Empty or non-existent web.config: rootWebConfig1.AppSettings.Settings.Count <= 0");
            }

        }

    }
}