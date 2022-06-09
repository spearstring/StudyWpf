using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;

namespace WpfSmartHomeMonitoringApp.Helpers
{
    public class Commons
    {
        public static string BROKERHOST { get; set; }
        public static string PUB_TOPIC { get; set; }
        public static MqttClient MQTT_CLIENT { get; set; }
        public static string CONNSTRING { get; set; }
        public static bool IS_CONNECT { get; set; }

        /// <summary>
        /// 메트로 UI용 메시지박스 실행 메서드
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static async Task<MessageDialogResult> ShowMessageAsync(
            string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, message, style, null);
        }

        internal static void ShowMessageAsync(string v)
        {
            throw new NotImplementedException();
        }
    }
}
