using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class TypeWorksShift
{
    public int IdtypeWorksShift { get; set; }

    public DateTime StartWorksShift { get; set; }

    public DateTime EndWorksShift { get; set; }

    public decimal WorkingRate { get; set; }

    public virtual WorkersShift? WorkersShift { get; set; }
}
