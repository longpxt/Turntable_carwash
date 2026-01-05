using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Text;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
//using Windows.Foundation;
//using System.Numerics;
using OpenCvSharp;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TronCarWashIoT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SerialDevice PLCserialPort = null;
        private DataWriter PLCdataWriteObject = null;
        private DataReader PLCdataReaderObject = null;
        private List<byte> LastBuffer = new List<byte>();
        private const int DistOfLida = 3215;
        private const string stFrameIDandAccess = "F90000FF00";
        private bool bNeedETX = false;
        private bool bComError = false;
        private List<string> ListOfWritePOSMX = new List<string>();
        private List<string> ListOfWritePOSTP = new List<string>();
        private List<string> ListOfWritePOSSay1 = new List<string>();
        private List<string> ListOfWritePOSSay2 = new List<string>();
        private int intPLCStatus = 0; 

        private int intIndexOfListWriteMX = 0;
        private int intIndexOfListWriteTP = 0;
        private int intIndexOfListWriteSay1 = 0;
        private int intIndexOfListWriteSay2 = 0;
        private const double WheelPointThreshold = 300;
        private List<Windows.Foundation.Point> ListOfPointCarRect = new List<Windows.Foundation.Point>();
        private List<Windows.Foundation.Point> ListOfPointWheels = new List<Windows.Foundation.Point>();
        private List<double> ListOfAngleWheels = new List<double>();
        private List<int> ListOfAngleSay = new List<int>();
        private List<Windows.Foundation.Point> ListOfNearPoint = new List<Windows.Foundation.Point>();
        private List<double> ListOfDistanceWheels = new List<double>();

        private List<ActionX> ListOfAction = new List<ActionX>();
        private int intIndexOfDuoiXe = 0;
        private int intIndexOfDauXe = 0;
        private bool bXeCao = false;
        private DateTime StartScanTime = new DateTime();
        private DateTime FinishScanTime = new DateTime();
        private DateTime StartWashTime = new DateTime();
        private DateTime FinishWashTime = new DateTime();

        //private LineX duongNoiX = new LineX();
        class Vector
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Vector(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
        class Rectangle
        {
            public Windows.Foundation.Point Point1 { get; set; }
            public Windows.Foundation.Point Point2 { get; set; }
            public Windows.Foundation.Point Point3 { get; set; }
            public Windows.Foundation.Point Point4 { get; set; }

            public Rectangle(Windows.Foundation.Point point1, Windows.Foundation.Point point2, Windows.Foundation.Point point3, Windows.Foundation.Point point4)
            {
                Point1 = point1;
                Point2 = point2;
                Point3 = point3;
                Point4 = point4;
            }
        }
        public struct ScanX
        {
            public float Alpha;
            public int Lida1Dist;

            public ScanX(float dAlpha, int dLida1dist)
            {
                Alpha = dAlpha;
                Lida1Dist = dLida1dist;
            }
        };

        public struct LineX
        {
            public Windows.Foundation.Point A;
            public Windows.Foundation.Point B;

            public LineX(Windows.Foundation.Point pA, Windows.Foundation.Point pB)
            {
                A = pA;
                B = pB;
            }
        };

        public struct TiaX
        {
            public double GocTiaX;
            public double DoDaiTiaX;
            public int LineChamX;

            public TiaX(double GocTX, double DoDaiTX, int LineChamTX)
            {
                GocTiaX = GocTX;
                DoDaiTiaX = DoDaiTX;
                LineChamX = LineChamTX;
            }

        }

        public struct DiemChamX
        {
            public Windows.Foundation.Point DiemCham;
            public int LineCham;

            public DiemChamX(Windows.Foundation.Point Diem_Cham, int Line_Cham)
            {
                DiemCham = Diem_Cham;
                LineCham = Line_Cham;
            }

        }

        public struct ActionX
        {
            public int RunMode;
            public double GocMX;
            public double GocTP;
            public int TocDo;
            public int ViTriPhunBot;
            public int NhanhPhunBot;
            public int ViTriPhunNuoc;
            public int DoDai;
            public int Delay;
            public double GocSay1;
            public double GocSay2;
            public bool Say;


            public ActionX(int Run_Mode, double Goc_MX, double Goc_TP, int Toc_Do, int ViTri_PhunBot, int Nhanh_PhunBot, int ViTri_PhunNuoc, int Do_Dai, int De_lay, double Goc_Say1, double Goc_Say2, bool bSay)
            {
                RunMode = Run_Mode;
                GocMX = Goc_MX;
                GocTP = Goc_TP;
                TocDo = Toc_Do;
                ViTriPhunBot = ViTri_PhunBot;
                NhanhPhunBot = Nhanh_PhunBot;
                ViTriPhunNuoc = ViTri_PhunNuoc;
                DoDai = Do_Dai;
                Delay = De_lay;
                GocSay1 = Goc_Say1;
                GocSay2 = Goc_Say2;
                Say = bSay;
            }
        }

        public struct GocThanXe
        {
            public double Angle;
            public int LineIndex;

            public GocThanXe(double Angle_, int Line_Index)
            {
                Angle = Angle_;
                LineIndex = Line_Index;
            }
        }

        public struct FoamArea
        {
            public double GocMX;
            public double GocLiaTrai;
            public double GocLiaPhai;
            public int ViTriFoam;
            public bool Lia;

            public FoamArea(double Goc_MX, double Goc_LiaTrai, double Goc_LiaPhai, int ViTri_Foam, bool Lia_)
            {
                GocMX = Goc_MX;
                GocLiaTrai = Goc_LiaTrai;
                GocLiaPhai = Goc_LiaPhai;
                ViTriFoam = ViTri_Foam;
                Lia = Lia_;
            }

        }


        const double hesoAp = 1.40;//1,4, 1.3
        const int tocdoBotThanXe = 130; //1,4 rpm    
        const int tocdoBotBanhXe = 80; //0,5 rpm. 
        const int tocdoBotDuoiXe = 100; //1 rpm 
        const int tocdoBotNocXe = 100; //1 rpm
        const int tocdoNuocBanhXe = 30; //
        const int tocdoNuocThanXe = 100; //
        const int tocdoNuocDuoiXe = 70; //80
        const int tocdoBotBong = 200;
        const int tocdoNuocBong = 150;
        const int tocdoGiamToc = 10; // 20    
 
        const int tocdoXoayNext = 300; 
        const int tocdoXoayNextSlow = 260; 

        const int tocdoSay = 40;
        const int tocdoDiChuyenSay = 300;
        const int tocdoDiChuyenSayNhanh = 250;
        const int tocdoDiChuyenSayCham = 50;
        const int tocdoDelay = 1;
        const int tocdoQuetSay = 10;
        const int buocXoay = 5;
        const int buocXoayHiRes = 1;
        const int BotDelay = 30;



        Windows.Foundation.Point tamXoay = new Windows.Foundation.Point(3000, 3000);
        Windows.Foundation.Point tamPhun = new Windows.Foundation.Point(666, 5333); //center point of rotation for spraying

        double gocXoay = 0;



        List<ScanX> ListOfScanBody = new List<ScanX>();
        List<ScanX> ListOfScanWheels = new List<ScanX>();
        List<ScanX> ListOfScanUpper = new List<ScanX>();

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource PLCReadCancellationTokenSource;

        public MainPage()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                //status.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

            }
            catch (Exception ex)
            {
                status.Text = "List Port Error: " + ex.Message;
            }

            OpenPLC();
        }

        private async void OpenPLC()
        {
            try
            {
                //PLCserialPort = await SerialDevice.FromIdAsync(listOfDevices[3].Id);
                //Id = "\\\\?\\FTDIBUS#VID_0403+PID_6001+AB0OHNP6A#0000#{86e0d1e0-8089-11d0-9ce4-08003e301f73}"
                PLCserialPort = await SerialDevice.FromIdAsync("\\\\?\\FTDIBUS#VID_0403+PID_6001+AB0OHNP6A#0000#{86e0d1e0-8089-11d0-9ce4-08003e301f73}");
                if (PLCserialPort == null) return;

                PLCserialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                PLCserialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                PLCserialPort.BaudRate = 115200;
                PLCserialPort.Parity = SerialParity.Odd;
                PLCserialPort.StopBits = SerialStopBitCount.One;
                PLCserialPort.DataBits = 7;
                PLCserialPort.Handshake = SerialHandshake.None;
                PLCReadCancellationTokenSource = new CancellationTokenSource();
                PLCListen();
            }
            catch (Exception ex)
            {
                status.Text = "Open PLC Com Error: " + ex.Message;

            }
        }

        private async void PLCListen()
        {
            try
            {
                if (PLCserialPort != null)
                {
                    PLCdataReaderObject = new DataReader(PLCserialPort.InputStream);
                    while (true)
                    {
                        await PLCReadAsync(PLCReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                status.Text = "Reading PLC task was cancelled";
                PLCCloseDevice();
            }
            catch (Exception ex)
            {
                status.Text = "Listen PLC Error: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (PLCdataReaderObject != null)
                {
                    PLCdataReaderObject.DetachStream();
                    PLCdataReaderObject = null;
                }
            }
        }

        private async Task PLCReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            cancellationToken.ThrowIfCancellationRequested();

            PLCdataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                loadAsyncTask = PLCdataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead >= 0)
                {
                    byte[] buffer = new byte[bytesRead];

                    PLCdataReaderObject.ReadBytes(buffer);

                    if (buffer.Length > 0)
                    {
                        if (buffer[0] == 0x02)//STX
                        {
                            bNeedETX = true;

                            int bufferLen = 0;
                            //find ETX
                            for (int i = 1; i < buffer.Length - 2; i++)
                            {
                                if (buffer[i] == 0x3)
                                {
                                    bufferLen = i + 2 + 1;
                                    bNeedETX = false;
                                    break;
                                }
                            }

                            if (bNeedETX)
                            {
                                LastBuffer.Clear();
                                for (int i = 0; i < buffer.Length; i++)
                                    LastBuffer.Add(buffer[i]);
                            }
                            else
                            {
                                var datalist = new List<byte>();
                                for (int i = 1; i < bufferLen - 2; i++)
                                    datalist.Add(buffer[i]);

                                var datacheck = new List<byte>();
                                for (int i = bufferLen - 2; i < bufferLen; i++)
                                    datacheck.Add(buffer[i]);

                                bool check = CheckData(datalist.ToArray(), datacheck.ToArray());
                                if (check)
                                {
                                    var dataTemp = new List<byte>();

                                    if ((buffer[1] == 0x46) && (buffer[2] == 0x39))
                                    {
                                        for (int i = 11; i < bufferLen - 3; i++)
                                            dataTemp.Add(buffer[i]);
                                    }
                                    else
                                    {
                                        for (int i = 5; i < bufferLen - 3; i++)
                                            dataTemp.Add(buffer[i]);
                                    }

                                    await DataProcess(dataTemp.ToArray());

                                    //status.Text = "CRC OK";
                                }
                                else
                                    status.Text = "Data error";
                            }
                        }
                        else if (buffer[0] == 0x06)//ACK //
                        {
                            status.Text = "ACK";
                            switch (intPLCStatus)
                            {
                                case 100:
                                    intPLCStatus = 101;
                                    SendStartScan();
                                    break;
                                case 200:
                                    intPLCStatus = 201;
                                    break;
                                case 300:
                                    intPLCStatus = 301;
                                    break;
                                case 310:
                                    intPLCStatus = 311;
                                    break;
                                case 320:
                                    intPLCStatus = 321;
                                    break;
                                case 400:
                                    SendPosDataForWash();
                                    break;
                                case 450:
                                    SendPosDataForWash();
                                    break;
                                case 460:
                                    SendPosDataForWash();
                                    break;
                                case 470:
                                    SendPosDataForWash();
                                    break;
                                case 480:
                                    SendStartWash();
                                    break;
                                case 500:
                                    intPLCStatus = 501;
                                    break;
                            }
                        }
                        else if (buffer[0] == 0x15) //NAK
                        {
                            switch (intPLCStatus)
                            {
                                case 100:
                                    intPLCStatus = 102;
                                    break;
                                case 200:
                                    intPLCStatus = 202;
                                    break;
                                case 300:
                                    intPLCStatus = 302;
                                    break;
                                case 310:
                                    intPLCStatus = 312;
                                    break;
                                case 320:
                                    intPLCStatus = 322;
                                    break;
                                case 400:
                                    intPLCStatus = 402;
                                    break;
                                case 450:
                                    intPLCStatus = 452;
                                    break;
                                case 460:
                                    intPLCStatus = 462;
                                    break;
                                case 500:
                                    intPLCStatus = 502;
                                    break;
                            }
                            status.Text = "NAK";
                        }
                        else if (bNeedETX) 
                        {
                            int bufferLen = 0;
                            for (int i = 0; i < buffer.Length; i++)
                                LastBuffer.Add(buffer[i]);

                            //find ETX
                            for (int i = 1; i < LastBuffer.Count - 2; i++)
                            {
                                if (LastBuffer[i] == 0x3)
                                {
                                    bufferLen = i + 2 + 1;
                                    break;
                                }
                            }

                            var datalist = new List<byte>();
                            for (int i = 1; i < bufferLen - 2; i++)
                                datalist.Add(LastBuffer[i]);

                            var datacheck = new List<byte>();
                            for (int i = bufferLen - 2; i < bufferLen; i++)
                                datacheck.Add(LastBuffer[i]);

                            bool check = CheckData(datalist.ToArray(), datacheck.ToArray());
                            if (check)
                            {
                                var dataTemp = new List<byte>();
                                if ((LastBuffer[1] == 0x46) && (LastBuffer[2] == 0x39))
                                {
                                    for (int i = 11; i < bufferLen - 3; i++)
                                        dataTemp.Add(LastBuffer[i]);
                                }
                                else
                                {
                                    for (int i = 5; i < bufferLen - 3; i++)
                                        dataTemp.Add(LastBuffer[i]);
                                }


                                await DataProcess(dataTemp.ToArray());

                                //status.Text = "CRC OK";
                            }
                            else
                                status.Text = "Data error";
                        }

                    }

                }
            }
        }

        private async Task DataProcess(byte[] data)
        {
            switch (intPLCStatus)
            {
                case 201: //sent scan cmd;
                    string stData = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    int intResult = Int32.Parse(stData, System.Globalization.NumberStyles.HexNumber);
                    if (intResult == 2)//read data scan from PLC to pc
                        SendReadScanData1();

                    break;
                case 300:
                    ListOfScanBody.Clear();
                    for (int i = 0; i < (data.Length / 4); i++)
                    {
                        byte[] byteData = new byte[4];
                        for (int x = 0; x < 4; x++)
                            byteData[x] = data[i * 4 + x];

                        string stBytetoString = Encoding.ASCII.GetString(byteData);
                        ScanX ScanData = new ScanX(i * 1, Convert.ToInt32(stBytetoString, 16)); //i*1 == 1 degree
                        ListOfScanBody.Add(ScanData);
                    }
                    SendReadScanData2();

                    break;

                case 310:
                    ListOfScanWheels.Clear();
                    for (int i = 0; i < (data.Length / 4); i++)
                    {
                        byte[] byteData = new byte[4];
                        for (int x = 0; x < 4; x++)
                            byteData[x] = data[i * 4 + x];

                        string stBytetoString = Encoding.ASCII.GetString(byteData);
                        ScanX ScanData = new ScanX(i * 1, Convert.ToInt32(stBytetoString, 16));//i*1 == 1 degree
                        ListOfScanWheels.Add(ScanData);
                    }
                    SendReadScanData3();

                    break;
                case 320:
                    ListOfScanUpper.Clear();
                    for (int i = 0; i < (data.Length / 4); i++)
                    {
                        byte[] byteData = new byte[4];
                        for (int x = 0; x < 4; x++)
                            byteData[x] = data[i * 4 + x];

                        string stBytetoString = Encoding.ASCII.GetString(byteData);
                        ScanX ScanData = new ScanX(i * 1, Convert.ToInt32(stBytetoString, 16));//i*1 == 1 degree
                        ListOfScanUpper.Add(ScanData);
                    }
                    await WriteToCsvFileAsync();
                    ScanToBorder();

                    break;
                case 501:
                    WashDone();
                    break;
            }
        }

        private bool CheckData(byte[] datalist, byte[] datacheck)
        {
            bool Checked = false;
            int sumCheck = 0;
            foreach (byte data in datalist)
            {
                sumCheck += data;
            }
            string check_datalist = sumCheck.ToString("X");
            if (check_datalist.Length > 2)
            {
                check_datalist = check_datalist.Substring(check_datalist.Length - 2);
            }

            string check_datacheck = Encoding.ASCII.GetString(datacheck);

            if (check_datalist == check_datacheck)
                Checked = true;
            else
                Checked = false;

            return Checked;
        }

        private async void SendCommand(string stCommand)
        {
            bComError = false;
            try
            {
                if (PLCserialPort != null)
                {
                    PLCdataWriteObject = new DataWriter(PLCserialPort.OutputStream);
                    var cmdEncode = encoding_command(stCommand);
                    await PLCWriteAsync(cmdEncode);
                }
                else
                {
                    status.Text = "PLC not connected";
                    bComError = true;
                }
            }
            catch (Exception ex)
            {
                status.Text = "PLC Error: " + ex.Message;
                bComError = true;
            }
            finally
            {
                // Cleanup once complete
                if (PLCdataWriteObject != null)
                {
                    PLCdataWriteObject.DetachStream();
                    PLCdataWriteObject = null;
                }
            }
        }

        private void SendStartScan()
        {
            //Format 1 Frame 3C
            string stCommand = "1401";
            string stSubcommand = "0001";
            string Devicecode = "M*000031";//00-000000
            string Numofdevide = "0001";
            string Writedata = "1";
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + Writedata;

            intPLCStatus = 200;//
            SendCommand(cmd);
        }

        private void SendStartWash()
        {
            //Format 1 Frame 3C
            string stCommand = "1401";
            string stSubcommand = "0001";
            string Devicecode = "M*000030";//00-000000
            string Numofdevide = "0001";
            string Writedata = "1";
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + Writedata;

            intPLCStatus = 500;//
            SendCommand(cmd);
        }

        private void SendPosDataForScan()
        {
            StartScanTime = DateTime.Now;

            string DataWrite = "";
            int intNumberofDevice;
            int intDeviceCode;
            string Devicecode = "";
            string Numofdevide = "";
            string stCommand = "";
            string stSubcommand = "";
            string cmd = "";

            intPLCStatus = 100;//

            ListOfWritePOSMX.Clear();

            ListOfWritePOSMX.Add(PositionDataMXSingle(0, 0, 0, 270, 3600));//1.8 rpm
            for (int i = 0; i < ListOfWritePOSMX.Count; i++)
                DataWrite += ListOfWritePOSMX[i];

            intNumberofDevice = ListOfWritePOSMX.Count * 10;
            intDeviceCode = 2000;
            Devicecode = "00U000000G*" + intDeviceCode.ToString("D6") + "000";//00-U000-000-G*000000-000
            Numofdevide = intNumberofDevice.ToString("X4");
            stCommand = "1401";
            stSubcommand = "0080";

            cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + DataWrite;
            SendCommand(cmd);
        }

        private void SendPosDataForWash()
        {
            if (intPLCStatus == 402)
                return;//error when send;
            else if (bComError)
                return;//comport error;

            string DataWrite = "";
            int intNumberofDevice;
            int intDeviceCode;
            string Devicecode = "";
            string Numofdevide = "";
            string stCommand = "";
            string stSubcommand = "";
            string cmd = "";

            int intStepSend = 80;//max data to send to PLC


            switch (intPLCStatus)
            {
                case 400:
                    int intEndOfStepSend = 0;
                    if ((intIndexOfListWriteMX + intStepSend) >= ListOfWritePOSMX.Count - 1)
                    {
                        intEndOfStepSend = ListOfWritePOSMX.Count;
                        intPLCStatus = 450; 
                    }
                    else
                    {
                        intEndOfStepSend = intIndexOfListWriteMX + intStepSend;
                    }

                    for (int i = intIndexOfListWriteMX; i < intEndOfStepSend; i++)
                        DataWrite += ListOfWritePOSMX[i];

                    intNumberofDevice = (intEndOfStepSend - intIndexOfListWriteMX) * 10;
                    if (intNumberofDevice > 0)
                    {
                        intDeviceCode = 2000 + intIndexOfListWriteMX * 10;

                        intIndexOfListWriteMX = intEndOfStepSend;

                        Devicecode = "00U000000G*" + intDeviceCode.ToString("D6") + "000";//00-U000-000-G*000000-000
                        Numofdevide = intNumberofDevice.ToString("X4");
                        stCommand = "1401";
                        stSubcommand = "0080";

                        cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + DataWrite;
                        SendCommand(cmd);
                    }
                    break;

                case 450:
                    intEndOfStepSend = 0;
                    if ((intIndexOfListWriteTP + intStepSend) >= ListOfWritePOSTP.Count - 1)
                    {
                        intEndOfStepSend = ListOfWritePOSTP.Count - 1;
                        intPLCStatus = 460; 
                    }
                    else
                    {
                        intEndOfStepSend = intIndexOfListWriteTP + intStepSend;
                    }

                    for (int i = intIndexOfListWriteTP; i < intEndOfStepSend; i++)
                        DataWrite += ListOfWritePOSTP[i];

                    intNumberofDevice = (intEndOfStepSend - intIndexOfListWriteTP) * 10;
                    if (intNumberofDevice > 0)
                    {
                        intDeviceCode = 8000 + intIndexOfListWriteTP * 10;

                        intIndexOfListWriteTP = intEndOfStepSend;

                        Devicecode = "00U000000G*" + intDeviceCode.ToString("D6") + "000";//00-U000-000-G*000000-000
                        Numofdevide = intNumberofDevice.ToString("X4");
                        stCommand = "1401";
                        stSubcommand = "0080";

                        cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + DataWrite;
                        SendCommand(cmd);
                    }
                    break;

                case 460:
                    intEndOfStepSend = 0;
                    if ((intIndexOfListWriteSay1 + intStepSend) >= ListOfWritePOSSay1.Count - 1)
                    {
                        intEndOfStepSend = ListOfWritePOSSay1.Count - 1;
                        intPLCStatus = 470; 
                    }
                    else
                    {
                        intEndOfStepSend = intIndexOfListWriteSay1 + intStepSend;
                    }

                    for (int i = intIndexOfListWriteSay1; i < intEndOfStepSend; i++)
                        DataWrite += ListOfWritePOSSay1[i];

                    intNumberofDevice = (intEndOfStepSend - intIndexOfListWriteSay1) * 10;
                    if (intNumberofDevice > 0)
                    {
                        intDeviceCode = 14000 + intIndexOfListWriteSay1 * 10;

                        intIndexOfListWriteSay1 = intEndOfStepSend;

                        Devicecode = "00U000000G*" + intDeviceCode.ToString("D6") + "000";//00-U000-000-G*000000-000
                        Numofdevide = intNumberofDevice.ToString("X4");
                        stCommand = "1401";
                        stSubcommand = "0080";

                        cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + DataWrite;
                        SendCommand(cmd);
                    }
                    
                    break;

                case 470:
                    intEndOfStepSend = 0;
                    if ((intIndexOfListWriteSay2 + intStepSend) >= ListOfWritePOSSay2.Count - 1)
                    {
                        intEndOfStepSend = ListOfWritePOSSay2.Count - 1;
                        intPLCStatus = 480; 
                    }
                    else
                    {
                        intEndOfStepSend = intIndexOfListWriteSay2 + intStepSend;
                    }

                    for (int i = intIndexOfListWriteSay2; i < intEndOfStepSend; i++)
                        DataWrite += ListOfWritePOSSay2[i];

                    intNumberofDevice = (intEndOfStepSend - intIndexOfListWriteSay2) * 10;
                    if (intNumberofDevice > 0)
                    {
                        intDeviceCode = 20000 + intIndexOfListWriteSay2 * 10;

                        intIndexOfListWriteSay2 = intEndOfStepSend;

                        Devicecode = "00U000000G*" + intDeviceCode.ToString("D6") + "000";//00-U000-000-G*000000-000
                        Numofdevide = intNumberofDevice.ToString("X4");
                        stCommand = "1401";
                        stSubcommand = "0080";

                        cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + DataWrite;
                        SendCommand(cmd);
                    }
               
                    break;

            }

       
            double TotalofData = ListOfWritePOSMX.Count + ListOfWritePOSTP.Count + ListOfWritePOSSay1.Count + ListOfWritePOSSay2.Count;
            double DataSent = intIndexOfListWriteMX + intIndexOfListWriteTP + intIndexOfListWriteSay1 + intIndexOfListWriteSay2 + 4;
            double Percent = DataSent / TotalofData * 100;

            DataTransfer.Value = Percent;
            ///

        }

        private void CreateData2()
        {
            StartWashTime = DateTime.Now;

            int intGocMX = 0;
            ClearPOS();//
            TimHocBanhXe();
            CreateActionList();

            if (ListOfAction.Count == 0)
                return;

            //move to fist pos ListOfFoamAction
            AddPOS(2, MCode(3, 0, 0, 0, false), 0, tocdoDelay, -3, (int)ListOfAction[0].GocTP * 10, 0, 0);
            //open foam and wait
            AddPOS(2, MCode(3, 0, 1, 1, false), 0, tocdoBotThanXe, 0, (int)ListOfAction[0].GocTP * 10, 0, 0);

            ActionX Action;
            for (int j = 0; j < ListOfAction.Count; j++)
            {
                Action = ListOfAction[j];
                intGocMX = (int)Action.GocMX;
                int intHz = (int)(Math.Sqrt(Action.DoDai) * hesoAp);
                if (intHz == 0)
                    intHz = 3;

                if (j == ListOfAction.Count - 1)
                    AddPOS(1, MCode(intHz, Action.ViTriPhunNuoc, Action.ViTriPhunBot, Action.NhanhPhunBot, Action.Say), Action.Delay, Action.TocDo, intGocMX * 10, (int)Action.GocTP * 10, (int)Action.GocSay1 * 10, (int)Action.GocSay1 * 10);
                else
                    AddPOS(Action.RunMode, MCode(intHz, Action.ViTriPhunNuoc, Action.ViTriPhunBot, Action.NhanhPhunBot, Action.Say), Action.Delay, Action.TocDo, intGocMX * 10, (int)Action.GocTP * 10, (int)Action.GocSay1 * 10, (int)Action.GocSay1 * 10);
            }
            AddPOS(0, MCode(3, 0, 0, 0, false), 0, tocdoDelay, intGocMX * 10 + 3, 0, 0, 0);
        }


        private void SendReadScanData1()
        {
            //Format 1 Frame 3C
            string stCommand = "0401";
            string stSubcommand = "0000";
            string Devicecode = "D*000400";//00-000000
            string Numofdevide = "0168"; //360 = 168 hex
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide;

            intPLCStatus = 300;//
            SendCommand(cmd);
        }
        private void SendReadScanData2()
        {
            //Format 1 Frame 3C
            string stCommand = "0401";
            string stSubcommand = "0000";
            string Devicecode = "D*000800";//00-000000
            string Numofdevide = "0168"; //360 = 168 hex
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide;

            intPLCStatus = 310;//
            SendCommand(cmd);
        }
        private void SendReadScanData3()
        {
            //Format 1 Frame 3C
            string stCommand = "0401";
            string stSubcommand = "0000";
            string Devicecode = "D*001200";//00-000000
            string Numofdevide = "0168"; //360 = 168 hex
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide;

            intPLCStatus = 320;//
            SendCommand(cmd);
        }

        private string PositionDataMX(int Operation, int MCode, int Sleep, int RPM, int Angle)
        {
            string stOperation = "00";
            switch (Operation)
            {
                case 0:
                    stOperation = "00";
                    break;
                case 1:
                    stOperation = "01";
                    break;
                case 2:
                    stOperation = "11";
                    break;
            }

            int PosAddress = Angle * 40;
            int Speed = RPM * 24; 
            int Dwell = Sleep * 1000; 
            string stInterPolation = "00";
            string stABS = "11010";


            string stPosControl = stABS + "1111" + stInterPolation + stOperation; //"1111" thời gian tăng giảm tốc, chương 5 trang 92
            string stHexPosControl = Convert.ToInt32(stPosControl, 2).ToString("X4");
            string Writedata = stHexPosControl + Int2Hex(MCode, false) + Int2Hex(Dwell, false) + "0000" + Int2Hex(Speed, true) + Int2Hex(PosAddress, true) + "00000000";

            return Writedata;
        }

        private string PositionDataMXSingle(int Operation, int MCode, int Sleep, int RPM, int Angle)
        {
            string stOperation = "00";
            switch (Operation)
            {
                case 0:
                    stOperation = "00";
                    break;
                case 1:
                    stOperation = "01";
                    break;
                case 2:
                    stOperation = "11";
                    break;
            }

            int PosAddress = Angle * 40;
            int Speed = RPM * 24; 
            int Dwell = Sleep * 1000; 
            string stInterPolation = "00";
            string stABS = "0001";//01H - ABS1: 1 axis linear control (ABS)


            string stPosControl = stABS + "1111" + stInterPolation + stOperation; 
            string stHexPosControl = Convert.ToInt32(stPosControl, 2).ToString("X4");
            string Writedata = stHexPosControl + Int2Hex(MCode, false) + Int2Hex(Dwell, false) + "0000" + Int2Hex(Speed, true) + Int2Hex(PosAddress, true) + "00000000";

            return Writedata;
        }

        private string PositionDataMXDelay(int Operation, int MCode, int Sleep, int RPM, int Angle)
        {
            string stOperation = "00";
            switch (Operation)
            {
                case 0:
                    stOperation = "00";
                    break;
                case 1:
                    stOperation = "01";
                    break;
                case 2:
                    stOperation = "11";
                    break;
            }

            int PosAddress = Angle;
            int Speed = RPM; 
            int Dwell = Sleep * 1000; 
            string stInterPolation = "00";
            string stABS = "11010";


            string stPosControl = stABS + "1111" + stInterPolation + stOperation; 
            string stHexPosControl = Convert.ToInt32(stPosControl, 2).ToString("X4");
            string Writedata = stHexPosControl + Int2Hex(MCode, false) + Int2Hex(Dwell, false) + "0000" + Int2Hex(Speed, true) + Int2Hex(PosAddress, true) + "00000000";

            return Writedata;
        }

        private void SendPositionDataMX(int Device, int Operation, bool InterPolation, int MCode, int Sleep, int RPM, int Angle)
        {
            string stOperation = "00";
            switch (Operation)
            {
                case 0:
                    stOperation = "00";
                    break;
                case 1:
                    stOperation = "01";
                    break;
                case 2:
                    stOperation = "11";
                    break;
            }

            int PosAddress = Angle * 40;//resolution 0,1 degree
            int Speed = RPM * 240; //2400 = 1 rpm=
            int Dwell = Sleep * 1000; // 1 s, Sleep < 65 s
            string stInterPolation = "00";
            string stABS = "0001";

            if (InterPolation)
            {
                stInterPolation = "01";
                stABS = "1010";
            }

            string stPosControl = stABS + "1111" + stInterPolation + stOperation;
            string stHexPosControl = Convert.ToInt32(stPosControl, 2).ToString("X4");
            string Devicecode = "00U000000G*" + Device.ToString("D6") + "000";//00-U000-000-G*000000-000
            string Numofdevide = "0008";
            string stCommand = "1401";
            string stSubcommand = "0080";

            string Writedata = stHexPosControl + Int2Hex(MCode, false) + Int2Hex(Dwell, false) + "0000" + Int2Hex(Speed, true) + Int2Hex(PosAddress, true);
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + Writedata;

            SendCommand(cmd);
        }

        private string PositionDataTP(int Angle)
        {
    
            if (Angle > 850)
                Angle = 850;
            else if (Angle < 50)
                Angle = 50;

            int PosAddress = Angle * 4; 
            string Writedata = "000000000000000000000000" + Int2Hex(PosAddress, true) + "00000000";

            return Writedata;
        }
        private string PositionDataSay(int Angle)
        {
            int PosAddress = Angle * 4; 
            string Writedata = "000000000000000000000000" + Int2Hex(PosAddress, true) + "00000000";

            return Writedata;
        }

        private void ClearPOS()
        {
            ListOfWritePOSMX.Clear();
            ListOfWritePOSTP.Clear();
            ListOfWritePOSSay1.Clear();
            ListOfWritePOSSay2.Clear();
        }
        private void AddPOS(int Operation, int MCode, int Sleep, int RPM, int MXAngle, int TPAngle, int Say1Angle, int Say2Angle)
        {
            ListOfWritePOSMX.Add(PositionDataMX(Operation, MCode, Sleep, RPM, MXAngle));
            ListOfWritePOSTP.Add(PositionDataTP(TPAngle));
            ListOfWritePOSSay1.Add(PositionDataSay(Say1Angle));
            ListOfWritePOSSay2.Add(PositionDataSay(Say2Angle));
        }

        private void SendPositionDataTP(int Device, int Angle)
        {
            string Devicecode = "00U000000G*" + Device.ToString("D6") + "000";//00-U000-000-G*000000-000
            string Numofdevide = "0002";
            string stCommand = "1401";
            string stSubcommand = "0080";

            int PosAddress = Angle * 4; //resulution 0,1 degree
            string Writedata = Int2Hex(PosAddress, true);
            string cmd = stFrameIDandAccess + stCommand + stSubcommand + Devicecode + Numofdevide + Writedata;

            SendCommand(cmd);
        }

        private string Int2Hex(int intData, bool bX8)
        {
            string stReturn;
            if (!bX8)
                stReturn = intData.ToString("X4");
            else
            {
                string stTemp = intData.ToString("X8");
                stReturn = stTemp.Substring(4, 4) + stTemp.Substring(0, 4);
            }
            return stReturn;
        }

        private int MCode(int Hz, int NhanhPhunNuoc, int NhanhPhunBot, int Form, bool bSay)
        {
            int HzTemp = (int)(Hz / 3);
            if (HzTemp > 31)
                HzTemp = 31;
            string HzBin = Convert.ToString(HzTemp, 2).PadLeft(5, '0');
            string NhanhPhunNuocBin = Convert.ToString(NhanhPhunNuoc, 2).PadLeft(4, '0');
            string NhanhPhunBotBin = Convert.ToString(NhanhPhunBot, 2).PadLeft(4, '0');

            string SayBin = "0";
            if (bSay)
                SayBin = "1";

            string FormBin = "00";
            if (Form == 1) //1: Wash, 2: Wax; 0: Off
                FormBin = "01";
            else if (Form == 2)
                FormBin = "10";
            else if (Form == 0)
                FormBin = "00";

            string stMCode = SayBin + FormBin + NhanhPhunBotBin + NhanhPhunNuocBin + HzBin;

            return Convert.ToInt32(stMCode, 2);
        }

        private string IntToBinaryMcode(int number, int len)
        {
            string binary = "";
            for (int i = 0; i <= len; i++)
            {
                if (number == i + 1)
                    binary += "1";
                else
                    binary += "0";
            }

            return binary;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnConfirm.IsEnabled = false;
            btnConfirm_2.IsEnabled = false;
            btnReWash.IsEnabled = false;
            btnStart.Content = "Scanning...";

            bXeCao = false;

            SendPosDataForScan();
        }


        private List<byte> encoding_command(string cmd)
        {
            byte[] textToASCII = Encoding.ASCII.GetBytes(cmd);
            var cmdList = new List<byte>();
            cmdList.Add(0x05); //ENQ

            int calSum = 0;

            foreach (byte ascii in textToASCII)
            {
                calSum += ascii;
                cmdList.Add(ascii);
            }

            byte[] sumCheck = Encoding.ASCII.GetBytes(calSum.ToString("X"));
            cmdList.Add(sumCheck[sumCheck.Length - 2]);
            cmdList.Add(sumCheck[sumCheck.Length - 1]);

            return cmdList;
        }

        private async Task PLCWriteAsync(List<byte> cmd)
        {
            Task<UInt32> storeAsyncTask;

            if (cmd.Count != 0)
            {
                PLCdataWriteObject.WriteBytes(cmd.ToArray());

                storeAsyncTask = PLCdataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    status.Text = "Command has been sent to the PLC";
                }
                status.Text = "";
            }

        }

        private void PLCCloseDevice()
        {
            if (PLCserialPort != null)
            {
                PLCserialPort.Dispose();
            }

            PLCserialPort = null;

        }

        public async Task WriteToCsvFileAsync()
        {

            var storageFolder = ApplicationData.Current.LocalFolder;
            
            string FileName = StartScanTime.Year.ToString() + StartScanTime.Month.ToString() + StartScanTime.Day.ToString() + StartScanTime.Hour.ToString() + StartScanTime.Minute.ToString();
            var file = await storageFolder.CreateFileAsync(FileName + ".csv", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                using (var writer = new System.IO.StreamWriter(stream, Encoding.UTF8))
                {                   

                    for (int i=0;i< ListOfScanBody.Count; i++)
                    {
                        ScanX BodyX = ListOfScanBody[i];
                        ScanX WheelX = ListOfScanWheels[i];
                        ScanX UpperX = ListOfScanUpper[i];

                        await writer.WriteLineAsync($"{BodyX.Alpha},{BodyX.Lida1Dist},{WheelX.Lida1Dist},{UpperX.Lida1Dist}");
                    }

                    await writer.FlushAsync();
                }
                
            }
            
        }


        private void ScanToBorder()
        {
            cvs_Draw.Children.Clear();

            List<OpenCvSharp.Point> ListOfPointBody = new List<OpenCvSharp.Point>();
            List<Windows.Foundation.Point> ListOfPointScanWheels = new List<Windows.Foundation.Point>();
            List<Windows.Foundation.Point> ListOfPointUpper = new List<Windows.Foundation.Point>();


            foreach (ScanX ScanData in ListOfScanBody)
            {
                OpenCvSharp.Point point;
                double alpha = ScanData.Alpha; // dec alpha                
                int length = (int)(DistOfLida - ScanData.Lida1Dist); 
                if (ScanData.Lida1Dist > 4000) // 
                    length = 0;

                // Tính toạ độ của điểm cuối 
                alpha -= 90;
                point.X = 3000 + (int)(length * Math.Cos(alpha * Math.PI / 180));
                point.Y = 3000 + (int)(length * Math.Sin(alpha * Math.PI / 180));

                ListOfPointBody.Add(point);

            }

            RotatedRect rRect = Cv2.MinAreaRect(ListOfPointBody);
            Point2f[] rRectPoint = rRect.Points();
            CapNhatToaDoXe(rRectPoint);


            //đổi dữ liệu scan wheels ra toạ độ
            foreach (ScanX ScanData in ListOfScanWheels)
            {
                Windows.Foundation.Point point;
                double alpha = ScanData.Alpha; //                
                int length = (int)(DistOfLida - ScanData.Lida1Dist); // 
                if (ScanData.Lida1Dist > 4000) //
                    length = 0;

                // Tính toạ độ của điểm cuối 
                alpha -= 90;
                point.X = 3000 + (int)(length * Math.Cos(alpha * Math.PI / 180));
                point.Y = 3000 + (int)(length * Math.Sin(alpha * Math.PI / 180));
                ListOfPointScanWheels.Add(point);
            }

            PointsToBanhXe(ListOfPointScanWheels);

            //đổi dữ liệu scan upper body ra toạ độ
            foreach (ScanX ScanData in ListOfScanUpper)
            {
                Windows.Foundation.Point point;
                double alpha = ScanData.Alpha; //                
                int length = (int)(DistOfLida - ScanData.Lida1Dist);  
                if (ScanData.Lida1Dist > 4000) // 
                    length = 0;

                // Tính toạ độ của điểm cuối 
                alpha -= 90;
                point.X = 3000 + (int)(length * Math.Cos(alpha * Math.PI / 180));
                point.Y = 3000 + (int)(length * Math.Sin(alpha * Math.PI / 180));
                ListOfPointUpper.Add(point);
            }
            PointsToDauDuoiXe(ListOfPointUpper);

 

            SolidColorBrush Yellow = new SolidColorBrush();
            SolidColorBrush Green = new SolidColorBrush();
            SolidColorBrush Blue = new SolidColorBrush();
            SolidColorBrush White = new SolidColorBrush();
            SolidColorBrush Gray = new SolidColorBrush();
            SolidColorBrush Red = new SolidColorBrush();
            SolidColorBrush Black = new SolidColorBrush();

            Yellow.Color = Colors.Yellow;
            Green.Color = Colors.Green;
            Blue.Color = Colors.Blue;
            White.Color = Colors.White;
            Gray.Color = Colors.DarkGray;
            Red.Color = Colors.Red;
            Black.Color = Colors.Black;

            Polygon Car = new Polygon();
            Polygon Car2 = new Polygon();
            Polygon Wheel = new Polygon();
            Ellipse Turnable = new Ellipse();
            Line Back = new Line();
            Line LineToBack = new Line();


            Car.Stroke = Yellow;
            Car.StrokeThickness = 2;
            Car2.Stroke = Red;
            Car2.StrokeThickness = 2;
            Wheel.Stroke = Green;
            Wheel.StrokeThickness = 1;
            Back.Stroke = Black;
            Back.StrokeThickness = 2;
            LineToBack.Stroke = Black;
            LineToBack.StrokeThickness = 2;

            Turnable.Stroke = Gray;
            Turnable.StrokeThickness = 1;
            Turnable.Fill = Gray;

            Turnable.Height = 430;
            Turnable.Width = 430;

            Canvas.SetTop(Turnable, 35);
            Canvas.SetLeft(Turnable, 35);
            cvs_Draw.Children.Add(Turnable);

            foreach (OpenCvSharp.Point point in ListOfPointBody)
            {
                Line dot = new Line();
                dot.Stroke = White;
                dot.StrokeThickness = 2;
                Windows.Foundation.Point pDot, pTemp;
                pTemp.X = point.X;
                pTemp.Y = point.Y;
                pDot = scalePointX(pTemp);
                dot.X1 = pDot.X - 1;
                dot.X2 = pDot.X + 1;
                dot.Y1 = pDot.Y - 1;
                dot.Y2 = pDot.Y + 1;
                cvs_Draw.Children.Add(dot);
            }

            foreach (Windows.Foundation.Point point in ListOfPointScanWheels)
            {
                Line dot = new Line();
                dot.Stroke = Blue;
                dot.StrokeThickness = 2;
                Windows.Foundation.Point pDot;
                pDot = scalePointX(point);
                dot.X1 = pDot.X - 1;
                dot.X2 = pDot.X + 1;
                dot.Y1 = pDot.Y - 1;
                dot.Y2 = pDot.Y + 1;
                cvs_Draw.Children.Add(dot);
            }

            foreach (Windows.Foundation.Point point in ListOfNearPoint)
            {
                Line dot = new Line();
                dot.Stroke = Red;
                dot.StrokeThickness = 2;
                Windows.Foundation.Point pDot;
                pDot = scalePointX(point);
                dot.X1 = pDot.X - 1;
                dot.X2 = pDot.X + 1;
                dot.Y1 = pDot.Y - 1;
                dot.Y2 = pDot.Y + 1;
                cvs_Draw.Children.Add(dot);
            }

            foreach (Windows.Foundation.Point point in ListOfPointUpper)
            {
                Line dot = new Line();
                dot.Stroke = Black;
                dot.StrokeThickness = 2;
                Windows.Foundation.Point pDot;
                pDot = scalePointX(point);
                dot.X1 = pDot.X - 1;
                dot.X2 = pDot.X + 1;
                dot.Y1 = pDot.Y - 1;
                dot.Y2 = pDot.Y + 1;
                cvs_Draw.Children.Add(dot);
            }

            for (int i = 0; i < ListOfPointCarRect.Count; i++)
            {
                int EndIndex = 0;
                if (i == ListOfPointCarRect.Count - 1)
                {
                    EndIndex = 0;
                }
                else
                    EndIndex = i + 1;

                Windows.Foundation.Point CenterPoint = new Windows.Foundation.Point();
                CenterPoint.X = (ListOfPointCarRect[i].X + ListOfPointCarRect[EndIndex].X) / 2;
                CenterPoint.Y = (ListOfPointCarRect[i].Y + ListOfPointCarRect[EndIndex].Y) / 2;
                CenterPoint = scalePointX(CenterPoint);

                Car.Points.Add(scalePointX(ListOfPointCarRect[i]));

                TextBlock TxtLenOfLine = new TextBlock();
                TxtLenOfLine.Foreground = Blue;
                TxtLenOfLine.FontSize = 12;
                TxtLenOfLine.Width = 100;
                TxtLenOfLine.Text = dodaiDuongThang(ListOfPointCarRect[i].X, ListOfPointCarRect[i].Y, ListOfPointCarRect[EndIndex].X, ListOfPointCarRect[EndIndex].Y).ToString();
                Canvas.SetTop(TxtLenOfLine, CenterPoint.Y - 20);
                Canvas.SetLeft(TxtLenOfLine, CenterPoint.X - 20);
                cvs_Draw.Children.Add(TxtLenOfLine);

            }
            cvs_Draw.Children.Add(Car);

            for (int i = 0; i < ListOfPointWheels.Count; i++)
            {
                Windows.Foundation.Point WheelTxtPoint = new Windows.Foundation.Point();
                Wheel.Points.Add(scalePointX(ListOfPointWheels[i]));

                TextBlock TxtLenOfLine = new TextBlock();
                TxtLenOfLine.Foreground = Blue;
                TxtLenOfLine.FontSize = 12;
                TxtLenOfLine.Width = 100;
                TxtLenOfLine.Text = ListOfDistanceWheels[i].ToString();
                WheelTxtPoint.X = ListOfPointWheels[i].X;
                WheelTxtPoint.Y = ListOfPointWheels[i].Y;
                WheelTxtPoint = scalePointX(WheelTxtPoint);
                Canvas.SetTop(TxtLenOfLine, WheelTxtPoint.Y - 30);
                Canvas.SetLeft(TxtLenOfLine, WheelTxtPoint.X - 30);
                cvs_Draw.Children.Add(TxtLenOfLine);

            }
            cvs_Draw.Children.Add(Wheel);

            Windows.Foundation.Point BackA, BackB;
            BackA.X = ListOfPointCarRect[intIndexOfDuoiXe].X;
            BackA.Y = ListOfPointCarRect[intIndexOfDuoiXe].Y;
            BackA = scalePointX(BackA);
            if (intIndexOfDuoiXe + 1 > 3)
            {
                BackB.X = ListOfPointCarRect[0].X;
                BackB.Y = ListOfPointCarRect[0].Y;
            }
            else
            {
                BackB.X = ListOfPointCarRect[intIndexOfDuoiXe + 1].X;
                BackB.Y = ListOfPointCarRect[intIndexOfDuoiXe + 1].Y;
            }
            BackB = scalePointX(BackB);
            Back.X1 = BackA.X;
            Back.Y1 = BackA.Y;
            Back.X2 = BackB.X;
            Back.Y2 = BackB.Y;

            cvs_Draw.Children.Add(Back);

            btnStart.Content = "Re-Scan";
            btnStart.IsEnabled = true;
            if (IsRectangle(ListOfPointCarRect[0], ListOfPointCarRect[1], ListOfPointCarRect[2], ListOfPointCarRect[3]) && IsRectangle(ListOfPointWheels[0], ListOfPointWheels[1], ListOfPointWheels[2], ListOfPointWheels[3]))
            {
                btnConfirm.IsEnabled = true;
                btnConfirm_2.IsEnabled = true;
            }

            FinishScanTime = DateTime.Now;
            ulong TotalSeconds = (ulong)((FinishScanTime - StartScanTime).TotalSeconds);

            Timer.Text = TotalSeconds.ToString();
        }

        private void VeXe(List<Windows.Foundation.Point> ListOfPoint, Windows.Foundation.Point diemCham)
        {
            cvs_Draw.Children.Clear();
            SolidColorBrush Red = new SolidColorBrush();
            Red.Color = Colors.Red;
            SolidColorBrush Black = new SolidColorBrush();
            Black.Color = Colors.Black;
            Polygon Car = new Polygon();
            Line LineTiaNuoc = new Line();


            Car.Stroke = Red;
            Car.StrokeThickness = 2;
            LineTiaNuoc.Stroke = Black;
            LineTiaNuoc.StrokeThickness = 2;

            for (int i = 0; i < ListOfPoint.Count; i++)
            {
                Car.Points.Add(scalePointX(ListOfPointCarRect[i]));
            }
            cvs_Draw.Children.Add(Car);

            Windows.Foundation.Point LineChamA, LineChamB;
            LineChamA = tamPhun;
            LineChamA = scalePointX(LineChamA);

            LineChamB = diemCham;
            LineChamB = scalePointX(LineChamB);
            LineTiaNuoc.X1 = LineChamA.X;
            LineTiaNuoc.Y1 = LineChamA.Y;
            LineTiaNuoc.X2 = LineChamB.X;
            LineTiaNuoc.Y2 = LineChamB.Y;

            cvs_Draw.Children.Add(LineTiaNuoc);
        }

        private bool IsRectangle(Windows.Foundation.Point p1, Windows.Foundation.Point p2, Windows.Foundation.Point p3, Windows.Foundation.Point p4)
        {
            LineX LineAB = new LineX(p1, p2);
            LineX LineAD = new LineX(p1, p4);
            LineX LineBA = new LineX(p2, p1);
            LineX LineBC = new LineX(p2, p3);

            double GocABD = Math.Round(gocHaiDuongThang(LineAB, LineAD), MidpointRounding.AwayFromZero);
            if (GocABD > 180)
                GocABD = 360 - GocABD;
            double GocBAC = Math.Round(gocHaiDuongThang(LineBA, LineBC), MidpointRounding.AwayFromZero);
            if (GocBAC > 180)
                GocBAC = 360 - GocBAC;

            if ((GocABD != 90) || (GocBAC != 90))
                return false;

            if (GocABD != GocBAC)
                return false;


            double LenAC = dodaiDuongThang(p1.X, p1.Y, p3.X, p3.Y);
            double LenBD = dodaiDuongThang(p2.X, p2.Y, p4.X, p4.Y);
            if (LenAC != LenBD)
                return false;

            return true;
        }

        private void CapNhatToaDoXe(Point2f[] rRectPoint)
        {
            ListOfPointCarRect.Clear();
            foreach (Point2f carPoint in rRectPoint)
            {
                Windows.Foundation.Point PTemp;
                PTemp.X = (carPoint.X);
                PTemp.Y = (carPoint.Y);
                ListOfPointCarRect.Add(PTemp);
            }
            CapNhatAngleSay();
        }

        private void CapNhatAngleSay()
        {
            ListOfAngleSay.Clear();
            if (ListOfPointCarRect.Count != 4) 
                return;

            for (int i = 0; i < ListOfPointCarRect.Count; i++)
            {
                LineX duongCoSo = new LineX();
                duongCoSo.A = tamXoay;
                duongCoSo.B.X = tamXoay.X;
                duongCoSo.B.Y = tamXoay.Y - 100;
                LineX duongGoc = new LineX();
                duongGoc.A = tamXoay;
                duongGoc.B = ListOfPointCarRect[i];

                ListOfAngleSay.Add((int)gocHaiDuongThang(duongCoSo, duongGoc));
            }

            ListOfAngleSay.Sort();

            //nếu góc đoạn đầu nhỏ hơn đoạn sau nghĩa là đó là đuôi xe, 
            if ((360 - ListOfAngleSay[3] + ListOfAngleSay[0]) < (ListOfAngleSay[1] - ListOfAngleSay[0]))
            {
                ListOfAngleSay.Insert(0, -1 * (360 - ListOfAngleSay[3]));
                ListOfAngleSay.RemoveAt(ListOfAngleSay.Count - 1);
            }

            ListOfAngleSay.Insert(ListOfAngleSay.Count, 360);

        }


        private void CreateActionList()
        {
            ActionX Action = new ActionX();
            ListOfAction.Clear();
            List<ActionX> ListOfActionNocTruoc = new List<ActionX>();
            List<ActionX> ListOfActionNocSau = new List<ActionX>();
            List<ActionX> ListOfActionThanDuoi = new List<ActionX>();
            List<ActionX> ListOfActionDuoiXe = new List<ActionX>();
            List<ActionX> ListOfActionRua = new List<ActionX>();
            List<ActionX> ListOfActionBong = new List<ActionX>();            
            List<ActionX> ListOfActionSay = new List<ActionX>();

            List<Windows.Foundation.Point> ListOfPoint = ListOfPointCarRect;

            double gX = 0;

            for (int i = 0; i <= (360 / buocXoayHiRes); i++) // 
            {
                TiaX Tia = TiaNuoc2(ListOfPoint);
                double len = Tia.DoDaiTiaX;
                bool bNearWheel = false;

                foreach (double Angle in ListOfAngleWheels)
                {
                    double AngleMin = Angle - 7;
                    double AngleMax = Angle + 5;

                    if ((gX >= AngleMin) && (gX <=AngleMax))
                        bNearWheel = true;
                }

                Action.RunMode = 2;
                Action.GocMX = gX;
                Action.GocTP = Tia.GocTiaX;
                Action.ViTriPhunBot = 1;
                Action.NhanhPhunBot = 1;
                Action.Say = false;
                if ( bNearWheel)
                    Action.TocDo = tocdoBotBanhXe;
                else
                    Action.TocDo = tocdoBotThanXe;

                if (Tia.LineChamX == -1) // tại giao điểm tránh quá tốc trụ phun
                    Action.TocDo = tocdoBotDuoiXe;


                if ( (bNearWheel) || (i % 10 == 0))//10 độ mới ghi 1 lần
                    ListOfActionThanDuoi.Add(Action);

                gX += buocXoayHiRes;
                ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
            }
            
            gX = 0;
            for (int i = 0; i <= (360 / buocXoayHiRes); i++) // 
            {
                TiaX Tia = TiaNuoc2(ListOfPoint);
                double len = Tia.DoDaiTiaX;
                bool bNearBack = false;
                bool bNearFront = false;

                if (Tia.LineChamX == intIndexOfDuoiXe)
                    bNearBack = true;
                if (Tia.LineChamX == intIndexOfDauXe)
                    bNearFront = true;

                if (bNearBack)
                {
                    Action.RunMode = 2;
                    Action.GocMX = gX; ;
                    Action.GocTP = Tia.GocTiaX;
                    Action.ViTriPhunBot = 2;
                    Action.NhanhPhunBot = 1;
                    Action.Say = false;
                    Action.TocDo = tocdoBotDuoiXe;
                    ListOfActionDuoiXe.Add(Action);

                    //thêm cho phần nóc sau
                    Action.RunMode = 2;
                    Action.GocMX = gX; ;
                    Action.GocTP = Tia.GocTiaX;
                    Action.ViTriPhunBot = 8;
                    Action.NhanhPhunBot = 1;
                    Action.Say = false;
                    Action.TocDo = tocdoBotDuoiXe;
                    ListOfActionNocSau.Add(Action);
                }

                if (bNearFront)
                {
                    Action.RunMode = 2;
                    Action.GocMX = gX; ;
                    Action.GocTP = Tia.GocTiaX;
                    Action.ViTriPhunBot = 8;
                    Action.NhanhPhunBot = 1;
                    Action.Say = false;
                    Action.TocDo = tocdoBotDuoiXe;
                    ListOfActionNocTruoc.Add(Action);
                }


                gX += buocXoayHiRes;
                ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
            }

            gX = 0;
            for (int i = 0; i <= (360 / buocXoayHiRes); i++) // 
            {
                TiaX Tia = TiaNuoc2(ListOfPoint);
                double len = Tia.DoDaiTiaX;
                int intNearIndex = 0; //


                foreach (double Angle in ListOfAngleWheels)
                {
                    double AngleMin = Angle - 7;
                    double AngleMax = Angle + 5;

                    if ((gX >= AngleMin) && (gX <= AngleMax))
                        intNearIndex = 1;
                }
                if ((Tia.LineChamX == intIndexOfDuoiXe) || (Tia.LineChamX == intIndexOfDauXe) || (Tia.LineChamX == -1))
                    intNearIndex = 2;


                Action.RunMode = 2;
                Action.GocMX = gX;
                Action.GocTP = Tia.GocTiaX;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                if (i < 60)
                {
                    Action.GocSay1 = i;
                    Action.GocSay2 = i;
                }

                Action.ViTriPhunNuoc = 3; 
                Action.DoDai = (int)Tia.DoDaiTiaX;
                Action.Say = false;

                switch (intNearIndex)
                {
                    case 1: 
                        Action.TocDo = tocdoNuocBanhXe;
                        break;
                    case 2:
                        Action.TocDo = tocdoNuocDuoiXe;
                        break;
                    default:
                        Action.TocDo = tocdoNuocThanXe;
                        break;
                }

                if ((intNearIndex > 0) || (i % 5 == 0))
                    ListOfActionRua.Add(Action);

                gX += buocXoayHiRes;
                ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
            }

            gX = 0;
            Action.RunMode = 1;
            Action.GocTP = 45;
            Action.ViTriPhunBot = 0;
            Action.NhanhPhunBot = 0;
            Action.ViTriPhunNuoc = 0;
            Action.DoDai = 0;
            Action.Say = false;
            Action.GocSay1 = 67.5;
            Action.GocSay2 = 67.5;
            Action.TocDo = tocdoGiamToc;
            Action.GocMX = gX + 2;
            ListOfActionBong.Add(Action);

            Action.GocMX = gX;
            ListOfActionBong.Add(Action);

            gX = 360;
            Action.ViTriPhunBot = 8;
            Action.NhanhPhunBot = 2;
            Action.TocDo = tocdoBotBong;
            Action.GocMX = gX;
            ListOfActionBong.Add(Action);

            Action.ViTriPhunBot = 0;
            Action.NhanhPhunBot = 0;
            Action.TocDo = tocdoNuocBong;
            Action.GocMX = gX - 2;
            Action.ViTriPhunNuoc = 3; 
            Action.DoDai = 1500;
            ListOfActionBong.Add(Action);

            gX = 720;
            Action.GocMX = gX;
            ListOfActionBong.Add(Action);


            List<GocThanXe> ListGocSay = CarRectToAngle();
            for (int i = 0; i < 3; i++)
            {
                Action.RunMode = 1;
                Action.GocTP = 0;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.ViTriPhunNuoc = 0;
                Action.DoDai = 0;
                Action.Say = true;

                double gocHongSay = 0;
                switch (i)
                {
                    case 0:
                        gocHongSay = 67.5;
                        break;
                    case 1:
                        gocHongSay = 45;
                        break;
                    case 2:
                        gocHongSay = 24;
                        break;
                    default:
                        gocHongSay = 0;
                        break;
                }

                for (int j = 0; j < ListGocSay.Count ; j++) // 
                {
                    Action.GocMX = ListGocSay[j].Angle + (360 * i);
                    if ( (ListGocSay[j].LineIndex == intIndexOfDauXe) || (ListGocSay[j].LineIndex == intIndexOfDuoiXe))
                        Action.TocDo = tocdoDiChuyenSayCham;
                    else
                        Action.TocDo = tocdoDiChuyenSayNhanh;

                    Action.GocSay1 = gocHongSay;
                    Action.GocSay2 = gocHongSay;
                    Action.Delay = 12;

                    ListOfActionSay.Add(Action);
                }
            }
            double DistDauXe = dodaiDuongThang(tamPhun.X, tamPhun.Y, ListOfPointCarRect[intIndexOfDauXe].X, ListOfPointCarRect[intIndexOfDauXe].Y);
            double DistDuoiXe = dodaiDuongThang(tamPhun.X, tamPhun.Y, ListOfPointCarRect[intIndexOfDuoiXe].X, ListOfPointCarRect[intIndexOfDuoiXe].Y);

            if (DistDuoiXe < DistDauXe)
            {
                Action = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1];
                Action.RunMode = 1;
                Action.Delay = 1;
                ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionThanDuoi);

                for (int i = 0; i < ListOfActionDuoiXe.Count; i++)
                {
                    Action = ListOfActionDuoiXe[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX - (360 - Action.GocMX);
                    ListOfActionDuoiXe[i] = Action;
                }
                ListOfActionDuoiXe = SortX(ListOfActionDuoiXe);

                Action = ListOfActionDuoiXe[0];
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 1;
                Action.Say = false;
                Action.TocDo = tocdoXoayNextSlow;
                Action.Delay = 1;
                ListOfActionDuoiXe[0] = Action;

                ListOfActionDuoiXe = UpdateGiamToc(ListOfActionDuoiXe);

                Action = ListOfActionDuoiXe[ListOfActionDuoiXe.Count - 1];
                Action.RunMode = 1;
                Action.Delay = 1;
                ListOfActionDuoiXe[ListOfActionDuoiXe.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionDuoiXe);

                for (int i = 0; i < ListOfActionNocSau.Count; i++)
                {
                    Action = ListOfActionNocSau[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + Action.GocMX - 360;
                    ListOfActionNocSau[i] = Action;
                }

                ListOfAction.AddRange(ListOfActionNocSau);

                for (int i = 0; i < ListOfActionNocTruoc.Count; i++)
                {
                    Action = ListOfActionNocTruoc[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + Action.GocMX;
                    ListOfActionNocTruoc[i] = Action;
                }

                Action = ListOfActionNocTruoc[0];
                Action.TocDo = tocdoXoayNextSlow;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.RunMode = 1;
                ListOfAction.Add(Action);

                ListOfActionNocTruoc = UpdateGiamToc(ListOfActionNocTruoc);

                Action = ListOfActionNocTruoc[ListOfActionNocTruoc.Count - 1];
                Action.RunMode = 1;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.Delay = 1;
                ListOfActionNocTruoc[ListOfActionNocTruoc.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionNocTruoc);

                Action = ListOfAction[ListOfAction.Count - 1];
                Action.RunMode = 1;
                Action.TocDo = tocdoXoayNextSlow;
                Action.GocTP = ListOfActionRua[0].GocTP;
                Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.Delay = BotDelay;
                ListOfAction.Add(Action);
            }
            else
            {
                Action = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1];
                Action.RunMode = 1;
                Action.Delay = 1;
                ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionThanDuoi);

                for (int i = 0; i < ListOfActionNocTruoc.Count; i++)
                {
                    Action = ListOfActionNocTruoc[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX - (360 - Action.GocMX);
                    ListOfActionNocTruoc[i] = Action;
                }
                ListOfActionNocTruoc = SortX(ListOfActionNocTruoc);

                Action = ListOfActionNocTruoc[0];
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 1;
                Action.Say = false;
                Action.Delay = 1;
                ListOfActionNocTruoc[0] = Action;

                ListOfActionNocTruoc = UpdateGiamToc(ListOfActionNocTruoc);

                Action = ListOfActionNocTruoc[ListOfActionNocTruoc.Count - 1];
                Action.RunMode = 1;
                Action.Delay = 1;
                ListOfActionNocTruoc[ListOfActionNocTruoc.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionNocTruoc);

                for (int i = 0; i < ListOfActionNocSau.Count; i++)
                {
                    Action = ListOfActionNocSau[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + Action.GocMX;
                    ListOfActionNocSau[i] = Action;
                }

                Action = ListOfActionNocSau[0];
                Action.TocDo = tocdoXoayNextSlow;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.RunMode = 1;
                ListOfAction.Add(Action);

                ListOfActionNocSau = UpdateGiamToc(ListOfActionNocSau);

                Action = ListOfActionNocSau[ListOfActionNocSau.Count - 1];
                Action.RunMode = 1;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.Delay = 1;
                ListOfActionNocSau[ListOfActionNocSau.Count - 1] = Action;

                ListOfAction.AddRange(ListOfActionNocSau);

                for (int i = 0; i < ListOfActionDuoiXe.Count; i++)
                {
                    Action = ListOfActionDuoiXe[i];
                    Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + 360 - (360 - Action.GocMX);
                    ListOfActionDuoiXe[i] = Action;
                }
                ListOfActionDuoiXe = SortX(ListOfActionDuoiXe);
                ListOfAction.AddRange(ListOfActionDuoiXe); 

                Action = ListOfAction[ListOfAction.Count - 1];
                Action.RunMode = 1;
                Action.TocDo = tocdoXoayNextSlow;
                Action.GocTP = ListOfActionRua[0].GocTP;
                Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX;
                Action.ViTriPhunBot = 0;
                Action.NhanhPhunBot = 0;
                Action.Say = false;
                Action.Delay = BotDelay;
                ListOfAction.Add(Action);
            }

            for (int i = 0; i < ListOfActionRua.Count; i++)
            {
                Action = ListOfActionRua[i];
                Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + Action.GocMX;
                ListOfActionRua[i] = Action;
            }
            Action = ListOfActionRua[0];
            Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX;
            Action.Delay = 3;
            ListOfActionRua.Insert(0, Action);            

            ListOfActionRua = UpdateGiamToc(ListOfActionRua);

            ListOfAction.AddRange(ListOfActionRua);

            for (int i = 0; i < ListOfActionSay.Count; i++)
            {
                Action = ListOfActionSay[i];
                Action.GocMX = ListOfActionThanDuoi[ListOfActionThanDuoi.Count - 1].GocMX + 360 + Action.GocMX;
                ListOfActionSay[i] = Action;
            }
            ListOfAction.AddRange(ListOfActionSay);
        }

        private List<ActionX> UpdateGiamToc(List<ActionX> ListX)
        {
            int intMaxStep = 4;
            if (ListX.Count < intMaxStep)
                intMaxStep = ListX.Count;
            for (int i = 0; i < intMaxStep; i++)
            {
                ActionX Action = ListX[ListX.Count - (1 + i)];
                Action.TocDo = tocdoGiamToc;
                ListX[ListX.Count - (1 + i)] = Action;
            }

            return ListX;
        }

        private List<GocThanXe> CarRectToAngle()
        {
            List<GocThanXe> ListReturn = new List<GocThanXe>();

            List<Windows.Foundation.Point> ListOfPoint = ListOfPointCarRect;

            LineX duongCoSo = new LineX(tamXoay, tamXoay);
            duongCoSo.B.Y -= 4000; 

            double gX = 0;

            for (int i = 0; i <= (360 / buocXoayHiRes); i++) // 
            {
                LineX[] lines = new LineX[4];
                lines[0] = new LineX(ListOfPoint[0], ListOfPoint[1]);
                lines[1] = new LineX(ListOfPoint[1], ListOfPoint[2]);
                lines[2] = new LineX(ListOfPoint[2], ListOfPoint[3]);
                lines[3] = new LineX(ListOfPoint[3], ListOfPoint[0]);


                for (int j = 0; j < 4; j++)
                {
                    if (DoIntersect(duongCoSo.A, duongCoSo.B, lines[j].A, lines[j].B))
                    {
                        GocThanXe GTX = new GocThanXe();
                        GTX.Angle = gX;
                        GTX.LineIndex = j;
                        if ((ListReturn.Count == 0) || (ListReturn[ListReturn.Count - 1].LineIndex != j))
                            ListReturn.Add(GTX);
                        else
                            ListReturn[ListReturn.Count - 1] = GTX;

                    }
                }
                gX += buocXoayHiRes;
                ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
            }

            ListReturn.Sort(delegate (GocThanXe x, GocThanXe y)
            {
                return x.Angle.CompareTo(y.Angle);
            });

            return ListReturn;
        }

        private List<ActionX> SortX(List<ActionX> ListX)
        {
            ListX.Sort(delegate (ActionX x, ActionX y)
            {
                return x.GocMX.CompareTo(y.GocMX);
            });
            ListX.Reverse();

            return ListX;
        }

        private static int Orientation(Windows.Foundation.Point a, Windows.Foundation.Point b, Windows.Foundation.Point c)
        {
            double value = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);

            if (value == 0) return 0;  // colinear
            return (value > 0) ? 1 : 2; // clock or counterclock wise
        }

        public static bool DoIntersect(Windows.Foundation.Point p1, Windows.Foundation.Point q1, Windows.Foundation.Point p2, Windows.Foundation.Point q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4) return true;

            // Special Cases
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false;
        }

        private static bool OnSegment(Windows.Foundation.Point p, Windows.Foundation.Point q, Windows.Foundation.Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
            {
                return true;
            }

            return false;
        }

        private double TriangleArea(Windows.Foundation.Point A, Windows.Foundation.Point B, Windows.Foundation.Point C)
        {
            return Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) / 2.0f);
        }

        private bool IsPointInRectangle(Windows.Foundation.Point A, Windows.Foundation.Point B, Windows.Foundation.Point C, Windows.Foundation.Point D, Windows.Foundation.Point P)
        {
            double rectangleArea = TriangleArea(A, B, C) + TriangleArea(A, C, D);
            double sumOfTriangleArea = TriangleArea(A, B, P) + TriangleArea(B, C, P) + TriangleArea(C, D, P) + TriangleArea(D, A, P);
            return Math.Abs(rectangleArea - sumOfTriangleArea) < 0.001f; // Chấp nhận một mức độ sai số nhỏ
        }


        private void PointsToDauDuoiXe(List<Windows.Foundation.Point> ListOfPointUpper)
        {
            int DuoixeIndex1 = TimDuoiXe(ListOfPointCarRect.ToArray());
            Windows.Foundation.Point[] duoiXe1 = new Windows.Foundation.Point[2];
            Windows.Foundation.Point[] duoiXe2 = new Windows.Foundation.Point[2];

            duoiXe1[0] = ListOfPointCarRect[DuoixeIndex1];
            if (DuoixeIndex1 + 1 > 3)
                duoiXe1[1] = ListOfPointCarRect[0];
            else
                duoiXe1[1] = ListOfPointCarRect[DuoixeIndex1 + 1];

            int DuoixeIndex2 = DuoixeIndex1 + 2;
            if (DuoixeIndex2 > 3)
                DuoixeIndex2 = DuoixeIndex1 - 2;

            duoiXe2[0] = ListOfPointCarRect[DuoixeIndex2];
            if (DuoixeIndex2 + 1 > 3)
                duoiXe2[1] = ListOfPointCarRect[0];
            else
                duoiXe2[1] = ListOfPointCarRect[DuoixeIndex2 + 1];

            double nearPointDistanceLast1 = double.MaxValue;
            double nearPointDistanceLast2 = double.MaxValue;

            foreach (Windows.Foundation.Point point in ListOfPointUpper)
            {
                Windows.Foundation.Point PVuongGoc;
                PVuongGoc = TimDuongVuongGoc(duoiXe1, point);
                if ((PVuongGoc.X != 0) && (PVuongGoc.Y != 0))
                {
                    double distance = dodaiDuongThang(point.X, point.Y, PVuongGoc.X, PVuongGoc.Y);
                    if (distance < nearPointDistanceLast1)
                    {
                        nearPointDistanceLast1 = distance;
                    }
                }
            }

            foreach (Windows.Foundation.Point point in ListOfPointUpper)
            {
                Windows.Foundation.Point PVuongGoc;
                PVuongGoc = TimDuongVuongGoc(duoiXe2, point);
                if ((PVuongGoc.X != 0) && (PVuongGoc.Y != 0))
                {
                    double distance = dodaiDuongThang(point.X, point.Y, PVuongGoc.X, PVuongGoc.Y);
                    if (distance < nearPointDistanceLast2)
                    {
                        nearPointDistanceLast2 = distance;
                    }
                }
            }

            if (nearPointDistanceLast2 < nearPointDistanceLast1)
            {
                intIndexOfDuoiXe = DuoixeIndex2;
                intIndexOfDauXe = DuoixeIndex1;
            }
            else
            {
                intIndexOfDuoiXe = DuoixeIndex1;
                intIndexOfDauXe = DuoixeIndex2;
            }


        }

        private int TimDuoiXe(Windows.Foundation.Point[] rRectPoint)
        {
            int lineIndex = 0;
            double lineLenLast = double.MaxValue;

            for (int i = 0; i < rRectPoint.Length; i++)
            {
                double lineLen = 0;
                if (i == rRectPoint.Length - 1)
                {
                    lineLen = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, rRectPoint[0].X, rRectPoint[0].Y);
                }
                else
                {
                    lineLen = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, rRectPoint[i + 1].X, rRectPoint[i + 1].Y);
                }

                if (lineLen < lineLenLast)
                {
                    lineLenLast = lineLen;
                    lineIndex = i;
                }
            }

            return lineIndex;
        }

        private void PointsToBanhXe(List<Windows.Foundation.Point> ListOfPointScanWheels)
        {
            int HongxeIndex = TimHongXe(ListOfPointCarRect.ToArray());
            int Hongxe2Index = 0;
            if ((HongxeIndex + 2) > 3)
                Hongxe2Index = HongxeIndex - 2;
            else
                Hongxe2Index = HongxeIndex + 2;

            Windows.Foundation.Point[] Line1 = new Windows.Foundation.Point[2];
            Windows.Foundation.Point[] Line2 = new Windows.Foundation.Point[2];
            if (HongxeIndex == 3)
            {
                Line1[0] = ListOfPointCarRect[3];
                Line1[1] = ListOfPointCarRect[0];
            }
            else
            {
                Line1[0] = ListOfPointCarRect[HongxeIndex];
                Line1[1] = ListOfPointCarRect[HongxeIndex + 1];
            }

            if (Hongxe2Index == 3)
            {
                Line2[0] = ListOfPointCarRect[3];
                Line2[1] = ListOfPointCarRect[0];
            }
            else
            {
                Line2[0] = ListOfPointCarRect[Hongxe2Index];
                Line2[1] = ListOfPointCarRect[Hongxe2Index + 1];
            }

            TimBanhXe(Line1, Line2, ListOfPointScanWheels);
        }

        private int TimHongXe(Windows.Foundation.Point[] rRectPoint)
        {
            int lineIndex = 0;
            double lineLenLast = 0;
            double lineDistanceLast = double.MaxValue;

            for (int i = 0; i < rRectPoint.Length; i++)
            {
                double lineLen = 0;
                double lineDistance = 0;
                if (i == rRectPoint.Length - 1)
                {
                    lineLen = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, rRectPoint[0].X, rRectPoint[0].Y);
                    lineDistance = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, 0, 0);
                    lineDistance += dodaiDuongThang(rRectPoint[0].X, rRectPoint[0].Y, 0, 0);
                }
                else
                {
                    lineLen = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, rRectPoint[i + 1].X, rRectPoint[i + 1].Y);
                    lineDistance = dodaiDuongThang(rRectPoint[i].X, rRectPoint[i].Y, 0, 0);
                    lineDistance += dodaiDuongThang(rRectPoint[i + 1].X, rRectPoint[i + 1].Y, 0, 0);
                }

                if (lineLen > lineLenLast)
                {
                    lineLenLast = lineLen;
                    lineIndex = i;
                    lineDistanceLast = lineDistance;
                }
                else if (lineLen == lineLenLast)
                {
                    if (lineDistance < lineDistanceLast)
                        lineIndex = i;
                }
            }

            return lineIndex;
        }

        private void TimBanhXe(Windows.Foundation.Point[] LineHong1, Windows.Foundation.Point[] LineHong2, List<Windows.Foundation.Point> ListOfPointScanWheels)
        {
            ListOfPointWheels.Clear();
            ListOfNearPoint.Clear();
            ListOfDistanceWheels.Clear();

            double LenOfLine = dodaiDuongThang(LineHong1[0].X, LineHong1[0].Y, LineHong1[1].X, LineHong1[1].Y);
            Windows.Foundation.Point EndLine1, EndLine2;

            EndLine1 = TimDiemThuocDuongThang(LineHong1[0], LineHong1[1], LenOfLine / 3);
            EndLine2 = TimDiemThuocDuongThang(LineHong1[1], LineHong1[0], LenOfLine / 3);

            Windows.Foundation.Point[] Line1 = new Windows.Foundation.Point[2];
            Line1[0] = LineHong1[0];
            Line1[1] = EndLine1;
            Windows.Foundation.Point Top1Point = TrungTamBanhXe(Line1, ListOfPointScanWheels);
            ListOfDistanceWheels.Add(dodaiDuongThang(Top1Point.X, Top1Point.Y, LineHong1[0].X, LineHong1[0].Y));

            Windows.Foundation.Point Top2Point = TimDuongVuongGoc(LineHong2, Top1Point);//nối line qua hông bên để đánh dấu vị trí bánh xe
            ListOfDistanceWheels.Add(dodaiDuongThang(Top2Point.X, Top2Point.Y, LineHong2[1].X, LineHong2[1].Y));

            Windows.Foundation.Point[] Line2 = new Windows.Foundation.Point[2];
            Line2[0] = EndLine2;
            Line2[1] = LineHong1[1];
            Windows.Foundation.Point Bottom1Point = TrungTamBanhXe(Line2, ListOfPointScanWheels);
            ListOfDistanceWheels.Add(dodaiDuongThang(Bottom1Point.X, Bottom1Point.Y, LineHong1[1].X, LineHong1[1].Y));

            Windows.Foundation.Point Bottom2Point = TimDuongVuongGoc(LineHong2, Bottom1Point);
            ListOfDistanceWheels.Add(dodaiDuongThang(Bottom2Point.X, Bottom2Point.Y, LineHong2[0].X, LineHong2[0].Y));

            ListOfPointWheels.Add(Top1Point);
            ListOfPointWheels.Add(Top2Point);
            ListOfPointWheels.Add(Bottom2Point);
            ListOfPointWheels.Add(Bottom1Point);
        }

        private Windows.Foundation.Point TimDiemThuocDuongThang(Windows.Foundation.Point A, Windows.Foundation.Point B, double distanceAC)
        {
            Vector vectorAB = new Vector(B.X - A.X, B.Y - A.Y);
            double lengthAB = Math.Sqrt(vectorAB.X * vectorAB.X + vectorAB.Y * vectorAB.Y);
            Vector unitVectorAB = new Vector(vectorAB.X / lengthAB, vectorAB.Y / lengthAB);
            Windows.Foundation.Point C = new Windows.Foundation.Point(A.X + distanceAC * unitVectorAB.X, A.Y + distanceAC * unitVectorAB.Y);

            return C;
        }

        private Windows.Foundation.Point TimDuongVuongGoc(Windows.Foundation.Point[] Line, Windows.Foundation.Point CPoint)
        {
            Windows.Foundation.Point PointReturn;
            double k = (Line[1].Y - Line[0].Y) / (Line[1].X - Line[0].X);
            double b = Line[0].Y - (k * Line[0].X);

            double k2 = -1 / k;
            double b2 = CPoint.Y - (k2 * CPoint.X);

            double x = (b2 - b) / (k - k2);
            double y = k * x + b;

            double dodaiA = dodaiDuongThang(Line[0].X, Line[0].Y, Line[1].X, Line[1].Y);
            double dodaiB1 = dodaiDuongThang(x, y, Line[0].X, Line[0].Y);
            double dodaiB2 = dodaiDuongThang(x, y, Line[1].X, Line[1].Y);

            if ((dodaiB1 < dodaiA) && (dodaiB2 < dodaiA))// điểm vuông góc nằm trên đường thẳng                
            {
                PointReturn.X = x;
                PointReturn.Y = y;
            }
            else
            {
                PointReturn.X = 0;
                PointReturn.Y = 0;
            }

            return PointReturn;
        }

        private Windows.Foundation.Point TrungTamBanhXe(Windows.Foundation.Point[] Line, List<Windows.Foundation.Point> ListOfPointScanWheels)
        {
            Windows.Foundation.Point PointReturn;

            List<Windows.Foundation.Point> pointsNearLine = new List<Windows.Foundation.Point>();
            List<Windows.Foundation.Point> PerpendicularPoint = new List<Windows.Foundation.Point>();// điểm vuông góc với đường thẳng

            foreach (Windows.Foundation.Point point in ListOfPointScanWheels)
            {
                Windows.Foundation.Point PVuongGoc;
                PVuongGoc = TimDuongVuongGoc(Line, point);
                if ((PVuongGoc.X != 0) && (PVuongGoc.Y != 0))
                {
                    double distance = dodaiDuongThang(point.X, point.Y, PVuongGoc.X, PVuongGoc.Y);
                    if (distance < WheelPointThreshold)
                    {
                        pointsNearLine.Add(point);
                        PerpendicularPoint.Add(PVuongGoc);
                        ListOfNearPoint.Add(point); 
                    }
                }

            }

            Windows.Foundation.Point closestPoint;
            Windows.Foundation.Point farthestPoint;
            double closestDistance = double.MaxValue;
            double farthestDistance = double.MinValue;

            foreach (Windows.Foundation.Point point in PerpendicularPoint)
            {
                double distance = Math.Sqrt(Math.Pow(point.X - Line[0].X, 2) + Math.Pow(point.Y - Line[0].Y, 2));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint.X = point.X;
                    closestPoint.Y = point.Y;
                }
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestPoint.X = point.X;
                    farthestPoint.Y = point.Y;
                }
            }

            PointReturn.X = (closestPoint.X + farthestPoint.X) / 2;
            PointReturn.Y = (closestPoint.Y + farthestPoint.Y) / 2;

            return PointReturn;

        }


        private Point2f DoiToaDoXe(Point2f rRectPoint)
        {
            Point2f pReturn;
            pReturn.X = rRectPoint.X;
            pReturn.Y = rRectPoint.Y;

            return pReturn;
        }

        private Windows.Foundation.Point Rotate(Windows.Foundation.Point p, double goc, Windows.Foundation.Point origin)
        {
            Windows.Foundation.Point rotated;
            //làm tròn toạ độ để khi tìm đường vuông góc không bị tràn dữ liệu;
            rotated.X = Math.Round((p.X - origin.X) * Math.Cos(-goc) - (p.Y - origin.Y) * Math.Sin(-goc) + origin.X, 0, MidpointRounding.AwayFromZero);
            rotated.Y = Math.Round((p.X - origin.X) * Math.Sin(-goc) + (p.Y - origin.Y) * Math.Cos(-goc) + origin.Y, 0, MidpointRounding.AwayFromZero);
            return rotated;
        }

        private List<Windows.Foundation.Point> RotateList(double buocXoay, List<Windows.Foundation.Point> ListOfPoint)
        {
            double theta = buocXoay * Math.PI / 180;
            for (int j = 0; j < ListOfPoint.Count; j++)
            {
                ListOfPoint[j] = Rotate(ListOfPoint[j], theta, tamXoay);
            }

            return ListOfPoint;
        }


        private Windows.Foundation.Point scalePointX(Windows.Foundation.Point xPoint)
        {
            Windows.Foundation.Point pReturn;
            pReturn.X = Math.Round(xPoint.X / 12, 0, MidpointRounding.AwayFromZero);
            pReturn.Y = 500 - Math.Round(xPoint.Y / 12, 0, MidpointRounding.AwayFromZero);

            return pReturn;
        }

        private Windows.Foundation.Point scalePointX2(OpenCvSharp.Point2f xPoint)
        {
            Windows.Foundation.Point pReturn;
            pReturn.X = (int)xPoint.X / 12;
            pReturn.Y = (int)xPoint.Y / 12;

            return pReturn;
        }

        private Int32 convertX(double xValue)
        {
            return Convert.ToInt32(Math.Round(xValue, 0, MidpointRounding.AwayFromZero));
        }

        private double dodaiDuongThang(double x1, double y1, double x2, double y2)
        {
            return Math.Round(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)), 0);
        }
        private double gocHaiDuongThang(LineX l1, LineX l2)
        {
            double angle1 = Math.Atan2(l1.B.Y - l1.A.Y, l1.B.X - l1.A.X);
            double angle2 = Math.Atan2(l2.B.Y - l2.A.Y, l2.B.X - l2.A.X);
            double angleBetween = angle2 - angle1;
            if (angleBetween < 0)
            {
                angleBetween += 2 * Math.PI;
            }
            return angleBetween * 180 / Math.PI;
        }

        private void TimHocBanhXe()
        {
            List<Windows.Foundation.Point> ListOfPoint = ListOfPointCarRect;
            List<Windows.Foundation.Point> ListOfWheels = ListOfPointWheels;

            double gX = 0;

            ListOfAngleWheels.Clear();

            double[] DiemChamLen = new double[4];
            for (int i = 0; i < DiemChamLen.Length; i++)
            {
                DiemChamLen[i] = double.MaxValue;
                ListOfAngleWheels.Add(0);//reset data
            }

            for (int i = 0; i < (360 / buocXoayHiRes); i++) // 
            {

                DiemChamX diemcham = timDiemCham2(ListOfPoint);
                for (int j = 0; j < ListOfWheels.Count; j++)
                {
                    double Len = dodaiDuongThang(diemcham.DiemCham.X, diemcham.DiemCham.Y, ListOfWheels[j].X, ListOfWheels[j].Y);
                    if (DiemChamLen[j] > Len)
                    {
                        DiemChamLen[j] = Len;
                        ListOfAngleWheels[j] = gX;

                    }
                }
                gX += buocXoayHiRes;

                ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
                ListOfWheels = RotateList(buocXoayHiRes, ListOfWheels);
            }

        }

        private DiemChamX timDiemCham()
        {
            LineX[] lines = new LineX[4];
            lines[0] = new LineX(ListOfPointCarRect[0], ListOfPointCarRect[1]);
            lines[1] = new LineX(ListOfPointCarRect[1], ListOfPointCarRect[2]);
            lines[2] = new LineX(ListOfPointCarRect[2], ListOfPointCarRect[3]);
            lines[3] = new LineX(ListOfPointCarRect[3], ListOfPointCarRect[0]);

            Windows.Foundation.Point[] corners = new Windows.Foundation.Point[4];
            corners[0] = ListOfPointCarRect[0];
            corners[1] = ListOfPointCarRect[1];
            corners[2] = ListOfPointCarRect[2];
            corners[3] = ListOfPointCarRect[3];

            DiemChamX pReturn;
            double dodaiTiaVuongGoc_last = 0;
            double dodaiTiaDinh_last = 0;
            bool timThayVuongGoc = false;
            pReturn.DiemCham.X = 0;
            pReturn.DiemCham.Y = 0;
            pReturn.LineCham = -1;
            for (int i = 0; i < 4; i++)
            {
                double k = (lines[i].B.Y - lines[i].A.Y) / (lines[i].B.X - lines[i].A.X);
                double b = lines[i].A.Y - (k * lines[i].A.X);

                double k2 = -1 / k;
                double b2 = tamPhun.Y - (k2 * tamPhun.X);

                double x = Math.Round((b2 - b) / (k - k2), 0);
                double y = Math.Round(k * x + b, 0);

                double dodaiA = dodaiDuongThang(lines[i].A.X, lines[i].A.Y, lines[i].B.X, lines[i].B.Y);
                double dodaiB1 = dodaiDuongThang(x, y, lines[i].A.X, lines[i].A.Y);
                double dodaiB2 = dodaiDuongThang(x, y, lines[i].B.X, lines[i].B.Y);
                double dodaiTiaVuongGoc = dodaiDuongThang(tamPhun.X, tamPhun.Y, x, y);

                if ((dodaiB1 < dodaiA) && (dodaiB2 < dodaiA))                
                {
                    timThayVuongGoc = true;
                    if ((dodaiTiaVuongGoc_last == 0) || (dodaiTiaVuongGoc_last > dodaiTiaVuongGoc))
                    {
                        pReturn.DiemCham.X = x;
                        pReturn.DiemCham.Y = y;
                        pReturn.LineCham = i;
                        dodaiTiaVuongGoc_last = dodaiTiaVuongGoc;
                    }
                }
            }

            if (!timThayVuongGoc)
            {
                for (int i2 = 0; i2 < 4; i2++)
                {
                    double dodaiTiaDinh = dodaiDuongThang(tamPhun.X, tamPhun.Y, corners[i2].X, corners[i2].Y);
                    if ((dodaiTiaDinh < dodaiTiaDinh_last) || (dodaiTiaDinh_last == 0))
                    {
                        pReturn.DiemCham.X = corners[i2].X;
                        pReturn.DiemCham.Y = corners[i2].Y;
                        dodaiTiaDinh_last = dodaiTiaDinh;
                    }

                }
            }

            return pReturn;
        }

        private DiemChamX timDiemCham2(List<Windows.Foundation.Point> ListOfPoint)
        {
            LineX[] lines = new LineX[4];
            lines[0] = new LineX(ListOfPoint[0], ListOfPoint[1]);
            lines[1] = new LineX(ListOfPoint[1], ListOfPoint[2]);
            lines[2] = new LineX(ListOfPoint[2], ListOfPoint[3]);
            lines[3] = new LineX(ListOfPoint[3], ListOfPoint[0]);

            Windows.Foundation.Point[] corners = new Windows.Foundation.Point[4];
            corners[0] = ListOfPoint[0];
            corners[1] = ListOfPoint[1];
            corners[2] = ListOfPoint[2];
            corners[3] = ListOfPoint[3];

            DiemChamX pReturn;
            double dodaiTiaVuongGoc_last = double.MaxValue;
            double dodaiTiaDinh_last = 0;
            bool timThayVuongGoc = false;
            pReturn.DiemCham.X = 0;
            pReturn.DiemCham.Y = 0;
            pReturn.LineCham = -1;
            for (int i = 0; i < 4; i++)
            {
                double k = (lines[i].B.Y - lines[i].A.Y) / (lines[i].B.X - lines[i].A.X);
                double b = lines[i].A.Y - (k * lines[i].A.X);

                double k2 = -1 / k;
                double b2 = tamPhun.Y - (k2 * tamPhun.X);

                double x = Math.Round((b2 - b) / (k - k2), 0);
                double y = Math.Round(k * x + b, 0);

                double dodaiA = dodaiDuongThang(lines[i].A.X, lines[i].A.Y, lines[i].B.X, lines[i].B.Y);
                double dodaiB1 = dodaiDuongThang(x, y, lines[i].A.X, lines[i].A.Y);
                double dodaiB2 = dodaiDuongThang(x, y, lines[i].B.X, lines[i].B.Y);
                double dodaiTiaVuongGoc = dodaiDuongThang(tamPhun.X, tamPhun.Y, x, y);

                if ((dodaiB1 < dodaiA) && (dodaiB2 < dodaiA) && (dodaiTiaVuongGoc < dodaiTiaVuongGoc_last))// điểm vuông góc nằm trên đường thẳng                
                {
                    LineX LineDoiDien = new LineX();
                    if (i > 1)
                        LineDoiDien = lines[i - 2];
                    else
                        LineDoiDien = lines[i + 2];

                    double LenLineChon = dodaiDuongThang(tamPhun.X, tamPhun.Y, lines[i].A.X, lines[i].A.Y) + dodaiDuongThang(tamPhun.X, tamPhun.Y, lines[i].B.X, lines[i].B.Y);
                    double LenLineDoiDien = dodaiDuongThang(tamPhun.X, tamPhun.Y, LineDoiDien.A.X, LineDoiDien.A.Y) + dodaiDuongThang(tamPhun.X, tamPhun.Y, LineDoiDien.B.X, LineDoiDien.B.Y);

                    if (LenLineChon < LenLineDoiDien)
                    {
                        timThayVuongGoc = true;
                        pReturn.DiemCham.X = x;
                        pReturn.DiemCham.Y = y;
                        pReturn.LineCham = i;
                        dodaiTiaVuongGoc_last = dodaiTiaVuongGoc;
                    }

                }
            }

            if (!timThayVuongGoc)
            {
                for (int i2 = 0; i2 < 4; i2++)
                {
                    double dodaiTiaDinh = dodaiDuongThang(tamPhun.X, tamPhun.Y, corners[i2].X, corners[i2].Y);
                    if ((dodaiTiaDinh < dodaiTiaDinh_last) || (dodaiTiaDinh_last == 0))
                    {
                        pReturn.DiemCham.X = corners[i2].X;
                        pReturn.DiemCham.Y = corners[i2].Y;
                        pReturn.LineCham = -1;
                        dodaiTiaDinh_last = dodaiTiaDinh;
                    }

                }
            }

            return pReturn;
        }

        private Windows.Foundation.Point timDiemChamLine(LineX line)
        {
            Windows.Foundation.Point pReturn;
            pReturn.X = 0;
            pReturn.Y = 0;

            double k = (line.B.Y - line.A.Y) / (line.B.X - line.A.X);
            double b = line.A.Y - (k * line.A.X);

            double k2 = -1 / k;
            double b2 = tamPhun.Y - (k2 * tamPhun.X);

            double x = Math.Round((b2 - b) / (k - k2), 0);
            double y = Math.Round(k * x + b, 0);

            double dodaiA = dodaiDuongThang(line.A.X, line.A.Y, line.B.X, line.B.Y);
            double dodaiB1 = dodaiDuongThang(x, y, line.A.X, line.A.Y);
            double dodaiB2 = dodaiDuongThang(x, y, line.B.X, line.B.Y);
            double dodaiTiaVuongGoc = dodaiDuongThang(tamPhun.X, tamPhun.Y, x, y);

            if ((dodaiB1 < dodaiA) && (dodaiB2 < dodaiA))                
            {
                pReturn.X = x;
                pReturn.Y = y;
            }

            return pReturn;
        }

        private TiaX TiaNuoc()
        {
            TiaX tiaReturn;
            DiemChamX diemcham = timDiemCham();
            tiaReturn.LineChamX = diemcham.LineCham;

            Windows.Foundation.Point diemCoso = new Windows.Foundation.Point(tamPhun.X, tamPhun.Y - 100);
            LineX duongCoso = new LineX(tamPhun, diemCoso);
            LineX duongTiaNuoc = new LineX(tamPhun, diemcham.DiemCham);
            tiaReturn.GocTiaX = Math.Round(gocHaiDuongThang(duongCoso, duongTiaNuoc), 1, MidpointRounding.AwayFromZero);
            tiaReturn.DoDaiTiaX = dodaiDuongThang(duongTiaNuoc.A.X, duongTiaNuoc.A.Y, duongTiaNuoc.B.X, duongTiaNuoc.B.Y);

            if (tiaReturn.GocTiaX > 180)
                tiaReturn.GocTiaX = 360 - tiaReturn.GocTiaX; 

            return tiaReturn;

        }

        private TiaX TiaNuoc2(List<Windows.Foundation.Point> ListOfPoint)
        {
            TiaX tiaReturn;
            DiemChamX diemcham = timDiemCham2(ListOfPoint);
            tiaReturn.LineChamX = diemcham.LineCham;

            Windows.Foundation.Point diemCoso = new Windows.Foundation.Point(tamPhun.X, tamPhun.Y - 100);
            LineX duongCoso = new LineX(tamPhun, diemCoso);
            LineX duongTiaNuoc = new LineX(tamPhun, diemcham.DiemCham);
            tiaReturn.GocTiaX = Math.Round(gocHaiDuongThang(duongCoso, duongTiaNuoc), 1, MidpointRounding.AwayFromZero);
            tiaReturn.DoDaiTiaX = dodaiDuongThang(duongTiaNuoc.A.X, duongTiaNuoc.A.Y, duongTiaNuoc.B.X, duongTiaNuoc.B.Y);

            if (tiaReturn.GocTiaX > 180)
                tiaReturn.GocTiaX = 360 - tiaReturn.GocTiaX; 

            return tiaReturn;

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            btnConfirm.Content = "Washing A...";
            btnConfirm.IsEnabled = false;
            btnConfirm_2.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnLoadCSV.IsEnabled = false;


            intPLCStatus = 400;
            intIndexOfListWriteMX = 0;
            intIndexOfListWriteSay1 = 0;
            intIndexOfListWriteSay2 = 0;
            CreateData2();
            SendPosDataForWash();

        }

        private void btnConfirm2_Click(object sender, RoutedEventArgs e)
        {
            btnConfirm_2.Content = "Washing B...";
            btnConfirm.IsEnabled = false;
            btnConfirm_2.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnLoadCSV.IsEnabled = false;

            bXeCao = true;

            intPLCStatus = 400;
            intIndexOfListWriteMX = 0;
            intIndexOfListWriteSay1 = 0;
            intIndexOfListWriteSay2 = 0;
            CreateData2();
            SendPosDataForWash();
        }

        private void WashDone()
        {
            FinishWashTime = DateTime.Now;

            btnConfirm.Content = "Confirm 1";
            btnConfirm_2.Content = "Confirm 2";
            btnConfirm.IsEnabled = false;
            btnConfirm_2.IsEnabled = false;
            btnReWash.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStart.Content = "Scan";
            btnLoadCSV.IsEnabled = true;

            ulong TotalSeconds = (ulong)(((FinishScanTime - StartScanTime) + (FinishWashTime - StartWashTime)).TotalSeconds);

            Timer.Text = TotalSeconds.ToString();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            List<Windows.Foundation.Point> ListOfPoint = ListOfPointCarRect;

            gocXoay += buocXoayHiRes;
            ListOfPoint = RotateList(buocXoayHiRes, ListOfPoint);
            DiemChamX diemCham = timDiemCham2(ListOfPoint);
            VeXe(ListOfPoint, diemCham.DiemCham);
        }

        private void btnReWash_Click(object sender, RoutedEventArgs e)
        {
            intPLCStatus = 480;
            SendStartWash();
        }

        private async void btnLoadCSV_Click(object sender, RoutedEventArgs e)
        {
            await PickAndProcessFileAsync();
        }


        public async Task PickAndProcessFileAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".csv");


            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                await ReadCsvFileAsync(file);
            }

        }

        public async Task ReadCsvFileAsync(StorageFile file)
        {
            StartScanTime = DateTime.Now;

            IList<string> lines = await FileIO.ReadLinesAsync(file);

            ListOfScanBody.Clear();
            ListOfScanWheels.Clear();
            ListOfScanUpper.Clear();

            foreach (var line in lines)
            {
                string[] lineData = line.Split(',');
                if (lineData.Length == 4)//
                {
                    ScanX ScanData = new ScanX();
                    ScanData.Alpha = Convert.ToInt32(lineData[0]);
                    ScanData.Lida1Dist = Convert.ToInt32(lineData[1]);
                    ListOfScanBody.Add(ScanData);

                    ScanData.Lida1Dist = Convert.ToInt32(lineData[2]);
                    ListOfScanWheels.Add(ScanData);

                    ScanData.Lida1Dist = Convert.ToInt32(lineData[3]);
                    ListOfScanUpper.Add(ScanData);
                }

            }

            ScanToBorder();
        }



    }
}
