using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

if (args.Length < 2) {
    Console.WriteLine("Usage:");
    Console.WriteLine("\tHimeChange unpack bundlefile [filedir]");
    Console.WriteLine("\tHimeChange pack bundlefile newbundlefile [filedir]");
    return;
}

// Read command line values

var bundleFile = args[1];
var newBundleFile = "";

// If a file directory was provided, read/write the files to that location
// Otherwise use the current directory
var fileDir = "";

var pack = false;
if (args[0] == "pack") {
    pack = true;

    newBundleFile = args[2];

    if (args.Length == 4) {
        fileDir = args[3];
    }
}
else {
    if (args.Length == 3) {
        fileDir = args[2];
    }
}

// Add a trailing "\" to the path if one was not provided
if (fileDir != "" && !fileDir.EndsWith("\\")) {
    fileDir = fileDir + "\\";
}

List<AssetsReplacer> replacers = new List<AssetsReplacer>();

var am = new AssetsManager();

// Load the file containing the assets
var bun = am.LoadBundleFile(bundleFile);

// Load first assets file from bundle (this assumes there is only one asset in the bundle)
var inst = am.LoadAssetsFileFromBundle(bun, 0, true);

// Go over every asset in that asset file
var table = inst.table;
foreach (var inf in table.assetFileInfo) {
    var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
    var pathValue = baseField.Get("Path").GetValue();
    var textValue = baseField.Get("Text").GetValue();

    // Ignore anything that doesn't have "path" and "text" fields
    if (pathValue != null && textValue != null) {
        var path = fileDir + pathValue.AsString();

        if (pack) {
            // Update the files contained in the bundle 

            // Skip over any files that don't exist
            if (!File.Exists(path)) {
                continue;
            }

            Console.WriteLine("Replaced file: " + path);

            var bytes = File.ReadAllBytes(path);

            // Get the bytes for the asset with the new data
            textValue.Set(bytes);
            var newBytes = baseField.WriteToByteArray();

            // Create a replacer for later updating the in memory asset pack
            var repl = new AssetsReplacerFromMemory(
                0, inf.index, (int)inf.curFileType,
                AssetHelper.GetScriptIndex(inst.file, inf),
                newBytes
            );

            replacers.Add(repl);
        }
        else {
            // Write the contents of the file to the path

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, textValue.AsStringBytes());
        }
    }
}

// If we are packing, perform the replacements
if (pack) {
    // Create updated asset file in memory using the replacers created earlier
    byte[] newAssetData;
    using (var stream = new MemoryStream())
    using (var writer = new AssetsFileWriter(stream)) {
        inst.file.Write(writer, 0, replacers, 0);

        writer.Close();
        stream.Close();

        newAssetData = stream.ToArray();
    }

    // Create a replacer for the bundle
    var bunRepl = new BundleReplacerFromMemory(inst.name, null, false, newAssetData, -1);

    // Create the writer for the new bundle file
    var bunWriter = new AssetsFileWriter(File.OpenWrite(newBundleFile));
    bun.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });

    bun.file.Close();
    bunWriter.Close();
}

am.UnloadAllAssetsFiles();