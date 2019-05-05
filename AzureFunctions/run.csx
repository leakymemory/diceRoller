#r "Newtonsoft.Json"
#r "System.Data"
#r "System.Web"

using System.Data;
using System.Net;
using System.Text;
using System.Web;

using System.Collections.Specialized;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    string reqContent = await req.Content.ReadAsStringAsync();
    NameValueCollection coll = HttpUtility.ParseQueryString(reqContent);

    var diceRoller = new DiceRoller();
    List<string> diceRolls = diceRoller.CalculateRoll(coll["text"]);

    var myObj = new {
        response_type = "in_channel",
        text = "<@" + coll["user_id"] + $"> roll results:",
        attachments = new object[] { 
            new {
                text = string.Join("\n", diceRolls),
                color = "#36a64f",
                mrkdwn = true
            }
        }
    };

    var jsonToReturn = JsonConvert.SerializeObject(myObj);

    return new HttpResponseMessage(HttpStatusCode.OK) {
        Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
    };
}

// Insert DiceRoller classes here