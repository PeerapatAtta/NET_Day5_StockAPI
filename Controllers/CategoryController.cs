//Endpoint for Category
using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStockAPI.Controllers;

//[Authorize(Roles = UserRoleModel.Admin+"," + UserRoleModel.Manager)]
[Authorize]
[ApiController]
[Route("api/[controller]")]

public class CategoryController : ControllerBase
{
    //DI > Object and constructor
    private readonly ApplicationDbContext _context;
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    //CRUD Category
    //Endpont for Get All Category
    [HttpGet]
    public ActionResult<category> getCategories()
    {
        //LINQ stand for "Language Integrated Query" // select* from category        
        var categories = _context.categories.ToList();
        //Response to client as JSON
        return Ok(categories);
    }

    //Endpont for get by ID
    [HttpGet("{id}")]
    public ActionResult<category> getCategory(int id)
    {
        //LINQ for get categories by ID // slect * from category where id = id
        var category = _context.categories.Find(id);
        if (category == null)
        {
            return NotFound();
        }
        //Response to client as JSON
        return Ok(category);
    }

    //Endpont for Add Category
    [HttpPost]
    public ActionResult<category> AddCategory([FromBody] category category)
    {
        //Add data in Catagories Table
        _context.categories.Add(category); //insert into category values(...)
        _context.SaveChanges(); //commit

        //Response to client as JSON
        return Ok(category);
    }

    //Endpont for Update Category
    [HttpPut("{id}")]
    public ActionResult<category> UpdateCategory(int id,[FromBody] category category){
        //Find category by ID
        var cat = _context.categories.Find(id); //select * from catogory where id =1
        if (cat == null)
        {
            return NotFound();
        }
        //Update data in Catagories Table
        cat.categoryname = category.categoryname; //update cat set categoryname = "...." where id = 1
        cat.categorystatus = category.categorystatus; //update cat set categorystatus = "...." where id = 1
        _context.SaveChanges(); //commit
        //Response to client as JSON
        return Ok(cat);
    }

    //Endpont for Delete Category
    [HttpDelete("{id}")]
    public ActionResult<category> DeleteCategory(int id){
        //Find category by ID
        var cat = _context.categories.Find(id); //select * from catogory where id =1
        if (cat == null)
        {
            return NotFound();
        }
        _context.categories.Remove(cat); //delete from category where id = 1
        _context.SaveChanges(); //commit
        //Response to client as JSON
        return Ok(cat);
    }

}