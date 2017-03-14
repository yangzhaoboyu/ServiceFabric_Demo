namespace Employee.Domain.Interface.Models.Request
{
    public class UserLoginRequestModel
    {
        /// <summary>
        ///     Gets or sets the cell phone.
        /// </summary>
        public string CellPhone { get; set; }

        /// <summary>
        ///     Gets or sets the pass word.
        /// </summary>
        public string PassWord { get; set; }
    }
}