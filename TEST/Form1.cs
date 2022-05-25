using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Collections;

using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;

using System.Windows.Forms.DataVisualization.Charting;


namespace TEST
{
    public partial class Form1 : Form
    {
        Encoding encSjis = Encoding.GetEncoding("shift-jis");

        private delegate void Delegate_write(string data);
        private bool    endflg      = false;                    // 処理終了時 True
        public int      timeOut     = 0;
        public int      sTimeOut    = 0;
        public string   serialdat   = "";

        // chart
        public string cName1 = "HR";
        public string cName2 = "BR";
        public string cName3 = "PHR";
        public string cName4 = "PBR";

        public int cCnt = 100;

        public string RName = "RaderMaker";


        // 面倒なのでpublic宣言
        public bool Dflg = false;
        public int commandNo = 0;
        public string TextData = "";


        public struct Rader
        {
            public class Pos
            {
                public double x { get; set; }
                public double y { get; set; }
            }

            public class Vital
            {
                public double br { get; set; }
                public double hr { get; set; }
            }

            public class Plot
            {
                public double br { get; set; }
                public double hr { get; set; }
            }

            public class Param
            {
                public double x { get; set; }
                public double y { get; set; }
                public double minS { get; set; }
                public double measT { get; set; }
                public double movT { get; set; }
                public double maxP { get; set; }
                public double minP { get; set; }
            }
        }

        public Rader.Pos Pos = new Rader.Pos();
        public Rader.Vital Vital = new Rader.Vital();
        public Rader.Plot Plot = new Rader.Plot();

        public Rader.Param Param = new Rader.Param();




        private void DataClear()
        {
            Pos.x = 0;
            Pos.y = 0;

            Vital.br = 0;
            Vital.hr = 0;

            Plot.br = 0;
            Plot.hr = 0;
        }





        public Form1()
        {
            InitializeComponent();
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            SerialPortSerch();

            textBox1.Clear();
            chartctl();
            RadarChart();

            DataClear();
        }



