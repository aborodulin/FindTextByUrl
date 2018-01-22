using System.IO;

namespace FindTextByUrl
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        SearchDirectory _searchDirectory = new SearchDirectory();

        public MainForm()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

            toolStripStatusLabel1.Text = "Version: " + Application.ProductVersion;

            this.urlTextBox.Text = ConfigRequest.Default.Url;
            this.extensionTextBox.Text = ConfigRequest.Default.Extensions;
            this.loginTextBox.Text = ConfigRequest.Default.Login;
            this.passwordTextBox.Text = ConfigRequest.Default.Password;
            this.searchTextBox.Text = ConfigRequest.Default.SearchKeyword;
            this.isBasicAuthcheckBox.Checked = ConfigRequest.Default.IsBasicAuth;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                SearchRequest searchRequest = new SearchRequest(urlTextBox.Text, extensionTextBox.Text, searchTextBox.Text, loginTextBox.Text, passwordTextBox.Text, isBasicAuthcheckBox.Checked);

                searchRequest.OnProgressUpdate += OnProgressUpdate;
                searchRequest.OnStatusUpdate += this.backgroundWorker1_ProgressChanged;

                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync(searchRequest);

                logTextBox.Text = String.Empty;
                searchButton.Text = "Stop";
            }
            else
            {
                backgroundWorker1.CancelAsync();
                searchButton.Text = "Cancelling";
                searchButton.Enabled = false;
            }
        }

        // This event handler is where the time-consuming work is done.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending)
            {
                toolStripStatusLabel1.Text = "Cancelling...";
                e.Cancel = true;
                
            }
            else
            {
                toolStripStatusLabel1.Text = "Searching...";
                SearchRequest request = e.Argument as SearchRequest;
                request.IsCancelled = () => worker.CancellationPending;

                if (this._searchDirectory.Request != null && this._searchDirectory.Request.UrlPath != request.UrlPath)
                {
                    this._searchDirectory.Clear();
                }

                this._searchDirectory.Search(request);
            }
        }

        /// <summary>
        /// Called when [progress update].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="message">The message.</param>
        private void OnProgressUpdate(int value, String message)
        {
            // Its another thread so invoke back to UI thread
            base.Invoke((Action)delegate
            {
                //this.logTextBox.Text += message;
                this.logTextBox.AppendText(message);
            });
        }
        
        // This event handler updates the progress.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SearchStat stat = e.UserState as SearchStat;
            if (stat != null)
            {
                toolStripStatusLabel1.Text = $"Found: {stat.AllFound} in {stat.FoundFiles}/{stat.AllFiles} ({e.ProgressPercentage}%)";
            }
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchButton.Enabled = true;
            searchButton.Text = "Search";

            //SoapResponse response = e.Result as SoapResponse;

            if (e.Cancelled == true)
            {
                toolStripStatusLabel1.Text = "Canceled! " + toolStripStatusLabel1.Text;
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "Error: " + e.Error.Message;
                logTextBox.AppendText(e.Error.ToString());

            }
            else
            {
                toolStripStatusLabel1.Text = "Done! " + toolStripStatusLabel1.Text;
                if (e.Result != null)
                {
                    logTextBox.AppendText(e.Result.ToString());
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the cleanCacheButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cleanCacheButton_Click(object sender, EventArgs e)
        {
            logTextBox.AppendText("Cleaning cache...");
            String error = SearchRequest.CleanCache();

            if (String.IsNullOrEmpty(error))
            {
                logTextBox.AppendText("Cleaned!");
            }
            else
            {
                logTextBox.AppendText(error);    
            }
            
        }

        /// <summary>
        /// Handles the FormClosing event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConfigRequest.Default.Url = this.urlTextBox.Text;
            ConfigRequest.Default.Extensions = this.extensionTextBox.Text;
            ConfigRequest.Default.Login = this.loginTextBox.Text;
            ConfigRequest.Default.Password = this.passwordTextBox.Text;
            ConfigRequest.Default.SearchKeyword = this.searchTextBox.Text;
            ConfigRequest.Default.IsBasicAuth = this.isBasicAuthcheckBox.Checked;
            ConfigRequest.Default.Save();
        }
    }
}
