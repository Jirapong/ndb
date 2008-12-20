mkdir Deploy

ilmerge BananaCoding.Tools.Database\Bin\Debug\ndb.exe BananaCoding.Tools.Database\Bin\Debug\BananaCoding.CommandLineParser.dll /out:Deploy\ndb.exe /t:exe
copy BananaCoding.Tools.Database\Bin\Debug\ndb.exe.config Deploy\ndb.exe.config
