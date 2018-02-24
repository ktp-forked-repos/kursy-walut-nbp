using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace KursyWalutNBP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string GetAPI(string date = "")
        {
            //return "[{\"table\":\"A\",\"no\":\"039 / A / NBP / 2018\",\"effectiveDate\":\"2018 - 02 - 23\",\"rates\":[{\"currency\":\"bat(Tajlandia)\",\"code\":\"THB\",\"mid\":0.1078},{\"currency\":\"dolar amerykański\",\"code\":\"USD\",\"mid\":3.3911}]}]";
            if (date != "")
                date = "/" + date; // tables/{table}/{date}/

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.nbp.pl/api/exchangerates/tables/A" + date + "/?format=json");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch //(WebException ex)
            {
                return "";
            }
        }

        public class Root
        {
            /*
                "table": "A",
                "no": "039/A/NBP/2018",
                "effectiveDate": "2018-02-23",
                "rates": [
            */
            public string table { get; set; }
            public string no { get; set; }
            public string effectiveDate { get; set; }
            public IList<Rates> rates { get; set; }
        }

        public class Rates
        {
            /*
                "currency": "bat (Tajlandia)",
                "code": "THB",
                "mid": 0.1078 
            */
            public string Currency { get; set; }
            public string Code { get; set; }
            public double Mid { get; set; }
        }

        public void LoadToTable(string JsonText)
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            if (JsonText == "")
            {
                MessageBox.Show("Brak danych, wybierz inną datę lub typ tabeli.");
            } else
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                Root[] json = jsonSerializer.Deserialize<Root[]>(JsonText);

                textBox1.Text = json[0].no.ToString();
                dateTimePicker1.Value = DateTime.Parse(json[0].effectiveDate);
                dataGridView1.DataSource = json[0].rates;

                dataGridView1.Columns["Currency"].HeaderText = "Nazwa";
                dataGridView1.Columns["Currency"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["Code"].HeaderText = "Kod";
                dataGridView1.Columns["Mid"].HeaderText = "Kurs";

                comboBox1.Items.Add("PLN");
                comboBox2.Items.Add("PLN");

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    row.Cells["Currency"].Value = row.Cells["Currency"].Value.ToString().First().ToString().ToUpper() + row.Cells["Currency"].Value.ToString().Substring(1);
                    comboBox1.Items.Add(row.Cells["Code"].Value);
                    comboBox2.Items.Add(row.Cells["Code"].Value);
                }

                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                numericUpDown1.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(GetAPI());
            numericUpDown1.Maximum = Decimal.MaxValue;
            LoadToTable(GetAPI());
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            LoadToTable(GetAPI(date));
        }

        private void Konwerter_ValueChanged(object sender, EventArgs e)
        {
            if ((comboBox1.SelectedIndex >= 0) && (comboBox2.SelectedIndex >= 0))
            {
                decimal kurs1 = 0, kurs2 = 0;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["Code"].Value.ToString().Equals(comboBox1.Items[comboBox1.SelectedIndex].ToString()))
                    {
                        kurs1 = Convert.ToDecimal(row.Cells["Mid"].Value.ToString());
                    }

                    if (row.Cells["Code"].Value.ToString().Equals(comboBox2.Items[comboBox2.SelectedIndex].ToString()))
                    {
                        kurs2 = Convert.ToDecimal(row.Cells["Mid"].Value.ToString());
                    }
                }

                if (comboBox1.Items[comboBox1.SelectedIndex].ToString().Equals("PLN"))
                    kurs1 = 1;

                if (comboBox2.Items[comboBox2.SelectedIndex].ToString().Equals("PLN"))
                    kurs2 = 1;

                if (kurs2 > 0) numericUpDown2.Value = Convert.ToDecimal(numericUpDown1.Value * kurs1 / kurs2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = numericUpDown2.Value;
            int temp = comboBox1.SelectedIndex;
            comboBox1.SelectedIndex = comboBox2.SelectedIndex;
            comboBox2.SelectedIndex = temp;
        }
    }
}
