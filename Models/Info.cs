using System.Collections.Generic;

namespace VEYMService.Models
{
    public class Info
    {
        public string masterId { get; set; }
        public int numberOfTrainings { get; set; }
        public int numberOfUsers { get; set; }
        public List<string> listOfAdminEmailAddresses { get; set; }
    }
}