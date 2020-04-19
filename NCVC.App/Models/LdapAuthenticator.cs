using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NCVC.App.Models
{

    public class LdapAuthenticator
    {
        private string host;
        private int port;
        private string basestr;
        private string id;
        private Func<LdapEntry, string> getName;

        public LdapAuthenticator(string host, int port, string basestr, string id, Func<LdapEntry, string> getName)
        {
            this.host = host;
            this.port = port;
            this.basestr = basestr;
            this.id = id;
            this.getName = getName;
        }

        public string FindName(string account, string search_user_account, string search_user_password)
        {
            var lc = new LdapConnection();
            lc.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, sslPolicyErrors) => true;  // Ignore cert. error
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                lc.SecureSocketLayer = true;
                lc.Connect(this.host, this.port);
                var dn = string.Format("{0}={1},{2}", this.id, account, this.basestr);
                lc.Bind(LdapConnection.Ldap_V3, string.Format("{0}={1},{2}", this.id, search_user_account, this.basestr), search_user_password);

                return getName(lc.Read(dn));
            }
            catch
            {
                return null;
            }
        }


        public (bool, string) Authenticate(string account, string password)
        {
            var lc = new LdapConnection();
            lc.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, sslPolicyErrors) => true;  // Ignore cert. error
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                lc.SecureSocketLayer = true;
                lc.Connect(this.host, this.port);
                var dn = string.Format("{0}={1},{2}", this.id, account, this.basestr);
                lc.Bind(LdapConnection.Ldap_V3, dn, password);
                return (true, getName(lc.Read(dn)));
            }
            catch (Exception)
            {
                return (false, null);
            }
            finally
            {
                lc.Disconnect();
            }
        }
    }
}
