using System;

namespace Employee.Service.Models.User
{
    public class UserState
    {
        public string CellPhone { get; set; }
        public string PassWord { get; set; }
        public string RealName { get; set; }
        public Guid UserIdentifier { get; set; }
    }
}