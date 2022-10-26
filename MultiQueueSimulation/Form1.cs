using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueModels;
using MultiQueueTesting;

namespace MultiQueueSimulation
{
    public partial class Form1 : Form
    {
        SimulationSystem simulationSystem = new SimulationSystem();
        bool isDataComplete = false;
        int serversCount = 0;
        //to open files
        OpenFileDialog fileDialog = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedItem = comboBox1.Items[0];
            comboBox2.SelectedItem = comboBox2.Items[0];
        }
        private void btnLoadFiles_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                bool textIsComplete = true;
                //read files


                //"NumberOfServers"
                //"StoppingNumber"
                //"StoppingCriteria"
                //"SelectionMethod"
                //"InterarrivalDistribution"
                //"1, 0.25"
                //"ServiceDistribution_Server1"
                //"5, 0.17"

                using (StreamReader reader = new StreamReader(fileDialog.FileName))
                {
                    string x = File.ReadAllText(fileDialog.FileName);
                    string line;
                    while ((line = reader.ReadLine()) != null && textIsComplete)
                    {
                        //check for number of servers
                        if (line.Equals("NumberOfServers"))
                        {
                            line = reader.ReadLine();
                            serversCount = checkInteger(line, 100, 1);
                            txtbx_NoServers.Text = serversCount.ToString();
                            if (serversCount <= 0) textIsComplete = false;

                        }
                        else if (line.Equals("StoppingNumber"))
                        {
                            line = reader.ReadLine();
                            int result = checkInteger(line, 1000, 1);
                            if (result > 0)
                                textBox1.Text = result.ToString();
                            else
                                textIsComplete = false;
                        }
                        else if (line.Equals("StoppingCriteria"))
                        {
                            line = reader.ReadLine();
                            int result = checkInteger(line, 2, 1);
                            if (result > 0)
                                comboBox1.SelectedIndex = result - 1;
                            else
                                textIsComplete = false;
                        }
                        else if (line.Equals("SelectionMethod"))
                        {
                            line = reader.ReadLine();
                            int result = checkInteger(line, 3, 1);
                            if (result > 0)
                                comboBox2.SelectedIndex = result - 1;
                            else
                                textIsComplete = false;
                        }
                        else if (line.Equals("InterarrivalDistribution"))
                        {
                            dataGridView2.Rows.Clear();
                            while (true)
                            {
                                line = reader.ReadLine();
                                if (line == "") break;
                                string[] columns = line.Split(',');
                                dataGridView2.Rows.Add(int.Parse(columns[0]), (int)(decimal.Parse(columns[1].Trim()) * 100));
                            }
                        }
                        else if (line.Equals("ServiceDistribution_Server1"))
                        {
                            //read servers ServiceDistribution
                            btnAddServers_Click(null, null);
                            int serverIndex = 1;
                            while (serverIndex <= serversCount)
                            {
                                DataGridView dgv = serversTabControl.TabPages[serverIndex - 1].Controls.OfType<DataGridView>().FirstOrDefault();

                                dgv.Rows.Clear();
                                //test
                                while (true)
                                {
                                    line = reader.ReadLine();
                                    if (line == null) break;
                                    if (line == "")
                                    {
                                        line = reader.ReadLine();
                                        break;
                                    }
                                    string[] columns = line.Split(',');
                                    DataGridViewRow row = (DataGridViewRow)dgv.Rows[0].Clone();

                                    row.Cells[0].Value = (int.Parse(columns[0]));
                                    row.Cells[1].Value = (int)(decimal.Parse(columns[1].Trim()) * 100);
                                    dgv.Rows.Add(row);

                                }
                                serverIndex++;
                            }
                        }
                    }
                }
            }
        }

        private int checkInteger(string input, int max, int min)
        {
            int result;
            if (Int32.TryParse(input, out result))
            {
                if (result > max && result < min) { return 0; }
                else return result;
            }
            else { return 0; }
        }
        private void btnAddServers_Click(object sender, EventArgs e)
        {
            //check data valid
            if (!int.TryParse(txtbx_NoServers.Text.ToString(), out serversCount))
            {
                isDataComplete = false;
                serversCount = 0;
                return;
            }

            if (serversCount > 0)
            {
                serversTabControl.TabPages.Clear();
                for (int i = 0; i < serversCount; i++)
                {
                    string title = "Server " + (i + 1).ToString();
                    TabPage newTab = new TabPage(title);
                    DataGridView newServerdg = new DataGridView();
                    newServerdg.Name = title;
                    newServerdg.Dock = DockStyle.Fill;
                    newServerdg.ColumnCount = 2;
                    newServerdg.Columns[0].Name = "Service Time";
                    newServerdg.Columns[1].Name = "Probability";
                    newTab.Controls.Add(newServerdg);

                    serversTabControl.TabPages.Add(newTab);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            valiadateAndRun();
        }
        private void valiadateAndRun()
        {
            //check servers count
            if (serversCount <= 0) { return; }

            isDataComplete = false;
            int StoppingNumber;
            List<Server> serversList = new List<Server>();
            List<TimeDistribution> InterarrivalDistribution = new List<TimeDistribution>();
            Enums.StoppingCriteria StoppingCriteria;
            Enums.SelectionMethod SelectionMethod;

            //check servers data
            for (int server = 0; server < serversCount; server++)
            {
                //create entry for current server
                Server currServer = new Server();
                currServer.ID = server + 1;
                //get the datagrid view for the current tab for the server
                DataGridView dgv = serversTabControl.TabPages[server].Controls.OfType<DataGridView>().FirstOrDefault();
                try
                {
                    //check distributions
                    for (int row = 0; row < dgv.RowCount - 1; row++)
                    {
                        TimeDistribution distribution = new TimeDistribution();
                        //service time
                        distribution.Time = (int)dgv.Rows[row].Cells[0].Value;
                        //prop
                        distribution.Probability = (decimal)((int)dgv.Rows[row].Cells[1].Value / 100);
                        currServer.TimeDistribution.Add(distribution);
                    }
                    //check if summ of prop is 100
                    if (currServer.TimeDistribution.Sum(x => Convert.ToInt32(x.Probability)) * 100 == 100)
                        serversList.Add(currServer);
                    else return;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            //finished servers
            //start
            try
            {
                //check distributions
                for (int row = 0; row < dataGridView2.RowCount - 1; row++)
                {
                    TimeDistribution distribution = new TimeDistribution();
                    //Interarrival  Time
                    distribution.Time = (int)dataGridView2.Rows[row].Cells[0].Value;
                    //prop
                    distribution.Probability = (decimal)dataGridView2.Rows[row].Cells[1].Value / 100;
                    InterarrivalDistribution.Add(distribution);
                }
                //check if summ of prop is 100
                if (InterarrivalDistribution.Sum(x => Convert.ToInt32(x.Probability)) * 100 != 100)
                    return;
            }
            catch (Exception)
            {
                throw;
            }

            //stopping number
            if (!Int32.TryParse(textBox1.Text, out StoppingNumber)) return;
            //criteria
            StoppingCriteria = (Enums.StoppingCriteria)comboBox1.SelectedIndex + 1;
            //SelectionMethod
            SelectionMethod = (Enums.SelectionMethod)comboBox2.SelectedIndex + 1;

            simulationSystem = new SimulationSystem();
            //assign data
            simulationSystem.NumberOfServers = serversCount;
            simulationSystem.Servers = serversList;
            simulationSystem.InterarrivalDistribution = InterarrivalDistribution;
            simulationSystem.StoppingCriteria = StoppingCriteria;
            simulationSystem.SelectionMethod = SelectionMethod;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}
