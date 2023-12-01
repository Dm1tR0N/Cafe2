using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class PymentMethod
{
    public int IdpymentMethods { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
