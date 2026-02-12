using System;

class Reverse{

    public bool rev(string input){
        char [] arr= input.ToCharArray();
        int b=0;
        int a=input.Length-1;
        while(b<a){
            char temp=arr[b];
            arr[b]=arr[a];
            arr[a]=temp;
            b++;
            a--;
        }
        // return new string(arr);
        string m1= new string(arr);
        if(m1==input){
            return true;
        }
        return false;
    }
}


class Program{
    public static void Main(){
        Console.WriteLine("Enter a string: ");
        string ask= Console.ReadLine();

        Reverse m1 = new Reverse();
        Console.WriteLine(m1.rev(ask));


    }
}