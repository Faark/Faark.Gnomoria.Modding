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
    /// Interaction logic for ModUI_Logs.xaml
    /// </summary>
    public partial class ModUI_Logs : UserControl
    {
        public ModUI_Logs()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cb_logSelect.IsEnabled = false;
            btn_clearLog.IsEnabled = false;
            rtb_logContent.Document.Blocks.Add(new Paragraph(new Run() { Text = @"Feature comming soon! Check MyDocuments\My Games\Gnomoria\Gnomoria.log and [Gnomoria-InstalDIR]\GnomoriaModded.log for now..." }));
        }

        private void cb_logSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_clearLog_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
