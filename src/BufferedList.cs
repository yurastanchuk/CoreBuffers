using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreBuffers {
    
public class 
BufferedListStorage<T>{
    public List<BufferedList<T>>[] Chunks {get; private set;} 
    public int Position {get;private set;}
    public BufferedListStorage(BufferCollections<T> collections) {
        Chunks = new List<BufferedList<T>>[10];
        _collections = collections;
        Position = -1;
    }

    private BufferCollections<T> _collections;
    
    private const int _chunkSize = 10000;
    private List<BufferedList<T>> _currentChunk => Chunks[_chunkId];
    private int _chunkId => Position / _chunkSize;
    private int _chunkPosition => Position - _chunkId * _chunkSize;
    private int _chunksCount;

    public BufferedList<T> 
    GetList()  {
        MoveCursor();

        if (_chunkPosition < _currentChunk.Count){
            var item = _currentChunk[_chunkPosition];
            item.Reclaim();
            return item;
        }
        var result = new BufferedList<T>(_collections);
        _currentChunk.Add(result);
        return result;
    }
    
    public void 
    Reclaim() {
        Position = -1;
    }

    private void
    MoveCursor() {
        Position++;
        if (_chunkId >= Chunks.Length)
            ExtendChunks();
        if (_chunkId >= _chunksCount) {
            Chunks[_chunkId] = new List<BufferedList<T>>(capacity: _chunkSize);
            _chunksCount++;
        }
    }

    private void
    ExtendChunks() {
        var newChunks = new List<BufferedList<T>>[Chunks.Length + 100];
        for (var i = 0; i < Chunks.Length; i++)
            newChunks[i] = Chunks[i];
        Chunks = newChunks;
    }
}

public static class 
BufferedList{
    public static BufferedList<T>
    New<T>() => new BufferedList<T>(new BufferCollections<T>());
}

public class 
BufferedList<T> : IEnumerable<T>, IList<T>{
    public T[] Objects {get; private set;}
    
    public int Count {get; private set;}
    public bool IsReadOnly { get; }

    public BufferedList(BufferCollections<T> collections, int initialSize = 10) {
        Objects = new T[initialSize];
        Count = 0;
        Collections = collections;
    }

    public BufferedList(int initialSize = 10) {
        Objects = new T[initialSize];
        Count = 0;
    }

    public BufferCollections<T>? Collections {get;}
    private SizedListStorage<T>? ListStorage => Collections?.ListStorage;
    private SizedArrayStorage<T>? ArrayStorage => Collections?.ArrayStorage;
    private ImmutableBufferStorage<T>? ImmutableBufferStorage => Collections?.ImmutableBufferStorage;
    
    public BufferedList(BufferedList<T> other) {
        Objects = new T[other.Count];
        Count = 0;
        Collections = other.Collections;
        AddBufferedList(other);
    }
    
    public int Capacity => Objects.Length;

    public int 
    IndexOf(T item) {
        var equalityComparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) {
            if (equalityComparer.Equals(Objects[i], item))
                return i;
        }
        return -1;
    }

    public void 
    Insert(int index, T item) {
        throw new NotImplementedException();
    }

    public bool 
    Remove(T item) {
        throw new NotImplementedException();
    }

    public void 
    RemoveAt(int index) {
        throw new NotImplementedException();
    }

    public T this[int index]{
        get => Objects[index];
        set{
            while (index >= Objects.Length) 
                ExtendBuffer();
            Objects[index] = value;
        }
    }

    public bool IsEmpty => Count == 0;

    public void 
    Add(T item) {
        if (Count == Objects.Length)
            ExtendBuffer();
        if (Count == Objects.Length)
            throw new InvalidOperationException();
        Objects[Count] = item;
        Count ++;
    }

    public void 
    Clear() => Objects.Clear();

    public bool 
    Contains(T item) {
        var equalityComparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) 
            if (equalityComparer.Equals(Objects[i], item))
                return true;
        
        return false;
    }

    public void 
    CopyTo(T[] array, int arrayIndex) {
        foreach (var element in this) 
            array[arrayIndex++] = element;
    }

    public void
    AddRange(BufferedList<T> other, int index, int count) {
        for (var i = index; i < index + count; i++) 
            Add(other[i]);
    }

    public void
    AddRange(T[] other) {
        for (var i = 0; i < other.Length; i++) 
            Add(other[i]);
    }

    public void
    AddBufferedList(BufferedList<T> other) {
        AddRange(other, 0, other.Count);
    }
    
    private void 
    ExtendBuffer() {
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
        var result = ListStorage?.GetList(Count) ?? new List<T>(Count);
        for (var i = 0; i < Count; i++) {
            if (result.Count <= i)
                result.Add(this[i]);
            else
                result[i] = this[i];
        }
        return result;
    }

    public List<T>
    ToNewList() {
        var result = new List<T>(Count);
        for (var i = 0; i < Count; i++) 
            result.Add(this[i]);
        
        return result;
    }

    public ImmutableBuffer<T>
    ToImmutableBuffer() => ImmutableBufferStorage?.EmptyBuffer.Add(this) ?? ImmutableBuffer<T>.Empty.Add(this);
    
    public T[]
    GetEmptyArray(int size) {
        var result = ArrayStorage?.GetArray(Count) ?? new T[Count];
        return result;
    }

    public T[]
    ToArray() {
        var result = ArrayStorage?.GetArray(Count) ?? new T[Count];
        for (var i = 0; i < Count; i++) 
            result[i] = this[i];

        return result;
    }

    public T[]
    ToNewArray() {
        var result = new T[Count];
        for (var i = 0; i < Count; i++) 
            result[i] = this[i];

        return result;
    }

    public void
    SetLast(T item) => Objects[Count - 1] = item;
    
    public T
    Last() => Objects[Count - 1];
}
}
