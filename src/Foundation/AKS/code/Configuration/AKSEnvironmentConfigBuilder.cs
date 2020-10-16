using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Configuration.ConfigurationBuilders;
namespace Sitecore.Demo.Platform.Foundation.AKS.Configuration
{
  public class AKSEnvironmentConfigBuilder : EnvironmentConfigBuilder
  {   
    public override string GetValue(string key)
    {
      return base.GetValue(key) ?? base.GetValue(key.Replace(":", "__"));
    }

    public override ICollection<KeyValuePair<string, string>> GetAllValues(string prefix)
    {
      ICollection<KeyValuePair<string, string>> values = base.GetAllValues(prefix);

      return values.Union(values
          .Where(pair => pair.Key.Contains("__"))
          .Select(pair => new KeyValuePair<string, string>(pair.Key.Replace("__", ":"), pair.Value)))
          .ToList();
    }
  }
}
