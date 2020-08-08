using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaharokAdmin
{
    public static class ErrorHandling
    {
        public static void Handling(Exception ex)
        {
            IEnumerable<string> messages = GetErrorMessages(ex);

            Console.WriteLine(String.Join(Environment.NewLine + Environment.NewLine, messages));
        }
        private static IEnumerable<string> GetErrorMessages(Exception ex)
        {
            if (!(ex is AggregateException))
                return new string[] { ex.Message };

            List<string> messages = new List<string>();

            foreach (var e in ((AggregateException)ex).InnerExceptions)
            {
                if (e is AggregateException)
                    messages.AddRange(GetErrorMessages(e));
                else
                    messages.Add(e.Message);
            }

            return messages;
        }
    }
}
