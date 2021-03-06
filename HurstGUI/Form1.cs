﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hurst_exp;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace HurstGUI
{
    public partial class Form1 : Form
    {
        Hurst hurst = new Hurst();
        Hurst_Result vysledky;

        public Form1()
        {
            InitializeComponent();
            chartHurst.Series.Clear();
            lbl_Hurst.Text = "Hurstův exponentt: ";
            lbl_rovnice.Text = "Rovnice přímky: ";
        }

        public static List<double> NactiData(string path) => (from radek in File.ReadAllLines(path) select Convert.ToDouble(radek)).ToList(); //Načítá data ze souboru

        public static List<double> NactiDataFixed(string path)
        {
            string radek;

            List<double> data = new List<double>();
            var filestream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var file = new StreamReader(filestream, Encoding.UTF8, true, 128);
            while ((radek = file.ReadLine()) != null)
            {
                radek = radek.Replace(".", ",");
                data.Add(Convert.ToDouble(radek));
            }
            return data;
        }

        private void buttonVypocti_Click(object sender, EventArgs e)
        {
            OpenFileDialog browser = new OpenFileDialog();
            browser.Filter = ("Text File|*.txt|CSV File|*.csv");
            browser.FilterIndex = 1;

            string path = "";

            if (browser.ShowDialog() == DialogResult.OK)
            {
                path = browser.FileName;
            }

            try
            {
                var data = path.Last() == 't' ? NactiDataFixed(path) : CtiCSV(path);

                vysledky = hurst.Spocti_Hurstuv_Koeficient(data);
                Grafuj(vysledky);
                lbl_Hurst.Text += vysledky.hurst.ToString();
                lbl_rovnice.Text += string.Format("y = {0} * x + {1}", vysledky.hurst, vysledky.YIntercept);
            }
            catch (Exception)
            {
                MessageBox.Show("Nesprávný formát vstupního souboru s daty", "Špatný formát", MessageBoxButtons.OK);
            }
        }

        public void Grafuj(Hurst_Result result)
        {
            chartHurst.Series.Add("Primka");
            chartHurst.Series["Primka"].XValueType = ChartValueType.Double;
            chartHurst.Series["Primka"].Points.AddXY(result.X.Min(), result.rovnicePrimky(result.X.Min()));
            chartHurst.Series["Primka"].Points.AddXY(result.X.Max(), result.rovnicePrimky(result.X.Max()));
            chartHurst.Series["Primka"].ChartType = SeriesChartType.Line;
            chartHurst.Series["Primka"].BorderWidth = 3;

            chartHurst.Series.Add("Data");
            chartHurst.Series["Data"].XValueType = ChartValueType.Double;
            for (int i = 0; i < result.X.Count - 1; i++)
            {
                chartHurst.Series["Data"].Points.AddXY(result.X[i], result.Y[i]);
            }
            chartHurst.Series["Data"].ChartType = SeriesChartType.Point;
            chartHurst.Series["Data"].BorderWidth = 3;
            chartHurst.Legends.Clear();
            //chartHurst.ChartAreas["0"].AxisX.Interval = 1;
        }
        public List<double> CtiCSV(string path)
        {
            List<double> dataCSV = new List<double>();
            using (var file = File.OpenText(path))
            {
                string line = file.ReadLine();
                while ((line = file.ReadLine()) != null)
                    dataCSV.Add(double.Parse(line.Split(';')[4].Replace('\"', ' ')));
                return dataCSV;
            }
        }
    }
}
