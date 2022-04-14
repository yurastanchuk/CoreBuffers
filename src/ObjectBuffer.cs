using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuffers {
    
public class 
EntityBuffer<T> where T : new() {
    public ObjectBuffer<T> Objects {get;}
    public BufferCollections<T> Collections {get;}

    public EntityBuffer() {
        Objects = new ObjectBuffer<T>();
        Collections = new BufferCollections<T>();
    }

    public ImmutableBufferStorage<T> ImmutableBufferStorage => Collections.ImmutableBufferStorage;
    public BufferedListStorage<T> BufferedListStorage => Collections.BufferedListStorage;
    public SizedListStorage<T> ListStorage => Collections.ListStorage;
    public SizedArrayStorage<T> ArrayStorage => Collections.ArrayStorage;

    public T
    GetObject() => Objects.GetObject();

    public ImmutableBuffer<T>
    GetImmutableBuffer(int size) => ImmutableBufferStorage.GetBuffer(size);

    public BufferedList<T>
    GetList() => BufferedListStorage.GetList();

    public List<T>
    GetSizedList(int size) => ListStorage.GetList(size);
    
    public T[]
    GetArray(int size) => ArrayStorage.GetArray(size);

    public ImmutableBuffer<T>
    CreateImmutableBuffer(T item) => ImmutableBufferStorage.CreateBuffer(item);
    
    public ImmutableBuffer<T>
    CreateImmutableBuffer(IList<T> source, int? count = null) => ImmutableBufferStorage.CreateBuffer(source, count);
    
    public void 
    Reclaim() {
        Objects.Reclaim();
        Collections.Reclaim();
    }
}

}
