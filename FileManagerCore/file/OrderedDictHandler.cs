using System;
using System.Collections.Generic;
using System.IO;

namespace com.hy.synology.filemanager.core.crypto
{
    public class OrderedDictHandler: IFileStreamHandler<IDictionary<string,Object>>
    {
        private readonly HandlerFactory _factory;
        public OrderedDictHandler(HandlerFactory factory)
        {
            this._factory = factory;
        }

        public Type ReturnType => typeof(IDictionary<string,Object>);
        public byte SupportedTag => 0x42;

        public IDictionary<string,Object> Handle(BinaryReader br)
        {
            IDictionary<string,Object> returnDict = new Dictionary<string, Object>();
            //read key, 0x10 expected
            Boolean hasNext = true;
            while (hasNext)
            {
                byte keyTag = br.ReadByte();
                switch (keyTag)
                {
                    case 0x10:
                        IFileStreamHandler<string> stringHandler = this._factory.GetHandler<string>(keyTag);
                        string key = stringHandler.Handle(br);
                        //read value
                        byte valueTag = br.ReadByte();
                        IFileStreamHandler<Object> valueHandler = this._factory.GetHandler<Object>(valueTag);
                        if (valueHandler == null)
                        {
                            throw new InvalidDataException();
                        }

                        object value = valueHandler.Handle(br);
                        returnDict.Add(key, value);
                        break;
                    case 0x40:
                        hasNext = false;
                        continue;
                    default:
                        throw new InvalidDataException();
                }
            }

            return returnDict;
        }
    }
}