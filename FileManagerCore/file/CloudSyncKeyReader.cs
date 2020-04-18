using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace com.hy.synology.filemanager.core.file
{
    public class CloudSyncKeyReader
    {
        //TODO refactor to decouple bouncy castle @HUYAO
        public AsymmetricCipherKeyPair GetKeyPair(string filePath)
        {
            try
            {
                string priContent = null;
                string pubContent = null;
                using ZipArchive archive = ZipFile.OpenRead(filePath);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.Equals("private.pem"))
                    {
                        using (Stream stream = entry.Open())
                        using (MemoryStream sr = new MemoryStream())
                        {
                            stream.CopyTo(sr);
                            priContent = Encoding.ASCII.GetString(sr.ToArray());
                        }
                    }

                    /*if (entry.Name.Equals("public.pem"))
                    {
                        using (Stream stream = entry.Open())
                        using (MemoryStream sr = new MemoryStream())
                        {
                            stream.CopyTo(sr);
                            pubContent = Encoding.ASCII.GetString(sr.ToArray());
                        }
                    }*/
                }

                if (priContent != null)
                {
                    using var reader = new StringReader(priContent);
                    var keyPair = (AsymmetricCipherKeyPair) new PemReader(reader).ReadObject();
                    return keyPair;
                }
                throw new InvalidDataException("{0} doesn't contain valid" );
            }
            catch (Exception ex)
            {
                //TODO log
                throw;
            }
        }
    }
}