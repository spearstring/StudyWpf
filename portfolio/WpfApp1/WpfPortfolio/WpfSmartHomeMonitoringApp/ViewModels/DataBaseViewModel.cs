using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using WpfSmartHomeMonitoringApp.Helpers;

namespace WpfSmartHomeMonitoringApp.ViewModels
{
    public class DataBaseViewModel : Screen
    {
        private string brokerUrl;
        private string topic;
        private string connString;
        private bool isConnected;
        private string dbLog;

        public string BrokerUrl
        {
            get { return brokerUrl; }
            set
            {
                brokerUrl = value;
                NotifyOfPropertyChange(() => BrokerUrl);
            }
        }

        public string Topic
        {
            get { return topic; }
            set
            {
                topic = value;
                NotifyOfPropertyChange(() => Topic);
            }
        }

        public string ConnString
        {
            get { return connString; }
            set
            {
                connString = value;
                NotifyOfPropertyChange(() => ConnString);
            }
        }

        public bool IsConnected
        {
            get => isConnected; set
            {
                isConnected = value;
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        public string DbLog
        {
            get => dbLog; 
            set
            {
                dbLog = value;
                NotifyOfPropertyChange(() => DbLog);
            }
        }

        public DataBaseViewModel()
        {
            BrokerUrl = Commons.BROKERHOST = "210.119.12.68";   // MQTT Broker Ip 설정
            Topic = Commons.PUB_TOPIC = "home/device/#";
            ConnString = Commons.CONNSTRING = "Data Source=PC01;Initial Catalog=OpenApiLab;Integrated Security=True";

            if (Commons.IS_CONNECT)
            {
                IsConnected = true;
            }
        }

        /// <summary>
        /// DB연결 + MQTT Broker 접속
        /// </summary>
        public void ConnectDb()
        {
            if (IsConnected)
            {
                Commons.MQTT_CLIENT = new MqttClient(BrokerUrl);

                try
                {
                    if (Commons.MQTT_CLIENT.IsConnected != true)
                    {
                        Commons.MQTT_CLIENT.MqttMsgPublishReceived += MQTT_CLIENT_MqttMsgPublishReceived;
                        Commons.MQTT_CLIENT.Connect("MONITOR");
                        Commons.MQTT_CLIENT.Subscribe(new string[] { Commons.PUB_TOPIC },
                            new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                        UpdateText(">>> MQTT Broker Connected");
                        IsConnected = Commons.IS_CONNECT = true;
                    }
                }
                catch (Exception ex)
                {
                    UpdateText($">>> Connention Error! {ex.Message}");

                }
            }
            else    // 접속끄기
            {
                try
                {
                    if (Commons.MQTT_CLIENT.IsConnected)
                    {
                        Commons.MQTT_CLIENT.MqttMsgPublishReceived -= MQTT_CLIENT_MqttMsgPublishReceived;   // 이벤트 연결 삭제
                        Commons.MQTT_CLIENT.Disconnect();
                        UpdateText(">>> MQTT Broker Disconnected...");
                        IsConnected = Commons.IS_CONNECT = false;
                    }
                }
                catch (Exception ex)
                {
                    UpdateText($">>> Connention Error! {ex.Message}");
                }
            }
        }

        private void UpdateText(string message)
        {
            DbLog += $"{message}\n";
        }

        /// <summary>
        /// Subscribe한 메시지 처리해주는 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MQTT_CLIENT_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            UpdateText(message);    // 센서데이터 출력
            setDataBase(message, e.Topic);   // DB에 저장
        }

        private void setDataBase(string message, string topic)
        {
            var currDatas = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

            Debug.WriteLine("currDatas");

            using (SqlConnection conn = new SqlConnection(Commons.CONNSTRING))
            {
                conn.Open();
                string strInQuery = @"INSERT INTO TblSmartHome
                                                   (DevId
                                                   , CurrTime
                                                   , Temp
                                                   , Humid)
                                             VALUES
                                                   (@DevId
                                                   , @CurrTime
                                                   , @Temp
                                                   , @Humid)";

                try
                {
                    SqlCommand cmd = new SqlCommand(strInQuery, conn);
                    SqlParameter parmDevId = new SqlParameter("@DevId", currDatas["DevId"]);
                    SqlParameter parmCurrTime = new SqlParameter("@CurrTime", DateTime.Parse(currDatas["CurrTime"]));   // 날짜형 변환필요!!
                    SqlParameter parmTemp = new SqlParameter("@Temp", currDatas["Temp"]);
                    SqlParameter parmHumid = new SqlParameter("@Humid", currDatas["Humid"]);

                    cmd.Parameters.Add(parmDevId);
                    cmd.Parameters.Add(parmCurrTime);
                    cmd.Parameters.Add(parmTemp);
                    cmd.Parameters.Add(parmHumid);

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        UpdateText(">>> DB Inserted.");     // 저장 성공
                    }
                    else
                    {
                        UpdateText(">>> DB Failed!!!!!");   // 저장 실패
                    }
                }
                catch (Exception ex)
                {
                    UpdateText($">>> DB Error! {ex.Message}");  // 예외
                }
            }   //  conn.Close () 불필요
        }
    }
}
