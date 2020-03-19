using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing; // for pen 
namespace IntelligentScissors
{
    class graphics
    {

        public List<int> orginal_path;
        List<int> tmp_path; 
        public graphics()
        {
            orginal_path = new List<int>();
            tmp_path = new List<int>();
        }

        /*
         *  list have the ID of pixels
         *  pring pixels itself 
         *  construct points 
         *  draw curve passing by them  
         */
        public void draw_line(List<int> l , graph g ,System.Windows.Forms.PictureBox pictureBox1)
        {
            assign(l);           
            Point[] points = new Point[orginal_path.Count+l.Count];
            // draw main path 
            int i = 0;
            for (; i<orginal_path.Count; i++)
            {
                int tid = orginal_path[i];
                pixel tmp1 = helper.un_flatten(tid, g.width);
                Point tmp2 = new Point(tmp1.y , tmp1.x); // col , row
                points[i] = tmp2;
            }

            // draw tmp temporary path 
            for (int j = 0; j < tmp_path.Count; j++)
            {
                int tid = tmp_path[j];
                pixel tmp1 = helper.un_flatten(tid, g.width);
                Point tmp2 = new Point(tmp1.y, tmp1.x); // col , row
                points[i++] = tmp2;
            }
            if(points.Length>=2)  // draw the whole curve 
            {
                pictureBox1.Refresh();
                MainForm.gr.DrawCurve(MainForm.p, points);
            }
        }
        public void assign(List<int> tmp)
        {
            // do we hold refernce or it will be destroied after going out ??
            tmp_path = null;
            tmp_path = tmp; 
        }
        public void conc(List<int> tmp)
        {
            // first node is duplicated 
            for(int i=0; i<tmp.Count; i++)
            {
                if(orginal_path.Count==0 || orginal_path[orginal_path.Count-1]!=tmp[i])
                orginal_path.Add(tmp[i]);
            }
        }
    }
}
