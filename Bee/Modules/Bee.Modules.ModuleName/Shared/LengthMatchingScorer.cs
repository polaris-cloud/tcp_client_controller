using FuzzySharp.SimilarityRatio.Scorer.Composite;

namespace Bee.Modules.Script.Shared;

public class LengthMatchingScorer : WeightedRatioScorer
{
    public override int Score(string s1, string s2)
    {
        // 自定义评分规则，这里可以根据字符串长度差距来进行判断
        int lengthDifference = s1.Length - s2.Length;

        // 例如，如果字符串长度差距大于某个阈值，返回一个较低的分数
        if (lengthDifference > 0)
        {
            return 0;
        }

        // 否则使用默认的相似度评分算法
        return base.Score(s1,s2);
    }
    
}