using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aras.IOM;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace AMLloader
{
    public partial class Main : Form
    {
        Innovator inn;
        HttpServerConnection conn;
        public bool isConnected;

        public Main()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connecting to the Aras Instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string url = tbUrl.Text;
            string database = tbDb.Text;
            string login = tbLogin.Text;
            string pass = tbPass.Text;
            
            try
            {
                conn = MyIConnection(url, database, login, pass);
            }
            catch (Exception)
            {
                MessageBox.Show("Connection Failed");
            }

        }

        /// <summary>
        /// Returns an HttpServerConnection for the selected instance
        /// </summary>
        /// <param name="url"></param>
        /// <param name="Db"></param>
        /// <param name="InnUser"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        private HttpServerConnection MyIConnection(string url, string Db, string InnUser, string Password)
        {
            HttpServerConnection connection = IomFactory.CreateHttpServerConnection(url, Db, InnUser, Password);

            Item Login = connection.Login();

            if (!Login.isError())
            {
                isConnected = true;
                inn = Login.getInnovator();
                btnExecute.Enabled = true;
            }
            else
            {
                btnExecute.Enabled = false;
            }
            return connection;
        }


        private void tbAMLFile_Enter(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text Files (.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Open the selected file to read.
                tbAMLFile.Text = openFileDialog1.FileName;
            }
        }

        public class logentry
        {
            public DateTime logtime { get; set; }
            public string logmessage { get; set; }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() => executeAMLLoad(uiScheduler));
        }

        public void executeAMLLoad(TaskScheduler uiScheduler)
        {
            // read the XML file
            XDocument doc = new XDocument();
            XDocument log = new XDocument();
            log.Add(new XElement("AML"));
            doc = XDocument.Load(tbAMLFile.Text);
            int defaultBatchSize =  int.Parse(tbBatchSize.Text);
            int batchSize = defaultBatchSize;
            // process each item per batch
            var fullNodeList = doc.Root.Elements("Item");

            // update batch process progress indicator
            Task.Factory.StartNew(() =>
            {
                toolStripProgressBar1.Maximum = fullNodeList.Count();
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel2.Text = "Batch Process...";
                toolStripStatusLabel1.Text = 0 + "/" + fullNodeList.Count();
            }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
         
            int errorcounter = 0;
        
            int index = 0;
            while (index< fullNodeList.Count()+1)
            {
                var nodes = fullNodeList.Select(x => x).Skip(index).Take(batchSize);
                Item result = inn.applyAML(string.Format("<AML>{0}</AML>", string.Concat(nodes)));
                if (result.isError())
                {
                    if (nodes.Count() > 1)
                    {
                        batchSize = (int)(batchSize / 2);
                    }
                    else
                    {
                        log.Root.Add(new XComment(result.getErrorString()));
                        log.Root.Add(nodes);
                        errorcounter++;

                        // update batch process errors indicator
                        Task.Factory.StartNew(() =>
                        {
                            toolStripStatusLabel3.Text = "("+ errorcounter + " Errors)";
                        }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
                        index++;
                    }
                }
                else
                {
                    index = index + nodes.Count();
                    batchSize = batchSize+ (int)((defaultBatchSize - batchSize)/2);
                }

                // update batch process progress indicator
                Task.Factory.StartNew(() =>
                {
                    toolStripProgressBar1.Value = index;
                    toolStripStatusLabel1.Text = index+ "/" + fullNodeList.Count();
                    toolStripStatusLabel2.Text = "Batch size :"+ batchSize;
                }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            }
            MessageBox.Show(errorcounter + " ERRORS");

            // saves log file
            log.Save("ERRORS.xml");

            // opens log file
            StartProcess("ERRORS.xml");
        }

        private void StartProcess(string path)
        {
            ProcessStartInfo StartInformation = new ProcessStartInfo();
            StartInformation.FileName = path;
            Process process = Process.Start(StartInformation);
            process.EnableRaisingEvents = true;
        }

        private void tbUrl_Leave(object sender, EventArgs e)
        {
            HttpServerConnection connection = IomFactory.CreateHttpServerConnection(tbUrl.Text);
            var dbs = connection.GetDatabases();
            tbDb.Items.Clear();
            foreach (var db in dbs)
            {
                tbDb.Items.Add(db);
            }

        }


        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            conn.Logout();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            tbUrl.Focus();
        }
    }
}
