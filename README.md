# What is this
This is a tool to decrypt Synology Cloud Sync encrypted files.
This repository is at a very early POC stage.

# Why it is here
I have a DS218+ and Microsoft is my new boss. This project is a playground of .Net.

# How to use it
A console application is released to prove it works.
1. Download the executable release. It is a single window x86 executable, **without**
the prerequisite of .Net installation.
2. Place the **key.zip** in the same directory as **SimpleCLI-x86.exe**
3. Drag the encrypted files and drop them on **SimpleCLI-x86.exe**
4. **SimpleCLI-x86.exe** will create a directory *output* in the same directory as 
itself and write the decrypted files into it.
5. Known limitation: this application is a POC, the unit test coverage is literally zero.
But it should be safe to use. The file is hashed to compare with the md5 stored in the meta.

# Special Thanks
This project is a remake of 
[marnix/synology-decrypt](https://github.com/marnix/synology-decrypt/).
Many thanks to [marnix](https://github.com/marnix) ]
who is the guy who did the reverse engineering.

# Planned development 
* <del>Implement file hash
* <del>Apply stream to read and process file data
* <del>Implement a ringbuffer to copy between streams 
* Apply async for io, the decrypting speed can reach 400MB/s, I have no idea if this async
  optimization is necessary.
* Implement Azure source connector
* Implement S3 source connector
* Implement WPF UX
* Apply Reactive

