using System;
using System.Web;
using System.Configuration;
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
        private string _serverName;

        public UseCanonicalNameModule()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public String ModuleName
        {
            get { return "UseCanonicalNameModule"; }
        }

        public void Init(HttpApplication r_objApplication)
        {
            _serverName = String.Empty;
            _useCanonicalName = false;

            LogMessageToFile("Init: reading configuration");
            // process configuration via web.config
            // TODO: OK to just use /web.config, or need to set as parameter?
            System.Configuration.Configuration rootWebConfig1 =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/WebSite1/web.config");

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
                    _serverName = serverNameConfig.Value;
                    LogMessageToFile("Config found, server name is " + _serverName);
                }
                else
                {
                    LogMessageToFile("No valid config found for UseCanonicalName");

                }
            }
            else
            {
                LogMessageToFile("Empty config? rootWebConfig1.AppSettings.Settings.Count <= 0");
            }

            // Register our event handler with Application object.
            r_objApplication.PreSendRequestHeaders += new EventHandler(this.PreSendRequestHeaders);

        }

        public void Dispose()
        {
            // Left blank because we dont have to do anything.
        }


        
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
            "{0:G}: {1}.", System.DateTime.Now, msg);
        sw.WriteLine(logLine);
    }
    finally
    {
        sw.Close();
    }
}
        private void PreSendRequestHeaders(object r_objSender, EventArgs r_objEventArgs)
        {

            LogMessageToFile("Beginning request handling");
            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = (HttpApplication)r_objSender;
            HttpContext context = application.Context;

            // First test: append header
            LogMessageToFile("Request processed; useCanonicalName is " + Convert.ToString(_useCanonicalName));
            LogMessageToFile("Server name is " + _serverName);
            context.Response.AppendHeader("X-UseCanonicalName", Convert.ToString(_useCanonicalName));
            context.Response.AppendHeader("X-ServerName", _serverName);

        }

    }
}