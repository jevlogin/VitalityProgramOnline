using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace VitalityProgramOnline.Models.Feedback
{
    public class QuestionAnswerPair
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PairId { get; set; }

        public string? Question { get; set; }
        public string? Answer { get; set; }

        public int ResponseDataId { get; set; }

        [ForeignKey("ResponseDataId")]
        public ResponseData ResponseData { get; set; }
    }
}