using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace com.hy.synology.filemanager.core.crypto
{
    public class AesCbcCryptor : IDecryptor
    {
        private  BufferedBlockCipher _cipher;
        private  ParametersWithIV _parametersWithIv;
        public AesCbcCryptor(byte[] key, byte[] iv)
        {
            KeyParameter keyParam = new KeyParameter(key);
            this._parametersWithIv = new ParametersWithIV(keyParam, iv, 0, iv.Length);
            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine);
            IBlockCipherPadding padding = new Pkcs7Padding();
            this._cipher = new BufferedBlockCipher(blockCipher);
            this._cipher.Init(false, this._parametersWithIv);
        }

        public byte[] DecryptBlock(byte[] encryptedBlock, bool isLastBlock)
        {
            if (isLastBlock)
            {
                byte[] decryptedBlock = new byte[this._cipher.GetOutputSize(encryptedBlock.Length)];
                int length = this._cipher.ProcessBytes(encryptedBlock, decryptedBlock, 0);
                this._cipher.DoFinal(decryptedBlock,length);
                return decryptedBlock;
            }
            return this._cipher.ProcessBytes(encryptedBlock);
        }
    }
}