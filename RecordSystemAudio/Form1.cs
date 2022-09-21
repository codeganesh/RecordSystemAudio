using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace RecordSystemAudio
{
    public partial class Form1 : Form
    {
        private string? outputFileName;
        private WasapiLoopbackCapture? capture;
        private WaveFileWriter? writer;
        public Form1()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void LoadDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            OutputDeviceComboBox.Items.AddRange(devices.ToArray());
            OutputDeviceComboBox.SelectedIndex = 0;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Wave files | *.wav";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            outputFileName = dialog.FileName;

            var device = (MMDevice)OutputDeviceComboBox.SelectedItem;
            capture = new WasapiLoopbackCapture(device);
            writer = new WaveFileWriter(outputFileName, capture.WaveFormat);
            capture.DataAvailable += (s, e) =>
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            };
            capture.RecordingStopped += (s, e) =>
            {
                writer.Dispose();
                capture.Dispose();
                StartButton.Enabled = true;
                StopButton.Enabled = false;

                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(outputFileName),
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            };

            capture.StartRecording();
            StartButton.Enabled = false;
            StopButton.Enabled = true;


        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (capture != null)
                capture.StopRecording();
        }
    }
}