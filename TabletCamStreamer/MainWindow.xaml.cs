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

            cbFrameFlip.SelectionChanged += CbFrameFlip_SelectionChanged;
            cbFrameFlip.ItemsSource = getFlipTypes();
            cbFrameFlip.SelectedIndex = 0;

            cbFrameRotate.ItemsSource = getRotationTypes();
            cbFrameRotate.SelectionChanged += CbFrameRotate_SelectionChanged;
            cbFrameRotate.SelectedIndex = 0;
        }

        string[] getFlipTypes()
        {
            return new string[] { "Choose Flip Types","Horizontal","Vertical","Both"};
        }
        string[] getRotationTypes()
        {
            return new string[] { "Choose Rotate Types", "90 CW", "90 CCW", "180" };
        }

        private void CbFrameFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbFrameFlip.SelectedIndex == 0)
            {
                ucFrameViewer.setFrameFlip(CamRetriever.FrameFlipType.None);
            }
            else if(cbFrameFlip.SelectedIndex == 1)
            {
                ucFrameViewer.setFrameFlip(CamRetriever.FrameFlipType.Horizontal);
            }
            else if(cbFrameFlip.SelectedIndex == 2)
            {
                ucFrameViewer.setFrameFlip(CamRetriever.FrameFlipType.Vertical);
            }
            else if(cbFrameFlip.SelectedIndex == 3)
            {
                ucFrameViewer.setFrameFlip(CamRetriever.FrameFlipType.Both);
            }
        }
        private void CbFrameRotate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbFrameRotate.SelectedIndex == 0)
            {
                ucFrameViewer.setFrameRotation(0);
            }
            else if(cbFrameRotate.SelectedIndex == 1)
            {
                ucFrameViewer.setFrameRotation(90);
            }
            else if (cbFrameRotate.SelectedIndex == 2)
            {
                ucFrameViewer.setFrameRotation(-90);
            }
            else if (cbFrameRotate.SelectedIndex == 3)
            {
                ucFrameViewer.setFrameRotation(180);
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
