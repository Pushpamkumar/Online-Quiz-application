using System;
class Student{
    private int id;
    private string name;
    public int Id{
            get{return id;}
            set{
                if(value>0){
                    id=value;
                }else{
                    Console.WriteLine("Invalid ID");
                }
            }
    
    
    }
    public string Name{
                get { return name;}
                set {
                    if(!string.IsNullOrEmpty(value)){
                        name=value;
                    }else{
                        Console.WriteLine("Invalid Name");
                    }
                }
    }

    public void Display(){
        Console.WriteLine("Id is "+ Id);
        Console.WriteLine("Name is "+Name);
    }
    public void SetData(int id, string name){
        this.id=id;
        this.name=name;
    }

}

class Program{
    static void Main(String[] args){
        Student p1=new Student();
        Console.WriteLine("Enter ID:");
        int Roll=Convert.ToInt32(Console.ReadLine());
        p1.Id=Roll;
        Console.WriteLine("Enter Name:");
        string NaStu=Console.ReadLine();
        p1.Name=NaStu;
        // p1.SetData(p1.Name, p1.Id);
        p1.Display();


    }
}