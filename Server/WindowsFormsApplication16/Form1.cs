using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
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
        NetworkStream ns;
        byte[] buffer = new byte[4];
        Socket s;


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
        public void retreive()
        {

            
                byte[] res = ReceiveVarData(s);



              

                byte[] dec = decompress(res);
                MemoryStream ms = new MemoryStream(dec);
                BinaryReader br = new BinaryReader(ms);

                //test(br);
                Bitmap first = FirstProcess(br);
                pictureBox1.Image =new Bitmap( first);

                while (true)
                {
                    byte[] del = ReceiveVarData(s);
                    this.Invoke(new Action(()=>this.Text=del.Length.ToString()+"kb"));
                    MemoryStream memory = new MemoryStream(del);
                    BinaryReader br2 = new BinaryReader(memory);
                    Bitmap curr = DeltaProcessing(first, br2);
                    pictureBox1.Image = curr.Clone() as Bitmap;
                    //   MessageBox.Show(res.Length.ToString());

                }
            

      
       
               
        }


        private unsafe Bitmap DeltaProcessing(Bitmap bmp, BinaryReader br)
        {          
          BitmapData  bmData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, 1920,1080), System.Drawing.Imaging.ImageLockMode.ReadWrite,PixelFormat.Format32bppRgb);
          IntPtr scan0 = bmData.Scan0;

            for (int i=0;i<br.BaseStream.Length/7;i++)
            {

                    byte* p = (byte*)((uint)(scan0) + br.ReadUInt32());
                    p[0] = br.ReadByte();
                    p[1] = br.ReadByte();
                    p[2] = br.ReadByte();
                    p[3] = 255;
               
           }

            bmp.UnlockBits(bmData);
            return bmp;

        }

        
        private void Form1_Load(object sender, EventArgs e)
        {  
            
        

            tl.Start();
            cl = tl.AcceptTcpClient();
            s = cl.Client;
            Thread th = new Thread(retreive);
            th.Start();
        }

        Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
        private unsafe Bitmap  FirstProcess (BinaryReader br)
        {

        
            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, 1920, 1080), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            IntPtr scan0 = bmData.Scan0;
            int stride = bmData.Stride;


            int nWidth = bmp.Width;
            int nHeight = bmp.Height;

        
            for (int y = 0; y < nHeight; y++)
            {
                byte* p = (byte*)scan0.ToPointer();
                p += y * stride;

                for (int x = 0; x < nWidth; x++)
                {
                   p[0] = br.ReadByte();
                   p[1] = br.ReadByte();
                   p[2] = br.ReadByte();
                   p[3] = 255; 
                   p += 4;

                }

            }
            bmp.UnlockBits(bmData);   
            return bmp;
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

        

    }
}
