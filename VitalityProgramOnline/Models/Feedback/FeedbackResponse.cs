using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VitalityProgramOnline.Models.User;


namespace VitalityProgramOnline.Models.Feedback
{
    public class FeedbackResponse
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponseId { get; set; }

        public DateTime ResponseDateTime { get; set; }

        public string Id { get; set; }

        public ResponseData ResponseData { get; set; }

        [ForeignKey("Id")]
        public ApplicationUser User { get; set; }
    }
}