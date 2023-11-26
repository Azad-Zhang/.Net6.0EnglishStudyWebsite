using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain
{
    public class ResetInfoRequest
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNum { get; set; }
    }
}
