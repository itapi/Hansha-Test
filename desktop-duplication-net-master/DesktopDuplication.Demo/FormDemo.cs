using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private DesktopDuplicator desktopDuplicator;

        public FormDemo()
        {
            InitializeComponent();

            
                desktopDuplicator = new DesktopDuplicator(0,0);
            
           
             
            
        }


        public void  send()
        {

            while (true)
            {
             //   Thread.Sleep(100);
                var screen = desktopDuplicator.GetFrame(0);
                DeltaAsync(screen);
                // MessageBox.Show(screen.NewPixels[0].ToString());

            }

        }
        int count = 0;
        public static byte[] compress(byte[] data)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                using (MemoryStream srcStream = new MemoryStream(data))
                    srcStream.CopyTo(gzipStream);
                return outStream.ToArray();
            }
        }

        public void StartAsync(ScreenFrame frame)
        {

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
               
                    for (var i = 0; i < frame.NewPixels.Length; i += 4)
                    {
                        writer.Write(frame.NewPixels[i]);
                        writer.Write(frame.NewPixels[i + 1]);
                        writer.Write(frame.NewPixels[i + 2]);
                    }
                  
                }

                byte[] data = compress(memoryStream.ToArray());
            //    MessageBox.Show(data.Length.ToString());
               SendVarData(data);

            }
        }





        public void DeltaAsync(ScreenFrame frame)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    for (var i = 0; i < frame.NewPixels.Length; i += 4)
                    {
                        if (frame.NewPixels[i] == frame.PreviousPixels[i] &&
                            frame.NewPixels[i + 1] == frame.PreviousPixels[i + 1] &&
                            frame.NewPixels[i + 2] == frame.PreviousPixels[i + 2]) continue;




                      
                        writer.Write((uint)i);
                        //MessageBox.Show(i.ToString());
                        //   Console.WriteLine(i.ToString());
                        writer.Write(frame.NewPixels[i]);
                     
                        writer.Write(frame.NewPixels[i + 1]);
                        writer.Write(frame.NewPixels[i + 2]);

                    }
                   
                    SendVarData(memoryStream.ToArray());
                   // MessageBox.Show(memoryStream.Length.ToString());
                }
            }
        }
        Socket sck;
        private void FormDemo_Shown(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient();
          
        
     
            client.Connect("localhost", 10);
            sck = client.Client;
          
            //MessageBox.Show(screen.NewPixels[i].ToString());

            Thread.Sleep(200);
          var screen = desktopDuplicator.GetFrame(0);
          StartAsync(screen);
          //  client.Connect("localhost", 10);

         
          
            //var screen = desktopDuplicator.GetFrame(0);

           // MessageBox.Show(screen.NewPixels[0].ToString());
            
               //MessageBox.Show(screen.PreviousPixels.Length.ToString());
         //       this.label1.Text = screen.NewPixels.Length.ToString();

                count++;
               
             // MessageBox.Show(   frame.AccumulatedFrames.ToString());
                   // this.pictureBox1.Image = frame.DesktopImage;

               Thread th = new Thread(send);
               th.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.Text = count.ToString();
            count = 0;
        }
      
     private  int SendVarData( byte[] data)
   {
      int total = 0;
      int size = data.Length;
      int dataleft = size;
      int sent;

      byte[] datasize = new byte[4];
      datasize = BitConverter.GetBytes(size);
     // MessageBox.Show(size.ToString());
      sent = sck.Send(datasize);

      while (total < size)
      {
         sent = sck.Send(data, total, dataleft, SocketFlags.None);
         total += sent;
         dataleft -= sent;
      }
      return total;
   }

        private void FormDemo_Load(object sender, EventArgs e)
        {
        
        }

    }
}
