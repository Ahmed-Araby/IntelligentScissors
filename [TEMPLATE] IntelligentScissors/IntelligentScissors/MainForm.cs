using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics; // for stopwatch 

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

         // Data 
        public static RGBPixel[,] ImageMatrix;
        graph g;
        bool ok = false;
        bool constructed;
        // end 

        // graphics 
        graphics draw_cutter;   // if my class is not public would it still be valid for mainfrme ???? but just it ????????????????????
        public static Pen p;
        public static Graphics gr;
        int x, y;   //does point return float do we have any interpation here ?????????????????????????????????????????????????????????????
        bool mov , down_up=false;
        // end 
        void reset()
        {
            g = null;
            p = null;
            gr = null;
            draw_cutter = null;
            ok = false;
            mov = false;
            constructed = false;
        }
        void init()
        {
            p = new Pen(Color.Red, 2);
            draw_cutter = new graphics();
            gr = pictureBox1.CreateGraphics();

            x = -1;
            y = -1;
            mov = false;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                
                reset();
                init();

                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                helper.set_boundries(ImageMatrix.GetLength(0) , ImageMatrix.GetLength(1)); // row , col
                /*
                 *  I think this step just have the aim of getting the pixel of where the 
                 */
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                
                int sz = ImageMatrix.GetLength(0) * ImageMatrix.GetLength(1);
                g = new graph(sz); // special case of init();

                g.set_width(ImageMatrix.GetLength(1));  // 1 is width = number of columns 

                //g.build(ImageMatrix);  // cuz time delay 
                ok = true;
            }
            // GUI Stuff 
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }   
   
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // the graph construction 
            // nodes id + neghibours 
            Stopwatch s = new Stopwatch();
            s.Start();
            
            if (!constructed)
            {

                // first time ever 
                g.g = new List<Tuple<int, double>>[g.size]; // 0 based id 
                for (int i = 0; i < g.size; i++)
                {
                    g.g[i] = new List<Tuple<int, double>>();
                }
                g.build(ImageMatrix);
                constructed = true;
            }
            FileStream f = new FileStream("output_me.txt", FileMode.Create , FileAccess.ReadWrite , FileShare.None , 65536 , false);
            StreamWriter sw = new StreamWriter(f , Encoding.UTF8 , 65536); 
            int sz = ImageMatrix.GetLength(0) * ImageMatrix.GetLength(1);
            sw.WriteLine("Constructed Graph: (Format: node_index|edges:(from, to, weight)... )");
            /*
             * to avoid the time of writing 
             * to Hard disk many times 
             */
            string tmp1 = "" , tmp2;  
            for(int i=0; i<sz; i++)
            {
                tmp1=i.ToString()+"|"+"edges:";
                for(int j=0; j<g.g[i].Count; j++)
                {
                    tmp2= "(" + i.ToString() + "," + g.g[i][j].Item1 + "," + g.g[i][j].Item2+")";
                    tmp1 += tmp2;
                }
                sw.WriteLine(tmp1);
            }
            s.Stop();
            //TimeSpan t = s.Elapsed;
            //long tt = t.Seconds;
            tmp2 = "Graph construction took: " + (s.ElapsedMilliseconds/(double)1000).ToString()+ " seconds.";
            sw.WriteLine(tmp2);
            /*
            -----------mile stone one ----------------------
            sw.WriteLine("The constructed graph");
            sw.WriteLine();
            int sz = ImageMatrix.GetLength(0) * ImageMatrix.GetLength(1);
            for(int i=0; i<sz; i++)
            {
                // intro 
                string tmp = " The  index node";
                sw.Write(tmp);
                sw.WriteLine(i);
                sw.WriteLine("Edges");
                // connections
                int j = 0;
                if (g.g[i].Count == 4)
                    j = 2;
                else if (g.g[i].Count == 3)
                    j = 1;
                for( ; j<g.g[i].Count; j++)
                {
                    int child = g.g[i][j].Item1;
                    double cost=g.g[i][j].Item2;
                    sw.Write("edge from   ");
                    sw.Write(i);
                    sw.Write("  To  ");
                    sw.Write(child);
                    sw.Write("  With Weights  ");
                    sw.WriteLine(cost);
                }
                if (g.g[i].Count == 3)
                    j = 1;
                else if (g.g[i].Count == 4)
                    j = 2;
                else
                    j = 0;
                for (int z=0; z <j; z++)
                {
                    int child = g.g[i][z].Item1;
                    double cost = g.g[i][z].Item2;
                    sw.Write("edge from   ");
                    sw.Write(i);
                    sw.Write("  To  ");
                    sw.Write(child);
                    sw.Write("  With Weights  ");
                    sw.WriteLine(cost);
                }
                // empty lines 
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();
            }
            */
            sw.Close();
            f.Close();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // some anchor point 
            x = e.X; // x- cordinate (column)
            y = e.Y; // y- cordinate (row)

            // first anchor point 
            if (mov == false)
            {
                //int tmp = g.id[new pixel(y, x)];  // what is it's use   ?????????????????????
                g.set_first_anchor_point(x, y);

                mov = true;  // Indicate that we have anchor point and we are able to start drawing 
            }
            //current anchor point 
            else
            {
                g.set_cur_anchor_point(x, y); // exchange with prv 
                pixel tmp_pixel = helper.un_flatten(g.get_prv_anchor_point(), g.width);
                List<int> strong_path = g.dijkstra(tmp_pixel.y, tmp_pixel.x, x, y);  // my new version of dijkstra 
                draw_cutter.conc(strong_path);
                //draw_cutter.draw_line(new List<int>() , g.rid , pictureBox1);
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // mouse 
            if (mov && ok)
            {
                g.set_free_point(e.X, e.Y);
                pixel tmp_pixel = helper.un_flatten(g.get_cur_anchor_point(), g.width);
                //List<int> path = g.dijkstra(g.get_cur_anchor_point() , g.get_cur_free_point());
                Stopwatch ss = new Stopwatch();
                ss.Start();
                List<int> path = g.dijkstra(tmp_pixel.y, tmp_pixel.x, e.X, e.Y); // col , row   ->>> my new version of dijkstra 
                ss.Stop();
                TimeSpan t= ss.Elapsed;
                double x = t.Milliseconds;
                textBox3.Text= x.ToString();
                draw_cutter.draw_line(path, g, pictureBox1);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            // current 
            int tmp_id = g.get_cur_anchor_point();
            pixel tmp_pixel1 = helper.un_flatten(tmp_id, g.width);
            // very first 
            tmp_id = g.get_first_anchor_point();
            pixel tmp_pixel2 = helper.un_flatten(tmp_id, g.width);
            List<int> last_path = g.dijkstra(tmp_pixel1.y, tmp_pixel1.x, tmp_pixel2.y, tmp_pixel2.x); // col , row 
            // no more live wires            
            mov = false;
            ok = false;
            draw_cutter.conc(last_path);
            draw_cutter.draw_line(new List<int>() , g , pictureBox1);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(!constructed)
            {

                // first time ever 
                g.g = new List<Tuple<int, double>>[g.size]; // 0 based id 
                for (int i = 0; i < g.size; i++)
                {
                    g.g[i] = new List<Tuple<int, double>>();
                }
                g.build(ImageMatrix);
                constructed = true;
            }
            // shortest path between two nodes 

            // input is ID's
            //int from = int.Parse(textBox1.Text.ToString());
            //int to = int.Parse(textBox2.Text.ToString());

            // input is pixles 
            Stopwatch s = new Stopwatch();
            s.Start();
            pixel ffrom = new pixel(1, 2); // row , col 
            pixel tto = new pixel(10, 10);
            int from = helper.flatten(ffrom.y, ffrom.x, g.width);
            int to = helper.flatten(tto.y , tto.x , g.width);
            // hard coded input 
            from = 1695577;
            to = 2055619;
            ffrom = helper.un_flatten(from, g.width);
            tto = helper.un_flatten(to, g.width);
            List<int> path=g.dijkstra(from , to);
            FileStream f = new FileStream("shortest_path.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(f , Encoding.UTF8 , 65536); // bigger buffer 
            string tmp = "";
            tmp = "The Shortest path from Node "+from+" at("+ffrom.y+", "+ffrom.x+") to Node "+to+" at("+tto.y+", "+tto.x+")";  // col , row 
            sw.WriteLine(tmp);
            tmp="Format: (node_index, x, y)";
            sw.WriteLine(tmp);
            pixel tmp_pixel = new pixel();
            for (int i=0; i<path.Count; i++)
            {
                tmp_pixel = helper.un_flatten(path[i], g.width);
                tmp="{ X = "+tmp_pixel.y+",Y = "+tmp_pixel.x+"},"+ tmp_pixel.y + ","+ tmp_pixel.x+")";
                sw.WriteLine(tmp);
            }
            s.Stop();
            tmp="Path construction took: "+(s.ElapsedMilliseconds/(double)1000).ToString()+" seconds.";
            sw.WriteLine(tmp);

             /*
             mile stone 1
             sw.WriteLine(path.Count);

             pixel tmp_pixel = helper.un_flatten(from , g.width);
             // first line 
             sw.Write(" The Shortest path from Node  ");
             sw.Write(from);
             sw.Write("at");
             sw.Write(" position   ");
             sw.Write(tmp_pixel.x);
             sw.Write("  ");
             sw.WriteLine(tmp_pixel.y);

             // second line
             tmp_pixel = helper.un_flatten(to, g.width);
             sw.Write(" The Shortest path to Node  ");
             sw.Write(to);
             sw.Write("at");
             sw.Write(" position   ");
             sw.Write(tmp_pixel.x);
             sw.Write("  ");
             sw.WriteLine(tmp_pixel.y);

             // path it self 
             for (int i=0; i<path.Count; i++)
             {
                 tmp_pixel = helper.un_flatten(path[i] , g.width);
                 sw.Write("Node  ");
                 sw.Write("{X=");
                 sw.Write(tmp_pixel.y);
                 sw.Write(",Y=");
                 sw.Write(tmp_pixel.x);
                 sw.Write("} ");
                 sw.Write(" at position x ");
                 sw.Write(tmp_pixel.y);
                 sw.Write(" at position y   ");
                 sw.WriteLine(tmp_pixel.x);               
             }*/
             sw.Close();
            f.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}