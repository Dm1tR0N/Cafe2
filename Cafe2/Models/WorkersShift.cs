using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class WorkersShift
{
    public int IdworkersShift { get; set; }

    public int Iduser { get; set; }

    public int IdtypeWorkShift { get; set; }

    public virtual User IduserNavigation { get; set; } = null!;

    public virtual TypeWorksShift IdworkersShiftNavigation { get; set; } = null!;
}
