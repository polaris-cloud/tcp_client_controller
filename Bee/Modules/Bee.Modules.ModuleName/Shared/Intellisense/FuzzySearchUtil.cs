using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using FuzzySharp.Extractor;
using FuzzySharp.PreProcess;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer;
using FuzzySharp.SimilarityRatio.Scorer.Composite;
using Polaris.Protocol.Model;

namespace Bee.Modules.Script.Shared.Intellisense;






public class FuzzySearchUtil
{
    private static IRatioScorer scorer = new LengthMatchingScorer();
    public static IEnumerable<ProtocolFormat> FilterAndSortByFuzzySharp(string keyword, IEnumerable<ProtocolFormat> itemList)
    {

        var fuzzyMatches = Process.ExtractAll(
            new ProtocolFormat(){BehaviorKeyword = keyword},
            itemList,
            s=>s.BehaviorKeyword,
            scorer: scorer,
            cutoff: 50);  // Adjust the cutoff score as needed);

        var filteredList = fuzzyMatches
            .OrderByDescending(match => match.Score)
            .Select(match => match.Value);

        return filteredList;
    }

    
}