using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;  // for graphic usage and points (pixels)

namespace IntelligentScissors
{
    /*
    * it represents a pixel of an image 
    * x is the hight of the pixel == it's row 
    * y is the width of the pixel == it's column  
    */

    struct pixel
    {
        /*
         * different from all others 
         *  x is the row 
         *  y is the col 
         */
        public int x, y;

        /*
         *  are we able to write non parametrized constructor ?????!!!! 
         */
        public pixel(int xx , int yy)
        {
            x = xx;
            y = yy;
        }
    }

    class graph
    {
        // right , left , down , up
        public int[] yd = { 0, 0, 1, -1 }; // row  
        public int[] xd = { 1, -1, 0, 0 };  // col
        /*
         * be aware that y mean row 
         * and x mean column 
         * 
         * 
         * 
         * 
         * sid = start anchor point 
         * cid = current anchor point 
         * fid = free point = mouse position 
         * id dictionary five lable id of int for each cordinates of a pixel 
         * rid reverse the operation of id to retrive the path of the pixels 
         * g is list of list of tuple  this tuple that hold id , weight of the neighpour point 
         * I guess it's better to use 2d array cuz no. of connection is standard  expect for borders 
         * first time bool var tell us that we have anchor point or not 
         */

        //it hold the next valid ID that did't get assiigned yet 
        /*
         * we were have a bug here 
         * Id was "static" so it was not reinitialized by setting graph object to null 
         * and building a new one which was producing ID's that is out of the range (run time error). 
         */
        public int width=0;  //  does static able to be private   ????  
        public int sid, prvid , cid, fid;
        public int size;
        //public Dictionary<pixel, int> id;
        //public Dictionary<int, pixel> rid;
        public List<Tuple<int, double>>[] g;
        public static bool first_time = false;


        public graph(int sz)
        {
            /*
             * sz is width*height of the image  
             */

            //id = new Dictionary<pixel, int>(sz + 2);
            //rid = new Dictionary<int, pixel>(sz + 2);
            /*g = new List<Tuple<int, double>>[sz + 2];
            for(int i=0; i<sz+2; i++)
            {
                g[i] = new List<Tuple<int, double>>();
            }*/
            size = sz;
        }

        /*
         * get called with the first click ever of the mouse in the picture 
         * y is row 
         * x is col 
         */
         public void set_width(int tmp)
        {
            width = tmp;
        }
        public void set_first_anchor_point(int x , int y)  // col , row 
        {
            int tid = helper.flatten(x , y , width);
            cid=sid=prvid=tid;
            first_time = true;
        }

        /*
         * y is row 
         * x is col  
         */
        public void set_free_point(int x , int y)
        {
            int tid = helper.flatten(x, y, width);
            fid = tid;
        }

        /*
         * y is row 
         * x is col  
         */
        public void set_cur_anchor_point(int x , int y) // col , row
        {
            int tid = helper.flatten(x, y, width);
            prvid = cid;
            cid = tid;
        }
        public int get_first_anchor_point()
        {
            return sid;
        }
        public int get_cur_anchor_point()
        {
            return cid;
        }
        public int get_cur_free_point()
        {
            return fid;
        }
        public int get_prv_anchor_point()
        {
            return prvid;
        }
        // build the graph 
        public void build(RGBPixel [,] image)
        {
            int r = image.GetLength(0);
            int c = image.GetLength(1);
            for(int i=0; i<r; i++)
            {
                for(int j=0; j<c; j++)
                {
                    Vector2D w = ImageOperations.CalculatePixelEnergies(j, i, image);  // energy (right , bellow)
                    if(i+1<r) // down child 
                    {
                        add_edge(j, i, j, i + 1, w.Y);
                    }
                    if(j+1<c) // right child 
                    {
                        add_edge(j, i, j + 1, i, w.X);
                    }
                }
            }
        }


        /*
         * y is row 
         * x is col  
         */
        public void add_edge(int x1 , int y1 , int x2 , int y2 , double w)
        {
            int tid1 = helper.flatten(x1, y1 , width);
            int tid2 = helper.flatten(x2, y2, width);

            w = (double)1 / w;
            w = helper.remove_infinty(w);

            g[tid1].Add(Tuple.Create(tid2, w));
            g[tid2].Add(Tuple.Create(tid1, w));
        }

        /*
         * shortest path algorithm ->>> Dijkstra   
         * scource is -> current_anchor_point 
         * destination is -> free_point 
         * 
         * we will use our p_q_min_heap as DS for selecting the clossest node 
         */

