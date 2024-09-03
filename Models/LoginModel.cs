//Model for Login page
using System.ComponentModel.DataAnnotations;

namespace DotnetStockAPI.Models;

public class LoginModel{

    [Required(ErrorMessage = "User Name is required")]
    [StringLength(50, ErrorMessage = "User Name is too long")]
    [MinLength(3, ErrorMessage = "User Name is too short")]    
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password is too short")]
    public string? Password { get; set; }
}

