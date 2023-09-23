using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Globalization;
using VitalityProgramOnline.Helper;
using System.ComponentModel.DataAnnotations;
using VitalityProgramOnline.Models.User;

namespace VitalityProgramOnline.Models.FoodDiary
{
    public class FoodDiaryEntry
    {
        #region Properties

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserIdForeignKey { get; set; }
        
        public long UserId { get; set; }
        
        public DateTime Date { get; set; }
        public MealType MealType { get; set; }
        public EatPurpose? EatPurpose { get; set; }
        public double? Weight { get; set; }
        public int MealTimeHours { get; set; }
        public int MealTimeMinutes { get; set; }
        public string? DishName { get; set; }
        public double? WaterAmount { get; set; }
        public double? ProteinAmount { get; set; }
        public double? FatAmount { get; set; }
        public double? CarbohydrateAmount { get; set; }
        public double Cost { get; set; }

        public ApplicationUser User { get; set; }

        #endregion


        #region Methods

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("Дата: ");
            stringBuilder.Append(Date.ToString("dd MMMM yyyy года", CultureInfo.GetCultureInfo("ru-RU")));


            if (Weight != null)
            {
                stringBuilder.Append(",\n Вес: ");
                stringBuilder.Append(Weight);
            }

            stringBuilder.Append(",\n Время приема пищи: ");
            stringBuilder.Append(MealTimeHours.ToString("D2"));
            stringBuilder.Append(":");
            stringBuilder.Append(MealTimeMinutes.ToString("D2"));


            if (MealType == MealType.Water)
            {
                stringBuilder.Append(",\n Типа блюда: ");
                stringBuilder.Append(GetMealTypeDescription(MealType));
            }

            if (WaterAmount != null)
            {
                stringBuilder.Append(",\n Количество воды: ");
                stringBuilder.Append(WaterAmount);
            }

            if (!string.IsNullOrEmpty(DishName))
            {
                stringBuilder.Append(",\n Название блюда: ");
                stringBuilder.Append(DishName);
            }

            if (EatPurpose != null)
            {
                stringBuilder.Append(",\n Цель употребления в пищу: ");
                stringBuilder.Append(GetEatPurpose(EatPurpose));
            }

            if (ProteinAmount != null)
            {
                stringBuilder.Append(",\n Количество белка: ");
                stringBuilder.Append(ProteinAmount);
            }

            if (FatAmount != null)
            {
                stringBuilder.Append(",\n Количество жира: ");
                stringBuilder.Append(FatAmount);
            }

            if (CarbohydrateAmount != null)
            {
                stringBuilder.Append(",\n Количество углеводов: ");
                stringBuilder.Append(CarbohydrateAmount);
            }

            stringBuilder.Append(",\n Стоимость блюда: ");
            stringBuilder.Append(Cost);

            return stringBuilder.ToString();
        }

        private string GetEatPurpose(EatPurpose? eatPurpose)
        {
            if (eatPurpose is { } eat)
            {
                switch (eat)
                {
                    case Helper.EatPurpose.Hunger:
                        return "Для утоления голода";
                    case Helper.EatPurpose.Health:
                        return "Для улучшения здоровья";
                    case Helper.EatPurpose.Weight:
                        return "Для контроля веса";
                    default:
                        return "Ничего";
                }
            }
            return "Ничего";
        }

        private string GetMealTypeDescription(MealType mealType)
        {
            switch (mealType)
            {
                case MealType.None:
                    return "Ничего";
                case MealType.Water:
                    return "Вода";
                case MealType.Eat:
                    return "Еда";
                default:
                    return mealType.ToString();
            }
        }

        #endregion
    }
}