        private void SerialPortSerch()
        {
            string[] portlist = SerialPort.GetPortNames();
            this.comboBox1.Items.Clear();
            this.comboBox1.Text = "";
            foreach (string PortName in portlist)
            {
                comboBox1.Items.Add(PortName);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }






        #region Vital Chart Init
        private void chartctl()
        {
            chart1.Series.Clear();              // メンバークリア
            chart1.ChartAreas.Clear();          // 描画エリアクリア
            chart1.Legends.Clear();             // 凡例非表示

            chart1.Series.Add(cName1);
            chart1.ChartAreas.Add(cName1);
            chart1.Series[cName1].ChartType = SeriesChartType.Spline;

            // add x軸目盛数値を非表示
            chart1.ChartAreas[cName1].AxisX.LabelStyle.Enabled = false;
            chart1.ChartAreas[cName1].AxisY.LabelStyle.Enabled = false;
            chart1.ChartAreas[cName1].AxisX.IsMarginVisible = false;
            chart1.ChartAreas[cName1].AxisY.IsMarginVisible = false;
            chart1.ChartAreas[cName1].AxisY.Maximum = 150;
            chart1.ChartAreas[cName1].AxisY.Minimum = 0;
            chart1.ChartAreas[cName1].AxisY.Interval = 10;

            chart1.ChartAreas[cName1].AxisX.LabelStyle.Enabled = false;
            chart1.ChartAreas[cName1].AxisY.LabelStyle.Enabled = false;
 
            chart1.ChartAreas[cName1].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[cName1].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[cName1].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[cName1].AxisX.MinorGrid.Enabled = false;

            chart1.ChartAreas[cName1].AxisX.MajorTickMark.Enabled = false;
            chart1.ChartAreas[cName1].AxisY.MajorTickMark.Enabled = false;

            // 呼吸表示
            chart1.Series.Add(cName2);
            chart1.Series[cName2].ChartType = SeriesChartType.Spline;

            // 心拍 (PLOT)
            chart1.Series.Add(cName3);
            chart1.Series[cName3].ChartType = SeriesChartType.Spline;

            // 呼吸 (PLOT)
            chart1.Series.Add(cName4);
            chart1.Series[cName4].ChartType = SeriesChartType.Spline;



            chart1.Series[cName1].Color = Color.Red;
            chart1.Series[cName1].BorderWidth = 2;

            chart1.Series[cName2].Color = Color.Blue;
            chart1.Series[cName2].BorderWidth = 2;

            chart1.Series[cName3].Color = Color.Orange;
            chart1.Series[cName3].BorderWidth = 2;

            chart1.Series[cName4].Color = Color.Gray;
            chart1.Series[cName4].BorderWidth = 2;

            for (int i=0; i<=cCnt; i++)
            {
                chart1.Series[cName1].Points.AddXY(i, 0);
                chart1.Series[cName2].Points.AddXY(i, 0);
                chart1.Series[cName3].Points.AddXY(i, 0);
                chart1.Series[cName4].Points.AddXY(i, 0);
            }
        }
        #endregion


        #region RaderChart Init
        private void RadarChart()
        {
            chart2.Series.Clear();              // メンバークリア
            chart2.ChartAreas.Clear();          // 描画エリアクリア
            chart2.Legends.Clear();             // 凡例非表示

            chart2.Series.Add(RName);
            chart2.ChartAreas.Add(RName);
            chart2.Series[RName].ChartType = SeriesChartType.Point;
            chart2.Series[RName].MarkerSize = 20;
            chart2.Series[RName].MarkerStyle = MarkerStyle.Circle;

            //chart2.ChartAreas[RName].BackColor = Color.DarkGray;

            // x軸 , y軸 目盛表示設定
            chart2.ChartAreas[RName].AxisX.LabelStyle.Enabled = true;
            chart2.ChartAreas[RName].AxisY.LabelStyle.Enabled = true;

            // 余白設定 (true:有り / false:無し)
            chart2.ChartAreas[RName].AxisX.IsMarginVisible = false;
            chart2.ChartAreas[RName].AxisY.IsMarginVisible = false;

            // Max 6m Default 4m
            chart2.ChartAreas[RName].AxisY.Maximum = 6;

            // Max ±6m Default ±3m
            chart2.ChartAreas[RName].AxisX.Minimum = -3;
            chart2.ChartAreas[RName].AxisX.Maximum = 3 ;
            chart2.ChartAreas[RName].AxisX.IsReversed = true;


            // x,y座標　クロス位置設定
            chart2.ChartAreas[RName].AxisX.Crossing = 0;
            chart2.ChartAreas[RName].AxisY.Crossing = 0;

            // 目盛インタバル値設定
            chart2.ChartAreas[RName].AxisX.Interval = 1;
            chart2.ChartAreas[RName].AxisY.Interval = 1;


            // X軸グリッド線表示設定
            chart2.ChartAreas[RName].AxisY.MajorGrid.Enabled = true;
            chart2.ChartAreas[RName].AxisY.MinorGrid.Enabled = false;

            // Y軸グリッド線表示設定
            chart2.ChartAreas[RName].AxisX.MajorGrid.Enabled = true;
            chart2.ChartAreas[RName].AxisX.MinorGrid.Enabled = false;


            chart2.Series[RName].Points.AddXY(0.00001, 0);
            chart2.Series[RName].Font = new Font("Arial", 8);

        }
        #endregion



        private void chartPlot(double val1, double val2, double val3, double val4)
        {
            for (int i = 1; i <= cCnt; i++)
            {
                chart1.Series[cName1].Points[i - 1].YValues = chart1.Series[cName1].Points[i].YValues;
                chart1.Series[cName2].Points[i - 1].YValues = chart1.Series[cName2].Points[i].YValues;
                chart1.Series[cName3].Points[i - 1].YValues = chart1.Series[cName3].Points[i].YValues;
                chart1.Series[cName4].Points[i - 1].YValues = chart1.Series[cName4].Points[i].YValues;
            }
            chart1.Series[cName1].Points.RemoveAt(cCnt);
            chart1.Series[cName1].Points.AddXY(cCnt, val1);

            chart1.Series[cName2].Points.RemoveAt(cCnt);
            chart1.Series[cName2].Points.AddXY(cCnt, val2);

            chart1.Series[cName3].Points.RemoveAt(cCnt);
            chart1.Series[cName3].Points.AddXY(cCnt, val3);

            chart1.Series[cName4].Points.RemoveAt(cCnt);
            chart1.Series[cName4].Points.AddXY(cCnt, val4);
        }



        private void DataOutPut()
        {
            double dummyH, dummyB;

            if (Dflg == true)
            {
                Dflg = false;
                if (commandNo < 4)
                {
                    // テキストデータ表示
                    label_x.Text = Pos.x.ToString("0.00");          // x座標表示
                    label_y.Text = Pos.y.ToString("0.00");          // y座標表示

                    label_Heart.Text = Vital.hr.ToString("0.00");   // 心拍数表示
                    label_Breath.Text = Vital.br.ToString("0.00");  // 呼吸数表示


                    // ** チャート表示データ作成 **
                    // 心拍 PLOT TEST
                    dummyH = Plot.hr / 50;  // TEST
                    dummyH = 50 + dummyH;

                    // 呼吸 PLOT TEST
                    dummyB = Plot.br / 10;

                    // Chart 書く!!
                    chartPlot(Vital.hr, (Vital.br * 4), dummyH, dummyB);


                    // X,Y Radar グラフ表示
                    chart2.Series[RName].Points.Clear();

                    double workX = Pos.x;
                    if (workX == 0) workX = 0.001;

                    DataPoint dp = new DataPoint(workX, Pos.y);
                    dp.Label = "ID-0";
                    chart2.Series[RName].Points.Add(dp);
/*
                    if (Pos.x == 0)
                    {
                        chart2.Series[RName].Points.AddXY(0.0001, Pos.y);
                    }
                    else
                    {
                        chart2.Series[RName].Points.AddXY(Pos.x, Pos.y);

                    }
*/
                }


                // console Log 表示
                TextData = TextData.Replace("\n", "\r\n");
                textBox1.AppendText(TextData);
                TextData = "";
            }

        }




        #region シリアルデータ出力処理
        // 文字列シリアル出力処理 ※やっつけ関数
        // HRS-R8A は文字列送信時、ウェイトを設けないと正常に受信出来ない様子。
        // 通常は "write"や"writeline"を用いる事で容易にデータ送信が可能だが、
        // 上記の事から1文字出力毎にwaitが必要。(約20ms程度)
        private void serialOut(string dat)
        {
            int i = dat.Length;
            string buf = "";
            for (int j = 0; j < i; j++)
            {
                buf = dat.Substring(j, 1);
                serialPort1.Write(buf);
                System.Threading.Thread.Sleep(50);
            }
            serialPort1.Write("\n");
            System.Threading.Thread.Sleep(50);
        }
        #endregion


        #region COM Port Open / Close
        /*!
         * COMポート：接続を開始します。
         */
        private bool serialPortOpen(string portname)
        {
            try
            {
                serialPort1.PortName = portname;        // 通信ポート番号
                //serialPort1.BaudRate = 115200;          // ボーレート
                serialPort1.BaudRate = 921600;          // ボーレート
                serialPort1.DataBits = 8;               // Data bit 8
                serialPort1.StopBits = StopBits.One;    // Stop bit 1
                serialPort1.Parity = Parity.None;       // Parity NONE
                serialPort1.NewLine = "\n";             // 改行コード
                serialPort1.Open();

                return (true);
            }
            catch (Exception)
            {
                MessageBox.Show("Error!!\nポートが開かれてます");

                return (false);
            }
        }
        /****************************************************************************/
        /*!
         * COMポート：接続を終了します。
         */
        private bool serialPortClose()
        {
            try
            {
                serialPort1.DiscardInBuffer();
                serialPort1.Close();
                return (true);
            }
            catch (Exception)
            {
                return(false);
            }
        }

        #endregion


        #region 受信データ処理
        private void serialRxDTask(string rxdat)
        {
            string[] param = new string[2];

            if (rxdat != null)
            {
                if (endflg == false)
                {
                    /*
                     * 色々と仮コード 
                     * シリアル受信を行ったデータ解析
                     * 「心拍、呼吸」、「移動」「準備中」などのステータスを取得し
                     * ステータスに応じた動作を行う
                     * 
                     * 改行コード "\n"を検出し文字列抽出を行う必要あり。
                     * デリミタ","にてデータ分割
                     * 分割を行ったデータが規定数あるか確認。
                     * 取得データを文字列から数値変換が必要。
                    */
                    string buf = "";
                    string work = "";
                    int total = 0;
                    int len = 0;

                    serialdat = serialdat + rxdat;


                    string command;
                    commandNo = 0;

                    work = serialdat;
                    total = serialdat.Length;               // 全データ数
                    len = work.IndexOf("\n");               // 改行コード位置検出
                    if (len != -1)
                    {
                        buf = work.Substring(0, len);

                        command = buf;
                        commandNo = 0;

                        if (command.Contains("Y:READY") == true) commandNo = 1;
                        if (command.Contains("N:") == true) commandNo = 2;
                        if (command.Contains("Y:MOVING") == true) commandNo = 3;

                        if (command.Contains("minS") == true) commandNo = 4;
                        if (command.Contains("measT") == true) commandNo = 5;
                        if (command.Contains("movT") == true) commandNo = 6;
                        if (command.Contains("maxP") == true) commandNo = 7;
                        if (command.Contains("minP") == true) commandNo = 8;
                        if (command.Contains("x ") == true) commandNo = 9;
                        if (command.Contains("y ") == true) commandNo = 10;


                        if (commandNo >= 4)
                        {
                            param = buf.Split(' ');
                            int aa = param.Length;
                            if (aa < 2)
                            {
                                commandNo = 0;
                            }
                        } else
                        {
                            Dflg = true;
                            DataOutPut();
                        }


                        switch (commandNo)
                        {
                            case 1: // 心拍データ受信処理
                                string[] arr = buf.Split(',');
                                if (arr.Length > 6)
                                {
                                    Pos.x = Convert.ToDouble(arr[1]);
                                    Pos.y = Convert.ToDouble(arr[2]);

                                    Vital.br = Convert.ToDouble(arr[4]);
                                    Vital.hr = Convert.ToDouble(arr[6]);

                                    Plot.br = Convert.ToDouble(arr[3]);
                                    Plot.hr = Convert.ToDouble(arr[5]);

                                }
                                break;
                            case 2: // 未検出処理
                                Pos.x = 0;
                                Pos.y = 0;
                                Vital.br = 0;
                                Vital.hr = 0;
                                Plot.br = 0;
                                Plot.hr = 0;
                                break;
                            case 3: // Move
                                string[] pos = buf.Split(',');
                                Pos.x = Convert.ToDouble(pos[1]);
                                Pos.y = Convert.ToDouble(pos[2]);
                                break;

                            case 4: // minS
                                textBox_minS.Text = param[1].ToString();
                                Param.minS = Convert.ToDouble(param[1].ToString());
                                break;
                            case 5: // measT
                                textBox_measT.Text = param[1].ToString();
                                Param.measT = Convert.ToDouble(param[1].ToString());
                                break;
                            case 6: // movT
                                textBox_movT.Text = param[1].ToString();
                                Param.movT = Convert.ToDouble(param[1].ToString());
                                break;
                            case 7: // maxP
                                textBox_maxP.Text = param[1].ToString();
                                Param.maxP = Convert.ToDouble(param[1].ToString());
                                break;
                            case 8: // minP
                                textBox_minP.Text = param[1].ToString();
                                Param.minP = Convert.ToDouble(param[1].ToString());
                                break;
                            case 9: // X
                                textBox_x.Text = param[1].ToString();
                                Param.x = Convert.ToDouble(param[1].ToString());
                                break;
                            case 10: // Y
                                textBox_y.Text = param[1].ToString();
                                Param.y = Convert.ToDouble(param[1].ToString());
                                break;
                            default:
                                break;
                        }

                        if ((len + 1) == total)
                        {
                            serialdat = "";
                        }
                        else
                        {
                            serialdat = serialdat.Substring((len + 1));
                        }
                    }
                    TextData = TextData + rxdat;
                }
            }
        }
        #endregion


        #region シリアルデータ受信処理
        private void serialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            sTimeOut = 100;

            if (endflg == true)
            {
                serialPortClose();
            }
            else
            {
                string data = serialPort1.ReadExisting();
                BeginInvoke(new Delegate_write(serialRxDTask), new Object[] { data });
            }

        }
        #endregion


