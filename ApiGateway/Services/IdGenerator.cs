

namespace ApiGateway.Services
{
    public class IdGenerator
    {
        public static string CreateLetterName(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

     
    }
}
