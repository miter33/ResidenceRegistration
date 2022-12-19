using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ResidenceRegistration
{
    internal class UserData
    {
        public string Email { get; set; }
        public int VerificationNumber { get; set; }
    }

    internal static class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string LAST_DATE = "2023-01-10";
        private static string[] KEY_WORDS =
        {
            "Identyfikator wizyty",
            "Wizyta została zarezerwowana na dzień",
            "Potwierdzenie rezerwacji",
            "Podsumowanie rezerwacji",
            "On the day of the visit, the ticket must be picked up in a ticket vending machine 15 minutes before the booked visit by selecting the option",
            "Usługa",
            "Lokalizacja"
        };

        static async Task Main()
        {
            //var dateReceiverUrl = "https://rezerwacje.bialystok.uw.gov.pl/?terminy=1&branch=183&service=201";

            var users = new List<UserData>
            {
                new UserData { Email = "alexg9000000@gmail.com", VerificationNumber = 6498 },
                new UserData { Email = "alexg9000000@gmail.com", VerificationNumber = 6498 },
                new UserData { Email = "aliaksei.huivan@gmail.com", VerificationNumber = 6498 },
                new UserData { Email = "aliaksei.huivan@gmail.com", VerificationNumber = 6498 },
                //new UserData { Email = "miter30@mail.ru", VerificationNumber = 6498 },
            };

            while (true)
            {
                var tasks = users.Select(user => GenerateFormData(user));

                await Task.WhenAll(tasks);
            }
        }

        private async static Task GenerateFormData(UserData user)
        {
            var timeUrlReceiver = $"https://rezerwacje.bialystok.uw.gov.pl/?godziny=1&branch=183&service=201&data={LAST_DATE}";

            var timeHttpResult = await httpClient.GetAsync(timeUrlReceiver);
            var timeResponse = await timeHttpResult.Content.ReadAsStringAsync();

            var timeStr = timeResponse.Substring(1, timeResponse.Length - 2);

            if (string.IsNullOrEmpty(timeStr)) return;

            var timeArray = GetSplittedStringContent(timeStr);

            Random random = new();

            var randomTime = timeArray[random.Next(timeArray.Length)];

            if (!TimeOnly.TryParse(randomTime, out TimeOnly time)) return;

            DisplayInfo(timeStr, DayParam.Times);

            await SubmitForm(randomTime, LAST_DATE, user.Email, user.VerificationNumber);
        }

        private async static Task SubmitForm(string time, string date, string email, int verification)
        {
            var encryptedTime = WebUtility.UrlEncode(time);
            var encryptedEmail = WebUtility.UrlEncode(email);

            var body = $"sform=123&kolejka=201&data_od={date}&godzina={encryptedTime}&weryfikacja={verification}&email={encryptedEmail}&zgoda=1&captha=acasjv&capthaHash=-1537224963";
            var contentType = "application/x-www-form-urlencoded";

            var httpContent = new StringContent(body, Encoding.UTF8, contentType);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var httpClientResponse = await httpClient.PostAsync("https://rezerwacje.bialystok.uw.gov.pl/?branch=183", httpContent);

            var result = await httpClientResponse.Content.ReadAsStringAsync();

            if (httpClientResponse.IsSuccessStatusCode && (KEY_WORDS.Any(key => result.Contains(key, StringComparison.OrdinalIgnoreCase))))
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"POSTED DATA (date: {date}, time: {time}, email: {email}, verification: {verification}):\n{body}");

                Console.ResetColor();

                await WriteToFile(email, date, time, result, "SUCCESS");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"SUBMIT FORM IS FAILED (date: {date}, time: {time}, email: {email}, verification: {verification})");

                Console.ResetColor();

                await WriteToFile(email, date, time, result, "FAILED");
            }
        }

        private static async Task WriteToFile(string email, string applyingDate, string applyingTime, string result, string status)
        {
            var ticks = DateTime.Now.Ticks;
            var time = applyingTime.Replace(':', '-');

            if(!Directory.Exists("REGISTRATIONS"))
            {
                Directory.CreateDirectory("REGISTRATIONS");
            }

            var fileName = $"REGISTRATIONS/{status}_{ticks}_{email}_{applyingDate}_{time}.html";

            await File.WriteAllTextAsync(fileName, result);
        }

        private static void DisplayInfo(string info, DayParam dayParam)
        {
            switch (dayParam)
            {
                case DayParam.Dates:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case DayParam.Times:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.WriteLine($"List of {dayParam}: " + info);
            Console.ResetColor();
        }

        private static string[] GetSplittedStringContent(string content)
        {
            return content.Replace("\"", string.Empty).Split(',');
        }
    }

    enum DayParam
    {
        Dates,
        Times
    }
}