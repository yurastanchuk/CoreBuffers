using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuffers {
    
public class 
EntityBuffer<T> where T : new() {
    public ImmutableBufferStorage<T> ImmutableBufferStorage {get;} 
    public BufferedListStorage<T> BufferedListStorage {get;}
    public ObjectBuffer<T> Objects {get;}

    public EntityBuffer() {
        Objects = new ObjectBuffer<T>();
        ListStorage = new SizedListStorage<T>();
        ArrayStorage = new SizedArrayStorage<T>();
        BufferedListStorage = new BufferedListStorage<T>(ListStorage, ArrayStorage);
        ImmutableBufferStorage = new ImmutableBufferStorage<T>(BufferedListStorage);
    }

    private SizedListStorage<T> ListStorage {get;} 
    private SizedArrayStorage<T> ArrayStorage {get;}

    public T
    GetObject() => Objects.GetObject();

    public ImmutableBuffer<T>
    GetImmutableBuffer(int size) => ImmutableBufferStorage.GetBuffer(size);

    public BufferedList<T>
    GetList() => BufferedListStorage.GetList();

    public List<T>
    GetList(int size) => ListStorage.GetList(size);
    
    public T[]
    GetArray(int size) => ArrayStorage.GetArray(size);

    public void 
    Reclaim() {
        Objects.Reclaim();
        ImmutableBufferStorage.Reclaim();
        BufferedListStorage.Reclaim();
        ListStorage.Reclaim();
        ArrayStorage.Reclaim();
    }
}

public class 
ValueCollectionsBuffer<T>{
    public ImmutableBufferStorage<T> ImmutableBufferStorage {get;} 
    public BufferedListStorage<T> BufferedListStorage {get;}

    public ValueCollectionsBuffer() {
        ListStorage = new SizedListStorage<T>();
        ArrayStorage = new SizedArrayStorage<T>();
        BufferedListStorage = new BufferedListStorage<T>(ListStorage, ArrayStorage);
        ImmutableBufferStorage = new ImmutableBufferStorage<T>(BufferedListStorage);
    }

    public SizedListStorage<T> ListStorage {get;} 
    public SizedArrayStorage<T> ArrayStorage {get;}

    public ImmutableBuffer<T>
    GetImmutableBuffer(int size) => ImmutableBufferStorage.GetBuffer(size);

    public BufferedList<T>
    GetList() => BufferedListStorage.GetList();

    public T[]
    GetArray(int size) => ArrayStorage.GetArray(size);

    public void 
    Reclaim() {
        ImmutableBufferStorage.Reclaim();
        BufferedListStorage.Reclaim();
        ListStorage.Reclaim();
        ArrayStorage.Reclaim();
    }
}

}
