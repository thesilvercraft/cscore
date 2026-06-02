vgmstream input addon for SilverCraft's fork of CSCore  
Does not include native binaries, source them yourself.  
To get the required `libvgmstream.so`, download https://github.com/vgmstream/vgmstream/
and compile with
```
cmake -B build -DBUILD_SHARED_LIBS=ON
cmake --build build
```