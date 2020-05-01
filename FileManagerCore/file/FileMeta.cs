using System;
using System.Collections.Generic;
using FileManagerCore.file;

namespace com.hy.synology.filemanager.core.file
{
    public class FileMeta3 : IFileMeta
    {
        public FileVersion MinVer => new FileVersion(3, 0);
        public FileVersion MaxVer => new FileVersion(3, 999);

        public bool Compress { get; private set; }
        public string Digest { get; private set; }
        public byte[] EncKey1 { get; private set; }
        public byte[] EncKey2 { get; private set; }
        public bool Encrypt { get; private set; }
        public string FileName { get; private set; }
        public string Key1Hash { get; private set; }
        public string Key2Hash { get; private set; }
        public string Salt { get; private set; }
        public string SessionKeyHash { get; private set; }
        public FileVersion Version { get; private set; }

        internal FileMeta3()
        {
        }

        public FileMeta3(bool compress, string digest, byte[] encKey1, byte[] encKey2, bool encrypt,
            string fileName, string key1Hash, string key2Hash, string salt,
            string sessionKeyHash, FileVersion version)
        {
            this.Compress = compress;
            this.Digest = digest;
            this.EncKey1 = encKey1;
            this.EncKey2 = encKey2;
            this.Encrypt = encrypt;
            this.FileName = fileName;
            this.Key1Hash = key1Hash;
            this.Key2Hash = key2Hash;
            this.Salt = salt;
            this.SessionKeyHash = sessionKeyHash;
            this.Version = version;
        }

        public static FileMeta3 fromDictionary(IDictionary<string,object> dict)
        {
            FileMeta3 instance = new FileMeta3();
            foreach (KeyValuePair<string, object> entry in dict)
            {
                switch (entry.Key)
                {
                    case "compress":
                        instance.Compress = (int) entry.Value > 0;
                        break;
                    case "digest":
                        instance.Digest = (string) entry.Value;
                        break;
                    case "enc_key1":
                        instance.EncKey1 =  Convert.FromBase64String((string) entry.Value);
                        break;
                    case "enc_key2":
                        instance.EncKey2 = Convert.FromBase64String((string) entry.Value);
                        break;
                    case "encrypt":
                        instance.Encrypt = (int) entry.Value > 0;
                        break;
                    case "file_name":
                        instance.FileName = (string) entry.Value;
                        break;
                    case "key1_hash":
                        instance.Key1Hash = (string) entry.Value;
                        break;
                    case "key2_hash":
                        instance.Key2Hash = (string) entry.Value;
                        break;
                    case "salt":
                        instance.Salt = (string) entry.Value;
                        break;
                    case "session_key_hash":
                        instance.SessionKeyHash = (string) entry.Value;
                        break;
                    case "version":
                        IDictionary<string, object> versionDict = (IDictionary<string, object>) entry.Value;
                        int major = (int)versionDict["major"];
                        int minor = (int)versionDict["minor"];
                        instance.Version = new FileVersion(major, minor);
                        break;
                    default:
                        //TODO log not supported key
                        break;
                }
            }

            return instance;
        }
    }
}