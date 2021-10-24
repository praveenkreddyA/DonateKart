using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace API_call_Fetching
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string res = "true";
            string totalAmountDesc = "";
            SortedDictionary<double, List<string>> amountdic = new SortedDictionary<double, List<string>>();
            DateTime cdt = DateTime.Today;
            DateTime last30daysdt = cdt.AddDays(-30);
            List<string> ActiveCompaignlst = new List<string>();
            List<string> ClosedCompaignlst = new List<string>();
            List<string> Last30DaysCompaignlst = new List<string>();
            string compaignName = "";
            using (HttpClient client = new HttpClient())
            {
                string url = "https://testapi.donatekart.com/api/campaign"; //string.Format("http://localhost:58749/login/validateLogin/{0}/{1}", new string[] { userName, password });
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();
                    string[] splitarr = res.Split("},{");
                    foreach (string data in splitarr)
                    {
                        string[] dataArr = data.Split(",");
                        double amount = 0;
                        compaignName = "";
                        StringBuilder sb = new StringBuilder();
                        bool isAdded = false;
                        foreach (string item in dataArr)
                        {
                            string[] temparr = item.Split(":");
                            if (temparr[0].Contains("totalAmount"))
                            {
                                sb.Append("," + item);
                                amount = Convert.ToDouble(temparr[1]);
                            }
                            if (temparr[0].Contains("procuredAmount"))
                            {
                                // sb.Append("," + item);
                                double procAmnt = Convert.ToDouble(temparr[1]);
                                if (procAmnt >= amount && isAdded == false)
                                    ClosedCompaignlst.Add(compaignName);
                            }
                            if (temparr[0].Contains("title"))
                            {
                                compaignName = temparr[1];
                                sb.Append(item);
                            }
                            if (temparr[0].Contains("endDate"))
                            {
                                string enddate = temparr[1].Replace("\"", "");
                                var endDateTime = DateTime.ParseExact(enddate, "yyyy-MM-ddTHH", CultureInfo.InvariantCulture);
                                DateTime dt = Convert.ToDateTime(endDateTime);
                                if (dt >= cdt)
                                    ActiveCompaignlst.Add(compaignName);
                                if (dt < cdt && dt >= last30daysdt)
                                    Last30DaysCompaignlst.Add(compaignName);
                                sb.Append("," + item);
                                if (endDateTime < cdt)
                                {
                                    isAdded = true;
                                    ClosedCompaignlst.Add(compaignName);
                                }
                            }
                            if (temparr[0].Contains("backersCount"))
                                sb.Append("," + item);
                        }
                        if (!amountdic.ContainsKey(amount))
                            amountdic.Add(amount, new List<string>());
                        amountdic[amount].Add("[" + sb.ToString() + "]");
                    }

                }
                else
                    res = response.StatusCode.ToString();
            }
            foreach (KeyValuePair<double, List<string>> item in amountdic.OrderByDescending(key => key.Key))
            {
                if (totalAmountDesc.Length > 0)
                    totalAmountDesc += ",";
                totalAmountDesc += String.Join(",", item.Value);
            }

            Console.WriteLine("First Solution Total Amount Desc Order \n" + totalAmountDesc);
            Console.WriteLine("--------------------\n\n-------------");
            Console.WriteLine("Second Solution Fetching Active Campaign List:\n" + String.Join(",", compaignName));
            Console.WriteLine("Second Solution Fetching Last 30 days Campaign List:\n" + String.Join(",", Last30DaysCompaignlst));
            Console.WriteLine("--------------------\n\n-------------");
            Console.WriteLine("Third Solution Fetch Closed Compaign List:\n" + String.Join(",", ClosedCompaignlst));
            Console.ReadLine();
        }
    }
}
