using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GnomoriaModUI
{
    /// <summary>
    /// Interaction logic for ModUI_LocalMods.xaml
    /// </summary>
    public partial class ModUI_LocalMods : UserControl
    {

        public static void setTimeOut(Action doWork, TimeSpan time)
        {

            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            myDispatcherTimer.Interval = time;
            myDispatcherTimer.Tick += delegate(object s, EventArgs args)
            {
                myDispatcherTimer.Stop();
                doWork();
            };
            myDispatcherTimer.Start();
        }





        public ModUI_LocalMods()
        {
            InitializeComponent();
            ic1.ItemsSource = new int[] { 1, 2, 4 };
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }


        private DateTime closeTogglePanel_at = DateTime.MinValue;
        private void showTogglePanel()
        {
            toggleSortPanel.Visibility = System.Windows.Visibility.Visible;
            showToggleSortButton.Visibility = System.Windows.Visibility.Hidden;
            closeTogglePanel_at = DateTime.MinValue;
        }
        private void showToggleSortPanel_Click(object sender, RoutedEventArgs e)
        {
            showTogglePanel();
        }
        private void toggleSortPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            showTogglePanel();
        }
        private void toggleSortPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            var myClose = closeTogglePanel_at = DateTime.Now + TimeSpan.FromMilliseconds(500);
            setTimeOut(() =>
            {
                if (closeTogglePanel_at == myClose)
                {
                    toggleSortPanel.Visibility = System.Windows.Visibility.Hidden;
                    showToggleSortButton.Visibility = System.Windows.Visibility.Visible;
                }
            }, TimeSpan.FromMilliseconds(500));
        }
        private void showToggleSortButton_MouseEnter(object sender, MouseEventArgs e)
        {
            showTogglePanel();
        }
    }
}