        #region 通信ポート処理
        // 通信ポート処理
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                if (button1.Text == "CONNECT")
                {
                    endflg = false;
                    serialPortOpen(comboBox1.Text);
                    if (serialPort1.IsOpen == true)
                    {
                        Dflg = false;
                        //timer1.Enabled = true;
                        button1.Text = "DisCONNECT";
                        serialPort1.DiscardInBuffer();          // バッファ初期化
                        serialOut("stop");
                        serialOut("stop");
                        textBox1.Clear();

                        button2.Enabled = true;
                        serialdat = "";

                        button8.Enabled = false;
                        groupBox5.Enabled = true;
                    }
                } else
                {
                    //timer1.Enabled = false;
                    serialPortClose();
                    button1.Text = "CONNECT";
                    button8.Enabled = true;
                    groupBox5.Enabled = false;
                }
            }
        }
        #endregion




        // HRS-R8A Parameter読出し (雑...
        private void ParameterRead()
        {
            serialOut("x");         // Xレンジ (0 - 6 m)
            serialOut("y");         // Yレンジ (0 - 6 m)
            serialOut("minS");      // 移動検出速度 (0.35mが設定されていた)
            serialOut("measT");     // 
            serialOut("movT");
            serialOut("maxP");
            serialOut("minP");
        }

        private void ParameterSet_x()
        {
            double val = Convert.ToDouble(textBox_x.Text);
            if (val <= 6)
            {
                Param.x = val;
                string dat = "x ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_y()
        {
            double val = Convert.ToDouble(textBox_y.Text);
            if (val <= 5)   // スペックシートではmax6mだが、設定値は5m
            {
                Param.y = val;
                string dat = "y ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_minS()
        {
            double val = Convert.ToDouble(textBox_minS.Text);
            if (val <= 5)
            {
                Param.minS = val;
                string dat = "minS ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_measT()
        {
            double val = Convert.ToDouble(textBox_measT.Text);
            if (val <= 20000)
            {
                Param.measT = val;
                string dat = "measT ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_movT()
        {
            double val = Convert.ToDouble(textBox_movT.Text);
            if (val <= 20000)
            {
                Param.movT = val;
                string dat = "movT ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_maxP()
        {
            double val = Convert.ToDouble(textBox_maxP.Text);
            if (val <= 20000)
            {
                string dat = "maxP ";
                dat += val.ToString();
                serialOut(dat);
            }
        }

        private void ParameterSet_minP()
        {
            double val = Convert.ToDouble(textBox_minP.Text);
            if (val <= 20000)
            {
                Param.minP = val;
                string dat = "minP ";
                dat += val.ToString();
                serialOut(dat);
            }
        }





        // フォーム閉じるよ処理
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen == true)
            {
                Dflg = false;
                //timer1.Enabled = false;
                serialPort1.Close();
            }

        }








        // Timer 処理
        // 一定間隔にてイベント発生するよ
        private void timer1_Tick(object sender, EventArgs e)
        {
        }









        #region ボタン処理
        // シリアルポート再検索
        private void button8_Click(object sender, EventArgs e)
        {
            SerialPortSerch();
        }




        // レーダーセンサ Start
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;        // "START"ボタン無効化
            button3.Enabled = true;         // "STOP"ボタン有効化
            ParameterRead();
            serialOut("start");

            DataClear();

            chartctl();
            RadarChart();
        }

        // レーダーセンサ Stop
        private void button3_Click(object sender, EventArgs e)
        {
            serialOut("stop");
            button2.Enabled = true;         // "START"ボタン有効化
            button3.Enabled = false;        // "STOP"ボタン無効化
        }

        // レーダーセンサ Restart
        private void button4_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;        // "START"ボタン無効化
            button3.Enabled = true;         // "STOP"ボタン有効化
            ParameterRead();
            serialOut("reset");
            chartctl();
            RadarChart();
        }

        // console Log 消去
        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        // console Log クリップボードコピー
        private void button7_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(textBox1.Text, true);
        }


        // コマンド送信処理 (仮)
        private void button6_Click(object sender, EventArgs e)
        {
            string buf = textBox2.Text;
            if (checkBox1.Checked == false)
            {
                serialOut(buf);
            }
            else
            {
                /* 
                 * Debug
                 * チェックボックス有効時 Wait 無しにてデータ送信
                 */
                serialPort1.WriteLine(buf);
            }
        }


        private void button_read_all_Click(object sender, EventArgs e)
        {
            ParameterRead();
        }

        private void button_set_all_Click(object sender, EventArgs e)
        {
            ParameterSet_x();
            ParameterSet_y();
            ParameterSet_minS();
            ParameterSet_measT();
            ParameterSet_movT();
            ParameterSet_maxP();
            ParameterSet_minP();
        }

        private void button_set_x_Click(object sender, EventArgs e)
        {
            ParameterSet_x();
        }

        private void button_set_y_Click(object sender, EventArgs e)
        {
            ParameterSet_y();
        }

        private void button_set_minS_Click(object sender, EventArgs e)
        {
            ParameterSet_minS();
        }

        private void button_set_measT_Click(object sender, EventArgs e)
        {
            ParameterSet_measT();
        }

        private void button_set_movT_Click(object sender, EventArgs e)
        {
            ParameterSet_movT();
        }

        private void button_set_maxP_Click(object sender, EventArgs e)
        {
            ParameterSet_maxP();
        }

        private void button_set_minP_Click(object sender, EventArgs e)
        {
            ParameterSet_minP();
        }

        private void button_read_x_Click(object sender, EventArgs e)
        {
            serialOut("x");
        }

        private void button_read_y_Click(object sender, EventArgs e)
        {
            serialOut("y");
        }

        private void button_read_minS_Click(object sender, EventArgs e)
        {
            serialOut("minS");
        }

        private void button_read_measT_Click(object sender, EventArgs e)
        {
            serialOut("measT");
        }

        private void button_read_movT_Click(object sender, EventArgs e)
        {
            serialOut("movT");
        }

        private void button_read_maxP_Click(object sender, EventArgs e)
        {
            serialOut("maxP");
        }

        private void button_read_minP_Click(object sender, EventArgs e)
        {
            serialOut("minP");
        }




        #endregion

    }
}
