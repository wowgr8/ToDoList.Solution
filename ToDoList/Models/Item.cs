using System.Collections.Generic;
namespace ToDoList.Models
{
  public class Item
  {
    public string Description {get; set;}
    private static List<Item> _instances = new List<Item> {};//This is a static variable.
    public static List<Item> GetAll()
    {
      return _instances;
    }
    public static void ClearAll()
    {
      _instances.Clear();
    }

    public Item(string description)//This is the constructor.
    {
      Description = description;
      _instances.Add(this);
    }

  }
}