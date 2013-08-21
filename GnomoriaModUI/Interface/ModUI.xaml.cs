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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ModUI : Window
    {
        public ModUI()
        {
            InitializeComponent();
        }

        private void setPanel(Control ctrl)
        {
            grid_selectedPanelHost.Children.Clear();
            grid_selectedPanelHost.Children.Add(ctrl);
            ctrl.Width = grid_selectedPanelHost.Width;
            ctrl.Height = grid_selectedPanelHost.Height;
            ctrl.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            ctrl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            grid_panelSelectionPopup.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_startgame_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_panelSelection_showMoreBox_MouseEnter(object sender, MouseEventArgs e)
        {
            grid_panelSelectionPopup.Visibility = System.Windows.Visibility.Visible;
        }

        private void grid_panelSelectionPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            grid_panelSelectionPopup.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid_panelSelectionPopup.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_setPanel_settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsPanel = new ModUI_Settings();
            setPanel(settingsPanel);
        }

        private void btn_setPanel_logs_Click(object sender, RoutedEventArgs e)
        {
            var logPanel = new ModUI_Logs();
            setPanel(logPanel);
        }

        private void btn_setPanel_showInstalled_Click(object sender, RoutedEventArgs e)
        {
            var locModsPanel = new ModUI_LocalMods();
            setPanel(locModsPanel);
        }
    }
}
