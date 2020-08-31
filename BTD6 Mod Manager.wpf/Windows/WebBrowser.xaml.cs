using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BTD6_Mod_Manager.Windows
{
    /// <summary>
    /// Interaction logic for WebBrowser.xaml
    /// </summary>
    public partial class WebBrowser : Window
    {
        #region Constructors
        public WebBrowser()
        {
            InitializeComponent();
        }

        public WebBrowser(string windowTitle) : this()
        {
            Title = windowTitle;
        }

        #endregion

        public void GoToURL(string url)
        {
            dynamic activeX = this.webBrowser.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.webBrowser, new object[] { });

            activeX.Silent = true;
            webBrowser.Navigate(url);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            webBrowser.Dispose();
        }
    }
}
