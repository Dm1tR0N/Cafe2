using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class Group
{
    public int Idgroup { get; set; }

    public string NameGroup { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
