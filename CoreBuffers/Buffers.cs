using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreBuffers {
    
public class 
StructArrayBuffer<T> where T: struct{
    public T[][] Buffers {get;private set;} 
    public StructArrayBuffer(int initialSize = 8) {
        Buffers = Array.Empty<T[]>();
        GetExpandBuffer(initialSize);
    }
    
    private T[][]
    GetExpandBuffer(int size) {
        var newBuffer = new T[size + 1][];
        for (int i = 0; i < Buffers.Length; i++) 
            newBuffer[i] = Buffers[i];
        for (int i = Buffers.Length; i <= size; i++) {
            newBuffer[i] = new T[i];
            for (int j = 0; j < i; j++) 
                newBuffer[i][j] = new T();
        }
        return newBuffer;
    }
        
    
    public T[] 
    GetArray(int size) {
        if (Buffers.Length > size)
            return Buffers[size];
        Buffers = GetExpandBuffer(size);
        return Buffers[size];
    }
}

public class 
SizedListStorage<T> {
    public List<ListSizedBuffer<T>> Buffers {get;private set;} 
    public SizedListStorage(int initialSize = 8) {
        Buffers = new List<ListSizedBuffer<T>>(8);
        GetExpandBuffer(initialSize);
    }
    
    private List<ListSizedBuffer<T>>
    GetExpandBuffer(int size) {
        var newBuffer = new List<ListSizedBuffer<T>>(size + 1);
        for (int i = 0; i < Buffers.Count; i++) 
            newBuffer.Add(Buffers[i]);
        for (int i = Buffers.Count; i <= size; i++) {
            newBuffer.Add(new ListSizedBuffer<T>(i));
        }
        return newBuffer;
    }
    
    public List<T> 
    GetList(int size) {
        if (Buffers.Count > size)
            return Buffers[size].GetObject();
        Buffers = GetExpandBuffer(size);
        return Buffers[size].GetObject();
    }

    public void
    Reclaim() {
        for (var i = 0; i < Buffers.Count; i++) {
            Buffers[i].Reclaim();
        }
    }
}

public class 
SizedArrayStorage<T> {
    public ArraySizedBuffer<T>[] Buffers {get;private set;} 
    public SizedArrayStorage(int initialSize = 8) {
        Buffers = new ArraySizedBuffer<T>[initialSize];
        for (int i = 0; i < Buffers.Length; i++) {
            Buffers[i] = new ArraySizedBuffer<T>(i);
        }
    }
    
    private ArraySizedBuffer<T>[]
    GetExpandBuffer(int size) {
        var newBuffer = new  ArraySizedBuffer<T>[size + 1];
        for (int i = 0; i < Buffers.Length; i++) 
            newBuffer[i] = Buffers[i];
        for (int i = Buffers.Length; i <= size; i++) {
            newBuffer[i] = new ArraySizedBuffer<T>(i);
        }
        return newBuffer;
    }
    
    public T[] 
    GetArray(int size) {
        if (Buffers.Length > size)
            return Buffers[size].GetObject();
        Buffers = GetExpandBuffer(size);
        return Buffers[size].GetObject();
    }
    
    public void
    Reclaim() {
        for (var i = 0; i < Buffers.Length; i++) {
            Buffers[i].Reclaim();
        }
    }
}

public class 
ListSizedBuffer<T> {
    public List<List<T>> Objects {get;} 
    public int Position {get;private set;}
    public int SizeOfList {get;}

    public ListSizedBuffer(int sizeOfList) {
        SizeOfList = sizeOfList;
        Objects = new List<List<T>>(1);
        Objects.Add(new List<T>(SizeOfList));
    }

    public List<T> GetObject() {
        Position++;
        if (Objects.Count > Position){
            var item = Objects[Position];
            return item;
        }
        var result = new List<T>(SizeOfList);
        Objects.Add(result);
        return result;
    }
    
    public void Reclaim() => Position = 0;
}

public class 
ArraySizedBuffer<T> {
    public List<T[]> Objects {get;} 
    public int Position {get;private set;}
    public int SizeOfArray {get;}

    public ArraySizedBuffer(int sizeOfArray) {
        SizeOfArray = sizeOfArray;
        Objects = new List<T[]>(1);
        Objects.Add(new T[SizeOfArray]);
    }

    public T[] GetObject() {
        Position++;
        if (Objects.Count > Position){
            var item = Objects[Position];
            return item;
        }
        var result = new T[SizeOfArray];
        Objects.Add(result);
        return result;
    }
    
    public void Reclaim() => Position = 0;
}

public class 
ObjectBuffer<T> where T: new() {
    public List<T> Objects {get;} 
    public int Position {get;private set;}
    public ObjectBuffer(int initialSize = 12) {
        Objects = new List<T>(capacity: initialSize);
        for (int i = 0; i < initialSize; i++) 
            Objects.Add(new T());
    }

    public ObjectBuffer(List<T> objects) {
        Objects = objects;
    }

    public T GetObject()  {
        Position++;
        if (Objects.Count > Position){
            var item = Objects[Position];
            return item;
        }
        var result = new T();
        Objects.Add(result);
        return result;
    }
    
    public void Reclaim() => Position = 0;
}

public class 
ListBuffer<T> where T: new() {
    public List<List<T>> Objects {get;} 
    public int Position {get;private set;}

    public ListBuffer() {
        Objects = new List<List<T>>();
        Position = 0;
    }

    public List<T> 
    GetObject() {
        if (Objects.Count > Position){
            var item = Objects[Position];
            item.Clear();
            return item;
        }
        var result = new List<T>();
        Objects.Add(result);
        Position++;
        return result;
    }
    
    public void Reclaim() => Position = 0;
}

public class 
ObjectsBuffer<T> : IEnumerable<T> {
    public T[] Objects {get; private set;}
    public int Count {get; private set;}
    public ObjectsBuffer(int initialSize = 12) {
        Objects = new T[initialSize];
        Count = 0;
    }
    public ObjectsBuffer() {
        Objects = new T[12];
        Count = 0;
    }
    private SizedListStorage<T> ListStorage {get;} = new SizedListStorage<T>();

    public int Capacity => Objects.Length;

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
    Clear() {
        Objects.Clear();
        Reclaim();
    } 

    public void 
    Add(T item) {
        if (Count == Objects.Length)
            ExtendBuffer();
            
        Objects[Count] = item;
        Count ++;
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
        internal Enumerator(ObjectsBuffer<T> buffer) {
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
    public ObjectsBuffer<T> 
    Reclaim() {
        Count = 0;
        ListStorage.Reclaim();
        //_enumerator.Reset();
        return this;
    }
    /// <summary>
    /// Use this if you need to reuse added entities
    /// </summary>
    /// <returns></returns>
    public ObjectsBuffer<T> 
    SetCount(int count) {
        Count = count;
        //_enumerator.Reset();
        return this;
    }

    public List<T>
    ToList() {
        var result = ListStorage.GetList(Count);
        for (var i = 0; i < Count; i++) 
            result.Add(this[i]);
        return result;
    }

    public void
    Reset() {
        Count = 0;
        ListStorage.Reclaim();
    }

}

public class 
ObjectsArrayBuffer<T> where T: new(){
    public T[][] Buffers {get;private set;} 
    public  ObjectsArrayBuffer(int initialSize = 8) {
        Buffers = Array.Empty<T[]>();
        GetExpandBuffer(initialSize);
    }
    
    private T[][]
    GetExpandBuffer(int size) {
        var newBuffer = new T[size + 1][];
        for (int i = 0; i < Buffers.Length; i++) 
            newBuffer[i] = Buffers[i];
        for (int i = Buffers.Length; i <= size; i++) {
            newBuffer[i] = new T[i];
            for (int j = 0; j < i; j++) 
                newBuffer[i][j] = new T();
        }
        return newBuffer;
    }
        
    
    public T[] 
    GetArray(int size) {
        if (Buffers.Length > size)
            return Buffers[size];
        Buffers = GetExpandBuffer(size);
        return Buffers[size];
    }
}

public class 
ObjectPool<T> where T: new() {
    public Stack<T> Objects {get;}

    public static readonly ObjectPool<T> Shared = new ObjectPool<T>();

    public ObjectPool(int initialSize = 12) {
        Objects = new Stack<T>(capacity:initialSize);
        for (int i = 0; i < initialSize; i++) 
            Objects.Push(new T());
    }

    public T 
    Rent() {
        if (Objects.TryPop(out var result))
            return result;
        return new T();
    }
    
    public void 
    Return(T @object) {
        lock(Objects) 
            Objects.Push(@object);
    }
}

public class 
LazyBuffer<Tin, Tout>{
    private Tout _value {get; set;}
    public Tout Value {
        get {
            if (ValueCreated)
                return _value;
            _value = GetValue(Source);
            ValueCreated = true;
            return _value;
        }
    }
    public Tin Source {get; private  set;}
    public Func<Tin, Tout> GetValue {get;}
    private bool ValueCreated {get; set;} 
    public LazyBuffer(Tin source, Func<Tin, Tout> getValue) {
        GetValue = getValue;
        Source = source;
    }
    public void Reset() {
        ValueCreated = false;
    }
}

}
