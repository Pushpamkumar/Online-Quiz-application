using System;
class Program {
    static int add(int a, int b){
        return a+b;
    }
    static void main(){
        Console.Write("enter number1");
        int m=int.Parse(Console.ReadLine());
         Console.Write("enter number2");
         int m1=int.Parse(Console.ReadLine());
         int mk=add(m,m1);
         Console.WriteLine("result is %d"+ mk);
    }

}  