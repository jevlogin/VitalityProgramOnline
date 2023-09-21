using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using VitalityProgramOnline.Helper;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;


namespace VitalityProgramOnline.Models.User
{
    public class ProgressUsers
    {
        #region Fields

        public event Action<ProgressUsers>? ProgressUpdated;

        private DateTime _dateTimeOfTheNextStep;
        private DateTime _dateNextDay;
        private UpdateState _updateState = UpdateState.None;
        private int _currentDay;
        private int _currentStep;
        private bool _isTheNextStepSheduledInTime;
        private bool _isTheNextDaysUpdateIsCompleted;

        private Timer? _timerEvent;
        private Timer? _timerNextDay;

        #endregion


        #region Properties

        [Key]
        public long UserId { get; set; }
        public string Id { get; set; }

        [ForeignKey(nameof(Id))]
        public ApplicationUser User { get; set; }

        public UpdateState UpdateState
        {
            get
            {
                return _updateState;
            }
            set
            {
                _updateState = value;
                switch (_updateState)
                {
                    case UpdateState.None:
                        Console.WriteLine($"Нет обновлений - UpdateState.None");
                        ProgressUpdated?.Invoke(this);

                        break;
                    case UpdateState.FullUpdate:
                        Console.WriteLine($"Запускаем полное обновление");
                        ProgressUpdated?.Invoke(this);

                        break;
                    case UpdateState.UpdateDate:
                        Console.WriteLine($"Запускаем обновление даты и шагов выполнения");
                        StartCheckingDailyProgressUpdates();
                        StartCheckingForUpdatesOfProgressSteps();
                        ProgressUpdated?.Invoke(this);

                        break;
                }
            }
        }

        [Column]
        public DateTime DateNextDay
        {
            get => _dateNextDay;
            set
            {
                _dateNextDay = value;
                if (DateTime.Today < _dateNextDay)
                {
                    IsTheNextDaysUpdateIsCompleted = false;
                    if (_timerNextDay != null)
                    {
                        TimerNextDayDispose();
                    }
                    UpdateState = UpdateState.UpdateDate;
                }
            }
        }

        [Column]
        public DateTime DateTimeOfTheNextStep
        {
            get => _dateTimeOfTheNextStep;
            set
            {
                _dateTimeOfTheNextStep = value;
                if (DateTime.Today < _dateTimeOfTheNextStep)
                {
                    IsTheNextStepSheduledInTime = false;
                    if (_timerEvent != null)
                    {
                        TimerNextStepDispose();
                    }
                    UpdateState = UpdateState.UpdateDate;
                }
            }
        }

        [Column]
        public bool IsTheNextStepSheduledInTime
        {
            get => _isTheNextStepSheduledInTime;
            set
            {
                _isTheNextStepSheduledInTime = value;
            }
        }

        [Column]
        public bool IsTheNextDaysUpdateIsCompleted
        {
            get => _isTheNextDaysUpdateIsCompleted;
            set
            {
                _isTheNextDaysUpdateIsCompleted = value;
            }
        }

        [Column]
        public int CurrentDay
        {
            get => _currentDay;
            set
            {
                _currentDay = value;
                IsTheNextDaysUpdateIsCompleted = true;

                UpdateState = UpdateState.FullUpdate;
            }
        }

        [Column]
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                IsTheNextStepSheduledInTime = true;
                UpdateState = UpdateState.FullUpdate;
            }
        }

        #endregion


        #region ClassLifeCycles

        [JsonConstructor]
        public ProgressUsers(long userId, string id, int currentDay, int currentStep, DateTime dateTimeOfTheNextStep, DateTime dateNextDay,
            bool isTheNextStepSheduledInTime, UpdateState updateState)
        {
            UserId = userId;
            Id = id;
            _currentDay = currentDay;
            _currentStep = currentStep;
            _dateTimeOfTheNextStep = dateTimeOfTheNextStep;
            _dateNextDay = dateNextDay;
            _isTheNextStepSheduledInTime = isTheNextStepSheduledInTime;
            _isTheNextDaysUpdateIsCompleted = isTheNextStepSheduledInTime;
            _updateState = updateState;
        }

        #endregion


        #region ProgressStep

        private void StartCheckingForUpdatesOfProgressSteps()
        {
            if (DateTime.UtcNow.ToLocalTime() < DateTimeOfTheNextStep && !IsTheNextStepSheduledInTime)
            {
                if (_timerEvent == null)
                {
                    _timerEvent = new Timer
                    {
                        Interval = GetIntervalMilliseconds(DateTimeOfTheNextStep),
                        AutoReset = false,
                    };
                }
                else if (true)
                {
                    TimerNextStepDispose();
                    _timerEvent.Interval = GetIntervalMilliseconds(DateTimeOfTheNextStep);
                }
                _timerEvent.Elapsed += CheckSheduledEvent;
                _timerEvent.Start();
            }
            else if (!IsTheNextStepSheduledInTime && _timerEvent == null)
            {
                _timerEvent = new Timer
                {
                    Interval = 10000,
                    AutoReset = false,
                };
                _timerEvent.Elapsed += CheckSheduledEvent;
                _timerEvent.Start();
            }
        }

        private double GetIntervalMilliseconds(DateTime dateTimeOfTheNextStep)
        {
            return (dateTimeOfTheNextStep - DateTime.UtcNow.ToLocalTime()).TotalMilliseconds;
        }

        private void CheckSheduledEvent(object? sender, ElapsedEventArgs e)
        {
            CurrentStep++;
            if (_timerEvent != null)
            {
                TimerNextStepDispose();
            }
        }

        private void TimerNextStepDispose()
        {
            _timerEvent.Elapsed -= CheckSheduledEvent;
            _timerEvent.Close();
        }

        #endregion


        #region NextDay

        private void StartCheckingDailyProgressUpdates()
        {
            if (DateTime.UtcNow.ToLocalTime() < DateNextDay || !IsTheNextDaysUpdateIsCompleted)
            {
                if (_timerNextDay != null)
                    _timerNextDay.Close();

                _timerNextDay = new Timer
                {
                    Interval = (DateNextDay - DateTime.UtcNow.ToLocalTime()).TotalMilliseconds,
                    AutoReset = false
                };

                _timerNextDay.Elapsed += CheckEventNextDay;
                _timerNextDay.Start();
            }
        }

        private void CheckEventNextDay(object? sender, ElapsedEventArgs e)
        {
            _currentStep = 1;
            CurrentDay++;
            if (_timerNextDay != null)
            {
                TimerNextDayDispose();
            }
        }

        private void TimerNextDayDispose()
        {
            _timerNextDay.Elapsed -= CheckEventNextDay;
            _timerEvent.Close();
        }
        #endregion
    }
}
