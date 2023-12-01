using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class UserDocument
{
    public int IduserDocument { get; set; }

    public byte[] ScanContract { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
