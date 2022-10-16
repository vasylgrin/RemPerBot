using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using static RemBerBot_BL.Controller.Controller.ObjectControllerBase;

namespace RemBerBot_BL.Controller.Controller
{
    public static class DictionaryController
    {
        public static Dictionary<long, OperationEnum> OperationDictionary { get; set; } = new();
        public static Dictionary<long, NavigationEnum> NavigationDictionary { get; set; } = new();

        /// <summary>
        /// Sets the value in the dictionary.
        /// </summary>
        /// <typeparam name="G">The data type that is the value for the dictionary.</typeparam>
        /// <param name="chatId">Chat id.</param>
        /// <param name="value">The value that we set in the dictionary.</param>
        /// <param name="dictionary">Dictionary in which data is stored.</param>
        public static void SetDictionary<G>(this Dictionary<long, G> dictionary, long chatId, G value)
        {
            if (dictionary.ContainsKey(chatId))
            {
                dictionary[chatId] = value;
            }
            else
            {
                dictionary.Add(chatId, value);
            }
        }

        public static void SetStartValueOfDictionaryForNewUser<G>(this Dictionary<long, G> dictionary, long chatId)
        {
            if (!dictionary.ContainsKey(chatId))
            {
                dictionary[chatId] = default!;
            }
        }
    }
}
