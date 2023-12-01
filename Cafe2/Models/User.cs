using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class User
{
    public int Iduser { get; set; }

    public string FirstName { get; set; } = null!;

    public string SecondName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public byte[]? Photo { get; set; }

    public int? Iddocuments { get; set; }

    public int? Idgroup { get; set; }

    public virtual ICollection<HistoryUser> HistoryUsers { get; set; } = new List<HistoryUser>();

    public virtual UserDocument? IddocumentsNavigation { get; set; }

    public virtual Group? IdgroupNavigation { get; set; }

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

    public virtual ICollection<WorkersShift> WorkersShifts { get; set; } = new List<WorkersShift>();
}
