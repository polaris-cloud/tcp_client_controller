using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using FuzzySharp.PreProcess;
using FuzzySharp.SimilarityRatio.Scorer;

namespace Bee.Modules.Script.Shared;

public class FuzzySearchUtil
{
    private static IRatioScorer scorer=new LengthMatchingScorer();
    public static IEnumerable<string> FilterAndSortByFuzzySharp(string keyword, IEnumerable<string> itemList)
    {

        var fuzzyMatches = FuzzySharp.Process.ExtractAll(
            keyword,
            itemList,
            scorer:scorer,
            cutoff: 50);  // Adjust the cutoff score as needed);

        var filteredList = fuzzyMatches
            .OrderByDescending(match => match.Score)
            .Select(match => match.Value);

        return filteredList;
    }
}