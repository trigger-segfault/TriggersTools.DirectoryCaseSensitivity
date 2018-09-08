# TriggersTools.DirectoryCaseSensitivity ![AppIcon](https://i.imgur.com/rxOsY1w.png)

[![NuGet Version](https://img.shields.io/nuget/v/TriggersTools.DirectoryCaseSensitivity.svg?style=flat)](https://www.nuget.org/packages/TriggersTools.DirectoryCaseSensitivity/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TriggersTools.DirectoryCaseSensitivity.svg?style=flat)](https://www.nuget.org/packages/TriggersTools.DirectoryCaseSensitivity/)
[![Creation Date](https://img.shields.io/badge/created-september%202018-A642FF.svg?style=flat)](https://github.com/trigger-death/TriggersTools.DirectoryCaseSensitivity/commit/b7a822e96dd0503425fbe6f135f723cf28ac207c)

A library for working with Windows 10, April 2018 Update's addition of per-directory case sensitivity.

**Important Note:** Although Windows now supports case-sensitive folders, most programs still do not, and will not behave properly when files with matching case-insensitive names exist. Only use `DirectoryCaseSensitivity.SetCaseSensitive()` when appropriate.

## Basic Example

Below is some of the basic functionality of the static class `DirectoryCaseSensitivity`.

```cs
using TriggersTools.IO.Windows;

// Check if we're in a high enough version of Windows. Calculated once per runtime
if (!DirectoryCaseSensitivity.IsSupported())
    return;

const string dir = "Case Sensitive Folder";
const string file1 = "file1.txt";
const string FILE1 = "FILE1.txt";

// Create a case sensitive Folder
DirectoryCaseSensitivity.Create(dir, true);

// Outputs
//   True
Console.WriteLine($"Is Case Sensitive: {DirectoryCaseSensitivity.IsCaseSensitive(dir)}");

// Create files with identical case-insensitive names
File.Create(file1).Close();
File.Create(FILE1).Close();

// Outputs:
//   file1.txt
//   FILE1.txt
foreach (string file in Directory.EnumerateFiles(dir))
    Console.WriteLine(Path.GetFileName(file));

// Now disable case sensitivity
// IoException: 2 files have the same case-insensitive name
DirectoryCaseSensitivity.SetCaseSensitive(dir, false);
```

## Create/Inherit

`DirectoryCaseSensitivity` also comes with methods to create case sensitive directories, and/or inherit the case sensitivity of the parent directory. It's important to note that directories created through `Directory.CreateDirectory()` will always be case-insensitive when first created.

```cs
const string dir = "Case Sensitive Folder";
const string subdir1 = dir + @"\Directory.CreateDirectory";
const string subdir2 = dir + @"\DirectoryCaseSensitivity.Create";
const string subdir3 = dir + @"\DirectoryCaseSensitivity.CreateInherit";

// Create a case sensitive Folder
DirectoryCaseSensitivity.Create(dir, true);

Directory.CreateDirectory(subdir1);
DirectoryCaseSensitivity.Create(subdir2, true);
DirectoryCaseSensitivity.CreateInherit(subdir3);

// Output:
//    Directory.CreateDirectory               False
//    DirectoryCaseSensitivity.Create         True
//    DirectoryCaseSensitivity.CreateInherit  True
foreach (string subdir in Directory.EnumerateDirectories(dir)) {
    bool enabled = DirectoryCaseSensitivity.IsCaseSensitive(subdir);
    Console.WriteLine($"{Path.GetFileName(subdir),-40} {enabled}");
}

// Inherit will take an existing directory and set
// its case sensitivity to that of its parent.
// It also returns the inherited sensitivity.
// Outputs:
//   Directory.CreateDirectory inherit: True
Console.WriteLine($"{subdir1} inherit: {DirectoryCaseSensitivity.Inherit(sibdir1)}");
```

## DirectoryInfo Extensions

`DirectoryInfo` gets extension methods for `IsCaseSensitive()` and `SetCaseSensitive()`.

```cs
const string dir = "Case Sensitive Folder";

Directory.CreateDirectory(dir);
DirectoryInfo dirInfo = new DirectoryInfo(dir);

// Outputs
//   False
Console.WriteLine($"Is Case Sensitive: {dirInfo.IsCaseSensitive()}");

dirInfo.SetCaseSensitive(true);

// Outputs
//   True
Console.WriteLine($"Is Case Sensitive: {dirInfo.IsCaseSensitive()}");
```
