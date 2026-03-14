using System.ComponentModel.DataAnnotations;

namespace Furniture_Store.ViewModels;

public class CartPageViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Total => Items.Sum(x => x.LineTotal);

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Получатель")]
    public string RecipientName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 5)]
    [Display(Name = "Адрес доставки")]
    public string DeliveryAddress { get; set; } = string.Empty;
}
