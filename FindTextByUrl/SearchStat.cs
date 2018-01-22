using System;
using System.Collections.Generic;
using System.Linq;

namespace FindTextByUrl
{
    public class SearchStat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchStat"/> class.
        /// </summary>
        /// <param name="files">The files.</param>
        public SearchStat(List<SearchFile> files)
        {
            this.Files = files;
        }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public List<SearchFile> Files { get; private set; }

        /// <summary>
        /// Gets or sets all found.
        /// </summary>
        /// <value>
        /// All found.
        /// </value>
        public Int32 AllFound
        {
            get
            {
                Int32 count = 0;
                Files.ForEach(f => count += f.FoundCount);

                return count;
            }
        }

        /// <summary>
        /// Gets or sets the found files.
        /// </summary>
        /// <value>
        /// The found files.
        /// </value>
        public Int32 FoundFiles
        {
            get
            {
                return Files.Count(f => f.FoundCount > 0);
            }
        }

        /// <summary>
        /// Gets the finished files.
        /// </summary>
        /// <value>
        /// The finished files.
        /// </value>
        public Int32 FinishedFiles
        {
            get
            {
                return Files.Count(f => f.IsFinished);
            }
        }

        /// <summary>
        /// Gets or sets all files.
        /// </summary>
        /// <value>
        /// All files.
        /// </value>
        public Int32 AllFiles => Files.Count;

        /// <summary>
        /// Gets the percent finished.
        /// </summary>
        /// <value>
        /// The percent finished.
        /// </value>
        public Int32 PercentFinished
        {
            get
            {
                return this.AllFiles == 0 ? 100 : 100 * this.FinishedFiles / this.AllFiles;
            }
        }

    }
}
