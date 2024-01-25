using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bee.Modules.Script.Tests
{
    public class FuzzySearchTest
    {
        [Fact]
        void Filter()
        {
            List<string> itemList = new List<string>
            {
                "apple",
                "banana",
                "fasdban",
                "cherry",
                "grape",
                "kiwi"
            };

            string keyword = "banan";

            List<string> filteredList = FilterAndSortByFuzzySharp( keyword,itemList).ToList();

            Trace.WriteLine("Filtered and sorted list:");
            foreach (string item in filteredList)
            {
                Trace.WriteLine(item);
            }
        }

        static IEnumerable<string> FilterAndSortByFuzzySharp(string keyword,IEnumerable<string> itemList)
        {
            
            var fuzzyMatches = FuzzySharp.Process.ExtractAll(
                keyword,
                itemList,
                cutoff: 50);  // Adjust the cutoff score as needed);

             var filteredList= fuzzyMatches
                .OrderByDescending(match => match.Score)
                .Select(match => match.Value)
                .ToList();

            return filteredList;
        }
    }
}
