namespace com.hy.synology.filemanager.core.crypto
{
    public interface IDecryptor
    {
        byte[] DecryptBlock(byte[] encryptedBlock, bool isLastBlock);
    }
}