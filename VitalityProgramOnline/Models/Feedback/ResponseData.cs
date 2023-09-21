using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace VitalityProgramOnline.Models.Feedback
{
    public class ResponseData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponseDataId { get; set; }
        public List<QuestionAnswerPair>? Responses { get; set; }

        public int ResponseId { get; set; }

        [ForeignKey("ResponseId")]
        public FeedbackResponse FeedbackResponse { get; set; }
    }
}
