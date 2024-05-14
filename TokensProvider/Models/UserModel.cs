using System.ComponentModel.DataAnnotations;

namespace TokensProvider.Models;

public class UserModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email address", Prompt = "Enter your email address", Order = 2)]
    [Required(ErrorMessage = "Email address is required")]
    [RegularExpression(@"^(([^<>()\]\\.,;:\s@\""]+(\.[^<>()\]\\.,;:\s@\""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;
}
