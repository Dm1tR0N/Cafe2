using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class HistoryUser
{
    public int IdhistoryUsers { get; set; }

    public int Iduser { get; set; }

    public int Status { get; set; }

    public DateTime Date { get; set; }

    public virtual User IduserNavigation { get; set; } = null!;

    public virtual EmployeeStatus StatusNavigation { get; set; } = null!;
}
