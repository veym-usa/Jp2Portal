using System.Collections.Generic;
using VEYMService.Models;

namespace VEYMServices.Models
{
    public class Training
    {
        public string trainingID { get; set; }
        public string trainingName { get; set; }
        public int trainingUserCapacity { get; set; }
        public int currentTrainingUserCount{ get; set; }
        public List<TrainingUserItem> signupList { get; set; }
    }
}