using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreBuffers {
    
public static class 
BuffersHelper{

    public static ImmutableBuffer<TOut> 
    MapToImmutableBuffer<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> convert) => 
        ImmutableBuffer.Create<TOut>(enumerable.Select(convert).ToArray());

    public static BufferedList<T> 
    ToBufferedList<T>(this List<T> iEnumerable, BufferedList<T> bufferedList) where T : new() {
        foreach (var item in iEnumerable) {
            bufferedList.Add(item);
        }
        return bufferedList;
    }
    
    public static BufferedList<T> 
    ToBufferedList<T>(this T[] iEnumerable, BufferedList<T> bufferedList) where T : new() {
        foreach (var item in iEnumerable) {
            bufferedList.Add(item);
        }
        return bufferedList;
    }
     

    public static ImmutableBuffer<T> 
    ToImmutableBuffer<T>(this IList<T> list) where T : new() =>
        ImmutableBuffer.Create(list);

    public static ImmutableBuffer<T> 
    ToImmutableBuffer<T>(this IList<T> list, ImmutableBufferStorage<T> buffer) where T : new() =>
        buffer.CreateBuffer(list);

    public static ImmutableBuffer<T> 
    ToImmutableBuffer<T>(this IList<T> list, EntityBuffer<T> buffer) where T : new() =>
        buffer.CreateImmutableBuffer(list);
    
    public static ImmutableBuffer<T> 
    ToImmutableBuffer<T>(this T[] array, ImmutableBufferStorage<T> bufferStorage) where T : new() =>
        bufferStorage.CreateBuffer(array);

    public static ImmutableBuffer<T> 
    ToImmutableBuffer<T>(this IEnumerable<T> array, ImmutableBufferStorage<T>? bufferStorage = null) =>
        bufferStorage != null 
            ? bufferStorage.CreateBuffer(array.ToList())
            : ImmutableBufferStorage<T>.Empty.CreateBuffer(array.ToList());
    
    public static ImmutableBuffer<T> 
    SingleToImmutableBuffer<T>(this T item, ImmutableBufferStorage<T>? bufferStorage = null) =>
        bufferStorage != null 
            ? bufferStorage.CreateBuffer(item)
            : ImmutableBufferStorage<T>.Empty.CreateBuffer(item);

    public static ImmutableBuffer<T> ToImmutableBuffer<T>(this BufferedList<T> bufferedList, ImmutableBufferStorage<T> bufferStorage) where T : new() =>
        bufferStorage.CreateBuffer(bufferedList.Objects, bufferedList.Count);

    public static T[]
    Clear<T>(this T[] array) {
        Array.Clear(array, 0, array.Length);
        return array;
    }
    
    public static void
    AddOrAddToBufferedList<TKey, TValue>(this Dictionary<TKey, BufferedList<TValue>> dictionary, TKey key, TValue value, BufferedListStorage<TValue>? listBuffer = null) {
        if (dictionary.TryGetValue(key, out var list))
            list.Add(value);
        else {
            var newList = listBuffer?.GetList() ?? new BufferedList<TValue>();
            newList.Add(value);
            dictionary.Add(key, newList);
        }
    }

    public static void 
    AddOrAddToBuffer<TKey, TValue>(this Dictionary<TKey, ImmutableBuffer<TValue>> dictionary, TKey key, TValue value, ImmutableBufferStorage<TValue> bufferStorage) {
        if (dictionary.TryGetValue(key, out var list))
            dictionary[key] = list.Add(value);
        else 
            dictionary.Add(key, bufferStorage.CreateBuffer(value));
    }

    public static void 
    ForEach<T>(this BufferedList<T> items, Action<T> action) {
        foreach(var item in items)
            action(item);
    }

    public static BufferedList<T>
    ToBufferedList<T>(this T item, BufferCollections<T> buffer) {
        var result = buffer.BufferedListStorage.GetList();
        result.Add(item);
        return result;
    }

    public static BufferedList<T>
    ToBufferedList<T>(this T item, EntityBuffer<T> buffer) where T : new() {
        var result = buffer.BufferedListStorage.GetList();
        result.Add(item);
        return result;
    }

    public static BufferedList<T>
    ToBufferedList<T>(this T item) {
        var result = new BufferedList<T> {
            item
        };
        return result;
    }

    public static T
    Last<T>(this BufferedList<T> list) => list[^1];

    public static BufferedList<TOut> 
    MapToBufferedList<TIn, TOut>(this IList<TIn> list, Func<TIn, TOut> convert, BufferedList<TOut>? result = null) {
        result ??= new BufferedList<TOut>(list.Count);
        for (int i = 0; i < list.Count; i++) result.Add(convert(list[i]));
        return result;
    }

    public static BufferedList<TOut> 
    MapToBufferedList<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> convert, BufferedList<TOut>? result = null) {
        result ??= new BufferedList<TOut>();
        foreach (var item in enumerable) 
            result.Add(convert(item));
        return result;
    }
}
}
