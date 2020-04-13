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
        private Func<LdapEntry, (string, string)> getEmailAndName;

        public LdapAuthenticator(string host, int port, string basestr, string id, Func<LdapEntry, (string, string)> getEmailAndName)
        {
            this.host = host;
            this.port = port;
            this.basestr = basestr;
            this.id = id;
            this.getEmailAndName = getEmailAndName;
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

                var (name, email) = getEmailAndName(lc.Read(dn));
                return name;
            }
            catch
            {
                return null;
            }
        }


        public (bool, string, string) Authenticate(string account, string password)
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
                var (email, name) = getEmailAndName(lc.Read(dn));
                return (true, email, name);
            }
            catch (Exception e)
            {
                /*
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                Exception ex = e.InnerException;
                while (ex != null)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                }
                */
                return (false, null, null);
            }
            finally
            {
                lc.Disconnect();
            }
        }
    }
}
