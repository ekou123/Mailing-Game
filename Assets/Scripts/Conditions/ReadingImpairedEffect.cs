using System.Text;
using UnityEngine;

public class ReadingImpairedEffect : ConditionEffect
{
    public float scrambleInterval = 0.8f;
    public float wobbleStrength = 2f;

    private static readonly System.Collections.Generic.Dictionary<char, char> mirrorMap = new()
    {
        { 'b', 'd' }, { 'd', 'b' },
        { 'p', 'q' }, { 'q', 'p' },
        { 'n', 'u' }, { 'u', 'n' }
    };

    public override string ProcessLabel(string originalText)
    {
        char[] chars = originalText.ToCharArray();

        for (int i = 0; i < chars.Length - 1; i++)
        {
            if (char.IsLetter(chars[i]) && Random.value < 0.15f)
                (chars[i], chars[i + 1]) = (chars[i + 1], chars[i]);
        }

        for (int i = 0; i < chars.Length; i++)
        {
            if (mirrorMap.ContainsKey(chars[i]) && Random.value < 0.2f)
                chars[i] = mirrorMap[chars[i]];
        }

        var result = new StringBuilder();
        foreach (char c in chars)
        {
            if (char.IsLetter(c))
            {
                float offset = Random.Range(-wobbleStrength, wobbleStrength);
                result.Append($"<voffset={offset:F1}em>{c}</voffset>");
            }
            else result.Append(c);
        }

        return result.ToString();
    }
}