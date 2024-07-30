using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace etwdump;

class Program
{
    static int Main(string[] args)
    {
        if (args is [])
        {
            Console.WriteLine($"No provider to listen to. Use '{System.AppDomain.CurrentDomain.FriendlyName} <guid>'. Example: '{System.AppDomain.CurrentDomain.FriendlyName} 5a02809f-fbb3-4874-a478-65d401f49b8d'");
            return 0;
        }

        Console.WriteLine($"Listening events from {string.Join(", ", args)}");

        using var etwSession = new TraceEventSession("MyEtwLog", null);
        foreach (var provider in args)
        {
            etwSession.EnableProvider(
                providerGuid: Guid.Parse(provider),
                providerLevel: TraceEventLevel.Always,
                options: new TraceEventProviderOptions()
                {
                    Arguments =
                    [
                        new KeyValuePair<string, string>("FilterAndPayloadSpecs", "[AS]*\r\n*")
                    ],
                });
        }
        etwSession.Source.Dynamic.All += data => Console.WriteLine($"H: {data.ProviderName} - {data.EventName}: {string.Join(" ", data.PayloadNames.Take(8).Select(name => $"{name}={data.PayloadStringByName(name)}"))}");

        Console.CancelKeyPress += (_, _) => etwSession.Dispose();

        etwSession.Source.Process();

        Console.WriteLine("Good bye!");

        return 0;
    }
}
