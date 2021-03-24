namespace com.hy.synology.filemanager.core.crypto
{
    public interface IEncryptor
    {
        byte[] Encrypt(byte[] dataBlock);
    }
}