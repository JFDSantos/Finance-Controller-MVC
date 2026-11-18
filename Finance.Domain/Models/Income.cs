using System.Security.Cryptography.X509Certificates;

using Finance.Domain.Models.Enums;

namespace Finance.Domain.Models
{   
    public class Income : Moviment
    {

        public Income()
        {
            Type = EMovimentType.Income;
        }
    }
}
