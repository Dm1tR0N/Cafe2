using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class Table
{
    public int Idtable { get; set; }

    public string NameTable { get; set; } = null!;

    public int? UserTableId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? UserTable { get; set; }
}
