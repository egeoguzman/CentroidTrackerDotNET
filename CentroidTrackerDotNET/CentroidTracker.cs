using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NumSharp;


namespace CentroidTrackerDotNET
{
    public class Centroid
    {
        Utilities utils = new Utilities();
        public int maxDisappeared = 50;
        public int nextObjectID = 0;
        Dictionary<int, (double x, double y)> objects;
        Dictionary<int, int> disappeared;
        public Centroid()
        {
            this.maxDisappeared = 50;
            this.nextObjectID = 0;
            this.objects = new Dictionary<int, (double, double)>();
            this.disappeared = new Dictionary<int, int>();

        }
        public void Register((double, double) centroid)
        {
            this.objects[this.nextObjectID] = centroid;
            this.disappeared[this.nextObjectID] = 0;
            this.nextObjectID += 1;
        }

        public void Deregister(int objectID)
        {
            this.objects.Remove(objectID);
            this.disappeared.Remove(objectID);
        }

        public Dictionary<int, (double, double)> Update(List<List<double>> rects)
        //List of Tuple => Item1=StartX Item2=StartY Item3=EndX Item4=EndY
        {
            // check to see if the list of input bounding box rectangles
            // is empty
            if (rects.Count == 0)
            {
                // loop over any existing tracked objects and mark them
                // as disappeared
                foreach(int objectID in this.disappeared.Keys)
                {
                    this.disappeared[objectID] += 1;

                    // if we have reached a maximum number of consecutive
                    // frames where a given object has been marked as
                    // missing, deregister it
                    if (this.disappeared[objectID] > this.maxDisappeared)
                    {
                        this.Deregister(objectID);
                    }
                }
                // return early as there are no centroids or tracking info
                // to update
                return this.objects;
            }
            // initialize an array of input centroids for the current frame
            //var inputCentroids = np.zeros((rects.Length,2));
            //int[,] inputCentroids = new int[rects.Length,2];
            //Array.Clear(inputCentroids, 0, inputCentroids.Length);
            List<(int x, int y)> inputCentroids = new List<(int x, int y)>();
            // loop over the bounding box rectangles
            for (int i = 0; i < rects.Count; i++)
            {
                // use the bounding box coordinates to derive the centroid
                var cX = (int)((rects[i][0] + rects[i][2]) / 2.0);
                var cY = (int)((rects[i][1] + rects[i][3]) / 2.0);
                inputCentroids.Add((cX, cY));
            }
            
            // if we are currently not tracking any objects take the input
            // centroids and register each of them
            if (this.objects.Count == 0)
            {
                for(int i = 0; i < inputCentroids.Count; i++)
                {
                    this.Register(inputCentroids[i]);
                }
            }
            // otherwise, are are currently tracking objects so we need to
            // try to match the input centroids to existing object
            // centroids
            else
            {
                //grab the set of object IDs and corresponding centroids
                //List<int> _objectIDs = new List<int>(this.objects.Keys);
                //List<(double,double)> objectCentroids = new List<(double,double)>(this.objects.Values);
                //int[] objectIDs = new int[this.objects.Keys.Count];
                List<int> objectIDs = new List<int>();
                foreach(int id in this.objects.Keys)
                {
                    objectIDs.Add(id);
                }
                List<(double x, double y)> objectCentroids = new List<(double x, double y)>();
                foreach((double x, double y) objCent in this.objects.Values)
                {
                    objectCentroids.Add(objCent);
                }
                //List<(double x, double y)> objectCentroids = new List<(double x, double y)>(this.objects.Values);
                //for (int i=0;i<this.objects.Keys.Count;i++)
                //{
                //    foreach(int k in this.objects.Keys)
                //    {
                //        objectIDs[i] = k;

                //    }
                //}
                //for(int i=0;i < this.objects.Values.Count;i++)
                //{
                //    foreach((double,double) k in this.objects.Values)
                //    {
                //        objectCentroids[i] = k;
                //    }
                //}

                double[,] D = new double[objectCentroids.Count, inputCentroids.Count];
                for (int i = 0; i < objectCentroids.Count; i++)
                {
                    for (int j = 0; j < inputCentroids.Count; j++)
                    {

                        // compute the distance between each pair of object
                        // centroids and input centroids, respectively -- our
                        // goal will be to match an input centroid to an existing
                        // object centroid
                        D[i, j] = Math.Sqrt((Math.Pow((objectCentroids[i].x - inputCentroids[j].x), 2)) + (Math.Pow((objectCentroids[i].y - inputCentroids[j].y), 2)));

                    }
                }
                // in order to perform this matching we must (1) find the
                // smallest value in each row and then (2) sort the row
                // indexes based on their minimum values so that the row
                // with the smallest value as at the *front* of the index
                // list
                double[] D_min_row = utils.min_row(D);
                Array.Sort(D_min_row);
                int[] rows = new int[D_min_row.Length];
                int[] cols = new int[D_min_row.Length];

                for (int k = 0; k < D_min_row.Length; k++)
                {
                    rows[k] = utils.sorted_idx_row(D, D_min_row[k]);
                    cols[k] = utils.sorted_idx_col(D, D_min_row[k]);
                }
                //foreach (int ege in cols)
                //    Console.WriteLine(ege);
                

                // next, we perform a similar process on the columns by
                // finding the smallest value in each column and then
                // sorting using the previously computed row index list
                //D_min_col = utils.min_row(D);
                //Array.Sort(D_min_col);

                //for (int k = 0; k < D_min_col.Length; k++)
                //{
                //    cols[k] = utils.sorted_idx_col(D, D_min_col[k]);
                //}

                // in order to determine if we need to update, register,
                // or deregister an object we need to keep track of which
                // of the rows and column indexes we have already examined
                SortedSet<int> usedRows = new SortedSet<int>();
                SortedSet<int> usedCols = new SortedSet<int>();
                //SortedSet<int> unusedRows = new SortedSet<int>();
                //SortedSet<int> unusedCols = new SortedSet<int>();
                SortedSet<int> D_HASHSET_Rows = new SortedSet<int>();
                SortedSet<int> D_HASHSET_Cols = new SortedSet<int>();

               // Zip the rows and the cols as a tuple
                //var zipped = rows.Zip(cols, (row, col) => (row, col));

                //loop over the combination of the(row, column) index tuples
                //foreach (var coordinate in rows.Zip(cols, (row, col) => (row, col)))
                //{

                //    //Console.WriteLine(coordinate);
                //    // if we have already examined either the row or
                //    // column value before, ignore it val
                //    if ((usedRows.Contains(coordinate.row)) || (usedCols.Contains(coordinate.col)))
                //        continue;
                //    // otherwise, grab the object ID for the current row,
                //    // set its new centroid, and reset the disappeared counter
                //    int objectID = objectIDs[coordinate.row];
                //    //Console.WriteLine(objectID);
                //    this.objects[objectID] = inputCentroids[coordinate.col];
                //    this.disappeared[objectID] = 0;

                //    // indicate that we have examined each of the row and column indexes, respectively
                //    usedRows.Add(coordinate.row);
                //    usedCols.Add(coordinate.col);
                //}
                for (int i = 0; i < rows.Length; i++)
                {
                    if ((usedRows.Contains(rows[i])) || (usedCols.Contains(cols[i])))
                        continue;
                    int objectıd = objectIDs[rows[i]];
                    for (int t = 0; t < this.objects.Count; t++)
                    {
                        if (this.objects.ElementAt(t).Key == objectıd)
                        {
                            this.objects[objectıd] = inputCentroids[cols[i]];
                        }
                    }
                    this.disappeared[objectıd] = 0;

                    usedRows.Add(rows[i]);
                    usedCols.Add(cols[i]);
                }
                for (int i = 0; i < objectCentroids.Count; i++)
                {
                    D_HASHSET_Rows.Add(i);
                }
                for (int i = 0; i < inputCentroids.Count; i++)
                {
                    D_HASHSET_Cols.Add(i);
                }
                //compute both the row and column index we have NOT yet examined
                SortedSet<int> unusedRows = new SortedSet<int>(D_HASHSET_Rows.Except(usedRows));
                SortedSet<int> unusedCols = new SortedSet<int>(D_HASHSET_Cols.Except(usedCols));
                //unusedRows = D_HASHSET_Rows;
                //unusedCols = D_HASHSET_Cols;
                //    unusedRows.Add(val);
                //}
                //foreach(int val in D_HASHSET_Cols)
                //{
                //    unusedCols.Add(val);
                //}

                // in the event that the number of object centroids is
                // equal or greater than the number of input centroids
                // we need to check and see if some of these objects have
                // potentially disappeared
                if (objectCentroids.Count >= inputCentroids.Count)
                {
                    //loop over the unused row indexes
                    foreach (int row in unusedRows)
                    {
                        // grab the object ID for the corresponding row
                        // index and increment the disappeared counter
                        int objectID = objectIDs[row];
                        this.disappeared[objectID] += 1;

                        // check to see if the number of consecutive
                        // check to see if the number of consecutive
                        // frames the object has been marked "disappeared"
                        // for warrants deregistering the object
                        if (this.disappeared[objectID] > this.maxDisappeared)
                        {
                            this.Deregister(objectID);
                        }

                    }
                }
                // otherwise, if the number of input centroids is greater
                // than the number of existing object centroids we need to
                // register each new input centroid as a trackable object
                else
                {
                    foreach (int col in unusedCols)
                    {
                        this.Register((inputCentroids[col].x, inputCentroids[col].y));

                    }
             
                }
                
            }
            return this.objects;
        }
    }
}
