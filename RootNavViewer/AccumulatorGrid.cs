using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RootNav.Viewer
{
    class AccumulatorGrid
    {
        private double[][][][] quadIndex = { new double[4096][][], new double[4096][][], new double[4096][][], new double[4096][][] };

        public void Accumulate(int i, int j, double value)
        {
            int quad = i >= 0 ? (j >= 0 ? 0 : 1) : (j >= 0 ? 2 : 3);

            // + + 0
            // + - 1
            // - + 2
            // - - 3

            int absI = Math.Abs(i);
            int absJ = Math.Abs(j);

            int key1 = (absJ / 100) * 64 + (absI / 100);

            int key2 = ((absJ % 100) / 10) * 10 + (absI % 100) / 10;;

            int key3 = (absJ % 10) * 10 + (absI % 10);

            double[][][] currentQuad = quadIndex[quad];

            if (currentQuad[key1] == null)
            {
                currentQuad[key1] = new double[100][];
            }

            if (currentQuad[key1][key2] == null)
            {
                currentQuad[key1][key2] = new double[100];
            }

            // Finally accumulate
            currentQuad[key1][key2][key3] += value;

        }

        public double this[int i, int j]
        {
            get
            {
                int quad = i >= 0 ? (j >= 0 ? 0 : 1) : (j >= 0 ? 2 : 3);

                int absI = Math.Abs(i);
                int absJ = Math.Abs(j);

                int key1 = (absJ / 100) * 64 + (absI / 100);

                int key2 = ((absJ % 100) / 10) * 10 + (absI % 100) / 10; ;

                int key3 = (absJ % 10) * 10 + (absI % 10);

                if (quadIndex[quad][key1] != null)
                {
                    if (quadIndex[quad][key1][key2] != null)
                    {
                        return quadIndex[quad][key1][key2][key3];
                    }
                }

                return 0.0;
            }
        }

        public double Max
        {
            get
            {
                double max = double.MinValue;
                foreach (var split in quadIndex)
                {
                    foreach (var subIndex in split)
                    {
                        if (subIndex == null)
                            continue;

                        foreach (var array in subIndex)
                        {
                            if (array == null)
                                continue;

                            for (int i = 0; i < 100; i++)
                            {
                                if (array[i] > max)
                                {
                                    max = array[i];
                                }
                            }
                        }
                    }
                }
                return max;
            }
        }

    }


    class AccumulatorGridUnbounded
    {
        private Dictionary<Point, double[][]>[] quadIndex = { new Dictionary<Point, double[][]>(),
                                                                                new Dictionary<Point, double[][]>(),
                                                                                new Dictionary<Point, double[][]>(),
                                                                                new Dictionary<Point, double[][]>() };

        public void Accumulate(int i, int j, double value)
        {
            int quad = i >= 0 ? (j >= 0 ? 0 : 1) : (j >= 0 ? 2 : 3);

            // + + 0
            // + - 1
            // - + 2
            // - - 3

            int absI = Math.Abs(i);
            int absJ = Math.Abs(j);

            int keyI = absI / 100;
            int keyJ = absJ / 100;

            int subI = (absI % 100) / 10;
            int subJ = (absJ % 100) / 10;

            int subsubI = absI % 10;
            int subsubJ = absJ % 10;

            Dictionary<Point, double[][]> currentQuad = quadIndex[quad];

            Point indexPoint = new Point(keyI, keyJ);

            if (!currentQuad.ContainsKey(indexPoint))
            {
                currentQuad.Add(indexPoint, new double[100][]);
            }

            Point subIndexPoint = new Point(subI, subJ);

            if (currentQuad[indexPoint][subJ * 10 + subI] == null)
            {
                currentQuad[indexPoint][subJ * 10 + subI] = new double[100];
            }

            // Finally accumulate
            currentQuad[indexPoint][subJ * 10 + subI][subsubJ * 10 + subsubI] += value;

        }

        public double this[int i, int j]
        {
            get
            {
                int quad = i >= 0 ? (j >= 0 ? 0 : 1) : (j >= 0 ? 2 : 3);

                int absI = Math.Abs(i);
                int absJ = Math.Abs(j);

                int keyI = absI / 100;
                int keyJ = absJ / 100;
                Point p1 = new Point(keyI, keyJ);

                int subI = (absI % 100) / 10;
                int subJ = (absJ % 100) / 10;
                Point p2 = new Point(subI, subJ);

                int subsubI = absI % 10;
                int subsubJ = absJ % 10;

                if (quadIndex[quad].ContainsKey(p1))
                {
                    if (quadIndex[quad][p1][subJ * 10 + subI] != null)
                    {
                        return quadIndex[quad][new Point(keyI, keyJ)][subJ * 10 + subI][subsubJ * 10 + subsubI];
                    }
                }

                return 0.0;
            }
        }

        public double Max
        {
            get
            {
                double max = double.MinValue;
                foreach (var split in quadIndex)
                {
                    foreach (var subIndex in split.Values)
                    {
                        foreach (var array in subIndex)
                        {
                            if (array == null)
                                continue;

                            for (int i = 0; i < 10; i++)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    if (array[j * 10 + i] > max)
                                    {
                                        max = array[j * 10 + i];
                                    }
                                }
                            }
                        }
                    }
                }
                return max;
            }
        }

    }



}
