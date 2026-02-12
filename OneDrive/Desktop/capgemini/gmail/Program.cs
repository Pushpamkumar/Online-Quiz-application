using System;
using System.Text.RegularExpressions;

class Match{
    public bool Matching(string input){
        if(string.IsNullOrWhiteSpace(input)){
            return false;
        }

        string pattern =@"^[A-Za-z0-9]([A-Za-z0-9._]{0,28}[A-Za-z0-9])?@gmail\.com$";
        bool valid= Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        return valid;
    }
}

class Program{
    public static void Main(){
        Console.WriteLine("Enter a string: ");
        string mou=Console.ReadLine();

        Match m1= new Match();
        bool push = m1.Matching(mou);

        if(!push){
            Console.WriteLine("Not a Valid Email");

        }else{
            Console.WriteLine("Valid Email");
        }


    }
}