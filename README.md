# NCMDump.NET

Decipher .ncm file to mp3 || flac

## Feature

Highly Optimized Corelib.

Keep ID3 tags and cover image.

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/Result.png"/>

## Changelog

### v1.5

Upgrade to .Net 7 Runtime with so Fxxxxxx high performance.
[Download Link](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.4-windows-x64-installer ".NET 7.0 Desktop Runtime (v7.0.4) - Windows x64 Installer")

Add check box: Delete .ncm file when done 

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/Demo2.png"/>

### v1.4

UI adjustment.

Performance Improvement.

<img src="https://github.com/kingsznhone/NCMDump.NET/blob/main/Demo1.png"/>

### v1.3

Bug fix.

Async optimization.

### v1.2

Bug fix.

Drag directory || Select folder on GUI support.

### v1.1

Drag directory on executable support.

Bug fix

## Usage

### CLI

Drag .ncm file or directory on exe

### GUI

Don't ask, Just use.

## System Requirement

### OS : Windows 10 10.0.19041.0 with .NET7 Desktop Runtime

## API

### public bool Convert(string path)

Convert NCM file into MP3/FLAC format in synchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` True if convert success

<br/>

### public async Task\<bool\> ConvertAsync(string path)

Convert NCM file into MP3/FLAC format in asynchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` True if convert success

## Refrence
<https://github.com/mono/taglib-sharp>

<https://github.com/HandyOrg/HandyControl>

<https://github.com/anonymous5l/ncmdump>
