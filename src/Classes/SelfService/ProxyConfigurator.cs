using AJ_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    //static class ProxyConfigurator
    //{
    //    static public bool UseProxy { get { return Settings.Default.ProxyEnabled; } }

    //    static public IWebProxy GetWebProxy
    //    {
    //        get
    //        {
    //            if (!UseProxy)
    //            {
    //                return null;
    //            }
    //            else
    //            {
    //                var proxyHost = Settings.Default.ProxyHostName;
    //                var proxyPort = Settings.Default.ProxyPort;
    //                var proxyUserName = Settings.Default.ProxyUserName;
    //                var proxyPassword = Settings.Default.ProxyPassword;
    //                var proxyBypassOnLocal = Settings.Default.ProxyBypassOnLocal;
    //                var proxyUseDefaultCredentials = Settings.Default.ProxyUseDefaultCredentials;

    //                var proxy = new WebProxy
    //                {
    //                    Address = new Uri($"http://{proxyHost}:{proxyPort}"),
    //                    BypassProxyOnLocal = proxyBypassOnLocal,
    //                    UseDefaultCredentials = proxyUseDefaultCredentials,

    //                    // *** These creds are given to the proxy server, not the web server ***
    //                    Credentials = new NetworkCredential(
    //                                        userName: proxyUserName,
    //                                        password: proxyPassword)
    //                };

    //                return proxy;
    //            }
    //        }
    //    }
    //}
}
