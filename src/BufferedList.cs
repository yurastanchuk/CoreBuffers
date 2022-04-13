using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreBuffers {
    
public class 
BufferedListStorage<T>{
    public List<BufferedList<T>> Items {get;} 
    public int Position {get;private set;}
    public BufferedListStorage(SizedListStorage<T> sizedListStorage, SizedArrayStorage<T> sizedArrayStorage, int initialSize = 0) {
        Items = new List<BufferedList<T>>(capacity: initialSize);
        ListStorage = sizedListStorage;
        ArrayStorage = sizedArrayStorage;
        for (int i = 0; i < initialSize; i++) 
            Items.Add(new BufferedList<T>(ListStorage, ArrayStorage));
    }
    /// <summary>
    /// Is used to get List from BufferedList
    /// </summary>
    public SizedListStorage<T> ListStorage {get;} 
    public SizedArrayStorage<T> ArrayStorage {get;}

    public BufferedList<T> 
    GetList()  {
        Position++;
        if (Items.Count > Position){
            var item = Items[Position];
            item.Reclaim();
            return item;
        }
        var result = new BufferedList<T>(ListStorage, ArrayStorage);
        Items.Add(result);
        return result;
    }
    
    public void Reclaim() {
        Position = 0;
        ListStorage.Reclaim();
        ArrayStorage.Reclaim();
    }
}

public static class 
BufferedList{
    public static BufferedList<T>
    New<T>() => new BufferedList<T>(new SizedListStorage<T>(), new SizedArrayStorage<T>());
}

public class 
BufferedList<T> : IEnumerable<T>, IList<T>{
    public T[] Objects {get; private set;}
    public bool Remove(T item) {
        throw new NotImplementedException();
    }

    public int Count {get; private set;}
    public bool IsReadOnly { get; }

    /// <summary>
    /// Is used to get List from BufferedList
    /// </summary>
    private SizedListStorage<T> ListStorage {get;}
    private SizedArrayStorage<T> ArrayStorage {get;} 
    public BufferedList(SizedListStorage<T> listStorage, SizedArrayStorage<T> arrayStorage, int initialSize = 10) {
        Objects = new T[initialSize];
        Count = 0;
        ListStorage =  listStorage;
        ArrayStorage = arrayStorage;
    }

    public BufferedList(int initialSize = 10) {
        Objects = new T[initialSize];
        Count = 0;
        ListStorage =  new SizedListStorage<T>();
        ArrayStorage = new SizedArrayStorage<T>();
    }
    
    public BufferedList(BufferedList<T> other) {
        Objects = new T[other.Count];
        Count = 0;
        AddBufferedList(other);
        ListStorage = other.ListStorage;
        ArrayStorage = other.ArrayStorage;
    }
    
    public int Capacity => Objects.Length;

    public int IndexOf(T item) {
        var equalityComparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) {
            if (equalityComparer.Equals(Objects[i], item))
                return i;
        }
        return -1;
    }

    public void Insert(int index, T item) {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index) {
        throw new NotImplementedException();
    }

    public T this[int index]
    {
        get => Objects[index];
        set{
            while (index >= Objects.Length) 
                ExtendBuffer();
            Objects[index] = value;
        }
    }

    public void 
    Add(T item) {
        if (Count == Objects.Length)
            ExtendBuffer();
            
        Objects[Count] = item;
        Count ++;
    }

    public void Clear() {
        Objects.Clear();
    }

    public bool Contains(T item) {
        var equalityComparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) {
            if (equalityComparer.Equals(Objects[i], item))
                return true;
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex) {
        foreach (var element in this) 
            array[arrayIndex++] = element;
    }

    public void
    AddRange(BufferedList<T> other, int index, int count) {
        for (var i = index; i < index + count; i++) 
            Add(other[i]);
    }

    public void
    AddBufferedList(BufferedList<T> other) {
        AddRange(other, 0, other.Count);
    }
    
    private void ExtendBuffer() {
        var result = new T[Objects.Length + 4]; 
        for (var i = 0; i < Objects.Length; i++) 
            result[i] = Objects[i];
        Objects = result;
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
    
    public struct Enumerator : IEnumerator<T>, IEnumerator{
        private readonly T[] _items;
        private int _index;
        private T? _current;
        private readonly int _size;
        internal Enumerator(BufferedList<T> buffer) {
            _items = buffer.Objects;
            _index = 0;
            _current = default;
            _size = buffer.Count;
        }

        public void Dispose(){ }

        public bool MoveNext() {
            if (_index < _size) {
                _current = _items[_index];
                _index++;
                return true;
            }
            return false;
        }
        
        public T Current => _current!;

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset() {
            _index = 0;
            _current = default;
        }
    }

    /// <summary>
    /// Use this if you need to fill buffer with new objects
    /// </summary>
    /// <returns></returns>
    public BufferedList<T> 
    Reclaim() {
        Count = 0;
        //_enumerator.Reset();
        return this;
    }
    /// <summary>
    /// Use this if you need to reuse added entities
    /// </summary>
    /// <returns></returns>
    public BufferedList<T> 
    SetCount(int count) {
        Count = count;
        //_enumerator.Reset();
        return this;
    }
    public List<T>
    ToList() {
        var result = ListStorage.GetList(Count);
        for (var i = 0; i < Count; i++) {
            if (result.Count <= i)
                result.Add(this[i]);
            else
                result[i] = this[i];
        }
        return result;
    }

    public T[]
    GetEmptyArray(int size) {
        var result = ArrayStorage.GetArray(Count);
        return result;
    }

    public T[]
    ToArray() {
        var result = ArrayStorage.GetArray(Count);
        for (var i = 0; i < Count; i++) 
            result[i] = this[i];

        return result;
    }
}
}
