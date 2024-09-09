//Product Controller
using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ProductController : ControllerBase
{
    //DI > Make new Object and Constructor
    private readonly ApplicationDbContext _context;

    //IwebHostEnvironment is?
    //ContentRootPath: Route to specific folder
    //WebRootPath:Route to wwwroot folder
    private readonly IWebHostEnvironment _env;

    //Constructor
    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    //Endpont for Get All Product
    [HttpGet]
    public ActionResult<product> getProducts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 2,
        [FromQuery] string? searchQuery = null,
        [FromQuery] int? selectedCategory = null
    )
    {
        //skip is การข้าวข้อมูล
        int skip = (page - 1) * limit;
        //LINQ stand for "Language Integrated Query" // select* from product
        //var products = _context.products.ToList();

        //Get with condition
        //var products = _context.products.Where(x => x.unitinstock>=10).ToList();

        //Connect product to category
        var query = _context.products
        .Join(
            _context.categories,
            p => p.categoryid,
            c => c.categoryid,
            (p, c) => new
            {
                p.productid,
                p.productname,
                p.unitprice,
                p.unitinstock,
                p.productpicture,
                p.categoryid,
                p.createddate,
                p.modifieddate,
                c.categoryname
            }
        );

        //If search Query
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(p => EF.Functions.ILike(p.productname!, $"%{searchQuery}%"));
        }

        //If selected Category
        if (selectedCategory.HasValue)
        {
            query = query.Where(p => p.categoryid == selectedCategory.Value);
        }
        //Count all products
        var totalRecords = query.Count();

        //Get products
        var products = query
        .Skip(skip)
        .Take(limit)
        .ToList();

        //Response to client as JSON
        return Ok(
            new
            {
                Total = totalRecords,
                Products = products
            }
        );
    }

    //Endpont for get by ID
    [HttpGet("{id}")]
    public ActionResult<product> getProduct(int id)
    {
        //LINQ for get products by ID // slect * from product where id = id
        //Connect product to category
        var product = _context.products
        .Join(
            _context.categories,
            p => p.categoryid,
            c => c.categoryid,
            (p, c) => new
            {
                p.productid,
                p.productname,
                p.unitprice,
                p.unitinstock,
                p.productpicture,
                p.categoryid,
                p.createddate,
                p.modifieddate,
                c.categoryname
            }
        )
        .FirstOrDefault(p => p.productid == id);

        if (product == null)
        {
            return NotFound();
        }

        //Response to client as JSON
        return Ok(product);
    }

    //Endpont for Create Product
    [HttpPost]
    // public ActionResult<product> CreateProduct([FromBody] product product) => Basic Use
    public async Task<ActionResult<product>> CreateProduct([FromForm] product product, IFormFile? image)
    {
        //Add data in Products Table
        _context.products.Add(product);

        //If upload image
        if (image != null)
        {
            //Create image file name by guid
            string fildeName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            //Define path to uploads folder
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
            //Check there is uploads folder
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fildeName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            //Save image file in Database
            product.productpicture = fildeName;
        } else
        {
            product.productpicture = "noimg.jpg";
        }

        _context.SaveChanges();

        //Response to client as JSON
        return Ok(product);
    }

    //Endpont for Update Product
    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id, [FromBody] product product)
    {
        //Find product by ID
        var productData = _context.products.Find(id); //select * from product where id =1

        if (productData == null)
        {
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
    public ActionResult<product> DeleteProduct(int id)
    {
        //Find product by ID
        var product = _context.products.Find(id); //select * from product where id =1

        if (product == null)
        {
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