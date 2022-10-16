using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Pretpark;

internal class Program
{
    static string? userAgent;
    
    private static void Main(string[] args)
    {
        var teller = 0;
        
        var server = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), 5000);
        server.Start();
        while (true)
        {
            using var connectie = server.AcceptSocket();
            using Stream request = new NetworkStream(connectie);
            using var requestLezer = new StreamReader(request);

            var regel1 = requestLezer.ReadLine()?.Split(" ");
            if (regel1 == null) return;

            (var methode, var url, var httpversie) = (regel1[0], regel1[1], regel1[2]);
            var regel = requestLezer.ReadLine();
            var contentLength = 0;
            while (!string.IsNullOrEmpty(regel) && !requestLezer.EndOfStream)
            {
                var stukjes = regel.Split(":");
                var (header, waarde) = (stukjes[0], stukjes[1]);
                if (header.ToLower() == "content-length")
                    contentLength = int.Parse(waarde);
                regel = requestLezer.ReadLine(); 
                
                if (header.ToLower() == "content-length")
                    contentLength = int.Parse(waarde);
                  
                regel = requestLezer.ReadLine();
                if (header == "User-Agent")
                {
                    userAgent = waarde;
                }
            }

            if (contentLength > 0)
            {
                var bytes = new char[contentLength];
                requestLezer.Read(bytes, 0, contentLength);
            }

            if (url == "" || url == "/")
            {
                Console.WriteLine(httpversie);
                var content = File.ReadAllText("home.html") + userAgent;
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
            else if (url == "/contact")
            {
                var content = File.ReadAllText("contact.html");
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
            else if (url == "/teller")
            {
                teller++;
                var content = "Teller: " + teller;
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
            else if (url.Contains("/add"))
            {
                var myUri = new Uri("http://localhost:5000" + url);
                var a = int.Parse(HttpUtility.ParseQueryString(myUri.Query).Get("a"));
                var b = int.Parse(HttpUtility.ParseQueryString(myUri.Query).Get("b"));
                var som = a + b;
                var content = "Som: " + som;
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
            else if (url.Contains("/mijnteller"))
            {
                var myUri = new Uri("http://localhost:5000" + url);
                var mijnTeller = 0;
                if (url.Contains("?t="))
                {
                    mijnTeller = int.Parse(HttpUtility.ParseQueryString(myUri.Query).Get("t"));
                }

                var content = $"De teller staat op {mijnTeller}, klik <a href='localhost:5000/mijnteller?t={mijnTeller+1}'>hier</a> om te verhogen";
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 200 OK\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
            else
            {
                var content = "<h1>404 Not Found</h1><p>Pagina niet gevonden of bestaat niet. Helaas ;)</p>";
                connectie.Send(Encoding.ASCII.GetBytes(
                    $"HTTP/1.0 404 Not Found\r\nContent-Type: text/html\r\nContent-Length: {content.Length}\r\n\r\n{content}"));
            }
        }
    }
}