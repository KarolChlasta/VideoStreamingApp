using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TabletCamStreamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        StreamingServer streamingServer;
        bool isStreaming = false;
        public MainWindow()
        {
            InitializeComponent();
            cbCamSelection.SelectionChanged += CbCamSelection_SelectionChanged;
            string[] camDevices = CamRetriever.getCameraList();
            cbCamSelection.ItemsSource = camDevices;
            if(camDevices.Length > 0)
            {
                cbCamSelection.SelectedIndex = 0;
            }
        }
        private void CbCamSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ucFrameViewer.changeCamera(cbCamSelection.SelectedIndex);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ucFrameViewer.Close();
        }

        private void btnStream_Click(object sender, RoutedEventArgs e)
        {
            if(!isStreaming)
            {
                streamingServer = new StreamingServer(ucFrameViewer.MainCamRetriever.GrabFrames(),true);
                streamingServer.Start(4040);
                btnStream.Content = "Stop Streaming";
                cbCamSelection.IsEnabled = false;
            }
            else
            {
                streamingServer?.Dispose();
                streamingServer = null;
                btnStream.Content = "Start Streaming";
                cbCamSelection.IsEnabled = true;
            }
            isStreaming = !isStreaming;
        }
    }
}
