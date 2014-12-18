using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xinanshuji
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dtBeginTime = DateTime.Now;
            int a, b, c;
            GF gf1 = new GF();
            for (int i = 0; i < 100000; i++)
            {
                a = RandomStatic.ProduceIntRandom(0, 255);
                b = RandomStatic.ProduceIntRandom(0, 255);
                c = gf1.Mul(a, b);
            }
            DateTime dtEndTime = DateTime.Now;
            TimeSpan ts = dtEndTime.Subtract(dtBeginTime);
            Console.WriteLine("乘法所花的时间:");
            Console.WriteLine(ts.ToString());
            DateTime dtBeginTime1 = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                a = RandomStatic.ProduceIntRandom(1, 255);
                b = gf1.Inverse(a);
            }
            DateTime dtEndTime1 = DateTime.Now;
            TimeSpan ts1 = dtEndTime1.Subtract(dtBeginTime1);
            Console.WriteLine("求逆元所花的时间:");
            Console.WriteLine(ts1.ToString());
           

            int[] SBox = new int[256];
            int[,] SBinary = new int[256, 8];
            int[,] Bias=new int[256,256];
            int [] y = new int [8];
            int [] z={1,1,0,0,0,1,1,0};

            //  转化为二进制
            for (int i = 0; i < 256; i++)
            {
                int n = i;
                for (int j = 0; j < 8; j++)
                {
                    SBinary[i, j] = n % 2;
                    n = n / 2;
                }
            }
           
            //  计算SBox------Subbyte
            for (int i = 0; i < 256; i++)
                {
                    if (i != 0) b = gf1.Inverse(i);
                    else b = 0;
                    int n = 1;
                    SBox[i] = 0;
                    for (int k = 0; k < 8; k++)
                    {
                        y[k] = SBinary[b, k] + SBinary[b, (k + 4) % 8] + SBinary[b, (k + 5) % 8] + SBinary[b, (k + 6) % 8] + SBinary[b, (k + 7) % 8] + z[k];
                        y[k] = y[k] % 2;
                        SBox[i] += y[k] * n;
                        n = n * 2;
                    }
                }

          //  for (int i = 0; i < 256; i++)
          //  {
          //      Console.Write("i=");
           //     Console.Write(i);
          //      Console.Write("    y=");
           //     Console.WriteLine(SBox[i]);
           // }
            //   线性分析
            for (int input_sum = 0; input_sum < 256; input_sum++)
                for (int output_sum = 0; output_sum < 256; output_sum++)
                {
                    Bias[input_sum, output_sum] = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        int m=0, n=0;
                        for (int j = 0; j < 8; j++)
                        {
                            m = m + SBinary[input_sum, j] * SBinary[i, j];
                            n = n + SBinary[output_sum, j] * SBinary[SBox[i], j];
                        }
                        m = m % 2;
                        n = n % 2;
                        if (m == n) Bias[input_sum, output_sum]++;
                    }
                    Bias[input_sum, output_sum] -= 128;
                }

            int[] input=new int[10];
            int[] output=new int[10];
            int[] g = new int[10];

            //   求偏差最大的前十个线性方程
            for (int i = 0; i < 10; i++)
            {
                int m,n1=0,n2=0;
                m=Bias[0,0];
                for(int j=0; j<256;j++)
                    for(int k=0; k<256;k++)
                        if (Bias[j, k] > m)
                        {
                            m = Bias[j, k];
                            n1 = j;
                            n2 = k;
                        }
                input[i] = n1;
                output[i] = n2;
                g[i] = m;
                Bias[n1, n2] = -128;
            }
            //   输出
            Console.WriteLine("偏差最大的前十个线性方程：");
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (j != 0) Console.Write('+');
                    Console.Write(SBinary[input[i], j]);
                    Console.Write('x');
                    Console.Write(j);
                }
                Console.Write('=');
                for (int j = 0; j < 8; j++)
                {
                    if (j != 0) Console.Write('+');
                    Console.Write(SBinary[output[i], j]);
                    Console.Write('x');
                    Console.Write(j);
                }
                Console.Write("     Bias=" );
                Console.Write(g[i]);
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }

    class GF
    {
        private int[] Table = new int[256];
        private int[] ArcTable = new int[256];
        private int[] InverseTable = new int[256];

        public GF()
        {
            Table[0] = 1;
            int i;
            for (i = 1; i < 255; ++i)
            {
                Table[i] = (Table[i - 1] << 1) ^ Table[i - 1];
                bool state = Convert.ToBoolean(Table[i] & 0x100);
                if (state)
                {
                    Table[i] ^= 0x11B;
                }
            }
            for (i = 0; i < 255; ++i)
                ArcTable[Table[i]] = i;
            InverseTable[0] = 0;
            for (i = 1; i < 256; ++i)
            {
                int k = ArcTable[i];
                k = 255 - k;
                k %= 255;
                InverseTable[i] = Table[k];
            }
        }

        public int Plus(int x, int y)
        {
            return x ^ y;
        }

        public int Minus(int x, int y)
        {
            return x ^ y;
        }

        public int Mul(int x, int y)
        {
            if ((x == 0) || (y == 0))
                return 0;
            return Table[(ArcTable[x] + ArcTable[y]) % 255];
        }

        public int Inverse(int x)
        {
            if (x == 0)
            {
                Console.WriteLine("0 does not hava a inverse");
                return 0;
            }
            else return InverseTable[x];
        }

        public int Div(int x, int y)
        {
            if (y == 0)
            {
                Console.WriteLine("y can not be 0");
                return 0;
            }
            else return this.Mul(x, InverseTable[y]);
        }
    }
    public static class RandomStatic
    {
        public static double ProduceDblRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());//使用Guid的哈希值做种子 
            return r.NextDouble();
        }
        public static int ProduceIntRandom(int minValue, int maxValue)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return r.Next(minValue, maxValue + 1);
        }
    }
}
