using System.Linq;

namespace FindTextByUrl
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 
    /// </summary>
    public class SearchFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFile" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="localPath">The local path.</param>
        public SearchFile(SearchRequest request, String localPath)
        {
            this.Request = request;
            this.LocalPath = localPath;
            this.Results = String.Empty;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        public String LocalPath { get; set; }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public SearchRequest Request { get; private set; }

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public String Results { get; private set; }

        /// <summary>
        /// Gets the name of the hash file.
        /// </summary>
        /// <value>
        /// The name of the hash file.
        /// </value>
        public Int32 HashFileName
        {
            get
            {
                return this.FullPath.AbsolutePath.GetHashCode();
            }
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public Uri FullPath
        {
            get
            {
                Uri baseUri = new Uri(this.Request.UrlPath);

                Uri fileUrl = new Uri(baseUri, this.LocalPath);

                return fileUrl;
            }
        }

        /// <summary>
        /// Gets the found count.
        /// </summary>
        /// <value>
        /// The found count.
        /// </value>
        public Int32 FoundCount { get; private set; } = 0;

        /// <summary>
        /// Gets a value indicating whether this instance is finished.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is finished; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsFinished { get; private set; } = false;

        #endregion Properties

        #region Public

        /// <summary>
        /// Searches the specified key word.
        /// </summary>
        public void Search()
        {
            String text;
            String cachedFilePath = Path.Combine(this.Request.TempDirectory, HashFileName.ToString());
            if (File.Exists(cachedFilePath))
            {
                text = File.ReadAllText(cachedFilePath);
                this.Log("Read cached file {0}, hash={1}, size={2}", this.LocalPath, this.HashFileName, text.Length);
            }
            else
            {
                text = ReadByBuffer();
                File.WriteAllText(cachedFilePath, text);
                this.Log("Load file {0}, size={1}", this.LocalPath, text.Length);
            }

            string pattern = this.Request.SearchKeyWord;

            Match m = Regex.Match(text, pattern);

            if (!m.Success)
            {
                this.Log("Matches not found.");
            }

            while (m.Success && !this.Request.IsCancelled())
            {
                this.Log("'{0}' found at position {1}", m.Value, m.Index);
                this.FoundCount += 1;
                this.Request.UpdateStatus();
                Int32 startIndex = m.Index - 50 > 0 ? m.Index - 50 : 0;
                Int32 length = startIndex + m.Value.Length + 100 < text.Length ? m.Value.Length + 100 : text.Length - startIndex;

                this.Log(text.Substring(startIndex, length < 0 ? 0 : length));
                this.Log(String.Empty);
                m = m.NextMatch();
            }

            this.IsFinished = true;
            this.Request.UpdateStatus();
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Log(String message, params Object[] args)
        {
            try
            {
                this.Request.Log(String.Format(message, args) + Environment.NewLine);
            }
            catch
            {
                this.Request.Log(message + Environment.NewLine);
            }
        }

        /// <summary>
        /// Resets the stat.
        /// </summary>
        public void ResetStat()
        {
            this.FoundCount = 0;
            this.IsFinished = false;
        }

        #endregion Public

        #region Private

        /// <summary>
        /// Searches the by buffer.
        /// </summary>
        private String ReadByBuffer()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.FullPath);

            if (!String.IsNullOrEmpty(this.Request.BasicAuthString))
            {
                request.Headers["Authorization"] = this.Request.BasicAuthString;
                request.PreAuthenticate = true;
            }

            if (this.Request.Credential != null)
            {
                request.Credentials = this.Request.Credential;
                request.UseDefaultCredentials = false;
            }

            // execute the request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();

            String tempString = null;
            int count = 0;
            byte[] buf = new byte[1024 * 100];
            StringBuilder sb = new StringBuilder();

            do
            {
                // fill the buffer with data
                count = resStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0 && !this.Request.IsCancelled()); // any more data to read?

            if (this.Request.IsCancelled())
            {
                this.Log("Cancelled");
                return String.Empty;
            }

            return sb.ToString();
        }

        #endregion Private
    }
}
