using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace FindTextByUrl
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchRequest
    {
        #region Fields

        private const String _tempDirectoryName = "TempFiles";
        public delegate void ProgressUpdate(int value, String currentMessage);
        public event ProgressUpdate OnProgressUpdate;
        public delegate void StatusUpdate(object sender, ProgressChangedEventArgs e);
        public event StatusUpdate OnStatusUpdate;
        public Func<Boolean> IsCancelled;

        #endregion Fields
        
        #region Constructors

        public SearchRequest(String urlPath, String extensionList, String keyWord, String login, String password, Boolean isBasicAuth, Int32? takeFirst, Int32? takeLast)
        {
            this.UrlPath = urlPath;
            this.ExtensionList = extensionList;
            this.SearchKeyWord = keyWord;
            this.Login = login;
            this.Password = password;
            this.IsBasicAuth = isBasicAuth;
            this.TakeFirst = takeFirst;
            this.TakeLast = takeLast;
            
            if (!String.IsNullOrEmpty(extensionList))
            {
                this.Extensions = extensionList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!String.IsNullOrEmpty(this.Login))
            {
                if (this.IsBasicAuth)
                {
                    byte[] credentialBuffer = new UTF8Encoding().GetBytes(this.Login + ":" + this.Password);
                    this.BasicAuthString = "Basic " + Convert.ToBase64String(credentialBuffer);
                }
                else
                {
                    this.Credential = new NetworkCredential(this.Login, this.Password);

                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the stat.
        /// </summary>
        /// <value>
        /// The stat.
        /// </value>
        public SearchStat Stat { get; set; }

        /// <summary>
        /// Gets the URL path.
        /// </summary>
        /// <value>
        /// The URL path.
        /// </value>
        public String UrlPath { get; private set; }

        /// <summary>
        /// Gets the extension list.
        /// </summary>
        /// <value>
        /// The extension list.
        /// </value>
        public String ExtensionList { get; private set; }

        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public List<String> Extensions { get; private set; }

        /// <summary>
        /// Gets or sets the search key word.
        /// </summary>
        /// <value>
        /// The search key word.
        /// </value>
        public String SearchKeyWord { get; set; }

        /// <summary>
        /// Gets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        public String Login { get; private set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public String Password { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is basic authentication.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is basic authentication; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsBasicAuth { get; private set; }

        /// <summary>
        /// Gets the basic authentication string.
        /// </summary>
        /// <value>
        /// The basic authentication string.
        /// </value>
        public String BasicAuthString { get; private set; }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public NetworkCredential Credential { get; private set; }

        /// <summary>
        /// Gets the take first.
        /// </summary>
        /// <value>
        /// The take first.
        /// </value>
        public Int32? TakeFirst { get; private set; }

        /// <summary>
        /// Gets the take last.
        /// </summary>
        /// <value>
        /// The take last.
        /// </value>
        public Int32? TakeLast { get; private set; }

        /// <summary>
        /// Gets the temporary directory.
        /// </summary>
        /// <value>
        /// The temporary directory.
        /// </value>
        public String TempDirectory
        {
            get
            {
                String path = Path.Combine(Application.LocalUserAppDataPath, _tempDirectoryName, this.UrlPath.GetHashCode().ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
        
        #endregion Properties

        #region Public

        /// <summary>
        /// Cleans the cache.
        /// </summary>
        public static String CleanCache()
        {
            try
            {
                Directory.Delete(Path.Combine(Application.LocalUserAppDataPath, _tempDirectoryName), true);
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }

            return String.Empty;
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Log(String message, params Object[] args)
        {
            // Fire the event
            if (this.OnProgressUpdate!= null)
            {
                try
                {
                    this.OnProgressUpdate(1, String.Format(message, args) + Environment.NewLine);
                }
                catch
                {
                    this.OnProgressUpdate(1, message + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Updates the status.
        /// </summary>
        public void UpdateStatus()
        {
            if (this.OnStatusUpdate != null && this.Stat != null)
            {
                this.OnStatusUpdate(this, new ProgressChangedEventArgs(this.Stat.PercentFinished, this.Stat));
            }
        }

        /// <summary>
        /// Loads the document.
        /// </summary>
        /// <returns></returns>
        public HtmlDocument LoadDocument(String url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HtmlWeb hw = new HtmlWeb();
            hw.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.137 Safari/537.36";

            if (!String.IsNullOrEmpty(this.BasicAuthString))
            {
                hw.PreRequest += (request) =>
                {
                    request.PreAuthenticate = true;
                    request.Headers["Authorization"] = this.BasicAuthString;
                    return true;
                };
            }

            if (this.Credential != null)
            {
                hw.PreRequest += (request) =>
                {
                    request.Credentials = this.Credential;
                    request.UseDefaultCredentials = false;
                    return true;
                };
            }

            return hw.Load(url);
        }

        #endregion Public

        #region Private

        #endregion Private
    }
}
