using Microsoft.AspNetCore.Mvc;
using ToDoList.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;


namespace ToDoList.Controllers
{
  [Authorize] // this allows acces to the ItemsController only if a user is logged in. In this scenario, the entirety of the controller is shielded from unauthorized users. We can negate this by including an [AllowAnonymous] attribute above any specific methods that we want unauthorized users to have access to. For example, we could put [AllowAnonymous] above the Index route, if we want users to be able to see a list of items, but require authorization before they view details. 
  public class ItemsController : Controller
  {
    private readonly ToDoListContext _db;
    private readonly UserManager<ApplicationUser> _userManager; // We need an instance of UserManager to work with signed-in users.

    //also included a constructor to instantiate private readonly instances of the database and the UserManager.
    public ItemsController(UserManager<ApplicationUser> userManager, ToDoListContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    //We start by using the async modifier because this action will run asynchronously. Because the action is asynchronous, it also returns a Task containing an action result.
    public async Task<ActionResult> Index()
    {
      // we locate the unique identifier for the currently-logged-in user and assign it the variable name userId. [this] refers to the ItemController itself. FindFirst() is a method that locates the first record that meets the provided criteria.
      //This method takes ClaimTypes.NameIdentifier as an argument. This is why we need a using directive for System.Security.Claims. We specify ClaimTypes.NameIdentifier to locate the unique ID associated with the current account. NameIdentifier is a property that refers to an Entity's unique ID.
      //Finally, we include the ? operator, this is called an existential operator. It states that we should only call the property to the right of the ? if the method to the left of the ? doesn't return null. Essentially, the code states that if this.User.FindFirst(ClaimTypes.NameIdentifier) returns null, don't call the property to the right of the existential operator. However, if it doesn't return null, it retrieves Value property.
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      // Once we have the userId value, we're ready to call our async method
      //First we call the UserManager service that we've injected into this controller. We call the FindByIdAsync() method, which is a built-in Identity method used to find a user's account by their unique ID. Then we provide the userId we just located as an argument to FindByIdAsync()
      //because [Async] suffix in this methods name, we know it runs asynchronously. We include the await keyword so the code will wait for Identity to locate the correct user before moving on.
      var currentUser = await _userManager.FindByIdAsync(userId);
      //Finally, we create a variable to store a collection containing only the Items that are associated with the currently-logged-in user's unique Id property
      //We use the Where() method, which is a LINQ method we can use to query a collection in a way that echoes the logic of SQL. We can use Where() to make many different kinds of queries, as the method accepts an expression to filter our results.
      //In this case, we're simply asking Entity to find items in the database where the user id associated with the item is the same id as the id that belongs to the currentUser. This ensures users only see their own tasks in the view.
      var userItems = _db.Items.Where(entry => entry.User.Id == currentUser.Id).ToList();
      return View(userItems);
    }

    public ActionResult Create()
    {
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Item item, int CategoryId)
    {
      //The first two lines of this action are exactly the same as the first two lines of our Index() action. We start by finding the value of the current user. Then we associate the current user with the Item's User property. This makes the association so that an Item belongs to a User
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      item.User = currentUser;
      // Finally we add the item to the database and save it.
      _db.Items.Add(item);
      _db.SaveChanges();
      if (CategoryId != 0)
      {
          _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisItem = _db.Items
        .Include(item => item.JoinEntities)
        .ThenInclude(join => join.Category)
        .FirstOrDefault(item => item.ItemId == id);
      return View(thisItem);

    }

    public ActionResult Edit(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult Edit(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.Entry(item).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult AddCategory(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult AddCategory(Item item, int CategoryId)
    {
        if (CategoryId != 0)
        {
          _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
          _db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult DeleteCategory(int joinId)
    {
        var joinEntry = _db.CategoryItem.FirstOrDefault(entry => entry.CategoryItemId == joinId);
        _db.CategoryItem.Remove(joinEntry);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      _db.Items.Remove(thisItem);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}

// // PRE ENTITY CONTROLLER BELOW:

// // using Microsoft.AspNetCore.Mvc;
// // using ToDoList.Models;
// // using System.Collections.Generic;

// // namespace ToDoList.Controllers
// // {
// //   public class ItemsController : Controller
// //   {
// //     // [HttpGet("/items")]
// //     // public ActionResult Index()
// //     // {
// //     //   List<Item> allItems = Item.GetAll();
// //     //   return View(allItems);
// //     // }

// //     [HttpGet("/categories/{categoryId}/items/new")]
// //     public ActionResult New(int categoryId)
// //     {
// //       Category category = Category.Find(categoryId);
// //       return View(category);
// //     }

// //     // [HttpPost("/items")]
// //     // public ActionResult Create(string description)
// //     // {
// //     //   Item myItem = new Item(description);
// //     //   return RedirectToAction("Index");
// //     // }

// //     [HttpGet("/categories/{categoryId}/items/{itemId}")]
// //     public ActionResult Show(int categoryId, int itemId)
// //     {
// //       Item item = Item.Find(itemId);
// //       Category category = Category.Find(categoryId);
// //       Dictionary<string, object> model = new Dictionary<string, object>();
// //       model.Add("item", item);
// //       model.Add("category", category);
// //       return View(model);
// //     }

// //     [HttpPost("/items/delete")]
// //     public ActionResult DeleteAll()
// //     {
// //       Item.ClearAll();
// //       return View();
// //     }
// //   }
// // }