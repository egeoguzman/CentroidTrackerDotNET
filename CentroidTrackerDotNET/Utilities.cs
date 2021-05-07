using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NumSharp;


namespace CentroidTrackerDotNET
{
    public class Utilities
    {
        public double calcDist(double x1, double y1, double x2, double y2)
        {
            double x = x1 - x2;
            double y = y1 - y2;
            double dist = Math.Sqrt((x * x) + (y * y));

            return dist;
        }
        public double[] min_row(double[,] a)
        {
            double[] min = new double[a.GetLength(0)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                double[] c = new double[a.GetLength(1)];
                for (int x = 0; x < a.GetLength(1); x++)
                    c[x] = a[i, x];
                min[i] = c.Min();
            }
            return min;
        }
        public double[] min_col(double[,] a)
        {
            double[] min = new double[a.GetLength(1)];
            for (int i = 0; i < a.GetLength(1); i++)
            {
                double[] c = new double[a.GetLength(0)];
                for (int x = 0; x < a.GetLength(0); x++)
                    c[x] = a[x, i];
                min[i] = c.Min();
            }
            return min;
        }

        public int sorted_idx_row(double[,] matrix, double value)
        {
            int w = matrix.GetLength(0); // width
            int h = matrix.GetLength(1); // height

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (matrix[x, y].Equals(value))
                        return x;
                }
            }

            return 0;
        }
        public int sorted_idx_col(double[,] matrix, double value)
        {
            int w = matrix.GetLength(0); // width
            int h = matrix.GetLength(1); // height

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (matrix[x, y].Equals(value))
                        return y;
                }
            }

            return 0;
        }
    }
}