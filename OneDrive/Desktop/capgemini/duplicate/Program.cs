using System;
using System.Collections.Generics;

class Duplicate{

    public List<int> remove(int [] arr){
         HashSet <int> set=new HashSet<>();
         List<int> result = new List<>();
         foreach ( var num in arr){
            if(set.Add(num)){
                result.Add(num);
            }
         }
         return result;

    }
}

class Program{
        public static void Main(String[] args){
            Duplicate d= new Duplicate();
            
        }


}