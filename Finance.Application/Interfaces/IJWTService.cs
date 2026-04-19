using Finance.Application.ViewModel;
using Finance.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface IJWTService
    {
        string GenerateToken(UserSelectDto user);
    }
}
