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
               // searchRequest.Cancel();

                //searchRequest.OnCancel 
                   // backgroundWorker1.
               // backgroundWorker1.ProgressChanged += ;

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
            //this.logTextBox = (sender as SearchDirectory).Results;

            toolStripStatusLabel1.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchButton.Enabled = true;
            searchButton.Text = "Search";

            //SoapResponse response = e.Result as SoapResponse;

            if (e.Cancelled == true)
            {
                toolStripStatusLabel1.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "Error: " + e.Error.Message;
                logTextBox.AppendText(e.Error.ToString());

            }
            else
            {
                toolStripStatusLabel1.Text = "Done!";
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
    }
}
