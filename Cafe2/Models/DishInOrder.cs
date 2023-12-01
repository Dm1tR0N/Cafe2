using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class DishInOrder
{
    public int IddishInOrder { get; set; }

    public int Idorder { get; set; }

    public int Iddish { get; set; }

    public int Count { get; set; }

    public virtual Menu IddishNavigation { get; set; } = null!;

    public virtual Order IdorderNavigation { get; set; } = null!;
}
