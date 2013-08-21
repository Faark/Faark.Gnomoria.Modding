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
    /// Interaction logic for ModUI_Settings.xaml
    /// </summary>
    public partial class ModUI_Settings : UserControl
    {
        public ModUI_Settings()
        {
            InitializeComponent();
        }


        public event EventHandler OnSaveButtonClicked;
        public bool CheckForUpdates
        {
            get
            {
                return cb_checkForUpdates.IsChecked.Value;
            }
            set
            {
                cb_checkForUpdates.IsChecked = value;
            }
        }
        public bool SendInstallID
        {
            get
            {
                return cb_sendInstallId.IsChecked.Value;
            }
            set
            {
                cb_sendInstallId.IsChecked = value;
            }
        }
        public bool AutoUpdate
        {
            get
            {
                return cb_autoUpdate.IsChecked.Value;
            }
            set
            {
                cb_autoUpdate.IsChecked = value;
            }
        }
        public bool UseBetaVersions
        {
            set
            {
                cb_updateToBetaVersions.IsChecked = value;
            }
            get
            {
                return cb_updateToBetaVersions.IsChecked.Value;
            }
        }

        private void setBtnState(bool state)
        {
            btn_cancel.IsEnabled = state;
            btn_save.IsEnabled = state;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            setBtnState(false);
        }
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            setBtnState(false);
            if (OnSaveButtonClicked != null)
            {
                OnSaveButtonClicked.Invoke(this, new EventArgs());
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            setBtnState(false);
            throw new NotImplementedException();
        }

        private void anyCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            setBtnState(true);
            System.Windows.Forms.MessageBox.Show((sender as CheckBox).IsChecked.ToString());
        }



    }
}
