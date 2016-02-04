using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System.Threading;

/// <summary>
/// Get Notes from MIDI interface and trigger VLC Play from a playlist
/// </summary>
namespace VLCMidiRemote
{
    public partial class Main : Form
    {
        private const int SysExBufferSize = 128;
        private InputDevice inDevice = null;
        private SynchronizationContext context;

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Between A and Z
            if (e.KeyChar >= 65 && e.KeyChar <= 90)
            {
                logMe("Got key, ignoring (code commented out)\n");
                // PlayPlayListItem(e.KeyChar - 65 + 5);
                e.Handled = true;
            }
        }

        private void PlayPlayListItem(int iItem)
        {
            WebRequest req;
            req = WebRequest.Create("http://" + Properties.Settings.Default.VLCAddress.ToString() +
                "/requests/status.xml?command=pl_play&id=" + iItem.ToString());
            // Note to self: netCredential does not work with VLC, as it does not challenge
            // properly the client. Work around: add auth header directly
            string credentials = String.Format("{0}:{1}", "", Properties.Settings.Default.VLCPassword.ToString());
            byte[] bytes = Encoding.ASCII.GetBytes(credentials);
            string base64 = Convert.ToBase64String(bytes);
            string authorization = String.Concat("Basic ", base64);
            req.Headers.Add("Authorization", authorization);

            var response = "";
            try
            {
                response = req.GetResponse().ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initMidi();
        }

        private void initMidi()
        {
            if (InputDevice.DeviceCount == 0)
            {
                MessageBox.Show("No MIDI input devices available.", "Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Close();
            }
            else
            {
                try
                {
                    logMe("Found " + InputDevice.DeviceCount.ToString() +
                        " midi devices. Initializing MIDI interface... ");

                    context = SynchronizationContext.Current;
                    inDevice = new InputDevice(0);
                    inDevice.ChannelMessageReceived += HandleChannelMessageReceived;
                    inDevice.Error += new EventHandler<ErrorEventArgs>(inDevice_Error);
                    inDevice.StartRecording();
                    logMe("Done!\n");
                }
                catch (Exception ex)
                {
                    logMe("Error initializing MIDI interface: " + ex.Message);
                    Close();
                }
            }
        }

        private void inDevice_Error(object sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.Error.Message, "Error!",
                   MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        private void HandleChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            context.Post(delegate (object dummy)
            {
                if (cbDebug.Checked)
                {
                    logMe("Got MIDI " + e.Message.Command.ToString() +
                        " on channel " + (e.Message.MidiChannel + 1) + ", note: " +
                        e.Message.Data1 + "\n");
                }
                if ((e.Message.Command.ToString()=="NoteOn") && 
                    (e.Message.MidiChannel + 1 == nudMidiChannel.Value))
                {
                    if (e.Message.Data1 >=48)
                    {
                        // 48 == C3
                        int iPlaylistItem = e.Message.Data1 - 48;
                        logMe("Playing playlist item #" + iPlaylistItem.ToString() + "\n");
                        PlayPlayListItem(iPlaylistItem + 4);
                    }
                }
            }, null);
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            inDevice.StopRecording();
            if (inDevice != null)
            {
                inDevice.Reset();
                inDevice.Close();
            }
            Close();
        }

        private void btStartVLC_Click(object sender, EventArgs e)
        {
            // Start VLC with a playlist and HTTP interface
            // "c:\Program Files\VideoLAN\VLC\vlc.exe" --extraintf http  "D:\Videos\sd.playlist.xspf"

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Properties.Settings.Default.VLCPath;
            startInfo.Arguments = @"--extraintf http """ + 
                Properties.Settings.Default.VLCPlaylist.ToString() + @"""";
            logMe("Starting VLC... ");
            Process.Start(startInfo);
            
            System.Threading.Thread.Sleep(2000);
            logMe("Done\n");
            // Open that playlist (play the first item)
            PlayPlayListItem(4);
        }

        private void logMe(string strText)
        {
            rtbLogs.Text += strText;
            rtbLogs.ScrollToCaret();
        }
    }
}
