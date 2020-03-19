using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligentScissors
{
    class helper
    {
        public static int col;   // number of row
        public static int row;  //  number of columns 
        public static void set_boundries(int r , int c)  // row , col
        {
            row = r;
            col = c;
        }
        public static int flatten(int x , int y , int width) // col , row , image width 
        {
            int tid = y * width + x;
            return tid;
        }
        public static pixel un_flatten(int tid , int width) // pixel id , image width 
        {
            int y = tid / width; // row 
            int x = tid % width; // col 
            return new pixel(y, x); // row , col -> in correct ordeer 
        }
        public static double infinty()
        {
            double zero = 0;
            double number = 1;
            number = number / zero;
            return number;
        }
        public static double remove_infinty(double val)
        {
            if(double.IsInfinity(val))
            {
                val = 1e16;
            }
            return val;
        }
        public static bool valid(int x , int y)  // col , row 
        {
            /*
             *  check if the current cordinates have a valid pixel
             *  inside our grid 
             */

            if (x < col && x >= 0 && y < row && y >= 0)
                return true;
            return false; // shut fuck your mouth
        }
    }
}
