using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

public class Startup
{
    private static Dictionary<int, Task<string>> _tasks = new Dictionary<int, Task<string>>();
    private static int _taskPointer = 0;

    public static string Invoke(string input)
    {
        var parts = input.Split('|');
        var response = string.Empty;
        Task<string> task = null;
        switch (parts[0])
        {
            case "send":
                var pointer = _taskPointer++;
                _tasks.Add(pointer, Task.Factory.StartNew<string>(() => {return Request(parts[1]);}));
                response = pointer.ToString();
                break;
            case "response":
                if (!_tasks.TryGetValue(Convert.ToInt32(parts[1]), out task))
                {
                    response = "error";
                    break;
                }
                if (!task.IsCompleted)
                {
                    response = "wait";
                    break;
                }
                response = task.Result;
                break;
        }
        return response;
    }

    private static string Request(string url)
    {
        string html = string.Empty;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            html = reader.ReadToEnd();
        }
        return html;
    }
}