using System;
using Newtonsoft.Json;

namespace GolfGroup.Api.Helpers
{
  public static class LogHelper
  {
    public static string Dump(this object toDump)
    {
      return Dump(toDump, true);
    }

    public static string Dump(this object toDump, bool indented)
    {
      return JsonConvert.SerializeObject(toDump, indented ? Formatting.Indented : Formatting.None);
    }

    public static T JsonCast<T>(this object toDump)
    {
      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(toDump));
    }

    public static T JsonClone<T>(this T first)
    {
      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(first));
    }

    public static T Dump<T>(this T toDump, string message)
    {
      Console.Out.WriteLine(message + ": " + toDump.Dump());
      return toDump;
    }
  }
}
