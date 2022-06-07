using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMvvmApp.Helpers
{
    public class Commons
    {
        /// <summary>
        /// 이메일 검증
        /// </summary>
        /// <param name="email"></param>
        /// <returns>true 이메일주소형태 아님</returns>
        public static bool IsvalidEmail(string email)
        {
            return Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        }

        /// <summary>
        /// 나이계산
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int CalcAge(DateTime value)
        {
            int middle;
            if (DateTime.Now.Month < value.Month || DateTime.Now.Month == value.Month || DateTime.Now.Day < value.Day)
            {
                middle = DateTime.Now.Year - value.Year -1;
            }
            else
            { 
                middle = DateTime.Now.Year - value.Year;
            }

            return middle;
        }

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
    }
}
