namespace FindTextByUrl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HtmlAgilityPack;

    public class SearchDirectory : List<SearchFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchDirectory" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public SearchDirectory()
        {
        }

        #region Properties

        /// <summary>
        /// Gets the URL path.
        /// </summary>
        /// <value>
        /// The URL path.
        /// </value>
        public SearchRequest Request { get; private set; }
        
        #endregion Properties

        #region Public

        /// <summary>
        /// Searches this instance.
        /// </summary>
        /// <returns></returns>
        public void Search(SearchRequest request)
        {
            this.Request = request;
            
            if (String.IsNullOrEmpty(this.Request.SearchKeyWord))
            {
                this.Request.Log("Search text is empty");
                return;
            }

            if (String.IsNullOrEmpty(this.Request.ExtensionList))
            {
                this.Add(new SearchFile(this.Request, String.Empty));
            }
            else
            {
                this.GetAllSearchFiles();
            }

            this.Request.Stat = new SearchStat(this);

            foreach (SearchFile searchFile in this)
            {
                if (!this.Request.IsCancelled())
                {
                    searchFile.Search();
                }
            }
        }
        
        #endregion Public

        #region Private

        /// <summary>
        /// Gets all search files.
        /// </summary>
        private void GetAllSearchFiles()
        {
            try
            {
                HtmlDocument doc = this.Request.LoadDocument(this.Request.UrlPath);

                if (doc == null || doc.DocumentNode == null)
                {
                    throw new Exception("Document couldn't be loaded.");
                }

                HtmlNodeCollection hrefs = doc.DocumentNode.SelectNodes("//a[@href]");

                if (hrefs == null)
                {
                    throw new Exception("No urls inside document.");
                }

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];

                    if (att != null && this.Request.Extensions.Any(a => att.Value.EndsWith("." + a)))
                    {
                        SearchFile file = this.FirstOrDefault(f => f.LocalPath == att.Value);

                        if(file == null)
                        {
                            file = new SearchFile(this.Request, att.Value);
                            file.Log("Found new file:{0}", att.Value);
                            this.Add(file);
                        }
                        else
                        {
                            file.ResetStat();
                            file.Log("Use existing file:{0}", att.Value);
                        }

                        // Change search word on new for old files.
                        file.Request.SearchKeyWord = this.Request.SearchKeyWord;
                    }
                }
            }
            catch (Exception e)
            {
                this.Request.Log(e.Message);
            }
        }

        #endregion Private
    }
}
