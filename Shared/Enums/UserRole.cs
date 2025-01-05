using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums;

public enum UserRole
{
    [Description("Super Admin")]
    Master,
    [Description("Admin")]
    Admin,
    [Description("Cashier")]
    Cashier,
    [Description("Manager")]
    Manager,
    [Description("Store")]
    Store,
    [Description("Other Staff")]
    Staff
}
