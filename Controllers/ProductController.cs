//Product Controller
using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ProductController : ControllerBase{
    //DI > Object+Constructor
    private readonly ApplicationDbContext _context;
    public ProductController(ApplicationDbContext context){
        _context = context;
    }

    //Endpont for Get All Product
    [HttpGet]
    public ActionResult<product> getProducts(){
        //LINQ stand for "Language Integrated Query" // select* from product
        var products = _context.products.ToList();
        //Response to client as JSON
        return Ok(products);
    }

    //Endpont for get by ID
    [HttpGet("{id}")]
    public ActionResult<product> getProduct(int id){
        //LINQ for get products by ID // slect * from product where id = id
        var product = _context.products.Find(id);
        if (product == null){
            return NotFound();
        }
        //Response to client as JSON
        return Ok(product);
    }

    //Endpont for Create Product
    [HttpPost]
    public ActionResult<product> CreateProduct([FromBody] product product){
        //Add data in Products Table
        _context.products.Add(product);
        _context.SaveChanges();

        //Response to client as JSON
        return Ok(product);
    }

    //Endpont for Update Product
    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id,[FromBody] product product){
        //Find product by ID
        var productData = _context.products.Find(id); //select * from product where id =1

        if (productData == null){
            return NotFound();
        }

        //Update data in Products Table
        productData.productname = product.productname; //update pro set productname = "...." where id = 1
        productData.unitprice = product.unitprice;
        productData.unitinstock = product.unitinstock;
        productData.productpicture = product.productpicture;
        productData.categoryid = product.categoryid;
        productData.modifieddate = product.modifieddate;

        //Update data in Products Table
        _context.SaveChanges(); 

        //Response to client as JSON
        return Ok(productData);
    }

    //Endpont for Delete Product
    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id){
        //Find product by ID
        var product = _context.products.Find(id); //select * from product where id =1

        if (product == null){
            return NotFound();
        }

        //Delete data in Products Table
        _context.products.Remove(product); 
        //Save Changes
        _context.SaveChanges(); 
        //Response to client as JSON
        return Ok(product);
    }
}