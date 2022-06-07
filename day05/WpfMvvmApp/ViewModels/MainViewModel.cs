using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfMvvmApp.Helpers;
using WpfMvvmApp.Models;

namespace WpfMvvmApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // View에서 사용하기 위해 만든 멤버변수
        private string inFirstName;
        private string inLastName;
        private string inEmail;
        private DateTime inDate;

        private string outFirstName;
        private string outLastName;
        private string outEmail;
        private string outDate;

        private string outAdult;
        private string outBirthday;

        // 실제 사용하는 속성(Property)
        public string InFirstName
        {
            get { return inFirstName; }
            set
            {
                inFirstName = value;
                RaiseProperthChanged("InFirstName");     // 값이 바뀜 공지!
            }
        }

        public string InLastName
        {
            get { return inLastName; }
            set
            {
                inLastName = value;
                RaiseProperthChanged("InLastName");     // 값이 바뀜 공지!
            }
        }

        public string InEmail
        { 
            get { return inEmail; }
            set
            {
                inEmail = value;
                RaiseProperthChanged("InEmail");     // 값이 바뀜 공지!
            }
        }

        public DateTime InDate
        {
            get { return inDate; }
            set
            {
                inDate = value;
                RaiseProperthChanged("inDate");     // 값이 바뀜 공지!
            }
        }

        public string OutFirstName
        {
            get { return outFirstName; }
            set
            {
                outFirstName = value;
                RaiseProperthChanged("OutFirstName");     // 값이 바뀜 공지!
            }
        }

        public string OutLastName
        {
            get { return outLastName; }
            set
            {
                outLastName = value;
                RaiseProperthChanged("OutLastName");     // 값이 바뀜 공지!
            }
        }

        public string OutEmail
        {
            get { return outEmail; }
            set
            {
                outEmail = value;
                RaiseProperthChanged("OutEmail");     // 값이 바뀜 공지!
            }
        }

        public string OutDate
        {
            get { return outDate; }
            set
            {
                outDate = value;
                RaiseProperthChanged("OutDate");     // 값이 바뀜 공지!
            }
        }

        public string OutAdult
        {
            get { return outAdult; }
            set
            {
                outAdult = value;
                RaiseProperthChanged("OutAdult");     // 값이 바뀜 공지!
            }
        }

        public string OutBirthday
        {
            get { return outBirthday; }
            set
            {
                outBirthday = value;
                RaiseProperthChanged("OutBirthday");     // 값이 바뀜 공지!
            }
        }

        // 값이 전부 적용되어 버튼을 활성화 하기 위한 명령
        private ICommand proceedCommand;
        public ICommand ProceedCommand
        {
            get { return proceedCommand ?? (
                    proceedCommand = new RelayCommand<object> (
                        o => proceed(), o => !string.IsNullOrEmpty(inFirstName) &&
                                            !string.IsNullOrEmpty(inLastName) &&
                                            !string.IsNullOrEmpty(inEmail) &&
                                            !string.IsNullOrEmpty(inDate.ToString())
                        )); 
            }
        }

        // 버튼 클릭시 일어나는 실제 명령
        private async void proceed()
        {
            try
            {
                Person person = new Person(inFirstName, inLastName, inEmail, inDate);

                await Task.Run(() => OutFirstName = person.FirstName);
                await Task.Run(() => OutLastName = person.LastName);
                await Task.Run(() => OutEmail = person.Email);
                await Task.Run(() => OutDate = person.Date.ToString("yyyy-MM-dd"));
                await Task.Run(() => OutAdult = person.IsAdult.ToString());
                await Task.Run(() => OutBirthday = person.IsBirthday.ToString());
            }
            catch (Exception ex)
            {
                Commons.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        public MainViewModel()
        {
            this.inDate = DateTime.Parse("1990-01-01");
        }
    }
}
