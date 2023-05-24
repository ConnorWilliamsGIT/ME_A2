using MECHENG_313_A2.Tasks;
using MECHENG_313_A2.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MECHENG_313_A2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnTask2Clicked(object sender, EventArgs e)
        {
            IController controller = new Task2();
            TaskPage taskPage = new TaskPage(controller);
            Application.Current.MainPage = taskPage;
        }

        private async void OnTask3Clicked(object sender, EventArgs e)
        {
            IController controller = new Task3();
            TaskPage taskPage = new TaskPage(controller);
            taskPage.HideTickButton();
            Application.Current.MainPage = taskPage;
        }
    }
}
