using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDRSE_Find_Route
{
    public partial class Form1 : Form
    {
        findroute.star_st start = new findroute.star_st();
        findroute.star_st end = new findroute.star_st();
        findroute fr = new findroute();
        string savefilename = "DEFAULT";
        enum state { route = 0, zen = 1 }
        static state prog = state.route;
        static int percent;
        int maxdev = 20;
        int maxdist = 0;
        int numofjumps = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = comboBox2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (button1.Text)
            {
                case "Find Route":
                    timer1.Start();
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0:
                            start = fr.searchbyname(textBox1.Text.ToLower());
                            break;
                        case 1:
                            start = fr.currentlocation(textBox1.Text.ToLower());
                            break;
                    }
                    if (start.name == null)
                    {
                        MessageBox.Show(textBox1.Text + " could not be found.");
                    }
                    switch (comboBox2.SelectedIndex)
                    {
                        case 0:
                            end = fr.searchbyname(textBox2.Text.ToLower());
                            break;
                        case 1:
                            break;
                    }
                    if (end.name == null)
                    {
                        MessageBox.Show(textBox2.Text + " could not be found.");
                    }
                    if (start.name == null || end.name == null)
                        return;
                    backgroundWorker1.WorkerSupportsCancellation = true;
                    backgroundWorker1.RunWorkerAsync();
                    button1.Text = "Cancel";
                    break;
                case "Cancel":
                    backgroundWorker1.CancelAsync();
                    button1.Text = "Cancelling...";
                    break;
                case "Cancelling...":
                    MessageBox.Show("We are cancelling, please wait", "Please Wait");
                    break;
            }
        }
        private void setProgress(object sender, EventArgs e)
        {
            progressBar1.Step = percent - progressBar1.Step;
            progressBar1.PerformStep();
            if(!backgroundWorker1.IsBusy)
            {
                timer1.Stop();
                button1.Text = "Find Route";
                progressBar1.Step = -100;
                progressBar1.PerformStep();
                if(checkBox1.Checked)
                {
                    Process.Start("notepad.exe", start.name + "-" + end.name + "route.csv");
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<findroute.star_st> list = new List<findroute.star_st>();
            list.Add(start);
            double totaldist = 0;
            StringBuilder bldr = new StringBuilder();
            switch (prog)
            {
                case state.route:
                    findroute.star_st ret = start;
                    while ((ret = fr.findnext(ret, end, maxdev, maxdist)).name != end.name)
                    {
                        percent = Convert.ToInt16(Math.Floor((start.distance(end) - ret.distance(end)) / (start.distance(end))*100));
                        totaldist += list[list.Count - 1].distance(ret);
                        list.Add(ret);
                    }
                    totaldist += list[list.Count - 1].distance(end);
                    list.Add(end);
                    bldr.AppendLine("As the Crow Flys: " + start.distance(end) + " and total: " + totaldist + " | RSE stars eliminated: " + (list.Count - 2));
                    findroute.star_st prev = new findroute.star_st();
                    foreach (findroute.star_st x in list)
                    {
                        double dist = 0;
                        if (prev.name != null)
                            dist = prev.distance(x);
                        bldr.AppendLine(x.name + ", " + x.coord.x.ToString(CultureInfo.InvariantCulture) + ", " + x.coord.y.ToString(CultureInfo.InvariantCulture) + ", " + x.coord.z.ToString(CultureInfo.InvariantCulture) + ", " + dist.ToString(CultureInfo.InvariantCulture));
                        prev = x;
                    }
                    if (savefilename == "DEFAULT")
                        savefilename = start.name + "-" + end.name + "route.csv";
                    try
                    {
                        File.WriteAllText(savefilename, bldr.ToString());
                    }
                    catch (Exception e2)
                    {
                        MessageBox.Show("Unable to save file. Please contact Rapidfirecrh with this error: " + e2.Message);
                    }
                    break;
                case state.zen:
                    findroute.star_st zenret = start;
                    while (list.Count != numofjumps + 1)
                    {
                        zenret = fr.zen(zenret, maxdist == 0 ? 18000 : maxdist);
                        percent = Convert.ToInt16(Math.Floor((double)(list.Count / numofjumps * 100)));
                        totaldist += list[list.Count - 1].distance(zenret);
                        list.Add(zenret);
                    }
                    bldr.AppendLine("total distance: " + totaldist + " | RSE stars eliminated: " + (list.Count - 1));
                    findroute.star_st prevbldr = new findroute.star_st();
                    foreach (findroute.star_st x in list)
                    {
                        double dist = 0;
                        if (prevbldr.name != null)
                            dist = prevbldr.distance(x);
                        bldr.AppendLine(x.name + ", " + x.coord.x.ToString(CultureInfo.InvariantCulture) + ", " + x.coord.y.ToString(CultureInfo.InvariantCulture) + ", " + x.coord.z.ToString(CultureInfo.InvariantCulture) + ", " + dist.ToString(CultureInfo.InvariantCulture));
                        prev = x;
                    }
                    if (savefilename == "DEFAULT")
                        savefilename = "zen-" + start.name + "route.csv";
                    try
                    {
                        File.WriteAllText(savefilename, bldr.ToString());
                    }
                    catch (Exception e3)
                    {
                        MessageBox.Show("Unable to save file. Please contact Rapidfirecrh with this error: " + e3.Message);
                    }
                    break;
            }
        }
    }
}
