using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers;

public interface IConverter
{
    public byte[] ConvertImageToByte(Guid id);
}
