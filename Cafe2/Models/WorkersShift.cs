using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class WorkersShift
{
    public int IdworkersShift { get; set; }

    public int Iduser { get; set; }

    public int IdtypeWorkShift { get; set; }

    public DateTime dateWorkersShift { get; set; }

    public virtual User IduserNavigation { get; set; } = null!;

    public virtual TypeWorksShift IdworkersShiftNavigation { get; set; } = null!;
}

// Scaffold-DbContext "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=2583"
