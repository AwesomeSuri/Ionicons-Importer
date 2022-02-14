using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RGP.Ionicons.Editor
{
    public static class SvgSearcher
    {
        public static string[] GetFileNames(string root, string searchFilter, string graphicType)
        {
            string[] names = Directory.GetFiles(root);

            // read zip file
            var filteredFiles = new List<string>();
            foreach (var n in names)
            {
                var name = Path.GetFileName(n);
                var split = name.Split('.');
                name = split[0];
                
                var match = true;
                string lowerCase;

                // no meta files
                if (split[split.Length - 1].Equals("meta", StringComparison.InvariantCultureIgnoreCase)) continue;

                split = split[0].Split('-');

                // check for graphic type
                for (var j = 0; j < split.Length; j++)
                {
                    if (j == split.Length - 1)
                    {
                        if (graphicType.Equals("Filled", StringComparison.InvariantCultureIgnoreCase) &&
                            (split[j].Equals("Outline", StringComparison.InvariantCultureIgnoreCase) ||
                             split[j].Equals("Sharp", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            match = false;
                            break;
                        }

                        if (!graphicType.Equals("Filled", StringComparison.InvariantCultureIgnoreCase) &&
                            !split[j].Equals(graphicType, StringComparison.InvariantCultureIgnoreCase))
                        {
                            match = false;
                            break;
                        }
                    }
                }

                if (!match) continue;

                // check for search filters
                if (searchFilter.Length == 0)
                {
                    filteredFiles.Add(name);
                    continue;
                }

                match = false;
                string[] filters = searchFilter.Split(' ');
                foreach (string filter in filters)
                {
                    lowerCase = filter.ToLower();
                    if (split.Any(s => s.ToLower().Contains(lowerCase)))
                    {
                        match = true;
                    }
                }

                if (match)
                {
                    filteredFiles.Add(name);
                }
            }

            return filteredFiles.ToArray();
        }
    }
}
