using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using NAudio.Wave;
using System;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using static Google.Api.Gax.Grpc.Gcp.AffinityConfig.Types;
using System.Text.RegularExpressions;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn;
        private SpeechClient speechClient;
        private SpeechClient.StreamingRecognizeStream streamingCall;

        public Form1()
        {
            try
            {
                InitializeComponent();

                // Khởi tạo client cho Speech-to-Text
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"secret_key.json");
                speechClient = SpeechClient.Create();

                // Khởi tạo nguồn âm thanh
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1) // Mono 16kHz
                };
                waveIn.DataAvailable += OnDataAvailable;

                // Bắt đầu stream dữ liệu âm thanh
                BeginStreaming();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task BeginStreaming()
        {
            try
            {
                streamingCall = speechClient.StreamingRecognize();
                await streamingCall.WriteAsync(new StreamingRecognizeRequest
                {
                    StreamingConfig = new StreamingRecognitionConfig
                    {
                        Config = new RecognitionConfig
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = "vi-VN",
                        },
                        InterimResults = true,
                    },
                });

                waveIn.StartRecording();

                await Task.Run(async () =>
                {
                    while (await streamingCall.GetResponseStream().MoveNextAsync())
                    {
                        var results = streamingCall.GetResponseStream().Current.Results;
                        if (results != null && results.Count > 0)
                        {
                            var transcript = results[0].Alternatives[0].Transcript;
                            Invoke(new Action(() => {
                                tb1.Text = transcript;
                                process_form(transcript);
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Lỗi trong quá trình stream: {ex.Message}", "Lỗi Stream", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (streamingCall == null)
                return;

            try
            {
                await streamingCall.WriteAsync(new StreamingRecognizeRequest
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(e.Buffer, 0, e.BytesRecorded)
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Lỗi khi gửi dữ liệu âm thanh: {ex.Message}", "Lỗi Âm Thanh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
            }

            if (streamingCall != null)
            {
                try
                {
                    streamingCall.WriteCompleteAsync().Wait();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đóng stream: {ex.Message}", "Lỗi Đóng Stream", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void process_form(string command)
        {
            var formNames = new Dictionary<string, Type> {
                    { "form1", typeof(Form1) },
                    { "form2", typeof(Form2) },
                    };

            string normalizedCommand = Regex.Replace(command.ToLower(), @"\s+", " ").Trim();

            Match match = Regex.Match(normalizedCommand, @"\bmở\s+form\s+(\d+)");

            if (match.Success)
            {
                string formNumber = match.Groups[1].Value;
                string formKeyToOpen = $"form{formNumber}";

                if (formNames.TryGetValue(formKeyToOpen, out Type formType))
                {
                    Form formToOpen = (Form)Activator.CreateInstance(formType);
                    formToOpen.Show();
                }
            }
            
        }
    }
}