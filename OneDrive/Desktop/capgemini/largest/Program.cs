using System;

class Largest{

    public int large(int[] input){
        int max=input[0];
        for(int i=1;i<input.Length;i++){
            if(input[i]>max){
                max=input[i];
            }
        }
        return max;

    }
}


class Program{
    public static void Main(){
        Console.Write("Enter input size arr:");
        int m = Convert.ToInt32(Console.ReadLine());
        int [] arr = new int[m];
        for(int i=0;i<m;i++){
            arr[i]= Convert.ToInt32(Console.ReadLine());
        }
        Largest m1= new Largest();
        int result = m1.large(arr);

        Console.WriteLine("Max Element is : "+  result);

    }
}