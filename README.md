# HimeChange
A program for extracting and replacing files from the unity bundles used by Bokuhime Project. This is primarily a wrapper around the [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET) library for reading unity assets.

Current limitations:
* Only lua files are supported.
* Files can only be replaced, new files can not be added.


# Usage
## Unpack
`HimeChange unpack bundlefile [filedir]`

* `bundlefile` is the unity bundle file containing the files to extract.
* `filedir` is an optional parameter for directory to output the extracted files. If ommited, files will be extracted to the current directory.

Note that this will overwrite any existing files in the extracted location. 

## Pack
`HimeChange pack bundlefile newbundlefile [filedir]`

* `bundlefile` is the unity bundle file containing the files to that should be replaced.
* `newbundlefile` is the name for the new bundle with the replaced files.
* `filedir` is an optional parameter for directory to read the files that will be put in the bundle. If ommited, files will be read from the current directory.

When packing, the original `bundlefile` is not modified.