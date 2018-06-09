using System;
using Microsoft.Extensions.Logging;

namespace CacheFlow.Logging
{
    public class LoggingExtensions
    {
        public static string Formatter<TState>(TState state, Exception exception)
        {
            if (exception != null)
                return exception.Message;

            if (state != null)
                return state.ToString();

            return "Не удалось отформатировать сообщение.";
        }


        public static EventId GetEventId<TEnum>(TEnum serviceEvent) where TEnum : Enum
        {
            var name = Enum.GetName(typeof(TEnum), serviceEvent);
            var value = Convert.ToInt32(serviceEvent);

            return new EventId(value, name);
        }

    }
}
