using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace VitalityProgramOnline.Models.User.Settings
{
    public class UserBotSettings : EmptyBotSettings
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string Id { get; set; }

        public long UserId { get; set; }

        public TimeSpan? MorningTime { get; set; }
        public TimeSpan? EveningTime { get; set; }


        public ApplicationUser User { get; set; }

        public override string ToString()
        {
            return $"<b>Самое ранне сообщение с</b>: {MorningTime.Value.Hours}:{MorningTime.Value.Minutes} часов утра.\n\n<b>Самое позднее до</b>: {EveningTime} часов";
        }
    }
}
