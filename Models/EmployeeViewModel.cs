using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNummber { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string TaxId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateJoined { get; set; }
    }
}
