using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuffers {
    
public class 
BytesPool{
    public BytesChunk[] Chunks {get; private set;} 
    public int Position {get;private set;}
    public BytesPool(int chunkSize = 1024 * 1024) {
        _chunkSize = chunkSize;
        Chunks = new BytesChunk[100];
        Chunks[0] = new BytesChunk(_chunkSize);
        Position = 0;
    }

    private readonly int _chunkSize;
    private BytesChunk _currentChunk => Chunks[_chunkId];
    private int _chunkId => Position / _chunkSize;
    private int _chunkPosition => Position - _chunkId * _chunkSize;
    
    public BytesRent
    RentBytes(int bytesCount) =>
        HasChunkFreeBytes(bytesCount) ? RentFromCurrentChunk(bytesCount) : GoToNextChunk().RentFromCurrentChunk(bytesCount);
    
    private BytesRent
    RentFromCurrentChunk(int bytesCount) {
        var result = new BytesRent(bytesChunk: _currentChunk, startIndex: _chunkPosition, endIndex: _chunkPosition + bytesCount - 1);
        Position += bytesCount;
        return result;
    }
    
    public void 
    Reclaim() =>
        Position = 0;
    
    private bool
    HasCurrentChunk() {
        if (!HasSpaceForChunk()) {
            ExtendChunks();
            return false;
        }
        return Chunks[_chunkId] != null;
    }

    private bool
    HasSpaceForChunk() => Chunks.Length > _chunkId;

    private BytesPool
    GoToNextChunk() {
        Position = (_chunkId + 1) * _chunkSize;
        if (!HasSpaceForChunk())
            ExtendChunks();
        Chunks[_chunkId] = new BytesChunk(chunkSize: _chunkSize);
        return this;
    }

    private void
    ExtendChunks() {
        var newChunks = new BytesChunk[Chunks.Length + 100];
        for (var i = 0; i < Chunks.Length; i++)
            newChunks[i] = Chunks[i];
        Chunks = newChunks;
    }

    private bool
    HasChunkFreeBytes(int bytesCount) {
        if (!HasCurrentChunk())
            Chunks[_chunkId] = new BytesChunk(chunkSize: _chunkSize);
        return _chunkPosition + bytesCount <= _chunkSize;
    }
}

public class 
BytesChunk{
    public byte[] Bytes {get;}

    public BytesChunk(int chunkSize) {
        Bytes = new byte[chunkSize];
    }
    
    public int Length => Bytes.Length;
}

public readonly struct 
BytesRent{
    public BytesChunk BytesChunk {get;}
    public int StartIndex {get;} 
    public int EndIndex {get;}

    public BytesRent(BytesChunk bytesChunk, int startIndex, int endIndex) {
        BytesChunk = bytesChunk;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public int RentLength => EndIndex - StartIndex + 1;
}
}
