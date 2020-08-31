using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTD6_Mod_Manager.Windows
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        public RenameWindow()
        {
            InitializeComponent();
            KeyDown += Rename_TextBox_KeyDown;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Rename_TextBox.Focus();
        }
        
        bool tooltipShown = false;
        private void HandleRename()
        {
            if (Rename_TextBox.Text.Length > 0)
            {
                var args = new RenameEventArgs();
                args.NewName = Rename_TextBox.Text;
                OnRenameComplete(args);
            }

            if (Rename_TextBox.Text.Length <= 0 && !tooltipShown)
            {
                tooltipShown = true;
                ToolTip t = new ToolTip();
                t.Content = "You need to enter something to rename it to";
                t.IsOpen = true;

                new Thread(() =>
                {
                    Thread.Sleep(1500);
                    t.Dispatcher.BeginInvoke((Action)(() => { t.IsOpen = false; }));
                    tooltipShown = false;
                }).Start();
            }
        }

        #region UI Events
        private void Okay_Button_Click(object sender, RoutedEventArgs e) => HandleRename();
        private void Rename_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                HandleRename();
            if (e.Key == Key.Escape)
                this.Close();
        }

        #endregion


        #region Events
        public static event EventHandler<RenameEventArgs> RenameComplete;
        public class RenameEventArgs : EventArgs
        {
            public string NewName { get; set; }
        }

        public void OnRenameComplete(RenameEventArgs e)
        {
            EventHandler<RenameEventArgs> handler = RenameComplete;
            if (handler != null)
                handler(this, e);
        }
        #endregion

    }
}
