using System.Text;
using NAudio.Wave;

namespace Ogg_Wav_Convertor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Ogg to wav Conversion
        public static void Ogg_wav(string oggpath)
        {
            string temp_wav = "";
            try
            {
                temp_wav = Directory.GetCurrentDirectory();
                temp_wav += "\\temp.wav";                              // Create temp wav            

                using (var vorbis = new NVorbis.VorbisReader(@oggpath))
                using (var outFile = System.IO.File.Create(@temp_wav))
                using (var writer = new System.IO.BinaryWriter(outFile))
                {
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write(0);
                    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                    writer.Write(Encoding.ASCII.GetBytes("fmt "));
                    writer.Write(18);
                    writer.Write((short)1); // PCM format
                    writer.Write((short)vorbis.Channels);
                    writer.Write(vorbis.SampleRate);
                    writer.Write(vorbis.SampleRate * vorbis.Channels * 2);  // avg bytes per second
                    writer.Write((short)(2 * vorbis.Channels)); // block align
                    writer.Write((short)16); // bits per sample
                    writer.Write((short)0); // extra size

                    writer.Write(Encoding.ASCII.GetBytes("data"));
                    writer.Flush();
                    var dataPos = outFile.Position;
                    writer.Write(0);

                    var buf = new float[vorbis.SampleRate / 10 * vorbis.Channels];
                    int count;
                    while ((count = vorbis.ReadSamples(buf, 0, buf.Length)) > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var temp = (int)(32767f * buf[i]);
                            if (temp > 32767)
                            {
                                temp = 32767;
                            }
                            else if (temp < -32768)
                            {
                                temp = -32768;
                            }
                            writer.Write((short)temp);
                        }
                    }
                    writer.Flush();

                    writer.Seek(4, System.IO.SeekOrigin.Begin);
                    writer.Write((int)(outFile.Length - 8L));

                    writer.Seek((int)dataPos, System.IO.SeekOrigin.Begin);
                    writer.Write((int)(outFile.Length - dataPos - 4L));

                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Ogg_wav()", "BEL_DAQ_Host", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String ogg_path = @"D:\Database\Channel.ogg";
            Ogg_wav(ogg_path);   
        }
    }
}
