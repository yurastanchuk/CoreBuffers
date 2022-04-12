using System;
using System.Collections.Generic;

namespace CoreBuffers {
    
public static class 
BuffersHelper{
    
    public static BufferedList<T>
    ToBufferedList<T>(this IEnumerable<T> iEnumerable, BufferedList<T> bufferedList) where T : new() {
        foreach (var item in iEnumerable) {
            bufferedList.Add(item);
        }
        return bufferedList;
    }
     
    public static ImmutableBuffer<T>
    ToImmutableBuffer<T>(this IList<T> list, ImmutableBufferStorage<T>? buffer = null) where T : new() =>
        buffer != null ? buffer.CreateBuffer(list) : ImmutableBuffer.Create(list);

    public static ImmutableBuffer<T>
    ToImmutableBuffer<T>(this T[] array, ImmutableBufferStorage<T> bufferStorage) where T : new() =>
        bufferStorage.CreateBuffer(array);

    public static ImmutableBuffer<T>
    ToImmutableBuffer<T>(this BufferedList<T> bufferedList, ImmutableBufferStorage<T> bufferStorage) where T : new() =>
        bufferStorage.CreateBuffer(bufferedList.Objects, bufferedList.Count);

    public static T[]
    Clear<T>(this T[] array) {
        Array.Clear(array, 0, array.Length);
        return array;
    }
}
}
