using RemBerBot_BL.Models;

namespace MySuperUniversalBot_BL.Models
{
    public class Period : ModelBase
    {
        #region Property

        /// <summary>
        /// The reminder ID for the database.
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// Chat ID of the user who saves/receives period.
        /// </summary>
        public override long ChatId { get; set; }

        /// <summary>
        /// Duration of menstruation.
        /// </summary>
        public int DurationMenstruation { get; set; }

        public int DurationMenstruationVariable { get; set; } = -1;

        /// <summary>
        /// Cycle duration.
        /// </summary>
        public int DurationСycle { get; set; }

        /// <summary>
        /// Date of last menstruation.
        /// </summary>
        public DateTime DateOfLastMenstruation { get; set; }

        /// <summary>
        /// The date of the next menstruation.
        /// </summary>
        public DateTime DateOfNextMenstruation { get; set; }

        /// <summary>
        /// List of users to notify about the event.
        /// </summary>
        public List<NotifyTheUser> NotifyTheUser { get; set; } = new();

        /// <summary>
        /// The property that shows whether the user was notified, if false, then it was not. By default, false.
        /// </summary>
        public bool isNotify { get; set; } = false;

        #endregion

        /// <summary>
        /// Creates a new period of ovulation.
        /// </summary>
        /// <param name="chatId">Chat ID of the user who saves/receives period.</param>
        /// <param name="durationMenstruation">Duration of menstruation.</param>
        /// <param name="durationСycle">Cycle duration.</param>
        /// <param name="dateOfLastMenstruation">Date of last menstruation.</param>
        /// <param name="dateOfNextMenstruation">The date of the next menstruation.</param>
        /// <exception cref="ArgumentException"></exception>
        public Period(long chatId, int durationMenstruation, int durationСycle, DateTime dateOfLastMenstruation, DateTime dateOfNextMenstruation)
        {
            #region check for null

            if (chatId <= 0)
            {
                throw new ArgumentException($"\"{nameof(chatId)}\" cannot be empty or null", nameof(chatId));
            }
            else if (durationMenstruation <= 0)
            {
                throw new ArgumentException($"\"{nameof(durationMenstruation)}\" cannot be empty or null", nameof(durationMenstruation));
            }
            else if (durationСycle <= 0)
            {
                throw new ArgumentException($"\"{nameof(durationСycle)}\" cannot be empty or null", nameof(durationСycle));
            }
            else if (dateOfLastMenstruation == DateTime.MinValue)
            {
                throw new ArgumentException($"\"{nameof(dateOfLastMenstruation)}\" cannot be empty or null", nameof(dateOfLastMenstruation));
            }
            if (dateOfNextMenstruation == DateTime.MinValue)
            {
                throw new ArgumentException($"\"{nameof(dateOfNextMenstruation)}\" cannot be empty or null", nameof(dateOfNextMenstruation));
            }

            #endregion

            ChatId = chatId;
            DurationСycle = durationСycle;
            DurationMenstruation = durationMenstruation;
            DateOfLastMenstruation = dateOfLastMenstruation;
            DateOfNextMenstruation = dateOfNextMenstruation;
        }

        public Period()
        {
        }

        public override string PrintData()
        {
            return $"Тривалість менструації: {DurationMenstruation}\nТривалість циклу: {DurationСycle}\nДата останьої менструації: {DateOfLastMenstruation.ToShortDateString()}\nДата наступної менструації: {DateOfNextMenstruation.ToShortDateString()}";
        }
    }
}
