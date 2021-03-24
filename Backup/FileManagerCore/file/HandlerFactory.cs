using System;
using System.Collections;
using System.Collections.Generic;

namespace com.hy.synology.filemanager.core.crypto
{
    public class HandlerFactory
    {
        private readonly IDictionary<byte,IFileStreamHandler<object>> _handlerDict;

        public HandlerFactory()
        {
            this._handlerDict = new Dictionary<byte,IFileStreamHandler<object>>();
        }

        public void AddHandler(IFileStreamHandler<object> handler)
        {
            this._handlerDict[handler.SupportedTag] = handler;
        }
        public IFileStreamHandler<T> GetHandler<T>(byte tag)
        {
            this._handlerDict.TryGetValue(tag, out var returnValue);
            return (IFileStreamHandler<T>) returnValue;
        }
    }
}