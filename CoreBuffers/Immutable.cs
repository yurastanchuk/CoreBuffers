using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoreBuffers {
    
public class 
ImmutableBuffer {
    public static ImmutableBuffer<T>
    Create<T>(T item) => new ImmutableBuffer<T>(buffer: new T[1]{item}, bufferStorage: new ImmutableBufferStorage<T>(listsStorage: new SizedListStorage<T>(), arrayStorage: new SizedArrayStorage<T>()));

    public static ImmutableBuffer<T>
    Create<T>(T[] objects) => new ImmutableBuffer<T>(buffer: objects, bufferStorage: new ImmutableBufferStorage<T>(listsStorage: new SizedListStorage<T>(), arrayStorage: new SizedArrayStorage<T>()));

    public static ImmutableBuffer<T>
    Create<T>(IList<T> objects) => new ImmutableBuffer<T>(buffer: objects.ToArray(), bufferStorage: new ImmutableBufferStorage<T>(listsStorage: new SizedListStorage<T>(), arrayStorage: new SizedArrayStorage<T>()));
}

public class 
ImmutableBuffer<T> : IEnumerable<T>, IList<T>{
    public T[] Objects {get;}
    
    public ImmutableBuffer(int size, ImmutableBufferStorage<T> bufferStorage, SizedListStorage<T>? listsBuffer = null,  SizedArrayStorage<T>? arrayBuffer = null) {
        Objects = new T[size];
        States = bufferStorage;
        Count = size;
        ListStorage = listsBuffer ?? new SizedListStorage<T>();
        ArrayStorage = arrayBuffer ?? new SizedArrayStorage<T>();
    }
    
    public ImmutableBuffer(T[] buffer, ImmutableBufferStorage<T> bufferStorage, SizedListStorage<T>? listsBuffer = null,  SizedArrayStorage<T>? arrayBuffer = null) {
        Objects = buffer;
        Count = buffer.Length;
        States = bufferStorage;
        ListStorage = listsBuffer ?? new SizedListStorage<T>();
        ArrayStorage = arrayBuffer ?? new SizedArrayStorage<T>();
    }

    public static ImmutableBuffer<T>
    Empty {get;} = new ImmutableBuffer<T>(size: 0, bufferStorage: new ImmutableBufferStorage<T>(listsStorage: new SizedListStorage<T>(), arrayStorage: new SizedArrayStorage<T>()));
    
    public bool IsReadOnly => true;
    
    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
    
    public struct Enumerator : IEnumerator<T>, IEnumerator{
        private readonly T[] _items;
        private int _index;
        private T? _current;
        private readonly int _size;
        internal Enumerator(ImmutableBuffer<T> buffer) {
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

    public int Count {get;}

    public ImmutableBufferStorage<T> States {get;}
    
    /// <summary>
    /// Is used to get List from BufferedList
    /// </summary>
    private SizedListStorage<T> ListStorage {get;} 
    private SizedArrayStorage<T> ArrayStorage {get;} 

    public List<T>
    GetMutableList(int size) => ListStorage.GetList(size);

    public T[]
    GetEmptyArray(int size) => ArrayStorage.GetArray(size);

    public T this[int index] => Objects[index];
    
    public ImmutableBuffer<T>
    Add(T item) {
        var newImmutableBuffer = States.GetBuffer(Count + 1);
        for (var i = 0; i < Count; i++) 
            newImmutableBuffer.Objects[i] = Objects[i];
        newImmutableBuffer.Objects[Count] = item;
        return newImmutableBuffer;
    }

    public ImmutableBuffer<T>
    Add(IList<T> item, int? count = null) {
        count ??= item.Count;
        var newImmutableBuffer = States.GetBuffer(Count + count.Value);
        for (var i = 0; i < Count; i++) 
            newImmutableBuffer.Objects[i] = Objects[i];
        for (var i = Count; i < count; i++) 
            newImmutableBuffer.Objects[i] = item[i];
        return newImmutableBuffer;
    }
    
    public ImmutableBuffer<T>
    Add(T[] item) {
        var newImmutableBuffer = States.GetBuffer(Count + item.Length);
        for (var i = 0; i < Count; i++) 
            newImmutableBuffer.Objects[i] = Objects[i];
        for (var i = Count; i < item.Length; i++) 
            newImmutableBuffer.Objects[i] = item[i];
        return newImmutableBuffer;
    }
    
    public ImmutableBuffer<T>
    RemoveAt(int index) {
        var newImmutableBuffer = States.GetBuffer(Count - 1);
        int _objectsAdded = 0;
        for (var i = 0; i < Objects.Length; i++) {
            if (i == index)
                continue;
            newImmutableBuffer.Objects[_objectsAdded] = Objects[i];
            _objectsAdded++;
        }
        return newImmutableBuffer;
    }

    public ImmutableBuffer<T>
    SetItem(int index, T newItem) {
        var newImmutableBuffer = States.GetBuffer(Count);
        if (index >= Count)
            throw new IndexOutOfRangeException();
        for (var i = 0; i < Objects.Length; i++) {
            if (i == index)
                newImmutableBuffer.Objects[i] = newItem;
            else
                newImmutableBuffer.Objects[i] = Objects[i];
        }
        return newImmutableBuffer;
    }

    public ImmutableBuffer<T>
    ReplaceLast(T newItem) {
        var newImmutableBuffer = States.GetBuffer(Count);
        for (var i = 0; i < Objects.Length; i++) {
            if (i == Objects.Length - 1)
                newImmutableBuffer.Objects[i] = newItem;
            else
                newImmutableBuffer.Objects[i] = Objects[i];
        }
        return newImmutableBuffer;
    }
    
    public T
    Last() => Objects[Count - 1];
    
    public int IndexOf(T item) {
        var equalityComparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++) {
            if (equalityComparer.Equals(Objects[i], item))
                return i;
        }
        return -1;
    }
    
    void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
    void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
    void ICollection<T>.Add(T item) => throw new NotSupportedException();
    void ICollection<T>.Clear() => throw new NotSupportedException();
    bool ICollection<T>.Remove(T item)  => throw new NotSupportedException();
    T IList<T>.this[int index] { get => Objects[index]; set => throw new NotImplementedException(); }

    public bool 
    Contains(T item) {
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
    
    public bool 
    VerifyArgumentNonEmpty(string name) =>
        Count > 0 ? true : throw new ArgumentException(name, $"Expected minimum count to be {1} but was {Count}");

    public bool 
    VerifyArgumentCount(int minCount, string? name = null) =>
        Count < minCount ? true : throw new ArgumentException(name, $"Expected minimum count to be {minCount} but was {Count}");
    
}

public class 
ImmutableBufferStorage<T> {
    private List<ImmutableBufferSizedStorage<T>> Buffers {get;set;}
    private BufferedListStorage<T> BufferedLists {get;set;} 
    public  ImmutableBufferStorage(SizedListStorage<T> listsStorage, SizedArrayStorage<T> arrayStorage, int initialSize = 8) {
        Buffers = new List<ImmutableBufferSizedStorage<T>>(initialSize);
        BufferedLists = new BufferedListStorage<T>(initialSize);
        GetExpandBuffer(initialSize);
        ListStorage = listsStorage ?? new SizedListStorage<T>();
        ArrayStorage = arrayStorage ?? new SizedArrayStorage<T>();
    }

    private SizedListStorage<T> ListStorage {get;}
    private SizedArrayStorage<T> ArrayStorage {get;}
    
    private List<ImmutableBufferSizedStorage<T>>
    GetExpandBuffer(int size) {
        var newBuffer = new List<ImmutableBufferSizedStorage<T>>(size + 1);
        for (int i = 0; i < Buffers.Count; i++) 
            newBuffer.Add(Buffers[i]);
        for (int i = Buffers.Count; i <= size; i++) {
            newBuffer.Add(new ImmutableBufferSizedStorage<T>(i, this));
        }
        return newBuffer;
    }
    
    public ImmutableBuffer<T>
    Empty => Buffers[0].GetObject(this, ListStorage, ArrayStorage);
    
    public ImmutableBuffer<T>
    GetBuffer(int size) {
        if (Buffers.Count > size)
            return Buffers[size].GetObject(this , ListStorage, ArrayStorage);
        Buffers = GetExpandBuffer(size);
        return Buffers[size].GetObject(this , ListStorage, ArrayStorage);
    }

    public BufferedList<T>
    GetList() => BufferedLists.GetList();

    public void
    Reclaim() {
        ListStorage.Reclaim();
        ArrayStorage.Reclaim();
        for (var i = 0; i < Buffers.Count; i++) 
            Buffers[i].Reclaim();
        
        BufferedLists.Reclaim();
    }

    public ImmutableBuffer<T>
    CreateBuffer(IList<T> source, int? count = null) {
        var result = GetBuffer(0);
        return result.Add(source, count);
    }

    public ImmutableBuffer<T>
    CreateBuffer(T item) {
        var result = GetBuffer(0);
        return result.Add(item);
    }
        
}

public class 
ImmutableBufferSizedStorage<T> {
    private List<ImmutableBuffer<T>> Objects {get;}
    private int Position {get;set;}
    private int SizeOfList {get;}

    public ImmutableBufferSizedStorage(int sizeOfList, ImmutableBufferStorage<T> bufferStorage) {
        SizeOfList = sizeOfList;
        Objects = new List<ImmutableBuffer<T>>(1);
        Objects.Add(new ImmutableBuffer<T>(SizeOfList, bufferStorage));
    }

    public ImmutableBuffer<T> 
    GetObject(ImmutableBufferStorage<T> bufferStorage, SizedListStorage<T> listsStorage,  SizedArrayStorage<T> arrayStorage) {
        Position++;
        if (Objects.Count > Position){
            var item = Objects[Position];
            return item;
        }
        var result = new ImmutableBuffer<T>(size: SizeOfList, bufferStorage: bufferStorage,  listsStorage, arrayStorage);
        Objects.Add(result);
        return result;
    }
    
    public void Reclaim() => Position = 0;
}

}
