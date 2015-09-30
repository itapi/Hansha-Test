using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication16
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TcpListener tl = new TcpListener(IPAddress.Any, 10);
        TcpClient cl;      
        byte[] buffer = new byte[4];
        Socket s;
        int count = 0;
        public static byte[] decompress(byte[] compressed)
        {
            using (MemoryStream inStream = new MemoryStream(compressed))
            using (GZipStream gzipStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (MemoryStream outStream = new MemoryStream())
            {
                gzipStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }
      
            public  async Task Run(Control control, Bitmap bitmap,Socket s)
            {
              byte[] res = decompress(ReceiveVarData(s));//receiving the array.      
                await DispatchStartAsync(control, bitmap, res);

                    while (true)
                    {
                        count++;
                        byte[] res2 = (ReceiveVarData(s));
                        await DispatchDeltaAsync(control, bitmap, res2);

                    }             
            }

            private static async Task DispatchStartAsync(Control control, Bitmap bitmap, byte []buffer)
            {
               
                control.Invoke(new Action(() =>
                {
                    ProcessStart(bitmap, buffer);
                    control.Refresh();
                }));
            }

            private static async Task DispatchDeltaAsync(Control control, Bitmap bitmap, byte[] buffer)
            {
              

                control.Invoke(new Action(() =>
                {
                    ProcessDelta(bitmap, buffer);
                    control.Refresh();
                }));
            }

            private static unsafe void ProcessStart(Bitmap bitmap, byte[] buffer)
            {
                var imageBoundaries = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var imageData = bitmap.LockBits(imageBoundaries, ImageLockMode.WriteOnly, bitmap.PixelFormat);

              
                    var pointer = (byte*)imageData.Scan0;

                    for (int i = 0; i < buffer.Length; i += 3)
                    {                   
                        pointer[0] = buffer[i ];
                        pointer[1] = buffer[i + 1];
                        pointer[2] = buffer[i + 2];
                        pointer += 4;


                    }
                bitmap.UnlockBits(imageData);
            }

            private static unsafe void ProcessDelta(Bitmap bitmap, byte[] buffer)
            {

            var imageBoundaries = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var imageData = bitmap.LockBits(imageBoundaries, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var basePointer = (byte*) imageData.Scan0;


                for (int i = 0; i < buffer.Length;i+=7 )
                {

                    var pointer = basePointer + BitConverter.ToInt32(buffer,i);
                        pointer[0] = buffer[i+4];
                        pointer[1] = buffer[i+5]; 
                        pointer[2] = buffer[i+6];
                    
           }

            bitmap.UnlockBits(imageData);
            
        }
    
        private void Form1_Load(object sender, EventArgs e)
        {
            tl.Start();
            cl = tl.AcceptTcpClient();
            s = cl.Client;

            var bitmap = new Bitmap(1920, 1080, PixelFormat.Format32bppRgb);
            pictureBox1.Image = bitmap;
            Task.Run(() => Run(this, bitmap,s));
            
            
        }
        

        
       

       
    private static byte[] ReceiveVarData(Socket s)
   {
      int total = 0;
      int recv;
      byte[] datasize = new byte[4];

      recv = s.Receive(datasize, 0, 4, 0);
      int size = BitConverter.ToInt32(datasize, 0);
      int dataleft = size;
      byte[] data = new byte[size];

        
      while(total < size)
      {
         recv = s.Receive(data, total, dataleft, 0);
         if (recv == 0)
         {
         
            break;
         }
         total += recv;
         dataleft -= recv;
      }
      return data;
   }

    private void timer1_Tick(object sender, EventArgs e)
    {
        this.Text = count.ToString();
        count = 0;
    }

        

    }
}
