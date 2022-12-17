using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ResidenceRegistration
{
    public static class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main()
        {
            var dateReceiverUrl = "https://rezerwacje.bialystok.uw.gov.pl/?terminy=1&branch=183&service=201";

            while (true)
            {
                var dateHttpResult = await httpClient.GetAsync(dateReceiverUrl);
                var dateResponse = await dateHttpResult.Content.ReadAsStringAsync();

                var dateStr = dateResponse.Substring(1, dateResponse.Length - 2);

                if (string.IsNullOrEmpty(dateStr)) continue;

                var dateArray = GetSplittedStringContent(dateStr);
                //var lastDate = "2023-01-09"; // dateArray[dateArray.Length - 1]
                var lastDate = dateArray[dateArray.Length - 1];

                if (!DateOnly.TryParse(lastDate, out DateOnly date)) continue;

                DisplayInfo(dateStr, DayParam.Dates);

                //var isDateValid = date >= DateTime.Now.AddDays(21);
                //if (!isDateValid) continue;

                var timeReceiverUrl = $"https://rezerwacje.bialystok.uw.gov.pl/?godziny=1&branch=183&service=201&data={lastDate}";

                var timeHttpResult = await httpClient.GetAsync(timeReceiverUrl);
                var timeResponse = await timeHttpResult.Content.ReadAsStringAsync();

                var timeStr = timeResponse.Substring(1, timeResponse.Length - 2);

                if (string.IsNullOrEmpty(timeStr)) continue;

                var timeArray = GetSplittedStringContent(timeStr);

                Random random = new();

                var randomTime = timeArray[random.Next(timeArray.Length)];

                if (!TimeOnly.TryParse(randomTime, out TimeOnly time)) continue;

                DisplayInfo(timeStr, DayParam.Times);

                await SubmitForm(randomTime, lastDate);
            }
        }

        private async static Task SubmitForm(string time, string date)
        {
            time = WebUtility.UrlEncode(time);

            var body = $"sform=123&kolejka=201&data_od={date}&godzina={time}&weryfikacja=6498&email=alexg9000000%40gmail.com&zgoda=1&captha=acasjv&capthaHash=-1537224963";
            var contentType = "application/x-www-form-urlencoded";

            var httpContent = new StringContent(body, Encoding.UTF8, contentType);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await httpClient.PostAsync("https://rezerwacje.bialystok.uw.gov.pl/?branch=183", httpContent);

            var tiResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"POSTED DATA: " + body + tiResponse);
            } 
            else
            {
                Console.WriteLine("SUBMIT FORM IS FAILED");
            }
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



//httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript;q=0.9,image/avif,image/webp,*/*;q=0.8");
//httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
//httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
//httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
//httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
//httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0");
//httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

//var json = JsonConvert.SerializeObject(new
//{
//    sform = "123",
//    kolejka = "185",
//    data_od = date,
//    godzina = time,
//    weryfikacja = "9987",
//    email = "miter30@mail.ru",
//    zgoda = "1",
//    captha = "kfbvmf", // mpyxbl
//    capthaHash = "-1142273983" // -1051315615
//});