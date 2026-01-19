using System;

class Program
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int upto = int.Parse(Console.ReadLine());

        int[] result = MultiplicationRow(n, upto);

        Console.WriteLine(string.Join(" ", result));
    }

    static int[] MultiplicationRow(int n, int upto)
    {
        int[] row = new int[upto];

        for (int i = 1; i <= upto; i++)
        {
            row[i - 1] = n * i;
        }

        return row;
    }
}