        public List<int> dijkstra(int f , int to)
        {
            // infinity 
            double inf = helper.infinty();

            Priority_Queue q = new Priority_Queue(10); // size is number of nodes in the graph 
            int node = f;
            int dist = to;
            double[] distance = new double[size+2];
            int[] path = new int[size+2];

            // initialize 
            for (int i = 0; i < size; i++)
            {
                distance[i] = inf;
            }
            distance[node] = 0;
            path[node] = -1; // no parent 
            q.insert(node, 0);
            // end 
            while(!q.empty())
            {
                // pick clossest  node 
                node = q.top().id;
                double dnode = q.top().w;
                // delete 
                q.pop();

                // prunning 
                if (node == dist) 
                    break;
                if (dnode > distance[node])
                    continue;

                // relax
                for (int i = 0; i < g[node].Count; i++)
                { 
                    int child = g[node][i].Item1;
                    double cost= g[node][i].Item2; 
                    if(distance[child]>dnode+cost)
                    {
                        distance[child] = dnode + cost;
                        path[child] = node; 
                        q.insert(child, distance[child]); 
                    }
                }
            }

            /*
             * construct the path 
             * order of the path don't matter 
             * cut it just draw throw them 
             */

            List<int> l = new List<int>();      
            l.Add(dist);
            int tid = dist;
            while (path[tid]!=-1) 
            {
                l.Add(path[tid]);
                tid = path[tid]; // go back 
            }
            //l.Reverse();
            return l;
        }

        public List<int> dijkstra(int xf , int yf , int xt , int yt)  //col , row ->> source , destination 
        {
            // Data holders 
            double inf = helper.infinty();
            int []path = new int[size + 2];
            double []dis = new double[size + 2];
            Priority_Queue q = new Priority_Queue(10);
            int node = helper.flatten(xf, yf, width); // col , row
            int to = helper.flatten(xt, yt, width); //col , row 

            //initialize 
            for(int i=0; i<=size; i++)
            {
                dis[i] = inf;
            }
            dis[node] = 0;
            path[node] = -1;
            q.insert(node, 0);

            while(!q.empty())
            {
                // pick clossest 
                node = q.top().id;
                double dnode = q.top().w;
                // delete 
                q.pop();
                pixel tmp_pixel = helper.un_flatten(node, width);
                // update the node that I relax from 
                xf = tmp_pixel.y; // col 
                yf = tmp_pixel.x; // row 

                if (node == to)
                    break;
                if (dis[node] < dnode)
                    continue;

                List<Tuple<int, double>> neighbours = new List<Tuple<int, double>>();

                // build neghbours  4 connectivity 
                Vector2D v = ImageOperations.CalculatePixelEnergies(xf, yf, MainForm.ImageMatrix);
                int tmp_id; // hold my child that I will add 
                double w;
                if(helper.valid(xf , yf+1))  // bellow 
                {
                    w = v.Y;
                    w = (double)1 / w;
                    w = helper.remove_infinty(w);
                    tmp_id = helper.flatten(xf, yf+1 , width);
                    neighbours.Add(Tuple.Create(tmp_id, w));
                }
                if(helper.valid(xf+1 , yf))  // to the right 
                {
                    w = v.X;
                    w = (double)1 / w;
                    w = helper.remove_infinty(w);
                    tmp_id = helper.flatten(xf+1, yf, width);
                    neighbours.Add(Tuple.Create(tmp_id, w));
                }
                if(helper.valid(xf , yf-1))  // above me 
                {
                    v= ImageOperations.CalculatePixelEnergies(xf, yf - 1, MainForm.ImageMatrix);
                    w = v.Y;
                    w = (double)1 / w;
                    w = helper.remove_infinty(w);
                    tmp_id = helper.flatten(xf, yf - 1, width);
                    neighbours.Add(Tuple.Create(tmp_id, w));
                }
                if(helper.valid(xf-1 , yf)) // to the left
                {
                    v = ImageOperations.CalculatePixelEnergies(xf-1, yf, MainForm.ImageMatrix);
                    w = v.X; // energy 
                    w = (double)1 / w;  // weight 
                    w = helper.remove_infinty(w);
                    tmp_id = helper.flatten(xf - 1, yf , width);
                    neighbours.Add(Tuple.Create(tmp_id, w));
                }

                // relax 
                for(int i=0; i<neighbours.Count; i++)
                {
                    int child = neighbours[i].Item1;
                    double cost = neighbours[i].Item2;
                    if(dis[child]>dnode+cost)
                    {
                        dis[child] = dnode + cost;
                        path[child] = node; // your current parent 
                        q.insert(child, dis[child]);
                    }
                }
            }

            // build the path  ,   we are always sure that we will reach our distination 
            List<int> shp = new List<int>();
            shp.Add(to);
            while(path[to]!=-1)
            {
                shp.Add(path[to]);
                to = path[to];
            }
            shp.Reverse();
            return shp;
        }
    }
}