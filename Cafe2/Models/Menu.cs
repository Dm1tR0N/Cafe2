using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class Menu
{
    public int Iddish { get; set; }

    public string NameDish { get; set; } = null!;

    public string Description { get; set; } = null!;

    public TimeOnly CookingTime { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<DishInOrder> DishInOrders { get; set; } = new List<DishInOrder>();
}
