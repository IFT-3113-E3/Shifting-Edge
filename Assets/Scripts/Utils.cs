using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    public static IEnumerable<T> OrderRandomly<T>(this IEnumerable<T> sequence)
    {
        Random random = new Random();
        List<T> copy = sequence.ToList();

        while (copy.Count > 0)
        {
            int index = random.Next(copy.Count);
            yield return copy[index];
            copy.RemoveAt(index);
        }
    }
}
