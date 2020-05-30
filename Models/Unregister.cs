namespace VEYMService.Models
{
    public class Unregister
    {
        public string membershipID { get; set; }
        public string trainingID { get; set; }
        public bool sendToTheBack { get; set; }
    }
}